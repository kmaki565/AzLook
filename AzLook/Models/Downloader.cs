using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLook.Models
{
    internal class Downloader
    {
        private readonly AzBlob az;
        public Downloader(string sasUrl)
        {
            az = new AzBlob(sasUrl);
        }
        public async Task DownloadLog(DateTime dateTime)
        {
            var logBlobs = az.ListBlobs();
            var targetLog = logBlobs.FirstOrDefault(x => x.Contains(dateTime.ToString("yyyy/MM/dd/HH")));
                
            if (targetLog == null) 
                throw new FileNotFoundException();
            
            await az.DownloadBlob(targetLog, "myLog.txt", overwrite: true);
        }
    }
}
