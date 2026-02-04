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
using System.IO;
using Newtonsoft.Json;
using StudentLibrary.model;

namespace StudentLibrary.view
{
    public partial class HomePage : Form
    {
        private string studentName;
        private HomeService homeService;
        private Timer refreshTimer;

        public HomePage()
        {
            InitializeComponent();
            homeService = new HomeService();
            SetupRefreshTimer();
            SetupDataGridView();
        }

        public HomePage(string username) : this()
        {
            this.studentName = username;
            if (!string.IsNullOrWhiteSpace(username))
            {
                lblGreet.Text = $"Welcome Back, {username}";
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadDashboardData();
            LoadRecentActivity();
            LoadNotifications();
        }

        private void SetupDataGridView()
        {
            dgvRecentActivity.Columns.Clear();
            dgvRecentActivity.Columns.Add("BookTitle", "Book Title");
            dgvRecentActivity.Columns.Add("Date", "Date");
            dgvRecentActivity.Columns.Add("Status", "Status");
            
            dgvRecentActivity.Columns[0].Width = 300;
            dgvRecentActivity.Columns[1].Width = 150;
            dgvRecentActivity.Columns[2].Width = 120;
            
            dgvRecentActivity.DefaultCellStyle.SelectionBackColor = Color.Teal;
            dgvRecentActivity.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        private void LoadRecentActivity()
        {
            if (string.IsNullOrWhiteSpace(studentName))
                return;

            try
            {
                dgvRecentActivity.Rows.Clear();
                
                var activities = homeService.GetRecentActivities(studentName, 10);
                
                foreach (var activity in activities)
                {
                    int rowIndex = dgvRecentActivity.Rows.Add(
                        activity.BookTitle,
                        activity.Date.ToString("MMM dd, yyyy"),
                        activity.Status
                    );
                    
                    // Color code by status
                    var row = dgvRecentActivity.Rows[rowIndex];
                    switch (activity.Status)
                    {
                        case "Overdue":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(139, 0, 0);
                            break;
                        case "Borrowed":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220);
                            break;
                        case "Returning":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                            break;
                        case "Returned":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                            row.DefaultCellStyle.ForeColor = Color.Gray;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recent activity: {ex.Message}");
            }
        }

        private void LoadNotifications()
        {
            if (string.IsNullOrWhiteSpace(studentName))
                return;

            try
            {
                lstNotifications.Items.Clear();

                // Use shared LibraryManagementSystem bin/Debug/Data/notifications folder
                string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                string libraryManagementPath = System.IO.Path.Combine(desktopPath, "LibraryManagementSystem", "bin", "Debug", "Data");
                string baseNotificationsFolder = Path.Combine(libraryManagementPath, "notifications");
                string studentNotificationsFolder = Path.Combine(baseNotificationsFolder, studentName);
                string notificationsFile = Path.Combine(studentNotificationsFolder, "notifications.json");

                Console.WriteLine($"Loading notifications for student: {studentName}");
                Console.WriteLine($"Looking for notifications at: {notificationsFile}");

                if (!File.Exists(notificationsFile))
                {
                    Console.WriteLine($"Notifications file not found at: {notificationsFile}");
                    lblNotificationCount.Text = "0";
                    lstNotifications.Items.Add("No notifications");
                    return;
                }

                string json = File.ReadAllText(notificationsFile);
                var notifications = JsonConvert.DeserializeObject<List<Notification>>(json);

                if (notifications == null || notifications.Count == 0)
                {
                    Console.WriteLine("No notifications found in file");
                    lblNotificationCount.Text = "0";
                    lstNotifications.Items.Add("No new notifications");
                    return;
                }

                Console.WriteLine($"Found {notifications.Count} notifications");

                // Get recent notifications (last 5) and format them
                var recentNotifications = notifications
                    .OrderByDescending(n => n.CreatedDate)
                    .Take(5)
                    .ToList();

                lblNotificationCount.Text = recentNotifications.Count.ToString();

                foreach (var notification in recentNotifications)
                {
                    // Extract book title from message and format
                    string bookTitle = ExtractBookTitle(notification.Message);
                    string formattedMessage = FormatNotificationMessage(notification.Type, bookTitle, notification.Message);
                    
                    // Shorten for display in list
                    string shortMessage = formattedMessage.Length > 50 
                        ? formattedMessage.Substring(0, 47) + "..." 
                        : formattedMessage;
                    
                    lstNotifications.Items.Add(shortMessage);
                }

                Console.WriteLine($"Displayed {recentNotifications.Count} recent notifications");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");
                lblNotificationCount.Text = "0";
                lstNotifications.Items.Add("Error loading notifications");
            }
        }

        private string ExtractBookTitle(string message)
        {
            // Extract book title from message (between quotes)
            int start = message.IndexOf("'");
            if (start >= 0)
            {
                int end = message.IndexOf("'", start + 1);
                if (end > start)
                {
                    return message.Substring(start + 1, end - start - 1);
                }
            }
            return "Unknown";
        }

        private string FormatNotificationMessage(string type, string bookTitle, string originalMessage)
        {
            switch (type?.ToLower())
            {
                case "approved":
                    if (originalMessage.Contains("Due date:"))
                    {
                        int dueDateIndex = originalMessage.IndexOf("Due date:");
                        string dueDate = originalMessage.Substring(dueDateIndex + 10).Trim();
                        return $"✓ {bookTitle} - Due: {dueDate}";
                    }
                    return $"✓ {bookTitle} - Ready for pickup";

                case "rejected":
                    return $"✗ {bookTitle} - Request denied";

                case "overdue":
                    if (originalMessage.Contains("₱"))
                    {
                        int fineStart = originalMessage.IndexOf("₱");
                        int fineEnd = originalMessage.IndexOf(" ", fineStart);
                        if (fineEnd < 0) fineEnd = originalMessage.Length;
                        string fine = originalMessage.Substring(fineStart, fineEnd - fineStart);
                        return $"⚠ {bookTitle} - Overdue ({fine} penalty)";
                    }
                    return $"⚠ {bookTitle} - Overdue";

                case "pending":
                    return $"⏳ {bookTitle} - Awaiting approval";

                default:
                    return originalMessage;
            }
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 5000; // Refresh every 5 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadDashboardData();
            LoadRecentActivity();
            LoadNotifications();
        }

        private void LoadDashboardData()
        {
            if (string.IsNullOrWhiteSpace(studentName))
            {
                Console.WriteLine("HomePage: No student name provided");
                label2.Text = "0";
                label5.Text = "0";
                label9.Text = "0";
                label12.Text = "₱ 0.00";
                return;
            }

            try
            {
                Console.WriteLine($"HomePage: Loading dashboard data for '{studentName}'");
                
                // Load summary statistics filtered by current student
                var stats = homeService.GetDashboardStats(studentName);

                // Update card values
                label2.Text = stats.BooksBorrowed.ToString();
                label5.Text = stats.DueSoon.ToString();
                label9.Text = stats.Overdue.ToString();
                label12.Text = $"₱ {stats.TotalFines:F2}";
                
                // Update remaining time display
                label4.Text = stats.RemainingTimeDisplay;
                
                // Show/hide "Requires immediate return" label based on overdue count
                label8.Visible = stats.Overdue > 0;

                // Update card colors based on status
                UpdateCardColors(stats);

                Console.WriteLine($"HomePage: Loaded stats - Borrowed: {stats.BooksBorrowed}, Due Soon: {stats.DueSoon}, Overdue: {stats.Overdue}, Fines: ₱{stats.TotalFines:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HomePage Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Set default values on error
                label2.Text = "0";
                label5.Text = "0";
                label9.Text = "0";
                label12.Text = "₱ 0.00";
            }
        }

        private void UpdateCardColors(DashboardStats stats)
        {
            // Change overdue card to red if there are overdue books
            if (stats.Overdue > 0)
            {
                panel3.BackColor = Color.FromArgb(220, 53, 69); // Red
            }
            else
            {
                panel3.BackColor = Color.Teal;
            }

            // Change fines card to orange if there are unpaid fines
            if (stats.TotalFines > 0)
            {
                panel4.BackColor = Color.FromArgb(255, 193, 7); // Orange
                label12.ForeColor = Color.Black;
                label13.ForeColor = Color.Black;
                label11.ForeColor = Color.Black;
            }
            else
            {
                panel4.BackColor = Color.Teal;
                label12.ForeColor = Color.White;
                label13.ForeColor = Color.White;
                label11.ForeColor = Color.White;
            }

            // Change due soon card to yellow if books are due soon
            if (stats.DueSoon > 0)
            {
                panel2.BackColor = Color.FromArgb(255, 193, 7); // Yellow/Orange
                label5.ForeColor = Color.Black;
                label7.ForeColor = Color.Black;
                label4.ForeColor = Color.Black;
            }
            else
            {
                panel2.BackColor = Color.Teal;
                label5.ForeColor = Color.White;
                label7.ForeColor = Color.White;
                label4.ForeColor = Color.White;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (refreshTimer != null)
            {
                refreshTimer.Stop();
                refreshTimer.Dispose();
            }
        }
    }
}
