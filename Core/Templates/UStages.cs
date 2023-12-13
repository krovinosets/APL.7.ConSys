using System;
using System.Collections;
using System.Collections.Generic;
using Core.Stages;

namespace Core.Templates
{
    public class UStages : IEnumerable
    {
        public List<IStage> Collections;
        public List<IStage> Analysis;
        public List<IStage> Integrations;

        public UQueue CollectionsQueue;
        public UQueue AnalysisQueue;
        public UQueue IntegrationsQueue;

        private List<Tuple<List<IStage>, UQueue>> _iter;
        public List<IStage> Threads;
        
        public UStages()
        {
            Collections = new List<IStage>();
            Analysis = new List<IStage>();
            Integrations = new List<IStage>();

            CollectionsQueue = new UQueue();
            AnalysisQueue = new UQueue();
            IntegrationsQueue = new UQueue();

            Threads = new List<IStage>();
            
            _iter = new List<Tuple<List<IStage>, UQueue>>()
            {
                new Tuple<List<IStage>, UQueue>(Collections, CollectionsQueue),
                new Tuple<List<IStage>, UQueue>(Analysis, AnalysisQueue),
                new Tuple<List<IStage>, UQueue>(Integrations, IntegrationsQueue),
            };
        }

        public void Update()
        {
            Threads.AddRange(Collections);
            Threads.AddRange(Analysis);
            Threads.AddRange(Integrations);
        }

        public void Remove(IStage stage)
        {
            if(stage is Collection){
                Collections.Remove(stage);
                _iter[0].Item1.Remove(stage);
            }
            else if (stage is Analysis)
            {
                Analysis.Remove(stage);
                _iter[1].Item1.Remove(stage);
            }
            else
            {
                Integrations.Remove(stage);
                _iter[2].Item1.Remove(stage);
            }

            Threads.Remove(stage);
        }
        
        public IEnumerator GetEnumerator()
        {
            return _iter.GetEnumerator();
        }
    }
}