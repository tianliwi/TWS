﻿using System;
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
    public class BackTestEngine
    {
        // run backtest
        protected ConcurrentQueue<Order> orderQueue;

        public DateTime tickMinDate;
        public DateTime tickMaxDate;
        public DateTime tickCur;

        public string[] tickFiles;
        private DataRepo dataRepo;

        private LinkedList<FxHistoricalDataEntry> barM1;
        private LinkedList<FxHistoricalDataEntry> barH4;

        public BackTestEngine()
        {
            dataRepo = new DataRepo("EUR", "2012");
            barM1 = new LinkedList<FxHistoricalDataEntry>();
            barH4 = new LinkedList<FxHistoricalDataEntry>();
            orderQueue = new ConcurrentQueue<Order>();
            LoadTicks();
        }

        public void LoadTicks()
        {
            Console.WriteLine("Starting loading tick data...");
            dataRepo.LoadCSV();
            tickMinDate = dataRepo.DataM1.Keys.Min();
            tickMaxDate = dataRepo.DataM1.Keys.Max();
            tickCur = tickMinDate.AddMinutes(-1);
        }

        public void Start()
        {
            double R, open=0, tp=0, sl=0, R2;
            bool orderOpened = false;
            bool orderClosed = true;
            double profit = 0;
            double loss = 0;
            int winNum = 0;
            int loseNum = 0;
            int h4Cnt = 0;
            Order order = new Order();
            double total = 25000;
            while (tickCur < tickMaxDate)
            {
                Play();
                if (dataRepo.DataH4.ContainsKey(tickCur))
                {
                    barH4.AddFirst(dataRepo.DataH4[tickCur]);
                    if (orderOpened && !orderClosed)
                    {
                        double bid = barH4.First.Value.OpenBid;
                        if (bid > open)
                        {
                            profit += bid - open;
                            total += total * profit;
                            Console.WriteLine(total);
                            winNum++;
                            //Console.WriteLine("Timeout with profit at {0}.", tickCur);
                        } else if(bid < open)
                        {
                            loss += open - bid;
                            total -= total * loss;
                            Console.WriteLine(total);
                            loseNum++;
                            //Console.WriteLine("Timeout with loss at {0}.", tickCur);
                        }
                        orderOpened = false;
                        orderClosed = true;
                    }
                    if (h4Cnt++ >= 25)
                    {
                        double high = getLastHigh(6);
                        double low = getLastLow(6);
                        if (high > low)
                        {
                            R = high - low;
                            R2 = getLastHigh(12) - getLastLow(12);
                            if (R2 >= 0.013 && R2 <= 0.019)
                            {
                                open = barH4.First.Value.OpenAsk - R * 0.12;
                                sl = -0.005;
                                tp = R * 0.12;
                                orderOpened = false;
                                orderClosed = false;

                                //Console.WriteLine("At {0}, put order with limit price {1}, stoploss {2}, take profit {3}.", tickCur, open, sl, tp);
                            }
                        }
                    }
                }
                if(dataRepo.DataM1.ContainsKey(tickCur))
                {
                    barM1.AddFirst(dataRepo.DataM1[tickCur]);
                    if (orderClosed) continue;
                    FxHistoricalDataEntry cur = barM1.First.Value;
                    if(!orderOpened)
                    {
                        if ((cur.LowAsk < open) && cur.HighAsk > open)
                        {
                            orderOpened = true;
                            //Console.WriteLine("\nOrder placed at {0}.", tickCur);
                        }
                    }
                    if(orderOpened)
                    {
                        if (cur.LowBid < open + sl && cur.HighBid > open + sl)
                        {
                            loss += sl;
                            total -= total * loss;
                            //Console.WriteLine("Stop loss at {0}.", tickCur);
                            Console.WriteLine(total);
                            orderClosed = true;
                            loseNum++;
                        } else if (cur.LowBid < open + tp && cur.HighBid > open + tp)
                        {
                            profit += tp;
                            total += total * profit;
                            //Console.WriteLine("Take profit at {0}.", tickCur);
                            Console.WriteLine(total);
                            orderClosed = true;
                            winNum++;
                        }
                    }
                }
            }
            Console.WriteLine("win {0}, {1}\nlose {2},{3}", winNum, profit, loseNum, loss);
            Console.WriteLine(total);
        }

        private double getLastHigh(int n)
        {
            double res = -1;
            LinkedListNode<FxHistoricalDataEntry> node = barH4.First;
            node = node.Next;
            while (n-- > 0)
            {
                res = Math.Max(res, node.Value.HighAsk);
                if (node.Next == null) break;
                node = node.Next;
            }
            return res;
        }

        private double getLastLow(int n)
        {
            double res = 100;
            LinkedListNode<FxHistoricalDataEntry> node = barH4.First;
            node = node.Next;
            while (n-- > 0)
            {
                res = Math.Min(res, node.Value.LowAsk);
                if (node.Next == null) break;
                node = node.Next;
            }
            return res;
        }

        protected void Play()
        {
            tickCur = tickCur.AddMinutes(1);
        }
    }
}
