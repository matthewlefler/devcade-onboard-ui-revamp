use crate::api::{download_banner, download_game, download_icon, game_list, game_list_from_fs};
use crate::DevcadeGame;

/**
 * A request received by the backend from the frontend. The u32 is the request ID, which is used to
 * identify the request in the response to the frontend.
 */
pub enum Request {
    GetGameList(u32),
    GetGameListFromFs(u32),
    GetGame(u32, String), // String is the game ID
    DownloadGame(u32, String), // String is the game ID
    DownloadIcon(u32, String), // String is the game ID
    DownloadBanner(u32, String), // String is the game ID
}

/**
 * A response sent by the backend to the frontend. The u32 is the request ID, which is used to
 * identify the request in the response to the frontend.
 */
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
        Request::GetGameList(id) => {
            match game_list().await {
                Ok(games) => Response::game_list_from_id(id, games),
                Err(err) => Response::err_from_id_and_err(id, err),
            }
        },
        Request::GetGameListFromFs(id) => {
            match game_list_from_fs() {
                Ok(games) => Response::game_list_from_id(id, games),
                Err(err) => Response::err_from_id_and_err(id, err),
            }
        },
        Request::GetGame(id, game_id) => {
            match game_list().await {
                Ok(game) => {
                    match game.into_iter().find(|g| g.id == game_id) {
                        Some(game) => Response::game_from_id(id, game),
                        None => Response::err_from_id(id, format!("Game with ID {} not found", game_id)),
                    }
                }
                Err(err) => Response::err_from_id_and_err(id, err),
            }
        },
        Request::DownloadGame(id, game_id) => {
            match download_game(game_id).await {
                Ok(_) => Response::ok_from_id(id),
                Err(err) => Response::err_from_id_and_err(id, err),
            }
        },
        Request::DownloadIcon(id, game_id) => {
            match download_icon(game_id).await {
                Ok(_) => Response::ok_from_id(id),
                Err(err) => Response::err_from_id_and_err(id, err),
            }
        },
        Request::DownloadBanner(id, game_id) => {
            match download_banner(game_id).await {
                Ok(_) => Response::ok_from_id(id),
                Err(err) => Response::err_from_id_and_err(id, err),
            }
        },
    }
}