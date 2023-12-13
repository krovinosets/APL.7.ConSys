using System;
using System.IO;

namespace LoggerManager
{
    public class FileLogger
    {
        private readonly string _filePath;
        private static Object _locker = new Object();
        
        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }
        
        public void Write(string message)
        {
            lock (_locker)
            {
                using (StreamWriter sw = new StreamWriter(_filePath, true))
                {
                    sw.WriteLine(message);
                }
            }
        }
    }
}