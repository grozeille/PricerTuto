using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class HistoricalData
    {
        public DateTime Date { get; set; }

        public double Close { get; set; }

        public double Volatility { get; set; }
    }
}
