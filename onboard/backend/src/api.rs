use std::error::Error;
use std::path::Path;
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

const API_URL: &str = "https://devcade-api.apps.okd4.csh.rit.edu/api"; // TODO: Get from ENV
const FILE_PATH: &str = "/tmp/devcade"; // TODO: Get from ENV

/**
 * Get a list of games from the API. This is the preferred method of getting games.
 */
pub async fn game_list() -> Result<Vec<DevcadeGame>, Box<dyn Error>> {
    let games = network::request_json(format!("{}/games", API_URL).as_str()).await?;
    Ok(games)
}

/**
 * Get the list of games currently installed on the filesystem. This can be used if the API is down.
 * This is not the preferred method of getting games.
 */
pub fn game_list_from_fs() -> Result<Vec<DevcadeGame>, Box<dyn Error>> {
    let mut games = Vec::new();
    for entry in std::fs::read_dir(FILE_PATH)? {
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
    let path = Path::new(FILE_PATH).join(game_id.clone()).join("banner.png");
    if path.exists() {
        return Ok(());
    }
    let bytes = network::request_bytes(format!("{}/games/download/banner/{}", API_URL, game_id).as_str()).await?;
    std::fs::write(path, bytes)?;
    Ok(())
}

/**
 * Download's a game's icon from the API.
 */
pub async fn download_icon(game_id: String) -> Result<(), Box<dyn Error>> {
    let path = Path::new(FILE_PATH).join(game_id.clone()).join("icon.png");
    if path.exists() {
        return Ok(());
    }
    let bytes = network::request_bytes(format!("{}/games/download/icon/{}", API_URL, game_id).as_str()).await?;
    std::fs::write(path, bytes)?;
    Ok(())
}

/**
 * Download's a game's zip file from the API and unzips it into the game's directory. If the game is
 * already downloaded, it will check if the hash is the same. If it is, it will not download the game
 * again.
 */
pub async fn download_game(game_id: String) -> Result<(), Box<dyn Error>> {
    let path = Path::new(FILE_PATH).join(game_id.clone()).join("game.json");

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

    let bytes = network::request_bytes(format!("{}/games/download/game", API_URL).as_str()).await?;

    // Unzip the game into the game's directory
    let mut zip = zip::ZipArchive::new(std::io::Cursor::new(bytes))?;
    for i in 0..zip.len() {
        let mut file = zip.by_index(i)?;
        let outpath = Path::new(FILE_PATH).join(game.id.clone()).join(file.name());
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
