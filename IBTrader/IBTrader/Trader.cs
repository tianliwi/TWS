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
using System.Xml;

namespace IBTrader
{
    public class Trader
    {
        public IBClient ibClient;
        private EReaderMonitorSignal signal;
        private EReader reader;
        private List<HistoricalDataMessage> historicalDataList;
		public Dictionary<string, FxHistoricalDataEntry> _fxHistoricalDataDict;

        delegate void MessageHandlerDelegate(IBMessage message);

        public Trader()
        {
            signal = new EReaderMonitorSignal();
            ibClient = new IBClient(signal);
            
			_fxHistoricalDataDict = new Dictionary<string, FxHistoricalDataEntry>();

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
        
        public void GetHistoricalData(int reqId, Contract contract, string endTime, string duration, string barSize, string whatToShow)
        {
            ibClient.ClientSocket.reqHistoricalData(reqId, contract, endTime, duration, barSize, whatToShow, 1, 1, new List<TagValue>());
        }
        public static string Date2Rfc(DateTime d)
        {
            return XmlConvert.ToString(d, XmlDateTimeSerializationMode.Utc);
        }
        public static string DateLocal2Rfc(DateTime d)
        {
            return XmlConvert.ToString(d, XmlDateTimeSerializationMode.Local);
        }
        public static DateTime Rfc2Date(string s)
        {
            return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Utc);
        }
        public static DateTime Rfc2DateLocal(string s)
        {
            return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Local);
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
							string date = Date2Rfc(dt.ToUniversalTime());
							if (_fxHistoricalDataDict.ContainsKey(date))
							{
								_fxHistoricalDataDict[date].openTime = date;
								_fxHistoricalDataDict[date].OpenAsk = b.Open;
								_fxHistoricalDataDict[date].HighAsk = b.High;
								_fxHistoricalDataDict[date].LowAsk = b.Low;
								_fxHistoricalDataDict[date].CloseAsk = b.Close;
							}
							else {
								FxHistoricalDataEntry d = new FxHistoricalDataEntry();
								d.openTime = date;
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
                            string date = Date2Rfc(dt.ToUniversalTime());
							if (_fxHistoricalDataDict.ContainsKey(date))
							{
								_fxHistoricalDataDict[date].openTime = date;
								_fxHistoricalDataDict[date].OpenBid = b.Open;
								_fxHistoricalDataDict[date].HighBid = b.High;
								_fxHistoricalDataDict[date].LowBid = b.Low;
								_fxHistoricalDataDict[date].CloseBid = b.Close;
							}
							else {
                                FxHistoricalDataEntry d = new FxHistoricalDataEntry();
								d.openTime = date;
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
