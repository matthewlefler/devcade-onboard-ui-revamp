use gatekeeper_members::GateKeeperMemberListener;
use lazy_static::lazy_static;
use libgatekeeper_sys::Nfc;
use serde_json::Value;
use std::fmt;
use std::sync::mpsc;
use std::sync::mpsc::{Receiver, Sender};
use std::sync::Mutex;
use std::thread;
use std::thread::JoinHandle;
use std::time::Duration;
use tokio::sync::oneshot;

type NfcCallback = oneshot::Sender<Option<String>>;
pub struct NfcClient {
    request_queue: Mutex<Sender<NfcRequest>>,
    thread: JoinHandle<()>,
}

enum NfcRequest {
    Tags {
        callback: NfcCallback,
    },
    User {
        association_id: String,
        callback: oneshot::Sender<Option<Value>>,
    },
}

lazy_static! {
    pub static ref NFC_CLIENT: NfcClient = Default::default();
}

impl Default for NfcClient {
    fn default() -> Self {
        let (tx, rx) = mpsc::channel();
        let thread = thread::spawn(|| {
            NfcClient::run(rx);
        });
        NfcClient {
            thread,
            request_queue: tx.into(),
        }
    }
}

const NFC_DEVICE_NAME: &str = "pn532_uart:/dev/ttyUSB0";

impl NfcClient {
    fn run(rx: Receiver<NfcRequest>) {
        loop {
            // Unwrap rationale: If the main thread is crashed, not much we can do
            let mut callback = rx.recv().unwrap();
            // Unwrap rationale: If we can't allocate memory, we're not long for this world anyways
            let mut nfc = Nfc::new().unwrap();
            let mut listener =
                match GateKeeperMemberListener::new(&mut nfc, NFC_DEVICE_NAME.to_string()) {
                    Some(listener) => listener,
                    None => {
                        log::error!("Couldn't build Gatekeeper listener?");
                        // Unwrap rationale: If the main thread is crashed, not much we can do
                        match callback {
                            NfcRequest::User { callback, .. } => callback.send(None).unwrap(),
                            NfcRequest::Tags { callback } => callback.send(None).unwrap(),
                        }
                        continue;
                    }
                };

            loop {
                match callback {
                    NfcRequest::User {
                        callback,
                        association_id,
                    } => {
                        callback
                            .send(listener.fetch_user(association_id).ok())
                            .unwrap();
                    }
                    NfcRequest::Tags { callback } => {
                        // Unwrap rationale: If the main thread is crashed, not much we can do
                        callback.send(listener.poll_for_user()).unwrap();
                    }
                }

                if let Ok(new_request) = rx.recv_timeout(Duration::from_secs(30)) {
                    callback = new_request;
                } else {
                    break;
                }
            }
        }
    }
    pub async fn submit(&self) -> Result<Option<String>, Box<dyn std::error::Error>> {
        let (tx, rx) = oneshot::channel();

        match self.request_queue.lock() {
            Ok(guard) => guard.send(NfcRequest::Tags { callback: tx })?,
            Err(_) => {
                return Err(Box::new(NfcThreadError));
            }
        };
        Ok(rx.await?)
    }
    pub async fn get_user(
        &self,
        association_id: String,
    ) -> Result<Option<Value>, Box<dyn std::error::Error>> {
        let (tx, rx) = oneshot::channel();

        match self.request_queue.lock() {
            Ok(guard) => guard.send(NfcRequest::User {
                association_id,
                callback: tx,
            })?,
            Err(_) => {
                return Err(Box::new(NfcThreadError));
            }
        };
        Ok(rx.await?)
    }
}

#[derive(Debug)]
struct NfcThreadError;

impl fmt::Display for NfcThreadError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "NfcThreadError")
    }
}

impl std::error::Error for NfcThreadError {}
