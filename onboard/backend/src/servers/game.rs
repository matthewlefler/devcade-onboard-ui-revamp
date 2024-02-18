use crate::command::handle;
use crate::servers::open_server;
use anyhow::anyhow;
use devcade_onboard_types::{Request, RequestBody, Response, ResponseBody};
use futures_util::future;
use std::sync::Arc;
use tokio::io::{AsyncWriteExt, Lines, WriteHalf};
use tokio::sync::Mutex;
use tokio::task;

pub async fn main(command_pipe: &str) -> ! {
    log::info!("Starting save/load process");
    log::debug!("Opened command pipe at {}", command_pipe);

    open_server(
        command_pipe,
        async move |mut lines: Lines<_>, writer: WriteHalf<_>| {
            let writer = Arc::new(Mutex::new(writer));
            let mut handles = vec![];
            log::debug!("New client connected to game socket");
            while let Some(line) = lines.next_line().await? {
                let command: Request = serde_json::from_str(&line)?;

                let writer = writer.clone();

                handles.push(task::spawn(async move {
                    let body: ResponseBody = match &command.body {
                        RequestBody::Ping => {
                            log::trace!("Handling command: {command}");
                            handle(command.body).await
                        }
                        RequestBody::Save(_, _, _)
                        | RequestBody::Load(_, _)
                        | RequestBody::Flush
                        | RequestBody::GetNfcTag(_)
                        | RequestBody::GetNfcUser(_) => {
                            log::debug!("Handling command: {command}");
                            handle(command.body).await
                        }
                        // Don't allow game save/load to (for example) download a game, launch a game,
                        // etc. If games could launch other games, it would update the 'current game' in
                        // crate::api and allow games to corrupt other games' save data (possibly
                        // maliciously!)
                        _ => anyhow!("Invalid command: {}", command).into(),
                    };
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
            log::info!("Game thread disconnecting");
            Ok(())
        },
    )
    .await
}
