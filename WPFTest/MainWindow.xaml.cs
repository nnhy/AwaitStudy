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
using AwaitTest;
using NewLife.Log;

namespace WPFTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(Object sender, RoutedEventArgs e)
        {
            Await.Start();
        }

        private void Window_Loaded(Object sender, RoutedEventArgs e)
        {
            XTrace.Log = new MyLog { Text = richTextBox };
            //XTrace.UseWinFormControl(richTextBox);
        }
    }

    class MyLog : Logger
    {
        public RichTextBox Text { get; set; }

        protected override void OnWrite(LogLevel level, String format, params Object[] args)
        {
            Text.Dispatcher.InvokeAsync(() =>
            {
                Text.AppendText(format.F(args) + Environment.NewLine);
            });
        }
    }
}