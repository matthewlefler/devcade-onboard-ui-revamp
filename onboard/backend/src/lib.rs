#![feature(is_some_and)]
#![feature(path_file_prefix)]

use anyhow::{anyhow, Error};
use log::{log, Level};

use tokio::net::unix::pipe::{OpenOptions, Receiver, Sender};

/**
 * All the servers run by the backend that communicate with other processes on devcade
 */
pub mod servers;

/**
 * Module for managing the API and routes
 */
pub mod api;

/**
 * Module for defining and handling commands sent to the backend and responses sent from the backend
 */
pub mod command;

/**
 * Module for talking to gatekeeper tags
 */
pub mod nfc;

/**
 * Module for safely getting environment variables, logging any errors that occur and providing
 * default values.
 */
pub mod env {
    // TODO Cache env vars? Probably not necessary
    use log::{log, Level};
    use std::env;

    static mut PRODUCTION: bool = true;

    /**
     * Get the path to the devcade directory. This is where games are installed.
     * If the value is not set in the environment, it will default to /tmp/devcade.
     */
    #[must_use]
    pub fn devcade_path() -> String {
        let path = env::var("DEVCADE_PATH");

        match path {
            Ok(path) => path,
            Err(e) => {
                log!(
                    Level::Warn,
                    "Error getting DEVCADE_PATH falling back to '/tmp/devcade': {}",
                    e
                );
                env::set_var("DEVCADE_PATH", "/tmp/devcade");
                String::from("/tmp/devcade")
            }
        }
    }

    /**
     * Get the URL of the API. This is where games are downloaded from.
     * If the value is not set in the environment, it will throw a fatal error and panic.
     */
    #[must_use]
    pub fn api_url() -> String {
        let url = if unsafe { PRODUCTION } {
            env::var("DEVCADE_API_DOMAIN")
        } else {
            env::var("DEVCADE_DEV_API_DOMAIN")
        };

        match url {
            Ok(url) => format!("https://{url}"),
            Err(e) => {
                if unsafe { PRODUCTION } {
                    log!(Level::Error, "Error getting DEVCADE_API_DOMAIN: {}", e);
                } else {
                    log!(Level::Error, "Error getting DEVCADE_DEV_API_DOMAIN: {}", e);
                }
                panic!();
            }
        }
    }

    /**
     * Sets whether the API will interact with the production or development API.
     */
    // This is thread safe because this is the only place that PRODUCTION can be modified
    // so there is no way for a race condition to occur.
    pub fn set_production(prod: bool) {
        log!(Level::Info, "Setting production to {}", prod);
        unsafe {
            PRODUCTION = prod;
        }
    }
}

/**
 * Make a FIFO at the given path. Uses an unsafe call to `libc::mkfifo`.
 */
fn mkfifo(path: &str) -> Result<(), Error> {
    log!(Level::Info, "Creating FIFO at {}", path);
    let path = std::path::Path::new(path);
    if path.exists() {
        // TODO: Check if it's a FIFO
        return Ok(());
    }
    if !path.parent().unwrap().exists() {
        std::fs::create_dir_all(path.parent().expect("Path has no parent"))?;
    }
    let path = path.to_str().expect("Path is not valid UTF-8");
    let path = std::ffi::CString::new(path)?;
    unsafe {
        let exit_code = libc::mkfifo(path.as_ptr(), 0o644);
        match exit_code {
            0 => Ok(()),
            c => Err(anyhow!(format!("mkfifo exited with code {c}"))),
        }
    }
}

/**
 * Opens a FIFO for reading. If the FIFO does not exist, it will be created.
 */
pub fn open_read_pipe(path: &str) -> Result<Receiver, Error> {
    if !std::path::Path::new(path).exists() {
        match mkfifo(path) {
            Ok(_) => (),
            Err(e) => {
                log!(Level::Error, "Error creating FIFO: {}", e);
                panic!();
            }
        }
    }

    let pipe = OpenOptions::new()
        // opening read_write allows the write end of the pipe to close without causing the read
        // end to close as well. This is necessary as if there is an unexpected error in the onboard
        // this will allow the main process to continue and wait for the onboard to restart.
        // This behavior is only supported on Linux, so on other POSIX systems the write end of the
        // pipe will fail to open as the file is already open for writing.
        .read_write(true)
        .open_receiver(path)?;

    Ok(pipe)
}

/**
 * Opens a FIFO for writing. If the FIFO does not exist, it will be created.
 */
pub fn open_write_pipe(path: &str) -> Result<Sender, Error> {
    if !std::path::Path::new(path).exists() {
        match mkfifo(path) {
            Ok(_) => (),
            Err(e) => {
                log!(Level::Error, "Error creating FIFO: {}", e);
                panic!();
            }
        }
    }

    let pipe = OpenOptions::new().open_sender(path)?;

    Ok(pipe)
}
