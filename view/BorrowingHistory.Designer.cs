namespace StudentLibrary.view
{
    partial class BorrowingHistory
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewBorrowingHistory = new System.Windows.Forms.DataGridView();
            this.BookTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BorrowedOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DueDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Penalty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Action = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBorrowingHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewBorrowingHistory
            // 
            this.dataGridViewBorrowingHistory.AllowUserToAddRows = false;
            this.dataGridViewBorrowingHistory.AllowUserToDeleteRows = false;
            this.dataGridViewBorrowingHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewBorrowingHistory.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewBorrowingHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Teal;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewBorrowingHistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewBorrowingHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewBorrowingHistory.EnableHeadersVisualStyles = false;
            this.dataGridViewBorrowingHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.BookTitle,
            this.Author,
            this.BorrowedOn,
            this.DueDate,
            this.Status,
            this.Penalty,
            this.Action});
            this.dataGridViewBorrowingHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewBorrowingHistory.Name = "dataGridViewBorrowingHistory";
            this.dataGridViewBorrowingHistory.ReadOnly = false;
            this.dataGridViewBorrowingHistory.RowHeadersWidth = 51;
            this.dataGridViewBorrowingHistory.RowTemplate.Height = 35;
            this.dataGridViewBorrowingHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewBorrowingHistory.Size = new System.Drawing.Size(945, 629);
            this.dataGridViewBorrowingHistory.TabIndex = 0;
            this.dataGridViewBorrowingHistory.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewBorrowingHistory_CellClick);
            this.dataGridViewBorrowingHistory.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewBorrowingHistory_CellMouseEnter);
            this.dataGridViewBorrowingHistory.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewBorrowingHistory_CellMouseLeave);
            // 
            // BookTitle
            // 
            this.BookTitle.FillWeight = 120F;
            this.BookTitle.HeaderText = "Book Title";
            this.BookTitle.MinimumWidth = 6;
            this.BookTitle.Name = "BookTitle";
            this.BookTitle.ReadOnly = true;
            // 
            // Author
            // 
            this.Author.FillWeight = 100F;
            this.Author.HeaderText = "Author";
            this.Author.MinimumWidth = 6;
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            // 
            // BorrowedOn
            // 
            this.BorrowedOn.FillWeight = 90F;
            this.BorrowedOn.HeaderText = "Borrowed";
            this.BorrowedOn.MinimumWidth = 6;
            this.BorrowedOn.Name = "BorrowedOn";
            this.BorrowedOn.ReadOnly = true;
            // 
            // DueDate
            // 
            this.DueDate.FillWeight = 80F;
            this.DueDate.HeaderText = "Due Date";
            this.DueDate.MinimumWidth = 6;
            this.DueDate.Name = "DueDate";
            this.DueDate.ReadOnly = true;
            // 
            // Status
            // 
            this.Status.FillWeight = 70F;
            this.Status.HeaderText = "Status";
            this.Status.MinimumWidth = 6;
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            // 
            // Penalty
            // 
            this.Penalty.FillWeight = 70F;
            this.Penalty.HeaderText = "Penalty";
            this.Penalty.MinimumWidth = 6;
            this.Penalty.Name = "Penalty";
            this.Penalty.ReadOnly = true;
            // 
            // Action
            // 
            this.Action.FillWeight = 90F;
            this.Action.HeaderText = "Action";
            this.Action.MinimumWidth = 6;
            this.Action.Name = "Action";
            this.Action.ReadOnly = false;
            this.Action.Text = "";
            this.Action.ToolTipText = "Return and pay penalty (if any)";
            this.Action.UseColumnTextForButtonValue = false;
            // 
            // BorrowingHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 629);
            this.Controls.Add(this.dataGridViewBorrowingHistory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BorrowingHistory";
            this.Text = "BorrowingHistory";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBorrowingHistory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView dataGridViewBorrowingHistory;
        private System.Windows.Forms.DataGridViewTextBoxColumn BookTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn BorrowedOn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DueDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn Penalty;
        private System.Windows.Forms.DataGridViewButtonColumn Action;
    }
}