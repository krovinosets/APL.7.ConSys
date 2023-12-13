using System;
using System.Configuration;
using Core.Templates;

namespace Core.Stages
{
    public class Integration : Stage
    {
        public Integration(string source, int id, UQueue nextQueue) : 
            base(
                source,
                id,
                nextQueue,
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("IntegrationsSpeed"))
            )
        {
        }
    }
}