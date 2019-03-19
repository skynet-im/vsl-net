using System;
using System.IO;
using System.Windows.Forms;
using VSL;

namespace VSLTest
{
    public static class Program
    {
        public static int Connects = 0;
        public static int Disconnects = 0;
        public static string TempPath;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CreateTempFolder();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void CreateTempFolder()
        {
            try
            {
                string path;

                path = Environment.GetEnvironmentVariable("VSLTest_Temp");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    TempPath = Directory.CreateDirectory(path).FullName;
                    return;
                }

                path = Environment.GetEnvironmentVariable("TEMP");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    TempPath = Directory.CreateDirectory(Path.Combine(path, "VSLTest")).FullName;
                    return;
                }
            }
            catch { }

            MessageBox.Show("The temporary folder could not be created. File transfer won't work.", "Folder missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Log(VSLSocket socket, string message)
        {
            Console.WriteLine($"[{socket.GetType()}] {message}");
        }
    }
}