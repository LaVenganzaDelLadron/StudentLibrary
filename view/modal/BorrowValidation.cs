using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentLibrary.view.modal
{
    public partial class BorrowValidation : Form
    {
        private string _studentFirstName;
        private string _studentLastName;
        private string _bookTitle;
        private bool _isPending;

        public BorrowValidation()
        {
            InitializeComponent();
            btnYes.Click += BtnYes_Click;
            btnNo.Click += BtnNo_Click;
        }

        public void SetStudentInfo(string firstName, string lastName)
        {
            _studentFirstName = firstName;
            _studentLastName = lastName;
        }

        public void SetBookTitle(string bookTitle)
        {
            _bookTitle = bookTitle;
            label1.Text = $"Do you want to borrow '{bookTitle}'?";
        }

        public string GetStudentFullName()
        {
            return $"{_studentFirstName} {_studentLastName}";
        }

        public void SetPendingState(bool isPending)
        {
            _isPending = isPending;
            if (_isPending)
            {
                btnYes.Enabled = false;
                btnYes.Text = "Pending";
            }
            else
            {
                btnYes.Enabled = true;
                btnYes.Text = "Yes";
            }
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }
}
