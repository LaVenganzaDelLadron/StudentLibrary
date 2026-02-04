using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentLibrary.model;

namespace StudentLibrary.controller
{
    internal class HomeService
    {
        private BorrowService borrowService;

        public HomeService()
        {
            borrowService = new BorrowService();
        }

        public DashboardStats GetDashboardStats(string studentName)
        {
            Console.WriteLine($"HomeService: Getting stats for student '{studentName}'");
            
            try
            {
                var allBorrows = borrowService.GetAllBorrows();
                Console.WriteLine($"HomeService: Total borrows in database: {allBorrows.Count}");
                
                if (allBorrows.Count == 0)
                {
                    Console.WriteLine("HomeService: No borrows found, returning empty stats");
                    return new DashboardStats
                    {
                        BooksBorrowed = 0,
                        DueSoon = 0,
                        Overdue = 0,
                        TotalFines = 0
                    };
                }
                
                // Log all unique student names to help debug
                var uniqueNames = allBorrows.Select(b => b.StudentName).Distinct().ToList();
                Console.WriteLine($"HomeService: Unique student names in database: {string.Join(", ", uniqueNames.Select(n => $"'{n}'"))}");
                Console.WriteLine($"HomeService: Searching for student: '{studentName}'");
                
                // Filter by student name (case-insensitive comparison)
                // Try exact match first, then try partial match for flexibility
                var studentBorrows = allBorrows
                    .Where(b => string.Equals(b.StudentName, studentName, StringComparison.OrdinalIgnoreCase) ||
                                b.StudentName.ToLower().Contains(studentName.ToLower()) ||
                                studentName.ToLower().Contains(b.StudentName.Split(' ')[0].ToLower())) // Match by first name
                    .ToList();
                    
                Console.WriteLine($"HomeService: Found {studentBorrows.Count} borrows matching '{studentName}'");
                
                if (studentBorrows.Count > 0)
                {
                    Console.WriteLine($"HomeService: Matched student borrows:");
                    foreach (var b in studentBorrows)
                    {
                        Console.WriteLine($"  - StudentName: '{b.StudentName}', Book: '{b.BookRequested}', Status: {b.Status}");
                    }
                }

                var stats = new DashboardStats();

                if (studentBorrows.Count == 0)
                {
                    Console.WriteLine($"HomeService: WARNING - No borrows found for '{studentName}'. Check if student name matches database.");
                    return stats; // Return empty stats
                }

                // Count currently borrowed books (Borrowed status only, exclude Overdue/Returning/Returned/Pending)
                stats.BooksBorrowed = studentBorrows.Count(b => 
                {
                    var status = borrowService.GetDisplayStatus(b);
                    Console.WriteLine($"  - Book: {b.BookRequested}, Status: {status}");
                    return status == "Borrowed";
                });

                // Count books due within 10 hours (only counting active Borrowed books)
                DateTime tenHoursFromNow = DateTime.Now.AddHours(10);
                var dueSoonBooks = studentBorrows.Where(b =>
                {
                    var status = borrowService.GetDisplayStatus(b);
                    if (status != "Borrowed") return false;
                    
                    DateTime dueDate = b.RequestDate.AddDays(1);
                    return DateTime.Now < dueDate && dueDate <= tenHoursFromNow;
                }).ToList();
                
                stats.DueSoon = dueSoonBooks.Count();
                
                // Calculate remaining time for the book due soonest
                if (dueSoonBooks.Count > 0)
                {
                    var soonestBook = dueSoonBooks.OrderBy(b => b.RequestDate.AddDays(1)).FirstOrDefault();
                    if (soonestBook != null)
                    {
                        DateTime dueDate = soonestBook.RequestDate.AddDays(1);
                        TimeSpan remaining = dueDate - DateTime.Now;
                        if (remaining.TotalHours > 0)
                        {
                            stats.RemainingTimeDisplay = $"Remaining {remaining.Hours:D2}:{remaining.Minutes:D2}";
                        }
                        else
                        {
                            stats.RemainingTimeDisplay = "Time expired!";
                        }
                    }
                }
                else
                {
                    stats.RemainingTimeDisplay = "No books due soon";
                }

                // Count overdue books
                stats.Overdue = studentBorrows.Count(b => borrowService.GetDisplayStatus(b) == "Overdue");

                // Calculate total unpaid fines (for Overdue books)
                stats.TotalFines = 0;
                foreach (var borrow in studentBorrows)
                {
                    var status = borrowService.GetDisplayStatus(borrow);
                    // Only charge fines for overdue books that haven't been paid
                    if (status == "Overdue" && !borrow.FinePaid)
                    {
                        stats.TotalFines += borrowService.CalculatePenalty(borrow);
                    }
                }

                Console.WriteLine($"HomeService: Stats calculated - Borrowed: {stats.BooksBorrowed}, Due Soon: {stats.DueSoon}, Overdue: {stats.Overdue}, Fines: ₱{stats.TotalFines:F2}");
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HomeService Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new DashboardStats
                {
                    BooksBorrowed = 0,
                    DueSoon = 0,
                    Overdue = 0,
                    TotalFines = 0
                };
            }
        }

        public List<RecentActivity> GetRecentActivities(string studentName, int maxItems = 5)
        {
            Console.WriteLine($"HomeService: Getting recent activities for student '{studentName}'");
            
            try
            {
                var allBorrows = borrowService.GetAllBorrows();
                Console.WriteLine($"HomeService: Total borrows in database: {allBorrows.Count}");
                
                // Filter by student name and get status for each borrow
                var studentBorrows = allBorrows
                    .Where(b => string.Equals(b.StudentName, studentName, StringComparison.OrdinalIgnoreCase))
                    .Select(b => new
                    {
                        Borrow = b,
                        Status = borrowService.GetDisplayStatus(b)
                    })
                    .OrderByDescending(x => GetActivityDate(x.Borrow, x.Status))
                    .Take(maxItems)
                    .ToList();
                
                Console.WriteLine($"HomeService: Found {studentBorrows.Count} activities for '{studentName}'");

                borrowService.LoadBookAuthors();

                var activities = new List<RecentActivity>();
                foreach (var item in studentBorrows)
                {
                    var borrow = item.Borrow;
                    var status = item.Status;
                    string description = GetActivityDescription(borrow, status);

                    activities.Add(new RecentActivity
                    {
                        BookTitle = borrow.BookRequested,
                        Description = description,
                        Status = status,
                        Date = GetActivityDate(borrow, status)
                    });
                }

                return activities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HomeService Error in GetRecentActivities: {ex.Message}");
                return new List<RecentActivity>();
            }
        }

        private DateTime GetActivityDate(Borrow borrow, string status)
        {
            switch (status)
            {
                case "Returned":
                    return borrow.ReturnDate;

                case "Returning":
                    return borrow.ReturnDate;

                case "Overdue":
                    return borrow.RequestDate.AddDays(1); // due date

                default: // Borrowed, Pending
                    return borrow.RequestDate;
            }
        }

        private string GetActivityDescription(Borrow borrow, string status)
        {
            switch (status)
            {
                case "Pending":
                    return $"Requested on {borrow.RequestDate:MMM dd}";
                case "Borrowed":
                    DateTime dueDate = borrow.RequestDate.AddDays(1);
                    return $"Due {dueDate:MMM dd}";
                case "Overdue":
                    DateTime overdueSince = borrow.RequestDate.AddDays(1);
                    TimeSpan overduePeriod = DateTime.Now - overdueSince;
                    int daysOverdue = (int)Math.Ceiling(overduePeriod.TotalDays);
                    return $"Overdue by {daysOverdue} day(s)";
                case "Returning":
                    return $"Return pending confirmation";
                case "Returned":
                    return $"Returned on {borrow.ReturnDate:MMM dd}";
                default:
                    return "";
            }
        }
    }

    public class DashboardStats
    {
        public int BooksBorrowed { get; set; }
        public int DueSoon { get; set; }
        public int Overdue { get; set; }
        public decimal TotalFines { get; set; }
        public string RemainingTimeDisplay { get; set; } = "No books due soon";
    }

    public class RecentActivity
    {
        public string BookTitle { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }
}
