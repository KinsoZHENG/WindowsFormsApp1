using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        SerialPort sp = null;   //声明一个串口类
        bool isOpen = false;    //打开串口标志位
        bool isSetProperty = false;     //属性设置标志位
        bool isHex = false;     //十六进制显示标志位


        public Form1()
        {
            InitializeComponent();      //窗口初始化
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            for(int i = 0; i < 10; i++)
            {
                cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbxCOMPort.SelectedIndex = 4;
            //列出常用波特率
            cbxBaudRate.Items.Add("1200");
            cbxBaudRate.Items.Add("2400");
            cbxBaudRate.Items.Add("4800");
            cbxBaudRate.Items.Add("9600");
            cbxBaudRate.Items.Add("19200");
            cbxBaudRate.Items.Add("38400");
            cbxBaudRate.Items.Add("43000");
            cbxBaudRate.Items.Add("56000");
            cbxBaudRate.Items.Add("57600");
            cbxBaudRate.Items.Add("115200");
            cbxBaudRate.SelectedIndex = 3;
            //停止位列表
            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;
            //列出数据位
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;
            //列出奇偶校验位
            cbxParity.Items.Add("无");
            cbxParity.Items.Add("奇校验");
            cbxParity.Items.Add("偶校验");
            cbxParity.SelectedIndex = 0;
            //设置为默认 char 显示
            rbnChar.Checked = true;
        }
        private void btnCheckCOM_Click(object sender,EventArgs e)       //串口检测
        {
            bool comExistence = false;  //有可用的串口标志位
            cbxCOMPort.Items.Clear();   //清除当前串口号中的所有名称
            for(int i = 0; i < 10; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
                    comExistence = true;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            /*if (comExistence)
            {
                cbxCOMPort.SelectedIndex = 0;   //使listbox显示第一个添加的串口
            }*/
            if(!comExistence)
            {
                MessageBox.Show("没有找到可用的串口", "错误提示");
            }
        }

        private bool CheckPortSetting()
        {
            if (cbxCOMPort.Text.Trim() == "") return false;
            if (cbxBaudRate.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            if (cbxParity.Text.Trim() == "") return false;
            if (cbxStopBits.Text.Trim() == "") return false;
            return true;
        }

        private bool CheckSendData()
        {
            if (tbxSendData.Text.Trim() == "") return false;
            return true;
        }

        private void SetPortProperty()      //设置串口属性
        {
            sp = new SerialPort();
            sp.PortName = cbxCOMPort.Text.Trim();   //设置串口名称
            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim());     //设置串口波特率

            float f = Convert.ToSingle(cbxStopBits.Text.Trim());    //设置停止位
            if(f == 0)                              // 设置为波特率 --- 0
            {
                sp.StopBits = StopBits.None;            
            }
            else if(f == 1.5)                      // 设置为波特率 --- 1.5 
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if(f == 1)                       // 设置为波特率 --- 1
            {
                sp.StopBits = StopBits.One;
            }
            else if(f == 2)                      // 设置为波特率 --- 2 
            {
                sp.StopBits = StopBits.Two;
            }
            else                                // 默认波特率 --- 1
            {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());     //设置数据位
            string s = cbxParity.Text.Trim();   //设置奇偶校验位
            if(s.CompareTo("无") == 0)
            {
                sp.Parity = Parity.None;
            }
            else if(s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if(s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }

            sp.ReadTimeout = -1;    //设置超时读取时间
            sp.RtsEnable = true;

            //定义DataRecevied时间,当串口收到数据后触发事件
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataRecevied);
            if (rbnHEX.Checked)
            {
                isHex = true;
            }
            else
            {
                isHex = false;
            }
        }

        private void sp_DataRecevied(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);//延时100ms等待接收数据完成
            //invoke跨线程访问ui
            this.Invoke(new EventHandler(delegate
            {
                if(isHex == false)          //HEX 方式显示
                {
                    tbxRecvData.Text += sp.ReadLine();
                    tbxRecvData.Text += Environment.NewLine;
                   // tbxRecvData.Text += 
                }
                else
                {
                    Byte[] RecevidedData = new Byte[sp.BytesToRead];    //创建接收字节数组
                    sp.Read(RecevidedData, 0, RecevidedData.Length);
                    String RecvDataText = null;
                    for(int i = 0; i < RecevidedData.Length - 1; i++)
                    {
                        RecvDataText += ("0x" + RecevidedData[i].ToString("X2") + " ");
                    }
                    tbxRecvData.Text += RecvDataText;
                }
                sp.DiscardInBuffer();   //丢弃接收缓冲区的数据
            }));
        }

        private void btnSend_Click(object sender, EventArgs e)      //发送串口数据
        {
            if (isOpen)  //写串口数据
            {
                try
                {
                    sp.WriteLine(tbxSendData.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("发送数据时发生错误！", "错误提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开！", "错误提示");
                return;
            }
        }

        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            if(isOpen == false)
            {
                if(!CheckPortSetting())     //检测串口设置
                {
                    MessageBox.Show("串口未设置!", "错误提示");
                    return;
                }
                if(!isSetProperty)  //串口未设置则设置串口
                {
                    SetPortProperty();
                    isSetProperty = true;
                }
                try//打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "关闭串口";
                    //串口打开后则相关的串口设置为 不可用
                    cbxCOMPort.Enabled = false;
                    cbxBaudRate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopBits.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHEX.Enabled = false;
                }
                catch (Exception)
                {
                    //打开串口失败后，相应标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或已被占用！", "错误提示");
                }
            }
            else
            {
                try  //打开串口
                {
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpenCom.Text = "打开串口";
                    //关闭串口后，串口设置选项更改成可使用
                    cbxCOMPort.Enabled = true;
                    cbxBaudRate.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxParity.Enabled = true;
                    cbxStopBits.Enabled = true;
                    rbnChar.Enabled = true;
                    rbnHEX.Enabled = true;

                }
                catch (Exception)
                {
                    MessageBox.Show ("关闭串口时发生未知错误");
                }
            }
        }
        private void btnCleanData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
            tbxSendData.Text = "";
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbxBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbxStopBits_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void tbxRecvData_TextChanged(object sender, EventArgs e)        //光标锁定，打印数据时 默认滚动条底部
        {
            this.tbxRecvData.SelectionStart = this.tbxRecvData.Text.Length;
            this.tbxRecvData.SelectionLength = 0;
            this.tbxRecvData.ScrollToCaret();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
