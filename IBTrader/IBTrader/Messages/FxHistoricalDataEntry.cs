﻿using System;
namespace IBTrader
{
	public class FxHistoricalDataEntry
	{
		public string openTime;
        public string closeTime;
		public double OpenAsk;
		public double OpenBid;
		public double HighAsk;
		public double HighBid;
		public double LowAsk;
		public double LowBid;
		public double CloseAsk;
		public double CloseBid;
		public double Volume;

		public FxHistoricalDataEntry()
		{
			Volume = -1;
		}
		public override string ToString()
		{
			return openTime +
                "," + closeTime +
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
