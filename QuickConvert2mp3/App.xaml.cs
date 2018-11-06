using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QuickConvert2mp3
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        Window window;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            window = new MainWindow(sender, e);
            window.Show();
        }
    }
}
