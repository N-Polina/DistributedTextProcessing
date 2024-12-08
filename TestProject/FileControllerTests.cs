using Microsoft.VisualStudio.TestTools.UnitTesting;
using DistributedTextProcessingWeb.Controllers;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Threading.Tasks;

namespace MyProject.Tests
{
    [TestClass]
    public class FileControllerTests
    {
        [TestMethod]
        public async Task UploadFile_ShouldReturnBadRequest_WhenFileIsNull()
        {
            // Arrange
            var controller = new FileController();
            IFormFile file = null;

            // Act
            var result = await controller.UploadFile(file);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task UploadFile_ShouldProcessFile_WhenFileIsValid()
        {
            // Arrange
            var controller = new FileController();
            var mockFile = new Mock<IFormFile>();

            var content = "Test file content";
            var fileName = "test.txt";
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);

            // Act
            var result = await controller.UploadFile(mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }
    }
}
