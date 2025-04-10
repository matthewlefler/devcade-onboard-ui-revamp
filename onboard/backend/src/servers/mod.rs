use anyhow::anyhow;
use futures_util::future;
use futures_util::FutureExt;
use log::{log, Level};
use std::fs::remove_file;
use std::future::Future;
use std::process::{Command, Stdio};
use std::sync::Arc;
use tokio::io::{AsyncBufReadExt, BufReader, Lines, ReadHalf, WriteHalf};
use tokio::net::{UnixListener, UnixStream};
use tokio::task;
use tokio::task::JoinError;

/**
 * Module for getting the paths to the pipes that the servers use to communicate
 */
pub mod path {
    use crate::env::devcade_path;

    /**
     * Get the path to the pipe that the frontend will write to
     */
    #[must_use]
    pub fn onboard_pipe() -> String {
        format!("{}/onboard.sock", devcade_path())
    }

    /**
     * Get the path to the pipe that the game thread will write to
     * */
    #[must_use]
    pub fn game_pipe() -> String {
        format!("{}/game.sock", devcade_path())
    }
}

/**
 * The onboard server is responsible for communicating with the frontend
 */
pub mod onboard;

/**
 * The game server is responsible for communicating with the running game and saving /
 * loading persistent data.
 * */
pub mod game;

/**
 * A struct to hold the handles to the threads spawned by the backend.
 */
pub struct ThreadHandles {
    /**
     * The handle to the onboard server thread
     */
    onboard: Option<tokio::task::JoinHandle<()>>,

    /**
     * The handle to the game server thread (handles save/load with the currently running game)
     */
    game_sl: Option<tokio::task::JoinHandle<()>>,
    /**
     * The handle to the gatekeeper thread (handles authentication for CSH users)
     */
    gatekeeper: Option<tokio::task::JoinHandle<()>>,
}

impl ThreadHandles {
    /**
     * Create a new empty ThreadHandles struct
     */
    #[must_use]
    pub fn new() -> Self {
        Self {
            onboard: None,
            game_sl: None,
            gatekeeper: None,
        }
    }

    /**
     * Restart the onboard server thread with the given pipe
     */
    pub fn restart_onboard(&mut self, command_pipe: String) {
        log!(Level::Info, "Starting onboard thread ...");
        self.onboard = Some(tokio::spawn(async move {
            onboard::main(command_pipe.as_str()).await;
        }));
    }

    /**
     * Restart the save / load server thread with the given pipe
     * */
    pub fn restart_game(&mut self, command_pipe: String) {
        log!(Level::Info, "Starting game thread ...");
        self.game_sl = Some(tokio::spawn(async move {
            game::main(command_pipe.as_str()).await;
        }));
    }

    /**
     * Check if the onboard server thread has errored and return the error if it has
     */
    pub fn onboard_error(&mut self) -> Option<JoinError> {
        if let Some(handle) = &self.onboard {
            if handle.is_finished() {
                let handle = self.onboard.take().unwrap();
                return handle.now_or_never()?.err();
            }
        }
        None
    }

    /**
     * Check if the game thread has errored and return the error if it has
     */
    pub fn game_error(&mut self) -> Option<JoinError> {
        if let Some(handle) = &self.game_sl {
            if handle.is_finished() {
                let handle = self.game_sl.take().unwrap();
                return handle.now_or_never()?.err();
            }
        }
        None
    }

    /**
     * Check if the gatekeeper thread has errored and return the error if it has
     */
    pub fn gatekeeper_error(&mut self) -> Option<JoinError> {
        if let Some(handle) = &self.gatekeeper {
            if handle.is_finished() {
                let handle = self.gatekeeper.take().unwrap();
                return handle.now_or_never()?.err();
            }
        }
        None
    }
}

impl Default for ThreadHandles {
    fn default() -> Self {
        Self::new()
    }
}

pub async fn open_server<'a, T, U>(path: &str, handle_client: T) -> !
where
    T: (Fn(Lines<BufReader<ReadHalf<UnixStream>>>, WriteHalf<UnixStream>) -> U)
        + Send
        + Sync
        + 'a + 'static,
    U: Future<Output = Result<(), anyhow::Error>> + Send + Sync + 'a + 'static,
{
    let listener = bind_listener(path).unwrap();
    let handle_client = Arc::new(handle_client);

    let mut handles = vec![];
    while let Ok((stream, _address)) = listener.accept().await {
        let handle_client = handle_client.clone();
        handles.push(task::spawn(async move {
            let (reader, writer) = tokio::io::split(stream);
            let reader = BufReader::new(reader);

            match handle_client(reader.lines(), writer).await {
                Ok(()) => log::info!("Finished handling connections from client"),
                Err(err) => log::error!("Finished handling connections from client: {:?}", err),
            }
        }));
    }
    future::join_all(handles).await;
    panic!("Looks like our server stopped serving?! This shouldn't happen.");
}

fn bind_listener(path: &str) -> Result<UnixListener, anyhow::Error> {
    match UnixListener::bind(path) {
        Ok(l) => Ok(l),
        Err(e) => match e.to_string().as_str() {
            "Address already in use (os error 98)" => {
                // Using std::process::Command instead of tokio equivalent because lsof doesn't
                // take long enough to bother
                let lsof = Command::new("lsof")
                    .arg(path)
                    .stdout(Stdio::null())
                    .status()?;
                if lsof.success() {
                    // lsof returns success if any process is using this file
                    Err(anyhow!("Failed to bind listener to path {}: {}", path, e))
                } else {
                    log::debug!("Socket was not closed correctly in last shutdown. Removing");
                    remove_file(path)?;
                    bind_listener(path)
                }
            }
            _ => Err(anyhow!("Failed to bind listener to path {}: {}", path, e)),
        },
    }
}
