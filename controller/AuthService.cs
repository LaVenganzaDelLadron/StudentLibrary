using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentLibrary.model;
using StudentLibrary.core;

namespace StudentLibrary.controller
{
    internal class AuthService
    {
        private Core core;

        public AuthService()
        {
            core = new Core();
        }

        public AuthResponse Login(string Email, string Password)
        {
            try
            {
                core.ConnectAsync().Wait();

                if (core.stream == null)
                {
                    return new AuthResponse
                    {
                        Status = "FAILED",
                        Message = "Connection failed: no stream available."
                    };
                }

                var request = new
                {
                    action = "login",
                    email = Email,
                    password = Password
                };

                string json = JsonConvert.SerializeObject(request);
                byte[] requestData = Encoding.UTF8.GetBytes(json);
                core.stream.Write(requestData, 0, requestData.Length);

                byte[] buffer = new byte[2048];
                int bytesRead = core.stream.Read(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    core.Disconnect();
                    return new AuthResponse
                    {
                        Status = "FAILED",
                        Message = "Connection failed: empty response from server."
                    };
                }

                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                core.Disconnect();

                var response = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                if (response == null)
                {
                    return new AuthResponse
                    {
                        Status = "FAILED",
                        Message = "Connection failed: invalid response from server."
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Status = "FAILED",
                    Message = $"Connection failed: {ex.Message}"
                };
            }
        }

    }
}
