use std::path::Path;
use std::process::Command;

static DEFAULT_DOTENV_PATH = "/usr/share/devcade/.env.template"
static DOTENV_PATH = "/home/devcade/.env"

fn dotenv_exists() -> bool {
    Path::new(DOTENV_PATH).exists()
}

fn create_dotenv_if_not_exists() -> bool {
    if dotenv_exists() {
        false
    }

    Command::new("cp")
        .arg(DEFAULT_DOTENV_PATH)
        .arg(DOTENV_PATH)
        .output()
}
