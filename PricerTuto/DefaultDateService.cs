using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class DefaultDateService : IDateService
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }
}
