using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using IBTrader;
using System.ComponentModel;
using System.Threading;
using System.Collections.Concurrent;

namespace BackTest
{
    public class BackTestEngine
    {
        // run backtest
        protected BackgroundWorker _playthread = new BackgroundWorker();
        protected ConcurrentQueue<string> cq = new ConcurrentQueue<string>();

        public BackTestEngine()
        {
            _playthread.DoWork += new DoWorkEventHandler(Play);
        }

        public void Start()
        {
            _playthread.RunWorkerAsync();

            while(true)
            {
                string s;
                if (cq.TryDequeue(out s))
                {
                    Console.WriteLine(s);
                }
            }

        }

        protected virtual void Play(object sender, DoWorkEventArgs e)
        {
            for(int i=0;i<10;i++)
            {
                cq.Enqueue(i.ToString());
                Thread.Sleep(500);
            }
        }
    }
}
