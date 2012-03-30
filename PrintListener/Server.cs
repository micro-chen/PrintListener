﻿using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace PrintListener
{
    public class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;

        public Server()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 515);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;
            string fileName = Guid.NewGuid().ToString();

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                    using (FileStream stream = new FileStream("C:\\" + fileName, FileMode.Append))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            writer.Write(message);
                            //writer.Write(bytesRead);
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                Console.Write(".");

            }

            tcpClient.Close();
            Console.WriteLine("file complete");

            RawPrinterHelper.SendFileToPrinter("Bullzip PDF Printer", "C:\\" + fileName);

        }
    }
}