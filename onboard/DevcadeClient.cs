using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework; // FIXME: Is this necessary for the client code?

// For making requests to the API
using System.Net.Http;
using Newtonsoft.Json;

namespace onboard
{
    public class DevcadeGame
    {
        public string id { get; set; }
        public string author { get; set; }
        public DateTime uploadDate { get; set; }
        public string name { get; set; }
        public string hash { get; set; }
        public string description { get; set; }
        public string iconLink { get; set; }
        public string bannerLink { get; set; }
    }

    public class DevcadeClient
    {
        private string _apiDomain;

        public DevcadeClient()
        {
            _apiDomain = Environment.GetEnvironmentVariable("DEVCADE_API_DOMAIN");
        }
        
        public List<DevcadeGame> GetGames()
        {
            using (var client = new HttpClient())
            {
                List<DevcadeGame> games;
                try {
                    string uri = $"https://{_apiDomain}/api/games/gamelist/"; // TODO: Env variable URI tld 
                    using (var responseBody = client.GetStringAsync(uri))
                    {
                        games = JsonConvert.DeserializeObject<List<DevcadeGame>>(responseBody.Result);
                        // TODO: Add error handling if there is no games from the API
                        if(games.Count == 0)
                        {
                            Console.WriteLine("Where the games at?");
                        }
                        return games;
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                return new List<DevcadeGame>();
            }
        }

        public void GetBanner(DevcadeGame game)
        {
            // TODO: This function works, now just get the game to draw those images
            //       Get some clarity on what each image is being used for

            // Path to where the banner image will be saved
            // Making this game.name will name the downloaded image have that name, could set it to anything like id etc..
            string path = $"/tmp/{game.name}";

            Console.WriteLine($"Downloading banner for: {game.name}");

            using (var client = new HttpClient())
            {
                try
                {
                    // Download the image from this uri, save it to the path
                    string uri = $"https://{_apiDomain}/api/games/download/banner/{game.id}";
                    using (var s = client.GetStreamAsync(uri))
                    {
                        using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                        {
                            s.Result.CopyTo(fs);
                        }
                    }
                }
                catch(HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
        }

        // Returns true if success and false otherwise
        // permissions can be an int or a string. For example it can also be +x, -x etc..
        bool Chmod(string filePath, string permissions = "700", bool recursive = false)
        {
            string cmd;
            if (recursive)
                cmd = $"chmod -R {permissions} {filePath}";
            else
                cmd = $"chmod {permissions} {filePath}";

            try
            {
                using (Process proc = Process.Start("/bin/bash", $"-c \"{cmd}\""))
                {
                    proc.WaitForExit();
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public void runGame(DevcadeGame game)
        {
            string gameName = game.name.Replace(' ', '_');
            Console.WriteLine($"Game is: {gameName}");
            string path = $"/tmp/{gameName}.zip";
            string URI = $"https://{_apiDomain}/api/games/download/{game.id}";
            Console.WriteLine($"Getting {gameName} from {URI}.");

            using (var client = new HttpClient())
            {
                using (var s = client.GetStreamAsync(URI))
                {
                    using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        s.Result.CopyTo(fs);
                    }
                }
            }

            try
            {
                Console.WriteLine($"Extracting {path}");
                // Extract the specified path (the zip file) to the specified directory (/tmp/, probably)
                System.IO.Directory.CreateDirectory($"/tmp/{gameName}");
                ZipFile.ExtractToDirectory(path, $"/tmp/{gameName}");
            } catch (System.IO.IOException e) {
                Console.WriteLine(e);
            }

            string execPath = $"/tmp/{gameName}/publish/{gameName.Replace("_","")}";
            Console.WriteLine($"Running {execPath}");
            Chmod(execPath,"+x",false);
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo(execPath) // chom
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = Path.GetDirectoryName(execPath)
                }
            };

            process.Start();
        }
    }
}
