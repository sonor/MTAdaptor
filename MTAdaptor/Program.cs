using System;
using System.Net;
using System.Threading;

namespace MTAdaptor
{
    class Program
    {
        public static bool recordMode = true;

        static IPAddress ipAddress = IPAddress.Any; //default localhost ip
        static int incomingPort = 8080;
        static int outcomingPort = 8081;

        [STAThread]
        static void Main()
        {
            if (recordMode)
            {
                Console.WriteLine("MTAdaptor Started in Record mode...");
                Console.WriteLine("===================================\n");

                try
                {
                    //THREAD FOR INCOMING CONNECTIONS
                    Thread incomingTcp = new Thread(delegate ()
                    {
                        ServerTCP myserver = new ServerTCP(ipAddress, incomingPort, recordMode);
                    });

                    incomingTcp.Start();

                    //THREAD FOR HTTP HOST CONNECTIONS
                    Thread outcomingHTTP = new Thread(delegate ()
                    {
                        ServerHTTP myserver = new ServerHTTP(ipAddress, outcomingPort);
                    });

                    outcomingHTTP.Start();

                }
                catch (Exception e)
                {
                    Console.WriteLine("! System error: Cannot start server threads !");
                }

              

                while (true)
                {

                }

            }
            else 
            {
                Console.WriteLine("MTAdaptor Started in Playback mode...");
                Console.WriteLine("===================================\n");

                try
                {
                    //THREAD FOR INCOMING CONNECTIONS
                    Thread incomingTcp = new Thread(delegate ()
                {
                    ServerTCP myserver = new ServerTCP(ipAddress, incomingPort, recordMode);
                });

                    incomingTcp.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("! System error: Cannot start server threads !");
                }
                

                while (true)
                {

                }


            }
           
        }
    }
}
