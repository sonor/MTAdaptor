using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTAdaptor
{
    public class ServerTCP
    {

        TcpListener mTCPListener;
        public static List<TcpClient> mClients;
        public static string response = null;
  

        public  ServerTCP(IPAddress ipAddress, int incomingPort, bool recordMode)
        {
          
                mClients = new List<TcpClient>();
                StartListeningForIncomingConnection(ipAddress, incomingPort, recordMode);
           


        }



        public  async void StartListeningForIncomingConnection(IPAddress ipAddress , int incomingPort, bool recordMode)
        {

            Console.WriteLine($"Incoming TCP running on {ipAddress}:{incomingPort}");
            if (!recordMode)
                Console.WriteLine("Waiting for job...");

            mTCPListener = new TcpListener(ipAddress, incomingPort);

            try
            {
                mTCPListener.Start();

                while (true)
                {
                    TcpClient returnedByAccept = await mTCPListener.AcceptTcpClientAsync();

                    mClients.Add(returnedByAccept);

                    Debug.WriteLine(
                        string.Format("Client connected successfully, count: {0} - {1}",
                        mClients.Count, returnedByAccept.Client.RemoteEndPoint)
                        );

                    TCPClientHandler(returnedByAccept);

                   

                }

            }
            catch (Exception e)
            {
               Console.WriteLine(e.ToString());
            }
        }
       
        private async void TCPClientHandler(TcpClient paramClient)
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = paramClient.GetStream();
                reader = new StreamReader(stream);

                char[] buff = new char[64];

                while (true)
                {
                    Debug.WriteLine("*** Ready to read");

                    int nRet = await reader.ReadAsync(buff, 0, buff.Length);
                    /*
                    System.Diagnostics.Debug.WriteLine("Returned: " + nRet);

                    if (nRet == 0)
                    {
                        RemoveClient(paramClient);

                        Console.WriteLine("Socket disconnected");
                        break;
                    }
                    */
                    string receivedText = new string(buff);

                    Console.WriteLine($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")} => Incoming TCP data receieved");

                    receivedText = receivedText.TrimEnd('\0');

                    ServerHTTP.responseData = receivedText; //changing data within recieved response data

                    ForwardHandler.TCP_HTTP_Forward(receivedText); // forwarding data to HTTP host

                    Array.Clear(buff, 0, buff.Length);

                    //SENDING DATA FROM SERVER TO TCP CLIENT

                    //SendToAll();
                        
                    

                   

                }

            }
            catch (Exception e)
            {
                RemoveClient(paramClient);
                Console.WriteLine(e.ToString());
            }

        }
        public void SendToAll()
        {
            string leMessage = ForwardHandler.TCPResponse;
            if (string.IsNullOrEmpty(leMessage))
            {
                return;
            }

            try
            {
                byte[] buffMessage = Encoding.ASCII.GetBytes(leMessage);

                foreach (TcpClient c in mClients)
                {
                    c.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return;

        }

        private void RemoveClient(TcpClient paramClient)
        {
            if (mClients.Contains(paramClient))
            {
                mClients.Remove(paramClient);
                Console.WriteLine(String.Format("Client removed, count: {0}", mClients.Count));
            }
        }

       
        /*
       public void StopServer()
       {
           try
           {
               if (mTCPListener != null)
               {
                   mTCPListener.Stop();
               }

               foreach (TcpClient c in mClients)
               {
                   c.Close();
               }

               mClients.Clear();
           }
           catch (Exception excp)
           {

               Debug.WriteLine(excp.ToString());
           }
       }
       */
    }
}
