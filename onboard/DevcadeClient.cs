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
using Amazon.S3.Transfer;
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

            ListBucketsResponse response = _s3Client.ListBucketsAsync().Result;
            foreach (S3Bucket b in response.Buckets)
            {
                Console.WriteLine("{0}\t{1}", b.BucketName, b.CreationDate);
            }

                
            // List all objects
            ListObjectsRequest listRequest = new ListObjectsRequest
            {
                BucketName = _bucketName,
            };

            ListObjectsResponse listResponse;
            do
            {
                // Get a list of objects
                listResponse = _s3Client.ListObjectsAsync(listRequest).Result;
                foreach (S3Object obj in listResponse.S3Objects)
                {
                    Console.WriteLine("Object - " + obj.Key);
                    Console.WriteLine(" Size - " + obj.Size);
                    Console.WriteLine(" LastModified - " + obj.LastModified);
                    Console.WriteLine(" Storage class - " + obj.StorageClass);
                }

                // Set the marker property
                listRequest.Marker = listResponse.NextMarker;
            } while (listResponse.IsTruncated);


            TransferUtility fileTransferUtility = new TransferUtility(_s3Client);
            return;


            // Note the 'fileName' is the 'key' of the object in S3 (which is usually just the file name)
            fileTransferUtility.Download("/tmp/bankshot.zip", _bucketName, "bankshot.zip");
        }

        public void runGame(string game)
        {
            //string objectKey = "EMR" + "/" + imagename;
            //EMR is folder name of the image inside the bucket 
            //GetObjectRequest request = new GetObjectRequest();
            //request.BucketName = _bucketName;
            //request.Key = game;
            //request.Key = "bankshot.zip";
            //GetObjectResponse response = GetObjectAsync(request).Result;
            //WriteResponseStreamToFileAsync(response, "/tmp/" + game);

            ListObjectsRequest request = new ListObjectsRequest();
            request.BucketName = _bucketName;
            do
            {
                ListObjectsResponse response = _s3Client.ListObjectsAsync(request).Result;

                // Process response.
                // ...

                // If response is truncated, set the marker to get the next 
                // set of keys.
                if (response.IsTruncated)
                {
                    request.Marker = response.NextMarker;
                }
                else
                {
                    request = null;
                }
            } while (request != null);

            TransferUtility fileTransferUtility = new TransferUtility(_s3Client);
            return;


            // Note the 'fileName' is the 'key' of the object in S3 (which is usually just the file name)
            fileTransferUtility.Download("/tmp/bankshot.zip", _bucketName, "bankshot.zip");

            //string path = "/tmp/" + game;
            //ZipFile.ExtractToDirectory(path, "/tmp");


            string path = "/tmp/bankshot.zip";
            ZipFile.ExtractToDirectory(path, "/tmp/");

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
            Console.WriteLine("Making call to s3");
            var response = await _s3Client.GetObjectAsync(request);
            Console.WriteLine(response.ToString());
            return response;
        }

        private async void WriteResponseStreamToFileAsync(GetObjectResponse response, string game)
        {
            Console.WriteLine("Object Retrive Call complete. Writing to file...");
            string path = "/tmp/" + game;
            CancellationToken chom;
            await response.WriteResponseStreamToFileAsync(path, false, chom);

            //var chom = await _s3Client.GetObjectAsync(request);
           // return response;
        }
    }
}