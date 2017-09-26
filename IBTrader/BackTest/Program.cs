using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using IBTrader;
using System.Globalization;
using System.IO;

namespace BackTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BackTestEngine engine = new BackTestEngine();
            double pnl = engine.Start(0.3, 0.74);
            return;
            double max = 0.0;
            double ma = -1, mb = -1;
            string row = string.Empty;
            string filename = IBTrader.Constants.BaseDir + "matrix_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            for (double a = 0.1; a < 0.7; a += .04)
            {
                for (double b = 0.1; b < 0.8; b += .04)
                {
                    double r = engine.Start(a, b);
                    if (r > max)
                    {
                        max = r;
                        ma = a;
                        mb = b;
                    }
                    Console.WriteLine("{0}, {1}: {2}", a, b, r.ToString("C", CultureInfo.CurrentCulture));
                    if (string.IsNullOrEmpty(row)) row = r.ToString();
                    else row = row + "," + Math.Round(r, 2).ToString();
                }
                File.AppendAllText(filename, row + "\n");
                row = string.Empty;
            }
            Console.WriteLine("\nMax revenue: {0}, {1}: {2}", ma, mb, max);
            //Console.WriteLine(pnl);
            //OldEngine oldEngine = new OldEngine();
            //oldEngine.Start();
        }
    }
}
