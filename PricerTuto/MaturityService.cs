using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class MaturityService
    {
        public DateTime GetNextMaturityDay(DateTime from)
        {
            return GetMaturityDay(from, new Maturity { Offset = 0, Type = MaturityType.Month });
        }

        /// <summary>
        /// http://fr.wikipedia.org/wiki/Jour_des_quatre_sorci%C3%A8res
        /// </summary>
        /// <param name="from"></param>
        /// <param name="maturity"></param>
        /// <returns></returns>
        public DateTime GetMaturityDay(DateTime from, Maturity maturity)
        {            
            DateTime nextDate = from;
            if (maturity.Type == MaturityType.Month)
            {
                nextDate = from.AddMonths(maturity.Offset);
            }
            else if (maturity.Type == MaturityType.Year)
            {
                nextDate = from.AddYears(maturity.Offset);
            }

            DateTime maturityDay = GetThirdFriday(new DateTime(nextDate.Year, 3, 1));

            while (nextDate > maturityDay)
            {
                maturityDay = GetThirdFriday(maturityDay.AddMonths(3));
            }

            return maturityDay;
        }

        private DateTime GetThirdFriday(DateTime day)
        {
            DateTime found = new DateTime(day.Year, day.Month, 1);
            int cptFriday = 0;
            
            while (cptFriday < 3)
            {
                if (found.DayOfWeek == DayOfWeek.Friday)
                {
                    cptFriday++;
                }

                if (cptFriday == 3)
                {
                    return found;
                }

                found = found.AddDays(1);
            }

            return found;
        }
    }
}
