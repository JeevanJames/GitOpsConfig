using System.Runtime.CompilerServices;

using IniFile;

using ScintillaNET;

using static ScintillaNET.Style.Json;

namespace GitOpsConfig.Client.WinGitOpsConfig;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void menuFileOpenFolder_Click(object sender, EventArgs e)
    {
        if (dlgBrowseFolder.ShowDialog(this) != DialogResult.OK)
            return;

        string rootDir = dlgBrowseFolder.SelectedPath;

        string appsDir = Path.Combine(rootDir, "apps");

        if (!Directory.Exists(appsDir))
        {
            MessageBox.Show(
                $"The specified configuration directory '{rootDir}' does not contain an apps sub-directory.",
                "No apps sub-directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        string[] appDirs = Directory.GetDirectories(appsDir, "*", SearchOption.TopDirectoryOnly);
        if (appDirs.Length == 0)
        {
            MessageBox.Show(
                $"The specified configuration directory '{rootDir}' does not contain any app sub-directories.",
                "No apps found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        statusConfigDir.Text = rootDir;

        treeAppFolders.Nodes.Clear();
        PopulateNodes(treeAppFolders, appsDir, CreateNode);

        treeSharedFolders.Nodes.Clear();
        string sharedDir = Path.Combine(rootDir, "shared");
        if (Directory.Exists(sharedDir))
            PopulateNodes(treeSharedFolders, sharedDir, CreateNode);

        static TreeNode CreateNode(TreeView tree, string name, string text) => tree.Nodes.Add(name, text);
    }

    private static void PopulateNodes<TTree>(TTree tree, string dir,
        Func<TTree, string, string, TreeNode> nodeCreator)
    {
        string[] subdirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
        foreach (string subdir in subdirs)
        {
            string subdirName = Path.GetFileName(subdir);
            TreeNode childNode = nodeCreator(tree, subdir, subdirName);
            PopulateNodes(childNode, subdir, (node, name, text) => node.Nodes.Add(name, text));
        }
    }

    private void treeFolders_AfterSelect(object sender, TreeViewEventArgs e)
    {
        PopulateVariables(e.Node);
        PopulateConfig(e.Node);
    }

    private void PopulateVariables(TreeNode? node)
    {
        lstVariables.Items.Clear();

        if (node is null)
        {
            lstVariables.BackColor = Color.Gray;
            return;
        }

        string dir = node.Name;
        string variablesFilePath = Path.Combine(dir, "variables.ini");
        if (!File.Exists(variablesFilePath))
        {
            lstVariables.BackColor = Color.Gray;
            return;
        }

        lstVariables.BackColor = Color.White;

        Ini ini = new(variablesFilePath, IniLoadSettings.ReadOnly);
        foreach (Section section in ini)
        {
            foreach (Property property in section)
            {
                string propertyName = $"{section.Name}.{property.Name}";
                ListViewItem item = new(propertyName);
                item.SubItems.Add(property.Value);
                lstVariables.Items.Add(item);
            }
        }
    }

    private void PopulateConfig(TreeNode? node)
    {
        txtConfig.Text = string.Empty;

        if (node is null)
            return;

        string dir = node.Name;
        string configFilePath = Path.Combine(dir, "appsettings.json");
        if (!File.Exists(configFilePath))
            return;

        string config = File.ReadAllText(configFilePath);
        txtConfig.Text = config;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        StyleCollection s = txtConfig.Styles;
        s[PropertyName].ForeColor = Color.Navy;
        s[Style.Json.String].ForeColor = Color.Maroon;
        s[BlockComment].ForeColor = Color.DarkGreen;
        s[LineComment].ForeColor = Color.DarkGreen;
        s[Style.Json.Uri].ForeColor = Color.Blue;
        s[Operator].ForeColor = Color.OrangeRed;
    }
}
