using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

class Server
{
    static void Main(string[] args)
    {
        int port = 11000;
        UdpClient server = new UdpClient(port);
        Console.WriteLine($"Сервер запущен на порту {port}.");
        IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
        string tempFilePath = "temp_points.txt";

        try
        {
            using (FileStream fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                Console.WriteLine("Ожидание данных от клиента...");
                while (true)
                {
                    byte[] receivedData = server.Receive(ref clientEndpoint);
                    if (receivedData.Length == 0) break;
                    fs.Write(receivedData, 0, receivedData.Length);

                    // Отправляем подтверждение на каждую часть
                    string ack = "OK";
                    byte[] ackBytes = Encoding.UTF8.GetBytes(ack);
                    server.Send(ackBytes, ackBytes.Length, clientEndpoint);
                }
            }
            Console.WriteLine("Файл успешно получен.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            server.Close();
        }
    }
}
