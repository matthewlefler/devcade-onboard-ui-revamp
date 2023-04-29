use crate::command::{handle, Request};
use crate::servers::open_server;
use futures_util::future;
use log::{log, Level};
use std::sync::{Arc, Mutex};
use tokio::io::AsyncWriteExt;
use tokio::task;

/**
 * Main function for the onboard process. This function handles all communication to/from the onboard
 * process. It reads commands from the command pipe and writes responses to the response pipe.
 *
 * This function will never return unless it panics and should be spawned as a thread.
 */
pub async fn main(command_pipe: &str) -> ! {
    // Vector for holding all the response futures so we can continue to read from the command pipe
    // while we wait for handle to finish.

    log!(Level::Info, "Starting onboard process");

    let command_pipe_path = command_pipe;

    log!(Level::Debug, "Opened command pipe at {}", command_pipe_path);

    open_server(command_pipe_path, async move |mut lines, writer| {
        let writer = Arc::new(Mutex::new(writer));
        let mut handles = vec![];
        while let Some(line) = lines.next_line().await? {
            let command: Request = serde_json::from_str(&line)?;

            if let Request::Ping(_) = &command {
                log!(Level::Trace, "Handling command: {}", command);
            } else {
                log!(Level::Debug, "Handling command: {}", command);
            }

            let writer = writer.clone();
            handles.push(task::spawn_local(async move {
                let response_body = task::spawn(async move {
                    let response = handle(command).await;
                    let mut response_body = serde_json::to_vec(&response)?;
                    response_body.push(b'\n');

                    Ok(response_body) as Result<Vec<u8>, anyhow::Error>
                })
                .await??;

                task::spawn_local(async move {
                    let mut writer = writer.lock().unwrap();
                    writer.write_all(&response_body).await?;
                    Ok(()) as Result<(), anyhow::Error>
                })
                .await?
            }));
        }
        future::join_all(handles).await;
        Ok(())
    })
    .await;
    panic!("Chom");
}
