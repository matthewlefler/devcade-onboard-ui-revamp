use std::error::Error;
use std::path::Path;
use crate::env::{api_url, devcade_path};
use crate::DevcadeGame;

/**
 * Internal module for network requests and JSON serialization
 */
mod network {
    use std::error::Error;
    use serde::Deserialize;

    /**
     * Request JSON from a URL and serialize it into a struct
     */
    pub async fn request_json<T: for<'de> Deserialize<'de>>(url: &str) -> Result<T, Box<dyn Error>> {
        let response = reqwest::get(url).await?;
        let json = response.json().await?;
        Ok(json)
    }

    /**
     * Request text from a URL
     */
    pub async fn request_text(url: &str) -> Result<String, Box<dyn Error>> {
        let response = reqwest::get(url).await?;
        let text = response.text().await?;
        Ok(text)
    }

    /**
     * Request binary data from a URL
     */
    pub async fn request_bytes(url: &str) -> Result<Vec<u8>, Box<dyn Error>> {
        let response = reqwest::get(url).await?;
        let bytes = response.bytes().await?;
        Ok(bytes.to_vec())
    }
}

mod route {
    pub fn game_list() -> String {
        String::from("/games/")
    }

    pub fn game(id: &str) -> String {
        format!("/games/{}", id)
    }

    pub fn game_icon(id: &str) -> String {
        format!("/games/{}/icon", id)
    }

    pub fn game_banner(id: &str) -> String {
        format!("/games/{}/banner", id)
    }

    pub fn game_download(id: &str) -> String {
        format!("/games/{}/game", id)
    }
}

/**
 * Get a list of games from the API. This is the preferred method of getting games.
 */
pub async fn game_list() -> Result<Vec<DevcadeGame>, Box<dyn Error>> {
    let games = network::request_json(format!("{}/{}", api_url(), route::game_list()).as_str()).await?;
    Ok(games)
}

/**
 * Get the list of games currently installed on the filesystem. This can be used if the API is down.
 * This is not the preferred method of getting games.
 */
pub fn game_list_from_fs() -> Result<Vec<DevcadeGame>, Box<dyn Error>> {
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

            let game = game_from_path(path_.to_str().unwrap());
            if game.is_ok() {
                games.push(game.unwrap());
            }
        }
    }
    Ok(games)
}

/**
 * Download's a game's banner from the API.
 */
pub async fn download_banner(game_id: String) -> Result<(), Box<dyn Error>> {

    let path = Path::new(devcade_path().as_str()).join(game_id.clone()).join("banner.png");
    if path.exists() {
        return Ok(());
    }
    let bytes = network::request_bytes(format!("{}/{}", api_url(), route::game_banner(game_id.as_str())).as_str()).await?;
    std::fs::write(path, bytes)?;
    Ok(())
}

/**
 * Download's a game's icon from the API.
 */
pub async fn download_icon(game_id: String) -> Result<(), Box<dyn Error>> {
    let api_url = api_url();
    let file_path = devcade_path();

    let path = Path::new(file_path.as_str()).join(game_id.clone()).join("icon.png");
    if path.exists() {
        return Ok(());
    }
    let bytes = network::request_bytes(format!("{}/{}", api_url, route::game_icon(game_id.as_str())).as_str()).await?;
    std::fs::write(path, bytes)?;
    Ok(())
}

/**
 * Download's a game's zip file from the API and unzips it into the game's directory. If the game is
 * already downloaded, it will check if the hash is the same. If it is, it will not download the game
 * again.
 */
pub async fn download_game(game_id: String) -> Result<(), Box<dyn Error>> {
    let path = Path::new(devcade_path().as_str()).join(game_id.clone()).join("game.json");

    let games = game_list().await?;
    let game = match games.iter().find(|g| g.id == game_id) {
        Some(game) => game,
        None => return Err("Game not found".into()),
    };

    // Check if the game is already downloaded, and if it is, check if the hash is the same
    if path.exists() {
        let game_ = game_from_path(path.to_str().unwrap())?;
        if game_.hash == game.hash {
            return Ok(());
        }
    }

    let bytes = network::request_bytes(format!("{}/{}", api_url(), route::game_download(game_id.as_str())).as_str()).await?;

    // Unzip the game into the game's directory
    let mut zip = zip::ZipArchive::new(std::io::Cursor::new(bytes))?;
    for i in 0..zip.len() {
        let mut file = zip.by_index(i)?;
        let outpath = Path::new(devcade_path().as_str()).join(game.id.clone()).join(file.name());
        if file.name().ends_with('/') {
            std::fs::create_dir_all(&outpath)?;
        } else {
            if let Some(p) = outpath.parent() {
                if !p.exists() {
                    std::fs::create_dir_all(&p)?;
                }
            }
            let mut outfile = std::fs::File::create(&outpath)?;
            std::io::copy(&mut file, &mut outfile)?;
        }
    }

    // Write the game's JSON file to the game's directory (this is used later to get the games from
    // the filesystem)
    let json = serde_json::to_string(game)?;
    std::fs::write(path.join("game.json"), json)?;
    Ok(())
}

/**
 * Launch a game by its ID. This will check if the game is downloaded, and if it is, it will launch
 * the game. This will block until the game is closed.
 */
pub async fn launch_game(game_id: String) -> Result<(), Box<dyn Error>> {
    let path = Path::new(devcade_path().as_str()).join(game_id.clone()).join("/publish");

    if !path.exists() {
        return Err("Game not downloaded".into());
    }

    // Infer executable name from *.runtimeconfig.json
    let mut executable = String::new();

    for entry in std::fs::read_dir(path.clone())? {
        let entry = entry?;
        let path = entry.path();
        if !path.is_file() {
            continue;
        }
        if let Some(extension) = path.extension() {
            if extension != "runtimeconfig.json" {
                continue;
            }
            executable = path.file_stem().unwrap().to_str().unwrap().to_string();
            break;
        }
    }

    // If no *.runtimeconfig.json file is found, look for a file with the same name as the game
    // (this is the case for games that don't use .NET)
    // TODO: Some better way to find executable name?
    if executable.is_empty() {
        let game = game_from_path(path.clone().parent().unwrap().join("game.json").to_str().unwrap())?;
        executable = game.name;
    }

    let path = path.join(executable);

    if !path.exists() {
        return Err("Game executable not found".into());
    }

    let mut child = std::process::Command::new(path)
        .spawn()
        .expect("failed to execute process");

    child.wait().expect("failed to wait on child");

    Ok(())
}

/**
 * Returns a devcade game if the file at the path is a JSON file contianing a devcade game
 */
fn game_from_path(path: &str) -> Result<DevcadeGame, Box<dyn Error>> {
    let path = Path::new(path);
    if !path.exists() {
        return Err("Path does not exist".into());
    }
    if path.is_dir() {
        return Err("Path is a directory".into());
    }
    let str = std::fs::read_to_string(path)?;

    let game: DevcadeGame = serde_json::from_str(&str)?;

    Ok(game)
}
