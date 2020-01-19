using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTAdaptor
{
    class ForwardHandler
    {
        public static string TCPResponse;
        static string httpUrl = "http://localhost:9001";

        public static bool TCP_HTTP_Forward(string request)
        {
          
            
            string id = MTAdaptor.Hashing.SHA3_512(request); //Get Hash from incoming TCP Data
            string buildedUrl = httpUrl + "/?" + "id=" + id;

            HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "body", request }
            };

            var content = new FormUrlEncodedContent(values);

            client.PostAsync(buildedUrl, content);
            Console.WriteLine($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")} /POST HTTP Forwarding => {httpUrl}");
            Console.WriteLine($"Data Hash: {id}");

            //PLAYBACK MODE
            if (!Program.recordMode)
            {
                GetHTTPResponseAsync(buildedUrl, content);
                


            }

            return true;
        }

        static async Task<string> GetHTTPResponseAsync(string urlEndPoint, FormUrlEncodedContent content)
        {
            HttpClient client = new HttpClient();

            string response = null;
            HttpResponseMessage getResponse = await client.PostAsync(urlEndPoint, content);
            if (getResponse.IsSuccessStatusCode)
            {
                response = await getResponse.Content.ReadAsStringAsync();
                if (response == "" || response == null)
                {
                    Console.WriteLine("! ERROR: HTTP 404 Bad request !");
                    Console.WriteLine($"===============================================\n");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString($"MM/dd/yyyy HH:mm:ss")} => Incoming HTTP Response from: {urlEndPoint}");

                    try
                    {
                        byte[] buffMessage = Encoding.ASCII.GetBytes(response);

                        foreach (TcpClient c in ServerTCP.mClients)
                        {
                            c.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                        }

                        Console.WriteLine($"{DateTime.Now.ToString($"MM/dd/yyyy HH:mm:ss")} Outcoming TCP data sent =>");
                        Console.WriteLine($"===============================================\n");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                }

            }

            return response;
        }
    }
}
