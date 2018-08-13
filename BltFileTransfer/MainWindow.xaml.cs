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
        }

        private void InitialPortName()
        {
            var PortNames = SerialPort.GetPortNames().ToList();
            PortNames.ForEach(a => ComboTxtPortName.Items.Add(a));
        }

        private void SendMessage(string message)
        {
            using (var port = new SerialPort(ComboTxtPortName.SelectedValue.ToString(), 9600))
            {
                port.Open();
                port.WriteLine(message);
                port.Close();
            }
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var sendstring=GetFileBinaryFromZip();
            SendMessage(sendstring);
        }

        private string GetFileBinaryFromZip()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "压缩文件|*.zip;*.jar";//文件扩展名
            dialog.CheckFileExists = true;
            dialog.ShowDialog();

            if (!string.IsNullOrEmpty(dialog.FileName))//可以上传压缩包.zip 或者jar包
            {

                byte[] byteArray = FileBinaryConvertHelper.File2Bytes(dialog.FileName);//文件转成byte二进制数组
                string JarContent = Convert.ToBase64String(byteArray);//将二进制转成string类型，可以存到数据库里面了       
                return JarContent;
            }
            else
            {
                throw new Exception();
            }
        }
    }

  
}
