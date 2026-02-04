using Newtonsoft.Json;
using StudentLibrary.core;
using StudentLibrary.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentLibrary.controller
{
    internal class StudentService
    {
        private Core core;

        public StudentService()
        {
            core = new Core(); // Your TcpClient wrapper
        }

        // Get all students from server
        public List<Users> GetAllStudents()
        {
            try
            {
                // Connect to server
                core.ConnectAsync().Wait();

                // Build request
                var request = new
                {
                    action = "getStudents"
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                // Send request
                core.stream.Write(data, 0, data.Length);

                // Receive response
                byte[] buffer = new byte[4096];
                int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                StudentResponse response = JsonConvert.DeserializeObject<StudentResponse>(jsonResponse);

                return response?.Students ?? new List<Users>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching students: {ex.Message}");
                return new List<Users>();
            }
            finally
            {
                core.Disconnect();
            }
        }

        // Optional: search students by keyword
        public List<Users> SearchStudents(string keyword)
        {
            try
            {
                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "searchStudents",
                    keyword = keyword
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                core.stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                StudentResponse response = JsonConvert.DeserializeObject<StudentResponse>(jsonResponse);

                return response?.Students ?? new List<Users>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching students: {ex.Message}");
                return new List<Users>();
            }
            finally
            {
                core.Disconnect();
            }
        }
    }
}
