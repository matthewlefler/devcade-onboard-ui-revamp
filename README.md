# Devcade-onboard
The onboard menu and control software for the Devcade custom arcade system.

## Building

Run the `update_onboard.sh` script located in HACKING

## Building (manual)

To build and run on the DCU, do the following from `./onboard/frontend`:
```
dotnet publish -c Release -r linux-x64 --no-self-contained
```
And the following fron `./onboard/backend`:
```
cargo build --release --target x86_64-unknown-linux-gnu
```
To put it on the DCU, compress the `publish` folder located at `./onboard/frontend/bin/Release/netcoreapp3.1/linux-x64` and `scp` that to the DCU.
You'll also want to `scp` `./onboard/backend/target/release` to the DCU. 

## HACKING

To setup and launch a development environment, you can do the following:

### Env Vars

There is a file called .env.template in the `./onboard` folder. Fill this in with appropriate values for the backend and frontend.


### Running outside a container

In onboard/frontend, run `dotnet run`

In onboard/backend, run `cargo run`

The frontend will log warnings about not being able to connect until the backend is up and running

### Building and Launching the Container

```
cd HACKING
./build-environment.sh
./launch-environment.sh
```

#### `mgcb`

The container has `mgcb-editor` installed. To run that, do this:
`dotnet mgcb-editor`
