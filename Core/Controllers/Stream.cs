using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Stages;
using Core.Templates;
using LoggerManager;
// ReSharper disable All

namespace Core.Controllers
{
    public class Stream
    {
        private static System.Timers.Timer _timeoutHopperEvent;

        public Object Linker = new Object();
        public UStages UStages;
        public List<int> Settings;
        public int Id;
        public bool Finished;

        public DateTime StartedTime;
        public DateTime FinishedTime;
        
        public Stream(int collectionAmount = 2, int analysisAmount = 2, int integrationAmount = 2, int id = 0)
        {
            UStages = new UStages();
            Id = id;
            StartedTime = DateTime.Now;
            Settings = new List<int>() { 0, 0, 0 };
            MetricsHolder.DataMetric.Add(Id, new List<Tuple<int, Dictionary<Status, TimeSpan>>>());
            InjectStages(collectionAmount, analysisAmount, integrationAmount);
            
            InjectHopperTimer();
        }

        // Финализатор
        ~Stream()
        {
            _timeoutHopperEvent.Dispose();
        }

        public void WakeUp(int c = 0, int a = 0, int i = 0)
        {
            Logger.Info($"Stream #{Id}", $"Отправили на пробуждение Collections = {c}, Analysis = {a}, Integrations = {i}");
            InjectStages(c, a, i);
        }
        
        private void OnCleanEvent()
        {
            Logger.Info($"Stream #{Id}", $"Количество потоков: {UStages.Threads.Count}");
            List<IStage> toDelete = new List<IStage>();
            for(int i = 0; i < UStages.Threads.Count; i++)
            {
                IStage stage = UStages.Threads[i];
                var lockedBySomeone = !Monitor.TryEnter(stage.GetLink());
                if (!lockedBySomeone)
                {
                    Monitor.Exit(stage.GetLink());
                    if (DateTime.Now > stage.GetLastActivity().AddSeconds(30))
                    {
                        Logger.Info($"Stream #{Id}", $"Удаляем поток из-за неактивности в течении 30 секунд!");
                        toDelete.Add(stage);
                    }
                }
            }

            foreach (var stage in toDelete)
            {
                stage.Stop();
                UStages.Remove(stage);
            }
            
            if (UStages.Threads.Count == 0)
            {
                _timeoutHopperEvent.Enabled = false;
                lock (Linker)
                {
                    Finished = true;
                    FinishedTime = DateTime.Now;
                    Monitor.Pulse(Linker);
                }
            }
        }

        public void join()
        {
            if(Finished)
                return;
            
            lock (Linker)
            {
                Monitor.Wait(Linker);
            }
        }
        
        private void InjectHopperTimer()
        {
            _timeoutHopperEvent = new System.Timers.Timer();
            _timeoutHopperEvent.Interval = 5000;
            _timeoutHopperEvent.Elapsed += OnQueueEvent;
            _timeoutHopperEvent.Enabled = true;
            Logger.Info($"Stream #{Id}", $"Обработчик Hopper создан, текущяя конфигурация [interval:{_timeoutHopperEvent.Interval}]");
        }

        private void CheckStatus()
        {
            int c = 0, a = 0, i = 0;
            Logger.Info($"Stream #{Id}", $"Состояние очередей: " +
                                       $"[Collection: {UStages.CollectionsQueue.QueueData.Count}, " +
                                       $"Analysis: {UStages.AnalysisQueue.QueueData.Count}, " +
                                       $"Integrations: {UStages.IntegrationsQueue.QueueData.Count}]");
            foreach (var stage in UStages.Threads)
            {
                var lockedBySomeone = !Monitor.TryEnter(stage.GetLink());
                if (!lockedBySomeone)
                {
                    Monitor.Exit(stage.GetLink());
                    if(stage is Collection)
                        c++;
                    else if (stage is Analysis)
                        a++;
                    else
                        i++;
                }
            }
            Logger.Info($"Stream #{Id}", $"Свободные обработчики: " +
                                              $"[Collection: {c}, " +
                                              $"Analysis: {a}, " +
                                              $"Integrations: {i}]");
        }
        
        private void OnQueueEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            CheckStatus();
            foreach (Tuple<List<IStage>, UQueue> stage in UStages)
            {
                foreach (IStage curStage in stage.Item1)
                {
                    lock (stage.Item2.QueueAccess)
                    {
                        if (stage.Item2.QueueData.Count == 0)
                        {
                            break;
                        }
                    }
                    var lockedBySomeone = !Monitor.TryEnter(curStage.GetLink());
                    if (!lockedBySomeone)
                    {
                        Monitor.Exit(curStage.GetLink());
                        lock (stage.Item2.QueueAccess)
                        {
                            Data data = stage.Item2.QueueData.Dequeue();
                            NextStage(curStage, data);
                            curStage.SendSignal(data);
                        }
                    }
                }
            }

            OnCleanEvent();
        }

        private void NextStage(IStage curStage, Data data)
        {
            if (curStage is Collection)
                data.Status = Status.Collection;
            else if (curStage is Analysis)
                data.Status = Status.Analysis;
            else
                data.Status = Status.Integration;
        }
        
        public void InjectStages(int amountC, int amountA, int amountI)
        {
            int size = Math.Max(amountC, Math.Max(amountA, amountI));
            for (int i = 0; i < size; i++)
            {
                if (i < amountC)
                {
                    Collection local = new Collection(
                        $"Сборщик данных #{i}",
                        Id,
                        UStages.AnalysisQueue);
                    UStages.Collections.Add(local);
                    local.StartThread();
                    Settings[0]++;
                    Logger.Info($"Stream #{Id}", $"Сборщик данных #{i} Создан");
                }

                if (i < amountA)
                {
                    Analysis local = new Analysis(
                        $"Анализатор данных #{i}",
                        Id,
                        UStages.IntegrationsQueue);
                    UStages.Analysis.Add(local);
                    local.StartThread();
                    Settings[1]++;
                    Logger.Info($"Stream #{Id}", $"Анализатор данных #{i} Создан");
                }

                if (i < amountI)
                {
                    Integration local = new Integration(
                        $"Интегратор данных #{i}",
                        Id,
                        null);
                    UStages.Integrations.Add(local);
                    local.StartThread();
                    Settings[2]++;
                    Logger.Info($"Stream #{Id}", $"Интегратор данных #{i} Создан");
                }
            }
            UStages.Update();
        }

        // 
        //
        // Overload Put Method
        //
        //
        public void Upload(params Data[] dataList)
        {
            Array.ForEach(dataList.ToArray(), data => UStages.CollectionsQueue.QueueData.Enqueue(data));
        }

        public void Upload(List<Data> dataList)
        {
            Array.ForEach(dataList.ToArray(), data => UStages.CollectionsQueue.QueueData.Enqueue(data));
        }

        public void Upload(Data data)
        {
            UStages.CollectionsQueue.QueueData.Enqueue(data);
        }
    }
}