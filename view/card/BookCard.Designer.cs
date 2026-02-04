namespace StudentLibrary.view.card
{
    partial class BookCard
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lbPublished = new System.Windows.Forms.Label();
            this.btnRequestBorrow = new System.Windows.Forms.Button();
            this.lblAvailability = new System.Windows.Forms.Label();
            this.btnViewInfo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblTitle.Location = new System.Drawing.Point(12, 48);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(83, 38);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Title";
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAuthor.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblAuthor.Location = new System.Drawing.Point(13, 103);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(98, 32);
            this.lblAuthor.TabIndex = 1;
            this.lblAuthor.Text = "Author";
            // 
            // lbPublished
            // 
            this.lbPublished.AutoSize = true;
            this.lbPublished.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPublished.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lbPublished.Location = new System.Drawing.Point(15, 150);
            this.lbPublished.Name = "lbPublished";
            this.lbPublished.Size = new System.Drawing.Size(82, 20);
            this.lbPublished.TabIndex = 2;
            this.lbPublished.Text = "Published";
            // 
            // btnRequestBorrow
            // 
            this.btnRequestBorrow.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRequestBorrow.Location = new System.Drawing.Point(197, 194);
            this.btnRequestBorrow.Name = "btnRequestBorrow";
            this.btnRequestBorrow.Size = new System.Drawing.Size(133, 53);
            this.btnRequestBorrow.TabIndex = 3;
            this.btnRequestBorrow.Text = "Request Borrow";
            this.btnRequestBorrow.UseVisualStyleBackColor = true;
            this.btnRequestBorrow.Visible = false;
            this.btnRequestBorrow.Click += new System.EventHandler(this.btnRequestBorrow_Click);
            // 
            // lblAvailability
            // 
            this.lblAvailability.AutoSize = true;
            this.lblAvailability.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvailability.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblAvailability.Location = new System.Drawing.Point(238, 9);
            this.lblAvailability.Name = "lblAvailability";
            this.lblAvailability.Size = new System.Drawing.Size(92, 25);
            this.lblAvailability.TabIndex = 4;
            this.lblAvailability.Text = "Available";
            // 
            // btnViewInfo
            // 
            this.btnViewInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnViewInfo.Location = new System.Drawing.Point(19, 194);
            this.btnViewInfo.Name = "btnViewInfo";
            this.btnViewInfo.Size = new System.Drawing.Size(133, 53);
            this.btnViewInfo.TabIndex = 5;
            this.btnViewInfo.Text = "View Info";
            this.btnViewInfo.UseVisualStyleBackColor = true;
            this.btnViewInfo.Click += new System.EventHandler(this.btnViewInfo_Click);
            // 
            // BookCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Teal;
            this.Controls.Add(this.btnViewInfo);
            this.Controls.Add(this.lblAvailability);
            this.Controls.Add(this.btnRequestBorrow);
            this.Controls.Add(this.lbPublished);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblTitle);
            this.Name = "BookCard";
            this.Size = new System.Drawing.Size(342, 259);
            this.Load += new System.EventHandler(this.BookCard_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lbPublished;
        private System.Windows.Forms.Button btnRequestBorrow;
        private System.Windows.Forms.Label lblAvailability;
        private System.Windows.Forms.Button btnViewInfo;
    }
}