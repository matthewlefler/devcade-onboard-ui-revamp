use serde::{Deserialize, Serialize};
use std::fmt::Display;

use crate::api::{download_banner, download_game, download_icon, game_list, game_list_from_fs};
use crate::DevcadeGame;

/**
 * A request received by the backend from the frontend. The u32 is the request ID, which is used to
 * identify the request in the response to the frontend.
 */
#[derive(Debug, Clone, Serialize, Deserialize)]
pub enum Request {
    GetGameList(u32),
    GetGameListFromFs(u32),
    GetGame(u32, String),        // String is the game ID
    DownloadGame(u32, String),   // String is the game ID
    DownloadIcon(u32, String),   // String is the game ID
    DownloadBanner(u32, String), // String is the game ID

    LaunchGame(u32, String), // String is the game ID
}

/**
 * A response sent by the backend to the frontend. The u32 is the request ID, which is used to
 * identify the request in the response to the frontend.
 */
#[derive(Debug, Clone, Serialize, Deserialize)]
pub enum Response {
    Ok(u32),
    Err(u32, String),

    GameList(u32, Vec<DevcadeGame>),
    Game(u32, DevcadeGame),
}

impl Response {
    fn ok_from_id(id: u32) -> Self {
        Response::Ok(id)
    }

    fn err_from_id(id: u32, err: String) -> Self {
        Response::Err(id, err)
    }

    fn err_from_id_and_err(id: u32, err: Box<dyn std::error::Error>) -> Self {
        Response::Err(id, err.to_string())
    }

    fn game_list_from_id(id: u32, games: Vec<DevcadeGame>) -> Self {
        Response::GameList(id, games)
    }

    fn game_from_id(id: u32, game: DevcadeGame) -> Self {
        Response::Game(id, game)
    }
}

/**
 * Handle a request from the frontend.
 */
pub async fn handle(req: Request) -> Response {
    match req {
        Request::GetGameList(id) => match game_list().await {
            Ok(games) => Response::game_list_from_id(id, games),
            Err(err) => Response::err_from_id_and_err(id, err),
        },
        Request::GetGameListFromFs(id) => match game_list_from_fs() {
            Ok(games) => Response::game_list_from_id(id, games),
            Err(err) => Response::err_from_id_and_err(id, err),
        },
        Request::GetGame(id, game_id) => match game_list().await {
            Ok(game) => match game.into_iter().find(|g| g.id == game_id) {
                Some(game) => Response::game_from_id(id, game),
                None => Response::err_from_id(id, format!("Game with ID {} not found", game_id)),
            },
            Err(err) => Response::err_from_id_and_err(id, err),
        },
        Request::DownloadGame(id, game_id) => match download_game(game_id).await {
            Ok(_) => Response::ok_from_id(id),
            Err(err) => Response::err_from_id_and_err(id, err),
        },
        Request::DownloadIcon(id, game_id) => match download_icon(game_id).await {
            Ok(_) => Response::ok_from_id(id),
            Err(err) => Response::err_from_id_and_err(id, err),
        },
        Request::DownloadBanner(id, game_id) => match download_banner(game_id).await {
            Ok(_) => Response::ok_from_id(id),
            Err(err) => Response::err_from_id_and_err(id, err),
        },
        Request::LaunchGame(id, game_id) => {
            // TODO: Implement
            Response::ok_from_id(id)
        }
    }
}

impl Display for Request {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Request::GetGameList(id) => write!(f, "[{:9}] Get Game List", id),
            Request::GetGameListFromFs(id) => write!(f, "[{:9}] Get Game List From Filesystem", id),
            Request::GetGame(id, game_id) => {
                write!(f, "[{:9}] Get Game object with id ({})", id, game_id)
            }
            Request::DownloadGame(id, game_id) => {
                write!(f, "[{:9}] Download game with id ({})", id, game_id)
            }
            Request::DownloadIcon(id, game_id) => {
                write!(f, "[{:9}] Download icon with id ({})", id, game_id)
            }
            Request::DownloadBanner(id, game_id) => {
                write!(f, "[{:9}] Download banner with id ({})", id, game_id)
            }
            Request::LaunchGame(id, game_id) => {
                write!(f, "[{:9}] Launch game with id ({})", id, game_id)
            }
        }
    }
}

// Used for debug logging
impl Display for Response {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Response::Ok(id) => write!(f, "[{:9}] Ok", id),
            Response::Err(id, err) => write!(f, "[{:9}] Err: {}", id, err),
            Response::GameList(id, games) => {
                write!(f, "[{:9}] Got game list with {} games", id, games.len())
            }
            Response::Game(id, game) => write!(f, "[{:9}] Downloaded game with id {}", id, game.id),
        }
    }
}

