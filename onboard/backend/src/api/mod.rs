use std::ffi::OsStr;
use std::os::unix::fs::PermissionsExt;
use anyhow::{anyhow, Error};
use std::path::Path;
use std::process::ExitStatus;
use std::thread;
use std::time::Duration;
use log::{Level, log};
use std::thread::JoinHandle;
use crate::env::{api_url, devcade_path};
use crate::api::schema::DevcadeGame;

/**
 * Module for defining the database schema, these are the same schema that the API uses, so they are
 * defined here as well to deserialize the responses from the API.
 */
pub mod schema;

/**
 * Internal module for network requests and JSON serialization
 */
mod network {
    use std::ops::Deref;
    use anyhow::Error;
    use lazy_static::lazy_static;
    use log::{Level, log};
    use serde::Deserialize;

    // Construct a static client to be used for all requests. Prevents opening a new connection for
    // every request.
    lazy_static! {
        static ref CLIENT: reqwest::Client = reqwest::Client::new();
    }

    /**
     * Request JSON from a URL and serialize it into a struct
     *
     * # Errors
     * This function will return an error if the request fails, or if the JSON cannot be deserialized
     */
    pub async fn request_json<T: for<'de> Deserialize<'de>>(url: &str) -> Result<T, Error> {
        log!(Level::Trace, "Requesting JSON from {}", url);
        let response = CLIENT.deref().get(url).send().await?;
        let json = response.json().await?;
        Ok(json)
    }

    /**
     * Request binary data from a URL
     *
     * # Errors
     * This function will return an error if the request fails.
     */
    pub async fn request_bytes(url: &str) -> Result<Vec<u8>, Error> {
        log!(Level::Trace, "Requesting binary from {}", url);
        let response = CLIENT.deref().get(url).send().await?;
        let bytes = response.bytes().await?;
        Ok(bytes.to_vec())
    }
}

/**
 * Internal module for API routes and URLs
 * This is used to make sure that the API routes are consistent across the codebase, and can be
 * changed from a single location.
 */
mod route {

    /**
     * Get the list of games
     */
    pub fn game_list() -> String {
        String::from("games/")
    }

    /**
     * Get a specific game by ID
     */
    pub fn game(id: &str) -> String {
        format!("games/{id}")
    }

    /**
     * Get a specific game's icon by ID
     */
    pub fn game_icon(id: &str) -> String {
        format!("games/{id}/icon")
    }

    /**
     * Get a specific game's banner by ID
     */
    pub fn game_banner(id: &str) -> String {
        format!("games/{id}/banner")
    }

    /**
     * Get a specific game's binary by ID
     */
    pub fn game_download(id: &str) -> String {
        format!("games/{id}/game")
    }
}

/**
 * Get a list of games from the API. This is the preferred method of getting games.
 *
 * # Errors
 * This function will return an error if the request fails, or if the JSON cannot be deserialized
 */
pub async fn game_list() -> Result<Vec<DevcadeGame>, Error> {
    let games = network::request_json(format!("{}/{}", api_url(), route::game_list()).as_str()).await?;
    Ok(games)
}

/**
 * Get a specific game from the API. This is the preferred method of getting games.
 *
 * # Errors
 * This function will return an error if the request fails, or if the JSON cannot be deserialized
 */
pub async fn get_game(id: &str) -> Result<DevcadeGame, Error> {
    let game = network::request_json(format!("{}/{}", api_url(), route::game(id)).as_str()).await?;
    Ok(game)
}

/**
 * Get the list of games currently installed on the filesystem. This can be used if the API is down.
 * This is not the preferred method of getting games.
 *
 * # Errors
 * This function will return an error if the filesystem cannot be read at the DEVCADE_PATH location.
 */
pub fn game_list_from_fs() -> Result<Vec<DevcadeGame>, Error> {
    let mut games = Vec::new();
    for entry in std::fs::read_dir(devcade_path())? {
        let entry = entry?;
        let path = entry.path();
        if !path.is_dir() {
            continue;
        }

        for entry_ in std::fs::read_dir(path)? {
            let entry_ = entry_?;
            let path_ = entry_.path();
            if path_.is_dir() {
                continue;
            }

            if let Ok(game) = game_from_path(path_.to_str().unwrap()) {
                games.push(game);
            }
        }
    }
    Ok(games)
}

/**
 * Download's a game's banner from the API.
 *
 * # Errors
 * This function will return an error if the request fails, or if the filesystem cannot be written to.
 */
pub async fn download_banner(game_id: String) -> Result<(), Error> {

    let path = Path::new(devcade_path().as_str()).join(game_id.clone()).join("banner.png");
    if path.exists() {
        return Ok(());
    }
    if !path.parent().unwrap().exists() {
        std::fs::create_dir_all(path.parent().unwrap())?;
    }

    let bytes = network::request_bytes(format!("{}/{}", api_url(), route::game_banner(game_id.as_str())).as_str()).await?;
    std::fs::write(path, bytes)?;
    Ok(())
}

/**
 * Download's a game's icon from the API.
 *
 * # Errors
 * This function will return an error if the request fails, or if the filesystem cannot be written to.
 */
pub async fn download_icon(game_id: String) -> Result<(), Error> {
    let api_url = api_url();
    let file_path = devcade_path();

    let path = Path::new(file_path.as_str()).join(game_id.clone()).join("icon.png");
    if path.exists() {
        return Ok(());
    }
    if !path.parent().unwrap().exists() {
        std::fs::create_dir_all(path.parent().unwrap())?;
    }

    let bytes = network::request_bytes(format!("{}/{}", api_url, route::game_icon(game_id.as_str())).as_str()).await?;
    std::fs::write(path, bytes)?;
    Ok(())
}

/**
 * Download's a game's zip file from the API and unzips it into the game's directory. If the game is
 * already downloaded, it will check if the hash is the same. If it is, it will not download the game
 * again.
 *
 * # Errors
 * This function will return an error if the request fails, or if the filesystem cannot be written to.
 */
pub async fn download_game(game_id: String) -> Result<(), Error> {
    let path = Path::new(devcade_path().as_str()).join(game_id.clone()).join("game.json");

    let game = get_game(game_id.as_str()).await?;

    // Check if the game is already downloaded, and if it is, check if the hash is the same
    if path.exists() {
        if let Ok(game_) = game_from_path(path.to_str().unwrap()) {
            if game_.hash == game.hash {
                return Ok(());
            }
        }
    }

    log!(Level::Info, "Downloading game {}...", game.name);

    let bytes = network::request_bytes(format!("{}/{}", api_url(), route::game_download(game_id.as_str())).as_str()).await?;

    log!(Level::Info, "Unzipping game {}...", game.name);
    log!(Level::Trace, "Zip file size: {} bytes", bytes.len());

    // Unzip the game into the game's directory
    let mut zip = zip::ZipArchive::new(std::io::Cursor::new(bytes))?;

    for i in 0..zip.len() {
        let mut file = match zip.by_index(i) {
            Ok(f) => f,
            Err(e) => {
                log!(Level::Warn, "Error getting file from zip: {}", e);
                continue;
            }
        };
        let out_path = Path::new(devcade_path().as_str()).join(game.id.clone()).join(file.name());
        log!(Level::Trace, "Unzipping file {} to {}", file.name(), out_path.to_str().unwrap());
        if file.name().ends_with('/') {
            match std::fs::create_dir_all(&out_path) {
                Ok(_) => {}
                Err(e) => {
                    log!(Level::Warn, "Error creating directory {}: {}", out_path.to_str().unwrap(), e);
                }
            }
        } else {
            if let Some(p) = out_path.parent() {
                if !p.exists() {
                    match std::fs::create_dir_all(p) {
                        Ok(_) => {}
                        Err(e) => {
                            log!(Level::Warn, "Error creating directory {}: {}", p.to_str().unwrap(), e);
                        }
                    };
                }
            }
            let mut outfile = match std::fs::File::create(&out_path) {
                Ok(f) => f,
                Err(e) => {
                    log!(Level::Warn, "Error creating file {}: {}", out_path.to_str().unwrap(), e);
                    continue;
                }
            };
            match std::io::copy(&mut file, &mut outfile) {
                Ok(_) => {}
                Err(e) => {
                    log!(Level::Warn, "Error copying file {}: {}", out_path.to_str().unwrap(), e);
                }
            };
        }
    }

    // Write the game's JSON file to the game's directory (this is used later to get the games from
    // the filesystem)
    log!(Level::Debug, "Writing game.json file for game {}...", game.name);
    log!(Level::Trace, "Game json path: {}", path.to_str().unwrap());
    let json = serde_json::to_string(&game)?;
    std::fs::create_dir_all(path.parent().unwrap()).unwrap();
    match std::fs::write(path, json) {
        Ok(_) => {}
        Err(e) => {
            log!(Level::Warn, "Error writing game.json file: {}", e);
        }
    };
    Ok(())
}

/**
 * Launch a game by its ID. This will check if the game is downloaded, and if it is, it will launch
 * the game. This returns a `JoinHandle`, which should be used to check for game exit and notify the
 * backend.
 *
 * # Errors
 * This function will return an error if the filesystem cannot be read from,
 * or if the game cannot be launched.
 *
 * # Panics
 * This function will never panic, but contains an `unwrap` call that will never fail. This section
 * is here to make clippy happy.
 */
pub async fn launch_game(game_id: String) -> Result<JoinHandle<ExitStatus>, Error> {
    let path = Path::new(devcade_path().as_str()).join(game_id.clone()).join("publish");

    log!(Level::Info, "Launching game {}...", game_id);
    log!(Level::Trace, "Game path: {}", path.to_str().unwrap());

    if !path.exists() {
        download_game(game_id.clone()).await?;
    }

    // Infer executable name from *.runtimeconfig.json
    let mut executable = String::new();

    for entry in std::fs::read_dir(path.clone())? {
        let entry = match entry {
            Ok(entry) => entry,
            Err(_) => continue,
        };
        let path = entry.path();
        if !path.is_file() {
            continue;
        }

        if let Some(extension) = path.extension().map(|s| s.to_str().unwrap_or("")) {
            if extension != "runtimeconfig.json" {
                continue;
            }
            executable = path.file_stem().unwrap_or(OsStr::new("")).to_str().unwrap_or("").to_string();
            break;

        }
    }

    // If no *.runtimeconfig.json file is found, look for a file with the same name as the game
    // (this is the case for games that don't use .NET)
    // TODO: Some better way to find executable name?
    if executable.is_empty() {
        // This parent().unwrap() is safe because the path is guaranteed to have a parent
        let game = game_from_path(path.clone().parent().unwrap().join("game.json").to_str().unwrap_or(""))?;
        executable = game.name;
    }

    let path = path.join(executable);

    if !path.exists() {
        return Err(anyhow!("Game executable not found"));
    }

    // Chmod +x the executable
    let mut perms = path.metadata()?.permissions();
    perms.set_mode(0o755);

    std::fs::set_permissions(path.clone(), perms)?;

    // Launch the game and silence stdout (allow the game to print to stderr)
    let mut child = std::process::Command::new(path.clone());

    child.stdout(std::process::Stdio::null());
    // Unfortunately this will bypass the log crate, so no pretty logging for games
    child.stderr(std::process::Stdio::inherit());
    child.current_dir(path.parent().unwrap()); // This unwrap is safe because it is guaranteed to have a parent

    let handle = thread::spawn(move || {
        let mut child_handle = child.spawn().expect("failed to execute child");
        child_handle.wait().expect("failed to wait on child")
    });

    tokio::time::sleep(Duration::from_millis(200)).await;

    if handle.is_finished() {
        let e = handle.join();
        // Even if the game exited successfully, if it exited after 200ms, something went wrong
        return Err(anyhow!("Error launching game: {:?}", e));
    }

    Ok(handle)
}

/**
 * Returns a devcade game if the file at the path is a JSON file containing a devcade game
 *
 * # Errors
 * This function will return an error if the file does not exist, is a directory, or if the file
 * cannot be read.
 */
fn game_from_path(path: &str) -> Result<DevcadeGame, Error> {
    log!(Level::Trace, "Reading game from path {}", path);
    let path = Path::new(path);
    if !path.exists() {
        return Err(anyhow!("Path does not exist"));
    }
    if path.is_dir() {
        return Err(anyhow!("Path is a directory"));
    }
    let str = std::fs::read_to_string(path)?;

    let game: DevcadeGame = serde_json::from_str(&str)?;

    Ok(game)
}
