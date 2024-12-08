namespace DistributedTextProcessingWeb.Models
{
    public class FileProcessingResult
    {
        public string FileName { get; set; } // Имя загруженного файла
        public string Status { get; set; }  // Статус обработки
        public string Results { get; set; } // Результаты обработки
    }
}
