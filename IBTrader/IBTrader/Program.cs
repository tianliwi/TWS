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
                ibClient.HistoricalData += (reqId, date, open, high, low, close, volume, count, WAP, hasGaps) =>
                    HandleMessage(new HistoricalDataMessage(reqId, date, open, high, low, close, volume, count, WAP, hasGaps));
                ibClient.ClientSocket.eConnect("127.0.0.1", 7496, 1);

                var reader = new EReader(ibClient.ClientSocket, signal);
                reader.Start();

                new Thread(() => { while (ibClient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

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

                Console.ReadKey();
                ibClient.ClientSocket.eDisconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static public void HandleMessage(IBMessage message)
        {
            if (this.InvokeRequired)
            {
                MessageHandlerDelegate callback = new MessageHandlerDelegate(HandleMessage);
                this.Invoke(callback, new object[] { message });
            }
            else
            {
                UpdateUI(message);
            }
        }
    }
}