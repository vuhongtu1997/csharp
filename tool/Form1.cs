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
using System.Threading;
using System.Linq.Expressions;

namespace tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string hexvalue = "";
        int time;
        int numbytes;
        byte[] rxbyte;
        int port_Name_length;
        byte mac_High = 0;
        byte mac_Low = 0;
        byte mac_High_Old = 0;
        byte mac_Low_Old = 0;
        byte mac_High_RSP = 1;
        byte mac_Low_RSP = 0;
        int time_Check = 0;
        int check_RSP = 0;
        int time_Old = 0;
        byte num_RSP = 0;
        byte num_RSP_Old = 0;
        byte[] check_node = { 0xe8, 0xff, 0x00, 0x00, 0x00, 0x00, 0x02, 0x01, 0xff, 0xff, 0x82, 0x4b };
        byte[] dim_cct = { 0xe8, 0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xff, 0xff, 0x82, 0x5e, 0xff, 0xff, 0x20, 0x03, 0x00, 0x00, 0x00 };
        byte[] off = { 0xe8, 0xff, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0xff, 0xff, 0x82, 0x02, 0x00, 0x00 };
        int pass_Count = 0;
        bool MACExit = false;


        private void timer1_Tick(object sender, EventArgs e)
        {
            time++;
            string cb_Text = cb_Com.Text;
            string[] port_Name = SerialPort.GetPortNames();

            if (port_Name_length == 0)
            {
                btn_Connect.Enabled = false;
                lb_State_Com.Text = "Disconnected";
                lb_State_Com.BackColor = Color.LightSalmon;
                btn_Connect.Text = "CONNECT";
                serialPort1.Close();
            }

            if (port_Name_length != port_Name.Length)
            {
                port_Name_length = port_Name.Length;
                {
                    if (port_Name_length != 0)
                    {
                        cb_Com.Items.Clear();
                        for (int i = 0; i < port_Name_length; i++)
                        {
                            cb_Com.Items.Add(port_Name[i]);
                        }
                        cb_Com.SelectedIndex = 0;

                        bool compare_Text = false;

                        for (int i = 0; i < port_Name.Length; i++)
                        {
                            if (string.Compare(cb_Text, port_Name[i], true) == 0)
                            {
                                compare_Text = true;
                                serialPort1.Close();
                            }
                        }

                        if (!compare_Text)
                        {
                            btn_Connect.Enabled = true;
                            lb_State_Com.Text = "Disconnected";
                            lb_State_Com.BackColor = Color.LightSalmon;
                            btn_Connect.Text = "CONNECT";
                            cb_Com.Enabled = true;
                        }
                    }
                    else
                    {
                        btn_Connect.Enabled = false;
                        lb_State_Com.Text = "Disconnected";
                        lb_State_Com.BackColor = Color.LightSalmon;
                        btn_Connect.Text = "CONNECT";
                        cb_Com.Text = null;
                        cb_Com.Items.Clear();
                        serialPort1.Close();
                    }
                }
            }

            if (!serialPort1.IsOpen)
            {
                MACExit = false;
            }

            if (MACExit == false && serialPort1.IsOpen)
            {
                mac_High_RSP = 0;
                mac_Low_RSP = 0;
                check_RSP = 0;
                serialPort1.Write(check_node, 0, 12);
            }

            if (MACExit && serialPort1.IsOpen)
            {                
                mac_Low_Old = mac_Low;
                mac_High_Old = mac_High;
                if (time - time_Check == 10 && check_RSP == 0)
                {
                    dim_cct[12] = dim_cct[13] = 0xff;
                    dim_cct[8] = mac_Low_Old;
                    dim_cct[9] = mac_High_Old;
                    dim_cct[15] = 0x03;
                    serialPort1.Write(dim_cct, 0, 19);
                    mac_High_RSP = 0;
                    mac_Low_RSP = 0;
                    time_Old = time;
                }

                if (time - time_Check == 25 && check_RSP == 1)
                {
                    dim_cct[12] = 0x66;
                    dim_cct[13] = 0x26;
                    serialPort1.Write(dim_cct, 0, 19);
                    mac_High_RSP = mac_Low_RSP = 0;
                    time_Old = time;
                }

                if (time - time_Check == 40 && check_RSP == 2)
                {
                    dim_cct[12] = 0x66;
                    dim_cct[13] = 0x26;
                    dim_cct[15] = 0x4e;
                    serialPort1.Write(dim_cct, 0, 19);
                    mac_High_RSP = mac_Low_RSP = 0;
                    time_Old = time;
                }

                if (time - time_Check == 55 && check_RSP == 3)
                {
                    dim_cct[12] = dim_cct[13] = 0xff;
                    dim_cct[15] = 0x4e;
                    serialPort1.Write(dim_cct, 0, 19);
                    mac_High_RSP = mac_Low_RSP = 0;
                    time_Old = time;
                }
                

                if (check_RSP == 4 && time - time_Check == 65)
                {
                    off[8] = mac_Low_Old;
                    off[9] = mac_High_Old;
                    serialPort1.Write(off, 0, 14);
                    time_Check = time;
                    MACExit = false;
                    mac_High_RSP = 0;
                    mac_Low_RSP = 0;
                    time_Old = time;
                    pass_Count++;
                }

                if (time - time_Old >= 40)
                {
                    time_Check = time;
                    MACExit = false;
                    mac_High_RSP = 0;
                    mac_Low_RSP = 0;
                    time_Old = time;
                    num_RSP_Old = num_RSP = 0;
                }
            }

            if (check_RSP == 5)
            {
                time_Check = time;
                MACExit = false;
                mac_High_RSP = 0;
                mac_Low_RSP = 0;
                time_Old = time;
                num_RSP_Old = num_RSP = 0;
            }

            if (mac_Low_RSP == mac_Low_Old && mac_High == mac_High_Old && num_RSP != num_RSP_Old)
            {
                check_RSP++;
                num_RSP_Old = num_RSP;
                mac_Low_RSP = mac_High_RSP = 0;
            }

            /*if (check_RSP == 3)
            {
                pass_Count++;
                lb_Pass.Text = "Pass: " + Convert.ToString(pass_Count);
            }*/
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            btn_Connect.Enabled = true;
            if (lb_State_Com.Text == "Disconnected")
            {
                serialPort1.PortName = cb_Com.Text;
                serialPort1.Open();
                btn_Connect.Text = "DISCONNECT";
                lb_State_Com.Text = "Connected";
                lb_State_Com.BackColor = Color.LightGreen;
                cb_Com.Enabled = false;
            }
            else
            {
                serialPort1.Close();
                btn_Connect.Text = "CONNECT";
                lb_State_Com.Text = "Disconnected";
                lb_State_Com.BackColor = Color.LightSalmon;
                cb_Com.Enabled = true;
            }
        }

        private void data_In(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(30);
            numbytes = serialPort1.BytesToRead;
            rxbyte = new byte[numbytes];
            if (numbytes >= 8)
            {
                for (int i = 0; i < numbytes; i++)
                {
                    rxbyte[i] = (byte)serialPort1.ReadByte();
                }
                if (rxbyte[0] == 0x0a && rxbyte[3] == 0x81)
                {
                    mac_Low = rxbyte[4];
                    mac_High = rxbyte[5];
                    time_Check = time;
                    MACExit = true;
                }
                if (rxbyte[0] == 0x0f && rxbyte[3] == 0xb5)
                {
                    mac_Low_RSP = rxbyte[6];
                    mac_High_RSP = rxbyte[7];
                    num_RSP = rxbyte[16];     
                }
                foreach (byte b in rxbyte)
                {
                    hexvalue = hexvalue + (b.ToString("X2")) + "";
                }
                this.Invoke(new EventHandler(Showdata));
                hexvalue = "";
            }

        }

        private void Showdata(object sender, EventArgs e)
        {
            
           
        }
    }
}
