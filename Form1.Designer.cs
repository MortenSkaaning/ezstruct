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
            this.components = new System.ComponentModel.Container();
            this.openPdbDialog = new System.Windows.Forms.OpenFileDialog();
            this.overViewGrid = new System.Windows.Forms.DataGridView();
            this.compilerDataView = new System.Windows.Forms.DataGridView();
            this.text_compilerLayoutTotals = new System.Windows.Forms.TextBox();
            this.computedLayoutView = new System.Windows.Forms.DataGridView();
            this.text_generatedLayoutTotals = new System.Windows.Forms.TextBox();
            this.text_Warnings = new System.Windows.Forms.TextBox();
            this.fieldsDetailView = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.chk_createCompiledLayout = new System.Windows.Forms.CheckBox();
            this.chk_createGenerateLayout = new System.Windows.Forms.CheckBox();
            this.chk_compiledLayoutPadding = new System.Windows.Forms.CheckBox();
            this.chk_generateLayoutPadding = new System.Windows.Forms.CheckBox();
            this.overViewBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.text_overViewFilter = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.overViewGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.compilerDataView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.computedLayoutView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldsDetailView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overViewBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // openPdbDialog
            // 
            this.openPdbDialog.Filter = "Symbol files|*.pdb|All files|*.*";
            this.openPdbDialog.RestoreDirectory = true;
            // 
            // overViewGrid
            // 
            this.overViewGrid.AllowUserToAddRows = false;
            this.overViewGrid.AllowUserToDeleteRows = false;
            this.overViewGrid.AllowUserToResizeRows = false;
            this.overViewGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.overViewGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.overViewGrid.Location = new System.Drawing.Point(12, 25);
            this.overViewGrid.MultiSelect = false;
            this.overViewGrid.Name = "overViewGrid";
            this.overViewGrid.ReadOnly = true;
            this.overViewGrid.RowHeadersVisible = false;
            this.overViewGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.overViewGrid.ShowEditingIcon = false;
            this.overViewGrid.Size = new System.Drawing.Size(641, 347);
            this.overViewGrid.TabIndex = 1;
            this.overViewGrid.SelectionChanged += new System.EventHandler(this.overViewGrid_SelectionChanged);
            // 
            // compilerDataView
            // 
            this.compilerDataView.AllowUserToAddRows = false;
            this.compilerDataView.AllowUserToDeleteRows = false;
            this.compilerDataView.AllowUserToResizeRows = false;
            this.compilerDataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.compilerDataView.Location = new System.Drawing.Point(659, 25);
            this.compilerDataView.Name = "compilerDataView";
            this.compilerDataView.ReadOnly = true;
            this.compilerDataView.RowHeadersVisible = false;
            this.compilerDataView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.compilerDataView.ShowEditingIcon = false;
            this.compilerDataView.Size = new System.Drawing.Size(468, 347);
            this.compilerDataView.TabIndex = 2;
            this.compilerDataView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.PaintStructDetailViews);
            // 
            // text_compilerLayoutTotals
            // 
            this.text_compilerLayoutTotals.Location = new System.Drawing.Point(792, 376);
            this.text_compilerLayoutTotals.Name = "text_compilerLayoutTotals";
            this.text_compilerLayoutTotals.ReadOnly = true;
            this.text_compilerLayoutTotals.Size = new System.Drawing.Size(267, 20);
            this.text_compilerLayoutTotals.TabIndex = 3;
            // 
            // computedLayoutView
            // 
            this.computedLayoutView.AllowUserToAddRows = false;
            this.computedLayoutView.AllowUserToDeleteRows = false;
            this.computedLayoutView.AllowUserToResizeRows = false;
            this.computedLayoutView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.computedLayoutView.Location = new System.Drawing.Point(1133, 25);
            this.computedLayoutView.Name = "computedLayoutView";
            this.computedLayoutView.ReadOnly = true;
            this.computedLayoutView.RowHeadersVisible = false;
            this.computedLayoutView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.computedLayoutView.ShowEditingIcon = false;
            this.computedLayoutView.Size = new System.Drawing.Size(416, 347);
            this.computedLayoutView.TabIndex = 4;
            this.computedLayoutView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.PaintStructDetailViews);
            // 
            // text_generatedLayoutTotals
            // 
            this.text_generatedLayoutTotals.Location = new System.Drawing.Point(1265, 376);
            this.text_generatedLayoutTotals.Name = "text_generatedLayoutTotals";
            this.text_generatedLayoutTotals.ReadOnly = true;
            this.text_generatedLayoutTotals.Size = new System.Drawing.Size(267, 20);
            this.text_generatedLayoutTotals.TabIndex = 5;
            // 
            // text_Warnings
            // 
            this.text_Warnings.Location = new System.Drawing.Point(1144, 422);
            this.text_Warnings.Multiline = true;
            this.text_Warnings.Name = "text_Warnings";
            this.text_Warnings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.text_Warnings.Size = new System.Drawing.Size(405, 328);
            this.text_Warnings.TabIndex = 6;
            this.text_Warnings.WordWrap = false;
            // 
            // fieldsDetailView
            // 
            this.fieldsDetailView.AllowUserToAddRows = false;
            this.fieldsDetailView.AllowUserToResizeRows = false;
            this.fieldsDetailView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.fieldsDetailView.Location = new System.Drawing.Point(12, 422);
            this.fieldsDetailView.Name = "fieldsDetailView";
            this.fieldsDetailView.ReadOnly = true;
            this.fieldsDetailView.RowHeadersVisible = false;
            this.fieldsDetailView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.fieldsDetailView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.fieldsDetailView.Size = new System.Drawing.Size(1126, 328);
            this.fieldsDetailView.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(656, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Compiled layout";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1130, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Generated layout";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 406);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Field details";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Symbols";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1141, 406);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Warnings";
            // 
            // chk_createCompiledLayout
            // 
            this.chk_createCompiledLayout.AutoSize = true;
            this.chk_createCompiledLayout.Checked = true;
            this.chk_createCompiledLayout.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_createCompiledLayout.Location = new System.Drawing.Point(658, 378);
            this.chk_createCompiledLayout.Name = "chk_createCompiledLayout";
            this.chk_createCompiledLayout.Size = new System.Drawing.Size(57, 17);
            this.chk_createCompiledLayout.TabIndex = 13;
            this.chk_createCompiledLayout.Text = "Create";
            this.chk_createCompiledLayout.UseVisualStyleBackColor = true;
            this.chk_createCompiledLayout.CheckedChanged += new System.EventHandler(this.chk_createCompiledLayout_CheckedChanged);
            // 
            // chk_createGenerateLayout
            // 
            this.chk_createGenerateLayout.AutoSize = true;
            this.chk_createGenerateLayout.Checked = true;
            this.chk_createGenerateLayout.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_createGenerateLayout.Location = new System.Drawing.Point(1133, 378);
            this.chk_createGenerateLayout.Name = "chk_createGenerateLayout";
            this.chk_createGenerateLayout.Size = new System.Drawing.Size(57, 17);
            this.chk_createGenerateLayout.TabIndex = 14;
            this.chk_createGenerateLayout.Text = "Create";
            this.chk_createGenerateLayout.UseVisualStyleBackColor = true;
            this.chk_createGenerateLayout.CheckedChanged += new System.EventHandler(this.chk_createGenerateLayout_CheckedChanged);
            // 
            // chk_compiledLayoutPadding
            // 
            this.chk_compiledLayoutPadding.AutoSize = true;
            this.chk_compiledLayoutPadding.Checked = true;
            this.chk_compiledLayoutPadding.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_compiledLayoutPadding.Location = new System.Drawing.Point(721, 378);
            this.chk_compiledLayoutPadding.Name = "chk_compiledLayoutPadding";
            this.chk_compiledLayoutPadding.Size = new System.Drawing.Size(65, 17);
            this.chk_compiledLayoutPadding.TabIndex = 15;
            this.chk_compiledLayoutPadding.Text = "Padding";
            this.chk_compiledLayoutPadding.UseVisualStyleBackColor = true;
            this.chk_compiledLayoutPadding.CheckedChanged += new System.EventHandler(this.chk_compiledLayoutPadding_CheckedChanged);
            // 
            // chk_generateLayoutPadding
            // 
            this.chk_generateLayoutPadding.AutoSize = true;
            this.chk_generateLayoutPadding.Checked = true;
            this.chk_generateLayoutPadding.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_generateLayoutPadding.Location = new System.Drawing.Point(1194, 378);
            this.chk_generateLayoutPadding.Name = "chk_generateLayoutPadding";
            this.chk_generateLayoutPadding.Size = new System.Drawing.Size(65, 17);
            this.chk_generateLayoutPadding.TabIndex = 16;
            this.chk_generateLayoutPadding.Text = "Padding";
            this.chk_generateLayoutPadding.UseVisualStyleBackColor = true;
            this.chk_generateLayoutPadding.CheckedChanged += new System.EventHandler(this.chk_generateLayoutPadding_CheckedChanged);
            // 
            // text_overViewFilter
            // 
            this.text_overViewFilter.Location = new System.Drawing.Point(318, 3);
            this.text_overViewFilter.Name = "text_overViewFilter";
            this.text_overViewFilter.Size = new System.Drawing.Size(335, 20);
            this.text_overViewFilter.TabIndex = 17;
            this.text_overViewFilter.TextChanged += new System.EventHandler(this.text_overViewFilter_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(283, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Filter";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1557, 755);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.text_overViewFilter);
            this.Controls.Add(this.chk_generateLayoutPadding);
            this.Controls.Add(this.chk_compiledLayoutPadding);
            this.Controls.Add(this.chk_createGenerateLayout);
            this.Controls.Add(this.chk_createCompiledLayout);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.fieldsDetailView);
            this.Controls.Add(this.text_Warnings);
            this.Controls.Add(this.text_generatedLayoutTotals);
            this.Controls.Add(this.computedLayoutView);
            this.Controls.Add(this.text_compilerLayoutTotals);
            this.Controls.Add(this.compilerDataView);
            this.Controls.Add(this.overViewGrid);
            this.Name = "Form1";
            this.Text = "ezstruct";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.overViewGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.compilerDataView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.computedLayoutView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldsDetailView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overViewBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openPdbDialog;

        private System.Windows.Forms.DataGridView overViewGrid;
        private System.Windows.Forms.DataGridView compilerDataView;
        private System.Windows.Forms.DataGridView computedLayoutView;
        
        private System.Windows.Forms.TextBox text_compilerLayoutTotals;
        private System.Windows.Forms.TextBox text_generatedLayoutTotals;
        private System.Windows.Forms.TextBox text_Warnings;
        private System.Windows.Forms.DataGridView fieldsDetailView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chk_createCompiledLayout;
        private System.Windows.Forms.CheckBox chk_createGenerateLayout;
        private System.Windows.Forms.CheckBox chk_compiledLayoutPadding;
        private System.Windows.Forms.CheckBox chk_generateLayoutPadding;
        private System.Windows.Forms.BindingSource overViewBindingSource;
        private System.Windows.Forms.TextBox text_overViewFilter;
        private System.Windows.Forms.Label label6;
    }
}

