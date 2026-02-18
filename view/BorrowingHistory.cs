using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudentLibrary.controller;
using StudentLibrary.model;
using Newtonsoft.Json.Linq;

namespace StudentLibrary.view
{
    public partial class BorrowingHistory : Form
    {
        private BorrowService borrowService;
        private RejectService rejectService;
        private StudentService studentService;
        private string currentStudentName;
        private List<Borrow> allBorrows = new List<Borrow>();
        private string currentFilter = "All Status";

        public BorrowingHistory(string studentName = "")
        {
            InitializeComponent();
            borrowService = new BorrowService();
            rejectService = new RejectService();
            studentService = new StudentService();
            currentStudentName = studentName;

            // Apply styling
            ApplyTableStyling();
            SetupTooltips();
            LoadBorrowingHistory();
        }

        private void SetupTooltips()
        {
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(dataGridViewBorrowingHistory, "Click Return & Pay button to return a book and pay penalties");
        }

        private void ApplyTableStyling()
        {
            // Alternating row colors
            dataGridViewBorrowingHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dataGridViewBorrowingHistory.DefaultCellStyle.BackColor = Color.White;

            // Make buttons look better
            dataGridViewBorrowingHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(31, 78, 121);
            dataGridViewBorrowingHistory.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        private void LoadBorrowingHistory()
        {
            try
            {
                dataGridViewBorrowingHistory.Rows.Clear();

                // Get all borrows for this student
                allBorrows = borrowService.GetAllBorrows();
                borrowService.LoadBookAuthors();
                var filteredBorrows = string.IsNullOrEmpty(currentStudentName)
                    ? allBorrows
                    : allBorrows.Where(b => b.StudentName == currentStudentName).ToList();

                foreach (var borrow in filteredBorrows)
                {
                    // Calculate due date
                    DateTime? dueDate = borrowService.GetDueDate(borrow);

                    // Determine status display
                    string statusDisplay = borrowService.GetDisplayStatus(borrow);
                    string authorName = borrowService.GetAuthorForTitle(borrow.BookRequested);

                    // Calculate penalty if overdue
                    decimal penalty = borrowService.CalculatePenalty(borrow);

                    // Add row to table
                    int rowIndex = dataGridViewBorrowingHistory.Rows.Add(
                        borrow.BookRequested,
                        authorName,
                        borrow.RequestDate.ToString("MM/dd/yyyy"),
                        dueDate.HasValue ? dueDate.Value.ToString("MM/dd/yyyy") : "-",
                        statusDisplay,
                        penalty > 0 ? $"₱{penalty:F2}" : "₱0.00",
                        "Return & Pay"
                    );

                    // Store the borrow ID in the row tag for identification
                    dataGridViewBorrowingHistory.Rows[rowIndex].Tag = borrow.Id.ToString();

                    // Color code the status
                    ColorizeStatusCell(rowIndex, statusDisplay);

                    // Configure action button
                    ConfigureActionButtons(rowIndex, borrow, statusDisplay);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading borrowing history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ColorizeStatusCell(int rowIndex, string status)
        {
            DataGridViewCell statusCell = dataGridViewBorrowingHistory.Rows[rowIndex].Cells["Status"];
            
            switch (status)
            {
                case "Pending Approval":
                    statusCell.Style.BackColor = Color.FromArgb(255, 193, 7); // Orange
                    statusCell.Style.ForeColor = Color.Black;
                    break;
                case "Borrowed":
                    statusCell.Style.BackColor = Color.FromArgb(33, 150, 243); // Blue
                    statusCell.Style.ForeColor = Color.White;
                    break;
                case "Overdue":
                    statusCell.Style.BackColor = Color.FromArgb(244, 67, 54); // Red
                    statusCell.Style.ForeColor = Color.White;
                    break;
                case "Returning":
                    statusCell.Style.BackColor = Color.FromArgb(255, 152, 0); // Deep Orange
                    statusCell.Style.ForeColor = Color.White;
                    break;
                case "Returned":
                    statusCell.Style.BackColor = Color.FromArgb(76, 175, 80); // Green
                    statusCell.Style.ForeColor = Color.White;
                    break;
                default:
                    statusCell.Style.BackColor = Color.FromArgb(158, 158, 158); // Gray
                    statusCell.Style.ForeColor = Color.White;
                    break;
            }
        }

        private void ConfigureActionButtons(int rowIndex, Borrow borrow, string statusDisplay)
        {
            var actionCell = dataGridViewBorrowingHistory.Rows[rowIndex].Cells["Action"];

            // Hide button for Pending Approval, Returning, and Returned statuses
            if (statusDisplay == "Pending Approval" || statusDisplay == "Returning" || statusDisplay == "Returned")
            {
                actionCell.Value = "";
                actionCell.ReadOnly = true;
                return;
            }

            // Show Return button for Borrowed and Overdue
            // Check if book is overdue to show appropriate button text
            bool isOverdue = statusDisplay == "Overdue";
            
            actionCell.Value = isOverdue ? "Return & Pay" : "Return";
            actionCell.ReadOnly = false;
        }

        private void DataGridViewBorrowingHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string borrowId = dataGridViewBorrowingHistory.Rows[e.RowIndex].Tag?.ToString();
            if (string.IsNullOrEmpty(borrowId)) return;

            // Return button clicked
            if (e.ColumnIndex == dataGridViewBorrowingHistory.Columns["Action"].Index)
            {
                string status = dataGridViewBorrowingHistory.Rows[e.RowIndex].Cells["Status"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(status) || status == "Pending Approval" || status == "Returning" || status == "Returned")
                {
                    MessageBox.Show("This action is not available for the selected record.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string penaltyText = dataGridViewBorrowingHistory.Rows[e.RowIndex].Cells["Penalty"].Value?.ToString();
                string actionText = dataGridViewBorrowingHistory.Rows[e.RowIndex].Cells["Action"].Value?.ToString();
                ReturnAndPay(borrowId, penaltyText, actionText, e.RowIndex);
            }
        }

        private void ReturnBook(string borrowId, int rowIndex)
        {
            try
            {
                // Get borrow details
                if (!Guid.TryParse(borrowId, out Guid borrowGuid))
                {
                    MessageBox.Show("Invalid borrow ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var borrow = borrowService.GetAllBorrows().FirstOrDefault(b => b.Id == borrowGuid);
                if (borrow == null)
                {
                    MessageBox.Show("Borrow record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Return book '{borrow.BookRequested}'?",
                    "Confirm Return",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Update borrow status to Returned
                    borrowService.ReturnBook(borrowId);

                    MessageBox.Show("Book returned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload the table
                    LoadBorrowingHistory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error returning book: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PayFine(string borrowId, string penaltyText, int rowIndex)
        {
            try
            {
                // Extract fine amount
                string fineAmount = penaltyText.Replace("₱", "").Trim();
                if (!decimal.TryParse(fineAmount, out decimal amount))
                {
                    MessageBox.Show("Invalid fine amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Pay fine of ₱{amount:F2}?",
                    "Confirm Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Update borrow to mark fine as paid
                    borrowService.MarkFineAsPaid(borrowId);

                    MessageBox.Show($"Fine of ₱{amount:F2} paid successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload the table
                    LoadBorrowingHistory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridViewBorrowingHistory_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewBorrowingHistory.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(235, 242, 250);

                if (e.ColumnIndex == dataGridViewBorrowingHistory.Columns["Action"].Index)
                {
                    dataGridViewBorrowingHistory.Cursor = Cursors.Hand;
                }
            }
        }

        private void DataGridViewBorrowingHistory_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewBorrowingHistory.Cursor = Cursors.Default;

            if (e.RowIndex >= 0)
            {
                dataGridViewBorrowingHistory.Rows[e.RowIndex].DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                    ? Color.White
                    : Color.FromArgb(245, 245, 245);
            }
        }

        private void ReturnAndPay(string borrowId, string penaltyText, string actionText, int rowIndex)
        {
            if (!Guid.TryParse(borrowId, out Guid borrowGuid))
            {
                MessageBox.Show("Invalid borrow ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var borrow = borrowService.GetAllBorrows().FirstOrDefault(b => b.Id == borrowGuid);
            if (borrow == null)
            {
                MessageBox.Show("Borrow record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal amount = 0;
            string fineAmount = (penaltyText ?? string.Empty).Replace("₱", "").Trim();
            if (!string.IsNullOrEmpty(fineAmount))
            {
                decimal.TryParse(fineAmount, out amount);
            }

            bool hasPayment = actionText != null && actionText.Contains("Pay");
            string confirmationMessage = hasPayment && amount > 0
                ? $"Return '{borrow.BookRequested}' and pay ₱{amount:F2}?"
                : $"Return '{borrow.BookRequested}'?";

            var result = MessageBox.Show(
                confirmationMessage,
                "Confirm Return & Pay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            // Mark fine as paid if there's a penalty
            if (hasPayment && amount > 0)
            {
                borrowService.MarkFineAsPaid(borrowId);
            }

            borrowService.ReturnBook(borrowId);
            MessageBox.Show("Return & Pay completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadBorrowingHistory();
        }

        private void LoadBookAuthors()
        {
            borrowService.LoadBookAuthors();
        }

        private string GetAuthorForTitle(string title)
        {
            return borrowService.GetAuthorForTitle(title);
        }
    }
}
