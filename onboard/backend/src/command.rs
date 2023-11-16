use crate::api::{self, nfc_user};

use crate::api::{
    download_banner, download_game, download_icon, game_list, game_list_from_fs, launch_game,
    nfc_tags, persistence_flush, persistence_load, persistence_save, tag_games, tag_list, user,
};
use devcade_onboard_types::{RequestBody, ResponseBody};

/**
 * Handle a request from the frontend.
 */
pub async fn handle(req: RequestBody) -> ResponseBody {
    match req {
        RequestBody::Ping => ResponseBody::Pong,
        RequestBody::GetGameList => match game_list().await {
            Ok(games) => ResponseBody::GameList(games),
            Err(_) => match game_list_from_fs() {
                Ok(games) => ResponseBody::GameList(games),
                Err(err) => err.into(),
            },
        },
        RequestBody::GetGameListFromFs => match game_list_from_fs() {
            Ok(games) => ResponseBody::GameList(games),
            Err(err) => err.into(),
        },
        RequestBody::GetGame(game_id) => match game_list().await {
            Ok(game) => match game.into_iter().find(|g| g.id == game_id) {
                Some(game) => ResponseBody::Game(game),
                None => ResponseBody::Err(format!("Game with ID {game_id} not found")),
            },
            Err(err) => err.into(),
        },
        RequestBody::DownloadGame(game_id) => match download_game(game_id).await {
            Ok(_) => ResponseBody::Ok,
            Err(err) => err.into(),
        },
        RequestBody::DownloadIcon(game_id) => match download_icon(game_id).await {
            Ok(_) => ResponseBody::Ok,
            Err(err) => err.into(),
        },
        RequestBody::DownloadBanner(game_id) => match download_banner(game_id).await {
            Ok(_) => ResponseBody::Ok,
            Err(err) => err.into(),
        },
        RequestBody::LaunchGame(game_id) => match launch_game(game_id).await {
            Ok(_) => ResponseBody::Ok,
            Err(err) => err.into(),
        },
        RequestBody::SetProduction(prod) => {
            crate::env::set_production(prod);
            ResponseBody::Ok
        }
        RequestBody::GetTagList => match tag_list().await {
            Ok(tags) => ResponseBody::TagList(tags),
            Err(err) => err.into(),
        },
        RequestBody::GetTag(tag_name) => match tag_list().await {
            Ok(tags) => match tags.into_iter().find(|t| t.name == tag_name) {
                Some(tag) => ResponseBody::Tag(tag),
                None => ResponseBody::Err(format!("Tag with name {tag_name} not found")),
            },
            Err(err) => err.into(),
        },
        RequestBody::GetGameListFromTag(tag_name) => match tag_games(tag_name).await {
            Ok(games) => ResponseBody::GameList(games),
            Err(err) => err.into(),
        },
        RequestBody::GetUser(uid) => match user(uid).await {
            Ok(user) => ResponseBody::User(user),
            Err(err) => err.into(),
        },
        RequestBody::GetNfcTag(reader_id) => match nfc_tags(reader_id).await {
            Ok(association_id) => ResponseBody::NfcTag(association_id),
            Err(err) => err.into(),
        },
        RequestBody::GetNfcUser(association_id) => match nfc_user(association_id).await {
            Ok(user) => ResponseBody::NfcUser(user),
            Err(err) => err.into(),
        },
        RequestBody::Save(group, key, value) => {
            let group = format!("{}/{}", api::current_game().id, group);
            match persistence_save(group.as_str(), key.as_str(), value.as_str()).await {
                Ok(()) => ResponseBody::Ok,
                Err(err) => err.into(),
            }
        }
        RequestBody::Load(group, key) => {
            let group = format!("{}/{}", api::current_game().id, group);
            match persistence_load(group.as_str(), key.as_str()).await {
                Ok(s) => ResponseBody::Object(s),
                Err(err) => err.into(),
            }
        }
        RequestBody::Flush => match persistence_flush().await {
            Ok(()) => ResponseBody::Ok,
            Err(err) => err.into(),
        },
    }
}
