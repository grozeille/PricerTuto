using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public interface IProduct
    {
        double Price { get; set; }

        double Delta { get; set; }

        double Gamma { get; set; }

        double PayOff { get; set; }
    }
}
