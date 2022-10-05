using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Xna.Framework;


namespace onboard
{
    public class DevcadeClient
    {
        private string _bucketName = "devcade-games";

        private AmazonS3Config _config;
        private AmazonS3Client _s3Client;

        public DevcadeClient()
        {
            _config = new AmazonS3Config();
            _config.ServiceURL = "https://s3.csh.rit.edu";

            _s3Client = new AmazonS3Client(
                    accessKey,
                    secretKey,
                    _config
                    );

            listBuckets();
        }

        public void runGame(string game)
        {
            //string objectKey = "EMR" + "/" + imagename;
            //EMR is folder name of the image inside the bucket 
            GetObjectRequest request = new GetObjectRequest();
            request.BucketName = _bucketName;
            request.Key = game;
            GetObjectResponse response = GetObjectAsync(request).Result;
            WriteResponseStreamToFileAsync(response, "/tmp/" + game);

            string path = "/tmp/" + game;
            ZipFile.ExtractToDirectory(path, "/tmp");

            //string myPath = "C:\\Users\\dingus\\Downloads\\publish_noah_windoze\\publish\\BankShot.exe";

            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo("/tmp/bankshot/publish/BankShot")
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = Path.GetDirectoryName(path)
                }
            };

            process.Start();
        }

        public void listBuckets()
        {
            Task<ListBucketsResponse> response = ListBucketsAsync();
            
            foreach (S3Bucket b in response.Result.Buckets)
            {
                Console.WriteLine("{0}\t{1}", b.BucketName, b.CreationDate);
            }
        }

        // Async method to get a list of Amazon S3 buckets.
        private async Task<ListBucketsResponse> ListBucketsAsync()
        {
            var response = await _s3Client.ListBucketsAsync();
            return response;
        }
        private async Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request)
        {
            var response = await _s3Client.GetObjectAsync(request);
            return response;
        }

        private async void WriteResponseStreamToFileAsync(GetObjectResponse response, string game)
        {
            string path = "/tmp/" + game;
            CancellationToken chom;
            await response.WriteResponseStreamToFileAsync(path, false, chom);

            //var chom = await _s3Client.GetObjectAsync(request);
           // return response;
        }
    }
}