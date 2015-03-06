namespace Swarm2D.WindowsDebugger
{
	partial class DebugPanelForm
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
            this._treeViewEntities = new System.Windows.Forms.TreeView();
            this._buttonReset = new System.Windows.Forms.Button();
            this._listBoxComponents = new System.Windows.Forms.ListBox();
            this._labelSelectedEntity = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _treeViewEntities
            // 
            this._treeViewEntities.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._treeViewEntities.Location = new System.Drawing.Point(12, 12);
            this._treeViewEntities.Name = "_treeViewEntities";
            this._treeViewEntities.Size = new System.Drawing.Size(424, 604);
            this._treeViewEntities.TabIndex = 0;
            this._treeViewEntities.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnEntitiesTreeViewNodeMouseClick);
            this._treeViewEntities.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnEntitiesTreeViewNodeMouseDoubleClick);
            // 
            // _buttonReset
            // 
            this._buttonReset.Location = new System.Drawing.Point(442, 12);
            this._buttonReset.Name = "_buttonReset";
            this._buttonReset.Size = new System.Drawing.Size(149, 25);
            this._buttonReset.TabIndex = 1;
            this._buttonReset.Text = "Reset";
            this._buttonReset.UseVisualStyleBackColor = true;
            this._buttonReset.Click += new System.EventHandler(this.OnResetButtonClick);
            // 
            // _listBoxComponents
            // 
            this._listBoxComponents.FormattingEnabled = true;
            this._listBoxComponents.Location = new System.Drawing.Point(442, 56);
            this._listBoxComponents.Name = "_listBoxComponents";
            this._listBoxComponents.Size = new System.Drawing.Size(271, 186);
            this._listBoxComponents.TabIndex = 2;
            // 
            // _labelSelectedEntity
            // 
            this._labelSelectedEntity.AutoSize = true;
            this._labelSelectedEntity.Location = new System.Drawing.Point(442, 40);
            this._labelSelectedEntity.Name = "_labelSelectedEntity";
            this._labelSelectedEntity.Size = new System.Drawing.Size(84, 13);
            this._labelSelectedEntity.TabIndex = 3;
            this._labelSelectedEntity.Text = "-Selected Entity-";
            // 
            // DebugPanelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 628);
            this.Controls.Add(this._labelSelectedEntity);
            this.Controls.Add(this._listBoxComponents);
            this.Controls.Add(this._buttonReset);
            this.Controls.Add(this._treeViewEntities);
            this.Name = "DebugPanelForm";
            this.Text = "Debug Panel";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TreeView _treeViewEntities;
		private System.Windows.Forms.Button _buttonReset;
        private System.Windows.Forms.ListBox _listBoxComponents;
        private System.Windows.Forms.Label _labelSelectedEntity;
	}
}

