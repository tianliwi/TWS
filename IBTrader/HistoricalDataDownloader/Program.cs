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
            M1Loader m1Loader = new M1Loader("1/1/2016", "1/1/2017", "AUD");
            m1Loader.LoadM1Data();
            //H4Loader h4Loader = new H4Loader("1/1/2016", "1/1/2017", "AUD");
            //H4Loader h4Loader = new H4Loader("1/1/2017", "");
            //h4Loader.LoadM1Data();
        }
    }
}
