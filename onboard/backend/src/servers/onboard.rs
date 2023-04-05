use std::process::ExitStatus;
use std::thread::JoinHandle;
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
pub async fn main(command_pipe: &str, response_pipe: &str) -> ! {
    // Vector for holding all the response futures so we can continue to read from the command pipe
    // while we wait for handle to finish.

    log!(Level::Info, "Starting onboard process");

    let command_pipe_path = command_pipe;
    let command_pipe = loop {
        match open_read_pipe(command_pipe) {
            Ok(pipe) => break pipe,
            Err(e) => {
                {
                    log!(Level::Error, "Error opening command pipe: {}", e);
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

    let mut game_handle: Option<(u32, JoinHandle<ExitStatus>)> = None;

    // Main command handling loop
    loop {
        let mut buffer = [0; 4096];
        match command_pipe.try_read(&mut buffer) {
            Ok(_) => {
                log!(Level::Trace, "Read from command pipe: {}", String::from_utf8_lossy(&buffer).trim());
            }
            Err(e) => {
                if e.kind() == std::io::ErrorKind::WouldBlock {
                    // This happens when the pipe is empty, no handling needed
                    // allow the loop to continue with the buffer as an empty slice
                    // TODO: Check if allowing try_read without pipe.readable() will cause a read while the pipe is being written to
                } else {
                    log!(Level::Error, "Error reading from command pipe: {}", e);
                    continue; // TODO: Should we panic here? Or do some kind of recovery?
                }
            }
        }

        let mut buffer = String::from_utf8_lossy(&buffer).to_string();
        buffer = buffer.trim().to_string();
        buffer = buffer.trim_end_matches('\0').to_string(); // Rust doesn't treat NUL as whitespace or end of string

        if let Some((id, handle)) = game_handle.take() {
            if handle.is_finished() {
                log!(Level::Info, "Game with Request ID {} has finished", id);
                let response = Response::Ok(id);
                let mut response = match serde_json::to_string(&response) {
                    Ok(r) => r,
                    Err(e) => {
                        log!(Level::Error, "Error serializing response: {}", e);
                        continue;
                    }
                };
                log!(Level::Trace, "Writing response to pipe: {}", response);
                response.push('\n');
                match response_pipe.write_all(response.as_bytes()).await {
                    Ok(_) => {},
                    Err(e) => {
                        log!(Level::Error, "Error writing to response pipe: {}", e);
                    }
                }
            } else {
                // Game is still running, put it back in the option
                let _: &mut (u32, JoinHandle<ExitStatus>) = game_handle.insert((id, handle));
            }
        }

        if buffer.is_empty() {
            tokio::time::sleep(tokio::time::Duration::from_millis(200)).await;
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
            log!(Level::Warn, "Read from command pipe, but no valid commands were found (read {})", buffer);
            continue;
        }

        let command_futures = commands
            .into_iter()
            .map(handle)
            .collect::<Vec<_>>();

        for res in command_futures {
            let response = res.await;

            if let Response::Err(id, e) = &response {
                log!(Level::Warn, "Error handling command {}: {}", id, e);
            }
            
            let response = match response {
                Response::InternalGame(id, handle) => {
                    let _: &mut (u32, JoinHandle<ExitStatus>) = game_handle.insert((id, handle));
                    continue;
                }
                _ => response
            };

            let mut response = match serde_json::to_string(&response) {
                Ok(r) => r,
                Err(e) => {
                    log!(Level::Error, "Error serializing response: {}", e);
                    continue;
                }
            };
            log!(Level::Trace, "Writing response to pipe: {}", response);
            response.push('\n');
            match response_pipe.write_all(response.as_bytes()).await {
                Ok(_) => {},
                Err(e) => {
                    log!(Level::Error, "Error writing to response pipe: {}", e);
                }
            }
        }
    }
}