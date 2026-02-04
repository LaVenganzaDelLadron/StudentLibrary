using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudentLibrary.model;
using StudentLibrary.view.modal;

namespace StudentLibrary.view.card
{
    public partial class BookCard : UserControl
    {
        private Books _book;
        private Dashboard _dashboard;
        private bool _isBorrowPending;
        public event EventHandler<Books> OnViewInfo;
        public event EventHandler<Books> OnBorrowRequested;

        public BookCard()
        {
            InitializeComponent();
        }

        public void SetDashboard(Dashboard dashboard)
        {
            _dashboard = dashboard;
        }

        public void SetBookData(Books book)
        {
            _book = book;
            lblTitle.Text = book.Title;
            lblAuthor.Text = "By " + book.Author;
            lbPublished.Text = "Year: " + book.PublishedDate.Year.ToString();
            lblAvailability.Text = book.Copies > 0 ? $"Available: {book.Copies}" : "Not Available";
            lblAvailability.ForeColor = book.Copies > 0 ? Color.Green : Color.Red;
            
            // Show request borrow button only if copies available
            btnRequestBorrow.Visible = book.Copies > 0;
            if (btnRequestBorrow.Visible)
            {
                btnRequestBorrow.Enabled = true;
                btnRequestBorrow.Text = "Request Borrow";
            }
        }

        public void SetBorrowPending(bool isPending)
        {
            _isBorrowPending = isPending;
            if (!btnRequestBorrow.Visible)
            {
                return;
            }

            if (_isBorrowPending)
            {
                btnRequestBorrow.Enabled = false;
                btnRequestBorrow.Text = "Pending";
            }
            else
            {
                btnRequestBorrow.Enabled = true;
                btnRequestBorrow.Text = "Request Borrow";
            }
        }

        private void btnViewInfo_Click(object sender, EventArgs e)
        {
            OnViewInfo?.Invoke(this, _book);
        }

        private void btnRequestBorrow_Click(object sender, EventArgs e)
        {
            BorrowValidation borrowModal = new BorrowValidation();
            borrowModal.SetBookTitle(_book.Title);
            borrowModal.SetPendingState(_isBorrowPending);
            
            if (_dashboard != null)
            {
                borrowModal.SetStudentInfo(_dashboard.GetStudentFirstName(), _dashboard.GetStudentLastName());
            }
            
            if (borrowModal.ShowDialog() == DialogResult.Yes)
            {
                // Notify parent about borrow request
                OnBorrowRequested?.Invoke(this, _book);
            }
        }

        private void BookCard_Load(object sender, EventArgs e)
        {

        }
    }
}

