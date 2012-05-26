using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class MarketOption : Option
    {
        public double Bid { get; set; }

        public double Ask { get; set; }
    }
}
