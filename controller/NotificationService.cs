using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentLibrary.core;
using StudentLibrary.model;

namespace StudentLibrary.controller
{
    internal class NotificationService
    {
        private Core core;

        public NotificationService()
        {
            core = new Core();
        }

        public List<Notification> GetStudentNotifications(string studentId)
        {
            try
            {
                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "getStudentNotifications",
                    studentId = studentId
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    return new List<Notification>();
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                NotificationResponse response = JsonConvert.DeserializeObject<NotificationResponse>(jsonResponse);

                return response?.Notifications ?? new List<Notification>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
                return new List<Notification>();
            }
            finally
            {
                core.Disconnect();
            }
        }
    }
}
