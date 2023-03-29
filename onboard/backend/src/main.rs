#![feature(drain_filter)]

use env_logger;
use futures_util::FutureExt;
use log::{log, Level};
use tokio;
use tokio::io::{AsyncReadExt, AsyncWriteExt};
use tokio::net::unix::pipe::{OpenOptions, Receiver, Sender};

use backend::command::{handle, Request, Response};
use backend::env::devcade_path;

#[tokio::main]
async fn main() -> ! {
    env_logger::init();

    let command_pipe = format!("{}/read_onboard.pipe", devcade_path());
    let response_pipe = format!("{}/write_onboard.pipe", devcade_path());

    log!(Level::Debug, "command_pipe: {}", command_pipe);
    log!(Level::Debug, "response_pipe: {}", response_pipe);

    let onboard_command_pipe = match OpenOptions::new()

        // opening read_write allows the write end of the pipe to close without causing the read
        // end to close as well. This is necessary as if there is an unexpected error in the onboard
        // this will allow the main process to continue and wait for the onboard to restart.
        .read_write(true)
        .open_receiver(command_pipe) {
        Ok(pipe) => pipe,
        Err(e) => {
            log!(Level::Error, "Error opening command pipe: {}", e);
            panic!();
        }
    };

    let onboard_response_pipe = match OpenOptions::new()
        .open_sender(response_pipe) {
        Ok(pipe) => pipe,
        Err(e) => {
            log!(Level::Error, "Error opening response pipe: {}", e);
            panic!();
        }
    };

    // spawn a new thread for the onboard_main function
    tokio::spawn(async move {
        onboard_main(onboard_command_pipe, onboard_response_pipe).await;
    });

    // spawn a new thread for the game save/load handling


    // Spin forever
    loop {
        tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;
    }
}

/**
 * Main function for the onboard process. This function handles all communication to/from the onboard
 * process. It reads commands from the command pipe and writes responses to the response pipe.
 *
 * This function will never return.
 */
async fn onboard_main(mut command_pipe: Receiver, mut response_pipe: Sender) -> ! {
    // Vector for holding all the response futures so we can continue to read from the command pipe
    // while we wait for handle to finish.
    let mut response_futures = vec![];

    // Main command handling loop
    loop {
        let mut buffer = String::new();
        match command_pipe.read_to_string(&mut buffer).await {
            Ok(_) => {
                log!(Level::Debug, "[Onboard   ] Read from command pipe: {}", buffer);
            }
            Err(e) => {
                log!(Level::Error, "[Onboard   ] Error reading from command pipe: {}", e);
                continue; // TODO: Should we panic here?
            }
        }

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
            log!(Level::Warn, "[Onboard   ] No valid commands found in command pipe");
            continue;
        }

        let response_stream = commands.into_iter().map(move |command| {
            log!(Level::Debug, "[Onboard   ] Handling command: {:?}", command);
            handle(command).shared()
        });

        response_futures.append(&mut response_stream.collect::<Vec<_>>());

        // Use drain rather than into_iter so we can take ownership of the contents but still use
        // the original vector for the next iteration.
        let mut drain = response_futures.drain(0..response_futures.len());
        let mut still_waiting = vec![];

        // for each response that has finished, write it to the response pipe
        while let Some(f) = drain.next() {
            match f.clone().now_or_never() {
                Some(res) => {
                    log!(Level::Debug, "[Onboard   ] Got response: {:?}", res);
                    let response = serde_json::to_string(&res).expect("Failed to serialize response");
                    match response_pipe.write_all(response.as_bytes()).await {
                        Ok(_) => {
                            log!(Level::Debug, "[Onboard   ] Wrote response to pipe");
                        }
                        Err(e) => {
                            log!(Level::Error, "[Onboard   ] Error writing response to pipe: {}", e);
                        }
                    }
                }
                None => {
                    still_waiting.push(f);
                }
            }
        }

        // explicitly drop drain to allow borrowing response_futures as mutable
        // It has iterated through all the elements, so we don't need it anymore.
        drop(drain);

        response_futures.append(&mut still_waiting);
    }
}