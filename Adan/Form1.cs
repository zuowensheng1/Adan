using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Adan
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, string lParam);
        public Form1()
        {
            InitializeComponent();
        }

        DateTime t1 = DateTime.Now;
        DateTime t2 = DateTime.Now;
        IntPtr childHwnd1 = IntPtr.Zero;
        IntPtr childHwnd2 = IntPtr.Zero;
        IntPtr childHwnd3 = IntPtr.Zero;
        IntPtr childHwnd4 = IntPtr.Zero;
        IntPtr childHwnd5 = IntPtr.Zero;//刷新
        List<string> codes = new List<string>();
        Dictionary<string, int> JudgeOfCodes = new Dictionary<string, int>();
        const int WM_SETTEXT = 0x000C;//文本类型参数
        const int WM_CLICK = 0x00F5;
        private void Buy(string code, decimal price)
        {
            SendMessage(childHwnd1, WM_SETTEXT, 0, code);

            SendMessage(childHwnd2, WM_SETTEXT, 0, price.ToString());

            SendMessage(childHwnd3, WM_SETTEXT, 0, "400");
            Thread.Sleep(105);

            SendMessage(childHwnd4, WM_CLICK, 0, "0");

            //_run = !User32Util.SpyStockForm();
            _run = false;
        }

        public void CatchTenPercent(List<string> codeList)
        {
            String codesName = "";
            for (int i = 0; i < codeList.Count(); i++)
            {
                string code = codeList[i];
                if (code.StartsWith("60"))
                {
                    code = "sh" + code;
                }
                else if (code.StartsWith("30") || code.StartsWith("00"))
                {
                    code = "sz" + code;
                }
                if (i == 0)
                {
                    codesName += code;
                }
                else
                {
                    codesName += "," + code;
                }
            }

            _run = true;
            
            ParameterizedThreadStart th1 = new ParameterizedThreadStart(SpyWork);
            Thread thread = new Thread(th1);
            thread.Start(codesName);
        }
        bool _run = true;
        public void SpyWork(object code)
        {    
            while (_run)
            {
                if (CheckTime())
                {
                    _run = PostMessage(code.ToString());
                }
                else
                {
                    waitEvent.WaitOne(60000, false);
                    Console.WriteLine("sleep 1 min");
                    SendMessage(childHwnd5, WM_CLICK, 0, "0");
                }
            }    
        }
        private bool CheckTime()
        {
            if (DateTime.Now.Hour > 10) return false;
            //return true;
            if (DateTime.Now.Hour > 9 && DateTime.Now.Minute > 29 && DateTime.Now.Minute < 60)
            {
                return true;
            }
            //else if (DateTime.Now.Hour > 13 && DateTime.Now.Hour < 15)
            //{
            //    return false;
            //}
            else
            {
                return false;
            }    
        }
        AutoResetEvent waitEvent = new AutoResetEvent(false);
        public bool PostMessage(string listName)
        {
            string serviceAddress = "http://hq.sinajs.cn/list=" + listName;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceAddress);
            request.Method = "POST";
            request.ContentType = "application/json";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "GB2312"; //默认编码  
                }
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                t1 = DateTime.Now;
                Console.WriteLine(t1.ToString());
                
                string[] stockdatas = retString.Split(';');
                if (stockdatas.Length > 0)
                {
                    for (int i = 0; i < stockdatas.Length - 1; i++)
                    {

                        string[] tempstrs = stockdatas[i].Split('\"');
                        string[] arr = tempstrs[1].Split(',');
                        decimal yesPrice = Convert.ToDecimal(arr[2]);
                        decimal CurPrice = Convert.ToDecimal(arr[3]);
                        decimal Amount = Convert.ToDecimal(arr[9]);
                        decimal s1Counts = Convert.ToDecimal(arr[20]);
                        decimal s1Price = Convert.ToDecimal(arr[21]);
                        decimal s2Counts = Convert.ToDecimal(arr[22]);
                        decimal s2Price = Convert.ToDecimal(arr[23]);
                        decimal sAmout = s1Counts * s1Price + s2Counts * s2Price;

                        decimal b1Counts = Convert.ToDecimal(arr[10]);
                        decimal b1Price = Convert.ToDecimal(arr[11]);
                        decimal b2Counts = Convert.ToDecimal(arr[12]);
                        decimal b2Price = Convert.ToDecimal(arr[13]);
                        decimal bAmout = b1Counts * b1Price + b2Counts * b2Price;

                        if (CurPrice.Equals(0))
                        {
                            break;
                        }
                        decimal topPrice = Math.Round(yesPrice * (decimal)1.1, 2, MidpointRounding.AwayFromZero);
                        decimal secPrice = topPrice - (decimal)0.01;
                        if (Convert.ToDecimal(arr[3]).Equals(topPrice))
                        {
                            JudgeOfCodes[codes[i]] += 20;
                        }
                        if (CurPrice >= secPrice)
                        {
                            JudgeOfCodes[codes[i]] += 10;
                            if (bAmout > sAmout)
                            {
                                JudgeOfCodes[codes[i]] += 50;
                            }
                        }

                        if (JudgeOfCodes[codes[i]] > 60)
                        {
                            Buy(codes[i], topPrice);
                            DateTime t2 = DateTime.Now;
                            TimeSpan time = t2 - t1;
                            Console.WriteLine(time.ToString());
                            return false;
                        }
                    }
                }
                using (FileStream fs = new FileStream(@"F:\test.txt", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.BaseStream.Seek(0, SeekOrigin.End);
                        sw.WriteLine("{0}\n", retString, DateTime.Now);
                        sw.Flush();
                    }
                }
            }
            catch (WebException ex)
            {
                Thread.Sleep(1000);
            }

            Thread.Sleep(200);
            //waitEvent.WaitOne(600, false);
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int temp = Convert.ToInt32(textBox1.Text, 16);//16进制
            childHwnd1 = (IntPtr)temp;

            int temp2 = Convert.ToInt32(textBox2.Text, 16);
            childHwnd2 = (IntPtr)temp2;

            int temp3 = Convert.ToInt32(textBox3.Text, 16);
            childHwnd3 = (IntPtr)temp3;

            int temp4 = Convert.ToInt32(textBox4.Text, 16);
            childHwnd4 = (IntPtr)temp4;

            int temp5 = Convert.ToInt32(textBox5.Text, 16);
            childHwnd5 = (IntPtr)temp5;

            codes = textBox5.Text.Split(',').ToList();
            foreach (string code in codes)
            {
                if(!JudgeOfCodes.ContainsKey(code))
                    JudgeOfCodes.Add(code, 0);
            }
            CatchTenPercent(codes);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //string teststr = "v_sz000858=\"51~五 粮 液~000858~73.43~73.38~73.18~250617~129040~121410~73.42~144~73.41~940~73.40~922~73.39~50~73.38~77~73.43~113~73.44~107~73.45~124~73.46~27~73.47~29~15:00:04/73.43/1889/B/13870927/9627|14:57:00/73.40/21/S/154158/9539|14:56:57/73.41/5/M/36705/9537|14:56:54/73.40/16/S/117452/9535|14:56:51/73.41/20/B/148783/9533|14:56:49/73.41/33/B/242238/9532~20180720150139~0.05~0.07~73.83~71.50~73.43/250617/1822785336~250617~182279~0.66~25.79~~73.83~71.50~3.18~2787.23~2850.26~4.78~80.72~66.04~1.28~1733~72.73~14.33~29.46\";";
            //string[] strs = teststr.Split('\"');
            //string[] datas = strs[1].Split('~');
            //string[] recent6Volumes = datas[29].Split('|');

            //string[] recent1Volum = recent6Volumes[0].Split('/');
            //decimal recentCounts = Convert.ToDecimal(recent1Volum[2]);

            //string code = datas[2];
            //decimal curPrice = Convert.ToDecimal(datas[3]);
            //decimal yesPrice = Convert.ToDecimal(datas[4]);
            //decimal startPrice = Convert.ToDecimal(datas[5]);
            //decimal volume = Convert.ToDecimal(datas[6]);//成交量(手)
            
            //decimal totalAmount = Convert.ToDecimal(datas[37]);//万
            ////decimal s1Price = Convert.ToDecimal(datas[19]);
            //decimal s1Counts = Convert.ToDecimal(datas[20]);//手
            ////decimal s2Price = Convert.ToDecimal(datas[21]);
            //decimal s2Counts = Convert.ToDecimal(datas[22]);
            //decimal solder = s1Counts + s2Counts;

            ////decimal b1Price = Convert.ToDecimal(datas[9]);
            //decimal b1Counts = Convert.ToDecimal(datas[10]);
            ////decimal b2Price = Convert.ToDecimal(datas[11]);
            //decimal b2Counts = Convert.ToDecimal(datas[12]);
            //decimal buyer = recentCounts + b1Counts + b2Counts;

            //decimal topPrice = Convert.ToDecimal(datas[47]);
            //decimal lowestPrice = Convert.ToDecimal(datas[48]);
            //decimal secPrice = topPrice - (decimal)0.01;            

            //if (curPrice.Equals(topPrice))
            //{
            //    JudgeOfCodes[code] += 20;
            //}
            //if (curPrice >= secPrice)
            //{
            //    JudgeOfCodes[code] += 10;
            //    if (buyer > solder)
            //    {
            //        JudgeOfCodes[code] += 50;
            //    }
            //}

            //if (JudgeOfCodes[code] > 60)
            //{
            //    Buy(code, topPrice);
            //    DateTime t2 = DateTime.Now;
            //    TimeSpan time = t2 - t1;
            //    Console.WriteLine(time.ToString());
                
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _run = false;
        }
    }
}
