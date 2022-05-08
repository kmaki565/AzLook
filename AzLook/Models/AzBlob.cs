using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzLook.Models
{
    public class AzBlob
    {
        private readonly BlobContainerClient _containerClient;
        /// <summary>
        /// Initializes an Azure client based on an SAS URL pointing to a container. 
        /// </summary>
        public AzBlob(string sasUrl)
        {
            sasUrl.Trim();

            if (Regex.IsMatch(sasUrl, @"^https://[^/]+/[^/]+\?"))
            {
                // e.g., https://storageaccountname.blob.core.windows.net/containername?sp=rl&st=2022-02-15...
                _containerClient = new BlobContainerClient(new Uri(sasUrl));
                if (!string.IsNullOrEmpty(_containerClient?.Name))
                    return; // Assume this is a valid SAS.
            }
            
            throw new ArgumentException("Supplied string is not a valid SAS URL pointing to a container.");
        }

        /// <summary>
        /// Lists all blob names in the container. If a prefix is supplied,
        /// lists only blobs whose names start with the prefix.
        /// </summary>
        /// <returns>All blob names in the container</returns>
        public IEnumerable<string> ListBlobs(string prefix = null)
        {
            foreach (BlobItem blobItem in _containerClient.GetBlobs(BlobTraits.None, BlobStates.None, prefix))
            {
                yield return blobItem.Name;
            }
        }
        /// <summary>
        /// Downloads a blob in the container.
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="localPath">Local file name to be created, or directory name where the file will be downloaded. 
        /// This can either be an absolute path or a relative path from current working directory.</param>
        /// <returns></returns>
        public async Task DownloadBlob(string blobName, string localPath = ".", bool overwrite = false)
        {
            CheckNetwork();
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            if (blobClient.BlobContainerName != _containerClient.Name || blobClient.Name != blobName)
                throw new InvalidOperationException("Supplied SAS URL is not valid for this operation.");

            // Path.Combine ignores first path if second path contains an absolute path.
            // So you can safely supply an absolute path for localPath.
            string targetPath = Path.Combine(Directory.GetCurrentDirectory(), localPath);
            // If it's directory, append the last part of the blob name (after "/" delimiter).
            if (Directory.Exists(targetPath))
                targetPath = Path.Combine(targetPath, Path.GetFileName(blobName));
            var targetFile = new FileInfo(targetPath);

            if (!overwrite && targetFile.Exists)
                throw new InvalidOperationException($"{targetFile.FullName} already exists.");

            try
            {
                await blobClient.DownloadToAsync(targetFile.FullName);
            }
            catch (Azure.RequestFailedException)
            {
                // An empty file is created, so clean up.
                targetFile.Delete();
                throw;
            }
        }
        public async Task<Stream> DownloadBlobToStream(string blobName)
        {
            CheckNetwork();
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            if (blobClient.BlobContainerName != _containerClient.Name || blobClient.Name != blobName)
                throw new InvalidOperationException("Supplied SAS URL is not valid for this operation.");

            var ms = new MemoryStream();
            await blobClient.DownloadToAsync(ms);
            return ms;
        }
        
        /// <summary>
        /// Confirms network connection. Throws exception if not connected.
        /// </summary>
        private static void CheckNetwork()
        {
            // This is the site Windows 11 tries to connect at network init.
            var request = (HttpWebRequest)WebRequest.Create("http://www.msftconnecttest.com/connecttest.txt");
            request.KeepAlive = false;
            request.Timeout = 5000;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.ContentLength > 0)
                    return;
            }
            throw new WebException("Internet unreachable.");
        }
    }
}
