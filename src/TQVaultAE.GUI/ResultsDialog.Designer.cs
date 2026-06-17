//-----------------------------------------------------------------------
// <copyright file="ResultsDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	/// <summary>
	/// Results dialog form designer class
	/// </summary>
	public partial class ResultsDialog
	{
		/// <summary>
		/// Data Grid View for displaying the search results
		/// </summary>
		private System.Windows.Forms.DataGridView resultsDataGridView;

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
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.resultsDataGridView = new System.Windows.Forms.DataGridView();
            this.columnItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnQuality = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnContainerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnContainerType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.resultsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // resultsDataGridView
            // 
            this.resultsDataGridView.AllowUserToAddRows = false;
            this.resultsDataGridView.AllowUserToDeleteRows = false;
            this.resultsDataGridView.AllowUserToResizeRows = false;
            this.resultsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.resultsDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            this.resultsDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.resultsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Gold;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.resultsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.resultsDataGridView.ColumnHeadersHeight = 24;
            this.resultsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.resultsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnItem,
            this.columnQuality,
            this.columnContainerName,
            this.columnContainerType,
            this.columnLevel});
            this.resultsDataGridView.GridColor = System.Drawing.Color.Khaki;
            this.resultsDataGridView.Location = new System.Drawing.Point(10, 35);
            this.resultsDataGridView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.resultsDataGridView.MultiSelect = false;
            this.resultsDataGridView.Name = "resultsDataGridView";
            this.resultsDataGridView.ReadOnly = true;
            this.resultsDataGridView.RowHeadersVisible = false;
            this.resultsDataGridView.RowHeadersWidth = 51;
            this.resultsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.resultsDataGridView.ShowCellToolTips = false;
            this.resultsDataGridView.ShowEditingIcon = false;
            this.resultsDataGridView.Size = new System.Drawing.Size(1026, 564);
            this.resultsDataGridView.TabIndex = 1;
			this.resultsDataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsDataGridViewCellClick);
			this.resultsDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsDataGridViewCellDoubleClick);
			this.resultsDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsDataGridViewCellMouseEnter);
            this.resultsDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsDataGridViewCellMouseLeave);
            this.resultsDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsDataGridViewRowEnter);
            this.resultsDataGridView.RowLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsDataGridViewRowLeave);
            this.resultsDataGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ResultsDataGridViewKeyPress);
            this.resultsDataGridView.MouseLeave += new System.EventHandler(this.ResultsDataGridViewMouseLeave);
            // 
            // columnItem
            // 
            this.columnItem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Gold;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            this.columnItem.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnItem.HeaderText = "Item";
            this.columnItem.MinimumWidth = 6;
            this.columnItem.Name = "columnItem";
            this.columnItem.ReadOnly = true;
            this.columnItem.Width = 65;
            // 
            // columnQuality
            // 
            this.columnQuality.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.Gold;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            this.columnQuality.DefaultCellStyle = dataGridViewCellStyle3;
            this.columnQuality.HeaderText = "Quality";
            this.columnQuality.MinimumWidth = 6;
            this.columnQuality.Name = "columnQuality";
            this.columnQuality.ReadOnly = true;
            this.columnQuality.Width = 82;
            // 
            // columnContainerName
            // 
            this.columnContainerName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.Gold;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            this.columnContainerName.DefaultCellStyle = dataGridViewCellStyle4;
            this.columnContainerName.HeaderText = "Container";
            this.columnContainerName.MinimumWidth = 6;
            this.columnContainerName.Name = "columnContainerName";
            this.columnContainerName.ReadOnly = true;
            this.columnContainerName.Width = 101;
            // 
            // columnContainerType
            // 
            this.columnContainerType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.Gold;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.Black;
            this.columnContainerType.DefaultCellStyle = dataGridViewCellStyle5;
            this.columnContainerType.HeaderText = "ContainerType";
            this.columnContainerType.MinimumWidth = 6;
            this.columnContainerType.Name = "columnContainerType";
            this.columnContainerType.ReadOnly = true;
            this.columnContainerType.Width = 133;
            // 
            // columnLevel
            //
            this.columnLevel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.Gold;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black;
            this.columnLevel.DefaultCellStyle = dataGridViewCellStyle6;
            this.columnLevel.HeaderText = "Level";
            this.columnLevel.MinimumWidth = 6;
            this.columnLevel.Name = "columnLevel";
            this.columnLevel.ReadOnly = true;
            this.columnLevel.Width = 71;
            // 
            // ResultsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(1046, 621);
            this.Controls.Add(this.resultsDataGridView);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResultsDialog";
            this.OriginalFormSize = new System.Drawing.Size(837, 497);
            this.Padding = new System.Windows.Forms.Padding(10, 35, 10, 22);
            this.ResizeCustomAllowed = true;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Results";
            this.Shown += new System.EventHandler(this.ResultsDialogShown);
            this.Resize += new System.EventHandler(this.ResultsDialog_Resize);
            this.Controls.SetChildIndex(this.resultsDataGridView, 0);
            ((System.ComponentModel.ISupportInitialize)(this.resultsDataGridView)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridViewTextBoxColumn columnItem;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnQuality;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnContainerName;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnContainerType;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnLevel;
	}
}