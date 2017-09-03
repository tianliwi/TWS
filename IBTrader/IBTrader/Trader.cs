using System;
using System.Collections.Generic;
using System.Globalization;
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
        public List<FxHistoricalDataMessage> _fxHistoricalDataList;
		public Dictionary<string, FxHistoricalDataMessage> _fxHistoricalDataDict;
        private bool _allHistoricalDataLoaded;

        delegate void MessageHandlerDelegate(IBMessage message);

        public Trader()
        {
            signal = new EReaderMonitorSignal();
            ibClient = new IBClient(signal);

            _fxHistoricalDataList = new List<FxHistoricalDataMessage>();
			_fxHistoricalDataDict = new Dictionary<string, FxHistoricalDataMessage>();
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
						HistoricalDataMessage b = (HistoricalDataMessage)message;
						if (b.RequestId == (int)MessageType.FxHistoricalAsk)
						{
							DateTime dt = DateTime.ParseExact(b.Date, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture);
							dt += new TimeSpan(3, 0, 0);
							string date = dt.ToString("yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture);
							if (_fxHistoricalDataDict.ContainsKey(date))
							{
								_fxHistoricalDataDict[date].Date = date;
								_fxHistoricalDataDict[date].OpenAsk = b.Open;
								_fxHistoricalDataDict[date].HighAsk = b.High;
								_fxHistoricalDataDict[date].LowAsk = b.Low;
								_fxHistoricalDataDict[date].CloseAsk = b.Close;
							}
							else {
								FxHistoricalDataMessage d = new FxHistoricalDataMessage();
								d.Date = date;
								d.OpenAsk = b.Open;
								d.HighAsk = b.High;
								d.LowAsk = b.Low;
								d.CloseAsk = b.Close;
								_fxHistoricalDataDict.Add(date,d);
							}
						}
						else if (b.RequestId == (int)MessageType.FxHistoricalBid)
						{
							DateTime dt = DateTime.ParseExact(b.Date, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture);
							dt += new TimeSpan(3, 0, 0);
							string date = dt.ToString("yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture);
							if (_fxHistoricalDataDict.ContainsKey(date))
							{
								_fxHistoricalDataDict[date].Date = date;
								_fxHistoricalDataDict[date].OpenBid = b.Open;
								_fxHistoricalDataDict[date].HighBid = b.High;
								_fxHistoricalDataDict[date].LowBid = b.Low;
								_fxHistoricalDataDict[date].CloseBid = b.Close;
							}
							else {
								FxHistoricalDataMessage d = new FxHistoricalDataMessage();
								d.Date = date;
								d.OpenBid = b.Open;
								d.HighBid = b.High;
								d.LowBid = b.Low;
								d.CloseBid = b.Close;
								_fxHistoricalDataDict.Add(date, d);
							}
						}
						break;
					}
                case MessageType.HistoricalDataEnd:
                    {
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
