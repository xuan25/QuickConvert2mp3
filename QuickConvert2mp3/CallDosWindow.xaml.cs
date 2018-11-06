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
using System.Windows.Shapes;

using System.Diagnostics;
using System.Management;
//using System.Drawing;

namespace QuickConvert2mp3
{
    /// <summary>
    /// CallDosWindow.xaml 的交互逻辑
    /// </summary>
    
    // 1.定义委托 
    public delegate void DelReadStdOutput(string result);
    public delegate void DelReadErrOutput(string result);
    public partial class CallDosWindow : Window
    {
        // 2.定义委托事件  
        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;

        public CallDosWindow()
        {
            InitializeComponent();
            Init();
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.Left = SystemParameters.WorkArea.Size.Width/2 - this.Width/2;
            this.Top = SystemParameters.WorkArea.Size.Height/2 - this.Height/2;
        }
        private void Init()
        {
            //3.将相应函数注册到委托事件中  
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
            ReadErrOutput += new DelReadErrOutput(ReadErrOutputAction);
        }
        
        private int PID;  //记录线程PID

        //开始编码【入口】
        public void startCoding(string COMMAND)
        {
            PID = RealAction("cmd.exe", "/c " + COMMAND);  // 启动进程执行相应命令（同时获取进程PID
        }

        
        private int RealAction(string StartFileName, string StartFileArg)
        {
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = StartFileName;      // 命令  
            CmdProcess.StartInfo.Arguments = StartFileArg;      // 参数  

            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口  
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入  
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出  
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            //CmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;  

            CmdProcess.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            CmdProcess.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);

            CmdProcess.EnableRaisingEvents = true;                      // 启用Exited事件  
            CmdProcess.Exited += new EventHandler(CmdProcess_Exited);   // 注册进程结束事件  

            CmdProcess.Start();
            CmdProcess.BeginOutputReadLine();
            CmdProcess.BeginErrorReadLine();

            // 如果打开注释，则以同步方式执行命令，此例子中用Exited事件异步执行。  
            // CmdProcess.WaitForExit();  
            int PID = CmdProcess.Id;
            return PID;

        }

        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                // 4. 异步调用，需要invoke  
                //this.Dispatcher.Invoke(ReadStdOutput, new object[] { e.Data });
                this.Dispatcher.Invoke(ReadStdOutput, e.Data);
                Console.WriteLine("reO:" + e.Data);
            }
        }

        private void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                //this.Dispatcher.Invoke(ReadErrOutput, new object[] { e.Data });
                this.Dispatcher.Invoke(ReadErrOutput, e.Data);
                Console.WriteLine("reE:" + e.Data);
            }
        }

        private void ReadStdOutputAction(string result)
        {
            this.textBoxShowRet.AppendText("\r\n" + result);
            Console.WriteLine("printO:" + result);
            //Refresh();
        }

        private void ReadErrOutputAction(string result)
        {
            this.textBoxShowRet.AppendText("\r\n" + result);
            Console.WriteLine("printE:" + result);
            //Refresh();
        }

        private void CmdProcess_Exited(object sender, EventArgs e)
        {
            // 执行结束后触发
            //MessageBox.Show("转换完成");
            this.Dispatcher.Invoke(Hide);

        }

        public static void KillProcessAndChildren(int pid)  //终止线程函数
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                Console.WriteLine(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                /* process already exited */
            }
        }

        private void AllowDrag(object sender, MouseButtonEventArgs e)  //允许拖拽函数
        {
            this.DragMove();
        }

        //取消
        private void Cancel()
        {
            KillProcessAndChildren(PID);
            //System.Environment.Exit(0);
            textBoxShowRet.Text = "";
            this.Close();
        }

        private void CancelCheck(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("退出指令");
            MessageBoxResult result = MessageBox.Show("是否终止任务？", "快捷编码", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            //MessageBox.Show(result.ToString());
            if (result.ToString() == "Yes")
            {
                Cancel();
            }
        }
    }
}
