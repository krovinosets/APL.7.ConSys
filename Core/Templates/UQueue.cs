using System.Collections.Generic;

namespace Core.Templates
{
    public class UQueue
    {
        public readonly object QueueAccess = new object();
        public Queue<Data> QueueData = new Queue<Data>();
    }
}