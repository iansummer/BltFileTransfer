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
using System.IO.Ports;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace BltFileTransfer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitialPortName();
            InitialAction();
        }

        private void InitialPortName()
        {
            var PortNames = SerialPort.GetPortNames().ToList();
            ComboTxtPortName.ItemsSource = PortNames;
            ComboTxtPortName_R.ItemsSource = PortNames;

            var BaudRateList = new List<int>() { 9600,19200,};
            ComboTxtBaudRate.ItemsSource = BaudRateList;
            ComboTxtBaudRate_R.ItemsSource = BaudRateList;
            ComboTxtBaudRate.SelectedIndex = 1;
            ComboTxtBaudRate_R.SelectedIndex = 1;

            Progress.Maximum = 100;
        }


        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var path= SelectFileName();
            if (path.Equals(string.Empty)) return;
            using (var port = new SerialPort(ComboTxtPortName.SelectedValue.ToString(), (int)ComboTxtBaudRate.SelectedValue))
            {
                port.Open();
                await SendTask(port,path);
                port.Close();
            }
        }


        private SerialPort ReveievePoer;

        private void BtnReceieve_Click(object sender, RoutedEventArgs e)
        {
            if ((Application.Current.Properties["Status"] == null)|| ((bool)Application.Current.Properties["Status"] == false))
            {
                ReveievePoer = new SerialPort(ComboTxtPortName_R.SelectedValue.ToString(), (int)ComboTxtBaudRate_R.SelectedValue);
                ReveievePoer.DataReceived += DataReceivedHandler;
                ReveievePoer.ReadBufferSize = 1024;
                ReveievePoer.ReadTimeout = 1000;
                File.Delete(@"E:\MyTemp\2.zip");
                ReveievePoer.Open();
                Application.Current.Properties["Status"] = true;

            }
            else
            {
                ReveievePoer.Close();
                ReveievePoer.Dispose();
                Application.Current.Properties["Status"] = false; 
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort sp = (SerialPort)sender;
            var length = sp.BytesToRead;
            byte[] a = new byte[length];
            sp.Read(a, 0, length);
            //var bytes=System.Text.Encoding.UTF8.GetBytes(str);
            if (a.Length == 0) return;
            WriteData(@"E:\MyTemp\2.zip", a);


        }

        private void WriteData(string filepath, byte[] data)
        {
            FileStream fs;
            if (File.Exists(filepath))
            {
                 fs = new FileStream(filepath, FileMode.Append);
            }
            else
            {
                 fs = new FileStream(filepath, FileMode.CreateNew);
            }
           
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(data, 0, data.Length);
            }
            fs.Close();
        }

        private async Task SendTask(SerialPort port, string filepath)
        {
          
           await Task.Run(()=> {
               using (FileStream sr = new FileStream(filepath, FileMode.Open))
               {
                   long fileLength = sr.Length;
                   long fileposition = 0;
                   sr.Position = fileposition;
                   long leftlength = fileLength;
                   byte[] buff;
                   while (leftlength > 0)
                   {
                       Thread.Sleep(100);
                       if (leftlength > 1024)
                       {
                           buff = new byte[1024];
                           sr.Read(buff, 0, Convert.ToInt32(1024)); 
                           port.Write(buff, 0, 1024);
                           leftlength = leftlength - 1024;
                       }
                       else
                       {
                           buff = new byte[leftlength];
                           sr.Read(buff, 0, (int)leftlength);
                           port.Write(buff, 0, (int)leftlength);
                           leftlength = 0;
                       }
                       this.Dispatcher.Invoke(BindingValue, (fileLength-leftlength) * 100 / fileLength);
                   }
               }
               port.Close();

           });
        }
  
        private string SelectFileName()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "压缩文件|*.zip;*.jar";
            dialog.CheckFileExists = true;
            dialog.ShowDialog();
            return dialog.FileName;
        }


        private void InitialAction()
        {
            BindingValue = BindingProgressValue;
        }

        private Action<double> BindingValue;


        private void BindingProgressValue(double value)
        {
            Progress.Value = value;
        }

   
    }

  
}
