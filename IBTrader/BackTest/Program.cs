﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using IBTrader;

namespace BackTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BackTestEngine backTestEngine = new BackTestEngine();
            backTestEngine.Start();
        }
    }
}