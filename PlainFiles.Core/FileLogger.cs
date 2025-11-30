using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlainFiles.Core
{
    public class FileLogger
    {
        private readonly string _logPath;

        public FileLogger(string logPath)
        {
            _logPath = logPath;
        }

        public void Log(string username, string action, string result)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | User: {username} | Action: {action} | Result: {result}";
            File.AppendAllLines(_logPath, new[] { line });
        }
    }
}