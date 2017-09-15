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
            M1Loader m1Loader = new M1Loader("2/23/2013", "1/1/2014", "EUR");
            m1Loader.LoadM1Data();
            //DataProcessor dataProc = new DataProcessor("2016");
            //dataProc.generateData();
        }
    }
}
