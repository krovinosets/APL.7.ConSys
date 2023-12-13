using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Templates
{
    public class Data
    {

        public readonly int Size;
        public readonly int Id;
        public Status Status;
        private Random _random = new Random();
        public Dictionary<Status, TimeSpan> timeLoad;
        
        public Data(int id, int sizeMb)
        {
            Id = id;
            Size = sizeMb;
            Status = Status.Waiting;
            timeLoad = new Dictionary<Status, TimeSpan>();
        }

        public void Handle(int est)
        {
            int sfx = _random.Next(0, est);
            Thread.Sleep(est * 1000 + sfx * 1000);
        }
    }
}