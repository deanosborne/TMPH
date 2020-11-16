using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalGUI3
{
    public class FullList
    {
        public FullList()
        {
            lstSchool = new List<string>();
        }
        public PropertyList PropertyList { get; set; }
        public RatesList RatesList { get; set; }
        public SalesList SalesList { get; set; }
        public List<string> lstSchool { get; set; }

        private string ListingID;
        public string listingID
        {
            get { return ListingID; }
            set { ListingID=value; }
        }

        public string PAddress { get; set; }

        public string Sellprice { get; set; }

        public string Price { get; set; }
        public string SAddress { get; set; }
        public string Rates { get; set; }

        public string LV { get; set; }

        public string IV { get; set; }

        public string RV { get; set; }


        public FullList(string ListingID, string PAddress, string Sellprice, string Rates, string LV, string IV, string RV, List<string>lstSchool)
        {
            this.ListingID = ListingID;
            this.PAddress = PAddress;
            this.Sellprice = Sellprice;
            this.Rates = Rates;
            this.LV = LV;
            this.IV = IV;
            this.RV = RV;
            this.lstSchool = lstSchool;
        }


        public virtual string PrintF()
        {
            string fullinfo = "";
            fullinfo += ListingID + "\r\n";
            fullinfo += "Address: " + PAddress + "\r\n";
            fullinfo += "Selling for: " + Sellprice + "\r\n";
            fullinfo += "\r\n";
            fullinfo += "Rates: " + Rates + "\r\n";
            fullinfo += "Land value: " + LV + "\r\n";
            fullinfo += "Improvement value: " + IV + "\r\n";
            fullinfo += "Capital value: " + RV + "\r\n";
            fullinfo += "\r\n";
            fullinfo += string.Join(Environment.NewLine, lstSchool);
            return fullinfo;
        }


    }
}
