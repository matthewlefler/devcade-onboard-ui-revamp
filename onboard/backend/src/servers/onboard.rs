use log::{log, Level};
use tokio::io::AsyncWriteExt;
use crate::{open_read_pipe, open_write_pipe};
use crate::command::{handle, Request, Response};

/**
 * Main function for the onboard process. This function handles all communication to/from the onboard
 * process. It reads commands from the command pipe and writes responses to the response pipe.
 *
 * This function will never return unless it panics and should be spawned as a thread.
 */
pub async fn onboard_main(command_pipe: &str, response_pipe: &str) -> ! {
    // Vector for holding all the response futures so we can continue to read from the command pipe
    // while we wait for handle to finish.

    log!(Level::Info, "Starting onboard process");

    let command_pipe_path = command_pipe;
    let command_pipe = loop {
        match open_read_pipe(command_pipe) {
            Ok(pipe) => break pipe,
            Err(e) => {
                match e {
                    _ => {
                        log!(Level::Error, "Error opening command pipe: {}", e);
                    }
                }
                tokio::time::sleep(tokio::time::Duration::from_millis(500)).await;
            }
        }
    };

    log!(Level::Debug, "Opened command pipe at {}", command_pipe_path);

    // Open the response pipe
    // This is opening here as it requires a receiver on the other end, which is opened by in C#.
    let response_pipe_path = response_pipe;
    let mut response_pipe = loop {
        match open_write_pipe(response_pipe) {
            Ok(pipe) => break pipe,
            Err(e) => {
                match e.to_string().as_str() {
                    "No such device or address (os error 6)" => {} // This is expected, just wait for the other end to open
                    _ => {
                        log!(Level::Error, "Error opening response pipe: {}", e);
                    }
                }
                tokio::time::sleep(tokio::time::Duration::from_millis(500)).await;
            }
        }
    };

    log!(Level::Debug, "Opened response pipe at {}", response_pipe_path);

    // Main command handling loop
    loop {
        log!(Level::Trace, "Waiting for command");

        match command_pipe.readable().await {
            Ok(_) => {},
            Err(e) => {
                log!(Level::Error, "Error waiting for command pipe to be readable: {}", e);
                continue; // TODO: Should we panic here? Or do some kind of recovery?
            }
        }

        let mut buffer = [0; 4096];
        match command_pipe.try_read(&mut buffer) {
            Ok(_) => {
                log!(Level::Trace, "Read from command pipe: {}", String::from_utf8_lossy(&buffer));
            }
            Err(e) => {
                match e.kind() {
                    std::io::ErrorKind::WouldBlock => {
                        // For some reason, the pipe is readable but we can't read from it.
                        // This happens after every loop exactly once, so we just ignore it.
                        log!(Level::Trace, "Command pipe not readable");
                        continue;
                    }
                    _ => {
                        log!(Level::Error, "Error reading from command pipe: {}", e);
                        continue; // TODO: Should we panic here? Or do some kind of recovery?
                    }
                }
            }
        }

        let buffer = String::from_utf8_lossy(&buffer);

        if buffer.is_empty() {
            tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;
            continue;
        }

        let parts = buffer
            .lines()
            .filter(|line| !line.is_empty())
            .collect::<Vec<&str>>();

        let commands = parts
            .iter()
            .filter_map(|line| serde_json::from_str::<Request>(line).ok())
            .collect::<Vec<Request>>();

        if commands.is_empty() {
            log!(Level::Warn, "Read from command pipe, but no valid commands were found");
            continue;
        }

        let command_futures = commands
            .into_iter()
            .map(|command| handle(command))
            .collect::<Vec<_>>();

        for res in command_futures {
            let response = res.await;
            match &response {
                Response::Err(id, e) => {
                    log!(Level::Warn, "Error handling command {}: {}", id, e);
                }
                _ => {}
            }
            let response = serde_json::to_string(&response).unwrap();
            log!(Level::Trace, "Writing response to pipe: {}", response);
            match response_pipe.write_all(response.as_bytes()).await {
                Ok(_) => {},
                Err(e) => {
                    log!(Level::Error, "Error writing to response pipe: {}", e);
                }
            }
        }
    }
}