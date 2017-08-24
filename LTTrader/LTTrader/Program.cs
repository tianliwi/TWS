using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using IBApi;
using BaseModel;

namespace LTTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            IBClient client = new IBClient();
            client.Connect();
            var reader = new EReader(client.ClientSocket, client.Signal);
            reader.Start();

            new Thread(() =>
            {
                while (client.isConnected)
                {
                    client.Signal.waitForSignal();
                    reader.processMsgs();
                }
            })
            {
                IsBackground = false
            }.Start();



            Contract contract = new Contract();
            contract.Symbol = "EUR";
            contract.SecType = "CASH";
            contract.Exchange = "IDEALPRO";
            contract.Currency = "USD";

            string strEndDate = "20160505 00:00:00 UTC";
            string strDuration = "20 D";
            string strBarSize = "1 day";
            string strWhatToShow = "BID";

            client.ClientSocket.reqHistoricalData(111, contract, strEndDate,
                strDuration, strBarSize, strWhatToShow, 0, 1,
                new List<TagValue>());

            Console.ReadKey();

            client.ClientSocket.eDisconnect();
        }
    }
}
