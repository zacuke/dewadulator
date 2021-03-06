using System;
using Extensibility;
using EnvDTE;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
//using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using Dewadulator.Classes;
using EnvDTE80;
using System.Drawing;
using Microsoft.SqlServer.Management;
using Microsoft.SqlServer.Management.SqlStudio.Explorer;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.Common;

using System.Resources;
using System.Globalization;


namespace Dewadulator
{

    public class Connect : IDTExtensibility2, IDTCommandTarget
    {

        private DTE _applicationDTE;
        private DTE2 _applicationObject;
        private AddIn _addInInstance;


        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        //[System.Runtime.InteropServices.DllImport("user32.dll", ExactSpelling = true)]
        //static extern IntPtr SetTimer(IntPtr hWnd, IntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);
        //delegate void TimerProc(IntPtr hWnd, uint uMsg, IntPtr nIDEvent, uint dwTime);
        //TimerProc m_proc;

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {
                // debug_message("OnConnection::");

                _addInInstance = (EnvDTE.AddIn)addInInst;
                _applicationDTE = (DTE)_addInInstance.DTE;
                _applicationObject = (DTE2)_addInInstance.DTE;

                if (connectMode == ext_ConnectMode.ext_cm_Startup)
                // if(1==1)
                {
                       
                    //grab reference to ObjectExplorer
                    ContextService contextService = null;
                    ObjectExplorerService objectExplorerService = (ObjectExplorerService)ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));

                    //events when expanding
                    TreeView tv = GetValue<TreeView>(objectExplorerService, "Tree");
                    tv.AfterExpand += new TreeViewEventHandler(TreeViewEventHandler);

                    //sometimes its 0 sometimes its 1
                    //for some reason this has to come afer the code above
                    foreach (var x in objectExplorerService.Container.Components)
                        if (x is ContextService)
                            contextService = (ContextService)x;
                     
         
                    //events when right clicking
                    INavigationContextProvider objectExplorerContext = contextService.ObjectExplorerContext;
                    objectExplorerContext.CurrentContextChanged += new NodesChangedEventHandler(this.Provider_SelectionChanged);


                }
                if (connectMode == ext_ConnectMode.ext_cm_UISetup)
                {
                    UpdateToolsMenu(0);

                }
                
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
   
    }
   
        //keeps track of when a menu is already added
        private HierarchyObject _tableMenu = null;
        private HierarchyObject _tableMenu2 = null;


        //matches only on Database node
        private Regex _tableRegex0 = new Regex("^Server\\[[^\\]]*\\]/Database\\[[^\\]]*\\]$");

        //matches only on table node
        private Regex _tableRegex = new Regex("^Server\\[[^\\]]*\\]/Database\\[[^\\]]*\\]/Table\\[[^\\]]*\\]$");

        //matches only on column node under tables
        private Regex _tableRegex2 = new Regex("^Server\\[[^\\]]*\\]/Database\\[[^\\]]*\\]/Table\\[[^\\]]*\\]/Column\\[[^\\]]*\\]$");

        private void Provider_SelectionChanged(object sender, NodesChangedEventArgs args)
        {
            try
            {
                INavigationContextProvider navigationContextProvider = (INavigationContextProvider)sender;
                INodeInformation nodeInformation = (args.ChangedNodes.Count > 0) ? args.ChangedNodes[0] : null;

                //first part handles Tables , second adds the menu for Columns
                //SSMS plugins are essentialy gigantic hacks
                if (this._tableMenu == null && this._tableRegex.IsMatch(nodeInformation.Context))
                {
                    this._tableMenu = (HierarchyObject)nodeInformation.GetService(typeof(IMenuHandler));
                    tableMenuItem value = new tableMenuItem();
                    this._tableMenu.AddChild(string.Empty, value);
                }
                else if (this._tableMenu2 == null && this._tableRegex2.IsMatch(nodeInformation.Context))
                {
                    this._tableMenu2 = (HierarchyObject)nodeInformation.GetService(typeof(IMenuHandler));
                    tableMenuItem value = new tableMenuItem();
                    this._tableMenu2.AddChild(string.Empty, value);
                }
                //else if (this._tableMenu2 == null && this._tableRegex0.IsMatch(nodeInformation.Context))
                //{
                //    this._tableMenu2 = (HierarchyObject)nodeInformation.GetService(typeof(IMenuHandler));
                //    tableMenuItem value = new tableMenuItem();
                //    this._tableMenu2.AddChild(string.Empty, value);
                //}
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }


        private void TreeViewEventHandler(object sender, TreeViewEventArgs e)
        {
            try
            {
                SqlClientData MyData = new SqlClientData();

                Application.DoEvents();

                
                UIConnectionInfo _myCurrentConnection = null;

                if (Microsoft.SqlServer.Management.UI.VSIntegration.ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo != null)
                {
                    _myCurrentConnection = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo.UIConnectionInfo;
                }

                SqlConnectionInfo _connInfo = new SqlConnectionInfo();
                //_connInfo.ServerName = _myCurrentConnection.ServerName;
                //_connInfo.UserName = _myCurrentConnection.UserName;

                string ServerName = _myCurrentConnection.ServerName;
                string DatabaseName;
                    
               // ObjectExplorerService objectExplorerService = (ObjectExplorerService)ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));
          //      TreeView tv = GetValue<TreeView>(objectExplorerService, "Tree");

                if (e.Node.Text.StartsWith("Tables"))
                {

                    int counter = 0;

                    List<TreeNode> NodesToRemove = new List<TreeNode>();
                    foreach (TreeNode x in e.Node.Nodes)
                    {
                        //x.Text = "blag";
                        if (x.Text == "System Tables" || x.Text == "FileTables")
                            continue;

                        string TableName = x.Text.Split(' ')[0];

                        string NewText = DataCache.GetNewText(TableName);

                        DatabaseName = e.Node.Parent.Text.Split(' ')[0];

                       int rowcnt = 1;

                        if (ServerName.Length > 0 && DatabaseName.Length > 0 && x.Text.Length > 0)
                            rowcnt = DataCache.GetRowCount(ServerName, DatabaseName, TableName);


                        string TextToAppend = " (" + NewText + ")";

                        if (!x.Text.EndsWith(TextToAppend) && !String.IsNullOrEmpty(NewText))
                            x.Text += TextToAppend;

                        //x.NodeFont = new Font(x.TreeView.Font, FontStyle.Italic);

                        if (!ShowEmptyTables && rowcnt == 0)
                            NodesToRemove.Add(x);
                        
                       // x.Text += counter.ToString();
                        counter += 1;
                    }

                    foreach (TreeNode x in NodesToRemove)
                    {
                        e.Node.TreeView.Nodes.Remove(x);
                    }



                }
                else if (e.Node.Text.StartsWith("Columns"))
                {
                    foreach (TreeNode x in e.Node.Nodes)
                    {
                        //x.Text = "blag";
                        string ColName;

                        ColName = x.Text.Split(' ')[0];


                        string NewText = DataCache.GetNewText(ColName);


                        string TextToAppend = " (" + NewText + ")";

                        if (!x.Text.EndsWith(TextToAppend) && !String.IsNullOrEmpty(NewText))
                            x.Text += TextToAppend;
                    }
                }


                if (e.Node.Parent != null && e.Node.Parent.Text.StartsWith("Tables"))
                {
                    string ColName;

                    ColName = e.Node.Text.Split(' ')[0];


                    string NewText = DataCache.GetNewText(ColName);

                    string TextToAppend = " (" + NewText + ")";

                    if (!e.Node.Text.EndsWith(TextToAppend) && !String.IsNullOrEmpty(NewText))
                        e.Node.Text += TextToAppend;

                    //  e.Node.


                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
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

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }


        /// <summary>
        /// General function for writing debug messages
        /// </summary>
        /// <param name="debug_string"></param>
        private void debug_message(string debug_string)
        {
            // put what ever logging you want.  All debugging messaging going to Output window.
            System.Diagnostics.Debug.WriteLine(string.Format("*** {0} : {1} ***", DateTime.Now.ToShortTimeString(), debug_string));
        }



        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "Dewadulator.Connect.Dewadulator")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        private bool ShowEmptyTables = false;

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "Dewadulator.Connect.Dewadulator")
                {
                    if (ShowEmptyTables)
                    {
                        ShowEmptyTables = false;
                        UpdateToolsMenu(0);

                    }
                    else
                    {
                        ShowEmptyTables = true;

                        UpdateToolsMenu(1907);
                    }

                    handled = true;
                    return;
                }
            }
        }


        private void UpdateToolsMenu(int imageIndex)
        {

            var contextGUIDS = new object[] { };
            Commands2 commands = (Commands2)this._applicationObject.Commands;
            string toolsMenuName = "Tools";

            //Place the command on the tools menu.
            //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            CommandBar menuBarCommandBar = ((CommandBars)this._applicationObject.CommandBars)["MenuBar"];

            //Find the Tools command bar on the MenuBar command bar:
            CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
            CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

            //    var commands = dte.Commands.Cast<EnvDTE.Command>();

            foreach (object command in _applicationDTE.Commands)
            {
                string name = ((dynamic)command).Name;
                if (name.ToLower().Contains("dewadulator"))
                    // MessageBox.Show(((dynamic)command).Name);
                    ((dynamic)command).Delete();
            }

            //foreach(var cmd in commands)
            //{
            //    MessageBox.Show(cmd.ToString());
            //}

            //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
            //  just make sure you also update the QueryStatus/Exec method to include the new command names.
            try
            {
                //Add a command to the Commands collection:
                Command command = commands.AddNamedCommand2(this._addInInstance, "Dewadulator", "Show Empty Tables", "Shows tables with zero rows", true, imageIndex, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                //Add a control for the command to the tools menu:
                if ((command != null) && (toolsPopup != null))
                {
                    command.AddControl(toolsPopup.CommandBar, 1);
                }
            }
            catch (ArgumentException)
            {
                //If we are here, then the exception is probably because a command with that name
                //  already exists. If so there is no need to recreate the command and we can 
                //  safely ignore the exception.
            }
        }

    }
}