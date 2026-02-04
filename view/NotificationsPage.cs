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
using Newtonsoft.Json;
using System.IO;

namespace StudentLibrary.view
{
    public partial class NotificationsPage : Form
    {
        private string studentName;
        private string studentId;
        private Timer refreshTimer;
        private FileSystemWatcher notificationWatcher;
        private string notificationsFilePath;

        public NotificationsPage()
        {
            InitializeComponent();
        }

        public NotificationsPage(string studentName) : this()
        {
            this.studentName = studentName;
            this.studentId = studentName; // Use student name as ID for now
            Console.WriteLine($"Notifications initialized with studentName: {studentName}");
            InitializeRefreshTimer();
        }

        public NotificationsPage(string studentName, string studentId) : this()
        {
            this.studentName = studentName;
            this.studentId = studentId;
            Console.WriteLine($"Notifications initialized with studentName: {studentName}, studentId: {studentId}");
            InitializeRefreshTimer();
            InitializeFileWatcher();
        }

        private void InitializeFileWatcher()
        {
            try
            {
                // Use shared LibraryManagementSystem bin/Debug/Data/notifications folder
                string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                string libraryManagementPath = System.IO.Path.Combine(desktopPath, "LibraryManagementSystem", "bin", "Debug", "Data");
                string notificationsFolder = ResolveNotificationsFolder(libraryManagementPath);
                notificationsFilePath = Path.Combine(notificationsFolder, "notifications.json");

                if (!Directory.Exists(notificationsFolder))
                {
                    Directory.CreateDirectory(notificationsFolder);
                }

                notificationWatcher = new FileSystemWatcher(notificationsFolder);
                notificationWatcher.Filter = "notifications.json";
                notificationWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                notificationWatcher.Changed += NotificationFile_Changed;
                notificationWatcher.EnableRaisingEvents = true;

                Console.WriteLine($"File watcher initialized for: {notificationsFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing file watcher: {ex.Message}");
            }
        }

        private string ResolveNotificationsFolder(string libraryManagementPath)
        {
            string baseFolder = Path.Combine(libraryManagementPath, "notifications");
            string primary = !string.IsNullOrWhiteSpace(studentId) ? Path.Combine(baseFolder, studentId) : string.Empty;
            string fallback = !string.IsNullOrWhiteSpace(studentName) ? Path.Combine(baseFolder, studentName) : string.Empty;

            if (!string.IsNullOrWhiteSpace(primary) && Directory.Exists(primary))
            {
                return primary;
            }

            if (!string.IsNullOrWhiteSpace(fallback))
            {
                if (!Directory.Exists(fallback))
                {
                    Directory.CreateDirectory(fallback);
                }
                return fallback;
            }

            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            return baseFolder;
        }

        private void NotificationFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    System.Threading.Thread.Sleep(500); // Wait for file write to complete
                    LoadNotifications();
                }));
            }
            else
            {
                System.Threading.Thread.Sleep(500);
                LoadNotifications();
            }
        }

        private void InitializeRefreshTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 3000; // Refresh every 3 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadNotifications();
        }

        private void Notifications_Load(object sender, EventArgs e)
        {
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            try
            {
                dgvNotifications.Rows.Clear();

                // Load server-side notifications from file
                LoadServerNotifications();

                if (dgvNotifications.Rows.Count == 0)
                {
                    dgvNotifications.Rows.Add("Info", "No notifications at this time", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), "-");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");
                MessageBox.Show($"Error loading notifications: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadServerNotifications()
        {
            try
            {
                // Load from shared LibraryManagementSystem bin/Debug/Data/notifications.json file
                string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                string libraryManagementPath = System.IO.Path.Combine(desktopPath, "LibraryManagementSystem", "bin", "Debug", "Data");
                
                // Use student name to find the notifications folder
                string baseNotificationsFolder = Path.Combine(libraryManagementPath, "notifications");
                string studentNotificationsFolder = Path.Combine(baseNotificationsFolder, studentName);
                string notificationsFile = Path.Combine(studentNotificationsFolder, "notifications.json");

                Console.WriteLine($"Loading notifications for student: {studentName}");
                Console.WriteLine($"Looking for notifications at: {notificationsFile}");

                if (!File.Exists(notificationsFile))
                {
                    Console.WriteLine($"Notifications file not found at: {notificationsFile}");
                    Console.WriteLine($"Available path would be: {studentNotificationsFolder}");
                    return;
                }

                string json = File.ReadAllText(notificationsFile);
                Console.WriteLine($"Loaded JSON: {json.Substring(0, Math.Min(100, json.Length))}...");

                var notifications = JsonConvert.DeserializeObject<List<Notification>>(json);

                if (notifications == null || notifications.Count == 0)
                {
                    Console.WriteLine("No notifications found in file");
                    return;
                }

                Console.WriteLine($"Found {notifications.Count} notifications");

                // Group notifications by book title and type, keep only the latest
                var groupedNotifications = notifications
                    .GroupBy(n => new { BookTitle = ExtractBookTitle(n.Message), Type = n.Type })
                    .Select(g => new
                    {
                        Notifications = g.OrderByDescending(n => n.CreatedDate).ToList(),
                        Latest = g.OrderByDescending(n => n.CreatedDate).First(),
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Latest.CreatedDate)
                    .ToList();

                foreach (var group in groupedNotifications)
                {
                    var notification = group.Latest;
                    string bookTitle = ExtractBookTitle(notification.Message);
                    string status = notification.IsRead ? "Read" : "New";
                    
                    // Create clean, scannable message
                    string cleanMessage = FormatNotificationMessage(notification.Type, bookTitle, notification.Message);
                    
                    // Add count if multiple notifications for same book/type
                    if (group.Count > 1)
                    {
                        cleanMessage += $" ({group.Count}x)";
                    }

                    int rowIndex = dgvNotifications.Rows.Add(
                        notification.Type,
                        cleanMessage,
                        notification.CreatedDate.ToString("MMM dd, HH:mm"),
                        status
                    );

                    // Color code based on type
                    DataGridViewRow row = dgvNotifications.Rows[rowIndex];
                    switch (notification.Type?.ToLower())
                    {
                        case "approved":
                            row.Cells[0].Style.ForeColor = Color.FromArgb(0, 128, 0); // Green
                            row.Cells[0].Style.Font = new Font(dgvNotifications.Font, FontStyle.Bold);
                            break;
                        case "rejected":
                            row.Cells[0].Style.ForeColor = Color.FromArgb(220, 20, 60); // Red
                            row.Cells[0].Style.Font = new Font(dgvNotifications.Font, FontStyle.Bold);
                            break;
                        case "overdue":
                            row.Cells[0].Style.ForeColor = Color.FromArgb(255, 140, 0); // Orange
                            row.Cells[0].Style.Font = new Font(dgvNotifications.Font, FontStyle.Bold);
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 230); // Light orange background
                            break;
                        case "pending":
                            row.Cells[0].Style.ForeColor = Color.FromArgb(33, 150, 243); // Blue
                            row.Cells[0].Style.Font = new Font(dgvNotifications.Font, FontStyle.Bold);
                            break;
                        default:
                            row.Cells[0].Style.ForeColor = Color.FromArgb(96, 96, 96); // Gray
                            break;
                    }

                    // Highlight unread notifications
                    if (!notification.IsRead)
                    {
                        row.DefaultCellStyle.Font = new Font(dgvNotifications.Font, FontStyle.Bold);
                        row.Cells[3].Style.ForeColor = Color.FromArgb(255, 140, 0); // Orange for "New"
                        row.Cells[3].Style.Font = new Font(dgvNotifications.Font, FontStyle.Bold);
                    }
                }

                Console.WriteLine($"Displayed {groupedNotifications.Count} unique notifications");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading server notifications: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading notifications: {ex.Message}", "Notification Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    // Extract due date if present
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
                    // Extract fine amount if present
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

        private void LoadPendingRequests()
        {
            try
            {
                var borrowService = new BorrowService();
                var borrows = borrowService.GetBorrows();

                if (borrows == null) return;

                var pendingBorrows = borrows.Where(b =>
                    b.Status == StudentLibrary.enumerator.BorrowStatus.Pending &&
                    string.Equals(b.StudentName?.Trim(), studentName?.Trim(), StringComparison.OrdinalIgnoreCase)
                ).ToList();

                foreach (var borrow in pendingBorrows)
                {
                    string message = $"Your request for '{borrow.BookRequested}' is pending approval";
                    dgvNotifications.Rows.Add(
                        "Pending",
                        message,
                        borrow.RequestDate.ToString("yyyy-MM-dd HH:mm"),
                        "Waiting"
                    );
                    dgvNotifications.Rows[dgvNotifications.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading pending requests: {ex.Message}");
            }
        }

        private void LoadApprovedRequests()
        {
            try
            {
                var borrowService = new BorrowService();
                var borrows = borrowService.GetBorrows();

                if (borrows == null) return;

                var approvedBorrows = borrows.Where(b =>
                    b.Status == StudentLibrary.enumerator.BorrowStatus.Borrowed &&
                    string.Equals(b.StudentName?.Trim(), studentName?.Trim(), StringComparison.OrdinalIgnoreCase)
                ).ToList();

                foreach (var borrow in approvedBorrows)
                {
                    DateTime dueDate = borrow.RequestDate.AddDays(1); // 24 hours validity
                    TimeSpan timeRemaining = dueDate - DateTime.Now;

                    string message = $"Your request for '{borrow.BookRequested}' has been approved! Due: {dueDate:yyyy-MM-dd HH:mm}";
                    string status = timeRemaining.TotalHours > 0 ?
                        $"{(int)timeRemaining.TotalHours}h {timeRemaining.Minutes}m left" :
                        "Overdue";

                    dgvNotifications.Rows.Add(
                        "Approved",
                        message,
                        borrow.RequestDate.ToString("yyyy-MM-dd HH:mm"),
                        status
                    );
                    dgvNotifications.Rows[dgvNotifications.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGreen;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading approved requests: {ex.Message}");
            }
        }

        private void LoadRejectedRequests()
        {
            try
            {
                var rejectService = new RejectService();
                var response = rejectService.GetRejectedBorrowsResponse();

                if (response == null || response.RejectedBorrows == null || response.RejectedBorrows.Count == 0) return;

                var myRejections = response.RejectedBorrows.Where(r =>
                    string.Equals(r.StudentName?.Trim(), studentName?.Trim(), StringComparison.OrdinalIgnoreCase)
                ).ToList();

                foreach (var reject in myRejections)
                {
                    string message = $"Your request for '{reject.BookRequested}' was rejected by the librarian";
                    dgvNotifications.Rows.Add(
                        "Rejected",
                        message,
                        reject.RequestDate.ToString("yyyy-MM-dd HH:mm"),
                        "Declined"
                    );
                    dgvNotifications.Rows[dgvNotifications.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightCoral;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading rejected requests: {ex.Message}");
            }
        }

        private void LoadOverdueBooks()
        {
            try
            {
                var borrowService = new BorrowService();
                var borrows = borrowService.GetBorrows();

                if (borrows == null) return;

                var myBorrows = borrows.Where(b =>
                    (b.Status == StudentLibrary.enumerator.BorrowStatus.Borrowed || b.Status == StudentLibrary.enumerator.BorrowStatus.Overdue) &&
                    string.Equals(b.StudentName?.Trim(), studentName?.Trim(), StringComparison.OrdinalIgnoreCase)
                ).ToList();

                foreach (var borrow in myBorrows)
                {
                    DateTime dueDate = borrow.RequestDate.AddDays(1); // 24 hours validity
                    TimeSpan timeRemaining = dueDate - DateTime.Now;

                    // Check if almost due (less than 2 hours remaining)
                    if (timeRemaining.TotalHours > 0 && timeRemaining.TotalHours <= 2)
                    {
                        string message = $"⚠️ URGENT: '{borrow.BookRequested}' is due soon! Return before {dueDate:HH:mm} to avoid fines";
                        dgvNotifications.Rows.Add(
                            "Due Soon",
                            message,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                            $"{(int)timeRemaining.TotalHours}h {timeRemaining.Minutes}m left"
                        );
                        dgvNotifications.Rows[dgvNotifications.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Orange;
                    }
                    // Check if overdue
                    else if (timeRemaining.TotalHours <= 0)
                    {
                        int hoursOverdue = (int)Math.Abs(timeRemaining.TotalHours);
                        decimal fine = hoursOverdue * 3; // ₱3 per hour

                        string message = $"❌ OVERDUE: '{borrow.BookRequested}' was due at {dueDate:yyyy-MM-dd HH:mm}. Please return immediately!";
                        dgvNotifications.Rows.Add(
                            "Overdue",
                            message,
                            dueDate.ToString("yyyy-MM-dd HH:mm"),
                            $"₱{fine} ({hoursOverdue}h)"
                        );
                        dgvNotifications.Rows[dgvNotifications.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Red;
                        dgvNotifications.Rows[dgvNotifications.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading overdue books: {ex.Message}");
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
            if (notificationWatcher != null)
            {
                notificationWatcher.EnableRaisingEvents = false;
                notificationWatcher.Dispose();
            }
        }
    }
}
