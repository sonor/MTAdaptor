﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

class Server
{
    TcpListener server = null;
    public Server(string ip, int port)
    {
        IPAddress localAddr = IPAddress.Parse(ip);
        server = new TcpListener(localAddr, port);
        server.Start();
        StartListener();
    }
    public void StartListener()
    {
        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = server.AcceptTcpClient();
               
                var cidEndppoint = (IPEndPoint)client.Client.LocalEndPoint;


                if (cidEndppoint.Port == 8080) 
                {
                    Console.WriteLine("8080 Connected!");
                    Thread t = new Thread(new ParameterizedThreadStart(HandleDeivce));
                    t.Start(client);
                }
                else if (cidEndppoint.Port == 8081)
                {
                    Console.WriteLine("8081 Connected!");
                    // Создаем поток
                    Thread Thread = new Thread(new ParameterizedThreadStart(ClientHandler));
                    // И запускаем этот поток, передавая ему принятого клиента
                    Thread.Start(client);
                }


            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
            server.Stop();
        }
    }
    public void HandleDeivce(Object obj)
    {
        string url = "http://localhost:9001";
        string request = "SKUNK WORKS IN DERIVCO WORLD!";
        string id = MTAdaptor.Hashing.SHA3_512(request);
        string buildedUrl = url + "/?" + "id=" + id;

        var values = new Dictionary<string, string>
            {

                { "body", request }
            };
        var content = new FormUrlEncodedContent(values);

        HttpClient client = new HttpClient();


        client.PostAsync(buildedUrl, content);

    //var responseString = response.Content.ReadAsStringAsync();



    /*
    TcpClient client = (TcpClient)obj;
    var stream = client.GetStream();
    string imei = String.Empty;
    string data = null;
    Byte[] bytes = new Byte[256];
    int i;
    try
    {
        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
        {
            string hex = BitConverter.ToString(bytes);
            data = Encoding.ASCII.GetString(bytes, 0, i);
            Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);
            string str = "Hey Device!";
            Byte[] reply = System.Text.Encoding.ASCII.GetBytes(str);
            stream.Write(reply, 0, reply.Length);
            Console.WriteLine("{1}: Sent: {0}", str, Thread.CurrentThread.ManagedThreadId);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine("Exception: {0}", e.ToString());
        client.Close();
    }
    */
}

    static void ClientHandler(Object StateInfo)
    {

        // Просто создаем новый экземпляр класса Client и передаем ему приведенный к классу TcpClient объект StateInfo
        new Client((TcpClient)StateInfo);

    }

  
       
}


class Client
{

    // Отправка страницы с ошибкой
    private void SendError(TcpClient Client, int Code)
    {
        // Получаем строку вида "200 OK"
        // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
        string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
        // Код простой HTML-странички
        string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
        // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
        string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
        // Приведем строку к виду массива байт
        byte[] Buffer = Encoding.ASCII.GetBytes(Str);
        // Отправим его клиенту
        Client.GetStream().Write(Buffer, 0, Buffer.Length);
        // Закроем соединение
        Client.Close();
    }

    // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
    public Client(TcpClient Client)
    {
        Console.WriteLine($"HTTP LISTENER STARTED ON:");
        // Объявим строку, в которой будет хранится запрос клиента
        string Request = "";
        // Буфер для хранения принятых от клиента данных
        byte[] Buffer = new byte[1024];
        // Переменная для хранения количества байт, принятых от клиента
        int Count;
        // Читаем из потока клиента до тех пор, пока от него поступают данные
        while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
        {
            // Преобразуем эти данные в строку и добавим ее к переменной Request
            Request += Encoding.ASCII.GetString(Buffer, 0, Count);
            // Запрос должен обрываться последовательностью \r\n\r\n
            // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
            // Нам не нужно получать данные из POST-запроса (и т. п.), а обычный запрос
            // по идее не должен быть больше 4 килобайт
            if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
            {
                break;
            }
        }

        // Парсим строку запроса с использованием регулярных выражений
        // При этом отсекаем все переменные GET-запроса
        Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

        // Если запрос не удался
        if (ReqMatch == Match.Empty)
        {
            // Передаем клиенту ошибку 400 - неверный запрос
            SendError(Client, 400);
            return;
        }

        // Получаем строку запроса
        string RequestUri = ReqMatch.Groups[1].Value;
        // Приводим ее к изначальному виду, преобразуя экранированные символы
        // Например, "%20" -> " "
        RequestUri = Uri.UnescapeDataString(RequestUri);

        // Если в строке содержится двоеточие, передадим ошибку 400
        // Это нужно для защиты от URL типа http://example.com/../../file.txt
        if (RequestUri.IndexOf("..") >= 0)
        {
            SendError(Client, 400);
            return;
        }

        // Если строка запроса оканчивается на "/", то добавим к ней index.html
        if (RequestUri.EndsWith("/"))
        {
            RequestUri += "index.html";
        }

        string FilePath = "www/" + RequestUri;

        // Если в папке www не существует данного файла, посылаем ошибку 404
        if (!File.Exists(FilePath))
        {
            SendError(Client, 404);
            return;
        }

        // Получаем расширение файла из строки запроса
        string Extension = RequestUri.Substring(RequestUri.LastIndexOf('.'));

        // Тип содержимого
        string ContentType = "";

        // Пытаемся определить тип содержимого по расширению файла
        switch (Extension)
        {
            case ".htm":
            case ".html":
                ContentType = "text/html";
                break;
            case ".css":
                ContentType = "text/stylesheet";
                break;
            case ".js":
                ContentType = "text/javascript";
                break;
            case ".jpg":
                ContentType = "image/jpeg";
                break;
            case ".jpeg":
            case ".png":
            case ".gif":
                ContentType = "image/" + Extension.Substring(1);
                break;
            default:
                if (Extension.Length > 1)
                {
                    ContentType = "application/" + Extension.Substring(1);
                }
                else
                {
                    ContentType = "application/unknown";
                }
                break;
        }

        // Открываем файл, страхуясь на случай ошибки
        FileStream FS;
        try
        {
            FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception)
        {
            // Если случилась ошибка, посылаем клиенту ошибку 500
            SendError(Client, 500);
            return;
        }

        // Посылаем заголовки
        string Headers = "HTTP/1.1 200 OK\nContent-Type: " + ContentType + "\nContent-Length: " + FS.Length + "\n\n";
        byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Headers);
        Client.GetStream().Write(HeadersBuffer, 0, HeadersBuffer.Length);

        // Пока не достигнут конец файла
        while (FS.Position < FS.Length)
        {
            // Читаем данные из файла
            Count = FS.Read(Buffer, 0, Buffer.Length);
            // И передаем их клиенту
            Client.GetStream().Write(Buffer, 0, Count);
        }

        // Закроем файл и соединение
        FS.Close();
        Client.Close();
    }
}