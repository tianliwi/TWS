using System;
using System.Collections.Generic;
using IBApi;
using System.Threading;

namespace IBHistoricalConsole
{
    public class Downloader
    {
        public void Run()
        {
            string strEndDate = "20160505 00:00:00 UTC";
            string strDuration = "28800 S";
            string strBarSize = "1 min";
            string strWhatToShow = "BID_ASK";
            EReaderMonitorSignal signal = new EReaderMonitorSignal();
            EWrapperLit ibClient = new EWrapperLit();

            try
            {
                ibClient.ClientSocket.eConnect("127.0.0.1", 7490, 1);

                var reader = new EReader(ibClient.ClientSocket, signal);
                reader.Start();

                new Thread(() =>
                {
                    while (ibClient.ClientSocket.IsConnected())
                    {
                        signal.waitForSignal();
                        reader.processMsgs();
                    }
                })
                { IsBackground = false }.Start();

                Contract contract = new Contract();
                contract.Symbol = "EUR";
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";

                ibClient.ClientSocket.reqHistoricalData(111, contract, strEndDate,
                    strDuration, strBarSize, strWhatToShow, 0, 1,
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
