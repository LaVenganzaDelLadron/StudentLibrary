using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudentLibrary.core;

namespace StudentLibrary
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize and connect to server on background thread
            var core = new Core();
            _ = Task.Run(async () => await core.ConnectAsync());

            // Wait a moment for connection to establish
            System.Threading.Thread.Sleep(500);

            Application.Run(new Login());
        }
    }
}
