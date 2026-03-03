# Devcade-onboard
The onboard menu and control software for the Devcade custom arcade system.

## Building

Run the `update_onboard.sh` script located in HACKING

## Building (manual)

To build and run on the DCU, do the following from `./onboard/frontend`:
```Bash
dotnet publish -c Release -r linux-x64 --no-self-contained
```
And the following from `./onboard/backend`:
```Bash
cargo build --release --target x86_64-unknown-linux-gnu
```
To put it on the DCU, compress the `publish` folder located at `./onboard/frontend/bin/Release/netcoreapp3.1/linux-x64` and `scp` that to the DCU.
You'll also want to `scp` `./onboard/backend/target/release` to the DCU. 

## HACKING

To setup and launch a development environment, you can do the following:

### Env Vars

There is a file called `.env.template` in the `./onboard` folder. Copy the file to a new file called `.env` in the same directory. Then fill this in with appropriate values for the backend and frontend.

### Running outside a container

In **onboard/backend**, run `cargo run`

In the godot project manager, select the **onboard/godot-frontend** folder to open the frontend. <br>
Or run `godot --editor --path onboard/godot-frontend` 

The frontend will log warnings about not being able to connect until the backend is up and running

<!-- ### Building and Launching the Container

```
cd HACKING
./build-environment.sh
./launch-environment.sh
``` -->
