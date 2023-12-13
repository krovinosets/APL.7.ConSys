using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Core.Controllers;
using Core.Templates;
using LoggerManager;

namespace Core.Stages
{
    public class Stage : IStage
    {
        private Thread _thread;
        private object _linker = new object();
        private DateTime _lastActivity;

        private Data _data;

        private bool _inUse;
        private readonly UQueue _nextQueue;
        protected int SpeedMbs;
        private string _source;
        private int _id;

        protected Stage(string source, int id, UQueue nextQueue, int speedMbs)
        {
            _thread = new Thread(Start);
            _source = source;
            _nextQueue = nextQueue;
            _lastActivity = DateTime.Now;
            _inUse = true;
            _id = id;
            SpeedMbs = speedMbs;
        }

        public void StartOneThread(Data data)
        {
            Utilities utils = new Utilities();
            int ets = utils.CalculateETS(data.Size, SpeedMbs);
            DateTime start = DateTime.Now;
            Logger.Info(_source, $"Работаю с задачей, время работы ~{ets} секунд");
            data.Handle(ets);
            TimeSpan realTime = DateTime.Now - start;
            _lastActivity = DateTime.Now;
            Logger.Info(_source, $"Закончил работу с данными ID:{data.Id}. Время ожидаемое: ~{ets}, Реальное время: {realTime}");
            data.timeLoad.Add(data.Status, realTime);
        }
        
        public void StartThread()
        {
            _thread.Start();
        }
        
        public Thread GetThread()
        {
            return _thread;
        }
        
        public Object GetLink()
        {
            return _linker;
        }
        
        public void SendSignal(Data data)
        {
            lock (_linker)
            {
                _data = data;
                Monitor.Pulse(_linker);
            }
        }

        public void Stop()
        {
            lock (_linker)
            {
                Logger.Info(_source, "Остановка");
                _inUse = false;
                Monitor.Pulse(_linker);
            }
        }

        public DateTime GetLastActivity()
        {
            return _lastActivity;
        }
        
        public void Start()
        {
            Logger.Info(_source, $"Поток {Environment.CurrentManagedThreadId} запущен.");
            Utilities utils = new Utilities();
            while(_inUse){
                lock (_linker)
                {
                    Logger.Info(_source, "Встал на ожидание");
                    Monitor.Wait(_linker);
                    if(!_inUse)
                        break;
                    Logger.Info(_source, "Получен сигнал на работу");
                    if (_data == null)
                    {
                        Logger.Info(_source, "Data оказалось null");
                        continue;
                    }
                    
                    int ets = utils.CalculateETS(_data.Size, SpeedMbs);
                    DateTime start = DateTime.Now;
                    Logger.Info(_source, $"Работаю с задачей, время работы ~{ets} секунд");
                    _data.Handle(ets);
                    TimeSpan realTime = DateTime.Now - start;
                    _lastActivity = DateTime.Now;
                    Logger.Info(_source, $"Закончил работу с данными ID:{_data.Id}. Время ожидаемое: ~{ets}, Реальное время: {realTime}");
                    _data.timeLoad.Add(_data.Status, realTime);
                    if (_nextQueue == null)
                    {
                        _data.Status = Status.Ready;
                        MetricsHolder.DataMetric[_id].Add(new Tuple<int, Dictionary<Status, TimeSpan>>(_data.Id, _data.timeLoad));
                        continue;
                    }

                    lock (_nextQueue.QueueAccess)
                    {
                        _nextQueue.QueueData.Enqueue(_data);
                    }

                    _data = null;
                }
            }
        }
    }
}