using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using IBTrader;
using BackTest;

namespace DrawPNL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += new EventHandler(Form1_Load);
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {

            BackTestEngine engine = new BackTestEngine();
            double pnl = engine.Start(0.11, 0.12, true);
            var series = chart1.Series.ElementAt(0);// new Series("PnL");
            series.ChartType = SeriesChartType.Line;
            List<int> x = new List<int>();
            List<double> y = new List<double>();
            for(int k = 0; k < engine.pnlList.Count; k++)
            {
                x.Add(k);
                y.Add(engine.pnlList.Values.ElementAt(k));

            }
            series.Points.DataBindXY(x, y);
            //chart1.Series.RemoveAt(0);
            //chart1.Series.Add(series);
        }
    }
}
