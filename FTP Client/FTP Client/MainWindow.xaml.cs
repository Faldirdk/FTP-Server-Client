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
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;

namespace FTP_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> fuldFil = new List<string>();
        byte[] byteArr;
        string fileNameFullPath;
        string fileName;
        byte SOH = 0x01;
        string messageReceived;
        TcpClient tcpclnt;
        Stream stm;

        bool connectBool = false;
        bool fileNameBool = false;
        bool EOFBool = false;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Connect();            
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }

            ReceiveServerMessages();

        }

        private void ReceiveServerMessages()
        {
            Stream s = tcpclnt.GetStream();
            byte[] getMessage = new byte[1000];
            int k = s.Read(getMessage, 0, 1000);
            for (int i = 0; i < k; i++)
            {
                messageReceived += Convert.ToChar(getMessage[i]);
            }

            switch (messageReceived[0])
            {
                case '\u0006':
                    if (connectBool)
                    {                       
                    block_Display.Text += "Acknowledgement received \n Connection established \n";
                        connectBool = false;
                    }
                    else if (EOFBool)
                    {
                        block_Display.Text += "Acknowledgement received \n File transfered \n";
                        EOFBool = false;
                    }
                    else if (fileNameBool)
                    {
                        block_Display.Text += "Acknowledgement received \n Sending data... \n";
                        Send();
                        // EOF to server
                        fileNameBool = false;
                    }
                    break;
                case '\u0019':
                    block_Display.Text += "Negative acknowledgement received \n";
                    break;
                default:
                    break;
            }
        }

        private void btn_find_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.FilterIndex = 1;

            openFileDialog1.Multiselect = true;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                // Open the selected file to read.
                fileNameFullPath = openFileDialog1.FileName;
                fileName = System.IO.Path.GetFileName(fileNameFullPath);
            }
        }
        public void Connect()
        {
            tcpclnt = new TcpClient();
            Console.WriteLine("Connecting.....");

            tcpclnt.Connect("127.0.0.1", 8001);
            // use the ipaddress as in the server program

            Console.WriteLine("Connected");
            connectBool = true;
        }
        public void Send()
        {
            stm = tcpclnt.GetStream();

            stm.Write(byteArr, 0, byteArr.Length);

            //byte[] bb = new byte[5000];
            //int k = stm.Read(bb, 0, 5000);

            //for (int i = 0; i < k; i++)
            //    Console.Write(Convert.ToChar(bb[i]));

            //tcpclnt.Close();
        }
        public void Receive()
        {

        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(fileName);          
            AddKontrolTegn(SOH);
            Send();
            fileNameBool = true;
            byteArr = File.ReadAllBytes(fileNameFullPath);
            Send();
            Receive();
            byteArr = File.ReadAllBytes(fileNameFullPath);
            Send();
        }

        private void SendMessage(string besked)
        {
            stm = tcpclnt.GetStream();
            byteArr = Encoding.ASCII.GetBytes(besked);
        }

        public void AddKontrolTegn(byte kontrolTegn)
        {
            List<byte> byteList = new List<byte>();
            byteList = byteArr.ToList();
            byteList.Insert(0, kontrolTegn);
            byteArr = byteList.ToArray();
        }
    }
}