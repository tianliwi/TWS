using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //testData();
            //return;
            Trader trader = new Trader();
            trader.Connect();

            string _enddate = "20170801";// DateTime.Today.ToString("yyyyMMdd");

            DateTime _startTime = DateTime.SpecifyKind(DateTime.ParseExact(_enddate, "yyyyMMdd", null), DateTimeKind.Local);
            DateTime _endTime = _startTime + new TimeSpan(1, 0, 0, 0);          // one day later
            double sec = _endTime.Subtract(_startTime).TotalSeconds;      // should be 24 hours or 86,400 secs

            long space = 28800;

            long totalRequests = (long)sec / space;           // one request is 30 min, totalRequests = 48
            TimeSpan eightHours = new TimeSpan((int)space/3600, 0, 0);

            DateTime s = _startTime;
            DateTime t = _startTime + eightHours;
            string sym = "EUR";

            Console.WriteLine("Requesting historical bars for :" + sym);
            for (int i = 0; i < totalRequests; i++)
            {
                Console.WriteLine("Request #: " + (i + 1).ToString() + "/" + totalRequests);
                // 1 = 1 second
                Contract contract = new Contract();

                string duration = space.ToString() + " S";
                string barSize = "1 min";
                contract.Symbol = sym;
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";
                trader.ibClient.ClientSocket.reqHistoricalData(200, contract, t.ToString("yyyyMMdd HH:mm:ss"), duration, barSize, "MIDPOINT", 1, 1, new List<TagValue>());

                // Do not make more than 60 historical data requests in any ten-minute period.
                // If I have 10 names, each can only make 6 requests in ten minute;
                // I use 5 minute for a pause; Then 24 hours takes 120 min or 1.5hour
                // Thread.Sleep(new TimeSpan(0, 5, 0));
                // wait 10 secs
                Thread.Sleep(10000);
                s += eightHours;
                t += eightHours;
            }

            File.WriteAllLines("origin.csv", trader._historicalDataList);
            File.WriteAllLines("nodup.csv", trader._historicalDataList.Distinct().ToList());

            Console.WriteLine("done.");
            Console.ReadKey();
            trader.Disconnect();
            /*
            try
            {
                Trader trader = new Trader();
                trader.Connect();

                Contract contract = new Contract();
                string startTime = "20150101 00:00:00 GMT";
                string endTime = "20150102 00:00:00 GMT";//"20170901  06:16:00 GMT";// DateTime.Now.ToString("yyyyMMdd HH:mm:ss");

                string duration = "28800 S";
                string barSize = "1 min";
                //string whatToShow = "BID";
                contract.Symbol = "EUR";
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";

                trader.ibClient.ClientSocket.reqHistoricalData(200, contract, endTime, duration, barSize, "BID", 0, 1, new List<TagValue>());

                Console.ReadKey();
                trader.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            */
        }
        static void testData()
        {

            try
            {
                Trader trader = new Trader();
                trader.Connect();

                Contract contract = new Contract();
                string endTime = "20170830  14:15:00";//"20170901  06:16:00 GMT";// DateTime.Now.ToString("yyyyMMdd HH:mm:ss");

                string duration = "900 S";
                string barSize = "1 min";
                //string whatToShow = "BID";
                contract.Symbol = "EUR";
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";

                trader.ibClient.ClientSocket.reqHistoricalData(200, contract, endTime, duration, barSize, "BID", 0, 1, new List<TagValue>());

                Console.ReadKey();
                foreach(string s in trader._historicalDataList)
                {
                    Console.WriteLine(s);
                }
                trader.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
