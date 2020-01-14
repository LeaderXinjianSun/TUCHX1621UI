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
using TUCHX1621UI.Model;
using System.Data;
using BingLibrary.hjb.file;
using System.IO;
using OfficeOpenXml;
using BingLibrary.hjb;

namespace TUCHX1621UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 属性
        int Station{ set; get; }
        string LineID1 { set; get; }
        string LineID2 { set; get; }
        #endregion
        #region 变量
        private EpsonRC90 epsonRC90;
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Scan scan1,scan2;
        string LastBanci = "";
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName("TUCHX1621UI");//获取指定的进程名   
            if (myProcesses.Length > 1) //如果可以获取到知道的进程名则说明已经启动
            {
                System.Windows.MessageBox.Show("不允许重复打开软件");
                System.Windows.Application.Current.Shutdown();
            }
            else
            {

            }
        }
        #region 功能函数
        void AddMessage(string str)
        {
            string[] s = MsgTextBox.Text.Split('\n');
            if (s.Length > 1000)
            {
                MsgTextBox.Text = "";
            }
            if (MsgTextBox.Text != "")
            {
                MsgTextBox.Text += "\r\n";
            }
            MsgTextBox.Text += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + str;
        }
        #endregion
        #region 工作函数
        void StationEnterRun()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10000);
                try
                {
                    string StrMySQL = "Server=192.168.100.229;Database=leaderb;Uid=sunxinjian;Pwd=*963/852;pooling=false;CharSet=utf8;port=3306";
                    Mysql mysql = new Mysql();
                    if (mysql.Connect(StrMySQL))
                    {
                        //【轨道A】
                        string stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                        DataSet ds = mysql.Select(stm);
                        DataTable dt = ds.Tables["table0"];
                        if (dt.Rows.Count > 0)
                        {
                            int bordcount = (int)dt.Rows[0]["Station1"] + (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station7"] + (int)dt.Rows[0]["Station8"] + (int)dt.Rows[0]["Station9"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                            if (!GlobalVars.Fx5u_mid.ReadM("M2596") && bordcount < 21 && (int)dt.Rows[0]["Station7"] < 1)//不在进板；有进板需求；轨道1空
                            {
                                GlobalVars.Fx5u_mid.SetM("M2796", false);
                                GlobalVars.Fx5u_mid.SetM("M2596", true);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线A内板数:" + bordcount.ToString() + " < 21,申请进板");
                                }));
                            }
                            if (GlobalVars.Fx5u_mid.ReadM("M2796"))
                            {
                                GlobalVars.Fx5u_mid.SetM("M2796", false);
                                GlobalVars.Fx5u_mid.SetM("M2596", false);
                                stm = "UPDATE BODLINE SET Station9 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                mysql.executeQuery(stm);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线A进入1块板");
                                }));
                            }
                        }

                        //【轨道B】
                        stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                        ds = mysql.Select(stm);
                        dt = ds.Tables["table0"];
                        if (dt.Rows.Count > 0)
                        {
                            int bordcount = (int)dt.Rows[0]["Station1"] + (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station7"] + (int)dt.Rows[0]["Station8"] + (int)dt.Rows[0]["Station9"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                            if (!GlobalVars.Fx5u_mid.ReadM("M2601") && bordcount < 21 && (int)dt.Rows[0]["Station7"] < 1)//不在进板；有进板需求；轨道1空
                            {
                                GlobalVars.Fx5u_mid.SetM("M2801", false);
                                GlobalVars.Fx5u_mid.SetM("M2601", true);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线B内板数:" + bordcount.ToString() + " < 21,申请进板");
                                }));
                            }
                            if (GlobalVars.Fx5u_mid.ReadM("M2801"))
                            {
                                GlobalVars.Fx5u_mid.SetM("M2801", false);
                                GlobalVars.Fx5u_mid.SetM("M2601", false);
                                stm = "UPDATE BODLINE SET Station9 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                mysql.executeQuery(stm);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线B进入1块板");
                                }));
                            }
                        }
                    }
                    mysql.DisConnect();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AddMessage(ex.Message);
                    }));
                }
                
            }
        }
        #endregion
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Station = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Station", "NUM", "-1"));
                LineID1 = Inifile.INIGetStringValue(iniParameterPath, "System", "LineID1", "Line1");
                LineID2 = Inifile.INIGetStringValue(iniParameterPath, "System", "LineID2", "Line2");
                window1.Title = "TUCHX1621UI:" + Station.ToString();
                MachineID.Text = Inifile.INIGetStringValue(iniParameterPath, "System", "MachineID", "X1621_1");
                LastBanci = Inifile.INIGetStringValue(iniParameterPath, "Summary", "LastBanci", "null");
                switch (Station)
                {
                    case 1:
                        statusBarItem1.Visibility = Visibility.Collapsed;
                        string plc_ip = Inifile.INIGetStringValue(iniParameterPath, "System", "PLC2IP", "192.168.10.2");
                        int plc_port = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "System", "PLC2PORT", "8000"));
                        GlobalVars.Fx5u_mid = new Fx5u(plc_ip, plc_port);
                        scan1 = new Scan();
                        string COM = Inifile.INIGetStringValue(iniParameterPath, "System", "ScanCOM1", "COM0");
                        scan1.ini(COM);
                        scan2 = new Scan();
                        COM = Inifile.INIGetStringValue(iniParameterPath, "System", "ScanCOM2", "COM1");
                        scan2.ini(COM);
                        Task.Run(() => { StationEnterRun(); });
                        AddMessage("机台站:" + Station.ToString() + ";轨道入口功能开启");
                        break;
                    default:
                        break;
                }

                epsonRC90 = new EpsonRC90();
                epsonRC90.ModelPrint += ModelPrintEventProcess;
                UpdateUI();
                Task.Run(() => { Run(); });
                Task.Run(()=> { IORun(); });
                AddMessage("软件加载完成");

              
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }            
        }

        private void HomePageSelect(object sender, RoutedEventArgs e)
        {
            HomePage.Visibility = Visibility.Visible;
            ParameterPage.Visibility = Visibility.Collapsed;
        }

        private void ParameterPageSelect(object sender, RoutedEventArgs e)
        {
            HomePage.Visibility = Visibility.Collapsed;
            ParameterPage.Visibility = Visibility.Visible;
        }

        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }

        private void FuncButtonClick(object sender, RoutedEventArgs e)
        {
            //Mysql mysql = new Mysql();
            //if (mysql.Connect("Server=192.168.100.229;Database=leaderb;Uid=sunxinjian;Pwd=*936/852;pooling=false;CharSet=utf8;port=3306"))
            //{
            //    string stm = "SELECT * FROM BODLINE WHERE LineID = 'Line1' ORDER BY SIDATE DESC";
            //    DataSet aa = mysql.Select(stm);
            //    DataTable dt = aa.Tables[0];
            //    AddMessage(((int)dt.Rows[0]["Station2"]).ToString());
            //}
            //mysql.DisConnect();
            //Oracle oraDB1 = new Oracle("192.168.100.145", "dbadb01", "ictdata", "ictdata168");
            //if (oraDB1.isConnect())
            //{
            //    DataSet ds = oraDB1.executeQuery("select to_char(SYSDATE,'YYYY-MM-DD HH24:MI:SS') sDate FROM DUAL");
            //    AddMessage("读取到数据库 " + "192.168.100.145" + " 时间:" + ds.Tables[0].Rows[0][0].ToString());
            //}
            //oraDB1.disconnect();
        }
        private void ModelPrintEventProcess(string str)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                AddMessage(str);
            }));
        }
        async void UpdateUI()
        {
            while (true)
            {
                await Task.Delay(100);
                switch (Station)
                {
                    case 1:
                        EllipsePLCState2.Fill = GlobalVars.Fx5u_mid.Connect ? Brushes.Green : Brushes.Red;
                        break;
                    default:
                        break;
                }
                EllipseRobotState.Fill = (epsonRC90.IOReceiveStatus && epsonRC90.TestSendStatus && epsonRC90.TestReceiveStatus) ? Brushes.Green : Brushes.Red;
            }            
        }
        void Scan1GetBarcodeCallback(string barcode)
        {
            if (barcode != "Error")
            {
                Mysql mysql = new Mysql();
                if (mysql.Connect())
                {
                    string stm = "SELECT * FROM BODMSG WHERE SCBODBAR = '" + barcode + "' ORDER BY SIDATE DESC LIMIT 0,5";
                    DataSet ds = mysql.Select(stm);
                    DataTable dt = ds.Tables["table0"];
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0]["STATUS"] == DBNull.Value)
                        {
                            stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                            mysql.executeQuery(stm);
                            epsonRC90.BordBarcode[0] = barcode;
                            AddMessage("板 " + barcode + " 绑定");
                            GlobalVars.Fx5u_mid.SetM("M2600", true);
                        }
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                                mysql.executeQuery(stm);
                                epsonRC90.BordBarcode[0] = barcode;
                                AddMessage("板 " + barcode + " 绑定");
                                GlobalVars.Fx5u_mid.SetM("M2600", true);
                            }
                            else
                            {
                                AddMessage("板 " + barcode + " 已测过");
                                GlobalVars.Fx5u_mid.SetM("M2599", true);
                            }
                        }
                    }
                    else
                    {
                        stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                        mysql.executeQuery(stm);
                        epsonRC90.BordBarcode[0] = barcode;
                        AddMessage("板 " + barcode + " 绑定");
                        GlobalVars.Fx5u_mid.SetM("M2600", true);
                    }
                    GlobalVars.Fx5u_mid.SetM("M2597", true);
                }
                else
                {
                    AddMessage("Mysql数据库查询失败");
                    GlobalVars.Fx5u_mid.SetM("M2598", true);
                }
                mysql.DisConnect();
            }
            else
            {
                GlobalVars.Fx5u_mid.SetM("M2598", true);
            }
        }
        void Scan2GetBarcodeCallback(string barcode)
        {
            if (barcode != "Error")
            {
                Mysql mysql = new Mysql();
                if (mysql.Connect())
                {
                    string stm = "SELECT * FROM BODMSG WHERE SCBODBAR = '" + barcode + "' ORDER BY SIDATE DESC LIMIT 0,5";
                    DataSet ds = mysql.Select(stm);
                    DataTable dt = ds.Tables["table0"];
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0]["STATUS"] == DBNull.Value)
                        {
                            stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                            mysql.executeQuery(stm);
                            epsonRC90.BordBarcode[1] = barcode;
                            AddMessage("板 " + barcode + " 绑定");
                            GlobalVars.Fx5u_mid.SetM("M2605", true);
                        }
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                                mysql.executeQuery(stm);
                                epsonRC90.BordBarcode[1] = barcode;
                                AddMessage("板 " + barcode + " 绑定");
                                GlobalVars.Fx5u_mid.SetM("M2605", true);
                            }
                            else
                            {
                                AddMessage("板 " + barcode + " 已测过");
                                GlobalVars.Fx5u_mid.SetM("M2604", true);
                            }
                        }
                    }
                    else
                    {
                        stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                        mysql.executeQuery(stm);
                        epsonRC90.BordBarcode[1] = barcode;
                        AddMessage("板 " + barcode + " 绑定");
                        GlobalVars.Fx5u_mid.SetM("M2605", true);
                    }
                    GlobalVars.Fx5u_mid.SetM("M2602", true);
                }
                else
                {
                    AddMessage("Mysql数据库查询失败");
                    GlobalVars.Fx5u_mid.SetM("M2603", true);
                }
                mysql.DisConnect();
            }
            else
            {
                GlobalVars.Fx5u_mid.SetM("M2603", true);
            }
        }
        void Run()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                #region 扫码
                //扫码（载具）【A轨道】
                if (GlobalVars.Fx5u_mid.ReadM("M2797"))
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AddMessage("轨道A扫码");
                    }));
                    GlobalVars.Fx5u_mid.SetM("M2797", false);
                    GlobalVars.Fx5u_mid.SetMultiM("M2597", new bool[4] { false, false, false, false });
                    scan1.GetBarCode(Scan1GetBarcodeCallback);
                }
                //扫码（载具）【B轨道】
                if (GlobalVars.Fx5u_mid.ReadM("M2802"))
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AddMessage("轨道B扫码");
                    }));
                    GlobalVars.Fx5u_mid.SetM("M2802", false);
                    GlobalVars.Fx5u_mid.SetMultiM("M2602", new bool[4] { false, false, false, false });
                    scan2.GetBarCode(Scan2GetBarcodeCallback);
                }
                #endregion
                #region 上顶
                //从轨道上顶【A轨道】
                if (GlobalVars.Fx5u_mid.ReadM("M2798"))
                {
                    GlobalVars.Fx5u_mid.SetM("M2798", false);
                    Mysql mysql = new Mysql();
                    if (mysql.Connect())
                    {
                        string stm = "";
                        switch (Station)
                        {
                            case 1:
                                stm = "UPDATE BODLINE SET Station9 = 0 , Station1 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                break;
                            default:
                                break;
                        }                        
                        mysql.executeQuery(stm);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            AddMessage("线A上顶1块板");
                        }));
                        epsonRC90.ResetBord(0);
                    }
                    mysql.DisConnect();
                }
                //从轨道上顶【B轨道】
                if (GlobalVars.Fx5u_mid.ReadM("M2803"))
                {
                    GlobalVars.Fx5u_mid.SetM("M2803", false);
                    Mysql mysql = new Mysql();
                    if (mysql.Connect())
                    {
                        string stm = "";
                        switch (Station)
                        {
                            case 1:
                                stm = "UPDATE BODLINE SET Station9 = 0 , Station1 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                break;
                            default:
                                break;
                        }
                        mysql.executeQuery(stm);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            AddMessage("线B上顶1块板");
                        }));
                        epsonRC90.ResetBord(1);
                    }
                    mysql.DisConnect();
                }


                if (LastBanci != GlobalVars.GetBanci())
                {
                    LastBanci = GlobalVars.GetBanci();
                    Inifile.INIWriteValue(iniParameterPath, "Summary", "LastBanci", LastBanci);
                    WriteMachineData();
                    for (int i = 0; i < 4; i++)
                    {
                        epsonRC90.YanmadeTester[i].Clean();
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AddMessage(LastBanci + " 换班数据清零");
                    }));
                    
                }
                #endregion
            }
        }
        void IORun()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10);
                try
                {
                    bool[] X50 = GlobalVars.Fx5u_mid.ReadMultiM("X32", 16);
                    bool[] Y50 = new bool[16];
                    for (int i = 0; i < 16; i++)
                    {
                        epsonRC90.Rc90In[i] = X50[i];
                        Y50[i] = epsonRC90.Rc90Out[i];
                    }
                    GlobalVars.Fx5u_mid.SetMultiM("Y32", Y50);
                    bool[] M2700 = GlobalVars.Fx5u_mid.ReadMultiM("M2700", 64);
                    bool[] M2500 = new bool[64];
                    for (int i = 0; i < 64; i++)
                    {
                        epsonRC90.Rc90In[i + 16] = M2700[i];
                        M2500[i] = epsonRC90.Rc90Out[i + 16];
                    }
                }
                catch
                {

                }
            }
        }       
        private void WriteMachineData()
        {
            string excelpath = @"D:\X1621MachineData.xlsx";

            try
            {
                FileInfo fileInfo = new FileInfo(excelpath);
                if (!File.Exists(excelpath))
                {
                    using (ExcelPackage package = new ExcelPackage(fileInfo))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("MachineData");
                        worksheet.Cells[1, 1].Value = "日期时间";
                        for (int i = 0; i < 4; i++)
                        {
                            worksheet.Cells[1, 2 + i * 4].Value = "穴" + (i + 1).ToString() + "测试数";
                            worksheet.Cells[1, 3 + i * 4].Value = "穴" + (i + 1).ToString() + "良品数";
                            worksheet.Cells[1, 4 + i * 4].Value = "穴" + (i + 1).ToString() + "不良品数";
                            worksheet.Cells[1, 5 + i * 4].Value = "穴" + (i + 1).ToString() + "良率";
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            worksheet.Cells[1, 18 + i * 4].Value = "穴" + (i + 1).ToString() + "测试数AAB";
                            worksheet.Cells[1, 19 + i * 4].Value = "穴" + (i + 1).ToString() + "良品数AAB";
                            worksheet.Cells[1, 20 + i * 4].Value = "穴" + (i + 1).ToString() + "不良品数AAB";
                            worksheet.Cells[1, 21 + i * 4].Value = "穴" + (i + 1).ToString() + "良率AAB";
                        }
                        package.Save();
                    }
                }
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int newrow = worksheet.Dimension.End.Row + 1;
                    worksheet.Cells[newrow, 1].Value = System.DateTime.Now.ToString();
                    for (int i = 0; i < 4; i++)
                    {
                        worksheet.Cells[newrow, 2 + i * 4].Value = epsonRC90.YanmadeTester[i].TestCount_Nomal;
                        worksheet.Cells[newrow, 3 + i * 4].Value = epsonRC90.YanmadeTester[i].PassCount_Nomal;
                        worksheet.Cells[newrow, 4 + i * 4].Value = epsonRC90.YanmadeTester[i].FailCount_Nomal;
                        worksheet.Cells[newrow, 5 + i * 4].Value = epsonRC90.YanmadeTester[i].Yield_Nomal;

                        worksheet.Cells[newrow, 18 + i * 4].Value = epsonRC90.YanmadeTester[i].TestCount;
                        worksheet.Cells[newrow, 19 + i * 4].Value = epsonRC90.YanmadeTester[i].PassCount;
                        worksheet.Cells[newrow, 20 + i * 4].Value = epsonRC90.YanmadeTester[i].FailCount_Nomal;
                        worksheet.Cells[newrow, 21 + i * 4].Value = epsonRC90.YanmadeTester[i].Yield_Nomal;
                    }
                    package.Save();
                }
                //AddMessage("保存机台生产数据完成");
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    AddMessage(ex.Message);
                }));                
            }
        }
        
        private void LanguageSwitchChinese(object sender, RoutedEventArgs e)
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            string requestedCulture = @"Resources\zh-cn.xaml";
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void LanguageSwitchEnglish(object sender, RoutedEventArgs e)
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            string requestedCulture = @"Resources\en-us.xaml";
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void SaveParameterButtonClick(object sender, RoutedEventArgs e)
        {
            Inifile.INIWriteValue(iniParameterPath, "System", "MachineID", MachineID.Text);
        }

        private void MaterialPageSelect(object sender, RoutedEventArgs e)
        {

        }
    }
}
