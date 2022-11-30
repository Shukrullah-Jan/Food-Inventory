using Food_Inventory.Properties;
using System;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;

namespace Food_Inventory
{
    public partial class DashboardForm : Form
    {
        // database
        DBAccess database;
        Functions functions = new Functions();

        // settings variables
        public static string dataSourcePath;
        public static string accessTableName;
        public static string databaseName;
        public static Boolean settingsState;

        // other variables
        public static string phoneNumber;
        public static int slipNumber;

        private String foodInventoryFolderName = "Food Inventory Files";
        public DashboardForm()
        {
            InitializeComponent();
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {


            // check datasource related information
            settingsState = (Boolean)Settings.Default["settingsState"];

          
            if (settingsState == false)
            {
                if (!(Directory.Exists(Path.Combine(functions.getAvailableDriveName(), foodInventoryFolderName))))
                {
                    Directory.CreateDirectory(Path.Combine(functions.getAvailableDriveName(), foodInventoryFolderName));
                }

                dataSourcePath = 
                    Path.Combine(functions.getAvailableDriveName(), "Food Inventory Files\\Food_Inventory.accdb");
                accessTableName = "category_table";

                databaseName = Path.GetFileName(dataSourcePath);

                // save settings
              
                Settings.Default["dataSourcePath"] = dataSourcePath;
                Settings.Default["accessTableName"] = accessTableName;
                Settings.Default["settingsState"] = true;
                Settings.Default.Save();

            }
            else
            {
                dataSourcePath = Settings.Default["dataSourcePath"].ToString();
                accessTableName = Settings.Default["accessTableName"].ToString();
                databaseName = Path.GetFileName(dataSourcePath);

                // save settings
                Settings.Default["settingsState"] = true;
                Settings.Default.Save();

            }
            phoneNumber = Settings.Default["phoneNumber"].ToString();

            database = new DBAccess(dataSourcePath);

            try
            {
                database.createConn();
            }
            catch (OleDbException ex)
            {
                MessageBox.Show("Can't establish connection \nError in the Data source!!!\n\n" +
                    ex.Message, "Data source connection failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            // set new order user control
            UCControls.UCNewOrder newOrder = new UCControls.UCNewOrder();
            addUserControl(newOrder);


            // check if date is changed
            if (Settings.Default["currentDate"].ToString() == DateTime.Now.ToShortDateString())
            {
                slipNumber = int.Parse( Settings.Default["slipNumber"].ToString());
            }
            else
            {
                slipNumber = 1;
                Settings.Default["currentDate"] = DateTime.Now.ToShortDateString().ToString();
                Settings.Default["slipNumber"] = slipNumber.ToString();
                Settings.Default.Save();

            }
            
            

        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {
            UCControls.UCNewOrder newOrder = new UCControls.UCNewOrder();
            addUserControl(newOrder);

        }

        private void btnPendingOrders_Click(object sender, EventArgs e)
        {
            UCControls.UCPendingOrders pendingOrders = new UCControls.UCPendingOrders();
            addUserControl(pendingOrders);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
        
            UCControls.UCSettings settings = new UCControls.UCSettings();
            addUserControl(settings);
            
        }

        // ------------------ Functions -----------------------
      
        private void addUserControl(UserControl userControl)
        {
            // check user control type

            panelUserControls.Controls.Clear();
            panelUserControls.Controls.Add(userControl);
            userControl.BringToFront();
        }

       
    }
}
