using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace QuickConvert2mp3
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private void exportResource(string path, string source)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + path))
            {
                //释放资源到磁盘
                String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + "." + source))
                {
                    Byte[] b = new Byte[stream.Length];
                    stream.Read(b, 0, b.Length);
                    string s = AppDomain.CurrentDomain.BaseDirectory + path;
                    using (FileStream f = File.Create(s))
                    {
                        f.Write(b, 0, b.Length);
                    }
                }
            }
        }
        public MainWindow(object sender, StartupEventArgs e)
        {
            if(!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "support"))
            {
                MessageBox.Show("欢迎使用 快捷转换(MP3)工具 by 瑄\n\n本工具可在音视频文件的右键菜单中添加音频转换的选项\n使用此工具可以更加方便的对文件中的音频进行处理\n\n创意:饭団子\n制作:瑄\n\nFFMPEG版本: 3.3.3 32bits\nLAME版本: 3.99.5 32bits", "初始化", MessageBoxButton.OK, MessageBoxImage.Information);
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "support");
            }
            exportResource("support\\lame.exe", "support.lame.exe");
            exportResource("support\\ffmpeg.exe", "support.ffmpeg.exe");
            //MessageBox.Show("工作路径:" + AppDomain.CurrentDomain.BaseDirectory);
            //MessageBox.Show("程序路径:" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            //MessageBox.Show("参数1：" + e.Args[0]);
            //MessageBox.Show("参数2：" + e.Args[1]);
            if (e.Args.Length == 0)
            {
                //MessageBox.Show("无参数启动，管理模式");
                //面板初始化
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                if (!(principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator)))
                {
                    //创建启动对象 
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    //设置运行文件 
                    startInfo.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    //设置启动动作,确保以管理员身份运行 
                    startInfo.Verb = "runas";
                    //如果不是管理员，则启动UAC
                    try
                    {
                        System.Diagnostics.Process.Start(startInfo);
                    }catch (Exception){}
                    //退出 
                    System.Environment.Exit(0);
                }
            }
            else
            {
                //MessageBox.Show("含参启动，处理模式");
                //编码初始化
                process(e.Args[0], e.Args[1]);
            }
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Topmost = true;
            InitializeComponent();
        }

        RegistryKey Key = Registry.ClassesRoot;
        private void add_Click(object sender, RoutedEventArgs e)
        {
            state.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 0));
            state.Content = "设定中...";

            string cmd1 = "转换为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")";
            string cmd2 = "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\" \"%1\" \"" + rate.Text + "\"";
            string P1 = "SystemFileAssociations\\audio\\shell\\转换为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")";
            string P2 = "SystemFileAssociations\\audio\\shell\\转换为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")\\command";
            RegistryKey rightCommondKey1 = Key.CreateSubKey(P1);
            rightCommondKey1.SetValue(string.Empty, cmd1);
            rightCommondKey1.Close();
            RegistryKey rightCommondKey2 = Key.CreateSubKey(P2);
            rightCommondKey2.SetValue(string.Empty, cmd2);
            rightCommondKey2.Close();

            string cmd3 = "提取音轨为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")";
            string cmd4 = "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\" \"%1\" \"" + rate.Text + "\"";
            string P3 = "SystemFileAssociations\\video\\shell\\提取音轨为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")";
            string P4 = "SystemFileAssociations\\video\\shell\\提取音轨为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")\\command";
            RegistryKey rightCommondKey3 = Key.CreateSubKey(P3);
            rightCommondKey3.SetValue(string.Empty, cmd3);
            rightCommondKey3.Close();
            RegistryKey rightCommondKey4 = Key.CreateSubKey(P4);
            rightCommondKey4.SetValue(string.Empty, cmd4);
            rightCommondKey4.Close();

            state.Foreground = new SolidColorBrush(Color.FromRgb(20, 200, 20));
            state.Content = "设定完成！";
        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            state.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 0));
            state.Content = "移除中...";
            string P1 = "SystemFileAssociations\\audio\\shell\\转换为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")";
            string P2 = "SystemFileAssociations\\video\\shell\\提取音轨为" + rate.Text + "kbps(mp3)(&" + hotKey.Text + ")";
            try
            {
                Key.DeleteSubKeyTree(P1);
                Key.DeleteSubKeyTree(P2);
                state.Foreground = new SolidColorBrush(Color.FromRgb(20, 20, 200));
                state.Content = "移除完成！";
            }
            catch (Exception)
            {
                state.Foreground = new SolidColorBrush(Color.FromRgb(200, 20, 20));
                state.Content = "未找到快捷键，请确认参数正确";
            }
        }

        private void AllowDrag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void process(string Arg1, string Arg2)
        {
            string COMMAND = AppDomain.CurrentDomain.BaseDirectory + "support\\ffmpeg.exe -i \"" + Arg1 + "\" -vn -sn -v 0 -c:a pcm_s16le -f wav pipe:  | " + AppDomain.CurrentDomain.BaseDirectory + "support\\lame.exe -b " + Arg2 + " - \"" + Arg1 + "_" + Arg2 + "kbps.mp3\"";
            //MessageBox.Show(COMMAND);
            CallDosWindow cdw = new CallDosWindow();
            cdw.textBoxShowRet.Text = "COMMAND: " + COMMAND;
            cdw.startCoding(COMMAND);
            cdw.ShowDialog();

            System.Environment.Exit(0);
        }

        private void rate_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);
            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                int num = 0;
                if (!Int32.TryParse(textBox.Text, out num))
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                }
            }
            if (textBox.Text.Length > 3)
            {
                int m = textBox.SelectionStart;
                textBox.Text = textBox.Text.Remove(3);
                textBox.SelectionStart = m;
            }
        }

        private void hotKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text.Length > 1)
            {
                textBox.Text = textBox.Text.Remove(1);
            }
            textBox.Text = textBox.Text.ToUpper();
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("在管理面板中输入码率和快捷键\n单击操作按钮即可生成或移除右键菜单\n\n若右键菜单失效，可尝试启动管理面板并重新设定右键菜单\n\n文件处理时会弹出提示框\n使用鼠标左键可拖拽提示框位置\n使用鼠标右键单击可终止任务", "帮助", MessageBoxButton.OK, MessageBoxImage.Question);
        }
    }
}
