using System;
namespace IBTrader
{
	public class FxHistoricalDataMessage : IBMessage
	{
		public string Date;
		public double OpenAsk;
		public double OpenBid;
		public double HighAsk;
		public double HighBid;
		public double LowAsk;
		public double LowBid;
		public double CloseAsk;
		public double CloseBid;
		public double Volume;
		public FxHistoricalDataMessage()
		{
			Volume = -1;
		}
		public override string ToString()
		{
			return Date + 
				"," + OpenAsk +
				"," + OpenBid +
				"," + HighAsk +
				"," + HighBid +
				"," + LowAsk +
				"," + LowBid +
				"," + CloseAsk +
				"," + CloseBid +
				"," + Volume;
		}
	}
}
