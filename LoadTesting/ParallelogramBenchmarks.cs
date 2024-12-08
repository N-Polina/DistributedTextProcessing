using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace LoadTests
{
    public class ParallelogramBenchmarks
    {
        private const string FilePath = "test_points.txt"; // Тестовый файл
        private const string ServerIP = "127.0.0.1";
        private const int ServerPort = 11000;

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random();
            var points = Enumerable.Range(0, 1000)
                .Select(_ => $"{random.NextDouble() * 100} {random.NextDouble() * 100} {random.NextDouble() * 100}")
                .ToList();
            File.WriteAllLines(FilePath, points);
        }

        [Benchmark]
        public void SendFileToServer()
        {
            UdpClient client = new UdpClient();
            try
            {
                byte[] fileData = File.ReadAllBytes(FilePath);
                client.Send(fileData, fileData.Length, ServerIP, ServerPort);

                IPEndPoint serverEndpoint = new IPEndPoint(System.Net.IPAddress.Any, 0);
                byte[] response = client.Receive(ref serverEndpoint);

                string result = Encoding.UTF8.GetString(response);
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Сервер вернул пустой ответ.");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка соединения: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<ParallelogramBenchmarks>();

         //   Console.ReadKey();
        }
    }
}
