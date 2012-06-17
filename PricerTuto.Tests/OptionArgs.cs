using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PricerTuto;

namespace PricerTuto.Tests
{
    public class OptionArgs
    {
        public OptionType OptionType { get; set; }

        public string Underlying { get; set; }

        public double Spot { get; set; }

        public double Strike { get; set; }

        public DateTime Maturity { get; set; }

        public double Rate { get; set; }

        public double Volatility { get; set; }
    }
}
