use crate::env::{api_url, devcade_path};
use crate::nfc::NFC_CLIENT;
use anyhow::{anyhow, Error};
use devcade_onboard_types::{
    schema::{DevcadeGame, MinimalGame, Tag, User},
    Map, Player, Value,
};
use log::{log, Level};

use base64::Engine;
use lazy_static::lazy_static;
use libflatpak::{gio, prelude::*, Installation, Transaction};
use serde::Serialize;
use std::cell::Cell;
use std::collections::{HashMap, HashSet};
use std::os::unix::fs::PermissionsExt;
use std::path::{Path, PathBuf};
use std::process::Stdio;
use std::sync::Mutex;
use std::time::Duration;
use tokio::fs;
use tokio::process::Command;
use tokio::sync::oneshot;

lazy_static! {
    static ref CURRENT_GAME: Mutex<Cell<DevcadeGame>> =
        Mutex::new(Cell::new(DevcadeGame::default()));
    // basically just checks if a user 'devcade' exists. If so, assumes that this is running on the
    // machine, and saves to the homedir. Otherwise, saves to the cwd.
    static ref ON_MACHINE: bool = Path::new("/home/devcade").exists();
    static ref DB: tokio::sync::Mutex<HashMap<String, HashMap<String, String>>> = tokio::sync::Mutex::new(HashMap::new());
    static ref DB_MODIFIED: tokio::sync::Mutex<HashSet<String>> = tokio::sync::Mutex::new(HashSet::new());
}

/**
 * Internal module for network requests and JSON serialization
 */
mod network {
    use anyhow::Error;
    use lazy_static::lazy_static;
    use log::{log, Level};
    use serde::Deserialize;
    use std::ops::Deref;

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

    /**
     * Get all tags
     */
    pub fn tag_list() -> String {
        String::from("tags/")
    }

    /**
     * Get a specific tag
     */
    pub fn tag(name: &str) -> String {
        format!("tags/{name}")
    }

    /**
     * Get all games with a specific tag
     */
    pub fn tag_games(name: &str) -> String {
        format!("tags/{name}/games")
    }

    /**
     * Get a specific user
     */
    pub fn user(uid: &str) -> String {
        format!("users/{uid}")
    }
}

/**
 * Get a list of games from the API. This is the preferred method of getting games.
 *
 * # Errors
 * This function will return an error if the request fails, or if the JSON cannot be deserialized
 */
pub async fn game_list() -> Result<Vec<DevcadeGame>, Error> {
    let games: Vec<DevcadeGame> =
        network::request_json(format!("{}/{}", api_url(), route::game_list()).as_str()).await?;
    Ok(games
        .into_iter()
        .filter(|game| game.hash.is_some())
        .collect::<Vec<DevcadeGame>>())
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

            if let Ok(game) = game_from_path(&path_) {
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
    let path = Path::new(devcade_path().as_str())
        .join(game_id.clone())
        .join("banner.png");
    if path.exists() {
        return Ok(());
    }
    if !path.parent().unwrap().exists() {
        std::fs::create_dir_all(path.parent().unwrap())?;
    }

    let bytes = network::request_bytes(
        format!("{}/{}", api_url(), route::game_banner(game_id.as_str())).as_str(),
    )
    .await?;
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

    let path = Path::new(file_path.as_str())
        .join(game_id.clone())
        .join("icon.png");
    if path.exists() {
        return Ok(());
    }
    if !path.parent().unwrap().exists() {
        std::fs::create_dir_all(path.parent().unwrap())?;
    }

    let bytes = network::request_bytes(
        format!("{}/{}", api_url, route::game_icon(game_id.as_str())).as_str(),
    )
    .await?;
    std::fs::write(path, bytes)?;
    Ok(())
}

pub async fn nfc_tags(reader_id: Player) -> Result<Option<String>, Error> {
    assert!(reader_id == Player::P1);
    NFC_CLIENT
        .submit()
        .await
        .map_err(|err| anyhow!("Couldn't get NFC tags: {:?}", err))
}

pub async fn nfc_user(association_id: String) -> Result<Map<String, Value>, Error> {
    NFC_CLIENT
        .get_user(association_id)
        .await
        .map_err(|err| anyhow!("Couldn't get NFC user: {:?}", err))
}

async fn install_flatpak_bundle_async(bundle_path: PathBuf) -> Result<String, Error> {
    let (tx, rx) = oneshot::channel();
    std::thread::spawn(move || {
        tx.send(install_flatpak_bundle(&bundle_path))
            .expect("Server thread died before we could send flatpak install response?")
    });
    match rx.await {
        Ok(result) => result,
        Err(err) => Err(err.into()),
    }
}

fn install_flatpak_bundle(bundle_path: &Path) -> Result<String, Error> {
    let transaction = Transaction::for_installation(
        &Installation::new_user(None::<&gio::Cancellable>)?,
        None::<&gio::Cancellable>,
    )?;
    transaction.set_no_pull(false);
    transaction.set_no_interaction(true);
    transaction.add_default_dependency_sources();
    transaction.add_install_bundle(&gio::File::for_path(bundle_path), None)?;
    transaction.set_reinstall(true);
    let (tx_app_id, rx_app_id) = std::sync::mpsc::channel::<String>();
    transaction.connect_ready(move |transaction| {
        // Return false to abort!
        let mut app_name = None::<String>;
        for op in transaction.operations() {
            log::debug!(
                "Processing operation for bundle {:?}",
                op.bundle_path().map(|path| path.to_string())
            );
            if let Some(metadata) = op.metadata() {
                log::debug!("Checking metadata: {}", metadata.to_data().as_str());
                let name = metadata
                    .string("Application", "name")
                    .map(|name| name.to_string());
                if let Ok(name) = &name {
                    log::info!("Found an app name {name}");
                    app_name = Some(name.clone());
                }
                log::debug!("Name of bundle is {name:?}");
                match is_install_allowed(&metadata) {
                    Ok(true) => {
                        log::debug!("All permissions look OK on app {name:?}");
                    }
                    Ok(false) => {
                        log::error!("Aborting installation of {name:?}");
                        return false;
                    }
                    Err(err) => {
                        log::error!("Aborting installation of {name:?} due to error {err}");
                        return false;
                    }
                }
                // if let Ok(name) = metadata.string("Application", "name") {}
                // println!(
                //     "Found metadata for {:?}: {}",
                //     op.bundle_path(),
                //     metadata.to_data().as_str()
                // );
            } else {
                println!("no data for {:?}", op.bundle_path());
            }
        }
        tx_app_id.send(app_name.unwrap()).unwrap();
        // abort anyways, just testing :)
        true
    });
    transaction.run(None::<&gio::Cancellable>)?;
    Ok(rx_app_id.recv().unwrap())
    //Ok("todo".to_owned())
}

fn is_install_allowed(metadata: &gio::glib::KeyFile) -> Result<bool, Error> {
    if !metadata.has_group("Context") {
        return Ok(true);
    }
    let allowed_permissions = HashMap::from([
        ("shared", HashSet::from(["network", "ipc"])),
        ("sockets", HashSet::from(["x11", "pulseaudio"])),
        ("devices", HashSet::from(["dri"])),
        (
            "filesystems",
            HashSet::from(["/tmp/devcade/persistence.sock", "/tmp/devcade/game.sock"]),
        ),
    ]);

    for (realm, allowed_capabilities) in allowed_permissions.iter() {
        if !metadata.has_key("Context", realm)? {
            continue;
        }
        for capability in metadata
            .string_list("Context", realm)?
            .iter()
            .map(|entry| entry.to_str())
        {
            if !allowed_capabilities.contains(capability) {
                // Disallowed/unknown cap!
                log::error!("Unknown capability {realm}={capability} is not allowed!");
                return Ok(false);
            }
        }
    }

    for realm in metadata.keys("Context")?.iter().map(|entry| entry.to_str()) {
        if !allowed_permissions.contains_key(realm) {
            log::error!("Unknown realm {realm} is not allowed!");
            return Ok(false);
        }
    }

    Ok(true)
}

/**
 * Download's a game's zip file from the API and unzips it into the game's directory. If the game is
 * already downloaded, it will check if the hash is the same. If it is, it will not download the game
 * again.
 *
 * # Errors
 * This function will return an error if the request fails, or if the filesystem cannot be written to.
 */
pub async fn download_game(game_id: String) -> Result<DevcadeGame, Error> {
    log::debug!("Downloading a game!");
    let game_dir = Path::new(devcade_path().as_str()).join(game_id.clone());
    let game_json_path = game_dir.join("game.json");

    let local_game = game_from_path(&game_json_path);
    let mut game = match get_game(game_id.as_str()).await {
        Ok(game) => {
            log::debug!("Fetched game meta!");
            game
        }
        Err(err) => {
            log::warn!("Couldn't request live info on game! Falling back to local file! {err:?}");
            local_game
                .as_ref()
                .expect("Game not downloaded and we're offline!")
                .clone()
        }
    };
    // Is the current hash == the remote hash?
    if let Ok(local_game) = local_game {
        if local_game.hash == game.hash {
            return Ok(local_game);
        }
    }

    log!(Level::Info, "Downloading game {}...", game.name);

    let bytes = network::request_bytes(
        format!("{}/{}", api_url(), route::game_download(game_id.as_str())).as_str(),
    )
    .await?;

    log!(Level::Info, " game {}...", game.name);
    log!(Level::Trace, "Flatpak bundle size: {} bytes", bytes.len());

    // // install flatpak
    tokio::fs::create_dir_all(&game_dir).await?;
    let bundle_path = game_dir.join("bundle.flatpak").to_owned();
    tokio::fs::write(&bundle_path, &bytes).await?;

    game.flatpak_app_id = Some(install_flatpak_bundle_async(bundle_path).await?);
    log::info!("Hi, flatpak app id {:?}", game.flatpak_app_id);

    // Write the game's JSON file to the game's directory (this is used later to get the games from
    // the filesystem)
    log!(
        Level::Debug,
        "Writing game.json file for game {}...",
        game.name
    );
    log!(
        Level::Trace,
        "Game json path: {}",
        game_json_path.to_str().unwrap()
    );
    let json = serde_json::to_string(&game)?;
    match tokio::fs::write(&game_json_path, json).await {
        Ok(_) => {}
        Err(e) => {
            log!(Level::Warn, "Error writing game.json file: {}", e);
            return Err(e.into());
        }
    };
    log::debug!("Downloaded game {game:?}");

    Ok(game)
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
pub async fn launch_game(game_id: String) -> Result<(), Error> {
    let path = Path::new(devcade_path().as_str())
        .join(game_id.clone())
        .join("publish");

    log!(Level::Info, "Launching game {}...", game_id);
    log!(Level::Trace, "Game path: {}", path.to_str().unwrap());

    // Downloads game if we don't already have it
    let game = download_game(game_id.clone()).await?;

    // flush data every time a new game is opened (in case previous launched game forgor)
    match persistence_flush().await {
        Ok(_) => {}
        Err(e) => log::warn!("Failed to flush save cache: {e}"),
    }
    CURRENT_GAME.lock().unwrap().set(game.clone());

    // Launch the game and silence stdout (allow the game to print to stderr)
    Command::new("flatpak")
        .arg("run")
        .arg(game.flatpak_app_id.unwrap())
        // Unfortunately this will bypass the log crate, so no pretty logging for games
        .stdout(Stdio::inherit())
        .stderr(std::process::Stdio::inherit())
        // This unwrap is safe because it is guaranteed to have a parent
        .current_dir(path.parent().unwrap())
        .spawn()
        .expect("Failed to launch game")
        .wait()
        .await
        .expect("Failed to launch game");

    tokio::time::sleep(Duration::from_millis(200)).await;
    Ok(())
}

async fn locate_executable(path: &Path) -> Result<String, Error> {
    // Infer executable name from *.runtimeconfig.json
    for entry in std::fs::read_dir(path)? {
        let entry = match entry {
            Ok(entry) => entry,
            Err(_) => continue,
        };
        let path = entry.path();
        if !path.is_file() {
            continue;
        }

        if let Some(filename) = path.file_name().map(|s| s.to_str().unwrap_or("")) {
            if !filename.ends_with(".runtimeconfig.json") {
                continue;
            }
            log!(Level::Debug, "Found runtimeconfig.json file: {}", filename);
            let executable = filename
                .strip_suffix(".runtimeconfig.json")
                .unwrap()
                .to_string();
            log!(
                Level::Debug,
                "Executable inferred from runtimeconfig.json: {}",
                executable
            );
            return Ok(executable);
        }
    }

    // If no *.runtimeconfig.json file is found, look for a file with the same name as the game
    // (this is the case for games that don't use .NET)
    // TODO: Some better way to find executable name?
    // This parent().unwrap() is safe because the path is guaranteed to have a parent
    let game = game_from_path(&path.parent().unwrap().join("game.json"))?;
    Ok(game.name)
}

/**
 * Returns a list of all tags in the database
 *
 * # Errors
 * This function will return an error if the server cannot be reached, or if the server returns an
 * error.
 */
pub async fn tag_list() -> Result<Vec<Tag>, Error> {
    network::request_json(format!("{}/{}", api_url(), route::tag_list()).as_str()).await
}

/**
 * Returns a tag by its name
 *
 * # Errors
 * This function will return an error if the server cannot be reached, or if the server returns an
 * error.
 */
pub async fn tag(name: String) -> Result<Tag, Error> {
    network::request_json(format!("{}/{}", api_url(), route::tag(name.as_str())).as_str()).await
}

/**
 * Returns a list of all games with the given tag
 *
 * # Errors
 * This function will return an error if the server cannot be reached, or if the server returns an
 * error.
 */
pub async fn tag_games(name: String) -> Result<Vec<DevcadeGame>, Error> {
    let games: Vec<MinimalGame> = network::request_json(
        format!("{}/{}", api_url(), route::tag_games(name.as_str())).as_str(),
    )
    .await?;
    let games: Vec<_> = games.into_iter().map(game_from_minimal).collect();
    // await all the games and return them
    let games: Vec<Result<DevcadeGame, Error>> = futures_util::future::join_all(games).await;
    Ok(games
        .into_iter()
        .filter_map(|g| {
            if let Ok(g) = g {
                Some(g)
            } else {
                log!(
                    Level::Warn,
                    "Failed to get game by tag {name}: {}",
                    g.unwrap_err()
                );
                None
            }
        })
        .collect())
}

/**
 * Gets a user's information by their user ID
 *
 * # Errors
 * This function will return an error if the server cannot be reached, or if the server returns an
 * error.
 */
pub async fn user(uid: String) -> Result<User, Error> {
    network::request_json(format!("{}/{}", api_url(), route::user(uid.as_str())).as_str()).await
}

/**
 * Returns a devcade game if the file at the path is a JSON file containing a devcade game
 *
 * # Errors
 * This function will return an error if the file does not exist, is a directory, or if the file
 * cannot be read.
 */
fn game_from_path(path: &Path) -> Result<DevcadeGame, Error> {
    log!(Level::Trace, "Reading game from path {:?}", path);
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

async fn game_from_minimal(game: MinimalGame) -> Result<DevcadeGame, Error> {
    network::request_json::<DevcadeGame>(
        format!("{}/{}", api_url(), route::game(game.id.as_str())).as_str(),
    )
    .await
}

pub fn current_game() -> DevcadeGame {
    CURRENT_GAME.lock().unwrap().get_mut().clone()
}

// currently saves to the devcade machine (or local machine if running locally) in the future,
// should ideally use a remote database / something else.
pub async fn persistence_save(group: &str, key: &str, value: &str) -> Result<(), anyhow::Error> {
    log::trace!("saving data to {}/{} ({})", group, key, value);
    let (path, group) = from_group(group);
    let full_key = format!("{}/{}", path, group);

    let mut data = DB.lock().await;
    let mut mod_list = DB_MODIFIED.lock().await;

    let inner = get_submap_or_load(&mut data, full_key.clone()).await?;

    inner.insert(key.to_string(), value.to_string());
    mod_list.insert(full_key);

    Ok(())
}

/**
 * Load a value from using a group and key
 * group will start with a game_id, but can be further subdivided by the game to
 * */
pub async fn persistence_load(group: &str, key: &str) -> Result<String, anyhow::Error> {
    log::trace!("loading data from {}/{}", group, key);
    let (path, file_name) = from_group(group);
    let full_key = format!("{}/{}", path, file_name);

    let mut data = DB.lock().await;

    let inner = get_submap_or_load(&mut data, full_key.clone()).await?;

    inner
        .get(&key.to_string())
        .ok_or_else(|| anyhow!("Could not find key {} in group {}", key, full_key))
        .cloned()
}

/**
 * Flush all pending writes to the filesystem.
 * */
pub async fn persistence_flush() -> Result<(), anyhow::Error> {
    let mut data = DB.lock().await;
    let mut mod_list = DB_MODIFIED.lock().await;

    log::debug!(
        "Flushing data in db to file ({} modified groups)",
        mod_list.len()
    );

    for key in mod_list.iter() {
        let inner = get_submap_or_load(&mut data, key.clone()).await?;
        let file_name = format!("{}.save", key);
        log::debug!("Flushing to {}", file_name);
        let path = Path::new(&file_name);
        let dir = path.parent().expect("path failed to have parents");
        if !dir.exists() {
            fs::create_dir_all(dir).await?;
        }
        fs::write(path, serde_json::to_string(inner)?.as_bytes()).await?;
    }

    mod_list.clear();

    Ok(())
}

/**
 * Flushes all DB changes, and clears the in-memory cache. This shouldn't need to be done often but
 * can be done if some games are storing too much data and we need to save memory. I don't see this
 * actually needing use unless someone is maliciously (or stupidly) trying to store GBs of data at
 * a time.
 * */
pub async fn clear_db() -> Result<(), anyhow::Error> {
    log::info!("Flushing and clearing DB cache");
    persistence_flush().await?;

    let mut data = DB.lock().await;
    data.clear();

    Ok(())
}

fn from_group(group: &str) -> (String, String) {
    let save_path = Path::new(if *ON_MACHINE {
        "/home/devcade/.save"
    } else {
        "./.save"
    });

    let mut parts: Vec<String> = group.split('/').map(|a| a.to_string()).collect();
    let group = parts.pop().unwrap_or_default();
    let save_path = save_path.join(parts.join("/"));
    (save_path.to_str().unwrap_or("").to_string(), group)
}

/**
 * Gets the sub-map at a specified path, and returns the cached version, the version on the
 * filesystem, or a new empty HashMap, in order of preference.
 * */
async fn get_submap_or_load(
    db: &mut HashMap<String, HashMap<String, String>>,
    group: String,
) -> Result<&mut HashMap<String, String>, anyhow::Error> {
    let file_name = format!("{}.save", group);
    if !db.contains_key(&group) {
        if Path::new(&file_name).exists() {
            let map = serde_json::from_str::<HashMap<String, String>>(
                fs::read_to_string(file_name).await?.as_str(),
            )?;
            db.insert(group.clone(), map);
        } else {
            db.insert(group.clone(), HashMap::new());
        }
    }
    Ok(db.get_mut(&group).unwrap())
}

/**
 * Gets the total number of K, V pairs across the entire cache, as a rough proxy for how large the
 * current cache is.
 * */
pub async fn db_cache_size() -> usize {
    let data = DB.lock().await;
    data.values().map(|hm| hm.len()).sum()
}
