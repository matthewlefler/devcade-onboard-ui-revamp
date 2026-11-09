use std::io;
use std::path::Path;
use std::process::Command;

static DEFAULT_DOTENV_PATH: &str = "/usr/share/devcade/.env.template";
static DOTENV_PATH: &str = "/home/devcade/.env";

fn dotenv_exists() -> bool {
    Path::new(DOTENV_PATH).exists()
}

pub fn create_dotenv_if_not_exists() -> io::Result<std::process::Output> {
    if dotenv_exists() {
        return Err(io::Error::from(io::ErrorKind::AlreadyExists));
    }

    Command::new("cp")
        .arg(DEFAULT_DOTENV_PATH)
        .arg(DOTENV_PATH)
        .output()
}
