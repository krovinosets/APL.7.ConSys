using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Core;
using Core.Controllers;
using Core.Stages;
using Core.Templates;
using LoggerManager;


namespace ConSysV2
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Logger.ConnectFileLogger("./log.txt");

            List<List<int>> settings = new List<List<int>>()
            {
                new List<int>() { 1, 1, 1 },
                new List<int>(){2, 1, 2},
                new List<int>(){1, 2, 1},
                new List<int>(){2, 2, 2},
                new List<int>(){1, 0, 1},
            };

            int i = 0;
            List<Stream> models = new List<Stream>();
            foreach (var set in settings)
            {
                List<Data> dataList = new List<Data>();
                for (int amount = 0; amount < 20; amount++)
                {
                    dataList.Add(new Data(PrimaryKey.GetID(),1024));
                }
                
                Logger.Info("Main", $"Объекты DATA созданы, количество {dataList.Count}");
                Logger.Info("Main", $"Запущена модель с настройками {set[0]}, {set[1]}, {set[2]}");
                Stream stream = new Stream(set[0], set[1], set[2], i);
                models.Add(stream);
                stream.Upload(dataList);
                i++;
            }

            // Добавляем новые потоки в модель
            models[4].InjectStages(4, 4, 4);
            
            // Ожидаем завершение работы всех моделей
            foreach (var model in models)
            {
                Logger.Info("Main", $"Ожидаем завершение модели номер {model.Id}");
                model.join();
                Logger.Info("Main", $"Модель номер {model.Id} полностью завершила выполнение");
            }

            // Выводим отчет
            foreach (var model in models)
            {
                MetricsHolder.Output("Main", model.Id);
            }
            
            // Один поток
            OneThreadTask();
            
            foreach (var model in models)
            {
                DateTime finished = model.FinishedTime;
                DateTime started = model.StartedTime;
                TimeSpan sum = MetricsHolder.CalculateCommonSum(model.Id);
                Logger.Info("Main", $"---------------------------------------------------------------------");
                Logger.Info("Main", $"Модель Stream ID: {model.Id}");
                Logger.Info("Main", $"Настройки: Collections:{model.Settings[0]}, Analysis:{model.Settings[1]}, Integrations:{model.Settings[2]}");
                Logger.Info("Main", $"Время потраченное на решение: {sum}");
                Logger.Info("Main", $"реальное время работы: {finished - started}");
                Logger.Info("Main", $"Утечка времени: {finished - started - sum}");
            }
            
            Logger.Info("Main", $"---------------------------------------------------------------------");
            Logger.Info("Main", $"Max memory usage: {Process.GetCurrentProcess().PeakWorkingSet64} Bytes, {Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024} MBytes");
            Logger.Info("Main", $"Memory usage at end: {GC.GetTotalMemory(true)} Bytes, {GC.GetTotalMemory(true) / 1024} KBytes");
            Logger.Info("Main", $"Завершение программы");
        }
        
        public static void OneThreadTask()
        {
            List<Data> dataList = new List<Data>();
            for (int amount = 0; amount < 20; amount++)
            {
                dataList.Add(new Data(PrimaryKey.GetID(),1024));
            }
            var started = DateTime.Now;
            OneThread oneThread = new OneThread(dataList);
            oneThread.Start();
            var finished = DateTime.Now;
            MetricsHolder.Output("Main", 99);
            var sum = MetricsHolder.CalculateCommonSum(99);
            Logger.Info("Main", $"---------------------------------------------------------------------");
            Logger.Info("Main", $"Модель OneThread ID: 99");
            Logger.Info("Main", $"Настройки: Collections:1, Analysis:1, Integrations:1");
            Logger.Info("Main", $"Время потраченное на решение: {sum}");
            Logger.Info("Main", $"реальное время работы: {finished - started}");
            Logger.Info("Main", $"Утечка времени: {finished - started - sum}");
        }
    }
}