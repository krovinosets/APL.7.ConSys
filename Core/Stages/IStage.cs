using System;
using System.Threading;
using Core.Templates;

namespace Core.Stages
{
    public interface IStage
    {
        void Start();
        void SendSignal(Data data);
        void Stop();
        Object GetLink();
        Thread GetThread();
        DateTime GetLastActivity();
        void StartThread();
        void StartOneThread(Data data);
    }
}