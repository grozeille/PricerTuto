using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public interface IDateService
    {
        DateTime Now { get; }
    }
}
