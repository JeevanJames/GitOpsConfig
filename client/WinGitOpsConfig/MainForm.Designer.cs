#pragma warning disable CS0618 // Type or member is obsolete

using ScintillaNET;

namespace GitOpsConfig.Client.WinGitOpsConfig;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        manu = new MenuStrip();
        menuFile = new ToolStripMenuItem();
        menuFileOpenFolder = new ToolStripMenuItem();
        menuView = new ToolStripMenuItem();
        splitContainer = new SplitContainer();
        tabsNavigation = new TabControl();
        tabNavigationAppFolders = new TabPage();
        treeAppFolders = new TreeView();
        tabNavigationDeployments = new TabPage();
        tabNavigationSharedFolders = new TabPage();
        tabsResults = new TabControl();
        tabVariables = new TabPage();
        lstVariables = new ListView();
        colVariableName = new ColumnHeader();
        colVariableValue = new ColumnHeader();
        tabConfigFiles = new TabPage();
        txtConfig = new Scintilla();
        dlgBrowseFolder = new FolderBrowserDialog();
        toolStrip1 = new ToolStrip();
        tbtnOpenFolder = new ToolStripButton();
        tbtnRefresh = new ToolStripButton();
        statusBar = new StatusStrip();
        statusConfigDir = new ToolStripStatusLabel();
        treeSharedFolders = new TreeView();
        manu.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        tabsNavigation.SuspendLayout();
        tabNavigationAppFolders.SuspendLayout();
        tabNavigationSharedFolders.SuspendLayout();
        tabsResults.SuspendLayout();
        tabVariables.SuspendLayout();
        tabConfigFiles.SuspendLayout();
        toolStrip1.SuspendLayout();
        statusBar.SuspendLayout();
        SuspendLayout();
        // 
        // manu
        // 
        manu.Items.AddRange(new ToolStripItem[] { menuFile, menuView });
        manu.Location = new Point(0, 0);
        manu.Name = "manu";
        manu.Size = new Size(800, 24);
        manu.TabIndex = 0;
        manu.Text = "Main menu";
        // 
        // menuFile
        // 
        menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuFileOpenFolder });
        menuFile.Name = "menuFile";
        menuFile.Size = new Size(37, 20);
        menuFile.Text = "&File";
        // 
        // menuFileOpenFolder
        // 
        menuFileOpenFolder.Name = "menuFileOpenFolder";
        menuFileOpenFolder.Size = new Size(225, 22);
        menuFileOpenFolder.Text = "&Open Configuration Folder...";
        menuFileOpenFolder.Click += menuFileOpenFolder_Click;
        // 
        // menuView
        // 
        menuView.Name = "menuView";
        menuView.Size = new Size(44, 20);
        menuView.Text = "&View";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 49);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(tabsNavigation);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(tabsResults);
        splitContainer.Size = new Size(800, 379);
        splitContainer.SplitterDistance = 265;
        splitContainer.TabIndex = 1;
        // 
        // tabsNavigation
        // 
        tabsNavigation.Alignment = TabAlignment.Left;
        tabsNavigation.Controls.Add(tabNavigationAppFolders);
        tabsNavigation.Controls.Add(tabNavigationSharedFolders);
        tabsNavigation.Controls.Add(tabNavigationDeployments);
        tabsNavigation.Dock = DockStyle.Fill;
        tabsNavigation.Location = new Point(0, 0);
        tabsNavigation.Multiline = true;
        tabsNavigation.Name = "tabsNavigation";
        tabsNavigation.SelectedIndex = 0;
        tabsNavigation.Size = new Size(265, 379);
        tabsNavigation.TabIndex = 1;
        // 
        // tabNavigationAppFolders
        // 
        tabNavigationAppFolders.Controls.Add(treeAppFolders);
        tabNavigationAppFolders.Location = new Point(27, 4);
        tabNavigationAppFolders.Name = "tabNavigationAppFolders";
        tabNavigationAppFolders.Padding = new Padding(3);
        tabNavigationAppFolders.Size = new Size(234, 371);
        tabNavigationAppFolders.TabIndex = 0;
        tabNavigationAppFolders.Text = "App Folders";
        tabNavigationAppFolders.UseVisualStyleBackColor = true;
        // 
        // treeAppFolders
        // 
        treeAppFolders.Dock = DockStyle.Fill;
        treeAppFolders.Location = new Point(3, 3);
        treeAppFolders.Name = "treeAppFolders";
        treeAppFolders.Size = new Size(228, 365);
        treeAppFolders.TabIndex = 0;
        treeAppFolders.AfterSelect += treeFolders_AfterSelect;
        // 
        // tabNavigationDeployments
        // 
        tabNavigationDeployments.Location = new Point(27, 4);
        tabNavigationDeployments.Name = "tabNavigationDeployments";
        tabNavigationDeployments.Padding = new Padding(3);
        tabNavigationDeployments.Size = new Size(234, 371);
        tabNavigationDeployments.TabIndex = 1;
        tabNavigationDeployments.Text = "Deployments";
        tabNavigationDeployments.UseVisualStyleBackColor = true;
        // 
        // tabNavigationSharedFolders
        // 
        tabNavigationSharedFolders.Controls.Add(treeSharedFolders);
        tabNavigationSharedFolders.Location = new Point(27, 4);
        tabNavigationSharedFolders.Name = "tabNavigationSharedFolders";
        tabNavigationSharedFolders.Size = new Size(234, 371);
        tabNavigationSharedFolders.TabIndex = 2;
        tabNavigationSharedFolders.Text = "Shared Folders";
        tabNavigationSharedFolders.UseVisualStyleBackColor = true;
        // 
        // tabsResults
        // 
        tabsResults.Controls.Add(tabVariables);
        tabsResults.Controls.Add(tabConfigFiles);
        tabsResults.Dock = DockStyle.Fill;
        tabsResults.Location = new Point(0, 0);
        tabsResults.Multiline = true;
        tabsResults.Name = "tabsResults";
        tabsResults.SelectedIndex = 0;
        tabsResults.Size = new Size(531, 379);
        tabsResults.TabIndex = 0;
        // 
        // tabVariables
        // 
        tabVariables.Controls.Add(lstVariables);
        tabVariables.Location = new Point(4, 24);
        tabVariables.Name = "tabVariables";
        tabVariables.Padding = new Padding(3);
        tabVariables.Size = new Size(523, 351);
        tabVariables.TabIndex = 0;
        tabVariables.Text = "Variables";
        tabVariables.UseVisualStyleBackColor = true;
        // 
        // lstVariables
        // 
        lstVariables.Columns.AddRange(new ColumnHeader[] { colVariableName, colVariableValue });
        lstVariables.Dock = DockStyle.Fill;
        lstVariables.FullRowSelect = true;
        lstVariables.Location = new Point(3, 3);
        lstVariables.MultiSelect = false;
        lstVariables.Name = "lstVariables";
        lstVariables.Size = new Size(517, 345);
        lstVariables.TabIndex = 0;
        lstVariables.UseCompatibleStateImageBehavior = false;
        lstVariables.View = View.Details;
        // 
        // colVariableName
        // 
        colVariableName.Text = "Variable Name";
        colVariableName.Width = 200;
        // 
        // colVariableValue
        // 
        colVariableValue.Text = "Value";
        colVariableValue.Width = 300;
        // 
        // tabConfigFiles
        // 
        tabConfigFiles.Controls.Add(txtConfig);
        tabConfigFiles.Location = new Point(4, 24);
        tabConfigFiles.Name = "tabConfigFiles";
        tabConfigFiles.Padding = new Padding(3);
        tabConfigFiles.Size = new Size(523, 351);
        tabConfigFiles.TabIndex = 1;
        tabConfigFiles.Text = "Config Files";
        tabConfigFiles.UseVisualStyleBackColor = true;
        // 
        // txtConfig
        // 
        txtConfig.AutoCMaxHeight = 9;
        txtConfig.AutomaticFold = AutomaticFold.Show;
        txtConfig.BiDirectionality = BiDirectionalDisplayType.Disabled;
        txtConfig.CaretLineBackColor = Color.White;
        txtConfig.CaretLineVisible = true;
        txtConfig.Dock = DockStyle.Fill;
        txtConfig.Font = new Font("Consolas", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
        txtConfig.Lexer = Lexer.Json;
        txtConfig.LexerName = "json";
        txtConfig.Location = new Point(3, 3);
        txtConfig.Name = "txtConfig";
        txtConfig.ScrollWidth = 100;
        txtConfig.Size = new Size(517, 345);
        txtConfig.TabIndents = true;
        txtConfig.TabIndex = 0;
        txtConfig.Text = "scintilla1";
        txtConfig.UseRightToLeftReadingLayout = false;
        txtConfig.ViewWhitespace = WhitespaceMode.VisibleOnlyIndent;
        txtConfig.WrapMode = WrapMode.None;
        // 
        // dlgBrowseFolder
        // 
        dlgBrowseFolder.Description = "Select the configuration root directory";
        dlgBrowseFolder.ShowNewFolderButton = false;
        // 
        // toolStrip1
        // 
        toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip1.Items.AddRange(new ToolStripItem[] { tbtnOpenFolder, tbtnRefresh });
        toolStrip1.Location = new Point(0, 24);
        toolStrip1.Name = "toolStrip1";
        toolStrip1.Size = new Size(800, 25);
        toolStrip1.TabIndex = 2;
        toolStrip1.Text = "toolStrip1";
        // 
        // tbtnOpenFolder
        // 
        tbtnOpenFolder.DisplayStyle = ToolStripItemDisplayStyle.Image;
        tbtnOpenFolder.Image = (Image)resources.GetObject("tbtnOpenFolder.Image");
        tbtnOpenFolder.ImageTransparentColor = Color.Magenta;
        tbtnOpenFolder.Name = "tbtnOpenFolder";
        tbtnOpenFolder.Size = new Size(23, 22);
        tbtnOpenFolder.Text = "&New";
        tbtnOpenFolder.Click += menuFileOpenFolder_Click;
        // 
        // tbtnRefresh
        // 
        tbtnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Image;
        tbtnRefresh.Image = (Image)resources.GetObject("tbtnRefresh.Image");
        tbtnRefresh.ImageTransparentColor = Color.Magenta;
        tbtnRefresh.Name = "tbtnRefresh";
        tbtnRefresh.Size = new Size(23, 22);
        tbtnRefresh.Text = "toolStripButton1";
        // 
        // statusBar
        // 
        statusBar.Items.AddRange(new ToolStripItem[] { statusConfigDir });
        statusBar.Location = new Point(0, 428);
        statusBar.Name = "statusBar";
        statusBar.Size = new Size(800, 22);
        statusBar.TabIndex = 3;
        // 
        // statusConfigDir
        // 
        statusConfigDir.Name = "statusConfigDir";
        statusConfigDir.Size = new Size(164, 17);
        statusConfigDir.Text = "(No config directory selected)";
        // 
        // treeSharedFolders
        // 
        treeSharedFolders.Dock = DockStyle.Fill;
        treeSharedFolders.Location = new Point(0, 0);
        treeSharedFolders.Name = "treeSharedFolders";
        treeSharedFolders.Size = new Size(234, 371);
        treeSharedFolders.TabIndex = 1;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(splitContainer);
        Controls.Add(toolStrip1);
        Controls.Add(manu);
        Controls.Add(statusBar);
        MainMenuStrip = manu;
        Name = "MainForm";
        Text = "GitOps Configuration Manager";
        Load += MainForm_Load;
        manu.ResumeLayout(false);
        manu.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        tabsNavigation.ResumeLayout(false);
        tabNavigationAppFolders.ResumeLayout(false);
        tabNavigationSharedFolders.ResumeLayout(false);
        tabsResults.ResumeLayout(false);
        tabVariables.ResumeLayout(false);
        tabConfigFiles.ResumeLayout(false);
        toolStrip1.ResumeLayout(false);
        toolStrip1.PerformLayout();
        statusBar.ResumeLayout(false);
        statusBar.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip manu;
    private ToolStripMenuItem menuFile;
    private ToolStripMenuItem menuFileOpenFolder;
    private SplitContainer splitContainer;
    private TreeView treeAppFolders;
    private TabControl tabsResults;
    private TabPage tabVariables;
    private ListView lstVariables;
    private ColumnHeader colVariableName;
    private ColumnHeader colVariableValue;
    private TabPage tabConfigFiles;
    private FolderBrowserDialog dlgBrowseFolder;
    private ScintillaNET.Scintilla txtConfig;
    private TabControl tabsNavigation;
    private TabPage tabNavigationAppFolders;
    private TabPage tabNavigationDeployments;
    private ToolStripMenuItem menuView;
    private ToolStrip toolStrip1;
    private ToolStripButton tbtnOpenFolder;
    private StatusStrip statusBar;
    private ToolStripStatusLabel statusConfigDir;
    private ToolStripButton tbtnRefresh;
    private TabPage tabNavigationSharedFolders;
    private TreeView treeSharedFolders;
}
