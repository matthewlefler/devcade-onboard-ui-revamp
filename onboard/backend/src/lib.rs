#![feature(async_closure)]
#![feature(path_file_prefix)]

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
    use std::sync::Mutex;

    // TODO should be Mutex? Lmao
    static PRODUCTION: Mutex<bool> = Mutex::new(true);

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
                    "Error getting DEVCADE_PATH falling back to '$HOME/.devcade': {}",
                    e
                );
                let h = env::var("HOME").unwrap(); // if HOME is not set we have bigger issues
                env::set_var("DEVCADE_PATH", format!("{}/.devcade", h));
                format!("{}/.devcade", h)
            }
        }
    }

    /**
     * Get the URL of the API. This is where games are downloaded from.
     * If the value is not set in the environment, it will throw a fatal error and panic.
     */
    #[must_use]
    pub fn api_url() -> String {
        let url = if *PRODUCTION.lock().unwrap() {
            env::var("DEVCADE_API_DOMAIN")
        } else {
            env::var("DEVCADE_DEV_API_DOMAIN")
        };

        match url {
            Ok(url) => format!("https://{url}"),
            Err(e) => {
                if *PRODUCTION.lock().unwrap() {
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
        *PRODUCTION.lock().unwrap() = prod;
    }
}
