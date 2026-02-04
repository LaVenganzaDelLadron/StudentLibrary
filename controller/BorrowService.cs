using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentLibrary.core;
using StudentLibrary.model;
using StudentLibrary.view.modal;


namespace StudentLibrary.controller
{
    internal class BorrowService
    {
        private Core core;
        private static readonly string borrowFilePath = "borrow.json";
        public event EventHandler<List<Borrow>> OnBorrowsUpdated;
        private BookServices bookServices;
        private Dictionary<string, string> bookAuthorByTitle = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        public BorrowService()
        {
            core = new Core();
            bookServices = new BookServices();
        }

        public List<Borrow> GetBorrows()
        {
            try
            {
                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "getBorrows"
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);
                core.stream.Flush(); // Ensure data is sent

                byte[] buffer = new byte[65536];
                int totalBytesRead = 0;
                int bytesRead;

                // Read response with proper buffering
                while ((bytesRead = core.stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    
                    // Check if we have a complete JSON response
                    string partialResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                    if (partialResponse.Contains("\"Status\"") && 
                        (partialResponse.Contains("\"Borrows\"") || partialResponse.Contains("\"Message\"")))
                    {
                        break;
                    }
                    
                    if (totalBytesRead >= buffer.Length - 1)
                    {
                        break;
                    }
                }

                if (totalBytesRead <= 0)
                {
                    return new List<Borrow>();
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                BorrowResponse response = JsonConvert.DeserializeObject<BorrowResponse>(jsonResponse);

                return response?.Borrows ?? new List<Borrow>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching borrows: {ex.Message}");
                return new List<Borrow>();
            }
            finally
            {
                core.Disconnect();
            }
        }

        public List<Borrow> GetAllBorrows()
        {
            // First try to get from server via API (GetBorrows method)
            try
            {
                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "getBorrows"
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);
                core.stream.Flush();

                byte[] buffer = new byte[65536];
                int totalBytesRead = 0;
                int bytesRead;

                // Read response with proper buffering
                while ((bytesRead = core.stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    
                    // Check if we have a complete JSON response
                    string partialResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                    if (partialResponse.Contains("\"Status\"") && 
                        (partialResponse.Contains("\"Borrows\"") || partialResponse.Contains("\"Message\"")))
                    {
                        break;
                    }
                    
                    if (totalBytesRead >= buffer.Length - 1)
                    {
                        break;
                    }
                }

                if (totalBytesRead <= 0)
                {
                    return new List<Borrow>();
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                BorrowResponse response = JsonConvert.DeserializeObject<BorrowResponse>(jsonResponse);

                return response?.Borrows ?? new List<Borrow>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading borrows from server: {ex.Message}");
                return new List<Borrow>();
            }
            finally
            {
                core.Disconnect();
            }
        }

        public bool ReturnBook(string borrowId)
        {
            try
            {
                if (!Guid.TryParse(borrowId, out Guid id))
                {
                    return false;
                }

                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "returnBook",
                    borrowId = borrowId
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);
                core.stream.Flush();

                byte[] buffer = new byte[2048];
                int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    return false;
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);

                return response?.Status == "SUCCESS";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error returning book: {ex.Message}");
                return false;
            }
            finally
            {
                core.Disconnect();
            }
        }

        public bool MarkFineAsPaid(string borrowId)
        {
            try
            {
                if (!Guid.TryParse(borrowId, out Guid id))
                {
                    return false;
                }

                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "markFineAsPaid",
                    borrowId = borrowId
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);
                core.stream.Flush();

                byte[] buffer = new byte[2048];
                int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    return false;
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);

                return response?.Status == "SUCCESS";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking fine as paid: {ex.Message}");
                return false;
            }
            finally
            {
                core.Disconnect();
            }
        }

        public AuthResponse RequestBorrow(string studentName, string bookTitle)
        {
            try
            {
                core.ConnectAsync().Wait();

                if (core.stream == null)
                {
                    return new AuthResponse
                    {
                        Status = "FAILED",
                        Message = "Connection failed"
                    };
                }

                // Validate inputs
                if (string.IsNullOrWhiteSpace(studentName))
                {
                    return new AuthResponse
                    {
                        Status = "FAILED",
                        Message = "Student name is required"
                    };
                }

                if (string.IsNullOrWhiteSpace(bookTitle))
                {
                    return new AuthResponse
                    {
                        Status = "FAILED",
                        Message = "Book title is required"
                    };
                }

                var request = new
                {
                    action = "createBorrow",
                    studentName = studentName.Trim(),
                    bookTitle = bookTitle.Trim()
                };

                string json = JsonConvert.SerializeObject(request);
                byte[] requestData = Encoding.UTF8.GetBytes(json);
                core.stream.Write(requestData, 0, requestData.Length);
                core.stream.Flush(); // Ensure data is sent

                byte[] buffer = new byte[2048];
                int totalBytesRead = 0;
                int bytesRead;

                // Read response with proper buffering
                while ((bytesRead = core.stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    
                    // Check if we have a complete JSON response
                    string partialResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                    if (partialResponse.Contains("\"Status\""))
                    {
                        break;
                    }
                    
                    if (totalBytesRead >= buffer.Length - 1)
                    {
                        break;
                    }
                }

                if (totalBytesRead <= 0)
                {
                    return new AuthResponse
                    {
                        Status = "FAILED",
                        Message = "No response from server"
                    };
                }

                string responseJson = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                Console.WriteLine($"Server response: {responseJson}");
                AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(responseJson);

                return response ?? new AuthResponse
                {
                    Status = "FAILED",
                    Message = "Invalid response format"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error requesting borrow: {ex.Message}");
                return new AuthResponse
                {
                    Status = "FAILED",
                    Message = $"Error: {ex.Message}"
                };
            }
            finally
            {
                core.Disconnect();
            }
        }

        // Business logic methods moved from BorrowingHistory view

        public string GetDisplayStatus(Borrow borrow)
        {
            if (borrow == null)
                return "Unknown";

            // DEBUG: Log status value
            Console.WriteLine($"GetDisplayStatus - Book: {borrow.BookRequested}, Status: {borrow.Status} ({(int)borrow.Status}), ReturnDate: {borrow.ReturnDate}, ReceivedByLibrarian: {borrow.ReceivedByLibrarian}");

            // Returned (final state - librarian confirmed)
            if (borrow.ReceivedByLibrarian)
            {
                Console.WriteLine($"  -> Returning 'Returned' (ReceivedByLibrarian=true)");
                return "Returned";
            }

            // Returning (student clicked return but librarian not confirmed yet)
            // Only if ReturnDate is set to a real date (not DateTime.MinValue)
            if (borrow.ReturnDate != DateTime.MinValue)
            {
                Console.WriteLine($"  -> Returning 'Returning' (ReturnDate is set)");
                return "Returning";
            }

            // Borrowed or Overdue
            if (borrow.Status == enumerator.BorrowStatus.Borrowed)
            {
                DateTime dueDate = borrow.RequestDate.AddDays(1);
                Console.WriteLine($"  -> Status is Borrowed, DueDate: {dueDate}, Now: {DateTime.Now}");
                
                if (DateTime.Now > dueDate)
                {
                    Console.WriteLine($"  -> Returning 'Overdue'");
                    return "Overdue";
                }
                
                Console.WriteLine($"  -> Returning 'Borrowed'");
                return "Borrowed";
            }

            // Pending request
            if (borrow.Status == enumerator.BorrowStatus.Pending)
            {
                Console.WriteLine($"  -> Returning 'Pending'");
                return "Pending";
            }

            // Lost
            if (borrow.Status == enumerator.BorrowStatus.Lost)
            {
                Console.WriteLine($"  -> Returning 'Lost'");
                return "Lost";
            }

            // Explicit Returning status
            if (borrow.Status == enumerator.BorrowStatus.Returning)
            {
                Console.WriteLine($"  -> Returning 'Returning' (Status enum)");
                return "Returning";
            }

            // Safety fallback
            Console.WriteLine($"  -> Warning: Unexpected status, returning '{borrow.Status}'");
            return borrow.Status.ToString();
        }

        public List<Borrow> ApplyFilters(List<Borrow> borrows, string currentFilter, string searchText)
        {
            var filtered = borrows;

            if (currentFilter != "All Status")
            {
                filtered = filtered.Where(b => GetDisplayStatus(b) == currentFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.ToLowerInvariant();
                filtered = filtered.Where(b =>
                    (b.BookRequested != null && b.BookRequested.ToLowerInvariant().Contains(search)) ||
                    GetAuthorForTitle(b.BookRequested).ToLowerInvariant().Contains(search)).ToList();
            }

            return filtered;
        }

        public SummaryData GetSummaryData(List<Borrow> borrows)
        {
            int totalBorrowed = borrows.Count;
            int overdueCount = borrows.Count(b => GetDisplayStatus(b) == "Overdue");
            decimal totalPenalty = 0;

            foreach (var borrow in borrows)
            {
                DateTime dueDate = borrow.RequestDate.AddDays(1);
                if (borrow.Status != enumerator.BorrowStatus.Returned && borrow.Status != enumerator.BorrowStatus.Pending)
                {
                    if (DateTime.Now > dueDate)
                    {
                        TimeSpan overduePeriod = DateTime.Now - dueDate;
                        long hoursOverdue = (long)Math.Ceiling(overduePeriod.TotalHours);
                        totalPenalty += hoursOverdue * 3;
                    }
                }
            }

            return new SummaryData
            {
                TotalBorrowed = totalBorrowed,
                TotalPenalty = totalPenalty,
                OverdueCount = overdueCount
            };
        }

        public decimal CalculatePenalty(Borrow borrow)
        {
            decimal penalty = 0;
            DateTime dueDate = borrow.RequestDate.AddDays(1);

            // Only calculate penalty for active borrows (not Pending, Returning, or Returned)
            if (borrow.Status != enumerator.BorrowStatus.Returned && 
                borrow.Status != enumerator.BorrowStatus.Returning &&
                borrow.Status != enumerator.BorrowStatus.Pending)
            {
                if (DateTime.Now > dueDate)
                {
                    TimeSpan overduePeriod = DateTime.Now - dueDate;
                    long hoursOverdue = (long)Math.Ceiling(overduePeriod.TotalHours);
                    penalty = hoursOverdue * 3; // ₱3 per hour
                }
            }

            return penalty;
        }

        public void LoadBookAuthors()
        {
            if (bookAuthorByTitle.Count > 0)
            {
                return;
            }

            try
            {
                var books = bookServices.GetAllBooks();
                foreach (var book in books)
                {
                    if (!string.IsNullOrWhiteSpace(book.Title) && !bookAuthorByTitle.ContainsKey(book.Title))
                    {
                        bookAuthorByTitle[book.Title] = string.IsNullOrWhiteSpace(book.Author) ? "Unknown" : book.Author;
                    }
                }
            }
            catch
            {
                // Ignore author lookup errors
            }
        }

        public string GetAuthorForTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "Unknown";
            }

            if (bookAuthorByTitle.TryGetValue(title, out string author))
            {
                return author;
            }

            return "Unknown";
        }
    }

    // Helper class for summary data
    public class SummaryData
    {
        public int TotalBorrowed { get; set; }
        public decimal TotalPenalty { get; set; }
        public int OverdueCount { get; set; }
    }
}

