using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using System.Threading;

namespace IBTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            EReaderMonitorSignal signal = new EReaderMonitorSignal();
            try
            {
                IBClient ibClient = new IBClient(signal);

                ibClient.ClientSocket.eConnect("127.0.0.1", 7496, 1);

                var reader = new EReader(ibClient.ClientSocket, signal);
                reader.Start();

                new Thread(() => { while (ibClient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

                Contract contract = new Contract();
                string endTime = "20170824 00:00:04";
                string duration = "3000 S";
                string barSize = "1 min";
                string whatToShow = "MIDPOINT";
                contract.Symbol = "EUR";
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";
                ibClient.ClientSocket.reqHistoricalData(111, contract, endTime,
                    duration, barSize, whatToShow, 0, 1,
                    new List<TagValue>());

                Console.ReadKey();
                ibClient.ClientSocket.eDisconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
