/**
 This code is released as is with no warranties
 You are free to use and modify the code for personal use only

 Written by: Richard Myrick T. Arellaga
**/

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

namespace Fingerprint_Scanner_Demo
{
    public partial class frmDemo : Form
    {

        byte[] fpsdata = new byte[24];
        byte[] fpsreply = new byte[24];
        int checksum;
        int templateNumber;


        public frmDemo()
        {
            InitializeComponent();
        }

        private void frmDemo_Load(object sender, EventArgs e)
        {
            foreach(string s in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(s);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(btnConnect.Text == "Connect")
            {
                btnConnect.Text = "Disconnect";
                try
                {
                    serialPort1.PortName = comboBox1.Text.ToString();
                    serialPort1.BaudRate = 115200;
                    serialPort1.Open();
                    uartRx.Start();
                    lblStatus.Text = "Connected to " + comboBox1.Text.ToString();
                }
                catch { }

            }else if(btnConnect.Text == "Disconnect")
            {
                btnConnect.Text = "Connect";
                try
                {
                    uartRx.Stop();
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();
                    serialPort1.Open();
                    lblStatus.Text = "Disconnected";
                }
                catch { }

            }
        }

        private void calculateChecksum()
        {
            int x;
            checksum = 0;
            for (x = 0; x <= 21; x++)
            {
                checksum += fpsdata[x];
            }

            fpsdata[22] = Convert.ToByte(checksum & 0x00FF);
            fpsdata[23] = Convert.ToByte((checksum & 0xFF00) >> 8); ;
        }

        private string byteToHex(byte[] combyte)
        {
            StringBuilder builder = new StringBuilder(combyte.Length);
            foreach (byte data in combyte)
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            return builder.ToString().ToUpper();
        }

        private void uartRx_Tick(object sender, EventArgs e)
        {
            if (serialPort1.BytesToRead > 10)
            {
                serialPort1.Read(fpsreply, 0, 24);
            }

            txtReply.Text = byteToHex(fpsreply);

            if (fpsreply[2] == 0x02 && fpsreply[3] == 0x01 && fpsreply[6] == 0x00 && fpsreply[8] != 0xF4)
            {

                lblStatus.Text = "Hello User " + fpsreply[8].ToString();

            }
            else if (fpsreply[2] == 0x02 && fpsreply[3] == 0x01 && fpsreply[6] == 0x01 && fpsreply[9] == 0x12 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = "You are not enrolled";
            }
            else if (fpsreply[2] == 0x03 && fpsreply[3] == 0x01 && fpsreply[8] == 0xF1 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = "Please press a finger to enroll";
            }
            else if (fpsreply[2] == 0x03 && fpsreply[3] == 0x01 && fpsreply[8] == 0xF2 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = "Please press the same finger second time...";
            }
            else if (fpsreply[2] == 0x03 && fpsreply[3] == 0x01 && fpsreply[8] == 0xF3 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = "Please press the same finger last time";
            }
            else if (fpsreply[2] == 0x03 && fpsreply[3] == 0x01 && fpsreply[8] == 0xF4 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = "Lift Finger";
            }
            else if (fpsreply[2] == 0x03 && fpsreply[3] == 0x01 && fpsreply[4] == 0x06 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = "Enrolled!";
            }
            else if (fpsreply[2] == 0x05 && fpsreply[3] == 0x01 && fpsreply[4] == 0x04 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = "User " + fpsreply[8].ToString() + " Deleted";
            }
            else if (fpsreply[2] == 0x06 && fpsreply[3] == 0x01 && fpsreply[6] == 0x00 && fpsreply[8] != 0xF4)
            {
                lblStatus.Text = fpsreply[8].ToString() + "Users Deleted!";
            }else if(fpsreply[6] == 0x01 && fpsreply[8] == 0x15)
            {
                lblStatus.Text = "No enrolled fingerprint templates!";
            }
            else if (fpsreply[6] == 0x01 && fpsreply[8] == 0x14)
            {
                lblStatus.Text = "Location not empty";
            }
            else if (fpsreply[6] == 0x01 && fpsreply[8] == 0x21)
            {
                lblStatus.Text = "Bad image quality!";
            }
            else if (fpsreply[6] == 0x01 && fpsreply[8] == 0x60)
            {
                lblStatus.Text = "Location not valid";
            }
        }

        private void btnIdentify_Click(object sender, EventArgs e)
        {
            Array.Clear(fpsdata, 0, 24);

            fpsdata[0] = 0x55;
            fpsdata[1] = 0xAA;
            fpsdata[2] = 0x02;
            fpsdata[3] = 0x01;

            calculateChecksum();

            serialPort1.Write(fpsdata, 0, 24);
        }

        private void btnEnroll_Click(object sender, EventArgs e)
        {
            Array.Clear(fpsdata, 0, 24);

            templateNumber = Convert.ToInt32(txtTemplate.Text);

            fpsdata[0] = 0x55;
            fpsdata[1] = 0xAA;
            fpsdata[2] = 0x03;
            fpsdata[3] = 0x01;
            fpsdata[4] = 0x02;
            fpsdata[5] = Convert.ToByte((templateNumber & 0xFF00) >> 8);
            fpsdata[6] = Convert.ToByte(templateNumber & 0x00FF);

            calculateChecksum();

            serialPort1.Write(fpsdata, 0, 24);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Array.Clear(fpsdata, 0, 24);

            templateNumber = Convert.ToInt32(txtTemplate.Text);

            fpsdata[0] = 0x55;
            fpsdata[1] = 0xAA;
            fpsdata[2] = 0x05;
            fpsdata[3] = 0x01;
            fpsdata[4] = 0x02;
            fpsdata[5] = Convert.ToByte((templateNumber & 0xFF00) >> 8);
            fpsdata[6] = Convert.ToByte(templateNumber & 0x00FF);

            calculateChecksum();

            serialPort1.Write(fpsdata, 0, 24);
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {

            Array.Clear(fpsdata, 0, 24);

            fpsdata[0] = 0x55;
            fpsdata[1] = 0xAA;
            fpsdata[2] = 0x06;
            fpsdata[3] = 0x01;
            

            calculateChecksum();

            serialPort1.Write(fpsdata, 0, 24);

        }
    }
}
