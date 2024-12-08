using System;
using System.Diagnostics;
using System.IO;

namespace DistributedTextProcessingWeb.Helpers
{
    public static class MPIHelper
    {
        /// <summary>
        /// Выполняет команду MPI и возвращает результат.
        /// </summary>
        /// <param name="command">Команда для выполнения.</param>
        /// <returns>Результат выполнения команды.</returns>
        public static string ExecuteMPI(string command)
        {
            try
            {
                // Создаем процесс для выполнения команды
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {command}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // Читаем вывод процесса
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Ошибка выполнения MPI: {error}");
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка MPI: {ex.Message}");
            }
        }
    }
}
