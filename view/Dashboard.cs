using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using StudentLibrary.view.modal;

namespace StudentLibrary.view
{
    public partial class Dashboard : Form
    {
        private readonly string username;
        private readonly string firstName;
        private readonly string lastName;
        private HomePage homePage;
        private BrowseBookPage browseBookPage;
        BorrowingHistory borrowingHistory;
        private NotificationsPage notifications;
        private Logout logout = new Logout();



        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
            (
                int nLeftRect,
                int nTopRect,
                int nRightRec,
                int nBottomRect,
                int nWidthEllipse,
                int nHeightEllipse
            );

        public Dashboard()
        {
            InitializeComponent();

            pnlNav.Height = btnDashboard.Height;
            pnlNav.Top = btnDashboard.Top;
            pnlNav.Left = btnDashboard.Left;

            // Add click event to logo for refresh functionality
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Click += PictureBox1_Click;

            homePage = new HomePage();
            LoadForm(homePage);
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            // Refresh all data by recreating page instances
            string fullName = $"{firstName} {lastName}".Trim();
            string nameToPass = !string.IsNullOrWhiteSpace(fullName) ? fullName : username;
            
            homePage = string.IsNullOrWhiteSpace(nameToPass) ? new HomePage() : new HomePage(nameToPass);
            browseBookPage = new BrowseBookPage("", username, fullName, this);
            
            if (!string.IsNullOrWhiteSpace(fullName) || !string.IsNullOrWhiteSpace(username))
            {
                string notificationFilter = string.IsNullOrWhiteSpace(fullName) ? username : fullName;
                notifications = new NotificationsPage(notificationFilter, username);
            }
            
            // Determine which page is currently loaded and reload it
            if (pnlFormLoader.Controls.Count > 0)
            {
                Form currentForm = pnlFormLoader.Controls[0] as Form;
                
                if (currentForm is HomePage)
                {
                    LoadForm(homePage);
                    pnlNav.Height = btnDashboard.Height;
                    pnlNav.Top = btnDashboard.Top;
                }
                else if (currentForm is BrowseBookPage)
                {
                    LoadForm(browseBookPage);
                    pnlNav.Height = btnBooks.Height;
                    pnlNav.Top = btnBooks.Top;
                }
                else if (currentForm is NotificationsPage)
                {
                    LoadForm(notifications);
                    pnlNav.Height = btnNotifications.Height;
                    pnlNav.Top = btnNotifications.Top;
                }
            }

            MessageBox.Show("Data refreshed successfully!", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public Dashboard(string username, string firstName = "", string lastName = "") : this()
        {
            this.username = username;
            this.firstName = firstName;
            this.lastName = lastName;
            if (!string.IsNullOrWhiteSpace(username))
            {
                lblUsername.Text = username;
                // Pass full name (FirstName + LastName) to HomePage instead of just username
                string fullName = $"{firstName} {lastName}".Trim();
                string nameToPass = !string.IsNullOrWhiteSpace(fullName) ? fullName : username;
                homePage = new HomePage(nameToPass);
                LoadForm(homePage);
            }
        }

        public string GetStudentFirstName()
        {
            return firstName;
        }

        public string GetStudentLastName()
        {
            return lastName;
        }

        public void RefreshBorrowingPage()
        {
            // This method allows child forms to trigger a refresh
            // Currently just a placeholder for future enhancements
        }




        private void LoadForm(Form form)
        {
            try
            {
                pnlFormLoader.Controls.Clear();
                form.TopLevel = false;
                form.Dock = DockStyle.Fill;
                pnlFormLoader.Controls.Add(form);
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message);
            }
        }


        private void btnDashboard_Click(object sender, EventArgs e)
        {
            pnlNav.Height = btnDashboard.Height;
            pnlNav.Top = btnDashboard.Top;
            pnlNav.Left = btnDashboard.Left;
            
            if (homePage == null || homePage.IsDisposed)
            {
                string fullName = $"{firstName} {lastName}".Trim();
                string nameToPass = !string.IsNullOrWhiteSpace(fullName) ? fullName : username;
                homePage = string.IsNullOrWhiteSpace(nameToPass) ? new HomePage() : new HomePage(nameToPass);
            }
            
            lblTitle.Text = "Dashboard";

            LoadForm(homePage);
        }

        private void btnBooks_Click(object sender, EventArgs e)
        {
            pnlNav.Height = btnBooks.Height;
            pnlNav.Top = btnBooks.Top;
            pnlNav.Left = btnBooks.Left;
            
            if (browseBookPage == null || browseBookPage.IsDisposed)
            {
                string fullName = $"{firstName} {lastName}".Trim();
                browseBookPage = new BrowseBookPage("", username, fullName, this);
            }

            lblTitle.Text = "Books";

            LoadForm(browseBookPage);
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            pnlNav.Height = btnHistory.Height;
            pnlNav.Top = btnHistory.Top;
            pnlNav.Left = btnHistory.Left;
           
            string fullName = $"{firstName} {lastName}".Trim();
            borrowingHistory = new BorrowingHistory(fullName);
            lblTitle.Text = "Borrowing History";
            LoadForm(borrowingHistory);
        }

        private void btnNotifications_Click(object sender, EventArgs e)
        {
            pnlNav.Height = btnNotifications.Height;
            pnlNav.Top = btnNotifications.Top;
            pnlNav.Left = btnNotifications.Left;

            if (notifications == null || notifications.IsDisposed)
            {
                string fullName = $"{firstName} {lastName}".Trim();
                string nameFilter = string.IsNullOrWhiteSpace(fullName) ? username : fullName;
                notifications = new NotificationsPage(nameFilter, username);
            }

            lblTitle.Text = "Notifications";

            LoadForm(notifications);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            pnlNav.Height = btnLogout.Height;
            pnlNav.Top = btnLogout.Top;
            pnlNav.Left = btnLogout.Left;

            logout.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            browseBookPage?.Close();
            homePage?.Close();
            base.OnFormClosing(e);
        }
    }
}
