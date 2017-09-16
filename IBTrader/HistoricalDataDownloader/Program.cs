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
            //M1Loader m1Loader = new M1Loader("1/1/2011", "1/1/2012", "EUR");
            //m1Loader.JoinDailyData("EUR");
            //m1Loader.LoadM1Data();
            DataProcessor dataProc = new DataProcessor("2012");
            //dataProc.CheckData();
            dataProc.generateH4();
        }
    }
}
