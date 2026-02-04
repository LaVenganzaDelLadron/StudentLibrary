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
using StudentLibrary.enumerator;
using StudentLibrary.model;
using StudentLibrary.view.modal;

namespace StudentLibrary.view
{
    public partial class BrowseBookPage : Form
    {
        private BookServices bookServices;
        private BorrowService borrowService;
        private StudentService studentService;
        private List<Books> allBooks;
        private string _studentName;
        private string _studentFullName;
        private Dashboard _dashboard;

        public BrowseBookPage(string userId = "", string username = "", string fullName = "", Dashboard dashboard = null)
        {
            InitializeComponent();
            bookServices = new BookServices();
            borrowService = new BorrowService();
            studentService = new StudentService();
            _studentName = username;
            _studentFullName = fullName;
            _dashboard = dashboard;
            if (string.IsNullOrWhiteSpace(_studentFullName))
            {
                ResolveStudentFullName();
            }
            bookServices.OnBooksUpdated += BookServices_OnBooksUpdated;
            LoadBooks();
            LoadCategories();
            bookServices.StartListeningForUpdates();
        }

        private void ResolveStudentFullName()
        {
            if (string.IsNullOrWhiteSpace(_studentName))
            {
                return;
            }

            try
            {
                var students = studentService.GetAllStudents();
                var matched = students.FirstOrDefault(s =>
                    string.Equals(s.UserName, _studentName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Email, _studentName, StringComparison.OrdinalIgnoreCase));

                if (matched != null)
                {
                    _studentFullName = $"{matched.FirstName} {matched.LastName}".Trim();
                }
            }
            catch
            {
                // Keep fallback to username if lookup fails
            }
        }

        private void LoadCategories()
        {
            comboBoxCategory.Items.Clear();
            comboBoxCategory.Items.Add("All Categories");
            
            if (allBooks != null && allBooks.Count > 0)
            {
                var categories = allBooks.Select(b => b.Category).Distinct().OrderBy(c => c).ToList();
                foreach (var category in categories)
                {
                    if (!string.IsNullOrEmpty(category))
                    {
                        comboBoxCategory.Items.Add(category);
                    }
                }
            }
            
            comboBoxCategory.SelectedIndex = 0;
        }

        private void BookServices_OnBooksUpdated(object sender, List<Books> updatedBooks)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => RefreshBooks(updatedBooks)));
            }
            else
            {
                RefreshBooks(updatedBooks);
            }
        }

        private void RefreshBooks(List<Books> updatedBooks)
        {
            allBooks = updatedBooks;
            LoadCategories();
            LoadBooks(textBoxSearch.Text);
        }

        private void LoadBooks(string searchTerm = "")
        {
            dgvBooks.Rows.Clear();

            if (allBooks == null)
            {
                allBooks = bookServices.GetAllBooks();
            }

            var filteredBooks = allBooks;

            // Filter by category
            if (comboBoxCategory.SelectedIndex > 0)
            {
                string selectedCategory = comboBoxCategory.SelectedItem.ToString();
                filteredBooks = filteredBooks.Where(b => b.Category == selectedCategory).ToList();
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredBooks = filteredBooks.Where(b => 
                    b.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    b.Author.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            string studentNameToCheck = !string.IsNullOrWhiteSpace(_studentFullName) ? _studentFullName : _studentName;
            HashSet<string> pendingBookTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(studentNameToCheck))
            {
                var borrows = borrowService.GetBorrows();
                foreach (var borrow in borrows)
                {
                    if (borrow == null)
                    {
                        continue;
                    }

                    if (string.Equals(borrow.StudentName?.Trim(), studentNameToCheck, StringComparison.OrdinalIgnoreCase) &&
                        borrow.Status == BorrowStatus.Pending)
                    {
                        pendingBookTitles.Add(borrow.BookRequested ?? string.Empty);
                    }
                }
            }

            foreach (var book in filteredBooks)
            {
                string status = pendingBookTitles.Contains(book.Title ?? string.Empty) ? "Pending" : (book.Copies > 0 ? "Available" : "Out of Stock");
                string actions = status == "Pending" ? "..." : "⋮";
                
                dgvBooks.Rows.Add(
                    book.Title,
                    book.Author,
                    book.Category,
                    book.Copies,
                    status,
                    actions
                );
            }

            if (filteredBooks.Count == 0)
            {
                dgvBooks.Rows.Add("No books found", "", "", "", "", "");
            }
        }

        private void dgvBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvBooks.Rows.Count) return;

            DataGridViewRow row = dgvBooks.Rows[e.RowIndex];
            if (row.Cells[0].Value == null) return;

            string bookTitle = row.Cells[0].Value.ToString();
            
            // Column 5 is Actions
            if (e.ColumnIndex == 5)
            {
                // Check if this is a pending request - don't show menu
                string actions = row.Cells[5].Value?.ToString() ?? "";
                if (actions == "Disabled")
                {
                    MessageBox.Show("This book request is pending approval from the librarian.", "Pending Request", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // Get book status to enable/disable Request Book option
                string status = row.Cells[4].Value?.ToString() ?? "";
                bool canRequest = status == "Available";
                
                // Store the selected book title for the context menu handlers
                menuItemViewInfo.Tag = bookTitle;
                menuItemRequestBook.Tag = bookTitle;
                menuItemRequestBook.Enabled = canRequest;
                
                // Update menu item text to show why it's disabled
                if (!canRequest)
                {
                    if (status == "Pending")
                    {
                        menuItemRequestBook.Text = "Request Book (Already Pending)";
                    }
                    else if (status == "Out of Stock")
                    {
                        menuItemRequestBook.Text = "Request Book (Out of Stock)";
                    }
                }
                else
                {
                    menuItemRequestBook.Text = "Request Book";
                }
                
                // Remove old event handlers to prevent duplicates
                menuItemViewInfo.Click -= MenuItemViewInfo_Click;
                menuItemRequestBook.Click -= MenuItemRequestBook_Click;
                
                // Add event handlers
                menuItemViewInfo.Click += MenuItemViewInfo_Click;
                menuItemRequestBook.Click += MenuItemRequestBook_Click;
                
                // Show context menu at cursor position
                var cellRect = dgvBooks.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                contextMenuActions.Show(dgvBooks, cellRect.Left, cellRect.Bottom);
            }
        }

        private void MenuItemViewInfo_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem?.Tag is string bookTitle)
            {
                BookInfo_Show(bookTitle);
            }
        }

        private void MenuItemRequestBook_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem?.Tag is string bookTitle)
            {
                RequestBorrow(bookTitle);
            }
        }

        private void dgvBooks_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 5)
            {
                dgvBooks.Cursor = Cursors.Hand;
            }
        }

        private void dgvBooks_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgvBooks.Cursor = Cursors.Default;
        }

        private void dgvBooks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 4 && e.RowIndex >= 0) // Status column
            {
                string status = e.Value?.ToString() ?? "";
                
                if (status == "Available")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(0, 128, 0); // Green
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Pending")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(255, 140, 0); // Orange
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Out of Stock")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(220, 20, 60); // Red
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
            else if (e.ColumnIndex == 5 && e.RowIndex >= 0) // Actions column
            {
                string actions = e.Value?.ToString() ?? "";
                
                if (actions == "Disabled")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(128, 128, 128); // Gray
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }

        private void BookInfo_Show(string bookTitle)
        {
            var selectedBook = allBooks?.FirstOrDefault(b => string.Equals(b.Title, bookTitle, StringComparison.OrdinalIgnoreCase));
            if (selectedBook != null)
            {
                BookInfo bookInfo = new BookInfo(selectedBook);
                bookInfo.ShowDialog();
            }
        }

        private void RequestBorrow(string bookTitle)
        {
            try
            {
                string studentNameToSend = !string.IsNullOrWhiteSpace(_studentFullName) ? _studentFullName : _studentName;
                AuthResponse response = borrowService.RequestBorrow(studentNameToSend, bookTitle);
                
                if (response.Status == "SUCCESS")
                {
                    MessageBox.Show("Borrow request submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Refresh the book list to show updated pending status
                    LoadBooks(textBoxSearch.Text);
                    
                    // Refresh the dashboard if available
                    if (_dashboard != null)
                    {
                        _dashboard.RefreshBorrowingPage();
                    }
                }
                else
                {
                    MessageBox.Show($"Failed to submit borrow request: {response.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting borrow request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadBooks(textBoxSearch.Text);
        }

        private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadBooks(textBoxSearch.Text);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            bookServices.StopListeningForUpdates();
            base.OnFormClosing(e);
        }
    }
}

