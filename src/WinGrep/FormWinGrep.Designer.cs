namespace WinGrep
{
    partial class FormWinGrep
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWinGrep));
            this.lblCurFile = new System.Windows.Forms.Label();
            this.txtCurFile = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblResults = new System.Windows.Forms.Label();
            this.lblSearchText = new System.Windows.Forms.Label();
            this.lblFiles = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.lblDir = new System.Windows.Forms.Label();
            this.lbResults = new System.Windows.Forms.ListBox();
            this.checkedComboBox1 = new CheckComboBoxControl.CheckedComboBox();
            this.ckJustFiles = new System.Windows.Forms.CheckBox();
            this.ckIgnoreCase = new System.Windows.Forms.CheckBox();
            this.ckLineNumbers = new System.Windows.Forms.CheckBox();
            this.ckRecursive = new System.Windows.Forms.CheckBox();
            this.txtSearchText = new System.Windows.Forms.TextBox();
            this.ckCountLines = new System.Windows.Forms.CheckBox();
            this.txtDir = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblCurFile
            // 
            this.lblCurFile.Location = new System.Drawing.Point(11, 151);
            this.lblCurFile.Name = "lblCurFile";
            this.lblCurFile.Size = new System.Drawing.Size(84, 12);
            this.lblCurFile.TabIndex = 14;
            this.lblCurFile.Text = "Current File";
            // 
            // txtCurFile
            // 
            this.txtCurFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCurFile.BackColor = System.Drawing.SystemColors.Info;
            this.txtCurFile.Location = new System.Drawing.Point(11, 164);
            this.txtCurFile.Name = "txtCurFile";
            this.txtCurFile.ReadOnly = true;
            this.txtCurFile.Size = new System.Drawing.Size(472, 20);
            this.txtCurFile.TabIndex = 15;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(455, 24);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(28, 20);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "...";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblResults
            // 
            this.lblResults.Location = new System.Drawing.Point(11, 189);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(56, 16);
            this.lblResults.TabIndex = 16;
            this.lblResults.Text = "Results";
            // 
            // lblSearchText
            // 
            this.lblSearchText.Location = new System.Drawing.Point(203, 54);
            this.lblSearchText.Name = "lblSearchText";
            this.lblSearchText.Size = new System.Drawing.Size(196, 14);
            this.lblSearchText.TabIndex = 6;
            this.lblSearchText.Text = "Search Pattern (Regular Expression)";
            // 
            // lblFiles
            // 
            this.lblFiles.Location = new System.Drawing.Point(11, 54);
            this.lblFiles.Name = "lblFiles";
            this.lblFiles.Size = new System.Drawing.Size(84, 12);
            this.lblFiles.TabIndex = 4;
            this.lblFiles.Text = "File Extensions";
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Enabled = false;
            this.btnSearch.Location = new System.Drawing.Point(423, 116);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(60, 24);
            this.btnSearch.TabIndex = 13;
            this.btnSearch.Text = "Search";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // lblDir
            // 
            this.lblDir.Location = new System.Drawing.Point(11, 9);
            this.lblDir.Name = "lblDir";
            this.lblDir.Size = new System.Drawing.Size(60, 15);
            this.lblDir.TabIndex = 1;
            this.lblDir.Text = "Directory";
            // 
            // lbResults
            // 
            this.lbResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbResults.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lbResults.Location = new System.Drawing.Point(11, 204);
            this.lbResults.Name = "lbResults";
            this.lbResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbResults.Size = new System.Drawing.Size(472, 212);
            this.lbResults.TabIndex = 17;
            this.lbResults.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbResults_DrawItem);
            this.lbResults.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lbResults_MeasureItem);
            this.lbResults.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbResults_KeyDown);
            this.lbResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbResults_MouseDoubleClick);
            // 
            // checkedComboBox1
            // 
            this.checkedComboBox1.CheckOnClick = true;
            this.checkedComboBox1.DisplayMember = "Name";
            this.checkedComboBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.checkedComboBox1.DropDownHeight = 1;
            this.checkedComboBox1.FormattingEnabled = true;
            this.checkedComboBox1.IntegralHeight = false;
            this.checkedComboBox1.Location = new System.Drawing.Point(11, 68);
            this.checkedComboBox1.Name = "checkedComboBox1";
            this.checkedComboBox1.Size = new System.Drawing.Size(186, 21);
            this.checkedComboBox1.TabIndex = 5;
            this.checkedComboBox1.ValueSeparator = ", ";
            this.checkedComboBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedComboBox1_ItemCheck);
            this.checkedComboBox1.DropDown += new System.EventHandler(this.checkedComboBox1_DropDown);
            // 
            // ckJustFiles
            // 
            this.ckJustFiles.Checked = global::WinGrep.Properties.Settings.Default.FilesOnly;
            this.ckJustFiles.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGrep.Properties.Settings.Default, "FilesOnly", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ckJustFiles.Location = new System.Drawing.Point(207, 95);
            this.ckJustFiles.Name = "ckJustFiles";
            this.ckJustFiles.Size = new System.Drawing.Size(84, 21);
            this.ckJustFiles.TabIndex = 10;
            this.ckJustFiles.Text = "Just Files";
            this.ckJustFiles.Click += new System.EventHandler(this.ckJustFiles_Click);
            // 
            // ckIgnoreCase
            // 
            this.ckIgnoreCase.Checked = global::WinGrep.Properties.Settings.Default.IgnoreCase;
            this.ckIgnoreCase.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGrep.Properties.Settings.Default, "IgnoreCase", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ckIgnoreCase.Location = new System.Drawing.Point(103, 122);
            this.ckIgnoreCase.Name = "ckIgnoreCase";
            this.ckIgnoreCase.Size = new System.Drawing.Size(96, 18);
            this.ckIgnoreCase.TabIndex = 12;
            this.ckIgnoreCase.Text = "Ignore Case";
            // 
            // ckLineNumbers
            // 
            this.ckLineNumbers.Checked = global::WinGrep.Properties.Settings.Default.IncludeLineNumbers;
            this.ckLineNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckLineNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGrep.Properties.Settings.Default, "IncludeLineNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ckLineNumbers.Location = new System.Drawing.Point(103, 95);
            this.ckLineNumbers.Name = "ckLineNumbers";
            this.ckLineNumbers.Size = new System.Drawing.Size(96, 21);
            this.ckLineNumbers.TabIndex = 9;
            this.ckLineNumbers.Text = "Line Numbers";
            // 
            // ckRecursive
            // 
            this.ckRecursive.Checked = global::WinGrep.Properties.Settings.Default.IsRecursive;
            this.ckRecursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckRecursive.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGrep.Properties.Settings.Default, "IsRecursive", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ckRecursive.Location = new System.Drawing.Point(11, 95);
            this.ckRecursive.Name = "ckRecursive";
            this.ckRecursive.Size = new System.Drawing.Size(84, 21);
            this.ckRecursive.TabIndex = 8;
            this.ckRecursive.Text = "Recursive";
            // 
            // txtSearchText
            // 
            this.txtSearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchText.BackColor = System.Drawing.SystemColors.Window;
            this.txtSearchText.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::WinGrep.Properties.Settings.Default, "SearchPattern", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtSearchText.Location = new System.Drawing.Point(203, 68);
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.Size = new System.Drawing.Size(280, 20);
            this.txtSearchText.TabIndex = 7;
            this.txtSearchText.Text = global::WinGrep.Properties.Settings.Default.SearchPattern;
            this.txtSearchText.TextChanged += new System.EventHandler(this.tb_TextChanged_Verify);
            this.txtSearchText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_KeyDown_Search);
            // 
            // ckCountLines
            // 
            this.ckCountLines.Checked = global::WinGrep.Properties.Settings.Default.IncludeLineCount;
            this.ckCountLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckCountLines.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WinGrep.Properties.Settings.Default, "IncludeLineCount", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ckCountLines.Location = new System.Drawing.Point(11, 122);
            this.ckCountLines.Name = "ckCountLines";
            this.ckCountLines.Size = new System.Drawing.Size(84, 18);
            this.ckCountLines.TabIndex = 11;
            this.ckCountLines.Text = "Count Lines";
            // 
            // txtDir
            // 
            this.txtDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDir.BackColor = System.Drawing.SystemColors.Window;
            this.txtDir.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::WinGrep.Properties.Settings.Default, "SearchDirectory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtDir.Location = new System.Drawing.Point(11, 24);
            this.txtDir.Name = "txtDir";
            this.txtDir.Size = new System.Drawing.Size(436, 20);
            this.txtDir.TabIndex = 2;
            this.txtDir.Text = global::WinGrep.Properties.Settings.Default.SearchDirectory;
            this.txtDir.TextChanged += new System.EventHandler(this.tb_TextChanged_Verify);
            this.txtDir.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_KeyDown_Search);
            // 
            // FormWinGrep
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(494, 434);
            this.Controls.Add(this.lbResults);
            this.Controls.Add(this.checkedComboBox1);
            this.Controls.Add(this.ckJustFiles);
            this.Controls.Add(this.ckIgnoreCase);
            this.Controls.Add(this.ckLineNumbers);
            this.Controls.Add(this.ckRecursive);
            this.Controls.Add(this.lblCurFile);
            this.Controls.Add(this.txtCurFile);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblResults);
            this.Controls.Add(this.lblSearchText);
            this.Controls.Add(this.txtSearchText);
            this.Controls.Add(this.lblFiles);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.ckCountLines);
            this.Controls.Add(this.lblDir);
            this.Controls.Add(this.txtDir);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(402, 473);
            this.Name = "FormWinGrep";
            this.Text = "Win Grep";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWinGrep_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ckJustFiles;
        private System.Windows.Forms.CheckBox ckIgnoreCase;
        private System.Windows.Forms.CheckBox ckLineNumbers;
        private System.Windows.Forms.CheckBox ckRecursive;
        private System.Windows.Forms.Label lblCurFile;
        private System.Windows.Forms.TextBox txtCurFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.Label lblSearchText;
        private System.Windows.Forms.TextBox txtSearchText;
        private System.Windows.Forms.Label lblFiles;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.CheckBox ckCountLines;
        private System.Windows.Forms.Label lblDir;
        private System.Windows.Forms.TextBox txtDir;
        private CheckComboBoxControl.CheckedComboBox checkedComboBox1;
        private System.Windows.Forms.ListBox lbResults;

    }
}