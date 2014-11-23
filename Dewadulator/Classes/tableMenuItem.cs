using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections.Specialized;
using System.Reflection;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.SqlServer.Management.SqlStudio.Explorer;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration;

namespace Dewadulator.Classes
{
    /// <summary>
    /// table menu item extension
    /// </summary>
    class tableMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public tableMenuItem()
        {
            this.Text = "Edit Canonical Name";
        }
        public tableMenuItem(string text)
        {
            this.Text = text;
        }
        /// <summary>
        /// Invoke
        /// </summary>
        protected override void Invoke()
        {

        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new tableMenuItem();
        }

       // #region IWinformsMenuHandler Members

        /// <summary>
        /// Get Menu Items
        /// </summary>
        /// <returns></returns>
        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            ToolStripMenuItem item = new ToolStripMenuItem(this.Text);

            item.Click += new EventHandler(EditCanonicalName_Click);

            return new ToolStripItem[] { item };
        }

        void EditCanonicalName_Click(object sender, EventArgs e)
        {

     //       System.Windows.Forms.MessageBox.Show("test1");

             //myForm.ShowDialog(this.n);
            SqlClientData MyData = new SqlClientData();
            //get selected node

            ObjectExplorerService objectExplorerService = (ObjectExplorerService)ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));
            int num2;
            INodeInformation[] array;
            objectExplorerService.GetSelectedNodes(out num2, out array);
            if (num2 > 0)
            {
           //     string NewText = (string)MyData.TextScalar("SELECT TOP 1 NewText FROM dbo.BetterSQLNames WHERE OldText = @A", "@A", array[0].InvariantName);
                string NewText = DataCache.GetNewText(array[0].InvariantName);
                if (NewText == null)
                    NewText = "";

                EditCanonical myForm = new EditCanonical(array[0].InvariantName, NewText);
        
                myForm.ShowDialog();

                if (!String.IsNullOrEmpty(myForm.newname))
                {
                    TreeView tv = GetValue<TreeView>(objectExplorerService, "Tree");

                    if (!String.IsNullOrEmpty(NewText))
                    {
                        //in this case replace  
                        tv.SelectedNode.Text = tv.SelectedNode.Text.Replace(NewText, myForm.newname);
               
                    }
                    else
                    {
                        string TextToAppend = " (" + myForm.newname + ")";


                        //else append
                        tv.SelectedNode.Text = tv.SelectedNode.Text + TextToAppend;
               
                    }

                       //somehow change underlying treenode
                  //  object i = 1;
                }
            }
            


        }
        public static T GetValue<T>(object object_0, string string_0)
        {
            object obj2 = null;
            PropertyInfo property = object_0.GetType().GetProperty(string_0, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                obj2 = property.GetValue(object_0, null);
            }
            return (T)obj2;
        }
    }
}
