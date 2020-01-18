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
            Thread t = new Thread(delegate ()
            {
                // replace the IP with your system IP Address...
                Server myserver = new Server("127.0.0.1", 8080);
            });
            t.Start();

            Console.WriteLine("Server Started...!");

            Thread b = new Thread(delegate ()
            {
                // replace the IP with your system IP Address...
                Server myserver = new Server("127.0.0.1", 8081);
            });
            b.Start();

            Console.WriteLine("Server Started...!");
        }


    }
}
