using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Downloads log files for specified time period into stream.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<Stream> DownloadLogData(DateTime begin, DateTime end)
        {
            var logBlobs = az.ListBlobs();
            var ms = new MemoryStream();
            DateTime time = begin;
            while (string.Compare(time.ToString("yyyy/MM/dd/HH"), end.ToString("yyyy/MM/dd/HH")) <= 0)  //@BUG
            {
                foreach (var logBlob in logBlobs.Where(x => Regex.IsMatch(x, $@".+/{time.ToString("yyyy/MM/dd/HH")}.+\.txt")))
                {
                    var blobData = await az.DownloadBlobToStream(logBlob) as MemoryStream;
                    blobData.Position = 0;
                    blobData.CopyTo(ms);
                }
                time = time.AddHours(1);
            }
            return ms;
        }
    }
}
