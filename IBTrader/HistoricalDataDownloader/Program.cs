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
            //M1Loader m1Loader = new M1Loader("12/31/2016", "1/2/2017", "AUD");
            //m1Loader.JoinDailyData("AUD");
            //m1Loader.LoadM1Data();
            DataProcessor dataProc = new DataProcessor("AUD", "2017");
            //dataProc.CheckData();
            dataProc.generateH4("AUD");
            dataProc.generateD1("AUD");
        }
    }
}
