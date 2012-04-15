namespace ezstruct
{
    partial class Form1
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
            this.openPdbDialog = new System.Windows.Forms.OpenFileDialog();
            this.overViewGrid = new System.Windows.Forms.DataGridView();
            this.compilerDataView = new System.Windows.Forms.DataGridView();
            this.text_compilerDataTotals = new System.Windows.Forms.TextBox();
            this.computedLayoutView = new System.Windows.Forms.DataGridView();
            this.text_computedDataTotals = new System.Windows.Forms.TextBox();
            this.text_Warnings = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.overViewGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.compilerDataView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.computedLayoutView)).BeginInit();
            this.SuspendLayout();
            // 
            // overViewGrid
            // 
            this.overViewGrid.AllowUserToAddRows = false;
            this.overViewGrid.AllowUserToDeleteRows = false;
            this.overViewGrid.AllowUserToResizeRows = false;
            this.overViewGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.overViewGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.overViewGrid.Location = new System.Drawing.Point(15, 17);
            this.overViewGrid.MultiSelect = false;
            this.overViewGrid.Name = "overViewGrid";
            this.overViewGrid.ReadOnly = true;
            this.overViewGrid.RowHeadersVisible = false;
            this.overViewGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.overViewGrid.ShowEditingIcon = false;
            this.overViewGrid.Size = new System.Drawing.Size(473, 482);
            this.overViewGrid.TabIndex = 1;
            this.overViewGrid.SelectionChanged += new System.EventHandler(this.overViewGrid_SelectionChanged);
            // 
            // compilerDataView
            // 
            this.compilerDataView.AllowUserToAddRows = false;
            this.compilerDataView.AllowUserToDeleteRows = false;
            this.compilerDataView.AllowUserToResizeRows = false;
            this.compilerDataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.compilerDataView.Location = new System.Drawing.Point(494, 17);
            this.compilerDataView.Name = "compilerDataView";
            this.compilerDataView.ReadOnly = true;
            this.compilerDataView.RowHeadersVisible = false;
            this.compilerDataView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.compilerDataView.ShowEditingIcon = false;
            this.compilerDataView.Size = new System.Drawing.Size(535, 347);
            this.compilerDataView.TabIndex = 2;
            this.compilerDataView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.PaintStructDetailViews);
            // 
            // text_compilerDataTotals
            // 
            this.text_compilerDataTotals.Location = new System.Drawing.Point(494, 370);
            this.text_compilerDataTotals.Name = "text_compilerDataTotals";
            this.text_compilerDataTotals.ReadOnly = true;
            this.text_compilerDataTotals.Size = new System.Drawing.Size(267, 20);
            this.text_compilerDataTotals.TabIndex = 3;
            // 
            // computedLayoutView
            // 
            this.computedLayoutView.AllowUserToAddRows = false;
            this.computedLayoutView.AllowUserToDeleteRows = false;
            this.computedLayoutView.AllowUserToResizeRows = false;
            this.computedLayoutView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.computedLayoutView.Location = new System.Drawing.Point(1036, 17);
            this.computedLayoutView.Name = "computedLayoutView";
            this.computedLayoutView.ReadOnly = true;
            this.computedLayoutView.RowHeadersVisible = false;
            this.computedLayoutView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.computedLayoutView.ShowEditingIcon = false;
            this.computedLayoutView.Size = new System.Drawing.Size(514, 347);
            this.computedLayoutView.TabIndex = 4;
            this.computedLayoutView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.PaintStructDetailViews);
            // 
            // text_computedDataTotals
            // 
            this.text_computedDataTotals.Location = new System.Drawing.Point(1035, 370);
            this.text_computedDataTotals.Name = "text_computedDataTotals";
            this.text_computedDataTotals.ReadOnly = true;
            this.text_computedDataTotals.Size = new System.Drawing.Size(267, 20);
            this.text_computedDataTotals.TabIndex = 5;
            // 
            // text_Warnings
            // 
            this.text_Warnings.Location = new System.Drawing.Point(501, 406);
            this.text_Warnings.Multiline = true;
            this.text_Warnings.Name = "text_Warnings";
            this.text_Warnings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.text_Warnings.Size = new System.Drawing.Size(512, 127);
            this.text_Warnings.TabIndex = 6;
            this.text_Warnings.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1594, 548);
            this.Controls.Add(this.text_Warnings);
            this.Controls.Add(this.text_computedDataTotals);
            this.Controls.Add(this.computedLayoutView);
            this.Controls.Add(this.text_compilerDataTotals);
            this.Controls.Add(this.compilerDataView);
            this.Controls.Add(this.overViewGrid);
            this.Name = "Form1";
            this.Text = "ezstruct";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.overViewGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.compilerDataView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.computedLayoutView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openPdbDialog;

        private System.Windows.Forms.DataGridView overViewGrid;
        private System.Windows.Forms.DataGridView compilerDataView;
        private System.Windows.Forms.DataGridView computedLayoutView;
        
        private System.Windows.Forms.TextBox text_compilerDataTotals;
        private System.Windows.Forms.TextBox text_computedDataTotals;
        private System.Windows.Forms.TextBox text_Warnings;
    }
}

