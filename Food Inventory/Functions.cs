using System;
using System.Windows.Forms;
using System.IO;

namespace Food_Inventory
{
    internal class Functions
    {

        public string getAvailableDriveName()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            Boolean isCDriveReady = false;

            foreach (DriveInfo d in allDrives)
            {

                //Console.WriteLine("Drive {0}", d.Name);
                //Console.WriteLine("  Drive type: {0}", d.DriveType);
                if (d.IsReady == true)
                {

                    if (d.Name != "C:\\" && d.TotalFreeSpace >= 1000000000 && d.DriveType == DriveType.Fixed)
                    {
                        return d.Name;
                    }

                    if (d.Name == "C:\\" && d.TotalFreeSpace >= 5000000000 && d.DriveType == DriveType.Fixed)
                    {
                        isCDriveReady = true;
                    }

                    //Console.WriteLine("  Volume label: {0}", d.VolumeLabel);
                    //Console.WriteLine("  File system: {0}", d.DriveFormat);
                    //Console.WriteLine(
                    //    "  Available space to current user:{0, 15} bytes",
                    //    d.AvailableFreeSpace);

                    //Console.WriteLine(
                    //    "  Total available space:          {0, 15} bytes",
                    //    d.TotalFreeSpace);

                    //Console.WriteLine(
                    //    "  Total size of drive:            {0, 15} bytes ",
                    //    d.TotalSize);
                }
                else
                {
                    MessageBox.Show("No drive in your computer is ready or drives are out of minimum space for this operation\nStop any file operation and try again", "Drives are not ready");
                }

            }

            if (isCDriveReady == true) return "C:\\";

            return "";
        }
    }
}
