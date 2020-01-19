using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MTAdaptor
{
    class Program
    {

        static void Main(string[] args)
        {
            string ipAddress = (IPAddress.Any).ToString();
            int incomingPort = 8080;
            int hostPort = 8079; // for playback messages
            int outcomingPort = 8081;

            if (Server.recordMode)
            {
                Console.WriteLine("MTAdaptor Started in Record mode...");
                Console.WriteLine("===================================\n");

                Thread incomingTcp = new Thread(delegate ()
                {
                    Server myserver = new Server(ipAddress, incomingPort);
                });

                incomingTcp.Start();
                Console.WriteLine($"Incoming TCP running on {ipAddress}:{incomingPort}");
            }
            else
            {
                Console.WriteLine("MTAdaptor Started in Playback mode...");
                Console.WriteLine("=====================================\n");

                Thread incomingTcp = new Thread(delegate ()
                {
                    Server myserver = new Server(ipAddress, incomingPort);
                });

                incomingTcp.Start();

                Thread hostTCP = new Thread(delegate ()
                {
                    Server myserver = new Server(ipAddress, hostPort);
                });
                hostTCP.Start();

                Console.WriteLine($"Incoming TCP running on {ipAddress}:{incomingPort} | {hostPort}");

            }
            Thread outcomingTcp = new Thread(delegate ()
            {
                Server myserver = new Server(ipAddress, outcomingPort);
            });

            outcomingTcp.Start();

            Console.WriteLine($"Outcoming TCP running on {ipAddress}:{outcomingPort}");

            Console.WriteLine("Waiting for job...");

        }


    }
}
