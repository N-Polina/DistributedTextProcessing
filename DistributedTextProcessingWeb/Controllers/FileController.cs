using DistributedTextProcessingWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using MPI;

namespace DistributedTextProcessingWeb.Controllers
{
    [Route("[controller]")]
    public class FileController : Controller
    {
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, file.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Обрабатываем файл с использованием MPI
                var result = ProcessFileWithMPI(filePath);

                return View("Result", result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        private CalculationResult ProcessFileWithMPI(string filePath)
        {
            var lines = System.IO.File.ReadAllLines(filePath).ToList();
            var points = lines.Select(line => line.Split(' ')
                              .Select(double.Parse)
                              .ToArray()).ToList();

            var angles = new List<string>();
            var areas = new List<string>();

            // Создаем массив аргументов для MPI
            string[] mpiArgs = System.Environment.GetCommandLineArgs();

            using (new MPI.Environment(ref mpiArgs))
            {
                Intracommunicator comm = Communicator.world;
                int rank = comm.Rank;
                int size = comm.Size;

                int chunkSize = points.Count / size;
                int start = rank * chunkSize;
                int end = (rank == size - 1) ? points.Count : start + chunkSize;

                var localAreas = new List<string>();
                var localAngles = new List<string>();

                for (int i = start; i < end - 2; i++)
                {
                    for (int j = i + 1; j < points.Count - 1; j++)
                    {
                        for (int k = j + 1; k < points.Count; k++)
                        {
                            double area = CalculateParallelogramArea(points[i], points[j], points[k]);
                            localAreas.Add($"Точки: {FormatPoint(points[i])}, {FormatPoint(points[j])}, {FormatPoint(points[k])} | Площадь: {area:F2}");

                            double angle = CalculateAngle(points[i], points[j], points[k]);
                            localAngles.Add($"Точки: {FormatPoint(points[i])}, {FormatPoint(points[j])}, {FormatPoint(points[k])} | Угол: {angle:F2}°");
                        }
                    }
                }

                // Сбор результатов
                var allAreas = comm.Gather(localAreas, 0);
                var allAngles = comm.Gather(localAngles, 0);

                if (comm.Rank == 0)
                {
                    foreach (var areaList in allAreas)
                        areas.AddRange(areaList);

                    foreach (var angleList in allAngles)
                        angles.AddRange(angleList);
                }
            }

            return new CalculationResult
            {
                Points = points.Select(p => FormatPoint(p)).ToList(),
                Angles = angles,
                Areas = areas
            };
        }

        private double CalculateParallelogramArea(double[] p1, double[] p2, double[] p3)
        {
            double[] AB = { p2[0] - p1[0], p2[1] - p1[1], p2[2] - p1[2] };
            double[] AC = { p3[0] - p1[0], p3[1] - p1[1], p3[2] - p1[2] };

            double[] crossProduct = {
                AB[1] * AC[2] - AB[2] * AC[1],
                AB[2] * AC[0] - AB[0] * AC[2],
                AB[0] * AC[1] - AB[1] * AC[0]
            };

            return Math.Sqrt(crossProduct.Sum(c => c * c));
        }

        private double CalculateAngle(double[] p1, double[] p2, double[] p3)
        {
            double[] AB = { p2[0] - p1[0], p2[1] - p1[1], p2[2] - p1[2] };
            double[] AC = { p3[0] - p1[0], p3[1] - p1[1], p3[2] - p1[2] };

            double dotProduct = AB.Zip(AC, (a, b) => a * b).Sum();
            double magnitudeAB = Math.Sqrt(AB.Sum(a => a * a));
            double magnitudeAC = Math.Sqrt(AC.Sum(a => a * a));

            return Math.Acos(dotProduct / (magnitudeAB * magnitudeAC)) * (180.0 / Math.PI);
        }

        private string FormatPoint(double[] point)
        {
            return $"({point[0]}, {point[1]}, {point[2]})";
        }
    }
}
