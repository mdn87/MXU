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
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;

namespace MXU
{
    public partial class MXUForm : Form
    {
        private Button myButton;
        private FolderBrowserDialog fbDialog1 = new FolderBrowserDialog();
        private TextBox textBox1;
        private Button p1_btn1;
        private TreeView treeView1 = new TreeView();

        public MXUForm()
        {
            InitializeComponent();

            CreateMainMenu();
            CreateFlowLayoutPanel();
            fbDialog1.Description = "Select a target directory";


        }

        private void CreateMainMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
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
        private void CreateFlowLayoutPanel()
        {
            FlowLayoutPanel flPanel1 = new FlowLayoutPanel();
            flPanel1.Name = "flPanel1";
            flPanel1.Location = new System.Drawing.Point(0, 24);
            flPanel1.Anchor = AnchorStyles.Top;
            flPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flPanel1.MinimumSize = new System.Drawing.Size(600, 200);
            flPanel1.BorderStyle = BorderStyle.FixedSingle;
            flPanel1.Padding = new Padding(10);
            this.Controls.Add(flPanel1);

            FlowLayoutPanel p1_row1 = new FlowLayoutPanel();
            p1_row1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            p1_row1.Controls.Add(new Label { Text = "Project Dir: ", AutoSize = true, Height = 20, Anchor = AnchorStyles.Right });
            p1_row1.Height = 25;
            p1_row1.Width = 300;

            textBox1 = new TextBox();
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(200, 20);
            p1_row1.Controls.Add(textBox1);

            p1_btn1 = new Button { Text = "Button 1" };
            p1_btn1.Name = "p1_btn1";
            p1_btn1.Size = new System.Drawing.Size(75, 20);
            p1_btn1.Click += new EventHandler(p1_btn1_Click);
            p1_row1.Controls.Add(p1_btn1);

            flPanel1.Controls.Add(p1_row1);
            flPanel1.SetFlowBreak(p1_row1, true); // Ensure row 1 breaks to a new line

            FlowLayoutPanel p1_row2 = new FlowLayoutPanel();
            p1_row2.Height = 300;
            p1_row2.Width = 500;
            p1_row2.BorderStyle = BorderStyle.FixedSingle;

            treeView1.Height = 200;
            treeView1.Width = 600;
            p1_row2.Controls.Add(treeView1);

            flPanel1.Controls.Add(p1_row2);
            flPanel1.SetFlowBreak(p1_row2, true); // Ensure row 2 breaks to a new line if needed
        }





        private void PopulateTreeView(string rootPath)
        {
            treeView1.Nodes.Clear(); // Clear any existing nodes

            TreeNode rootNode = new TreeNode(rootPath);
            treeView1.Nodes.Add(rootNode);

            // Recursive function to populate child nodes
            PopulateTreeNode(rootNode, rootPath);
        }

        private void PopulateTreeNode(TreeNode parentNode, string path)
        {
            try
            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    TreeNode childNode = new TreeNode(Path.GetFileName(directory));
                    parentNode.Nodes.Add(childNode);
                    PopulateTreeNode(childNode, directory); // Recursively populate subdirectories
                }

                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    parentNode.Nodes.Add(new TreeNode(Path.GetFileName(file)));
                }
            }
            catch (Exception ex)
            {
                // Handle potential exceptions (e.g., access denied)
                MessageBox.Show($"Error accessing directory: {ex.Message}");
            }
        }

        // Load ????
        private void MXUForm_Load(object sender, EventArgs e)
        {

        }

        // Event Handlers
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("About My Application", "About");
        }
        private void p1_btn1_Click(object sender, EventArgs e)
        {

            if (fbDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = fbDialog1.SelectedPath;

                // Do something with the selected folder path (e.g., display in a textbox)
                PopulateTreeView(selectedFolderPath);
            }
        }
    }
}
