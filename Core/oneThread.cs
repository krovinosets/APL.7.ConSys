using System;
using System.Collections.Generic;
using Core.Controllers;
using Core.Stages;
using Core.Templates;

namespace Core
{
    public class OneThread
    {
        private Collection _collection;
        private Analysis _analysis;
        private Integration _integration;

        private List<Data> _dataList;

        public OneThread(List<Data> dataList)
        {
            MetricsHolder.DataMetric.Add(-1, new List<Tuple<int, Dictionary<Status, TimeSpan>>>());
            _collection = new Collection("Сборщик данных-ОДИН ПОТОК", -1, null);
            _analysis = new Analysis("Анализатор данных-ОДИН ПОТОК", -1, null);
            _integration = new Integration("Интегратор данных-ОДИН ПОТОК", -1, null);
            _dataList = new List<Data>(dataList);
        }

        public void Start()
        {
            MetricsHolder.DataMetric.Clear();
            foreach (var data in _dataList)
            {
                data.Status = Status.Collection;
                _collection.StartOneThread(data);
                data.Status = Status.Analysis;
                _analysis.StartOneThread(data);
                data.Status = Status.Integration;
                _integration.StartOneThread(data);
                data.Status = Status.Ready;
                MetricsHolder.DataMetric[-1].Add(new Tuple<int, Dictionary<Status, TimeSpan>>(data.Id, data.timeLoad));
            }
        }
    }
}