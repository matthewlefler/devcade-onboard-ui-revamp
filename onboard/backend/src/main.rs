use backend::servers::path::{onboard_command_pipe, onboard_response_pipe};
use backend::servers::ThreadHandles;
use log::{log, Level};

#[tokio::main]
async fn main() -> ! {
    #[cfg(not(target_os = "linux"))]
    {
        compile_error!("This project only supports Linux.\nTo build for linux, run `cargo build --target x86_64-unknown-linux-gnu`");
    }

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
            log!(Level::Error, "Onboard thread has panicked: {}", err);
            handles.restart_onboard(onboard_command_pipe(), onboard_response_pipe());
        }
        if let Some(err) = handles._game_error() {
            log!(Level::Error, "Game thread has panicked: {}", err);
            // TODO Restart game thread (once implemented)
        }
        if let Some(err) = handles._gatekeeper_error() {
            log!(Level::Error, "Gatekeeper thread has panicked: {}", err);
            // TODO Restart gatekeeper thread (once implemented)
        }
    }
}
