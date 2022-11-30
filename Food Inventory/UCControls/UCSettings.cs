using System;
using System.Data;
using System.Windows.Forms;
using Food_Inventory.Properties;
using System.Data.OleDb;

namespace Food_Inventory.UCControls
{
    public partial class UCSettings : UserControl
    {

        // database
        private DBAccess database;

        // variables 
        private String categoryName, categoryItems;
       
        public UCSettings()
        {
            InitializeComponent();
        }

        private void UCSettings_Load(object sender, EventArgs e)
        {
            // database related code
            database = new DBAccess(DashboardForm.dataSourcePath);

            tbAccessTableName.Text = Settings.Default["accessTableName"].ToString();
            tbDataSource.Text = Settings.Default["dataSourcePath"].ToString();
            refreshCbData();

        }

        private void btnAddNewCategory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbCategoryName.Text))
            {
                MessageBox.Show("Category name is empty!", "Invalid Category Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult result =
                MessageBox.Show("Do you want to add category?", "Add Category", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
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

                categoryName = tbCategoryName.Text.Trim();
                categoryItems = "";

                string query = "Insert into " + DashboardForm.accessTableName + " (categoryName, categoryItems)" +
                               "values (@categoryName ,@categoryItems)";
                OleDbCommand command = new OleDbCommand(query);

                command.Parameters.AddWithValue("@categoryName", categoryName);
                command.Parameters.AddWithValue("@categoryItems", categoryItems);


                try
                {
                    database.executeQuery(command);
                    MessageBox.Show("Category added successfully\n" + "Category Name: " + categoryName);

                    // reset some controls
                    tbCategoryName.Text = "";
                   
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show("Error!!! Can't insert record to database\n" + ex.Message);
                    database.closeConn();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    database.closeConn();
                    return;
                }
                database.closeConn();
                refreshCbData();
      

            }

        }

        private void btnRemoveCategory_Click(object sender, EventArgs e)
        {
         
            if (cbCategoryListToDelete.Items.Count > 0)
            {
                DialogResult result = MessageBox.Show("Do you want to delete selected category?", "Delete Category", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    try
                    {

                        string categoryName = cbCategoryListToDelete.SelectedItem.ToString();
                        string query = "Delete From " + DashboardForm.accessTableName + " where categoryName = '" + categoryName + "';";


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

                        OleDbCommand deleteCommand = new OleDbCommand(query);
                        int row = database.executeQuery(deleteCommand);

                        if (row >= 1)
                        {

                            MessageBox.Show( categoryName + " category removed successfully", "Remove Category");
                            // Read categories again
                            refreshCbData();
                        }
                        else
                        {
                            MessageBox.Show("Error!!!\n" + categoryName + " category can't be deleted");

                        }
                        database.closeConn();
                      
                    }
                    catch (OleDbException ex)
                    {
                        MessageBox.Show(ex.Message);
                        database.closeConn();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        database.closeConn();
                    }

                }

            }
            else
            {

                MessageBox.Show("No category to delete", "Empty Category");
                
            }

        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(tbItemName.Text))
            {
                MessageBox.Show("Item name is empty!", "Invalid Item Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var itemName = tbItemName.Text.Trim() + " ";
            float itemPrice;
            var categoryName = cbCategoryList.SelectedItem.ToString();

            try
            {
                if (String.IsNullOrEmpty(tbItemPrice.Text.ToString()))
                {
                    lblError.Text = "Error! enter a valid number";
                    return;
                }

                itemPrice = float.Parse(tbItemPrice.Text.Trim());


            }
            catch (FormatException)
            {
                lblError.Text = "Error! enter a valid number";
                return;
            }

            lblError.Text = "";

            DialogResult result =
                MessageBox.Show("Do you want to add item to category?", "Add Item", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
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

                // check if there are more ocurrences of record via valid ID

                string updateQuery = "";
                OleDbCommand updateCommand;
                int rowsUpdated = -1;

                if (!(string.IsNullOrEmpty(itemName)))
                {
                    try
                    {
                        string newQuery = "Select * From " + 
                            DashboardForm.accessTableName + " Where categoryName = '" + categoryName + "';";
                        
                        DataTable dtCategories = new DataTable();

                        database.readDatathroughAdapter(newQuery, dtCategories);

                        if (dtCategories.Rows.Count > 0)
                        {
                            var categoryItems = "";
                            if (dtCategories.Rows.Count > 0)
                            {
                                foreach (DataRow r in dtCategories.Rows)
                                {
                                    categoryItems = r["categoryItems"].ToString();
                                  
                                }

                            }

                            // adding lines
                            while (itemName.Length != 45 && itemName.Length <= 45)
                            {
                                itemName += "-";
                            }


                            if (itemName.Length < 15)
                            {
                                itemName += "------";
                            }
                            else if (itemName.Length > 14 && itemName.Length < 20)
                            {
                                itemName += "----";
                            }
                            else if ((itemName.Length > 19 && itemName.Length < 30) || itemName.Length > 45)
                            {
                                itemName += "--";
                            }

                            itemName += " /" + itemPrice + "\n";

                            categoryItems += itemName;
                         
                            // update record's data
                            updateQuery = "Update " + DashboardForm.accessTableName + " Set categoryItems = '" + categoryItems + "' Where categoryName = '" + categoryName + "';";

                            updateCommand = new OleDbCommand(updateQuery);

                            rowsUpdated = database.executeQuery(updateCommand);
                   

                        }

                                
                    }
                    catch (FormatException ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }

                if (rowsUpdated > 0)
                {

                    MessageBox.Show( itemName.Substring(0, itemName.IndexOf('-')) + " added to " + categoryName + " category", "New Item Added!!!", MessageBoxButtons.OK);

                }
                else
                {
                    MessageBox.Show("Error occured!!! Can't add new item", "Error!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    database.closeConn();
                    return;

                }
          
                database.closeConn();

            }
            
        }

        private void tbItemPrice_TextChanged(object sender, EventArgs e)
        {

            try
            {
                if (String.IsNullOrEmpty(tbItemPrice.Text.ToString()))
                {
                    lblError.Text = "Error! enter a valid item price";
                    return;
                }

                float itemPrice = float.Parse(tbItemPrice.Text.Trim());


            }
            catch (FormatException)
            {
                lblError.Text = "Error! enter a valid item price";
                return;
            }

            lblError.Text = "";
        }


        private void btnRemoveItem_Click(object sender, EventArgs e)
        {

            var itemName = tbItemNameRemove.Text.Trim();
            var categoryName = cbCategoryToRemoveItem.SelectedItem.ToString();

            if (string.IsNullOrEmpty(itemName.Trim()))
            {
                lblItemNameError.Text = "Item name should not be empty";
                return;
            }

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

            try
            {

                DataTable dtc = new DataTable();
               
                string q = "Select * From " + DashboardForm.accessTableName + " where categoryName = '" + categoryName + "';";
                database.readDatathroughAdapter(q, dtc);

                var categoryItems = "";
                if (dtc.Rows.Count > 0)
                {

                    foreach (DataRow r in dtc.Rows)
                    {
                        categoryItems = r["categoryItems"].ToString();
                        
                    }
                    if (string.IsNullOrEmpty(categoryItems.Trim()))
                    {
                        MessageBox.Show(itemName + " does not exist in " + categoryName + " category");
                        return;
                    }

                    // splitting category items
                    string [] allCategoryItems = categoryItems.Split('\n');
                    string newCategoryItems = "";
                    bool itemExists = false;

                    Console.WriteLine(allCategoryItems.Length);
                    if (allCategoryItems.Length > 0)
                    {
                        for (int i = 0; i < allCategoryItems.Length -1; i++)
                        {
                            var getItemName = allCategoryItems[i].Substring(0, allCategoryItems[i].IndexOf('-'));


                            if (getItemName.ToLower().Trim() == itemName.ToLower())
                            {
                                itemExists = true;

                            }
                            else
                            {
                                newCategoryItems += allCategoryItems[i] + "\n";
                            }

                        }

                        if (!itemExists)
                        {
                            MessageBox.Show(itemName + " does not exist in the " + categoryName);
                            return;
                        }
                    }

                    // delete item name if exists 

                    DialogResult result =
                        MessageBox.Show("Do you want to remove " 
                        + itemName + " form " + categoryName 
                        + " category?", "Remove Item", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {

                        // check if there are more ocurrences of record via valid ID
                        string updateQuery = "";
                        OleDbCommand updateCommand;
                        int rowsUpdated = -1;

                        try
                        {
                            string newQuery = "Select * From " +
                                DashboardForm.accessTableName + " Where categoryName = '" + categoryName + "';";

                            DataTable dtCategories = new DataTable();

                            database.readDatathroughAdapter(newQuery, dtCategories);

                            if (dtCategories.Rows.Count > 0)
                            {
                                // update record's data
                                updateQuery = "Update " + DashboardForm.accessTableName + " Set categoryItems = '" + newCategoryItems + "' Where categoryName = '" + categoryName + "';";

                                updateCommand = new OleDbCommand(updateQuery);

                                rowsUpdated = database.executeQuery(updateCommand);
                                database.closeConn();

                            }


                        }
                        catch (FormatException ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }

                        if (rowsUpdated > 0)
                        {

                            MessageBox.Show(itemName + " removed from " + categoryName + " category", "Remove Item", MessageBoxButtons.OK);

                        }
                        else
                        {
                            MessageBox.Show("Error occured!!! Can't remove Item", "Error!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            database.closeConn();
                            return;

                        }


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

            database.closeConn();
        }

        private void tbItemNameRemove_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbItemNameRemove.Text.Trim()))
            {
                lblItemNameError.Text = "Item name should not be empty";

            }
            else
            {
                lblItemNameError.Text = "";
            }

        }

        private void btnChangeDeliveryNumber_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(tbDeliveryNumber.Text))
            {
                MessageBox.Show("Enter a valid phone number!", "Invalid Number", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Do you want to change delivery phone number?", "Change Number", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                Settings.Default["phoneNumber"] = tbDeliveryNumber.Text.ToString().Trim();
                Settings.Default.Save();
                DashboardForm.phoneNumber = Settings.Default["phoneNumber"].ToString();
                MessageBox.Show("Delivery number changed to: " + Settings.Default["phoneNumber"].ToString());
            }
        }

        private void btnChangeDataSource_Click(object sender, EventArgs e)
        {
       

            DialogResult result = MessageBox.Show("Do you want to change data source and access table name?", 
                "Change Data Source", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
             
                Settings.Default["dataSourcePath"] = tbDataSource.Text.ToString().Trim();
                Settings.Default["accessTableName"] = tbAccessTableName.Text.ToString().Trim();
                Settings.Default.Save();
                DashboardForm.dataSourcePath = Settings.Default["dataSourcePath"].ToString();
                DashboardForm.accessTableName = Settings.Default["accessTableName"].ToString();
                MessageBox.Show("Data source path: " + Settings.Default["dataSourcePath"].ToString() +
                    "\nAccess table name: " + Settings.Default["accessTableName"], "Data Source changed");
            }
        }

        private void btnChangeSource_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Access files (*.accdb) | *.accdb";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                tbDataSource.Text = fd.FileName;

            }
        }

        // ---------------------- functions --------------------------

        private void refreshCbData()
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

            try
            {

                DataTable dtc = new DataTable();
                string q = "Select * from " + DashboardForm.accessTableName;
                database.readDatathroughAdapter(q, dtc);

                var categoryNames = "";
                if (dtc.Rows.Count > 0)
                {
                    cbCategoryListToDelete.Items.Clear();
                    cbCategoryList.Items.Clear();
                    cbCategoryToRemoveItem.Items.Clear();

                    foreach (DataRow r in dtc.Rows)
                    {
                        categoryNames = r["categoryName"].ToString();
                        cbCategoryListToDelete.Items.Add(categoryNames);
                        cbCategoryList.Items.Add(categoryNames);
                        cbCategoryToRemoveItem.Items.Add(categoryNames);
                    }

                    cbCategoryListToDelete.SelectedIndex = 0;
                    cbCategoryList.SelectedIndex = 0;
                    cbCategoryToRemoveItem.SelectedIndex = 0;
                    btnRemoveCategory.Enabled = true;
                }
                else
                {
                    btnRemoveCategory.Enabled = false;
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
            
        }

    }
}
