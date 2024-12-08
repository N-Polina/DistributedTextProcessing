using MPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Инициализация MPI
        using (new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;
            int rank = comm.Rank;
            int size = comm.Size;

            // Узел-координатор читает файл
            List<Point3D> points = null;
            if (rank == 0)
            {
                string filePath = args[0];
                points = ReadPointsFromFile(filePath);
            }

            // Распределяем данные между узлами
            comm.Broadcast(ref points, 0);

            // Вычисляем площади параллелограммов
            var results = CalculateParallelograms(points, rank, size);

            // Собираем результаты на главном узле
            var allResults = comm.Gather(results, 0);

            if (rank == 0)
            {
                // Объединяем и форматируем результаты
                var finalResults = allResults.SelectMany(r => r).ToList();

                // Сохраняем результат в файл
                string resultFile = "result.txt";
                File.WriteAllLines(resultFile, finalResults);

                // Печатаем результат
                PrintFormattedResults(points, finalResults);
            }
        }
    }

    static List<Point3D> ReadPointsFromFile(string filePath)
    {
        return File.ReadLines(filePath)
                   .Select(line => line.Split(' '))
                   .Select(parts => new Point3D(
                       double.Parse(parts[0]),
                       double.Parse(parts[1]),
                       double.Parse(parts[2])))
                   .ToList();
    }

    static List<string> CalculateParallelograms(List<Point3D> points, int rank, int size)
    {
        var results = new List<string>();
        int start = rank * (points.Count / size);
        int end = (rank == size - 1) ? points.Count : start + (points.Count / size);

        for (int i = start; i < end; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                for (int k = j + 1; k < points.Count; k++)
                {
                    var (area, angle1, angle2) = CalculateParallelogramAreaAndAngles(points[i], points[j], points[k]);

                    // Формат результата
                    string result = $"Точки: {points[i]}, {points[j]}, {points[k]} | " +
                                    $"Углы: {angle1:F2}, {angle2:F2} | " +
                                    $"Площадь: {area:F2}";
                    results.Add(result);
                }
            }
        }

        return results;
    }

    static (double Area, double Angle1, double Angle2) CalculateParallelogramAreaAndAngles(Point3D p1, Point3D p2, Point3D p3)
    {
        var vector1 = new Point3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        var vector2 = new Point3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

        // Векторное произведение
        var crossProduct = new Point3D(
            vector1.Y * vector2.Z - vector1.Z * vector2.Y,
            vector1.Z * vector2.X - vector1.X * vector2.Z,
            vector1.X * vector2.Y - vector1.Y * vector2.X
        );

        // Площадь
        double area = Math.Sqrt(crossProduct.X * crossProduct.X +
                                crossProduct.Y * crossProduct.Y +
                                crossProduct.Z * crossProduct.Z);

        // Скалярное произведение
        double dotProduct = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;

        // Длины векторов
        double magnitude1 = Math.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y + vector1.Z * vector1.Z);
        double magnitude2 = Math.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y + vector2.Z * vector2.Z);

        // Углы
        double cosTheta = dotProduct / (magnitude1 * magnitude2);
        double angle1 = Math.Acos(cosTheta) * (180 / Math.PI); // Угол в градусах

        return (area, angle1, 180 - angle1);
    }

    static void PrintFormattedResults(List<Point3D> points, List<string> results)
    {
        Console.WriteLine("Точки:");
        foreach (var point in points)
        {
            Console.WriteLine($"{point.X:F2} {point.Y:F2} {point.Z:F2}");
        }

        Console.WriteLine("\nРезультаты вычислений:");
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
    }
}

class Point3D
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString() => $"{X:F2} {Y:F2} {Z:F2}";
}
