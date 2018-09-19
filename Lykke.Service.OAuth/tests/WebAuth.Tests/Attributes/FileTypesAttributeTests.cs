using Microsoft.AspNetCore.Http;
using NSubstitute;
using WebAuth.Attributes;
using Xunit;

namespace WebAuth.Tests.Attributes
{
    public class FileTypesAttributeTests
    {
        [Fact]
        public void InValid_DocumentType_ThrowsValidationError()
        {
            //arrange
            var model = Substitute.For<IFormFile>();
            model.FileName.Returns("test.svg");
            var attribute = new FileTypesAttribute("jpg,png");

            //act
            var result = attribute.IsValid(model);

            //assert
            Assert.False(result);
        }

        [Fact]
        public void Valid_DocumentType_ThrowsNoValidationError()
        {
            //arrange
            var model = Substitute.For<IFormFile>();
            model.FileName.Returns("test.jpg");
            var attribute = new FileTypesAttribute("jpg,png");

            //act
            var result = attribute.IsValid(model);

            //assert
            Assert.True(result);
        }
    }
}