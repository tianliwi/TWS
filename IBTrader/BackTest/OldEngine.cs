using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using IBTrader;
using System.ComponentModel;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

namespace BackTest
{
    public class OldEngine
    {
        // run backtest
        protected ConcurrentQueue<string> cq = new ConcurrentQueue<string>();

        public DateTime tickMinDate;
        public DateTime tickMaxDate;
        public DateTime tickCur;

        public string[] tickFiles;

        private DataRepo data;

        private LinkedList<FxHistoricalDataEntry> barM1;
        private LinkedList<FxHistoricalDataEntry> barH4;

        public OldEngine()
        {
            data = new DataRepo("AUD", "2016");
            barM1 = new LinkedList<FxHistoricalDataEntry>();
            barH4 = new LinkedList<FxHistoricalDataEntry>();
            LoadTicks();
        }
        public void LoadTicks()
        {
            Console.WriteLine("Starting loading tick data...");
            data.LoadCSV();
            tickMinDate = data.DataM1.Keys.Min();
            tickMaxDate = data.DataM1.Keys.Max();
            tickCur = tickMinDate.AddMinutes(-1);
        }
        public void Start()
        {
            double R, open = 0, tp = 0, sl = 0, R2;
            bool orderOpened = false;
            bool orderClosed = true;
            double profit = 0;
            double loss = 0;
            int winNum = 0;
            int loseNum = 0;
            int h4Cnt = 0;
   
            while (tickCur < tickMaxDate)
            {
                Play();
                if (data.DataH4.ContainsKey(tickCur))
                {
                    barH4.AddFirst(data.DataH4[tickCur]);
                    if (h4Cnt++ >= 19)
                    {
                        R = getLastHigh(12) - getLastLow(12);
                        R2 = getLastHigh(18) - getLastLow(18);
                        if (true)//(R2 >= 0.013 && R2 <= 0.019)
                        {
                            open = barH4.First.Value.OpenAsk - R * 0.12;
                            sl = -0.005;
                            tp = R * 0.32;
                            orderOpened = false;
                            orderClosed = false;
                            //Console.WriteLine("At {0}, put order with limit price {1}, stoploss {2}, take profit {3}.", tickCur, open, sl, tp);
                        }
                    }
                }
                if (data.DataM1.ContainsKey(tickCur))
                {
                    barM1.AddFirst(data.DataM1[tickCur]);
                    if (orderClosed) continue;
                    FxHistoricalDataEntry cur = barM1.First.Value;
                    if (!orderOpened)
                    { 
                        if ((cur.LowBid < open) && cur.HighBid > open)
                        {
                            orderOpened = true;
                            //Console.WriteLine("Order placed at {0}.", tickCur);
                        }
                    }
                    if (orderOpened)
                    {
                        if (cur.LowBid < open + tp && cur.HighBid > open + tp)
                        {
                            profit += tp;
                            //Console.WriteLine("Take profit at {0}.", tickCur);
                            orderClosed = true;
                            winNum++;
                        }
                        if (cur.LowBid < open + sl && cur.HighBid > open + sl)
                        {
                            loss += sl;
                            //Console.WriteLine("Stop loss at {0}.", tickCur);
                            orderClosed = true;
                            loseNum++;
                        }
                    }
                }
            }
            Console.WriteLine("win {0}, {1}\nlose {2},{3}", winNum, profit, loseNum, loss);
        }
        private double getLastHigh(int n)
        {
            double res = -1;
            LinkedListNode<FxHistoricalDataEntry> node = barH4.First;
            while (n-- > 0)
            {
                if (node.Next == null) break;
                node = node.Next;
                res = Math.Max(res, node.Value.HighAsk);
            }
            return res;
        }
        private double getLastLow(int n)
        {
            double res = 1000;
            LinkedListNode<FxHistoricalDataEntry> node = barH4.First;
            while (n-- > 0)
            {
                if (node.Next == null) break;
                node = node.Next;
                res = Math.Min(res, node.Value.LowAsk);
            }
            return res;
        }
        protected void Play()
        {
            tickCur = tickCur.AddMinutes(1);
        }
    }
}