using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzLook.Models
{
    internal class LogReader
    {
        string filePath;
        public LogReader(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            filePath = path;
        }

        public List<LogItem> ReadItems()
        {
            var logItems = new List<LogItem>();
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    // Starts with a string like 2022-04-09 21:00:05.684 +09:00 [Information] Microsoft.AspNetCore.Hosting.Diagnostics: Bla bla bla
                    Match match = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}.\d{3} [+-]\d{2}:\d{2}) \[(\w+)\] ([^:]+): (.*)");
                    if (match.Success)
                    {
                        LogItem item = new LogItem 
                        { 
                            TimeGenerated = DateTime.Parse(match.Groups[1].Value),
                            Level = (LogLevel)Enum.Parse(typeof(LogLevel), match.Groups[2].Value),
                            Source = match.Groups[3].Value,
                            Message = match.Groups[4].Value
                        };
                        logItems.Add(item);
                    }
                    else
                    {
                        // Multiple lines log
                        logItems.Last().AddMessage(Environment.NewLine + line);
                    }
                }
            }
            return logItems;
        }
    }
}
