using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BingLibrary.hjb.file;
using System.Data;

namespace TUCHX1621UI.Model
{
    public enum TestStatus
    {
        Testing, PreTest, Tested, Err
    }
    public enum TestResult
    {
        Unknow, Pass, Ng, TimeOut
    }
    public class Tester
    {
        public static bool IsInSampleMode { set; get; } = false;
        public static bool IsInGRRMode { set; get; } = false;
        public int PassCount { set; get; }
        public int FailCount { set; get; }
        public int TestCount { set; get; }
        public double Yield { set; get; }

        public int PassCount_Nomal { set; get; }
        public int FailCount_Nomal { set; get; }
        public int TestCount_Nomal { set; get; }
        public double Yield_Nomal { set; get; }

        public double TestSpan { set; get; }
        public double TestIdle { set; get; }
        public double TestCycle { set; get; }
        public TestResult TestResult { set; get; }
        public TestStatus TestStatus { set; get; }
        public int Index { set; get; }
        Stopwatch idlesw;
        bool idleswflag;
        private string iniTesterResutPath = System.Environment.CurrentDirectory + "\\TesterResut.ini";
        public Tester(int index)
        {
            Index = index;
            idleswflag = false;
            idlesw = new Stopwatch();
            TestSpan = double.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestSpan", "0"));
            TestIdle = double.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestIdle", "0"));
            TestCycle = double.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCycle", "0"));
            PassCount = int.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "PassCount", "0"));
            PassCount_Nomal = int.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "PassCount_Nomal", "0"));
            FailCount = int.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "FailCount", "0"));
            FailCount_Nomal = int.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "FailCount_Nomal", "0"));
            TestCount = int.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCount", "0"));
            TestCount_Nomal = int.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCount_Nomal", "0"));
            Yield = double.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "Yield", "0"));
            Yield_Nomal = double.Parse(Inifile.INIGetStringValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "Yield_Nomal", "0"));
            Task.Run(() => { RunLoop(); });
        }
        public delegate void StartProcessedDelegate(int i);
        public async void Start(StartProcessedDelegate callback)
        {
            Stopwatch sw = new Stopwatch();
            Func<Task> startTask = () =>
            {
                return Task.Run(() =>
                {
                    idleswflag = false;
                    idlesw.Stop();
                    Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestIdle", TestIdle.ToString());
                    TestCycle = TestSpan + TestIdle;
                    Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCycle", TestCycle.ToString());
                    sw.Start();
                    TestStatus = TestStatus.Testing;
                    TestResult = TestResult.Unknow;
                    while (TestStatus == TestStatus.Testing)
                    {
                        TestSpan = Math.Round(sw.Elapsed.TotalSeconds, 2);
                        System.Threading.Thread.Sleep(100);
                    }
                });
            };
            await startTask();
            callback(Index);
            if (!IsInSampleMode && !IsInGRRMode)
            {
                UpdateNormal();
            }

            idleswflag = true;
            idlesw.Restart();
        }
        private void RunLoop()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10);
                if (idleswflag)
                {
                    TestIdle = Math.Round(idlesw.Elapsed.TotalSeconds, 2);
                    if (TestIdle > 99.99)
                    {
                        idleswflag = false;
                        idlesw.Stop();
                    }
                }
            }
        }
        public void UpdateNormal()
        {
            TestCount_Nomal++;
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestSpan", TestSpan.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCount_Nomal", TestCount_Nomal.ToString());
        }
        public void UpdateNormalWithTestTimes(String tr)
        {
            if (tr == "PASS")
            {
                PassCount_Nomal++;
            }
            else
            {
                FailCount_Nomal++;
            }
            if (PassCount_Nomal + FailCount_Nomal != 0)
            {
                Yield_Nomal = Math.Round((double)PassCount_Nomal / (PassCount_Nomal + FailCount_Nomal) * 100, 2);
            }
            else
            {
                Yield_Nomal = 0;
            }
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "PassCount_Nomal", PassCount_Nomal.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "FailCount_Nomal", FailCount_Nomal.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "Yield_Nomal", Yield_Nomal.ToString());
        }
        public void Update(TestResult tr)
        {
            TestCount++;
            if (tr == TestResult.Pass)
            {
                PassCount++;
            }
            else
            {
                FailCount++;
            }
            if (PassCount + FailCount != 0)
            {
                Yield = Math.Round((double)PassCount / (PassCount + FailCount) * 100, 2);
            }
            else
            {
                Yield = 0;
            }
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "PassCount", PassCount.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "FailCount", FailCount.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCount", TestCount.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "Yield", Yield.ToString());
        }
        public void Clean()
        {
            TestCount = 0;
            PassCount = 0;
            FailCount = 0;
            Yield = 0;
            TestCount_Nomal = 0;
            PassCount_Nomal = 0;
            FailCount_Nomal = 0;
            Yield_Nomal = 0;
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "PassCount_Nomal", PassCount_Nomal.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "FailCount_Nomal", FailCount_Nomal.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCount_Nomal", TestCount_Nomal.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "Yield_Nomal", Yield_Nomal.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "PassCount", PassCount.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "FailCount", FailCount.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "TestCount", TestCount.ToString());
            Inifile.INIWriteValue(iniTesterResutPath, "Tester" + (Index - 1).ToString(), "Yield", Yield.ToString());
        }
    }
    public class UploadSoftwareStatus
    {
        public delegate void PrintEventHandler(string ModelMessageStr);
        public event PrintEventHandler ModelPrint;
        public delegate void RecordPrintEventHandler(int index, string bar, string rst, string cyc, bool isRecord);
        public event RecordPrintEventHandler RecordPrint;
        public bool status { set; get; } = true;
        public bool start { set; get; } = false;
        public int index { set; get; } = 0;
        public string result;
        public string testerCycle;
        Stopwatch sw;
        private string iniFilepath = @"d:\test.ini";
        int timed;
        public UploadSoftwareStatus(int _index)
        {
            index = _index;
            sw = new Stopwatch();
            Task.Run(()=> { run(); });
        }
        private void run()
        {
            status = true;
            timed = 1000;
            int ngnum = 0;
            while (true)
            {

                if (start)
                {
                    string newbar = Inifile.INIGetStringValue(iniFilepath, "A", "bar" + index.ToString(), "0");
                    try
                    {
                        Oracle oraDB = new Oracle("qddb04.eavarytech.com", "mesdb04", "ictdata", "ictdata*168");
                        if (oraDB.isConnect())
                        {
                            string stm = String.Format("Select * from fluke_data WHERE BARCODE = '{0}'AND TRESULT = '{1}' ORDER BY ITSDATE DESC, ITSTIME DESC", newbar, result);
                            DataSet s = oraDB.executeQuery(stm);
                            DataTable dt = s.Tables[0];
                            if (dt.Rows.Count > 0)
                            {
                                string datestr = (string)dt.Rows[0]["ITSDATE"];
                                string timestr = (string)dt.Rows[0]["ITSTIME"];
                                if (datestr.Length == 8 && (timestr.Length == 5 || timestr.Length == 6))
                                {
                                    if (timestr.Length == 5)
                                    {
                                        timestr = "0" + timestr;
                                    }
                                    string datetimestr = string.Empty;
                                    datetimestr = string.Format("{0}/{1}/{2} {3}:{4}:{5}", datestr.Substring(0, 4), datestr.Substring(4, 2), datestr.Substring(6, 2), timestr.Substring(0, 2), timestr.Substring(2, 2), timestr.Substring(4, 2));
                                    DateTime updatetime = Convert.ToDateTime(datetimestr);
                                    TimeSpan sp = System.DateTime.Now - updatetime;
                                    if (sp.TotalSeconds < 30)
                                    {
                                        ngnum = 0;
                                        status = true;
                                        timed = 2000;
                                        start = false;
                                        ModelPrint("测试机" + index.ToString() + ": " + newbar + " 数据上传成功 " + updatetime.ToString());
                                        stm = String.Format("Select * from fluke_data WHERE BARCODE = '{0}' ORDER BY ITSDATE DESC, ITSTIME DESC", newbar, result);
                                        s = oraDB.executeQuery(stm);
                                        dt = s.Tables[0];
                                        if (dt.Rows.Count > 3)
                                        {
                                            RecordPrint(index, newbar, result, testerCycle, false);
                                        }
                                        else
                                        {
                                            RecordPrint(index, newbar, result, testerCycle, true);
                                        }

                                    }
                                    else
                                    {
                                        timed = 1000;
                                        ModelPrint("测试机" + index.ToString() + ": " + newbar + " 数据上传逾时 " + updatetime.ToString());
                                    }
                                }
                                else
                                {

                                    timed = 1000;
                                    ModelPrint("测试机" + index.ToString() + ": " + newbar + " 时间格式错误 " + datestr + " " + timestr);
                                }

                            }
                            else
                            {

                                timed = 1000;
                                ModelPrint("测试机" + index.ToString() + ": " + newbar + " 未查询到数据");
                            }
                        }
                        else
                        {

                            timed = 1000;
                            ModelPrint("测试机" + index.ToString() + ": " + newbar + " 数据库未连接");
                        }
                        oraDB.disconnect();
                    }
                    catch (Exception ex)
                    {
                        ModelPrint("测试机" + index.ToString() + ": " + newbar + " 查询数据库出错" + ex.Message);

                        timed = 1000;
                    }

                    if (sw.Elapsed.TotalSeconds > 10 && start)
                    {                        
                        if (++ngnum > 1)
                        {
                            ngnum = 0;
                            status = false;
                            ModelPrint("测试机" + index.ToString() + ": " + newbar + " 检测超时，2次，退出");
                        }
                        else
                        {
                            status = true;
                            ModelPrint("测试机" + index.ToString() + ": " + newbar + " 检测超时，1次，退出");
                        }

                        start = false;

                        timed = 2000;

                        RecordPrint(index, "****************************", result, testerCycle, true);
                    }
                }
                else
                {
                    timed = 2000;
                }
                System.Threading.Thread.Sleep(timed);
            }
        }
        public void StartCommand()
        {
            start = true;
            sw.Restart();
        }
        public void StopCommand()
        {
            start = false;
            status = true;
            sw.Stop();
            ModelPrint("测试机" + index.ToString() + ": 测试时间过短，排除");
            RecordPrint(index, "****************************", result, testerCycle, true);
        }
        public void ResetCommand()
        {
            start = false;
            status = true;
            sw.Stop();
        }

    }
}
