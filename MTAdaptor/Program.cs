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
            int outcomingPort = 8081;
            

            Console.WriteLine("MTAdaptor Started...");
            Console.WriteLine("====================");
           

            Thread incomingTcp = new Thread(delegate ()
            {
                Server myserver = new Server(ipAddress, incomingPort);
            });

            incomingTcp.Start();


            Console.WriteLine($"Incoming TCP running on {ipAddress}:{incomingPort}");

            Thread outcomingTcp = new Thread(delegate ()
            {
                Server myserver = new Server(ipAddress, outcomingPort);
            });

            outcomingTcp.Start();

            Console.WriteLine($"Outcoming TCP running on {ipAddress}:{outcomingPort}");

            Console.WriteLine("Waiting for connections...");

        }


    }
}
