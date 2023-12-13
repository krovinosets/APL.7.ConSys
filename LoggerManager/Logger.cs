using System;
using System.IO;
using System.Net;

namespace LoggerManager
{
    public static class Logger
    {
        private static FileLogger _fileLogger;

        private static void CleanFile(string filepath)
        {
            if (File.Exists(filepath))
                File.WriteAllText(filepath, string.Empty);
            else
            {
                File.Create(filepath);
            }
        }
        
        public static void ConnectFileLogger(string filePath)
        {
            CleanFile(filePath);
            _fileLogger = new FileLogger(filePath);
        }
        
        public static void Info(string source, string message)
        {
            string msg = $"[{DateTime.Now}][{source}/INFO] {message}";
            Console.WriteLine(msg);
            _fileLogger?.Write(msg);
        }
        
        public static void Warn(string source, string message)
        {
            string msg = $"[{DateTime.Now}][{source}/WARN] {message}";
            Console.WriteLine(msg);
            _fileLogger?.Write(msg);
        }
        
        public static void Error(string source, string message)
        {
            string msg = $"[{DateTime.Now}][{source}/ERROR] {message}";
            Console.WriteLine(msg);
            _fileLogger?.Write(msg);
        }
    }
}