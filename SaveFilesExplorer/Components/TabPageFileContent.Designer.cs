namespace SaveFilesExplorer.Components
{
	partial class TabPageFileContent
	{
		/// <summary> 
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur de composants

		/// <summary> 
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBoxDetectedKeys = new System.Windows.Forms.GroupBox();
            this.treeViewKeys = new System.Windows.Forms.TreeView();
            this.groupBoxKeyData = new System.Windows.Forms.GroupBox();
            this.groupBoxKeyInfos = new System.Windows.Forms.GroupBox();
            this.groupBoxFileInfos = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelFileInfos = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabelFilePath = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxDetectedKeys.SuspendLayout();
            this.groupBoxFileInfos.SuspendLayout();
            this.flowLayoutPanelFileInfos.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 89);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxDetectedKeys);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxKeyData);
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxKeyInfos);
            this.splitContainer1.Size = new System.Drawing.Size(705, 636);
            this.splitContainer1.SplitterDistance = 235;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 12;
            // 
            // groupBoxDetectedKeys
            // 
            this.groupBoxDetectedKeys.Controls.Add(this.treeViewKeys);
            this.groupBoxDetectedKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDetectedKeys.Location = new System.Drawing.Point(0, 0);
            this.groupBoxDetectedKeys.Name = "groupBoxDetectedKeys";
            this.groupBoxDetectedKeys.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxDetectedKeys.Size = new System.Drawing.Size(235, 636);
            this.groupBoxDetectedKeys.TabIndex = 9;
            this.groupBoxDetectedKeys.TabStop = false;
            this.groupBoxDetectedKeys.Text = "Detected Keys";
            // 
            // treeViewKeys
            // 
            this.treeViewKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewKeys.Location = new System.Drawing.Point(10, 25);
            this.treeViewKeys.Name = "treeViewKeys";
            this.treeViewKeys.Size = new System.Drawing.Size(215, 601);
            this.treeViewKeys.TabIndex = 2;
            // 
            // groupBoxKeyData
            // 
            this.groupBoxKeyData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxKeyData.Location = new System.Drawing.Point(0, 105);
            this.groupBoxKeyData.Name = "groupBoxKeyData";
            this.groupBoxKeyData.Size = new System.Drawing.Size(460, 531);
            this.groupBoxKeyData.TabIndex = 6;
            this.groupBoxKeyData.TabStop = false;
            this.groupBoxKeyData.Text = "Key Data";
            // 
            // groupBoxKeyInfos
            // 
            this.groupBoxKeyInfos.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxKeyInfos.Location = new System.Drawing.Point(0, 0);
            this.groupBoxKeyInfos.Name = "groupBoxKeyInfos";
            this.groupBoxKeyInfos.Size = new System.Drawing.Size(460, 105);
            this.groupBoxKeyInfos.TabIndex = 5;
            this.groupBoxKeyInfos.TabStop = false;
            this.groupBoxKeyInfos.Text = "Key Infos";
            // 
            // groupBoxFileInfos
            // 
            this.groupBoxFileInfos.Controls.Add(this.flowLayoutPanelFileInfos);
            this.groupBoxFileInfos.Controls.Add(this.linkLabelFilePath);
            this.groupBoxFileInfos.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxFileInfos.Location = new System.Drawing.Point(0, 0);
            this.groupBoxFileInfos.Name = "groupBoxFileInfos";
            this.groupBoxFileInfos.Size = new System.Drawing.Size(705, 89);
            this.groupBoxFileInfos.TabIndex = 11;
            this.groupBoxFileInfos.TabStop = false;
            this.groupBoxFileInfos.Text = "File Infos";
            // 
            // flowLayoutPanelFileInfos
            // 
            this.flowLayoutPanelFileInfos.Controls.Add(this.label1);
            this.flowLayoutPanelFileInfos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelFileInfos.Location = new System.Drawing.Point(3, 35);
            this.flowLayoutPanelFileInfos.Name = "flowLayoutPanelFileInfos";
            this.flowLayoutPanelFileInfos.Size = new System.Drawing.Size(699, 51);
            this.flowLayoutPanelFileInfos.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // linkLabelFilePath
            // 
            this.linkLabelFilePath.AutoSize = true;
            this.linkLabelFilePath.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkLabelFilePath.Location = new System.Drawing.Point(3, 18);
            this.linkLabelFilePath.Name = "linkLabelFilePath";
            this.linkLabelFilePath.Size = new System.Drawing.Size(115, 17);
            this.linkLabelFilePath.TabIndex = 0;
            this.linkLabelFilePath.TabStop = true;
            this.linkLabelFilePath.Text = "linkLabelFilePath";
            // 
            // TabPageContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBoxFileInfos);
            this.Name = "TabPageContent";
            this.Size = new System.Drawing.Size(705, 725);
            this.Load += new System.EventHandler(this.TabPageContent_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxDetectedKeys.ResumeLayout(false);
            this.groupBoxFileInfos.ResumeLayout(false);
            this.groupBoxFileInfos.PerformLayout();
            this.flowLayoutPanelFileInfos.ResumeLayout(false);
            this.flowLayoutPanelFileInfos.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.GroupBox groupBoxDetectedKeys;
		private System.Windows.Forms.TreeView treeViewKeys;
		private System.Windows.Forms.GroupBox groupBoxKeyData;
		private System.Windows.Forms.GroupBox groupBoxKeyInfos;
		private System.Windows.Forms.GroupBox groupBoxFileInfos;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelFileInfos;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel linkLabelFilePath;
	}
}
