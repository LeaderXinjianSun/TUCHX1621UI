﻿using System;
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
        int Station { set; get; }
        string LineID1 { set; get; }
        string LineID2 { set; get; }
        #endregion
        #region 变量
        private EpsonRC90 epsonRC90;
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Scan scan1, scan2, scan3, scan4;
        string LastBanci = "";
        uint liaoinput = 0;
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
                    Mysql mysql = new Mysql();
                    if (mysql.Connect())
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
                                stm = "UPDATE BODLINE SET Station9 = Station9 + 1 WHERE WHERE LineID = '" + LineID2 + "'";
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
        void DockStation1Run()
        {
            int cycle1 = 0, cycle2 = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                
                try
                {
                    //A轨道
                    cycle1++;
                    if (GlobalVars.Fx5u_left2.ReadM("M2797"))
                    {
                        GlobalVars.Fx5u_left2.SetM("M2797", false);
                        GlobalVars.Fx5u_left2.SetMultiM("M2596", new bool[5] { false, false, false, false, false });
                        scan3.GetBarCode(Scan3GetBarcodeCallback);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            AddMessage("接驳台1轨道A扫码");
                        }));
                    }
                    else
                    {
                        if (cycle1 > 20)
                        {
                            cycle1 = 0;
                            Mysql mysql = new Mysql();
                            if (mysql.Connect())
                            {
                                string stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                                DataSet ds = mysql.Select(stm);
                                DataTable dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int station7count = (int)dt.Rows[0]["Station7"];
                                    if (station7count > 0)
                                    {
                                        int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                                        if (bordcount < 5)//轨道+测试机板数量 < 5 ，下1块板
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2610", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线A内接驳台1后板数:" + bordcount.ToString() + " < 5,下1块板");
                                            }));
                                        }
                                        else
                                        {                                            
                                            int station8count = (int)dt.Rows[0]["Station8"];
                                            if (station8count < 5)//轨道 + 测试机板数量 >= 5 ，接驳台2#数量<5，下1块板
                                            {
                                                GlobalVars.Fx5u_left2.SetM("M2610", true);
                                                this.Dispatcher.Invoke(new Action(() =>
                                                {
                                                    AddMessage("线A内接驳台2板数:" + station8count.ToString() + " < 5,下1块板");
                                                }));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ;//没有存储板，无动作
                                    }
                                    
                                }
                            }
                            mysql.DisConnect();
                        }
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2798"))//存储响应【A轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2798", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station9 = Station9 - 1, Station7 = Station7 + 1 WHERE WHERE LineID = '" + LineID1 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台1存储1块板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2799"))//放新板响应【A轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2799", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station9 = Station9 - 1, Station10 = Station10 + 1 WHERE WHERE LineID = '" + LineID1 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台1放1块新板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2800"))//放存储板响应【A轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2800", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station7 = Station7 - 1, Station10 = Station10 + 1 WHERE WHERE LineID = '" + LineID1 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台1放1块存储板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    //B轨道
                    cycle2++;
                    if (GlobalVars.Fx5u_left2.ReadM("M2803"))
                    {
                        GlobalVars.Fx5u_left2.SetM("M2803", false);
                        GlobalVars.Fx5u_left2.SetMultiM("M2602", new bool[5] { false, false, false, false, false });
                        scan4.GetBarCode(Scan4GetBarcodeCallback);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            AddMessage("接驳台1轨道B扫码");
                        }));
                    }
                    else
                    {
                        if (cycle2 > 20)
                        {
                            cycle2 = 0;
                            Mysql mysql = new Mysql();
                            if (mysql.Connect())
                            {
                                string stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                                DataSet ds = mysql.Select(stm);
                                DataTable dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int station7count = (int)dt.Rows[0]["Station7"];
                                    if (station7count > 0)
                                    {
                                        int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                                        if (bordcount < 5)//轨道+测试机板数量 < 5 ，下1块板
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2611", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线B内接驳台1后板数:" + bordcount.ToString() + " < 5,下1块板");
                                            }));
                                        }
                                        else
                                        {
                                            int station8count = (int)dt.Rows[0]["Station8"];
                                            if (station8count < 5)//轨道 + 测试机板数量 >= 5 ，接驳台2#数量<5，下1块板
                                            {
                                                GlobalVars.Fx5u_left2.SetM("M2611", true);
                                                this.Dispatcher.Invoke(new Action(() =>
                                                {
                                                    AddMessage("线B内接驳台2板数:" + station8count.ToString() + " < 5,下1块板");
                                                }));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ;//没有存储板，无动作
                                    }

                                }
                            }
                            mysql.DisConnect();
                        }
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2804"))//存储响应【B轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2804", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station9 = Station9 - 1, Station7 = Station7 + 1 WHERE WHERE LineID = '" + LineID2 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台1存储1块板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2805"))//放新板响应【B轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2805", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station9 = Station9 - 1, Station10 = Station10 + 1 WHERE WHERE LineID = '" + LineID2 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台1放1块新板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2806"))//放存储板响应【B轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2806", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station7 = Station7 - 1, Station10 = Station10 + 1 WHERE WHERE LineID = '" + LineID2 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台1放1块存储板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                }
                catch(Exception ex)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AddMessage(ex.Message);
                    }));
                }
            }
        }
        void DockStation2Run()
        {
            int cycle1 = 0, cycle2 = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                try
                {
                    //A轨道
                    cycle1++;
                    if (GlobalVars.Fx5u_left2.ReadM("M2797"))
                    {
                        GlobalVars.Fx5u_left2.SetM("M2797", false);
                        GlobalVars.Fx5u_left2.SetMultiM("M2596", new bool[5] { false, false, false, false, false });
                        scan3.GetBarCode(Scan3GetBarcodeCallback2);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            AddMessage("接驳台2轨道A扫码");
                        }));
                    }
                    else
                    {
                        if (cycle1 > 20)
                        {
                            cycle1 = 0;
                            Mysql mysql = new Mysql();
                            if (mysql.Connect())
                            {
                                string stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                                DataSet ds = mysql.Select(stm);
                                DataTable dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int station8count = (int)dt.Rows[0]["Station8"];
                                    if (station8count > 0)
                                    {
                                        int bordcount = (int)dt.Rows[0]["Station11"] + (int)dt.Rows[0]["Station6"];
                                        if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2610", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线A内接驳台2后板数:" + bordcount.ToString() + " < 1,下1块板");
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        ;//没有存储板，无动作
                                    }

                                }
                            }
                            mysql.DisConnect();
                        }
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2798"))//存储响应【A轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2798", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station10 = Station10 - 1, Station8 = Station8 + 1 WHERE WHERE LineID = '" + LineID1 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台2存储1块板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2799"))//放新板响应【A轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2799", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station10 = Station10 - 1, Station11 = Station11 + 1 WHERE WHERE LineID = '" + LineID1 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台2放1块新板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2800"))//放存储板响应【A轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2800", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station8 = Station8 - 1, Station11 = Station11 + 1 WHERE WHERE LineID = '" + LineID1 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台2放1块存储板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    //B轨道
                    cycle2++;
                    if (GlobalVars.Fx5u_left2.ReadM("M2803"))
                    {
                        GlobalVars.Fx5u_left2.SetM("M2803", false);
                        GlobalVars.Fx5u_left2.SetMultiM("M2602", new bool[5] { false, false, false, false, false });
                        scan4.GetBarCode(Scan4GetBarcodeCallback2);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            AddMessage("接驳台1轨道B扫码");
                        }));
                    }
                    else
                    {
                        if (cycle2 > 20)
                        {
                            cycle2 = 0;
                            Mysql mysql = new Mysql();
                            if (mysql.Connect())
                            {
                                string stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                                DataSet ds = mysql.Select(stm);
                                DataTable dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int station8count = (int)dt.Rows[0]["Station7"];
                                    if (station8count > 0)
                                    {
                                        int bordcount = (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station11"];
                                        if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2611", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线B内接驳台2后板数:" + bordcount.ToString() + " < 1,下1块板");
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        ;//没有存储板，无动作
                                    }

                                }
                            }
                            mysql.DisConnect();
                        }
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2804"))//存储响应【B轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2804", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station10 = Station10 - 1, Station8 = Station8 + 1 WHERE WHERE LineID = '" + LineID2 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台2存储1块板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2805"))//放新板响应【B轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2805", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station10 = Station10 - 1, Station11 = Station11 + 1 WHERE WHERE LineID = '" + LineID2 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台2放1块新板");
                            }));
                        }
                        mysql.DisConnect();
                    }
                    if (GlobalVars.Fx5u_left2.ReadM("M2806"))//放存储板响应【B轨道】
                    {
                        GlobalVars.Fx5u_left2.SetM("M2806", false);
                        Mysql mysql = new Mysql();
                        if (mysql.Connect())
                        {
                            string stm = "UPDATE BODLINE SET Station8 = Station8 - 1, Station11 = Station11 + 1 WHERE WHERE LineID = '" + LineID2 + "'";
                            mysql.executeQuery(stm);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                AddMessage("线A接驳台2放1块存储板");
                            }));
                        }
                        mysql.DisConnect();
                    }
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
                window1.Title = "鵬鼎控股 測試工程部 D5X TUCHX1621UI:" + Station.ToString();
                MachineID.Text = Inifile.INIGetStringValue(iniParameterPath, "System", "MachineID", "X1621_1");
                LastBanci = Inifile.INIGetStringValue(iniParameterPath, "Summary", "LastBanci", "null");
                liaoinput = uint.Parse(Inifile.INIGetStringValue(iniParameterPath, "Summary", "liaoinput", "0"));
                string plc_ip = Inifile.INIGetStringValue(iniParameterPath, "System", "PLC2IP", "192.168.10.2");
                int plc_port = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "System", "PLC2PORT", "8000"));
                GlobalVars.Fx5u_mid = new Fx5u(plc_ip, plc_port);
                scan1 = new Scan();
                string COM = Inifile.INIGetStringValue(iniParameterPath, "System", "ScanCOM1", "COM0");
                scan1.ini(COM);
                scan2 = new Scan();
                COM = Inifile.INIGetStringValue(iniParameterPath, "System", "ScanCOM2", "COM1");
                scan2.ini(COM);
                switch (Station)
                {
                    case 1:
                        statusBarItem0.Visibility = Visibility.Collapsed;
                        statusBarItem1.Visibility = Visibility.Collapsed;
                        Task.Run(() => { StationEnterRun(); });
                        AddMessage("机台站:" + Station.ToString() + ";轨道入口功能开启");
                        break;
                    case 2:
                    case 6:
                        plc_ip = Inifile.INIGetStringValue(iniParameterPath, "System", "PLC0IP", "192.168.10.2");
                        plc_port = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "System", "PLC0PORT", "8000"));
                        GlobalVars.Fx5u_left1 = new Fx5u(plc_ip, plc_port);
                        plc_ip = Inifile.INIGetStringValue(iniParameterPath, "System", "PLC1IP", "192.168.10.2");
                        plc_port = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "System", "PLC0P1RT", "8000"));
                        GlobalVars.Fx5u_left2 = new Fx5u(plc_ip, plc_port);
                        scan3 = new Scan();
                        COM = Inifile.INIGetStringValue(iniParameterPath, "System", "ScanCOM3", "COM0");
                        scan3.ini(COM);
                        scan4 = new Scan();
                        COM = Inifile.INIGetStringValue(iniParameterPath, "System", "ScanCOM4", "COM1");
                        scan4.ini(COM);
                        switch (Station)
                        {
                            case 2:
                                Task.Run(() => { DockStation1Run(); });
                                AddMessage("机台站:" + Station.ToString() + ";接驳台1功能开启");
                                break;
                            case 6:
                                Task.Run(() => { DockStation2Run(); });
                                AddMessage("机台站:" + Station.ToString() + ";接驳台2功能开启");
                                break;
                            default:
                                break;
                        }
                        break;
                    case 3:
                    case 4:
                    case 5:
                        statusBarItem0.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }

                epsonRC90 = new EpsonRC90();
                epsonRC90.ModelPrint += ModelPrintEventProcess;
                UpdateUI();
                Task.Run(() => { Run(); });
                Task.Run(() => { IORun(); });
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
                #region 良率面板显示
                //良率界面显示
                string[] Yieldstrs0 = PassStatusProcess(epsonRC90.YanmadeTester[0].Yield_Nomal);
                PassStatusDisplay0.Text = "测试机1" + Yieldstrs0[0];
                switch (Yieldstrs0[1])
                {
                    case "Blue":
                        PassStatusDisplay0.Foreground = Brushes.Blue;
                        break;
                    case "Red":
                        PassStatusDisplay0.Foreground = Brushes.Red;
                        break;
                    case "Green":
                        PassStatusDisplay0.Foreground = Brushes.Green;
                        break;
                    case "Black":
                        PassStatusDisplay0.Foreground = Brushes.Black;
                        break;
                    default:
                        break;
                }
                string[] Yieldstrs1 = PassStatusProcess(epsonRC90.YanmadeTester[1].Yield_Nomal);
                PassStatusDisplay1.Text = "测试机2" + Yieldstrs1[0];
                switch (Yieldstrs1[1])
                {
                    case "Blue":
                        PassStatusDisplay1.Foreground = Brushes.Blue;
                        break;
                    case "Red":
                        PassStatusDisplay1.Foreground = Brushes.Red;
                        break;
                    case "Green":
                        PassStatusDisplay1.Foreground = Brushes.Green;
                        break;
                    case "Black":
                        PassStatusDisplay1.Foreground = Brushes.Black;
                        break;
                    default:
                        break;
                }
                string[] Yieldstrs2 = PassStatusProcess(epsonRC90.YanmadeTester[2].Yield_Nomal);
                PassStatusDisplay2.Text = "测试机3" + Yieldstrs2[0];
                switch (Yieldstrs2[1])
                {
                    case "Blue":
                        PassStatusDisplay2.Foreground = Brushes.Blue;
                        break;
                    case "Red":
                        PassStatusDisplay2.Foreground = Brushes.Red;
                        break;
                    case "Green":
                        PassStatusDisplay2.Foreground = Brushes.Green;
                        break;
                    case "Black":
                        PassStatusDisplay2.Foreground = Brushes.Black;
                        break;
                    default:
                        break;
                }
                string[] Yieldstrs3 = PassStatusProcess(epsonRC90.YanmadeTester[3].Yield_Nomal);
                PassStatusDisplay3.Text = "测试机4" + Yieldstrs3[0];
                switch (Yieldstrs3[1])
                {
                    case "Blue":
                        PassStatusDisplay3.Foreground = Brushes.Blue;
                        break;
                    case "Red":
                        PassStatusDisplay3.Foreground = Brushes.Red;
                        break;
                    case "Green":
                        PassStatusDisplay3.Foreground = Brushes.Green;
                        break;
                    case "Black":
                        PassStatusDisplay3.Foreground = Brushes.Black;
                        break;
                    default:
                        break;
                }

                switch (epsonRC90.YanmadeTester[0].TestResult)
                {
                    case TestResult.Ng:
                        Tester0.Result = "F";
                        break;
                    case TestResult.Pass:
                        Tester0.Result = "P";
                        break;
                    default:
                        Tester0.Result = "N";
                        break;
                }
                switch (epsonRC90.YanmadeTester[1].TestResult)
                {
                    case TestResult.Ng:
                        Tester1.Result = "F";
                        break;
                    case TestResult.Pass:
                        Tester1.Result = "P";
                        break;
                    default:
                        Tester1.Result = "N";
                        break;
                }
                switch (epsonRC90.YanmadeTester[2].TestResult)
                {
                    case TestResult.Ng:
                        Tester2.Result = "F";
                        break;
                    case TestResult.Pass:
                        Tester2.Result = "P";
                        break;
                    default:
                        Tester2.Result = "N";
                        break;
                }
                switch (epsonRC90.YanmadeTester[3].TestResult)
                {
                    case TestResult.Ng:
                        Tester3.Result = "F";
                        break;
                    case TestResult.Pass:
                        Tester3.Result = "P";
                        break;
                    default:
                        Tester3.Result = "N";
                        break;
                }
                TestCount_0.Text = epsonRC90.YanmadeTester[0].TestCount_Nomal.ToString();
                PassCount_0.Text = epsonRC90.YanmadeTester[0].PassCount_Nomal.ToString();
                TestCount_1.Text = epsonRC90.YanmadeTester[1].TestCount_Nomal.ToString();
                PassCount_1.Text = epsonRC90.YanmadeTester[1].PassCount_Nomal.ToString();
                TestCount_2.Text = epsonRC90.YanmadeTester[2].TestCount_Nomal.ToString();
                PassCount_2.Text = epsonRC90.YanmadeTester[2].PassCount_Nomal.ToString();
                TestCount_3.Text = epsonRC90.YanmadeTester[3].TestCount_Nomal.ToString();
                PassCount_3.Text = epsonRC90.YanmadeTester[3].PassCount_Nomal.ToString();
                TestCount_Total.Text = liaoinput.ToString();
                TestCount_Total1.Text = (epsonRC90.YanmadeTester[0].PassCount + epsonRC90.YanmadeTester[1].PassCount + epsonRC90.YanmadeTester[2].PassCount + epsonRC90.YanmadeTester[3].PassCount).ToString();
                if (liaoinput > 0)
                {
                    if ((double)(epsonRC90.YanmadeTester[0].PassCount + epsonRC90.YanmadeTester[1].PassCount + epsonRC90.YanmadeTester[2].PassCount + epsonRC90.YanmadeTester[3].PassCount) < liaoinput)
                    {
                        Yield_Total.Text = ((double)(epsonRC90.YanmadeTester[0].PassCount + epsonRC90.YanmadeTester[1].PassCount + epsonRC90.YanmadeTester[2].PassCount + epsonRC90.YanmadeTester[3].PassCount) / liaoinput * 100).ToString("F2");
                    }
                    else
                    {
                        Yield_Total.Text = "100";
                    }
                }
                else
                {
                    Yield_Total.Text = "0";
                }
                #endregion

                #region 时间统计
                TestTime0.Text = epsonRC90.YanmadeTester[0].TestSpan.ToString("F1");
                TestTime1.Text = epsonRC90.YanmadeTester[1].TestSpan.ToString("F1");
                TestTime2.Text = epsonRC90.YanmadeTester[2].TestSpan.ToString("F1");
                TestTime3.Text = epsonRC90.YanmadeTester[3].TestSpan.ToString("F1");
                TestIdle0.Text = epsonRC90.YanmadeTester[0].TestIdle.ToString("F1");
                TestIdle1.Text = epsonRC90.YanmadeTester[1].TestIdle.ToString("F1");
                TestIdle2.Text = epsonRC90.YanmadeTester[2].TestIdle.ToString("F1");
                TestIdle3.Text = epsonRC90.YanmadeTester[3].TestIdle.ToString("F1");
                TestCycle0.Text = epsonRC90.YanmadeTester[0].TestCycle.ToString("F1");
                TestCycle1.Text = epsonRC90.YanmadeTester[1].TestCycle.ToString("F1");
                TestCycle2.Text = epsonRC90.YanmadeTester[2].TestCycle.ToString("F1");
                TestCycle3.Text = epsonRC90.YanmadeTester[3].TestCycle.ToString("F1");

                TestCycleAve.Text = ((epsonRC90.YanmadeTester[0].TestCycle + epsonRC90.YanmadeTester[1].TestCycle + epsonRC90.YanmadeTester[2].TestCycle + epsonRC90.YanmadeTester[3].TestCycle) / 4).ToString("F1");
                #endregion
            }
        }
        private string[] PassStatusProcess(double f)
        {
            string[] strs = new string[2];
            if (f > 98)
            {
                strs[0] = "良率" + f.ToString() + "% 优秀";
                strs[1] = "Blue";
            }
            else
            {
                if (f > 95)
                {
                    strs[0] = "良率" + f.ToString() + "% 正常";
                    strs[1] = "Green";
                }
                else
                {
                    if (f == 0)
                    {
                        strs[0] = "良率" + f.ToString() + "% 未知";
                        strs[1] = "Black";
                    }
                    else
                    {
                        strs[0] = "良率" + f.ToString() + "% 异常";
                        strs[1] = "Red";
                    }
                }
            }
            return strs;
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
                            switch (Station)
                            {
                                case 1:
                                    stm = "UPDATE BODLINE SET Station9 = Station9 - 1 , Station1 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                    stm = "UPDATE BODLINE SET Station10 = Station10 - 1 , Station" + Station.ToString() + " = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                    break;
                                case 6:
                                    stm = "UPDATE BODLINE SET Station11 = Station11 - 1 , Station6 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
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
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                                mysql.executeQuery(stm);
                                epsonRC90.BordBarcode[0] = barcode;
                                AddMessage("板 " + barcode + " 绑定");
                                GlobalVars.Fx5u_mid.SetM("M2600", true);
                                switch (Station)
                                {
                                    case 1:
                                        stm = "UPDATE BODLINE SET Station9 = Station9 - 1 , Station1 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                        break;
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                        stm = "UPDATE BODLINE SET Station10 = Station10 - 1 , Station" + Station.ToString() + " = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                        break;
                                    case 6:
                                        stm = "UPDATE BODLINE SET Station11 = Station11 - 1 , Station6 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
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
                        switch (Station)
                        {
                            case 1:
                                stm = "UPDATE BODLINE SET Station9 = Station9 - 1 , Station1 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                stm = "UPDATE BODLINE SET Station10 = Station10 - 1 , Station" + Station.ToString() + " = 1 WHERE WHERE LineID = '" + LineID1 + "'";
                                break;
                            case 6:
                                stm = "UPDATE BODLINE SET Station11 = Station11 - 1 , Station6 = 1 WHERE WHERE LineID = '" + LineID1 + "'";
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
                            switch (Station)
                            {
                                case 1:
                                    stm = "UPDATE BODLINE SET Station9 = Station9 - 1 , Station1 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                    break;
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                    stm = "UPDATE BODLINE SET Station10 = Station10 - 1 , Station" + Station.ToString() + " = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                    break;
                                case 6:
                                    stm = "UPDATE BODLINE SET Station11 = Station11 - 1 , Station6 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
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
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "INSERT INTO BODMSG (SCBODBAR, STATUS) VALUES('" + barcode + "','ON')";
                                mysql.executeQuery(stm);
                                epsonRC90.BordBarcode[1] = barcode;
                                AddMessage("板 " + barcode + " 绑定");
                                GlobalVars.Fx5u_mid.SetM("M2605", true);
                                switch (Station)
                                {
                                    case 1:
                                        stm = "UPDATE BODLINE SET Station9 = Station9 - 1 , Station1 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                        break;
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                        stm = "UPDATE BODLINE SET Station10 = Station10 - 1 , Station" + Station.ToString() + " = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                        break;
                                    case 6:
                                        stm = "UPDATE BODLINE SET Station11 = Station11 - 1 , Station6 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
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
                        switch (Station)
                        {
                            case 1:
                                stm = "UPDATE BODLINE SET Station9 = Station9 - 1 , Station1 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                stm = "UPDATE BODLINE SET Station10 = Station10 - 1 , Station" + Station.ToString() + " = 1 WHERE WHERE LineID = '" + LineID2 + "'";
                                break;
                            case 6:
                                stm = "UPDATE BODLINE SET Station11 = Station11 - 1 , Station6 = 1 WHERE WHERE LineID = '" + LineID2 + "'";
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
        void Scan3GetBarcodeCallback(string barcode)
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
                            stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                            ds = mysql.Select(stm);
                            dt = ds.Tables["table0"];
                            if (dt.Rows.Count > 0)
                            {
                                int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                                if (bordcount < 5)//轨道+测试机板数量 < 5 不存储，放板
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2596", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线A内接驳台1后板数:" + bordcount.ToString() + " < 5,直接放板");
                                    }));
                                }
                                else
                                {
                                    int station8count = (int)dt.Rows[0]["Station8"];
                                    if (station8count < 5)//轨道+测试机板数量 >= 5 存储，接驳台2#数量<5，不存储，放板
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2596", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线A内接驳台2板数:" + station8count.ToString() + " < 5,直接放板");
                                        }));
                                    }
                                    else
                                    {
                                        int station7count = (int)dt.Rows[0]["Station7"];
                                        if (station7count < 10) //轨道 + 测试机板数量 >= 5 存储，接驳台2#数量>=5，本身已存板数<10，存储
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2597", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线A内接驳台1板数:" + station7count.ToString() + " < 10,存储板");
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                                ds = mysql.Select(stm);
                                dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                                    if (bordcount < 5)//轨道+测试机板数量 < 5 不存储，放板
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2596", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线A内接驳台1后板数:" + bordcount.ToString() + " < 5,直接放板");
                                        }));
                                    }
                                    else
                                    {
                                        int station8count = (int)dt.Rows[0]["Station8"];
                                        if (station8count < 5)//轨道+测试机板数量 >= 5 存储，接驳台2#数量<5，不存储，放板
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2596", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线A内接驳台2板数:" + station8count.ToString() + " < 5,直接放板");
                                            }));
                                        }
                                        else
                                        {
                                            int station7count = (int)dt.Rows[0]["Station7"];
                                            if (station7count < 10) //轨道 + 测试机板数量 >= 5 存储，接驳台2#数量>=5，本身已存板数<10，存储
                                            {
                                                GlobalVars.Fx5u_left2.SetM("M2597", true);
                                                this.Dispatcher.Invoke(new Action(() =>
                                                {
                                                    AddMessage("线A内接驳台1板数:" + station7count.ToString() + " < 10,存储板");
                                                }));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AddMessage("板 " + barcode + " 已测过");
                                GlobalVars.Fx5u_left2.SetM("M2600", true);
                            }
                        }
                    }
                    else
                    {
                        stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                        ds = mysql.Select(stm);
                        dt = ds.Tables["table0"];
                        if (dt.Rows.Count > 0)
                        {
                            int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                            if (bordcount < 5)//轨道+测试机板数量 < 5 不存储，放板
                            {
                                GlobalVars.Fx5u_left2.SetM("M2596", true);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线A内接驳台1后板数:" + bordcount.ToString() + " < 5,直接放板");
                                }));
                            }
                            else
                            {
                                int station8count = (int)dt.Rows[0]["Station8"];
                                if (station8count < 5)//轨道+测试机板数量 >= 5 存储，接驳台2#数量<5，不存储，放板
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2596", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线A内接驳台2板数:" + station8count.ToString() + " < 5,直接放板");
                                    }));
                                }
                                else
                                {
                                    int station7count = (int)dt.Rows[0]["Station7"];
                                    if (station7count < 10) //轨道 + 测试机板数量 >= 5 存储，接驳台2#数量>=5，本身已存板数<10，存储
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2597", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线A内接驳台1板数:" + station7count.ToString() + " < 10,存储板");
                                        }));
                                    }
                                }
                            }
                        }
                    }
                    GlobalVars.Fx5u_left2.SetM("M2598", true);
                }
                else
                {
                    GlobalVars.Fx5u_left2.SetM("M2599", true);
                }
                mysql.DisConnect();
            }
            else
            {
                GlobalVars.Fx5u_left2.SetM("M2599", true);
            }
        }
        /// <summary>
        /// 扫码确认是新板子了，再判断是留还是放
        /// </summary>        
        void Scan4GetBarcodeCallback(string barcode)
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
                            stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                            ds = mysql.Select(stm);
                            dt = ds.Tables["table0"];
                            if (dt.Rows.Count > 0)
                            {
                                int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                                if (bordcount < 5)//轨道+测试机板数量 < 5 不存储，放板
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2602", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线B内接驳台1后板数:" + bordcount.ToString() + " < 5,直接放板");
                                    }));
                                }
                                else
                                {
                                    int station8count = (int)dt.Rows[0]["Station8"];
                                    if (station8count < 5)//轨道+测试机板数量 >= 5 存储，接驳台2#数量<5，不存储，放板
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2602", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线B内接驳台2板数:" + station8count.ToString() + " < 5,直接放板");
                                        }));
                                    }
                                    else
                                    {
                                        int station7count = (int)dt.Rows[0]["Station7"];
                                        if (station7count < 10) //轨道 + 测试机板数量 >= 5 存储，接驳台2#数量>=5，本身已存板数<10，存储
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2603", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线B内接驳台1板数:" + station7count.ToString() + " < 10,存储板");
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                                ds = mysql.Select(stm);
                                dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                                    if (bordcount < 5)//轨道+测试机板数量 < 5 不存储，放板
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2602", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线B内接驳台1后板数:" + bordcount.ToString() + " < 5,直接放板");
                                        }));
                                    }
                                    else
                                    {
                                        int station8count = (int)dt.Rows[0]["Station8"];
                                        if (station8count < 5)//轨道+测试机板数量 >= 5 存储，接驳台2#数量<5，不存储，放板
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2602", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线B内接驳台2板数:" + station8count.ToString() + " < 5,直接放板");
                                            }));
                                        }
                                        else
                                        {
                                            int station7count = (int)dt.Rows[0]["Station7"];
                                            if (station7count < 10) //轨道 + 测试机板数量 >= 5 存储，接驳台2#数量>=5，本身已存板数<10，存储
                                            {
                                                GlobalVars.Fx5u_left2.SetM("M2603", true);
                                                this.Dispatcher.Invoke(new Action(() =>
                                                {
                                                    AddMessage("线B内接驳台1板数:" + station7count.ToString() + " < 10,存储板");
                                                }));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AddMessage("板 " + barcode + " 已测过");
                                GlobalVars.Fx5u_left2.SetM("M2606", true);
                            }
                        }
                    }
                    else
                    {
                        stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                        ds = mysql.Select(stm);
                        dt = ds.Tables["table0"];
                        if (dt.Rows.Count > 0)
                        {
                            int bordcount = (int)dt.Rows[0]["Station2"] + (int)dt.Rows[0]["Station3"] + (int)dt.Rows[0]["Station4"] + (int)dt.Rows[0]["Station5"] + (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station10"] + (int)dt.Rows[0]["Station11"];
                            if (bordcount < 5)//轨道+测试机板数量 < 5 不存储，放板
                            {
                                GlobalVars.Fx5u_left2.SetM("M2602", true);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线B内接驳台1后板数:" + bordcount.ToString() + " < 5,直接放板");
                                }));
                            }
                            else
                            {
                                int station8count = (int)dt.Rows[0]["Station8"];
                                if (station8count < 5)//轨道+测试机板数量 >= 5 存储，接驳台2#数量<5，不存储，放板
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2602", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线B内接驳台2板数:" + station8count.ToString() + " < 5,直接放板");
                                    }));
                                }
                                else
                                {
                                    int station7count = (int)dt.Rows[0]["Station7"];
                                    if (station7count < 10) //轨道 + 测试机板数量 >= 5 存储，接驳台2#数量>=5，本身已存板数<10，存储
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2603", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线B内接驳台1板数:" + station7count.ToString() + " < 10,存储板");
                                        }));
                                    }
                                }
                            }
                        }
                    }
                    GlobalVars.Fx5u_left2.SetM("M2604", true);
                }
                else
                {
                    GlobalVars.Fx5u_left2.SetM("M2605", true);
                }
                mysql.DisConnect();
            }
            else
            {
                GlobalVars.Fx5u_left2.SetM("M2605", true);
            }
        }
        void Scan3GetBarcodeCallback2(string barcode)
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
                            stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                            ds = mysql.Select(stm);
                            dt = ds.Tables["table0"];
                            if (dt.Rows.Count > 0)
                            {
                                int bordcount = (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station11"];
                                if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2596", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线A内接驳台2后板数:" + bordcount.ToString() + " < 5,直接放板");
                                    }));
                                }
                                else
                                {
                                    int station8count = (int)dt.Rows[0]["Station7"];
                                    if (station8count < 5) //轨道+测试机板数量 >= 1 存储，本身已存板数<5，存储
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2597", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线A内接驳台2板数:" + station8count.ToString() + " < 5,存储板");
                                        }));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                                ds = mysql.Select(stm);
                                dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int bordcount = (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station11"];
                                    if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2596", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线A内接驳台2后板数:" + bordcount.ToString() + " < 5,直接放板");
                                        }));
                                    }
                                    else
                                    {
                                        int station8count = (int)dt.Rows[0]["Station7"];
                                        if (station8count < 5) //轨道+测试机板数量 >= 1 存储，本身已存板数<5，存储
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2597", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线A内接驳台2板数:" + station8count.ToString() + " < 5,存储板");
                                            }));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AddMessage("板 " + barcode + " 已测过");
                                GlobalVars.Fx5u_left2.SetM("M2600", true);
                            }
                        }
                    }
                    else
                    {
                        stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID1 + "' ORDER BY SIDATE DESC";
                        ds = mysql.Select(stm);
                        dt = ds.Tables["table0"];
                        if (dt.Rows.Count > 0)
                        {
                            int bordcount = (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station11"];
                            if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                            {
                                GlobalVars.Fx5u_left2.SetM("M2596", true);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线A内接驳台2后板数:" + bordcount.ToString() + " < 5,直接放板");
                                }));
                            }
                            else
                            {
                                int station8count = (int)dt.Rows[0]["Station7"];
                                if (station8count < 5) //轨道+测试机板数量 >= 1 存储，本身已存板数<5，存储
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2597", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线A内接驳台2板数:" + station8count.ToString() + " < 5,存储板");
                                    }));
                                }
                            }
                        }
                    }
                    GlobalVars.Fx5u_left2.SetM("M2598", true);
                }
                else
                {
                    GlobalVars.Fx5u_left2.SetM("M2599", true);
                }
                mysql.DisConnect();
            }
            else
            {
                GlobalVars.Fx5u_left2.SetM("M2599", true);
            }
        }
        void Scan4GetBarcodeCallback2(string barcode)
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
                            stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                            ds = mysql.Select(stm);
                            dt = ds.Tables["table0"];
                            if (dt.Rows.Count > 0)
                            {
                                int bordcount = (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station11"];
                                if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2602", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线B内接驳台2后板数:" + bordcount.ToString() + " < 1,直接放板");
                                    }));
                                }
                                else
                                {
                                    int station8count = (int)dt.Rows[0]["Station8"];
                                    if (station8count < 5) //轨道+测试机板数量 >= 1 存储，本身已存板数<5，存储
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2603", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线B内接驳台2板数:" + station8count.ToString() + " < 5,存储板");
                                        }));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((string)dt.Rows[0]["STATUS"] == "OFF")
                            {
                                stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                                ds = mysql.Select(stm);
                                dt = ds.Tables["table0"];
                                if (dt.Rows.Count > 0)
                                {
                                    int bordcount = (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station11"];
                                    if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                                    {
                                        GlobalVars.Fx5u_left2.SetM("M2602", true);
                                        this.Dispatcher.Invoke(new Action(() =>
                                        {
                                            AddMessage("线B内接驳台2后板数:" + bordcount.ToString() + " < 1,直接放板");
                                        }));
                                    }
                                    else
                                    {
                                        int station8count = (int)dt.Rows[0]["Station8"];
                                        if (station8count < 5) //轨道+测试机板数量 >= 1 存储，本身已存板数<5，存储
                                        {
                                            GlobalVars.Fx5u_left2.SetM("M2603", true);
                                            this.Dispatcher.Invoke(new Action(() =>
                                            {
                                                AddMessage("线B内接驳台2板数:" + station8count.ToString() + " < 5,存储板");
                                            }));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AddMessage("板 " + barcode + " 已测过");
                                GlobalVars.Fx5u_left2.SetM("M2606", true);
                            }
                        }
                    }
                    else
                    {
                        stm = "SELECT * FROM BODLINE WHERE LineID = '" + LineID2 + "' ORDER BY SIDATE DESC";
                        ds = mysql.Select(stm);
                        dt = ds.Tables["table0"];
                        if (dt.Rows.Count > 0)
                        {
                            int bordcount = (int)dt.Rows[0]["Station6"] + (int)dt.Rows[0]["Station11"];
                            if (bordcount < 1)//轨道+测试机板数量 < 1 不存储，放板
                            {
                                GlobalVars.Fx5u_left2.SetM("M2602", true);
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    AddMessage("线B内接驳台2后板数:" + bordcount.ToString() + " < 1,直接放板");
                                }));
                            }
                            else
                            {
                                int station8count = (int)dt.Rows[0]["Station8"];
                                if (station8count < 5) //轨道+测试机板数量 >= 1 存储，本身已存板数<5，存储
                                {
                                    GlobalVars.Fx5u_left2.SetM("M2603", true);
                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        AddMessage("线B内接驳台2板数:" + station8count.ToString() + " < 5,存储板");
                                    }));
                                }
                            }
                        }
                    }
                    GlobalVars.Fx5u_left2.SetM("M2604", true);
                }
                else
                {
                    GlobalVars.Fx5u_left2.SetM("M2605", true);
                }
                mysql.DisConnect();
            }
            else
            {
                GlobalVars.Fx5u_left2.SetM("M2605", true);
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
                #region 测完下放
                if (GlobalVars.Fx5u_mid.ReadM("M2799"))
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AddMessage("轨道A测完下放");
                    }));
                    Mysql mysql = new Mysql();
                    if (mysql.Connect())
                    {
                        string stm = "UPDATE BODLINE SET Station" + Station.ToString() + " = 0 WHERE WHERE LineID = '" + LineID1 + "'";
                        mysql.executeQuery(stm);
                    }
                    mysql.DisConnect();
                    GlobalVars.Fx5u_mid.SetM("M2799", false);
                    
                }
                if (GlobalVars.Fx5u_mid.ReadM("M2804"))
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        AddMessage("轨道B测完下放");
                    }));
                    Mysql mysql = new Mysql();
                    if (mysql.Connect())
                    {
                        string stm = "UPDATE BODLINE SET Station" + Station.ToString() + " = 0 WHERE WHERE LineID = '" + LineID2 + "'";
                        mysql.executeQuery(stm);
                    }
                    mysql.DisConnect();
                    GlobalVars.Fx5u_mid.SetM("M2804", false);

                }
                #endregion

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
                { }
                try
                {
                    switch (Station)
                    {
                        case 1:
                            break;
                        case 2:
                        case 6:
                            {
                                bool[] M2764_2 = GlobalVars.Fx5u_left2.ReadMultiM("M2764", 32);
                                GlobalVars.Fx5u_left1.SetMultiM("M2564", M2764_2);
                                bool[] M2764 = GlobalVars.Fx5u_mid.ReadMultiM("M2764", 32);
                                GlobalVars.Fx5u_left2.SetMultiM("M2564", M2764);
                            }
                            break;
                        default:
                            {
                                bool[] M2764 = GlobalVars.Fx5u_mid.ReadMultiM("M2764", 32);
                                GlobalVars.Fx5u_left2.SetMultiM("M2564", M2764);
                            }
                            break;
                    }
                }
                catch
                { }


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
