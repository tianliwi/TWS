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
            M1Loader m1Loader = new M1Loader("1/6/2016", "1/1/2017", "AUD");
            //m1Loader.JoinDailyData("EUR");
            m1Loader.LoadM1Data();
            //DataProcessor dataProc = new DataProcessor("2011");
            //dataProc.CheckData();
            //dataProc.generateH4();
            //dataProc.generateD1();
        }
    }
}
