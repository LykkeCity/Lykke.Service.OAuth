using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace WebAuth.Tests.Url
{
    public class UrlBuilderTest
    {
        [Theory]
        [InlineData("http://test.com/", "http://test.com/?hello=world&some=text")]
        [InlineData("http://test.com/someaction", "http://test.com/someaction?hello=world&some=text")]
        [InlineData("http://test.com/someaction?q=1", "http://test.com/someaction?q=1&hello=world&some=text")]
        [InlineData("http://test.com/some#action", "http://test.com/some?hello=world&some=text#action")]
        [InlineData("http://test.com/some?q=1#action", "http://test.com/some?q=1&hello=world&some=text#action")]
        [InlineData(
             "http://test.com/someaction?q=test#anchor?value",
             "http://test.com/someaction?q=test&hello=world&some=text#anchor?value")]
        [InlineData("http://test.com/someaction#name#something",
             "http://test.com/someaction?hello=world&some=text#name#something")]
        public void Is_Url_With_Query_String_Builded_Correctly(string uri, string expectedUri)
        {
            //arrange
            var queryStrings = new Dictionary<string, string>
            {
                {"hello", "world"},
                {"some", "text"}
            };

            //act
            var result = QueryHelpers.AddQueryString(uri, queryStrings);

            //assert
            Assert.Equal(expectedUri, result);
        }
    }
}