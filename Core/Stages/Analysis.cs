using System;
using System.Collections.Generic;
using System.Configuration;
using Core.Templates;

namespace Core.Stages
{
    public class Analysis : Stage
    {
        public Analysis(string source, int id, UQueue nextQueue) : 
            base(
                source,
                id,
                nextQueue,
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("AnalysisSpeed"))
            )
        {
        }
    }
}