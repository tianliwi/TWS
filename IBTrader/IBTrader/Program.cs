using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using System.IO;
using System.Threading;

namespace IBTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines("E:/GitHub/TWS/Data/EUR/2017/2017_M1.csv");

            Dictionary<DateTime, FxHistoricalDataEntry> fxHistoricalDataDict = new Dictionary<DateTime, FxHistoricalDataEntry>();
            foreach(string line in lines)
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
                fxHistoricalDataDict[DateTime.SpecifyKind(DateTime.ParseExact(cols[0], "yyyyMMdd  HH:mm:ss", null), DateTimeKind.Local)] = entry;
            }
            DateTime mostRecentHist = fxHistoricalDataDict.Keys.Max();
        }
    }
}