use crate::command::handle;
use crate::servers::open_server;
use devcade_onboard_types::{Request, RequestBody, Response};
use futures_util::future;
use log::{log, Level};
use std::sync::Arc;
use tokio::io::AsyncWriteExt;
use tokio::sync::Mutex;
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

            if let RequestBody::Ping = &command.body {
                log!(Level::Trace, "Handling command: {}", command);
            } else {
                log!(Level::Debug, "Handling command: {}", command);
            }

            let writer = writer.clone();

            handles.push(task::spawn(async move {
                let body = handle(command.body).await;
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
        Ok(())
    })
    .await
}
