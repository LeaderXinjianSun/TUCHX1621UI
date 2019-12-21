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

namespace TUCHX1621UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddMessage("软件加载完成");
        }

        private void HomePageSelect(object sender, RoutedEventArgs e)
        {

        }

        private void ParameterPageSelect(object sender, RoutedEventArgs e)
        {

        }

        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }

        private void EpsonStartButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void EpsonPauseButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void EpsonContinueButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void EpsonReStartButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void FuncButtonClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
