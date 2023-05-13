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
        manu = new MenuStrip();
        menuFile = new ToolStripMenuItem();
        menuFileOpenFolder = new ToolStripMenuItem();
        splitContainer = new SplitContainer();
        treeFolders = new TreeView();
        tabControl1 = new TabControl();
        tabVariables = new TabPage();
        lstVariables = new ListView();
        colVariableName = new ColumnHeader();
        colVariableValue = new ColumnHeader();
        tabConfigFiles = new TabPage();
        txtConfig = new Scintilla();
        tabResolvedVariables = new TabPage();
        dlgBrowseFolder = new FolderBrowserDialog();
        manu.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        tabControl1.SuspendLayout();
        tabVariables.SuspendLayout();
        tabConfigFiles.SuspendLayout();
        SuspendLayout();
        // 
        // manu
        // 
        manu.Items.AddRange(new ToolStripItem[] { menuFile });
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
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 24);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(treeFolders);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(tabControl1);
        splitContainer.Size = new Size(800, 426);
        splitContainer.SplitterDistance = 266;
        splitContainer.TabIndex = 1;
        // 
        // treeFolders
        // 
        treeFolders.Dock = DockStyle.Fill;
        treeFolders.Location = new Point(0, 0);
        treeFolders.Name = "treeFolders";
        treeFolders.Size = new Size(266, 426);
        treeFolders.TabIndex = 0;
        treeFolders.AfterSelect += treeFolders_AfterSelect;
        // 
        // tabControl1
        // 
        tabControl1.Controls.Add(tabVariables);
        tabControl1.Controls.Add(tabConfigFiles);
        tabControl1.Controls.Add(tabResolvedVariables);
        tabControl1.Dock = DockStyle.Fill;
        tabControl1.Location = new Point(0, 0);
        tabControl1.Multiline = true;
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(530, 426);
        tabControl1.TabIndex = 0;
        // 
        // tabVariables
        // 
        tabVariables.Controls.Add(lstVariables);
        tabVariables.Location = new Point(4, 24);
        tabVariables.Name = "tabVariables";
        tabVariables.Padding = new Padding(3);
        tabVariables.Size = new Size(522, 398);
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
        lstVariables.Size = new Size(516, 392);
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
        tabConfigFiles.Size = new Size(522, 398);
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
        txtConfig.Size = new Size(516, 392);
        txtConfig.TabIndents = true;
        txtConfig.TabIndex = 0;
        txtConfig.Text = "scintilla1";
        txtConfig.UseRightToLeftReadingLayout = false;
        txtConfig.ViewWhitespace = WhitespaceMode.VisibleOnlyIndent;
        txtConfig.WrapMode = WrapMode.None;
        // 
        // tabResolvedVariables
        // 
        tabResolvedVariables.Location = new Point(4, 24);
        tabResolvedVariables.Name = "tabResolvedVariables";
        tabResolvedVariables.Size = new Size(522, 398);
        tabResolvedVariables.TabIndex = 2;
        tabResolvedVariables.Text = "Resolved Variables";
        tabResolvedVariables.UseVisualStyleBackColor = true;
        // 
        // dlgBrowseFolder
        // 
        dlgBrowseFolder.Description = "Select the configuration root directory";
        dlgBrowseFolder.ShowNewFolderButton = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(splitContainer);
        Controls.Add(manu);
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
        tabControl1.ResumeLayout(false);
        tabVariables.ResumeLayout(false);
        tabConfigFiles.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip manu;
    private ToolStripMenuItem menuFile;
    private ToolStripMenuItem menuFileOpenFolder;
    private SplitContainer splitContainer;
    private TreeView treeFolders;
    private TabControl tabControl1;
    private TabPage tabVariables;
    private ListView lstVariables;
    private ColumnHeader colVariableName;
    private ColumnHeader colVariableValue;
    private TabPage tabConfigFiles;
    private TabPage tabResolvedVariables;
    private FolderBrowserDialog dlgBrowseFolder;
    private ScintillaNET.Scintilla txtConfig;
}
