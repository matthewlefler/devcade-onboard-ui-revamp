use env_logger;
use futures_util::FutureExt;
use log::{log, Level};
use tokio;
use tokio::io::{AsyncReadExt, AsyncWriteExt};

use backend::command::{handle, Request};
use backend::env::devcade_path;
use backend::{open_read_pipe, open_write_pipe};

#[tokio::main]
async fn main() -> ! {
    match dotenv::from_filename("../.env") {
        Ok(_) => (),
        Err(e) => {
            log!(Level::Error, "Error loading .env file: {}", e);
        }
    }

    env_logger::init();

    let command_pipe = format!("{}/read_onboard.pipe", devcade_path());
    let response_pipe = format!("{}/write_onboard.pipe", devcade_path());

    // spawn a new thread for the onboard_main function
    tokio::spawn(async move {
        onboard_main(
            command_pipe,
            response_pipe,
        ).await;
    });

    let all_requests = Request::all();

    for request in all_requests {
        let json = serde_json::to_string(&request).unwrap();
        log!(Level::Debug, "[Main      ] Example json for request '{}': {}", request, json);
    }

    // Spin forever
    // let mut cx = std::task::Context::from_waker(noop_waker_ref());
    loop {
        // // Poll all the join handles in the vector
        // join_handles.retain_mut(|f| f.poll_unpin(&mut cx).is_pending());
        tokio::time::sleep(tokio::time::Duration::from_millis(500)).await;
    }
}

/**
 * Main function for the onboard process. This function handles all communication to/from the onboard
 * process. It reads commands from the command pipe and writes responses to the response pipe.
 *
 * This function will never return.
 */
async fn onboard_main(command_pipe: String, response_pipe: String) -> ! {
    // Vector for holding all the response futures so we can continue to read from the command pipe
    // while we wait for handle to finish.
    let mut response_futures = vec![];

    log!(Level::Info, "[Onboard   ] Starting onboard process");

    let command_pipe_path = command_pipe.clone();
    let mut command_pipe = loop {
        match open_read_pipe(command_pipe.as_str()) {
            Ok(pipe) => break pipe,
            Err(e) => {
                match e {
                    _ => {
                        log!(Level::Error, "[Onboard   ] Error opening command pipe: {}", e);
                    }
                }
                tokio::time::sleep(tokio::time::Duration::from_millis(500)).await;
            }
        }
    };

    log!(Level::Debug, "[Onboard   ] Opened command pipe at {}", command_pipe_path);

    // Open the response pipe
    // This is opening here as it requires a receiver on the other end, which is opened by in C#.
    let response_pipe_path = response_pipe.clone();
    let mut response_pipe = loop {
        match open_write_pipe(response_pipe.as_str()) {
            Ok(pipe) => break pipe,
            Err(e) => {
                match e.to_string().as_str() {
                    "No such device or address (os error 6)" => {} // This is expected, just wait for the other end to open
                    _ => {
                        log!(Level::Error, "[Onboard   ] Error opening response pipe: {}", e);
                    }
                }
                tokio::time::sleep(tokio::time::Duration::from_millis(500)).await;
            }
        }
    };

    log!(Level::Debug, "[Onboard   ] Opened response pipe at {}", response_pipe_path);

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
            log!(Level::Debug, "[Onboard   ] Handling command: {}", command);
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
                    log!(Level::Debug, "[Onboard   ] Got response: {}", res);
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