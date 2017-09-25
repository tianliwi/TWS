using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using IBTrader;
using System.Threading;
using System.IO;

namespace HistoricalDataDownloader
{
    class Program
    {

        static void Main(string[] args)
        {
            //M1Loader m1Loader = new M1Loader("1/6/2016", "1/1/2017", "AUD");
            //m1Loader.JoinDailyData("AUD");
            //m1Loader.LoadM1Data();
            DataProcessor dataProc = new DataProcessor("AUD", "2016");
            //dataProc.CheckData();
            dataProc.generateH4("AUD");
            //Console.WriteLine(Trader.Rfc2Date("2016-01-06T07:59:00Z").ToLocalTime());
            //Console.WriteLine(Trader.Rfc2Date("2016-01-06T08:00:00Z").ToLocalTime());
            //Console.WriteLine("{0}, {1}", dataProc.getH4Slot(Trader.Rfc2Date("2016-01-06T07:59:00Z")), dataProc.getH4Slot(Trader.Rfc2Date("2016-01-06T08:00:00Z")));
            //dataProc.generateD1("AUD");
        }
    }
}
