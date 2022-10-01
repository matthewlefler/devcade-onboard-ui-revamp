# Devcade-onboard
The onboard menu and control software for the Devcade custom arcade system.


## The Idiot

### Prereqs

Debian >=10

A user named `devcade`

`apt install xorg` and friends (I dont actually know what all is installed)

### Daemon

_daemons are always watching. They are always with you. So is Willard._

The Devcade Idiot is running Debian 10 with a very _very_ simple Xorg server setup. It has a systemd service configured to launch the onboarding program, along with said xorg server, as the `devcade` user.

You can find everything(tm) you need to set up the Devcade Idiot in `/idiot`. This includes the systemd service, which contains the path you need to install it at.

```
/etc/systemd/system/devcade-onboard.service
```

This should be interactable as a normal systemd service, so `enable`/`disable` it as normal.