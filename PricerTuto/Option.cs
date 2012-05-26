using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class Option : IProduct
    {
        public string UnderlyingName { get; set; }

        public OptionType OptionType { get; set; }

        public double Spot { get; set; }

        public DateTime Maturity { get; set; }

        public double Strike { get; set; }

        public double Volatility { get; set; }

        public double InterestRate { get; set; }

        public double Price { get; set; }

        public double Delta { get; set; }

        public double Gamma { get; set; }

        public double PayOff { get; set; }
    }
}
