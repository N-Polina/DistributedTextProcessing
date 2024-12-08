namespace MyApp
{
    public class Calculator
    {
        public int Add(int a, int b) => a + b;

        public int Subtract(int a, int b) => a - b;

        public int Multiply(int a, int b) => a * b;

        public double Divide(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException("Деление на ноль недопустимо.");
            return (double)a / b;
        }

        public async Task<int> AddAsync(int a, int b)
        {
            await Task.Delay(50); // Симуляция асинхронной работы
            return a + b;
        }
    }
}
