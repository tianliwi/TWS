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
using System.IO;

namespace BackTest
{
    public class BackTestEngine
    {
        // run backtest
        protected BackgroundWorker _playthread = new BackgroundWorker();
        protected ConcurrentQueue<string> cq = new ConcurrentQueue<string>();
        public DateTime tickMinDate;
        public DateTime tickMaxDate;
        public string[] tickFiles;
        public Dictionary<DateTime, FxHistoricalDataEntry> fxM1Dict = new Dictionary<DateTime, FxHistoricalDataEntry>();

        public BackTestEngine()
        {
            _playthread.DoWork += new DoWorkEventHandler(Play);
            LoadTicks("E:/GitHub/TWS/Data/EUR/2017/2017_M1.csv");
        }

        public void LoadTicks(string filename)
        {
            Console.WriteLine("Starting loading tick data...");
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                FxHistoricalDataEntry entry = new FxHistoricalDataEntry();
                entry.Date = cols[0];
                entry.OpenAsk = Convert.ToDouble(cols[1]);
                entry.OpenBid = Convert.ToDouble(cols[2]);
                entry.HighAsk = Convert.ToDouble(cols[3]);
                entry.HighBid = Convert.ToDouble(cols[4]);
                entry.LowAsk = Convert.ToDouble(cols[5]);
                entry.LowBid = Convert.ToDouble(cols[6]);
                entry.CloseAsk = Convert.ToDouble(cols[7]);
                entry.CloseBid = Convert.ToDouble(cols[8]);
                fxM1Dict[DateTime.SpecifyKind(DateTime.ParseExact(cols[0], "yyyyMMdd  HH:mm:ss", null), DateTimeKind.Local)] = entry;
            }
            tickMinDate = fxM1Dict.Keys.Min();
            tickMaxDate = fxM1Dict.Keys.Max();
            Console.WriteLine("Data loaded, from {0} to {1}.", tickMinDate, tickMaxDate);
        }

        public void Start()
        {
            _playthread.RunWorkerAsync();

            while (true)
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
                cq.Enqueue(Thread.CurrentThread.ManagedThreadId.ToString() + ", " + i.ToString());
                Thread.Sleep(500);
            }
        }

        public FxHistoricalDataEntry GetTickData(DateTime dateTime)
        {
            if (!fxM1Dict.ContainsKey(dateTime))
            {
                return null;
            }
            return fxM1Dict[dateTime];
        }
    }
}
