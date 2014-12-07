using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WCFClient.ServiceReference1;

namespace WCFClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServiceReference1.DownloadDataProviderServiceClient client = new DownloadDataProviderServiceClient();
            if (client.IsValidLogin("admin", "admin"))
            {
                MessageBox.Show("Success");
            }
            else
            {
                MessageBox.Show("Failure");
            }
        }
    }
}
