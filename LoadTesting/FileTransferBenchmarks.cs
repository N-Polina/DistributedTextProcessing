using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace LoadTests
{
    public class FileTransferBenchmarks
    {
        private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "test_points_medium.txt");
        private const string ServerIP = "127.0.0.1";
        private const int ServerPort = 11000;

        [GlobalSetup]
        public void Setup()
        {
            // Генерация тестового файла
            var random = new Random();
            var points = new string[1000];
            for (int i = 0; i < 1000; i++)
            {
                points[i] = $"{random.NextDouble() * 100} {random.NextDouble() * 100} {random.NextDouble() * 100}";
            }
            File.WriteAllLines(FilePath, points);
            Console.WriteLine($"Файл успешно создан: {FilePath}");
        }

        [Benchmark]
        public void SendFileToServer()
        {
            UdpClient client = new UdpClient();
            try
            {
                byte[] fileData = File.ReadAllBytes(FilePath);
                IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

                client.Send(fileData, fileData.Length, serverEndpoint);

                byte[] response = client.Receive(ref serverEndpoint);
                string result = Encoding.UTF8.GetString(response);

                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Сервер вернул пустой ответ.");
                }
            }
            finally
            {
                client.Close();
            }
        }
    }
}
