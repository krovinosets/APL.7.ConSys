using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Core.Templates;

namespace Core.Stages
{
    public class Collection : Stage
    {
        public Collection(string source, int id, UQueue nextQueue) : 
            base(
                source,
                id,
                nextQueue,
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("CollectionsSpeed"))
            )
        {
        }
        
    }
}