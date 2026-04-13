namespace Maestro.Editors.WebLayout
{
    partial class WebLayoutMenusCtrl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebLayoutMenusCtrl));            this.grdCommands = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TAB_CONTEXT_MENU = new System.Windows.Forms.TabPage();
            this.edContextMenu = new Maestro.Editors.WebLayout.MenuEditorCtrl();            this.TAB_TOOLBAR = new System.Windows.Forms.TabPage();
            this.edToolbar = new Maestro.Editors.WebLayout.MenuEditorCtrl();
            this.tabMenus = new System.Windows.Forms.TabControl();
            this.TAB_TASK_MENU = new System.Windows.Forms.TabPage();
            this.edTaskMenu = new Maestro.Editors.WebLayout.MenuEditorCtrl();
            this.btnAddFromCmdSet = new System.Windows.Forms.Button();
            this.contentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdCommands)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.TAB_CONTEXT_MENU.SuspendLayout();
            this.TAB_TOOLBAR.SuspendLayout();
            this.tabMenus.SuspendLayout();
            this.TAB_TASK_MENU.SuspendLayout();
            this.SuspendLayout();
            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.btnAddFromCmdSet);
            this.contentPanel.Controls.Add(this.groupBox1);
            this.contentPanel.Controls.Add(this.tabMenus);
            resources.ApplyResources(this.contentPanel, "contentPanel");
            // 
            // nodeIcon3
            //            // 
            // nodeTextBox3
            //            // 
            // nodeIcon1
            //            // 
            // nodeTextBox1
            //            // 
            // nodeIcon2
            //            // 
            // nodeTextBox2
            //            // 
            // grdCommands
            // 
            this.grdCommands.AllowUserToAddRows = false;
            this.grdCommands.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.grdCommands, "grdCommands");
            this.grdCommands.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdCommands.Name = "grdCommands";
            this.grdCommands.ReadOnly = true;
            this.grdCommands.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdCommands_CellClick);
            this.grdCommands.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdCommands_CellClick);
            this.grdCommands.DragLeave += new System.EventHandler(this.grdCommands_DragLeave);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.grdCommands);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // TAB_CONTEXT_MENU
            // 
            this.TAB_CONTEXT_MENU.Controls.Add(this.edContextMenu);
            resources.ApplyResources(this.TAB_CONTEXT_MENU, "TAB_CONTEXT_MENU");
            this.TAB_CONTEXT_MENU.Name = "TAB_CONTEXT_MENU";
            this.TAB_CONTEXT_MENU.UseVisualStyleBackColor = true;
            // 
            // edContextMenu
            // 
            resources.ApplyResources(this.edContextMenu, "edContextMenu");            this.edContextMenu.Name = "edContextMenu";
            // 
            // nodeIcon4
            //            // 
            // nodeTextBox4
            //            // 
            // TAB_TOOLBAR
            // 
            this.TAB_TOOLBAR.Controls.Add(this.edToolbar);
            resources.ApplyResources(this.TAB_TOOLBAR, "TAB_TOOLBAR");
            this.TAB_TOOLBAR.Name = "TAB_TOOLBAR";
            this.TAB_TOOLBAR.UseVisualStyleBackColor = true;
            // 
            // edToolbar
            // 
            resources.ApplyResources(this.edToolbar, "edToolbar");            this.edToolbar.Name = "edToolbar";
            // 
            // tabMenus
            // 
            resources.ApplyResources(this.tabMenus, "tabMenus");
            this.tabMenus.Controls.Add(this.TAB_TOOLBAR);
            this.tabMenus.Controls.Add(this.TAB_CONTEXT_MENU);
            this.tabMenus.Controls.Add(this.TAB_TASK_MENU);
            this.tabMenus.Name = "tabMenus";
            this.tabMenus.SelectedIndex = 0;
            // 
            // TAB_TASK_MENU
            // 
            this.TAB_TASK_MENU.Controls.Add(this.edTaskMenu);
            resources.ApplyResources(this.TAB_TASK_MENU, "TAB_TASK_MENU");
            this.TAB_TASK_MENU.Name = "TAB_TASK_MENU";
            this.TAB_TASK_MENU.UseVisualStyleBackColor = true;
            // 
            // edTaskMenu
            // 
            resources.ApplyResources(this.edTaskMenu, "edTaskMenu");            this.edTaskMenu.Name = "edTaskMenu";
            // 
            // btnAddFromCmdSet
            // 
            resources.ApplyResources(this.btnAddFromCmdSet, "btnAddFromCmdSet");
            this.btnAddFromCmdSet.Name = "btnAddFromCmdSet";
            this.btnAddFromCmdSet.UseVisualStyleBackColor = true;
            this.btnAddFromCmdSet.Click += new System.EventHandler(this.btnAddFromCmdSet_Click);
            // 
            // WebLayoutMenusCtrl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Name = "WebLayoutMenusCtrl";
            this.contentPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdCommands)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.TAB_CONTEXT_MENU.ResumeLayout(false);
            this.TAB_TOOLBAR.ResumeLayout(false);
            this.tabMenus.ResumeLayout(false);
            this.TAB_TASK_MENU.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grdCommands;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl tabMenus;
        private System.Windows.Forms.TabPage TAB_TOOLBAR;
        private System.Windows.Forms.TabPage TAB_CONTEXT_MENU;
        private System.Windows.Forms.TabPage TAB_TASK_MENU;
        private MenuEditorCtrl edToolbar;
        private MenuEditorCtrl edContextMenu;
        private MenuEditorCtrl edTaskMenu;
        private System.Windows.Forms.Button btnAddFromCmdSet;
    }
}


