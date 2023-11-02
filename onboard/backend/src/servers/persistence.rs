use crate::command::handle;
use crate::servers::open_server;
use anyhow::anyhow;
use devcade_onboard_types::{Request, RequestBody, Response, ResponseBody};
use futures_util::future;
use lazy_static::lazy_static;
use std::collections::{HashMap, HashSet};
use std::path::Path;
use std::sync::Arc;
use tokio::fs;
use tokio::io::AsyncWriteExt;
use tokio::sync::Mutex;
use tokio::task;

lazy_static! {
    // basically just checks if a user 'devcade' exists. If so, assumes that this is running on the
    // machine, and saves to the homedir. Otherwise, saves to the cwd.
    static ref ON_MACHINE: bool = Path::new("/home/devcade").exists();
    static ref DB: Mutex<HashMap<String, HashMap<String, String>>> = Mutex::new(HashMap::new());
    static ref DB_MODIFIED: Mutex<HashSet<String>> = Mutex::new(HashSet::new());
}

pub async fn main(command_pipe: &str) -> ! {
    log::info!("Starting save/load process");
    log::debug!("Opened command pipe at {}", command_pipe);

    open_server(command_pipe, async move |mut lines, writer| {
        let writer = Arc::new(Mutex::new(writer));
        let mut handles = vec![];
        log::debug!("New client connected to persistence socket");
        while let Some(line) = lines.next_line().await? {
            let command: Request = serde_json::from_str(&line)?;

            match &command.body {
                RequestBody::Save(_, _, _) | RequestBody::Load(_, _) | RequestBody::Flush => {
                    log::debug!("Handling command: {}", command);
                }
                RequestBody::Ping => {
                    log::trace!("Handling command: {}", command);
                }
                _ => {
                    log::warn!("Invalid command from game: {}", command);
                }
            }

            let writer = writer.clone();

            handles.push(task::spawn(async move {
                let body: ResponseBody = match &command.body {
                    RequestBody::Save(_, _, _)
                    | RequestBody::Load(_, _)
                    | RequestBody::Flush
                    | RequestBody::Ping => handle(command.body).await,
                    // Don't allow game save/load to (for example) download a game, launch a game,
                    // etc. If games could launch other games, it would update the 'current game' in
                    // crate::api and allow games to corrupt other games' save data (possibly
                    // maliciously!)
                    _ => anyhow!("Invalid command: {}", command).into(),
                };
                let response = Response {
                    request_id: command.request_id,
                    body,
                };
                log::debug!("Sending: {response}");
                let mut response = serde_json::to_vec(&response)?;
                response.push(b'\n');

                let mut writer = writer.lock().await;
                writer.write_all(&response).await?;
                Ok(()) as Result<(), anyhow::Error>
            }));
        }

        future::join_all(handles).await;
        log::info!("Persistence thread disconnecting");
        Ok(())
    })
    .await
}

// currently saves to the devcade machine (or local machine if running locally) in the future,
// should ideally use a remote database / something else.
pub async fn save(group: &str, key: &str, value: &str) -> Result<(), anyhow::Error> {
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
pub async fn load(group: &str, key: &str) -> Result<String, anyhow::Error> {
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
pub async fn flush() -> Result<(), anyhow::Error> {
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
    flush().await?;

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
    let group = parts.pop().unwrap_or(String::new());
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
