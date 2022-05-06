using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLook.Models
{
    internal class LogItem
    {
        public DateTime TimeGenerated { get; set; }
        public LogLevel Level { get; set; }
        public string Source { get; set; }
        public string  Message { get; set; }

        public void AddMessage(string messageToAdd)
        {
            Message += messageToAdd;
        }
    }

    public enum LogLevel 
    { 
        Trace, 
        Debug, 
        Information,
        Warning,
        Error,
        Critical,
        None
    }
}
