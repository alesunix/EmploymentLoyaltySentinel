using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SendMailOffenders
{
    internal class Program
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [STAThread]
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);/// скрыть консоль

            var icon = new NotifyIcon
            {
                Icon = new Icon("favicon.ico"),
                Text = "Отправка писем о нарушителях",
                Visible = true
            };
            icon.DoubleClick += Icon_DoubleClick;

            MainModel model = new MainModel();
            model.Init();

            Application.Run();
            Console.ReadLine();
        }
        private static void Icon_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите закрыть приложение?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Environment.Exit(0);
                Application.Exit();
            }
        }
    }
}
