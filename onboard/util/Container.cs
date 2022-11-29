using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;

namespace onboard.util; 

public static class Container {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);
    private static readonly ILog containerLog = LogManager.GetLogger("Container");
    
    private static List<Process> activeContainers = new ();

    public static event EventHandler<devcade.DevcadeGame> OnContainerBuilt = (_, _) => {
        logger.Trace("OnContainerBuild Invoked");
    };
    public static event EventHandler<(devcade.DevcadeGame, int)> OnProcessDied = (_, _) => {
        logger.Trace("OnProcessDied Invoked");
    };

    // Slightly cursed
    private const string gameTemplate = @"
FROM debian:buster

RUN apt-get update -y
RUN apt-get install -y wget

RUN wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb \
    -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update -y && \
    apt-get install -y dotnet-runtime-6.0 libgtk-3-0

COPY publish /app
RUN chmod +x /app/$GAME
WORKDIR /app";

    // Very cursed
    private static readonly string launchTemplate = @"
#!/bin/bash

set -e

# $CONTAINER here will be replaced with the container name by the launcher
uname=$USER
container=$CONTAINER
name=$CONTAINER

while getopts `:u:d:n:` option; do
    case $option in
        u) # change uname
            uname=$OPTARG;;
        d) # change container
            container=$OPTARG;;
        n) # change name
            name=$OPTARG;;
    esac
done

xauth_path=/tmp/xopp-dev-xauth

rm -rf `$xauth_path`
mkdir -p `$xauth_path`
cp `$HOME`/.Xauthority `$xauth_path`
chmod g+rwx `$xauth_path`/.Xauthority

podman run --name=`$name` --rm -it                                    \
    -e Display=`$DISPLAY`                                             \
    --network=host                                                    \
    --cap-add=SYS_PTRACE                                              \
    --group-add keep-groups                                           \
    --annotation io.crun.keep_original_groups=1                       \
    -v `$xauth_path`/.Xauthority:/root/.Xauthority:Z                  \
    -v ./:/devcade:Z                                                  \
    -v /tmp/.X11-unix:/tmo/.X11-unix                                  \
    --env 'PKG_CONFIG_PATH=/usr/local/lob/pkgconfig:$PKG_CONFIG_PATH' \
    `$container`
rm -rf `$xauth_path`".Replace("`", "\""); // Using backticks in place of quotes because I can't escape (please free me)

    public static void createDockerfileFromGame(devcade.DevcadeGame game) {
        logger.Debug("Creating Dockerfile for game " + game.name);
        // Replace the game name in the template and trim starting newline
        string dockerfile = gameTemplate.Replace("$GAME", game.name)[1..];
        string launchScript = launchTemplate.Replace("$CONTAINER", game.name.ToLower())[1..]; // use lowercase for container name
        try {
            File.WriteAllText($"/tmp/devcade/{game.name}/Dockerfile", dockerfile);
            File.WriteAllText($"/tmp/devcade/{game.name}/launch.sh", launchScript);
        }
        catch (Exception e) {
            logger.Error($"Failed to create Dockerfile for game {game.name}: {e}");
        }
        int ret = Cmd.chmod($"/tmp/devcade/{game.name}/launch.sh", "u+x");
        buildImageFromGame(game);
    }
    
    public static void buildImageFromGame(devcade.DevcadeGame game) {
        logger.Debug("Building image for game " + game.name);
        string command = $"podman build /tmp/devcade/{game.name} --tag={game.name.ToLower()}";
        try {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();
            logger.Debug(process.StandardOutput.ReadToEnd());
            string error = process.StandardError.ReadToEnd();
            if (error != "") {
                logger.Error(error);
            }
        }
        catch (Exception e) {
            logger.Error($"Failed to build image for game {game.name}: {e}");
            return;
        }
        OnContainerBuilt?.Invoke(null, game);
        runContainer(game);
    }

    public static void runContainer(devcade.DevcadeGame game) {
        logger.Debug("Running container for game " + game.name);
        string command = $"/tmp/devcade/{game.name}/launch.sh";
        try {
            var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            process.OutputDataReceived += (_, args) => {
                containerLog.Debug(args.Data);
            };
            process.ErrorDataReceived += (_, args) => {
                containerLog.Error(args.Data);
            };
            
            process.Start();
            process.WaitForExitAsync().ContinueWith(_ => {
                OnProcessDied?.Invoke(null, (game, process.ExitCode));
            });
        }
        catch (Exception e) {
            logger.Error($"Failed to run container for game {game.name}: {e}");
        }
    }
    
    public static void killContainers() {
        logger.Debug("Killing all containers");
        foreach (Process process in activeContainers) {
            process.Kill();
        }
    }
}