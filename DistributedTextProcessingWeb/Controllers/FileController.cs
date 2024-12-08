using DistributedTextProcessingWeb.Models;
using Microsoft.AspNetCore.Mvc;


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

                // Обрабатываем файл и получаем результаты
                var result = ProcessFile(filePath);

                // Передаем данные в представление
                return View("Result", result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("Result")]
        public IActionResult Result()
        {
            return View();
        }

        /// <summary>
        /// Обрабатывает файл и возвращает результат вычислений.
        /// </summary>
        private CalculationResult ProcessFile(string filePath)
        {
            var lines = System.IO.File.ReadLines(filePath).ToList();

            // Преобразование строк в точки
            var points = lines.Select(line => line.Split(' '))
                              .Select(parts => $"{parts[0]} {parts[1]} {parts[2]}")
                              .ToList();

            var angles = new List<string>();
            var areas = new List<string>();

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    for (int k = j + 1; k < points.Count; k++)
                    {
                        // Пример данных, вместо настоящих вычислений
                        angles.Add($"Точки: {points[i]}, {points[j]}, {points[k]} | Угол1: 45° | Угол2: 135°");
                        areas.Add($"Точки: {points[i]}, {points[j]}, {points[k]} | Площадь: 120.5");
                    }
                }
            }

            return new CalculationResult
            {
                Points = points,
                Angles = angles,
                Areas = areas
            };
        }
    }
}
