using Food_Inventory.Properties;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;


namespace Food_Inventory.UCControls
{
    public partial class UCNewOrder : UserControl
    {
        // database
        private DBAccess database;
        // variables 
        private float grandTotal;
        Dictionary<string, List<string>> masterList;
        private int printerHeight;

        public UCNewOrder()
        {
            InitializeComponent();
        }

        private void UCNewOrder_Load(object sender, EventArgs e)
        {
            // database related code
            database = new DBAccess(DashboardForm.dataSourcePath);

            // initialize food list
            masterList = new Dictionary<string, List<string>>();
            // get category names
            var categoryNames = getCategoryNames();


            readToMasterList(categoryNames);
            //// set foods list
            //System.Collections.Specialized.StringCollection 
            //    foods = (System.Collections.Specialized.StringCollection)Settings.Default["foodsList"];
            //setFoodList(foods);

            // assing values to variables
            tbGrandTotal.Text = "0";
            grandTotal = 0.0f;

            nudQuantity.Value = 1;
            tbPrice.Text = "0";
            printerHeight = 600;

        }

        // -------------------- Events ---------------
        private void lbFoods_MouseClick(object sender, MouseEventArgs e)
        {
            if (lbFoods.SelectedIndex < 0 || (lbFoods.SelectedItem.ToString().Contains("<") && lbFoods.SelectedItem.ToString().Contains(">")))
            {
                tbPrice.Text = "0";
                return;
            }
            var foodPrice = lbFoods.SelectedItem.ToString();
            tbPrice.Text = foodPrice.Substring(foodPrice.IndexOf("/") + 1);

            nudQuantity.Value = 1;
            calculateTotal();
        }
        private void nudQuantity_ValueChanged(object sender, EventArgs e)
        {
            calculateTotal();
        }

        private void tbPrice_KeyDown(object sender, KeyEventArgs e)
        {
            calculateTotal();
        }
        private void tbPrice_TextChanged(object sender, EventArgs e)
        {
            calculateTotal();
        }

        private void btnAddOrder_Click(object sender, EventArgs e)
        {

            // if custom was not checked
            if (lbFoods.SelectedIndex < 0 || (lbFoods.SelectedItem.ToString().Contains("<") && lbFoods.SelectedItem.ToString().Contains(">")))
            {
                MessageBox.Show("Select a valid food please", "Invalid row selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (nudQuantity.Value < 1)
            {
                MessageBox.Show("Please enter the quantity","Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;

            }
            if (lblError.Text.Length > 0 || string.IsNullOrEmpty(tbPrice.Text.Trim()))
            {
                MessageBox.Show("Error!\nPlease enter a valid food price", "Invalid Food Price", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
          

         

            // initialize variables
            bool foodFound = false;
            var originalSelectedFoodIndex = lbFoods.SelectedIndex;
            var selectedFoodIndex = lbFoods.SelectedIndex;
            var selectedFood = lbFoods.SelectedItem.ToString();
            //var selectedFoodPrice = selectedFood.Substring(selectedFood.IndexOf("/") + 1).Trim();
            string selectedFoodPrice = tbPrice.Text.ToString();
            string selectedFoodCategory = "";

            // get selected food category
            selectedFoodCategory = getSelectedFoodCategory(selectedFoodIndex);

            // get clean category name
            string cleanCategoryName = getCleanCategoryName(selectedFoodCategory);

            lbFoods.SelectedIndex = originalSelectedFoodIndex;


            // update master list
            foreach (KeyValuePair<string, List<string>> kvp in masterList)
            {
                if (kvp.Key == cleanCategoryName)
                {

                    for (int n = 0; n < kvp.Value.Count; n++)
                    {
                        if ((selectedFood.Substring(0, selectedFood.IndexOf("-")).Trim() ==
                            kvp.Value[n].Substring(0, kvp.Value[n].IndexOf("-")).Trim()) && foodFound == false )
                        {

                            // old price
                            string oldPrice = kvp.Value[n].Substring(kvp.Value[n].IndexOf("/") + 1);
                            oldPrice = oldPrice.Substring(0, oldPrice.IndexOf("*"));

                            string oldQuantity = "";
                            float fooodPrice = float.Parse(selectedFoodPrice);
                            int newQuantity = 0;


                            try
                            {
                            
                                oldQuantity = kvp.Value[n].Substring(kvp.Value[n].IndexOf("*") + 1);

                                if (string.IsNullOrWhiteSpace(oldQuantity))
                                {
           
                                    oldQuantity = "0";
                                }

                                // updated price
                                float updatedPrice = getUpdatedPrice(oldPrice, oldQuantity, selectedFoodPrice, nudQuantity.Value.ToString());

                                if (updatedPrice == -1) return;

                                if (nudQuantity.Value > 0)
                                {
                                    newQuantity += int.Parse(nudQuantity.Value.ToString());
                                }


                                newQuantity += int.Parse(oldQuantity);
                                fooodPrice = fooodPrice * newQuantity;

                                // update category item
                                kvp.Value[n] = selectedFood.Substring(0, selectedFood.LastIndexOf('-')) + "/" + updatedPrice + "*" + newQuantity;
                                
                            }
                            catch (FormatException ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }

                            foodFound = true;

                        }
                        

                    }
                }


            }

         

            // adding selected food to ordered food list
            var foodName = selectedFood.Substring(0, selectedFood.IndexOf("-"));
            var foodPrice = tbPrice.Text;

           lbOrderdFoods.Items
              .Add(foodName + " ---- (" + nudQuantity.Value.ToString() + " * " + foodPrice + ")");
            

            try
            {
                grandTotal += float.Parse(foodPrice) * float.Parse(nudQuantity.Value.ToString());
                tbGrandTotal.Text = grandTotal.ToString();
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }

        // returns -1 if there was an error
        float getUpdatedPrice (string oldPrice, string oldQuantity, string newPrice,string newQuantity)
        {

            try
            {

                float oldP = float.Parse(oldPrice);
                float newP = float.Parse(newPrice);
                int newQ = int.Parse(newQuantity);
                float updatedPrice = 0;

                if (string.IsNullOrWhiteSpace(oldQuantity) || oldQuantity == "0")
                {
                    updatedPrice = (newP * newQ);
                }
                else
                {
                    updatedPrice = (newP * newQ) + oldP;
                }
                   
                return updatedPrice;

            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }

            return -1;
        }
        private void btnReset_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox
                .Show("Do you want to clear order list?", "Clear orders", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                lbOrderdFoods.Items.Clear();
                lbFoods.Items.Clear();
                tbGrandTotal.Text = "0";
                grandTotal = 0.0f;
                masterList.Clear();
                readToMasterList(getCategoryNames());
            }

        }

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox
               .Show("Do you want to place order?", "Place orders", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {

            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {

            // this line of code works perfectly for thermal printer
            // to convert hundreths of an inch to mm multiply it by 0.254

            int height = getPaperHeight();

            if (height > 600)
            {
                printerHeight = height + 20;
            }

            printDocument1.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Reciept", 300, printerHeight);
            printPreviewDialog1.Document = printDocument1;

            printPreviewDialog1.ShowDialog();

            DashboardForm.slipNumber += 1;

            Settings.Default["slipNumber"] = DashboardForm.slipNumber.ToString();
            Settings.Default.Save();

        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            drawInitialLines(e);
            getPrintText(e);
        }

        //--------------------------- KG gram feature -----------------------------

        // ------------------------------- Functions ----------------------------------
        private void readToMasterList(string[] categoryNames)
        {
            if (categoryNames.Length > 0)
            {
                try
                {
                    database.createConn();
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show("Can't establish connection\nTry Again\nTip: check access database path\n" + ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't establish connection\nTry Again\n" + ex.Message);
                    return;
                }

                for (int i = 0; i < categoryNames.Length - 1; i++)
                {
                    masterList.Add(categoryNames[i], new List<string>());
                    // Read each category items
                    try
                    {
                        DataTable dtc = new DataTable();

                        string q = "Select * From " + DashboardForm.accessTableName + " where categoryName = '" + categoryNames[i] + "';";
                        database.readDatathroughAdapter(q, dtc);

                        var categoryItems = "";
                        if (dtc.Rows.Count > 0)
                        {

                            lbFoods.Items.Add("\t<<<<<< " + categoryNames[i] + " >>>>>> ");
                            foreach (DataRow r in dtc.Rows)
                            {
                                categoryItems = r["categoryItems"].ToString();
                            }

                            if (string.IsNullOrEmpty(categoryItems.Trim()))
                            {
                                continue;
                            }

                            // splitting category items
                            string[] allCategoryItems = categoryItems.Split('\n');

                            Console.WriteLine(allCategoryItems.Length);
                            if (allCategoryItems.Length > 0)
                            {
                                List<string> tmpItemsList = new List<string>();
                                for (int j = 0; j < allCategoryItems.Length - 1; j++)
                                {
                                    lbFoods.Items.Add(allCategoryItems[j]);
                                    tmpItemsList.Add(allCategoryItems[j] + "*" + " ");

                                }
                                masterList[categoryNames[i]] = tmpItemsList;

                            }
                        }


                    }
                    catch (OleDbException ex)
                    {
                        MessageBox.Show(ex.Message);
                        database.closeConn();
                    }
                    catch (NullReferenceException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }


                }
                database.closeConn();
                lbFoods.SelectedIndex = 0;

            }
        }


        private void drawInitialLines(PrintPageEventArgs e)
        {

            Image i = Resources.PHOTO_2022_11_08_14_54_16__1_;
        

            e.Graphics.DrawImage(i, 1, 2, 298, 50);
            e.Graphics.DrawString("Date: " + DateTime.Now.ToShortDateString() + "\t\t Slip No: " + DashboardForm.slipNumber, new Font("Cambria", 9, FontStyle.Regular), Brushes.Black, new Point(2, 53));
            e.Graphics.DrawString("Customer name: " + tbCustomerName.Text.ToString(), new Font("Cambria", 9, FontStyle.Regular), Brushes.Black, new Point(2, 66));
            drawDashes(e, 2, 76);
            e.Graphics.DrawString("No|", new Font("Arial", 9, FontStyle.Bold), Brushes.Black, new Point(2, 84));
            e.Graphics.DrawString("Menu", new Font("Arial", 9, FontStyle.Bold), Brushes.Black, new Point(70, 84));
            e.Graphics.DrawString("|" , new Font("Arial", 9, FontStyle.Bold), Brushes.Black, new Point(160, 84));
            e.Graphics.DrawString(" KG |", new Font("Arial", 9, FontStyle.Bold), Brushes.Black, new Point(163, 84));
            e.Graphics.DrawString(" Gram |", new Font("Arial", 9, FontStyle.Bold), Brushes.Black, new Point(193, 84));
            e.Graphics.DrawString(" Price ", new Font("Arial", 9, FontStyle.Bold), Brushes.Black, new Point(236, 84));
            drawDashes(e, 2, 90);
            // price vertical lines
            drawVerticalLines(e, 234, 96);
            drawVerticalLines(e, 234, 106);
            drawVerticalLines(e, 234, 112);

            // gram vertical lines
            drawVerticalLines(e, 190, 96);
            drawVerticalLines(e, 190, 106);
            drawVerticalLines(e, 190, 112);

            // kg vertical lines
            drawVerticalLines(e, 160, 96);
            drawVerticalLines(e, 160, 106);
            drawVerticalLines(e, 160, 112);

            // kg vertical lines
            drawVerticalLines(e, 19, 96);
            drawVerticalLines(e, 19, 106);
            drawVerticalLines(e, 19, 112);
        }

        private int getPaperHeight()
        {

            int y = 100;

            foreach (KeyValuePair<string, List<string>> kvp in masterList)
            {
                y += 7;
                y += 9;

                for (int n = 0; n < kvp.Value.Count; n++)
                {

                    y += 7;
                    y += 9;

                }

            }
            y += 28;
            return y;
        }

        private void getPrintText(PrintPageEventArgs e)
        {
            
            string categoryName;
            string itemName;
            string itemPrice;
            string itemQuantity;
            
            List<string> categoryItems = new List<string>();
            char ch = 'A';
            int y = 100;

            foreach (KeyValuePair<string, List<string>> kvp in masterList)
            {
                categoryName = kvp.Key;

                e.Graphics.DrawString(ch + ": " + formatCategoryName(categoryName) + "\n", new Font("Arial", 8, FontStyle.Bold), Brushes.Black, new Point(2, y));
                
                drawVerticalLines(e, 234, y);
                drawVerticalLines(e, 190, y);
                drawVerticalLines(e, 160, y);
                drawVerticalLines(e, 19, y);

                y += 7;
                drawDashes(e, 2, y);
                drawVerticalLines(e, 234, y);
                drawVerticalLines(e, 190, y);
                drawVerticalLines(e, 160, y);
                drawVerticalLines(e, 19, y);
                y += 9;
                
                for (int n = 0; n < kvp.Value.Count; n++)
                {
                    itemName = kvp.Value[n].Substring(0, kvp.Value[n].IndexOf("-"));
                    itemName = formatFoodName(itemName);
                    itemPrice = kvp.Value[n].Substring(kvp.Value[n].IndexOf("/") + 1);
                    itemPrice = itemPrice.Substring(0, itemPrice.IndexOf("*"));
                    itemQuantity = kvp.Value[n].Substring(kvp.Value[n].IndexOf("*") + 1);

                    if (string.IsNullOrWhiteSpace(itemQuantity) || itemQuantity == "0")
                    {

                        e.Graphics.DrawString("  " + (n + 1) + "    " + itemName.Trim() + "\n", new Font("Microsoft Sans Serif", 8, FontStyle.Regular), Brushes.Black, new Point(2, y));
                        drawVerticalLines(e, 234, y);
                        drawVerticalLines(e, 190, y);
                        drawVerticalLines(e, 160, y);
                        drawVerticalLines(e, 19, y);
                        y += 7;
                        drawDashes(e, 2, y);
                        drawVerticalLines(e, 234, y);
                        drawVerticalLines(e, 190, y);
                        drawVerticalLines(e, 160, y);
                        drawVerticalLines(e, 19, y);
                        y += 9;
                    }
                    else
                    {

                        e.Graphics.DrawString("  " + (n + 1) + "    " + itemName.Trim() + " * " + itemQuantity , new Font("Microsoft Sans Serif", 8, FontStyle.Regular), Brushes.Black, new Point(2, y));
                        e.Graphics.DrawString(itemPrice , new Font("Microsoft Sans Serif", 8, FontStyle.Regular), Brushes.Black, new Point(240, y));
                        drawVerticalLines(e, 234, y);
                        drawVerticalLines(e, 190, y);
                        drawVerticalLines(e, 160, y);
                        drawVerticalLines(e, 19, y);
                        y += 7;
                        drawDashes(e, 2, y);
                        drawVerticalLines(e, 234, y);
                        drawVerticalLines(e, 190, y);
                        drawVerticalLines(e, 160, y);
                        drawVerticalLines(e, 19, y);
                        y += 9;
                    }

                }
                ch++;
            

            }
            // draw total
            e.Graphics.DrawString(" Total ", new Font("Microsoft Sans Serif", 8, FontStyle.Bold), Brushes.Black, new Point(192, y));
            e.Graphics.DrawString(grandTotal.ToString(), new Font("Microsoft Sans Serif", 8, FontStyle.Bold), Brushes.Black, new Point(237, y));
            drawVerticalLines(e, 234, y);
            drawVerticalLines(e, 234, y+3);
            drawVerticalLines(e, 190, y);
            drawVerticalLines(e, 190, y+3);
            drawDashes(e, 2, y + 9);

            // draw delivery phone number
            e.Graphics.DrawString("\tFor Delivery: " + DashboardForm.phoneNumber, new Font("Arial", 9, FontStyle.Bold), Brushes.Black, new Point(2, y+21));
            drawDashes(e, 2, y + 28);
            y += 28;

            if ((y+20 <= 650))
            {
                // draw software designer number
                e.Graphics.DrawString("                Software Designer whatsApp# (03362073245)", new Font("Microsoft Sans Serif", 7, FontStyle.Regular), Brushes.Black, new Point(2, y + 14));
            }

        }

        private void drawVerticalLines(PrintPageEventArgs e,int x, int y)
        {
            e.Graphics.DrawString("|", new Font("Microsoft Sans Serif", 8, FontStyle.Regular), Brushes.Black, new Point(x, y));
        }
        private void drawDashes(PrintPageEventArgs e, int x, int y)
        {
            e.Graphics.DrawString("--------------------------------------------------------------------------------", new Font("Microsoft Sans Serif", 8, FontStyle.Regular), Brushes.Black, new Point(x, y));
        }

        private string[] getCategoryNames()
        {

            string categoryNames = "";
            string[] allCategoryNames = { };
            try
            {
                database.createConn();
            }
            catch (OleDbException ex)
            {
                MessageBox.Show("Can't establish connection\nTry Again\nTip: check access database path\n" 
                    + ex.Message);
                return allCategoryNames;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't establish connection\nTry Again\n" + ex.Message);
                return allCategoryNames;
            }

            try
            {

                DataTable dtc = new DataTable();

                string q = "Select * From " + DashboardForm.accessTableName;
                database.readDatathroughAdapter(q, dtc);

                if (dtc.Rows.Count > 0)
                {

                    foreach (DataRow r in dtc.Rows)
                    {
                        categoryNames += r["categoryName"].ToString() + "\n" ;

                    }

                    // splitting category items
                    allCategoryNames = categoryNames.Split('\n');

                    
                }

            }
            catch (OleDbException ex)
            {
                MessageBox.Show(ex.Message);
                database.closeConn();
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            database.closeConn();

            return allCategoryNames;
        }

        private string getCleanCategoryName(string categoryName)
        {
            string cleanFormat = "";
            for (int i = 0; i < categoryName.Length; i++)
            {
                if (!(categoryName[i] == '<' || categoryName[i] == '>'))
                {
                    cleanFormat += categoryName[i];
                }
            }
            cleanFormat = cleanFormat.Trim();
            return cleanFormat;
        }
        private string getSelectedFoodCategory(int selectedFoodIndex)
        {
            string foodCategory = "";
            for (int i = 0; i < selectedFoodIndex; i++)
            {
                lbFoods.SelectedIndex = i;
                if ((lbFoods.SelectedItem.ToString().Contains("<") && lbFoods.SelectedItem.ToString().Contains(">")))
                {
                    foodCategory = lbFoods.SelectedItem.ToString();
                }
            }
            return foodCategory;
        }

        private void calculateTotal()
        {
            try
            {
                var foodPrice = tbPrice.Text.ToString();
                if (String.IsNullOrEmpty(tbPrice.Text))
                {
                    foodPrice = "0";
                }
                if (String.IsNullOrEmpty(nudQuantity.Value.ToString())) nudQuantity.Value = 0;

                var totalPrice = (float.Parse(nudQuantity.Value.ToString()) * float.Parse(foodPrice));
                tbTotalPrice.Text = totalPrice.ToString();

 
                lblError.Text = "";
            }
            catch(FormatException ex)
            {
                lblError.Text = "Error! Enter numbers";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
       
        }

        private string formatCategoryName(string categoryName)
        {
            string newString = "";

            for (int i = 0; i < categoryName.Length; i++)
            {
                if (i == 0) newString += categoryName[i].ToString().ToUpper();
                else newString += categoryName[i];
            }

            if (newString.Length > 0 && newString.Length <= 5)
            {
                newString = ("\t         (" + newString) + ")";
            }
            else if (categoryName.Length > 5 && newString.Length <= 10)
            {
                newString = ("\t       (" + newString) + ")";
            }
            else if (newString.Length > 10 && newString.Length <= 15)
            {
                if (newString.Length <= 12)
                    newString = ("\t (" + newString) + ")";
                else
                    newString = ("\t(" + newString) + ")";
            }
            else if (newString.Length > 15 && newString.Length <=20)
            {
                newString = "       (" + (newString.Substring(0, 15) + "...)");
            }
            else if (newString.Length > 20)
            {
                newString = "    (" + (newString.Substring(0, 19) + "...)");
            }

            return newString;
        }
        private string formatFoodName(string foodName)
        {

            if (foodName.Length > 20)
            {
                foodName = (foodName.Substring(0, 19) + "...");
            }

            return foodName;
        }

      
    }
}
