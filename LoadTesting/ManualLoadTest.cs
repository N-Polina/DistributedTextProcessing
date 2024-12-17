using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadTests
{
    public static class ManualLoadTest
    {
        private static readonly string FilePath = GenerateFilePath("test_points_medium.txt");
        private const string ServerIP = "127.0.0.1";
        private const int ServerPort = 11000;
        private const int ClientCount = 100; // Количество параллельных клиентов

        private static int successfulRequests = 0;
        private static int failedRequests = 0;
        private static readonly object lockObj = new object();

        /// <summary>
        /// Запускает многопоточный нагрузочный тест.
        /// </summary>
        public static async Task RunTest()
        {
            Console.WriteLine("Запуск многопоточного нагрузочного теста...");

            // Проверяем и создаем файл при необходимости
            if (!File.Exists(FilePath))
            {
                Console.WriteLine("Файл не найден. Генерация файла...");
                GenerateTestFile(FilePath, 1000); // Создаем файл с 1000 точек
            }

            var tasks = new List<Task>();
            var stopwatch = Stopwatch.StartNew();

            // Запускаем клиентов в параллельных задачах
            for (int i = 0; i < ClientCount; i++)
            {
                tasks.Add(Task.Run(() => SendFile(FilePath, i)));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            GenerateReport(stopwatch.Elapsed);
        }

        /// <summary>
        /// Генерирует путь к файлу в рабочей директории.
        /// </summary>
        private static string GenerateFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, fileName);
        }

        /// <summary>
        /// Генерирует тестовый файл с точками.
        /// </summary>
        private static void GenerateTestFile(string filePath, int pointCount)
        {
            var random = new Random();
            var points = new List<string>();

            for (int i = 0; i < pointCount; i++)
            {
                points.Add($"{random.NextDouble() * 100} {random.NextDouble() * 100} {random.NextDouble() * 100}");
            }

            File.WriteAllLines(filePath, points);
            Console.WriteLine($"Файл успешно создан: {filePath}");
        }

        /// <summary>
        /// Отправляет файл на сервер по UDP.
        /// </summary>
        private static void SendFile(string filePath, int clientId)
        {
            UdpClient client = new UdpClient();
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                int chunkSize = 1024; // Размер одного пакета
                IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

                Console.WriteLine($"Клиент {clientId} отправляет файл...");

                for (int i = 0; i < fileData.Length; i += chunkSize)
                {
                    int size = Math.Min(chunkSize, fileData.Length - i);
                    byte[] chunk = new byte[size];
                    Array.Copy(fileData, i, chunk, 0, size);

                    client.Send(chunk, chunk.Length, serverEndpoint);

                    // Ожидаем подтверждение от сервера
                    client.Client.ReceiveTimeout = 3000;
                    try
                    {
                        byte[] response = client.Receive(ref serverEndpoint);
                        string result = Encoding.UTF8.GetString(response);

                        if (result != "OK")
                        {
                            Console.WriteLine($"Клиент {clientId}: Некорректный ответ от сервера.");
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine($"Клиент {clientId}: Тайм-аут при ожидании ответа. Повторная отправка...");
                        i -= chunkSize; // Повторная отправка пакета
                    }
                }

                lock (lockObj)
                {
                    successfulRequests++;
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    failedRequests++;
                }
                Console.WriteLine($"Клиент {clientId}: ошибка - {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        /// <summary>
        /// Генерирует отчет по результатам теста.
        /// </summary>
        private static void GenerateReport(TimeSpan totalTime)
        {
            string reportPath = Path.Combine(AppContext.BaseDirectory, "ManualLoadTestReport.txt");
            var report = new StringBuilder();

            report.AppendLine("=== Результат многопоточного теста ===");
            report.AppendLine($"Общее количество запросов: {ClientCount}");
            report.AppendLine($"Успешные запросы: {successfulRequests}");
            report.AppendLine($"Неудачные запросы: {failedRequests}");
            report.AppendLine($"Общее время выполнения: {totalTime.TotalSeconds:F2} секунд");
            report.AppendLine($"Среднее время на запрос: {(totalTime.TotalMilliseconds / ClientCount):F2} мс");

            Console.WriteLine(report.ToString());
            File.WriteAllText(reportPath, report.ToString());

            Console.WriteLine($"Отчет сохранен в файл: {reportPath}");
        }
    }
}
