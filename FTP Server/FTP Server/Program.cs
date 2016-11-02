using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace FTP_Server
{
    class Program
    {
        static string filNavn = "";
        static char kontrolChar;
        static FTPServer FTP = new FTPServer();
        static void Main(string[] args)
        {
            try
            {
                //IPAddress ipAd = IPAddress.Parse("127.0.0.1");
                // use local m/c IP address, and 
                // use the same in the client

                /* Initializes the Listener */
                TcpListener myList = new TcpListener(IPAddress.Any, 8001);

                /* Start Listeneting at the specified port */
                myList.Start();

                Console.WriteLine("The server is running at port 8001...");
                Console.WriteLine("The local End point is  :" +
                                  myList.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");

                Socket s = myList.AcceptSocket();

                Console.WriteLine("Connection accepted from " + s.RemoteEndPoint + "\n");


                ASCIIEncoding asen = new ASCIIEncoding();
                s.Send(new byte[] { 0x06 });
                Console.WriteLine("Sent Acknowledgement\n");

                Thread reader = new Thread(() =>
                {
                    while (true)
                    {
                        Receive(s);
                    }
                });
                reader.Start();


                

                

                byte[] b = new byte[1000];
                int k = s.Receive(b);
                //Console.WriteLine("Recieved...");
                File.WriteAllBytes(filNavn, b);



                /* clean up */
                //s.Close();
                //myList.Stop();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        private static void Receive(Socket s)
        {
            byte[] d = new byte[1000];
            int f = s.Receive(d);
            string byteToString = Encoding.ASCII.GetString(d);

            kontrolChar = byteToString[0];
            filNavn = byteToString.Substring(1).Trim('\0');

            switch (kontrolChar)
            {
                case '\u0001':
                    s.Send(new byte[] { 0x06 });
                    break;
                case '\u0004':

                default:
                    
                    break;
            }
            
        }
    }
}

