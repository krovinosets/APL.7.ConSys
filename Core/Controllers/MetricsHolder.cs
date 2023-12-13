using System;
using System.Collections.Generic;
using System.Linq;
using Core.Templates;
using LoggerManager;

namespace Core.Controllers
{
    public static class MetricsHolder
    {
        public static Dictionary<int, List<Tuple<int, Dictionary<Status, TimeSpan>>>> DataMetric = new Dictionary<int,List<Tuple<int, Dictionary<Status, TimeSpan>>>>();
        
        public static void Output(string source, int id)
        {
            string output = "";
            foreach (var item in DataMetric[id])
            {
                int id_ = item.Item1;
                output += $"\nData-{id_}\n(\n{string.Join(Environment.NewLine, item.Item2)}\n)\n";
            }
            Logger.Info(source, output);
        }

        public static TimeSpan CalculateCommonSum(int id)
        {
            TimeSpan sum = TimeSpan.Zero;
            foreach (var item in DataMetric[id])
            {
                foreach (var num in item.Item2)
                {
                    sum += num.Value.Duration();
                }
            }

            return sum;
        }
    }
}