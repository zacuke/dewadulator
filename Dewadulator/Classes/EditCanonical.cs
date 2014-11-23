using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dewadulator.Classes
{
    public partial class EditCanonical : Form
    {

        public string newname;
        public EditCanonical()
        {
            InitializeComponent();
        }

        public EditCanonical(string before, string after)
        {
            InitializeComponent();
            txtBefore.Text = before;
            txtAfter.Text = after;
           

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {

                //save
                DataCache.InvalidateCache(txtBefore.Text);
                SqlClientData MyData = new SqlClientData();
                MyData.TextNonQuery(" Update dbo.bettersqlnames Set newtext = @A where oldtext = @B; IF @@ROWCOUNT = 0 INSERT dbo.bettersqlnames (NewText, oldtext) VALUES (@A,@B)",
                "@A", txtAfter.Text, "@B", txtBefore.Text);

                newname = txtAfter.Text;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
             this.Close();

            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
