using System;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace HeadRequest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TCP Client Started...");
            Console.WriteLine("=====================");
            Console.WriteLine("Start to type messages...");
            string requestMessage = Console.ReadLine();


            while (requestMessage != "q")
            {

                using var client = new TcpClient();

                var hostname = "127.0.0.1";
                client.Connect(hostname, 8080);

                using NetworkStream networkStream = client.GetStream();
                networkStream.ReadTimeout = 2000;

                using var writer = new StreamWriter(networkStream);

                using var reader = new StreamReader(networkStream, Encoding.UTF8);

                byte[] bytes = Encoding.UTF8.GetBytes(requestMessage);
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Close();
                Console.WriteLine("message sent");
                requestMessage = Console.ReadLine();
            }

           
        }
    }
}