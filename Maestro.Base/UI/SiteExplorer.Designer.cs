namespace Maestro.Base.UI
{
    partial class SiteExplorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SiteExplorer));
            this.tsSiteExplorer = new System.Windows.Forms.ToolStrip();
            this.trvResources = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // tsSiteExplorer
            // 
            resources.ApplyResources(this.tsSiteExplorer, "tsSiteExplorer");
            this.tsSiteExplorer.Name = "tsSiteExplorer";
            // 
            // trvResources
            // 
            this.trvResources.AllowDrop = true;
            this.trvResources.BackColor = System.Drawing.SystemColors.Window;
            this.trvResources.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.trvResources.HideSelection = false;
            resources.ApplyResources(this.trvResources, "trvResources");
            this.trvResources.Name = "trvResources";
            this.trvResources.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.trvResources_ItemDrag);
            this.trvResources.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvResources_AfterSelect);
            this.trvResources.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.trvResources_BeforeExpand);
            this.trvResources.DragDrop += new System.Windows.Forms.DragEventHandler(this.trvResources_DragDrop);
            this.trvResources.DragEnter += new System.Windows.Forms.DragEventHandler(this.trvResources_DragEnter);
            this.trvResources.DragOver += new System.Windows.Forms.DragEventHandler(this.trvResources_DragOver);
            this.trvResources.KeyDown += new System.Windows.Forms.KeyEventHandler(this.trvResources_KeyDown);
            this.trvResources.KeyUp += new System.Windows.Forms.KeyEventHandler(this.trvResources_KeyUp);
            this.trvResources.MouseClick += new System.Windows.Forms.MouseEventHandler(this.trvResources_MouseClick);
            this.trvResources.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.trvResources_MouseDoubleClick);
            // 
            // SiteExplorer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.trvResources);
            this.Controls.Add(this.tsSiteExplorer);
            this.Name = "SiteExplorer";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsSiteExplorer;
        private System.Windows.Forms.TreeView trvResources;


    }
}
