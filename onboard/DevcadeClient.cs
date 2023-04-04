using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


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
        public DevcadeUser user { get; set; }
        public DevcadeTag[] tags { get; set; }
    }

    public class DevcadeUser
    {
        public string id { get; set; }
        public string user_type { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string picture { get; set; }
        public bool admin { get; set; }
    }

    public class DevcadeTag
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    public class DevcadeClient
    {
        private readonly string _apiProdDomain;
        private readonly string _apiDevDomain;
        private string _apiDomain;

        // Basically a semaphore for communicating between the main thread (doing the menu animations)
        // and the thread that loads the game.
        public bool DownloadFailed = false;

        public DevcadeClient()
        {
            _apiProdDomain = Environment.GetEnvironmentVariable("DEVCADE_API_DOMAIN");
            _apiDevDomain = Environment.GetEnvironmentVariable("DEVCADE_DEV_API_DOMAIN");
            _apiDomain = _apiProdDomain;
        }

        // Point at Dev/Prod
        public void SwapDomains()
        {
            if (_apiDomain == _apiDevDomain)
                _apiDomain = _apiProdDomain;
            else if (_apiDomain == _apiProdDomain)
                _apiDomain = _apiDevDomain;
            Console.WriteLine($"Switching to: {_apiDomain}");
        }

        public String GetDomain()
        {
            if (_apiDomain == _apiDevDomain)
                return "Development";

            //if (_apiDomain == _apiProdDomain)
            return "Production";
        }

        public List<DevcadeGame> GetGames()
        {
            using var client = new HttpClient();
            try
            {
                string uri = $"https://{_apiDomain}/games/"; // TODO: Env variable URI tld 
                using Task<string> responseBody = client.GetStringAsync(uri);
                List<DevcadeGame> games = JsonConvert.DeserializeObject<List<DevcadeGame>>(responseBody.Result);
                // TODO: Add error handling if there is no games from the API
                if (games == null || games.Count == 0)
                {
                    Console.WriteLine("Where the games at?");
                }
                return games;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return new List<DevcadeGame>();
        }

        public void GetBanner(DevcadeGame game)
        {
            // Path to where the banner image will be saved
            // Making this game.name will name the downloaded image have that name, could set it to anything like id etc..
            string path = $"/tmp/{game.id}Banner";

            Console.WriteLine($"Downloading banner for: {game.name}");

            using var client = new HttpClient();
            try
            {
                // Download the image from this uri, save it to the path
                string uri = $"https://{_apiDomain}/games/{game.id}/banner";
                using Task<Stream> s = client.GetStreamAsync(uri);
                using var fs = new FileStream(path, FileMode.OpenOrCreate);
                s.Result.CopyTo(fs);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private void getBanner(object callback)
        {
            var game = (DevcadeGame)callback;
            GetBanner(game);
            Menu.instance.notifyTextureAvailable(game.id);
        }

        public void getBannerAsync(DevcadeGame game)
        {
            ThreadPool.QueueUserWorkItem(getBanner, game);
        }

        // Returns true if success and false otherwise
        // permissions can be an int or a string. For example it can also be +x, -x etc..
        private static void Chmod(string filePath, string permissions = "700", bool recursive = false)
        {
            string cmd = recursive ? $"chmod -R {permissions} {filePath}" : $"chmod {permissions} {filePath}";

            try
            {
                using Process proc = Process.Start("/bin/bash", $"-c \"{cmd}\"");
                proc?.WaitForExit();
            }
            catch
            {
                // ignored
            }
        }


        public void startGame(DevcadeGame game)
        {
            DownloadFailed = false;
            ThreadPool.QueueUserWorkItem(DownloadGame, game);
        }

        private void DownloadGame(object gameObj)
        {
            try
            {
                var game = (DevcadeGame)gameObj;
                string gameName = game.name.Replace(' ', '_');
                Console.WriteLine($"Game is: {gameName}");
                string path = $"/tmp/{gameName}.zip";
                string URI = $"https://{_apiDomain}/games/{game.id}/game";
                Console.WriteLine($"Getting {game.name} from {URI}");

                using var client = new HttpClient();
                using Task<Stream> s = client.GetStreamAsync(URI);
                using var fs = new FileStream(path, FileMode.OpenOrCreate);
                s.Result.CopyTo(fs);
                notifyDownloadComplete(game);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DownloadFailed = true;
            }
        }

        public static void reportToDatadog(DevcadeGame game)
        {
            // Create a new UdpClient
            UdpClient udpClient = new UdpClient();

            // Create a new IPEndPoint for the destination IP address and port number
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8125;
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

            string gameName = game.name.Replace(' ', '_');
            // Convert the message to a byte array
            string message = $"devcade.game_launch:1|c|#game:{gameName}";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

            // Send the message
            udpClient.Send(bytes, bytes.Length, endPoint);

            // Close the UdpClient
            udpClient.Close();
        }

        private static void notifyDownloadComplete(DevcadeGame game)
        {
            string gameName = game.name.Replace(' ', '_');
            // Try extracting the game
            string path = $"/tmp/{gameName}.zip";
            Console.WriteLine($"Extracting {path}");
            if (Directory.Exists($"/tmp/{gameName}"))
            {
                Directory.Delete($"/tmp/{gameName}", true);
            }
            Directory.CreateDirectory($"/tmp/{gameName}");
            ZipFile.ExtractToDirectory(path, $"/tmp/{gameName}");

            // Try running the game
            // Infer the name of the executable based off of an automatically generated dotnet publish file
            // FIXME: This is fucking gross
            string[] binFiles = System.IO.Directory.GetFiles($"/tmp/{gameName}/publish/", "*.runtimeconfig.json");
            string execPath = binFiles[0].Split(".")[0];
            // Check if that worked. If it didn't, L plus ratio. 
            if (!File.Exists(execPath))
                throw new System.ComponentModel.Win32Exception();
            Console.WriteLine($"Running {execPath}");
            reportToDatadog(game);
            Chmod(execPath, "+x");
            Process proc = new()
            {
                StartInfo = new ProcessStartInfo(execPath)
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = Path.GetDirectoryName(execPath) ?? string.Empty,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };
            // Redirect stdout and stderr to the console
            proc.OutputDataReceived += (_, args) => Console.WriteLine($"[{game.name}] {args.Data}");
            proc.ErrorDataReceived += (_, args) => Console.WriteLine($"[{game.name}] {args.Data}");
            proc.Start();
            Game1.instance.setActiveProcess(proc);
        }
    }
}
