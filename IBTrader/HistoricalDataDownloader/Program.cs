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

            string _enddate = "20170831";

            DateTime _startTime = DateTime.SpecifyKind(DateTime.ParseExact(_enddate, "yyyyMMdd", null), DateTimeKind.Local);
            DateTime _endTime = _startTime + new TimeSpan(1, 0, 0, 0);          // one day later
            double sec = _endTime.Subtract(_startTime).TotalSeconds;      // should be 24 hours or 86,400 secs

            long space = 28800;

            long totalRequests = (long)sec / space;           // one request is 30 min, totalRequests = 48
            TimeSpan eightHours = new TimeSpan((int)space/3600, 0, 0);

            DateTime s = _startTime;
            DateTime t = _startTime + eightHours;
            string sym = "EUR";

			Console.WriteLine("Requesting historical bars for {0} on {1}...", sym, _startTime.ToShortDateString());
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

				trader.ibClient.ClientSocket.reqHistoricalData((int)MessageType.FxHistoricalAsk, contract, t.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "ASK", 1, 1, new List<TagValue>());
				Thread.Sleep(10000);

				trader.ibClient.ClientSocket.reqHistoricalData((int)MessageType.FxHistoricalBid, contract, t.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "BID", 1, 1, new List<TagValue>());

                // Do not make more than 60 historical data requests in any ten-minute period.
                // If I have 10 names, each can only make 6 requests in ten minute;
                // I use 5 minute for a pause; Then 24 hours takes 120 min or 1.5hour
                // Thread.Sleep(new TimeSpan(0, 5, 0));
                // wait 10 secs
                Thread.Sleep(10000);
                s += eightHours;
                t += eightHours;
            }

			string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)));
			Console.WriteLine(projectPath);
			File.WriteAllLines("/Users/tianli8012/GitHub/TWS/nodup.csv", trader._fxHistoricalDataDict.Distinct().Select(i => i.Value.ToString()));

			Console.WriteLine("done.");
            Console.ReadKey();
            trader.Disconnect();
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
                contract.Symbol = "EUR";
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";

				trader.ibClient.ClientSocket.reqHistoricalData((int)MessageType.FxHistoricalBid, contract, endTime, duration, barSize, "BID", 0, 1, new List<TagValue>());

                Console.ReadKey();
                foreach(var s in trader._fxHistoricalDataList)
                {
					Console.WriteLine(s.ToString());
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
