using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using System.Threading;

namespace IBTrader
{
    public class Trader
    {
        public IBClient ibClient;
        private EReaderMonitorSignal signal;
        private EReader reader;

        delegate void MessageHandlerDelegate(IBMessage message);

        public Trader()
        {
            signal = new EReaderMonitorSignal();
            ibClient = new IBClient(signal);
            
            ibClient.HistoricalData += (reqId, date, open, high, low, close, volume, count, WAP, hasGaps) =>
                HandleMessage(new HistoricalDataMessage(reqId, date, open, high, low, close, volume, count, WAP, hasGaps));
        }

        public void Connect()
        {
            ibClient.ClientSocket.eConnect("127.0.0.1", 7496, 1);
            reader = new EReader(ibClient.ClientSocket, signal);
            reader.Start();
            new Thread(() => { while (ibClient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
        }

        public void Disconnect()
        {
            ibClient.ClientSocket.eDisconnect();
        }

        public void HandleMessage(IBMessage message)
        {
            MessageHandlerDelegate callback = new MessageHandlerDelegate(HandleMessage);
        }

    }
}
