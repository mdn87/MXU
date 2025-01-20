using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MXUTools;
using System.Drawing.Printing;
using System.IO;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Interop.Common;
using System.Diagnostics;
using System.Reflection;

namespace MXU
{
    public partial class MXUForm : Form
    {
        private Button myButton;
        private FolderBrowserDialog fbDialog1 = new FolderBrowserDialog();
        private ComboBox driveComboBox = new ComboBox();
        private Button scanBtn = new Button();
        private TreeView treeView1 = new TreeView();
        private bool rootOnly = false;
        private TextBox infoTextBox = new TextBox();
        private MyAcadLib myAcadLib;

        public MXUForm()
        {
            InitializeComponent();
            this.Size = new Size(1024, 768);

            TreeNode rootNode = new TreeNode("C:\\");
            rootNode.Nodes.Add(new TreeNode()); // Add a dummy node
            treeView1.Nodes.Add(rootNode);

            CreateMainMenu();
            CreateTableLayoutPanel();
            fbDialog1.Description = "Select a target directory";
            InitializeTreeView();
            PopulateDriveComboBox();

            treeView1.BeforeExpand += new TreeViewCancelEventHandler(treeView1_BeforeExpand);
        }

        private void CreateMainMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;
            Controls.Add(menuStrip);

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            menuStrip.Items.Add(fileMenu);

            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            fileMenu.DropDownItems.Add(exitMenuItem);

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");
            menuStrip.Items.Add(helpMenu);

            ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("About");
            aboutMenuItem.Click += AboutMenuItem_Click;
            helpMenu.DropDownItems.Add(aboutMenuItem);
        }
        private void CreateTableLayoutPanel()
        {
            TableLayoutPanel tPanel1 = new TableLayoutPanel();
            tPanel1.Name = "tPanel1";
            tPanel1.Location = new Point(0, 24);
            tPanel1.ColumnCount = 2;
            tPanel1.RowCount = 3;
            tPanel1.Height = 6000;
            tPanel1.MinimumSize = new Size(800, 600);
            tPanel1.BorderStyle = BorderStyle.FixedSingle;
            tPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tPanel1.Padding = new Padding(10);
            // tPanel1.Dock = DockStyle.Top;
            this.Controls.Add(tPanel1);

            // Column 1
            // Row 1
            FlowLayoutPanel p1_row1 = new FlowLayoutPanel();
            p1_row1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            p1_row1.Controls.Add(new Label { Text = "Select Drive: ", AutoSize = true, Height = 20, Anchor = AnchorStyles.Right });
            p1_row1.Height = 25;
            p1_row1.Width = 400;

            driveComboBox.Name = "driveComboBox";
            driveComboBox.Size = new Size(200, 20);
            driveComboBox.SelectedIndexChanged += new EventHandler(driveComboBox_SelectedIndexChanged);
            p1_row1.Controls.Add(driveComboBox);

            tPanel1.Controls.Add(p1_row1, 0, 0);

            // Row 2
            FlowLayoutPanel p1_row2 = new FlowLayoutPanel();
            p1_row2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            p1_row2.Height = 360;
            p1_row2.Width = 400;
            p1_row2.BorderStyle = BorderStyle.FixedSingle;
            p1_row2.BackColor = Color.LightBlue;
            p1_row2.Padding = new Padding(10);

            Label tvLabel = new Label();
            tvLabel.Name = "tvLabel";
            tvLabel.Text = "DWG Files:";
            tvLabel.Size = new Size(360, 20);
            p1_row2.Controls.Add(tvLabel);

            treeView1.Name = "treeView1";
            treeView1.Height = 300;
            treeView1.Width = 360;
            p1_row2.Controls.Add(treeView1);

            tPanel1.Controls.Add(p1_row2, 0, 1);

            // Row 3
            FlowLayoutPanel p1_row3 = new FlowLayoutPanel();
            p1_row3.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            p1_row3.Height = 50;
            p1_row3.Width = 400;
            p1_row3.BorderStyle = BorderStyle.FixedSingle;
            p1_row3.BackColor = Color.LightCoral;
            p1_row3.Padding = new Padding(10);
            scanBtn = new Button { Text = "Scan" };
            scanBtn.Name = "scanBtn";
            scanBtn.Size = new Size(75, 20);
            scanBtn.Click += new EventHandler(scanBtn_Click);
            p1_row3.Controls.Add(scanBtn);

            tPanel1.Controls.Add(p1_row3, 0, 2);

            // Column 2
            // Row 1
            // Empty

            // Row 2
            // Add blank text area to display file info
            infoTextBox = new TextBox();
            infoTextBox.Multiline = true;
            infoTextBox.ReadOnly = true;
            infoTextBox.Dock = DockStyle.Fill;
            tPanel1.Controls.Add(infoTextBox, 1, 1);

        }
        private void PopulateDriveComboBox()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    driveComboBox.Items.Add(drive.Name);
                }
            }
            if (driveComboBox.Items.Count > 0)
            {
                driveComboBox.SelectedIndex = 0;
            }
        }
        private void driveComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDrive = driveComboBox.SelectedItem.ToString();
            TreeNode rootNode = new TreeNode(selectedDrive);
            PopulateTreeView(rootNode, selectedDrive);
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(rootNode);
        }
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes[0].Text == "" && e.Node.Nodes[0].Tag == null)
            {
                e.Node.Nodes.Clear();
                PopulateTreeView(e.Node, e.Node.Tag.ToString());
            }
        }
        private void PopulateTreeView(TreeNode parentNode, string path)
        {
            try
            {
                string[] directories = Directory.EnumerateDirectories(path).ToArray();

                foreach (string directory in directories)
                {
                    if (!string.IsNullOrEmpty(Path.GetFileName(directory)) &&
                        !Path.GetFileName(directory).StartsWith("$") &&
                        !IsHidden(directory))
                    {
                        TreeNode childNode = new TreeNode(Path.GetFileName(directory));
                        childNode.Tag = directory;
                        childNode.Nodes.Add(new TreeNode()); // Add a dummy node
                        parentNode.Nodes.Add(childNode);
                    }
                }

                string[] files = Directory.EnumerateFiles(path).ToArray();

                foreach (string file in files)
                {
                    if (!string.IsNullOrEmpty(Path.GetFileName(file)) &&
                        !Path.GetFileName(file).StartsWith("$") &&
                        !IsHidden(file))
                    {
                        TreeNode fileNode = new TreeNode(Path.GetFileName(file));
                        fileNode.Tag = file;
                        parentNode.Nodes.Add(fileNode);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory: {path}. Permission required: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error accessing directory: {path}. Error message: {ex.Message}");
            }
        }
        private bool IsHidden(string path)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(path);
                return (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            }
            catch (System.Exception)
            {
                // Handle exceptions (e.g., file not found)
                return false;
            }
        }





        // Load ????
        private void MXUForm_Load(object sender, EventArgs e)
        {

        }

        // Event Handlers
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("About My Application", "About");
        }
        private void InitializeTreeView()
        {
            // Set the desired row height
            treeView1.ItemHeight = 16; // Adjust as needed

            // You can remove these lines if you don't need a custom background for the nodes
            // treeView1.BackColor = Color.White; // Set background color
        }
        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Draw the node background (optional)
            // e.Graphics.FillRectangle(Brushes.White, e.Bounds); // Uncomment if needed

            // Draw the node text
            e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont, Brushes.Black, e.Bounds.X, e.Bounds.Y + 2); // Adjust text position as needed
        }
        private void scanBtn_Click(object sender, EventArgs e)
        {
            // Get full path of item selected in tree view
            string selectedTarget = treeView1.SelectedNode?.Tag?.ToString();
            if (selectedTarget != null)
            {
                // MessageBox.Show($"Selected: {selectedTarget}");
                GetSelectedItemInfo();
            }
        }
        private void GetSelectedItemInfo()
        {
            string selectedTarget = treeView1.SelectedNode?.Tag?.ToString();
            if (selectedTarget != null)
            {
                if (File.Exists(selectedTarget))
                {
                    FileInfo fileInfo = new FileInfo(selectedTarget);
                    infoTextBox.Text = $"File: {fileInfo.Name}" +
                        $"{Environment.NewLine}Size: {fileInfo.Length} bytes" +
                        $"{Environment.NewLine}Created: {fileInfo.CreationTime}";

                    if (fileInfo.Extension == ".dwg")
                    {
                        infoTextBox.Text += $"{Environment.NewLine}This is a DWG file";

                        try
                        {
                            string drawingPath = selectedTarget;
                            // AccoreConsoleWrapper wrapper = new AccoreConsoleWrapper(drawingPath);
                            // Console.WriteLine($"Opening drawing in AccoreConsole: {drawingPath}");
                            // wrapper.OpenDrawing();

                            // Run ObjectARX commands here
                            // wrapper.RunObjectARXCommand("_GetUniqueID");

                            try
                            {

                                Autodesk.AutoCAD.Interop.AcadApplication acadApp = new Autodesk.AutoCAD.Interop.AcadApplication();
                                Autodesk.AutoCAD.Interop.AcadDocument acadDoc = acadApp.Documents.Open(drawingPath);
                                dynamic acadDb = acadDoc.Database;
                                MyAcadLib myAcadLib = new MyAcadLib();

                                string result = myAcadLib._GetUniqueId(acadDb);
                                infoTextBox.Text += $"\nUniqueID from MyAcadLib DLL:\n{result}";
                                Console.WriteLine($"Executing ObjectARX command: _GetUniqueID");

                                acadDoc.Close(false);
                                acadApp.Quit();
                            }
                            catch (System.Exception ex)
                            {
                                MessageBox.Show($"Error executing DLL: {ex.Message}");
                            }

                            // wrapper.CloseDrawing();

                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show($"Error executing DLL: {ex.Message}");
                        }
                    }
                }
            
                else if (Directory.Exists(selectedTarget))
                {
                    // Handle directory information (unchanged)
                    DirectoryInfo dirInfo = new DirectoryInfo(selectedTarget);
                    infoTextBox.Text = $"Directory: {dirInfo.Name}" +
                                        $"{Environment.NewLine}Created: {dirInfo.CreationTime}";
                }
            }
        }

        public class AccoreConsoleWrapper
        {
            private Process _accoreConsoleProcess;
            private string _drawingFilePath;

            public AccoreConsoleWrapper(string drawingFilePath)
            {
                _drawingFilePath = drawingFilePath;
            }

            public void OpenDrawing()
            {
                // Create a new ProcessStartInfo object
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "C:\\Program Files\\Autodesk\\AutoCAD 2025\\accoreconsole.exe";
                startInfo.Arguments = $"/i \"{_drawingFilePath}\"";
                startInfo.UseShellExecute = false;

                // Create a new Process object
                _accoreConsoleProcess = new Process();
                _accoreConsoleProcess.StartInfo = startInfo;
                _accoreConsoleProcess.Start();
            }

            public void RunObjectARXCommand(string commandName, params object[] args)
            {
                string tempScriptPath = Path.GetTempFileName() + ".scr";
                Console.WriteLine($"Temp script path: {tempScriptPath}");

                try
                {
                    // Create the temporary script file
                    using (StreamWriter writer = new StreamWriter(tempScriptPath))
                    {
                        // Write NETLOAD command
                        writer.WriteLine($"NETLOAD \"C:\\Users\\mnewman\\source\\repos\\myAcadLib\\bin\\debug\\myAcadLib.dll\"");
                        // Write your ObjectARX command (replace with your actual command)
                        writer.WriteLine($"._{commandName}");
                    }

                    // Launch AccoreConsole with the script
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "C:\\Program Files\\Autodesk\\AutoCAD 2025\\accoreconsole.exe";
                    startInfo.Arguments = $"/i \"{_drawingFilePath}\" /s \"{tempScriptPath}\"";
                    startInfo.UseShellExecute = false;

                    using (Process scriptProcess = new Process())
                    {
                        scriptProcess.StartInfo = startInfo;
                        scriptProcess.Start();
                        scriptProcess.WaitForExit();
                    }
                }
                finally
                {
                    // Delete the temporary script file
                    if (File.Exists(tempScriptPath))
                    {
                        File.Delete(tempScriptPath);
                    }
                }
            }

            public void CloseDrawing()
            {
                if (!_accoreConsoleProcess.HasExited)
                {
                    _accoreConsoleProcess.Kill();
                }
            }
        }
    }
}
