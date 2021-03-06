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
using System.Threading;

namespace FTP_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //dsad
        List<string> fuldFil = new List<string>();
        byte[] byteArr;
        string fileNameFullPath;
        string fileName;
        byte SOH = 0x01;
        byte EOF = 0x04;
        int counter = 0;
        string messageReceived;
        TcpClient tcpclnt;
        Stream s;

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

            s = tcpclnt.GetStream();
            ReceiveServerMessages();
        }

        private void ReceiveServerMessages()
        {
            byte[] getMessage = new byte[100];
            int k = s.Read(getMessage, 0, 100);
            for (int i = 0; i < k; i++)
            {
                messageReceived += Convert.ToChar(getMessage[i]);
            }

            switch (messageReceived[0])
            {
                case '\u0006':

                    if (counter == 0)
                    {
                        Dispatcher.Invoke(() => { block_Display.Text += "Acknowledgement received \n Connection established \n"; });

                        counter++;

                    }

                    else if (counter == 1)
                    {
                        Dispatcher.Invoke(() => { block_Display.Text += "Acknowledgement received \n Sending data... \n"; });
                        // EOF to server
                        byteArr = File.ReadAllBytes(fileNameFullPath);
                        Send();
                        Thread.Sleep(100);
                        byteArr = new byte[] { EOF };
                        Send();
                        Dispatcher.Invoke(() => { block_Display.Text += "EOF sent"; });

                        counter++;

                    }
                    else if (counter == 2)
                    {
                        Dispatcher.Invoke(() => { block_Display.Text += "Acknowledgement received \n File transfered \n"; });
                        counter = 1;

                    }

                    break;
                case '\u0019':
                    Dispatcher.Invoke(() => { block_Display.Text += "Negative acknowledgement received \n"; });
                    break;
                default:
                    break;
            }
            s.Flush();
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
                tbResults.Text = fileName;
            }
        }
        public void Connect()
        {
            tcpclnt = new TcpClient();
            Console.WriteLine("Connecting.....");

            //tcpclnt.Connect("192.168.1.27", 8001);
            // Local tcpclnt.Connect("127.0.0.1", 8001);
            tcpclnt.Connect("10.131.67.167", 8001);

            // use the ipaddress as in the server program

            Console.WriteLine("Connected");

        }
        public void Send()
        {
            //s = tcpclnt.GetStream();

            s.Write(byteArr, 0, byteArr.Length);
            s.Flush();

        }
        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if (counter == 2)
            {
                counter = 1;
            }
            Thread reader = new Thread(() =>
            {
                while (true)
                {
                    ReceiveServerMessages();
                }
            });
            reader.Start();
            SendMessage(fileName);
            AddKontrolTegn(SOH);
            Send();

        }



        private void SendMessage(string besked)
        {
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

