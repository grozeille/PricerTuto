using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class GoogleOptionData
    {
        public GoogleOptionItem[] calls { get; set; }

        public GoogleOptionItem[] puts { get; set; }

        public double underlying_price { get; set; }

        public GoogleDate expiry { get; set; }

        public GoogleDate[] expirations { get; set; }
    }
}
