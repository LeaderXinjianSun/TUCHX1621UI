using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BingLibrary.hjb.net;
using BingLibrary.hjb.file;
using System.IO;
using BingLibrary.hjb;

namespace TUCHX1621UI.Model
{
    public class EpsonRC90
    {
        #region 变量
        public bool[] Rc90In = new bool[200];
        public bool[] Rc90Out = new bool[200];
        public TcpIpClient IOReceiveNet = new TcpIpClient();
        public TcpIpClient TestSentNet = new TcpIpClient();
        public TcpIpClient TestReceiveNet = new TcpIpClient();
        string Ip = "192.168.1.2";
        public bool IOReceiveStatus = false, TestSendStatus = false, TestReceiveStatus = false;
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        public string[] BordBarcode = new string[2] { "Null", "Null" };
        public ProducInfo[][] BarInfo = new ProducInfo[2][] { new ProducInfo[15], new ProducInfo[15] };
        public Tester tester = new Tester();
        #endregion
        #region 事件
        public delegate void PrintEventHandler(string ModelMessageStr);
        public event PrintEventHandler ModelPrint;
        #endregion
        #region 构造函数
        public EpsonRC90()
        {
            Ip = Inifile.INIGetStringValue(iniParameterPath, "EpsonRC90", "Ip", "192.168.1.2");
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    BarInfo[i][j] = new ProducInfo();
                    BarInfo[i][j].Barcode = "FAIL";
                    BarInfo[i][j].BordBarcode = "Null";
                    BarInfo[i][j].Status = 0;
                    BarInfo[i][j].TDate = DateTime.Now.ToString("yyyyMMdd");
                    BarInfo[i][j].TTime = DateTime.Now.ToString("HHmmss");
                }
            }
            for (int i = 0; i < 4; i++)
            {
                //Inifile.INIWriteValue(iniParameterPath, "Summary", "Tester1TestCount" + (i + 1).ToString(), "0");
                try
                {
                    tester.TestCount[i] = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Summary", "TesterTestCount" + (i + 1).ToString(), "0"));
                    tester.PassCount[i] = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Summary", "TesterPassCount" + (i + 1).ToString(), "0"));
                    if (tester.TestCount[i] > 0)
                    {
                        tester.Yield[i] = (double)tester.PassCount[i] / (double)tester.TestCount[i] * 100;
                    }
                    else
                    {
                        tester.Yield[i] = 0;
                    }

                    tester.OriginalTestCount[i] = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Summary", "TesterOriginalTestCount" + (i + 1).ToString(), "0"));
                    tester.OriginalPassCount[i] = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Summary", "TesterOriginalPassCount" + (i + 1).ToString(), "0"));
                    if (tester.OriginalTestCount[i] > 0)
                    {
                        tester.OriginalYield[i] = (double)tester.OriginalPassCount[i] / (double)tester.OriginalTestCount[i] * 100;
                    }
                    else
                    {
                        tester.OriginalYield[i] = 0;
                    }
                }
                catch
                { }

            }
            Run();
        }
        #endregion
        #region 机械手通讯      
        public async void checkIOReceiveNet()
        {
            while (true)
            {
                await Task.Delay(400);
                if (!IOReceiveNet.tcpConnected)
                {
                    await Task.Delay(1000);
                    if (!IOReceiveNet.tcpConnected)
                    {
                        bool r1 = await IOReceiveNet.Connect(Ip, 2007);
                        if (r1)
                        {
                            IOReceiveStatus = true;
                            ModelPrint("机械手IOReceiveNet连接");

                        }
                        else
                            IOReceiveStatus = false;
                    }
                }
                else
                { await Task.Delay(15000); }
            }
        }
        public async void checkTestSentNet()
        {
            while (true)
            {
                await Task.Delay(400);
                if (!TestSentNet.tcpConnected)
                {
                    await Task.Delay(1000);
                    if (!TestSentNet.tcpConnected)
                    {
                        bool r1 = await TestSentNet.Connect(Ip, 2000);
                        if (r1)
                        {
                            TestSendStatus = true;
                            ModelPrint("机械手TestSentNet连接");
                        }
                        else
                            TestSendStatus = false;
                    }
                }
                else
                {
                    await Task.Delay(15000);
                    TestSentNet.IsOnline();
                    if (!TestSentNet.tcpConnected)
                        ModelPrint("机械手TestSentNet断开");
                }
            }
        }
        public async void checkTestReceiveNet()
        {
            while (true)
            {
                await Task.Delay(400);
                if (!TestReceiveNet.tcpConnected)
                {
                    await Task.Delay(1000);
                    if (!TestReceiveNet.tcpConnected)
                    {
                        bool r1 = await TestReceiveNet.Connect(Ip, 2001);
                        if (r1)
                        {
                            TestReceiveStatus = true;
                            ModelPrint("机械手TestReceiveNet连接");
                        }
                        else
                            TestReceiveStatus = false;
                    }
                }
                else
                { await Task.Delay(15000); }
            }
        }
        private async void IORevAnalysis()
        {
            while (true)
            {
                //await Task.Delay(100);
                if (IOReceiveStatus == true)
                {
                    string s = await IOReceiveNet.ReceiveAsync();

                    string[] ss = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        s = ss[0];

                    }
                    catch
                    {
                        s = "error";
                    }

                    if (s == "error")
                    {
                        IOReceiveNet.tcpConnected = false;
                        IOReceiveStatus = false;
                        ModelPrint("机械手IOReceiveNet断开");
                    }
                    else
                    {
                        string[] strs = s.Split(',');
                        if (strs[0] == "IOCMD" && strs[1].Length == 200)
                        {
                            for (int i = 0; i < 200; i++)
                            {
                                Rc90Out[i] = strs[1][i] == '1' ? true : false;
                            }
                            string RsedStr = "";
                            for (int i = 0; i < 200; i++)
                            {
                                RsedStr += Rc90In[i] ? "1" : "0";
                            }
                            await IOReceiveNet.SendAsync(RsedStr);
                            //ModelPrint("IOSend " + RsedStr);
                            //await Task.Delay(1);
                        }
                        //ModelPrint("IORev: " + s);
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
        private async void TestRevAnalysis()
        {
            while (true)
            {
                if (TestReceiveStatus == true)
                {
                    string s = await TestReceiveNet.ReceiveAsync();

                    string[] ss = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        s = ss[0];
                    }
                    catch
                    {
                        s = "error";
                    }

                    if (s == "error")
                    {
                        TestReceiveNet.tcpConnected = false;
                        TestReceiveStatus = false;
                        ModelPrint("机械手TestReceiveNet断开");
                    }
                    else
                    {
                        ModelPrint("TestRev: " + s);
                        try
                        {
                            string[] strs = s.Split(';');
                            switch (strs[0])
                            {
                                case "ReleaseA"://区间号(0-7);产品1状态(3-5);产品2状态(3-5)
                                    if (strs.Length == 4)
                                    {
                                        SaveRelease(0, strs);
                                    }
                                    break;
                                case "ReleaseB":
                                    if (strs.Length == 4)
                                    {
                                        SaveRelease(1, strs);
                                    }
                                    break;
                                case "OriginalResult":
                                    if (strs.Length == 3)
                                    {
                                        UpdateOriginalResult(strs);
                                    }
                                    break;
                                case "ABBResult":
                                    if (strs.Length == 3)
                                    {
                                        UpdateResult(strs);
                                    }
                                    break;
                                case "AABResult":
                                    if (strs.Length == 3)
                                    {
                                        UpdateResult(strs);
                                    }
                                    break;
                                default:
                                    ModelPrint("无效指令： " + s);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            ModelPrint(ex.Message);
                        }
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
        #endregion
        #region 功能函数
        void Run()
        {
            checkIOReceiveNet();
            checkTestSentNet();
            checkTestReceiveNet();
            IORevAnalysis();
            TestRevAnalysis();
        }
        public void ResetBord(int index)
        {
            for (int i = 0; i < 15; i++)
            {
                BarInfo[index][i].Barcode = "FAIL";
                BarInfo[index][i].BordBarcode = BordBarcode[index];
                BarInfo[index][i].Status = 0;
                BarInfo[index][i].TDate = DateTime.Now.ToString("yyyyMMdd");
                BarInfo[index][i].TTime = DateTime.Now.ToString("HHmmss");
                string machinestr = Inifile.INIGetStringValue(iniParameterPath, "System", "MachineID", "X1621_1");
                Mysql mysql = new Mysql();
                if (mysql.Connect())
                {
                    string stm = "INSERT INTO BARBIND (MACHINE,SCBARCODE,SCBODBAR,SDATE,STIME,PCSSER,RESULT) VALUES ('" + machinestr + "','" + BarInfo[index][i].Barcode + "','"
                                    + BordBarcode[index] + "','" + BarInfo[index][i].TDate + "','" + BarInfo[index][i].TTime + "','" + (i + 1).ToString() + "','" + BarInfo[index][i].Status.ToString() + "')";
                    mysql.executeQuery(stm);
                }
                mysql.DisConnect();
            }
        }
        async void SaveRelease(int _index,string[] rststr)
        {
            int index = int.Parse(rststr[1]);
            await Task.Run(() =>
            {
                Mysql mysql = new Mysql();
                if (mysql.Connect())
                {
                    string stm = "";
                    for (int i = 0; i < 2 && index * 2 + i < 15; i++)
                    {
                        stm += "UPDATE BARBIND SET RESULT = '" + rststr[2 + i] + "' WHERE SCBARCODE = '" + BarInfo[_index][index * 2 + i].Barcode + "' AND SCBODBAR = '" + BarInfo[_index][index * 2 + i].BordBarcode
                        + "' AND SDATE = '" + BarInfo[_index][index * 2 + i].TDate + "' AND STIME = '" + BarInfo[_index][index * 2 + i].TTime + "' AND PCSSER = '" + (index * 2 + i + 1).ToString() + "'";
                    }
                    mysql.executeQuery(stm); 
                }
                mysql.DisConnect();
            });
            if (!Directory.Exists("D:\\生产记录\\" + DateTime.Now.ToString("yyyyMMdd")))
            {
                Directory.CreateDirectory("D:\\生产记录\\" + DateTime.Now.ToString("yyyyMMdd"));
            }
            string path = "D:\\生产记录\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + DateTime.Now.ToString("yyyyMMdd") + "生产记录.csv";
            for (int i = 0; i < 2 && index * 2 + i < 15; i++)
            {
                Csvfile.savetocsv(path, new string[] { DateTime.Now.ToString(), BarInfo[_index][index * 2 + i].Barcode, rststr[2 + i] == "4" ? "P" : "F" });
            }
        }
        void UpdateOriginalResult(string[] rststr)
        {
            int index = int.Parse(rststr[1]);

            switch (rststr[2])
            {
                case "3":
                    tester.OriginalResult[index] = "F";
                    break;
                case "4":
                    tester.OriginalResult[index] = "P";
                    break;
                default:
                    tester.OriginalResult[index] = "N";
                    break;
            }
            if (rststr[2] == "4" || rststr[2] == "3")
            {
                tester.OriginalTestCount[index]++;
            }
            if (rststr[2] == "4")
            {
                tester.OriginalPassCount[index]++;
            }
            if (tester.OriginalTestCount[index] > 0)
            {
                tester.OriginalYield[index] = (double)tester.OriginalPassCount[index] / (double)tester.OriginalTestCount[index] * 100;
            }
            else
            {
                tester.OriginalYield[index] = 0;
            }


            Inifile.INIWriteValue(iniParameterPath, "Summary", "TesterOriginalTestCount" + (index + 1).ToString(), tester.OriginalTestCount[index].ToString());
            Inifile.INIWriteValue(iniParameterPath, "Summary", "TesterOriginalPassCount" + (index + 1).ToString(), tester.OriginalPassCount[index].ToString());



        }
        void UpdateResult(string[] rststr)
        {
            int index = int.Parse(rststr[1]);

            switch (rststr[2])
            {
                case "3":
                    tester.Result[index] = "F";
                    break;
                case "4":
                    tester.Result[index] = "P";
                    break;
                default:
                    tester.Result[index] = "N";
                    break;
            }
            if (rststr[2] == "4" || rststr[2] == "3")
            {
                tester.TestCount[index]++;
            }
            if (rststr[2] == "4")
            {
                tester.PassCount[index]++;
            }
            if (tester.TestCount[index] > 0)
            {
                tester.Yield[index] = (double)tester.PassCount[index] / (double)tester.TestCount[index] * 100;
            }
            else
            {
                tester.Yield[index] = 0;
            }
            Inifile.INIWriteValue(iniParameterPath, "Summary", "TesterTestCount" + (index + 1).ToString(), tester.TestCount[index].ToString());
            Inifile.INIWriteValue(iniParameterPath, "Summary", "TesterPassCount" + (index + 1).ToString(), tester.PassCount[index].ToString());
        }
        #endregion
    }
    public class ProducInfo
    {
        //条码 板条码 产品状态 日期 时间
        public string Barcode { set; get; }
        public string BordBarcode { set; get; }
        public int Status { set; get; }
        public string TDate { set; get; }
        public string TTime { set; get; }
    }
    public class Tester
    {
        public string[] OriginalResult { set; get; }
        public int[] OriginalPassCount { set; get; }
        public int[] OriginalTestCount { set; get; }
        public Double[] OriginalYield { set; get; }
        public string[] Result { set; get; }
        public int[] PassCount { set; get; }
        public int[] TestCount { set; get; }
        public Double[] Yield { set; get; }
        public Tester()
        {
            OriginalResult = new string[4] { "N", "N", "N", "N" };
            OriginalPassCount = new int[4] { 0, 0, 0, 0 };
            OriginalTestCount = new int[4] { 0, 0, 0, 0 };
            OriginalYield = new double[4] { 0, 0, 0, 0 };
            Result = new string[4] { "N", "N", "N", "N" };
            PassCount = new int[4] { 0, 0, 0, 0 };
            TestCount = new int[4] { 0, 0, 0, 0 };
            Yield = new double[4] { 0, 0, 0, 0 };
        }
    }

}
