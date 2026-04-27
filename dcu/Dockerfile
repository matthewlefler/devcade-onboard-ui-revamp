FROM quay.io/fedora/fedora-bootc:latest



### User Creation
ARG PASSWORD_VAR
ARG USERNAME_VAR
# create devcade user
RUN useradd devcade
#         username:password
RUN echo "${USERNAME_VAR}:${PASSWORD_VAR}" | chpasswd
# add to sudoer's group
RUN sudo usermod -aG wheel devcade

WORKDIR /home/${USERNAME_VAR}
ENV HOME /home/${USERNAME_VAR}

### required software
RUN dnf install -y openbox gcc git
# Rust
RUN curl https://sh.rustup.rs -sSf | bash -s -- -y
ENV PATH="./.cargo/bin:${PATH}"
# git repo
RUN git clone https://github.com/matthewlefler/devcade-onboard-ui-revamp.git

### copy required files 
# openbox config file
COPY ./config-files/rc.xml ./.config/openbox/rc.xml
# xinitrc
COPY ./config-files/.xinitrc ./.xinitrc
# autologin service
COPY ./config-files/tty1_service_override.conf /etc/systemd/system/getty@tty1.service.d/override.conf
# export script
COPY ./config-files/export-all.sh ./export-all.sh
# bashrc
COPY ./config-files/.bashrc ./.bashrc
# onboard script
COPY ./config-files/onboard ./onboard
