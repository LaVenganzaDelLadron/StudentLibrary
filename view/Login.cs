using StudentLibrary.view;
using StudentLibrary.controller;
using StudentLibrary.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace StudentLibrary
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string email = textBoxEmail.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter email and password", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                AuthService authService = new AuthService();
                AuthResponse response = authService.Login(email, password);

                if (response == null)
                {
                    MessageBox.Show("Login failed: empty response from server.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (response.Status == "SUCCESS")
                {
                    MessageBox.Show($"Login successful! Welcome {response.Username}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Create student folder structure with full name only
                    string fullName = $"{response.FirstName} {response.LastName}".Trim();
                    CreateStudentFolderStructure(fullName);
                    
                    Dashboard dashboard = new Dashboard(response.Username, response.FirstName, response.LastName);
                    dashboard.FormClosed += (s, args) => this.Close();
                    dashboard.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show(response.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateStudentFolderStructure(string studentFullName)
        {
            try
            {
                // Use shared LibraryManagementSystem bin/Debug/Data/notifications folder
                // Only use full name (firstName + lastName) - no username-based folders
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string libraryManagementPath = Path.Combine(desktopPath, "LibraryManagementSystem", "bin", "Debug", "Data");
                string studentFolderPath = Path.Combine(libraryManagementPath, "notifications", studentFullName);
                
                // Create folder if it doesn't exist
                if (!Directory.Exists(studentFolderPath))
                {
                    Directory.CreateDirectory(studentFolderPath);
                }

                // Create notifications.json if it doesn't exist
                string notificationsFile = Path.Combine(studentFolderPath, "notifications.json");
                if (!File.Exists(notificationsFile))
                {
                    var emptyNotifications = new List<object>();
                    string json = JsonConvert.SerializeObject(emptyNotifications, Formatting.Indented);
                    File.WriteAllText(notificationsFile, json);
                }

                Console.WriteLine($"Student folder structure created for: {studentFullName} at {studentFolderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating student folder structure: {ex.Message}");
            }
        }

        private void checkBoxShowpassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = !checkBoxShowpassword.Checked;
        }
    }
}
