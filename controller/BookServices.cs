using Newtonsoft.Json;
using StudentLibrary.core;
using StudentLibrary.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace StudentLibrary.controller
{
    internal class BookServices
    {
        private Core core;
        public event EventHandler<List<Books>> OnBooksUpdated;

        public BookServices()
        {
            core = new Core();
        }   

        public List<Books> GetAllBooks()
        {
            try
            {
                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "getBooks"
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);

                // Read response with proper buffering
                byte[] buffer = new byte[65536]; // Increased buffer size
                int totalBytesRead = 0;
                int bytesRead;

                // Read until we get a complete response or no more data
                while ((bytesRead = core.stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    
                    // Check if we have a complete JSON response
                    string partialResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                    if (partialResponse.Contains("\"Status\"") && 
                        (partialResponse.Contains("\"Books\"") || partialResponse.Contains("\"Message\"")))
                    {
                        break;
                    }
                    
                    // Prevent infinite loop
                    if (totalBytesRead >= buffer.Length - 1)
                    {
                        break;
                    }
                }

                if (totalBytesRead <= 0)
                {
                    return new List<Books>();
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                Console.WriteLine($"Book response received: {jsonResponse.Length} bytes");

                BookResponse response = JsonConvert.DeserializeObject<BookResponse>(jsonResponse);
                return response?.Books ?? new List<Books>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching books: {ex.Message}");
                return new List<Books>();
            }
            finally
            {
                core.Disconnect();
            }
        }

        public List<Category> GetCategories()
        {
            try
            {
                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "getCategories"
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);
                core.stream.Flush();

                byte[] buffer = new byte[8192];
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
                    return new List<Category>();
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
                CategoryResponse response = JsonConvert.DeserializeObject<CategoryResponse>(jsonResponse);
                return response?.Categories ?? new List<Category>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories: {ex.Message}");
                return new List<Category>();
            }
            finally
            {
                core.Disconnect();
            }
        }

        public void StartListeningForUpdates()
        {
            Task.Run(() =>
            {
                try
                {
                    if (core.client == null || !core.client.Connected)
                    {
                        core.ConnectAsync().Wait();
                    }

                    var request = new
                    {
                        action = "listenUpdates"
                    };

                    string jsonRequest = JsonConvert.SerializeObject(request);
                    byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                    core.stream.Write(data, 0, data.Length);

                    while (core.client.Connected)
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                            break;

                        string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                            BookResponse response = JsonConvert.DeserializeObject<BookResponse>(jsonResponse);
                            if (response?.Books != null)
                            {
                                OnBooksUpdated?.Invoke(this, response.Books);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error listening for updates: {ex.Message}");
                }
            });
        }

        public void StopListeningForUpdates()
        {
            core.Disconnect();
        }
    }
}
