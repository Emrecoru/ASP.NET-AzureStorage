using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageLibrary.Services
{
    public class BlobStorage : IBlobStorage
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorage()
        {
            _blobServiceClient = new BlobServiceClient(ConnectionString.AzureConnectionString);
        }

        public string BlobUrl => "http://127.0.0.1:10000/devstoreaccount1";

        public async Task DeleteAsync(string fileName, ContainerNameEnum containerNameEnum)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerNameEnum.ToString());

            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        public Task<Stream> DownloadAsync(string fileName, ContainerNameEnum containerNameEnum)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerNameEnum.ToString());

            var blobClient = containerClient.GetBlobClient(fileName);

            var result = blobClient.DownloadContent();

            return Task.FromResult(result.Value.Content.ToStream());
        }

        public async Task<List<string>> GetLogAsync(string fileName)
        {
            List<string> logs = new List<string>();

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerNameEnum.logs.ToString());

            await containerClient.CreateIfNotExistsAsync();

            var appendBlobClient = containerClient.GetAppendBlobClient(fileName);

            await appendBlobClient.CreateIfNotExistsAsync();

            var info = await appendBlobClient.DownloadStreamingAsync();

            using (StreamReader sr = new StreamReader(info.Value.Content))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    logs.Add(line);
                }
            }

            return logs;
        }

        public List<string> GetNames(ContainerNameEnum containerNameEnum)
        {
            List<string> blobNames = new List<string>();

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerNameEnum.ToString());

            containerClient.CreateIfNotExistsAsync();

            var blobs = containerClient.GetBlobs();

            blobs.ToList().ForEach(x =>
            {
                blobNames.Add(x.Name);
            });

            return blobNames;
        }

        public async Task SetLogAsync(string text, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerNameEnum.logs.ToString());

            var appendBlobClient = containerClient.GetAppendBlobClient(fileName);

            await appendBlobClient.CreateIfNotExistsAsync();

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.Write($"{DateTime.Now} : {text} \n");

                    sw.Flush();
                    ms.Position = 0;

                    await appendBlobClient.AppendBlockAsync(ms);
                }
            }
        }

        public async Task UploadAsync(Stream fileStream, string fileName, ContainerNameEnum containerNameEnum)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerNameEnum.ToString());

            await containerClient.CreateIfNotExistsAsync();

            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(fileStream, overwrite:true);
        }
    }
}
