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
        public Tester[] YanmadeTester = new Tester[4];
        public UploadSoftwareStatus[] uploadSoftwareStatus = new UploadSoftwareStatus[4];
        #endregion
        #region 事件
        public delegate void PrintEventHandler(string ModelMessageStr);
        public event PrintEventHandler ModelPrint;
        #endregion
        #region 构造函数
        public EpsonRC90()
        {
            for (int i = 0; i < 4; i++)
            {
                YanmadeTester[i] = new Tester(i + 1);
                uploadSoftwareStatus[i] = new UploadSoftwareStatus(i + 1);
                uploadSoftwareStatus[i].ModelPrint += uploadprint;
                uploadSoftwareStatus[i].RecordPrint += RecordPrintOperate;
            }
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
                                    SaveRelease(1, strs);
                                    break;
                                case "TestResultCount":
                                    TestResult tr = strs[1] == "OK" ? TestResult.Pass : TestResult.Ng;
                                    YanmadeTester[int.Parse(strs[2]) - 1].Update(tr);
                                    break;
                                case "Start":
                                    YanmadeTester[int.Parse(strs[1]) - 1].Start(TestFinishOperate);
                                    break;
                                case "Finish":
                                    YanmadeTester[int.Parse(strs[1]) - 1].TestResult = strs[2] == "1" ? TestResult.Pass : TestResult.Ng;
                                    YanmadeTester[int.Parse(strs[1]) - 1].TestStatus = TestStatus.Tested;
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
        //放料到载盘；条码从夹爪转移到载盘
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
        }
        private void TestFinishOperate(int index)
        {
            uploadSoftwareStatus[index - 1].testerCycle = YanmadeTester[index - 1].TestSpan.ToString();
            uploadSoftwareStatus[index - 1].result = YanmadeTester[index - 1].TestResult == TestResult.Pass ? "PASS" : "FAIL";
            if (YanmadeTester[index - 1].TestSpan > 11)
            {
                uploadSoftwareStatus[index - 1].StartCommand();
            }
            else
            {
                uploadSoftwareStatus[index - 1].StopCommand();
            }
        }
        private void uploadprint(string str)
        {
            ModelPrint(str);
        }
        private void RecordPrintOperate(int index, string bar, string rst, string cyc, bool isRecord)
        {
            SaveCSVfileRecord(DateTime.Now.ToString(), bar, rst, cyc + " s", index.ToString());
            if (isRecord && !Tester.IsInSampleMode && !Tester.IsInGRRMode)
            {
                if (YanmadeTester[index - 1].TestSpan > 5)
                {
                    YanmadeTester[index - 1].UpdateNormalWithTestTimes(rst);
                }
                else
                {
                        ModelPrint(bar + " 测试时间小于5秒，不纳入良率统计");
                }
            }
            else
            {
                if (!isRecord && !Tester.IsInSampleMode && !Tester.IsInGRRMode)
                {
                    ModelPrint(bar + " 测试次数大于3次，不纳入良率统计");
                }
            }
            //try
            //{
            //    GlobalVar.Worksheet.Cells[(index - 1) * 2 + 3, 6].Value = Convert.ToInt32(GlobalVar.Worksheet.Cells[(index - 1) * 2 + 3, 6].Value) + 1;
            //    GlobalVar.Worksheet.Cells[(index - 1) * 2 + 1 + 3, 6].Value = Convert.ToInt32(GlobalVar.Worksheet.Cells[(index - 1) * 2 + 1 + 3, 6].Value) + 1;
            //}
            //catch (Exception ex)
            //{
            //    MsgText = AddMessage(ex.Message);
            //}
        }
        private void SaveCSVfileRecord(string TestTime, string Barcode, string TestResult, string TestCycleTime, string Index)
        {
            string filepath = "D:\\生产记录\\生产记录" + GlobalVars.GetBanci() + ".csv";
            if (!Directory.Exists("D:\\生产记录"))
            {
                Directory.CreateDirectory("D:\\生产记录");
            }
            try
            {
                if (!File.Exists(filepath))
                {
                    string[] heads = { "Time", "Barcode", "Result", "Cycle", "Index" };
                    Csvfile.savetocsv(filepath, heads);
                }
                string[] conte = { TestTime, Barcode, TestResult, TestCycleTime, Index };
                Csvfile.savetocsv(filepath, conte);
            }
            catch (Exception ex)
            {
                ModelPrint(ex.Message);
            }
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
}
