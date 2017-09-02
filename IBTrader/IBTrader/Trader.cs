using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using System.Threading;
using System.Data;
using System.IO;

namespace IBTrader
{
    public class Trader
    {
        public IBClient ibClient;
        private EReaderMonitorSignal signal;
        private EReader reader;
        private DataTable historicalDataChunk;
        private List<HistoricalDataMessage> historicalDataList;
        public List<string> _historicalDataList;
        private bool _allHistoricalDataLoaded;

        delegate void MessageHandlerDelegate(IBMessage message);

        public Trader()
        {
            signal = new EReaderMonitorSignal();
            ibClient = new IBClient(signal);

            _historicalDataList = new List<string>();
            _allHistoricalDataLoaded = false;

            historicalDataList = new List<HistoricalDataMessage>();

            ibClient.HistoricalData += (reqId, date, open, high, low, close, volume, count, WAP, hasGaps) =>
                HandleMessage(new HistoricalDataMessage(reqId, date, open, high, low, close, volume, count, WAP, hasGaps));
            ibClient.HistoricalDataEnd += (reqId, startDate, endDate) => HandleMessage(new HistoricalDataEndMessage(reqId, startDate, endDate)); ;
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

        private void prepareHistoricalDataTable()
        {
            historicalDataChunk = new DataTable("HDChunk");
            historicalDataChunk.Columns.Add("time", typeof(string));
            historicalDataChunk.Columns.Add("open", typeof(double));
            historicalDataChunk.Columns.Add("high", typeof(double));
            historicalDataChunk.Columns.Add("low", typeof(double));
            historicalDataChunk.Columns.Add("close", typeof(double));
            historicalDataChunk.Columns.Add("volume", typeof(double));
        }
        public void HandleMessage(IBMessage message)
        {
            switch (message.Type)
            {
                case MessageType.HistoricalData:
                    {
                        /*
                        HistoricalDataMessage hdMessage = (HistoricalDataMessage)message;
                        DataRow row = historicalDataChunk.NewRow();
                        row["time"] = hdMessage.Date;
                        row["open"] = hdMessage.Open;
                        row["high"] = hdMessage.High;
                        row["low"] = hdMessage.Low;
                        row["close"] = hdMessage.Close;
                        row["volume"] = 0;
                        historicalDataChunk.Rows.Add(row);
                        */
                        //historicalDataList.Add((HistoricalDataMessage)message);
                        HistoricalDataMessage b = (HistoricalDataMessage)message;
                        string line = b.Date + "," + b.Open + "," + b.High + "," + b.Low + "," + b.Close + "," + b.Volume;
                        _historicalDataList.Add(line);
                        break;
                    }
                case MessageType.HistoricalDataEnd:
                    {
                        /*
                        historicalDataChunk.WriteXml("test.xml");

                        var lines = new List<string>();
                        var valueLines = historicalDataChunk.AsEnumerable().Select(
                            row => string.Join(", ", row.ItemArray));
                        lines.AddRange(valueLines);
                        File.WriteAllLines("excel.csv", lines);
                        */
                        /*
                        var lines = new List<string>();
                        foreach(var data in historicalDataList)
                        {
                            var line = data.Date + ", " + data.Open + ", " + data.High + ", " + data.Low + ", " + data.Close + ", 0";
                            lines.Add(line);
                        }
                        File.WriteAllLines("excel.csv", lines);
                        HistoricalDataEndMessage hdeMessage = (HistoricalDataEndMessage)message;
                        Console.WriteLine("Historical data from {0} to {1} has been saved.", hdeMessage.StartDate, hdeMessage.EndDate);
                        */
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

    }
}
