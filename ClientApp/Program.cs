using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

class Client
{
    static void Main(string[] args)
    {
        string filePath = "points.txt";
        string serverIP = "127.0.0.1";
        int serverPort = 11000;

        UdpClient client = new UdpClient();
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не найден. Проверьте путь.");
                return;
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            int chunkSize = 1024;

            Console.WriteLine("Отправка файла...");
            for (int i = 0; i < fileData.Length; i += chunkSize)
            {
                int size = Math.Min(chunkSize, fileData.Length - i);
                byte[] chunk = new byte[size];
                Array.Copy(fileData, i, chunk, 0, size);

                client.Send(chunk, chunk.Length, serverIP, serverPort);
            }

            // Сигнал завершения передачи
            client.Send(new byte[0], 0, serverIP, serverPort);

            Console.WriteLine("Файл отправлен, ожидание ответа...");
            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] response = client.Receive(ref serverEndpoint);
            Console.WriteLine("Ответ от сервера: " + Encoding.UTF8.GetString(response));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}
