/// Property search GUI
/// Dean Osborne-Andrews 92020193
/// 100% working: 
/// 1.Find christchurch property from trademe
/// 2. Copy and paste link to URL box
/// 3. Press scrape, wait for results
/// 4. Either press Save under result box or move to property tab
/// 5. End
/// Manual search somewhat works but can cause errors

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Data.SqlClient;
using System.Collections;
using System.Reflection;

namespace FinalGUI3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //Creates vertical tabcontrol menu
            var g = e.Graphics;
            var text = this.tabControl1.TabPages[e.Index].Text;
            var sizeText = g.MeasureString(text, this.tabControl1.Font);

            var x = e.Bounds.Left + 3;
            var y = e.Bounds.Top + (e.Bounds.Height - sizeText.Height) / 2;

            g.DrawString(text, this.tabControl1.Font, Brushes.Black, x, y);
        }

        //Print values to resultbox, called from FullList
        public void ShowFull(FullList Print)
        {
            ResultBox.Text += Print.PrintF();
        }

        //This is for specific searching of addresses where letters are involved
        public static int NumFromLetter(char ch)
        {
            if (ch >= 'A' && ch <= 'Z') ch -= (char)('A' - 1);
            else
                if (ch >= 'a' && ch <= 'z')
            {
                ch -= (char)('a' - 1);
                int n = ch;
                ch = (char)(n + 26);
            }
            else ch = '\0'; // all other characters use binary zero
            return (int)ch;
        }


        private void scrapeButton_Click(object sender, EventArgs e)
        {

            #region Var declarations
            string rNumber = "";
            string rStreet = "";
            string number = "";
            string name = "";
            string kappa3 = "";
            string kappa4 = "";
            string Rates = "";
            string PAddress = "";
            string LV = "";
            string IV = "";
            string RV = "";
            number += NumberBox.Text.ToUpper();
            name += AddressBox.Text;
            #endregion

            #region AgilityPack declaration
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlWeb hw = new HtmlWeb();

            bool valid;

            //Checking link is valid and (in this scope) only from Trademe else raise error
            if (urlBox.Text.StartsWith(@"https://www.trademe.co.nz/a/property/residential/sale/"))
            {
                doc = hw.Load(urlBox.Text);
                valid = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid Trademe link", "Error", MessageBoxButtons.OK);
                valid = false;

            }

            if (valid == true)
            {
                if (cityBox.SelectedIndex == 0)
                {
                    #endregion

                    #region AgilityPack logic
                    var ProductsHtml = doc.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("tm-property-listing-body__container")).ToList();

                    var ProductListItems = ProductsHtml[0].Descendants("section")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Contains("l-container")).ToList();

                    //Find Listing ID
                    var ListingID = doc.DocumentNode.SelectSingleNode("//div[@class='tm-property-listing__listing-metadata-listingid']").InnerText.TrimStart();

                    //Find address
                    try
                    {
                        PAddress = doc.DocumentNode.SelectSingleNode("//h1[@class='tm-property-listing-body__location p-h3']").InnerText.TrimStart().ToString();
                    } catch (Exception ex)
                    {

                    }

                    //Find sellprice
                    var Sellprice = doc.DocumentNode.SelectSingleNode("//h2/strong[1]").InnerText.TrimStart();
                    #endregion

                    #region AgilityPack results
                    var aSplit = PAddress.Split(' '); //To select specific strings
                    string aNumber = aSplit[0]; //Get street number
                    string aSuburb = aSplit[3]; //Get suburb
                    string aStreet = aSplit[1] + " " + aSplit[2];//Get street name

                    rNumber += aNumber;
                    rStreet += aStreet;
                    NumberBox.Text = rNumber; //To show user street number
                    AddressBox.Text = rStreet; //To show street address
                    number += NumberBox.Text.ToUpper();
                    name += AddressBox.Text;

                    int index = number.IndexOf("/"); //helper for below
                    string index2 = number.Substring(index + 1); //helper for below
                    string index3 = Regex.Replace(index2, "[^0-9]", ""); //removes alphabet
                    string index4 = number.Substring(0, 1); //gets first char in string

                    var digits = new[] { '0' };

                    foreach (char c in number)
                    {

                        kappa3 += NumFromLetter(c);
                        kappa4 += kappa3.Trim('0');
                    }

                    string unitindex = "Unit " + index4 + ","; // unit if address like 1/
                    string flatindex = "Flat " + index4 + ",";// flat if ""
                    string unitk4 = "Unit " + kappa4 + ","; // unit if address like 123a
                    string flatk4 = "Flat " + kappa4 + ",";// flat if ""
                    string slashindex = kappa4 + "/" + index3;

                    Regex.IsMatch(number, @"^[a-zA-Z]+$");

                    string trimcomma = name.Trim(',');
                    #endregion

                    #region Selenium declaration
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArguments("headless");
                    #endregion
                    var chromeDriverService = ChromeDriverService.CreateDefaultService();
                    chromeDriverService.HideCommandPromptWindow = true;
                    #region Selenium logic
                    var browser = new ChromeDriver(chromeDriverService, chromeOptions);

                    //Rates starts here
                    browser.Navigate().GoToUrl("https://ccc.govt.nz/services/rates-and-valuations/rates-and-valuation-search");
                    Thread.Sleep(2000);
                    browser.SwitchTo().Frame(0);

                    //Normal search
                    var search = browser.FindElement(By.CssSelector("#ucAddressSearch_txtStreetNumberName"));
                    search.Click();
                    search.SendKeys(number + " " + trimcomma);
                    var find = browser.FindElement(By.CssSelector("#ucAddressSearch_txtStreetNumberName"));
                    find.Click();

                    Thread.Sleep(3000);
                    if (number.Any(Char.IsLetter))
                    {
                        var findresult1 = browser.FindElement(By.XPath("//*[text()[contains(., '" + number + "')]] | //*[text()[contains(., '" + unitk4 + "')]] | //*[text()[contains(., '" + flatk4 + "')]]"));
                        findresult1.Click();
                    }
                    else
                    {
                        var findresult2 = browser.FindElement(By.XPath("//*[text()[contains(., '" + number + "')]] | //*[text()[contains(., '" + unitindex + "')]] | //*[text()[contains(., '" + flatindex + "')]]"));
                        findresult2.Click();
                    }
                    Thread.Sleep(3000);
                    try
                    {
                        Rates = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropRatesTotAmd")).Text;
                        LV = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropLandValNext")).Text;
                        IV = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropImprValNext")).Text;
                        RV = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropCapValNext")).Text;
                    }
                    catch (NoSuchElementException)
                    {
                        DialogResult result = MessageBox.Show("Not found, try alternative search?", "Confirmation", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            scrapeButton.Enabled = false;
                            altSLabel.Enabled = true;
                            altSLabel.Visible = true;
                        }
                        else
                        {
                            scrapeButton.Enabled = false;
                            altSLabel.Enabled = true;
                            altSLabel.Visible = true;
                        }
                    }
                    Thread.Sleep(6000);
                    chromeOptions.AddArguments("headless");
                    #endregion
                    #region Selenium logic
                    //Homes starts here
                    var browser2 = new ChromeDriver(chromeDriverService, chromeOptions);
                    browser2.Navigate().GoToUrl("https://homes.co.nz/map/christchurch?lng=172.63949105013296&lat=-43.52800050205544&zoom=14&filter=sold");

                    Thread.Sleep(10000);
                    try
                    {
                        var search2 = browser2.FindElement(By.CssSelector("#autocomplete-search"));
                        search.Click();
                        search.SendKeys(PAddress);
                    }
                    catch (StaleElementReferenceException ex)
                    {
                        var search2 = browser2.FindElement(By.CssSelector("#autocomplete-search"));
                        search2.Click();
                        search2.SendKeys(PAddress);
                    }
                    Thread.Sleep(2000);
                    try
                    {
                        var findtext = browser2.FindElement(By.XPath("//div[text()[contains(., '" + number + "')]]"));
                        Thread.Sleep(1000);
                        findtext.Click();
                    }
                    catch (Exception ex)
                    {
                        var findtext = browser2.FindElement(By.XPath("//div[text()[contains(., '" + number + "')]]"));
                        Thread.Sleep(1000);
                        findtext.Click();
                    }
                    var button = browser2.FindElement(By.XPath("//span[@class='buttonContent']"));
                    button.Click();
                    IList<string> all = new List<string>();
                    IList<string> all1 = new List<string>();
                    Thread.Sleep(5000);

                    //Add price and address to list
                    foreach (var element in browser2.FindElements(By.XPath("//div/div[@class='priceContainer' and 1]/h3[@class='price' and 1]")))
                    {
                        all.Add(element.Text);
                    }
                    foreach (var element in browser2.FindElements(By.XPath("//div/div[@class='addressPrice' and 1]/h2[@class='address' and 1]")))
                    {
                        all1.Add(element.Text);

                    }
                    List<string> lstSchool = new List<string>();
                    Thread.Sleep(3000);

                    //Zip list
                    foreach (var nw in all.Zip(all1, Tuple.Create).ToList())
                    {
                        lstSchool.Add(nw.Item1 + " " + nw.Item2);
                    }

                    //Show list to resultbox
                    FullList Full = new FullList(ListingID, PAddress, Sellprice, Rates, LV, IV, RV, lstSchool);
                    ShowFull(Full);

                    //SQL Insert logic
                    string connString = FinalGUI3.Properties.Settings.Default.DB3ConnectionString;
                    SqlConnection sqlConn = new SqlConnection(connString);
                    string sql_Text = "INSERT INTO dbo.P (ListingID, PAddress, Sellprice, Rates, LV, IV, RV) VALUES (@ListingID, @PAddress, @Sellprice, @Rates, @LV, @IV, @RV);SELECT scope_identity();";
                    string sql_Text2 = "INSERT INTO dbo.R (SPrice, PropertyID) VALUES (@SPrice, @id)";
                    SqlCommand cmd = new SqlCommand(sql_Text, sqlConn);
                    SqlCommand cmd2 = new SqlCommand(sql_Text2, sqlConn);
                    sqlConn.Open();
                    cmd.Parameters.AddWithValue("@ListingID", ListingID);
                    cmd.Parameters.AddWithValue("@PAddress", PAddress);
                    cmd.Parameters.AddWithValue("@Sellprice", Sellprice);
                    cmd.Parameters.AddWithValue("@Rates", Rates);
                    cmd.Parameters.AddWithValue("@LV", LV);
                    cmd.Parameters.AddWithValue("@IV", IV);
                    cmd.Parameters.AddWithValue("@RV", RV);
                    decimal decimalBrandId = (decimal)cmd.ExecuteScalar();
                    int NewBrandId = (int)decimalBrandId;
                    foreach (var nw in all.Zip(all1, Tuple.Create).ToList())
                    {
                        cmd2.Parameters.Clear();
                        cmd2.Parameters.AddWithValue("@id", NewBrandId);
                        cmd2.Parameters.AddWithValue("@SPrice", (nw.Item1 + " " + nw.Item2 + "#"));
                        cmd2.ExecuteNonQuery();
                    }
                    cmd.ExecuteNonQuery();
                    sqlConn.Close();

                    //Create dynamic textbox
                    TextBox at = new TextBox();
                    at.Multiline = true;
                    at.ReadOnly = true;
                    const int x_margin = 0;
                    const int y_margin = 2;
                    at.Text = ListingID.ToString() + "\r\n" + PAddress.ToString() + "\r\n" + Sellprice.ToString()
                        + "\r\n" + Rates.ToString() + "\r\n" + LV.ToString() + "\r\n" + IV.ToString() + "\r\n" + RV.ToString() + string.Join(Environment.NewLine, lstSchool);
                    Size size = TextRenderer.MeasureText(at.Text, at.Font);
                    at.ClientSize =
                    new Size(size.Width + x_margin, size.Height + y_margin);
                    flowLayoutPanel1.Controls.Add(at);


                    tabControl1.SelectedIndex = 1;
                    browser.Close();
                    browser.Quit();

                }
                else
                {
                    MessageBox.Show("Please select Christchurch", "Error", MessageBoxButtons.OK);
                }
            }
        }
        #endregion



        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            #region Var declarations
            string result2 = "";
            string result3 = "";
            string number = "";
            string name = "";
            string kappa3 = "";
            string kappa4 = "";
            string Rates = "";
            string LV = "";
            string IV = "";
            string RV = "";
            number += NumberBox.Text.ToUpper();
            name += AddressBox.Text;
            #endregion

            #region AgilityPack declaration
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlWeb hw = new HtmlWeb();

            doc = hw.Load(urlBox.Text);
            #endregion

            #region AgilityPack logic
            var ProductsHtml = doc.DocumentNode.Descendants("div")
            .Where(node => node.GetAttributeValue("class", "")
            .Equals("tm-property-listing-body__container")).ToList();

            var ProductListItems = ProductsHtml[0].Descendants("section")
            .Where(node => node.GetAttributeValue("class", "")
            .Contains("l-container")).ToList();

            var ListingID = doc.DocumentNode.SelectSingleNode("//div[@class='tm-property-listing__listing-metadata-listingid']").InnerText.TrimStart();

            var PAddress = doc.DocumentNode.SelectSingleNode("//h1[@class='tm-property-listing-body__location p-h3 ng-star-inserted']").InnerText.TrimStart();

            var Sellprice = doc.DocumentNode.SelectSingleNode("//h2/strong[1]").InnerText.TrimStart();
            #endregion

            #region AgilityPack results
            var kappa = PAddress.Split(' ');
            string a = kappa[0]; //Get street number
            string d = kappa[3];
            string b = kappa[1] + " " + kappa[2];//Get street name
            result2 += a;
            result3 += b;
            NumberBox.Text = result2;
            AddressBox.Text = result3;

            number += NumberBox.Text.ToUpper();
            name += AddressBox.Text;
            int index = number.IndexOf("/"); //helper for below
            string index2 = number.Substring(index + 1); //helper for below
            string index3 = Regex.Replace(index2, "[^0-9]", ""); //removes alphabet
            string index4 = number.Substring(0, 1); //gets first char in string

            var digits = new[] { '0' };

            foreach (char c in number)
            {

                kappa3 += NumFromLetter(c);
                kappa4 += kappa3.Trim('0');
            }

            string unitindex = "Unit " + index4 + ","; // unit if address like 1/
            string flatindex = "Flat " + index4 + ",";// flat if ""
            string unitk4 = "Unit " + kappa4 + ","; // unit if address like 123a
            string flatk4 = "Flat " + kappa4 + ",";// flat if ""
            string slashindex = kappa4 + "/" + index3;

            Regex.IsMatch(number, @"^[a-zA-Z]+$");

            string trimcomma = name.Trim(',');
            #endregion

            #region Selenium declaration
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            //chromeOptions.AddArguments("--start-maximized");
            #endregion

            #region Selenium logic
            var browser = new ChromeDriver(chromeOptions);
            browser.Navigate().GoToUrl("https://ccc.govt.nz/services/rates-and-valuations/rates-and-valuation-search");
            Thread.Sleep(2000);
            browser.SwitchTo().Frame(0);

            //Alternative search
            var searchField = browser.FindElement(By.CssSelector("#ucAddressSearch_lbFullSearch"));
            searchField.Click();
            var searchField2 = browser.FindElement(By.CssSelector("#ucAddressSearch_txtStartStreetNo"));
            searchField2.Click();
            searchField2.SendKeys(index3);
            var searchField3 = browser.FindElement(By.CssSelector("#ucAddressSearch_txtToStreetNo"));
            searchField3.Click();
            searchField3.SendKeys(index3);
            var searchField4 = browser.FindElement(By.CssSelector("#ucAddressSearch_txtStreetName"));
            searchField4.Click();
            searchField4.SendKeys(trimcomma);
            var searchField6 = browser.FindElement(By.CssSelector("#ucAddressSearch_btnSearch"));
            searchField6.Click();

            Thread.Sleep(3000);
            if (number.Any(Char.IsLetter))
            {
                try
                {
                    var findresult1 = browser.FindElement(By.XPath("//*[text()[contains(., '" + number + "')]] | //*[text()[contains(., '" + unitk4 + "')]] | //*[text()[contains(., '" + flatk4 + "')]]"));
                    findresult1.Click();

                }
                catch (NoSuchElementException)
                {
                }
            }
            else
            {
                try
                {
                    var findresult2 = browser.FindElement(By.XPath("//*[text()[contains(., '" + number + "')]] | //*[text()[contains(., '" + unitk4 + "')]] | //*[text()[contains(., '" + flatk4 + "')]] | //*[text()[contains(., '" + slashindex + "')]]"));
                    findresult2.Click();
                }
                catch (NoSuchElementException)
                {
                }
            }
            Thread.Sleep(5000);
            Rates = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropRatesTotAmd")).Text;
            LV = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropLandValNext")).Text;
            IV = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropImprValNext")).Text;
            RV = browser.FindElement(By.CssSelector("#ucPropertyInformation_tcPropertyView_tpProperty_ucProperties_lblPropCapValNext")).Text;



            browser.Navigate().GoToUrl("https://homes.co.nz/map/christchurch?lng=172.63949105013296&lat=-43.52800050205544&zoom=14&filter=sold");
            Thread.Sleep(4000);

            //Normal search
            try
            {
                var search2 = browser.FindElement(By.CssSelector("#autocomplete-search"));
                search2.Click();
                search2.SendKeys(number + " " + trimcomma);
            }
            catch (StaleElementReferenceException ex)
            {
                var search2 = browser.FindElement(By.CssSelector("#autocomplete-search"));
                search2.Click();
                search2.SendKeys(number + " " + trimcomma);
            }
            Thread.Sleep(2000);
            var findtext = browser.FindElement(By.XPath("//div[text()[contains(., '" + number + "')]]"));
            var find2 = findtext.FindElement(By.XPath("//div[text()[contains(., '" + d + "')]]"));
            Thread.Sleep(2000);
            var button = browser.FindElement(By.XPath("//span[@class='buttonContent']"));
            button.Click();
            Thread.Sleep(2000);
            IList<string> all = new List<string>();
            IList<string> all1 = new List<string>();
            foreach (var element in browser.FindElements(By.XPath("//div/div[@class='priceContainer' and 1]/h3[@class='price' and 1]")))
            {
                all.Add(element.Text);
            }
            foreach (var element in browser.FindElements(By.XPath("//div/div[@class='addressPrice' and 1]/h2[@class='address' and 1]")))
            {
                all1.Add(element.Text);

            }
            List<string> lstSchool = new List<string>();


            foreach (var nw in all.Zip(all1, Tuple.Create).ToList())
            {
                lstSchool.Add(nw.Item1 + " " + nw.Item2);
            }

            FullList Full = new FullList(ListingID, PAddress, Sellprice, Rates, LV, IV, RV, lstSchool);
            ShowFull(Full);
            tabControl1.SelectedIndex = 1;
            browser.Close();
            browser.Quit();

            string connString = FinalGUI3.Properties.Settings.Default.DB3ConnectionString;
            SqlConnection sqlConn = new SqlConnection(connString);
            string sql_Text = "INSERT INTO dbo.P (ListingID, PAddress, Sellprice, Rates, LV, IV, RV) VALUES (@ListingID, @PAddress, @Sellprice, @Rates, @LV, @IV, @RV);SELECT scope_identity();";
            string sql_Text2 = "INSERT INTO dbo.R (SPrice, PropertyID) VALUES (@SPrice, @id)";
            SqlCommand cmd = new SqlCommand(sql_Text, sqlConn);
            SqlCommand cmd2 = new SqlCommand(sql_Text2, sqlConn);
            sqlConn.Open();
            cmd.Parameters.AddWithValue("@ListingID", ListingID);
            cmd.Parameters.AddWithValue("@PAddress", PAddress);
            cmd.Parameters.AddWithValue("@Sellprice", Sellprice);
            cmd.Parameters.AddWithValue("@Rates", Rates);
            cmd.Parameters.AddWithValue("@LV", LV);
            cmd.Parameters.AddWithValue("@IV", IV);
            cmd.Parameters.AddWithValue("@RV", RV);
            decimal decimalBrandId = (decimal)cmd.ExecuteScalar();
            int NewBrandId = (int)decimalBrandId;
            foreach (var nw in all.Zip(all1, Tuple.Create).ToList())
            {
                cmd2.Parameters.Clear();
                cmd2.Parameters.AddWithValue("@id", NewBrandId);
                cmd2.Parameters.AddWithValue("@SPrice", (nw.Item1 + " " + nw.Item2 + "#"));
                cmd2.ExecuteNonQuery();
            }
            //cmd.ExecuteNonQuery();
            sqlConn.Close();
            TextBox at = new TextBox();
            at.Multiline = true;
            at.ReadOnly = true;
            const int x_margin = 0;
            const int y_margin = 2;
            at.Text = ListingID.ToString() + "\r\n" + PAddress.ToString() + "\r\n" + Sellprice.ToString()
                + "\r\n" + Rates.ToString() + "\r\n" + LV.ToString() + "\r\n" + IV.ToString() + "\r\n" + RV.ToString() + string.Join(Environment.NewLine, lstSchool);
            Size size = TextRenderer.MeasureText(at.Text, at.Font);
            at.ClientSize =
            new Size(size.Width + x_margin, size.Height + y_margin);
            flowLayoutPanel1.Controls.Add(at);

            cmd.ExecuteNonQuery();
            sqlConn.Close();


            tabControl1.SelectedIndex = 1;
            browser.Close();
            browser.Quit();

        }




        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "Text (*.txt)|*.txt";
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var sw = new StreamWriter(saveFile.FileName, false))
                    sw.Write(ResultBox.Text);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                NumberBox.Visible = true;
                AddressBox.Visible = true;
                urlBox.Visible = false;
                label5.Visible = true;
                label4.Visible = true;
                label1.Visible = false;
            }
            else
            {
                NumberBox.Visible = false;
                AddressBox.Visible = false;
                urlBox.Visible = true;
                label5.Visible = false;
                label4.Visible = false;
                label1.Visible = true;
            }
        }

        private void refreshForm()
        {
            //Handler for loading from database
            string connString = FinalGUI3.Properties.Settings.Default.DB3ConnectionString;
            SqlConnection sqlConn = new SqlConnection(connString);
            SqlDataAdapter b = new SqlDataAdapter("SELECT PropertyID, ListingID, PAddress, Sellprice, Rates, LV, IV, RV, SPrice = " +
                "STUFF((SELECT DISTINCT ', ' + SPrice FROM dbo.R WHERE r.PropertyID = p.PropertyID FOR XML PATH('')), 1, 2, '') " +
                "FROM dbo.P GROUP BY PropertyID, ListingID, PAddress, Sellprice, Rates, LV, IV, RV", sqlConn);
            //Selects and groups from 2 databases
            DataSet ds = new DataSet();
            b.Fill(ds);
            sqlConn.Open();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                //Helper for formatting
                string dr8 = dr[8].ToString();
                string[] values = dr8.Split('#');
                string dr8join = string.Join(Environment.NewLine, values).Replace(",", "");

                //Dynamic control adding
                TextBox a = new TextBox();
                a.Multiline = true;
                a.ReadOnly = true;
                const int x_margin = 0;
                const int y_margin = 2;
                a.Text = dr[1].ToString() + "\r\n" + "Address:" + dr[2].ToString()
                    + "\r\n" + "Sell price:" + dr[3].ToString() + "\r\n" + "Rates:" + dr[4].ToString() + "\r\n" + "LV:" + dr[5].ToString()
                    + "\r\n" + "IV:" + dr[6].ToString() + "\r\n" + "RV:" + dr[7].ToString() + "\r\n" + "\r\n" + dr8join;
                Size size = TextRenderer.MeasureText(a.Text, a.Font);
                a.ClientSize =
                new Size(size.Width + x_margin, size.Height + y_margin);
                flowLayoutPanel1.Controls.Add(a);
            }


            sqlConn.Close();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            //Selects database data on load
            refreshForm();
            cityBox.SelectedIndex = 0;
        }

        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ResultBox.AppendText("It works!");
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            ResultBox.Text = String.Empty;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            refreshForm();
        }
    }
}