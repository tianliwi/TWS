using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using IBTrader;

namespace HistoricalDataDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            EReaderMonitorSignal signal = new EReaderMonitorSignal();
            try
            {
                Trader trader = new Trader();
                trader.Connect();

                Contract contract = new Contract();
                string endTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");
                Console.WriteLine(endTime);
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
        }
    }
}
