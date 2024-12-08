using System;
using System.Threading.Tasks;
using Xunit;
using MyApp;

namespace MyApp.Tests
{
    public class CalculatorTests
    {
        [Fact]
        public void Add_ReturnsCorrectSum()
        {
            var calculator = new Calculator();
            var result = calculator.Add(2, 3);
            Assert.Equal(5, result);
        }

        [Theory]
        [InlineData(10, 5, 5)]
        [InlineData(0, 5, -5)]
        [InlineData(-5, -5, 0)]
        public void Subtract_ReturnsCorrectDifference(int a, int b, int expected)
        {
            var calculator = new Calculator();
            var result = calculator.Subtract(a, b);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2, 3, 6)]
        [InlineData(-2, 3, -6)]
        [InlineData(0, 5, 0)]
        public void Multiply_ReturnsCorrectProduct(int a, int b, int expected)
        {
            var calculator = new Calculator();
            var result = calculator.Multiply(a, b);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(10, 2, 5.0)]
        [InlineData(9, 3, 3.0)]
        [InlineData(-6, 2, -3.0)]
        public void Divide_ReturnsCorrectQuotient(int a, int b, double expected)
        {
            var calculator = new Calculator();
            var result = calculator.Divide(a, b);
            Assert.Equal(expected, result, precision: 5);
        }

        [Fact]
        public void Divide_ByZero_ThrowsException()
        {
            var calculator = new Calculator();
            Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
        }

        [Fact]
        public async Task AddAsync_ReturnsCorrectSum()
        {
            var calculator = new Calculator();
            var result = await calculator.AddAsync(5, 5);
            Assert.Equal(10, result);
        }

        [Fact]
        public async Task AddAsync_HandlesNegativeNumbers()
        {
            var calculator = new Calculator();
            var result = await calculator.AddAsync(-3, -7);
            Assert.Equal(-10, result);
        }
    }
}
