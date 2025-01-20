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
using Autodesk.AutoCAD.GraphicsSystem;

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
        // ImageList imageList = new ImageList();
        // private int imageWidth;
        // private int imageHeight;

        public MXUForm()
        {
            InitializeComponent();
            this.Size = new Size(1024, 768);

            CreateMainMenu();
            CreateTableLayoutPanel();
            fbDialog1.Description = "Select a target directory";
            // InitializeImageList();
            PopulateDriveComboBox();
            
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
            tPanel1.Dock = DockStyle.Left;
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
            PopulateTreeView(selectedDrive);
        }

        private void PopulateTreeView(string currentPath)
        {
            treeView1.Nodes.Clear();
            TreeNode rootNode = new TreeNode(currentPath);
            rootNode.Tag = currentPath;
            treeView1.Nodes.Add(rootNode);
            PopulateTreeNode(rootNode, currentPath);
        }


        private void PopulateTreeNode(TreeNode parentNode, string path)
        {
            try
            {
                string[] directories;
                try
                {
                    directories = Directory.GetDirectories(path);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Access denied to directory: {path}");
                    return;
                }

                foreach (string directory in directories)
                {
                    TreeNode childNode = new TreeNode(Path.GetFileName(directory));
                    childNode.Tag = directory; // Store the actual path
                    childNode.Nodes.Add(new TreeNode("...")); // Placeholder for subdirectories

                    parentNode.Nodes.Add(childNode);
                }

                if (!rootOnly)
                {
                    string[] files;
                    try
                    {
                        files = Directory.GetFiles(path);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Access denied to files in directory: {path}");
                        return;
                    }

                    foreach (string file in files)
                    {
                        TreeNode fileNode = new TreeNode(Path.GetFileName(file));
                        fileNode.Tag = file; // Store the actual path
                        parentNode.Nodes.Add(fileNode);
                    }
                }
            }
            catch (Exception ex)
            {
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
                MessageBox.Show($"Selected: {selectedTarget}");
            }
        }
        
        /* imageList garbage //
        private void InitializeImageList()
        {
            int imageWidth = 16;
            int imageHeight = 16;
            
            imageList.ImageSize = new Size(imageWidth, imageHeight);

            try
            {
                imageList.Images.Add(new Bitmap(16, 16)); // Index 0: No Image
                imageList.Images.Add(CreateFilledColorImage(Color.Blue)); // Index 1: Blue
                imageList.Images.Add(CreateFilledColorImage(Color.Green)); // Index 2: Green for selected state

                imageList.Images.Add(System.Drawing.Image.FromFile("C:\\Users\\mnewman\\source\\repos\\MXU\\accept.png"));
                imageList.Images.Add(System.Drawing.Image.FromFile("C:\\Users\\mnewman\\source\\repos\\MXU\\cross.png"));
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Icon file not found: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            treeView1.ImageList = imageList;
            treeView1.ItemHeight = imageHeight;
            treeView1.ImageIndex = 1;
            treeView1.SelectedImageIndex = 2;

            Console.WriteLine("ImageList initialized");
            Console.WriteLine($"ImageList count: {imageList.Images.Count}");
            Console.WriteLine(imageList.Images[0].Size);

            // Assign imageHeight to a class-level variable
            this.imageHeight = imageHeight;
        }
        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Calculate the desired node height based on image size and text 
            int nodeHeight = imageHeight;

            // Draw the node background 
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);

            // Draw the image (if any)
            if (e.Node.ImageIndex != -1)
            {
                e.Graphics.DrawImage(imageList.Images[e.Node.ImageIndex], e.Bounds.X, e.Bounds.Y);
            }

            // Draw the node text
            e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont, Brushes.Black, e.Bounds.X + imageWidth + 4, e.Bounds.Y);
        }
        // Helper method to create a filled color image
        private Bitmap CreateFilledColorImage(Color color)
        {
            Bitmap filledImage = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(filledImage))
            {
                g.FillRectangle(new SolidBrush(color), 0, 0, 16, 16);
            }
            return filledImage;
        }
        */
    }
}
