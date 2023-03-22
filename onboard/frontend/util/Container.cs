using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;

namespace onboard.util; 

public static class Container {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);
    private static readonly ILog containerLog = LogManager.GetLogger("Container");
    
    private static List<Process> activeContainers = new ();

    public static event EventHandler<devcade.DevcadeGame> OnContainerBuilt = (_, _) => {
        logger.Trace("OnContainerBuild Invoked");
    };

    // Slightly cursed
    private const string gameTemplate = @"
FROM debian:buster

RUN apt-get update -y
RUN apt-get install -y wget git fonts-liberation fontconfig-config xterm pulseaudio zip unzip

RUN wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb \
    -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update -y && \
    apt-get install -y dotnet-runtime-6.0 libgtk-3-0

CMD [ ""/devcade/publish/$GAME"" ]";

    public static void createDockerfileFromGame(devcade.DevcadeGame game) {
        logger.Debug("Creating Dockerfile for game " + game.name);
        // Replace the game name in the template and trim starting newline
        string dockerfile = gameTemplate.Replace("$GAME", game.name)[1..];
        try {
            File.WriteAllText($"/tmp/devcade/{game.name}/Dockerfile", dockerfile);
        }
        catch (Exception e) {
            logger.Error($"Failed to create Dockerfile for game {game.name}: {e}");
        }
        buildImageFromGame(game);
    }
    
    public static void buildImageFromGame(devcade.DevcadeGame game) {
        logger.Debug("Building image for game " + game.name);
        string args = $"build /tmp/devcade/{game.name} --tag={game.name.ToLower()}";
        Cmd.exec("podman", args, logger);
        OnContainerBuilt?.Invoke(null, game);
        runContainer(game);
    }

    public static void runContainer(devcade.DevcadeGame game) {
        logger.Debug("Running container for game " + game.name);
        const string xauthPath = "/tmp/xopp-dev-auth";
        string home = Env.get("HOME").unwrap();
        string display = Env.get("DISPLAY").unwrap();
        Cmd.exec("rm", $"-rf {xauthPath}");
        Cmd.exec("mkdir", $"-p {xauthPath}");
        Cmd.exec("cp", $"{home}/.Xauthority {xauthPath}");
        Cmd.exec("chmod", $"g+rwx {xauthPath}/.Xauthority");
        // There's probably a better way to pass through sound than mounting /dev/snd but this works for now.
        Cmd.exec("podman", $@"run --name={game.name.ToLower()} --rm -it -u 0 -e DISPLAY={display} --network=host --cap-add=SYS_PTRACE --group-add keep-groups --annotation io.crun.keep_original_groups=1 -v /tmp/xopp-dev-auth/.Xauthority:/root/.Xauthority:Z -v /tmp/devcade/{game.name}:/devcade:Z -v /tmp/.X11-unix:/tmp/.X11-unix -v /dev/snd:/dev/snd --env 'PKG_CONFIG_PATH=/usr/local/lib/pkgconfig:$PKG_CONFIG_PATH' --net=host {game.name.ToLower()}", containerLog);
    }
    
    public static void killContainers() {
        logger.Debug("Killing all containers");
        foreach (Process process in activeContainers) {
            process.Kill();
        }
    }
}