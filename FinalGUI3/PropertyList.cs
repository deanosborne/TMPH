using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalGUI3
{
    public class PropertyList
    {
        public string ListingID { get; set; }

        public string PAddress { get; set; }

        public string Sellprice { get; set; }

        public PropertyList(string ListingID, string PAddress, string Sellprice)
        {
            this.ListingID = ListingID;
            this.PAddress = PAddress;
            this.Sellprice = Sellprice;
        }

    }

    public class SalesList
    {
        public string Price { get; set; }
        public string SAddress { get; set; }

        public SalesList(string Price, string SAddress)
        {
            this.Price = Price;
            this.SAddress = SAddress;
        }
    }

    public class RatesList
    {
        public string Rates { get; set; }

        public string LV { get; set; }

        public string IV { get; set; }

        public string RV { get; set; }

        public RatesList(string Rates, string LV, string IV, string RV)
        {
            this.Rates = Rates;
            this.LV = LV;
            this.IV = IV;
            this.RV = RV;
        }
    }


}
