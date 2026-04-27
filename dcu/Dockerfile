FROM quay.io/fedora/fedora-bootc:latest

### required software
RUN dnf install -y openbox

### User Creation
ARG PASSWORD_VAR
ARG USERNAME_VAR
# create devcade user
RUN useradd devcade
#         username:password
RUN echo "${USERNAME_VAR}:${PASSWORD_VAR}" | chpasswd
# add to sudoer's group
RUN sudo usermod -aG wheel devcade

RUN mkdir -p /home/${USERNAME_VAR}
WORKDIR /home/${USERNAME_VAR}
