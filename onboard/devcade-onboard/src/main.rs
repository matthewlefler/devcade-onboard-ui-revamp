use log::{Level, log};
use std::io;
use std::path::Path;
use std::process::{Child, Command, exit};
use std::thread::sleep;
use std::time::Duration;

static STABLE_DEVCADE_FRONTEND_PATH: &str = "/usr/bin/devcade_frontend";
static STABLE_DEVCADE_BACKEND_PATH: &str = "/usr/bin/devcade_backend";

static DEV_DEVCADE_FRONTEND_PATH: &str = "/home/devcade/devcade_frontend";
static DEV_DEVCADE_BACKEND_PATH: &str = "/home/devcade/devcade_backend";

static SPAWN_PROCESS_RETRIES: i32 = 10;
static POLLING_DELAY_SECS: u64 = 5;

static STABLE_ENV_NAME: &str = "stable";
static BRANCH_ENV_NAME: &str = "branch";
static GIT_REPO_ENV_NAME: &str = "gitrepo";

static DEFAULT_GIT_REPOSITORY: &str =
    "https://github.com/matthewlefler/devcade-onboard-ui-revamp.git";
static DEFAULT_GIT_BRANCH: &str = "main";

fn main() -> ! {
    #[cfg(not(target_os = "linux"))]
    {
        compile_error!(
            "This project only supports Linux.\nTo build for linux, run `cargo build --target x86_64-unknown-linux-gnu`"
        );
    }

    let env_file_paths = vec![
        std::path::PathBuf::from(".env"),
        std::env::home_dir()
            .map(|home| home.join(".env"))
            .unwrap_or(std::path::PathBuf::from(".env")),
        std::path::PathBuf::from("../.env"),
        std::path::PathBuf::from("/usr/share/devcade/.env"),
    ];

    let loaded = env_file_paths.iter().any(|path| {
        if dotenvy::from_path(&path).is_ok() {
            log!(Level::Debug, "Loaded environment from {}", path.display());
            true
        } else {
            false
        }
    });

    if !loaded {
        log!(Level::Error, "No .env file found");
    }

    let stable = match dotenvy::var(STABLE_ENV_NAME) {
        Ok(value) => match value.parse::<bool>() {
            Ok(value) => value,
            Err(error) => {
                log!(
                    Level::Error,
                    "Invalid value for stable: {},\n\tit must be either `true` or `false`",
                    error
                );
                false
            }
        },
        Err(error) => {
            log!(Level::Error, "{}", error);
            false
        }
    };

    let mut devcade_backend_path = Path::new(STABLE_DEVCADE_BACKEND_PATH);
    let mut devcade_frontend_path = Path::new(STABLE_DEVCADE_FRONTEND_PATH);

    if !stable {
        let mut err_happend = false;
        let git_repo_url = match dotenvy::var(GIT_REPO_ENV_NAME) {
            Ok(s) => s,
            Err(e) => {
                log!(
                    Level::Info,
                    "using default git repository: {} because: {}",
                    DEFAULT_GIT_REPOSITORY,
                    e
                );
                String::from(DEFAULT_GIT_REPOSITORY)
            }
        };

        if let Err(e) = git_repo_clone(&git_repo_url) {
            log!(
                Level::Error,
                "failed to clone git repository: {}, because: {}",
                git_repo_url,
                e
            );
            err_happend = true;
        }

        let branch_name = match dotenvy::var(BRANCH_ENV_NAME) {
            Ok(s) => s,
            Err(e) => {
                log!(
                    Level::Info,
                    "using default branch: {} because: {}",
                    DEFAULT_GIT_BRANCH,
                    e
                );
                String::from(DEFAULT_GIT_BRANCH)
            }
        };
        if let Err(e) = git_repo_switch_to_branch(&branch_name) {
            log!(
                Level::Error,
                "failed to switch to branch: {}, because: {}",
                branch_name,
                e
            );
            err_happend = true;
        }

        if !err_happend {
            devcade_backend_path = Path::new(DEV_DEVCADE_BACKEND_PATH);
            devcade_frontend_path = Path::new(DEV_DEVCADE_FRONTEND_PATH);
        }
    }

    let mut devcade_backend_cmd = Command::new(devcade_backend_path);
    let mut devcade_frontend_cmd = Command::new(devcade_frontend_path);

    let mut devcade_backend =
        match spawn_child_with_retries(&mut devcade_backend_cmd, SPAWN_PROCESS_RETRIES) {
            Ok(child) => child,
            Err(e) => {
                log!(Level::Error, "failed to spawn devcade_backend: {}", e);
                exit(-1)
            }
        };
    let mut devcade_frontend =
        match spawn_child_with_retries(&mut devcade_frontend_cmd, SPAWN_PROCESS_RETRIES) {
            Ok(child) => child,
            Err(e) => {
                log!(Level::Error, "failed to spawn devcade_frontend: {}", e);
                match devcade_backend.kill() {
                    Ok(_) => log!(Level::Info, "killed backend"),
                    Err(e) => log!(Level::Error, "failed to kill backend: {}", e),
                }
                exit(-1)
            }
        };

    loop {
        match devcade_backend.try_wait() {
            Err(e) => {
                log!(Level::Error, "failed to get backend status: {}", e);
                cleanup(&mut devcade_frontend, &mut devcade_backend);
                exit(-1);
            }
            Ok(Some(exit_status)) => {
                log!(
                    Level::Error,
                    "backend exited with exit status {}",
                    exit_status
                );

                devcade_backend =
                    match spawn_child_with_retries(&mut devcade_backend_cmd, SPAWN_PROCESS_RETRIES)
                    {
                        Ok(child) => child,
                        Err(e) => {
                            log!(Level::Error, "failed to spawn devcade_backend: {}", e);
                            cleanup(&mut devcade_frontend, &mut devcade_backend);
                            exit(-1)
                        }
                    }
            }
            _ => {} // continue running otherwise
        }

        match devcade_frontend.try_wait() {
            Err(e) => {
                log!(Level::Error, "failed to get frontend status: {}", e);
                cleanup(&mut devcade_frontend, &mut devcade_backend);
                exit(-1);
            }
            Ok(Some(exit_status)) => {
                log!(
                    Level::Error,
                    "frontend exited with exit status {}",
                    exit_status
                );

                devcade_frontend =
                    match spawn_child_with_retries(&mut devcade_frontend_cmd, SPAWN_PROCESS_RETRIES)
                    {
                        Ok(child) => child,
                        Err(e) => {
                            log!(Level::Error, "failed to spawn devcade_frontend: {}", e);
                            cleanup(&mut devcade_frontend, &mut devcade_backend);
                            exit(-1)
                        }
                    }
            }
            _ => {} // continue running otherwise
        }

        sleep(Duration::from_secs(POLLING_DELAY_SECS));
    }
}

fn spawn_child_with_retries(cmd: &mut Command, retries: i32) -> io::Result<Child> {
    let mut error = io::Error::from(io::ErrorKind::NotFound);
    for _ in 0..retries {
        match cmd.spawn() {
            Ok(child) => return Ok(child),
            Err(e) => error = e,
        }
    }
    log!(Level::Error, "failed to spawn cmd {:?}", cmd.get_program());
    Err(error)
}

fn git_repo_clone(repo: &String) -> std::io::Result<std::process::Output> {
    // cmd run `git clone {url}`
    Command::new("git").arg("clone").arg(repo).output()
}

fn git_repo_switch_to_branch(branch_name: &String) -> std::io::Result<std::process::Output> {
    // cmd run `git switch {branch_name}`
    Command::new("git").arg("switch").arg(branch_name).output()
}

fn cleanup(frontend: &mut Child, backend: &mut Child) {
    match frontend.kill() {
        Ok(_) => log!(Level::Info, "killed frontend"),
        Err(e) => log!(Level::Error, "failed to kill frontend: {}", e), // force reboot the machine perhaps?
    }
    match backend.kill() {
        Ok(_) => log!(Level::Info, "killed backend"),
        Err(e) => log!(Level::Error, "failed to kill backend: {}", e),
    }
}
