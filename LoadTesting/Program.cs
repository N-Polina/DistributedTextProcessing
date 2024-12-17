using BenchmarkDotNet.Running;
using System.Threading.Tasks;

namespace LoadTests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Запуск синхронного тестирования с BenchmarkDotNet ===");
            BenchmarkRunner.Run<FileTransferBenchmarks>();

            Console.WriteLine("\n=== Запуск многопоточного нагрузочного теста ===");
            await ManualLoadTest.RunTest();

            Console.WriteLine("Тесты завершены. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
