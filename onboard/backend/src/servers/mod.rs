use futures_util::FutureExt;
use log::{log, Level};
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
    pub fn onboard_command_pipe() -> String {
        format!("{}/read_onboard.pipe", devcade_path())
    }

    /**
     * Get the path to the pipe that the frontend will read from
     */
    #[must_use]
    pub fn onboard_response_pipe() -> String {
        format!("{}/write_onboard.pipe", devcade_path())
    }
}

/**
 * The onboard server is responsible for communicating with the frontend
 */
pub mod onboard;

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
     * Restart the onboard server thread with the given pipes
     */
    pub fn restart_onboard(&mut self, command_pipe: String, response_pipe: String) {
        log!(Level::Info, "Starting onboard thread ...");
        self.onboard = Some(tokio::spawn(async move {
            onboard::main(command_pipe.as_str(), response_pipe.as_str()).await;
        }));
    }

    /**
     * Check if the onboard server thread has errored and return the error if it has
     */
    pub fn onboard_error(&mut self) -> Option<JoinError> {
        let handle = self.onboard.take();
        if let Some(handle) = handle {
            return if handle.is_finished() {
                Some(handle.now_or_never()?.err()?)
            } else {
                self.onboard = Some(handle);
                None
            };
        }
        None
    }

    /**
     * Check if the game thread has errored and return the error if it has
     */
    pub fn _game_error(&mut self) -> Option<JoinError> {
        let handle = self.game_sl.take();
        if let Some(handle) = handle {
            return if handle.is_finished() {
                Some(handle.now_or_never()?.err()?)
            } else {
                self.game_sl = Some(handle);
                None
            };
        }
        None
    }

    /**
     * Check if the gatekeeper thread has errored and return the error if it has
     */
    pub fn _gatekeeper_error(&mut self) -> Option<JoinError> {
        let handle = self.gatekeeper.take();
        if let Some(handle) = handle {
            return if handle.is_finished() {
                Some(handle.now_or_never()?.err()?)
            } else {
                self.gatekeeper = Some(handle);
                None
            };
        }
        None
    }
}

impl Default for ThreadHandles {
    fn default() -> Self {
        Self::new()
    }
}
