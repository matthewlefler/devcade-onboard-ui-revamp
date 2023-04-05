
use log::{log, Level};

use backend::servers::path::{onboard_command_pipe, onboard_response_pipe};
use backend::servers::ThreadHandles;

#[tokio::main]
async fn main() -> ! {
    match dotenv::from_filename("../.env") {
        Ok(_) => (),
        Err(e) => {
            log!(Level::Error, "Error loading .env file: {}", e);
        }
    }
    env_logger::init();

    let mut handles: ThreadHandles = ThreadHandles::new();
    handles.restart_onboard(onboard_command_pipe(), onboard_response_pipe());

    // TODO Game Save / Load

    // TODO Gatekeeper / Authentication

    // Main loop
    loop {
        tokio::time::sleep(tokio::time::Duration::from_millis(1000)).await;
        // Check if any of the handles have finished
        if let Some(err) = handles.onboard_error() {
            log!(Level::Error, "Onboard thread has errored: {}", err);
            handles.restart_onboard(onboard_command_pipe(), onboard_response_pipe());
        }
        if let Some(err) = handles._game_error() {
            log!(Level::Error, "Game thread has errored: {}", err);
            // TODO Restart game thread
        }
        if let Some(err) = handles._gatekeeper_error() {
            log!(Level::Error, "Gatekeeper thread has errored: {}", err);
            // TODO Restart gatekeeper thread
        }
    }
}