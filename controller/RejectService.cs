using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using StudentLibrary.core;
using StudentLibrary.model;

namespace StudentLibrary.controller
{
    internal class RejectService
    {
        private Core core;

        public RejectService()
        {
            core = new Core();
        }

        public RejectResponse GetRejectedBorrowsResponse()
        {
            try
            {
                core.ConnectAsync().Wait();

                var request = new
                {
                    action = "getRejectedBorrows"
                };

                string jsonRequest = JsonConvert.SerializeObject(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                core.stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    return new RejectResponse
                    {
                        Status = "FAILED",
                        Message = "No response from server",
                        RejectedBorrows = new List<Reject>()
                    };
                }

                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                RejectResponse response = JsonConvert.DeserializeObject<RejectResponse>(jsonResponse);

                if (response == null)
                {
                    return new RejectResponse
                    {
                        Status = "FAILED",
                        Message = "Invalid response format",
                        RejectedBorrows = new List<Reject>()
                    };
                }

                response.RejectedBorrows = response.RejectedBorrows ?? new List<Reject>();
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rejected borrows: {ex.Message}");
                return new RejectResponse
                {
                    Status = "FAILED",
                    Message = ex.Message,
                    RejectedBorrows = new List<Reject>()
                };
            }
            finally
            {
                core.Disconnect();
            }
        }
    }
}
