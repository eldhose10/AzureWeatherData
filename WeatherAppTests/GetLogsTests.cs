using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq; 

namespace FetchWeatherData.Tests
{
    public class GetLogsTests
    {
        [Fact]
        public async Task GetLogs_ReturnsBadRequest_WhenInvalidDatesProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GetLog>>();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "start", "invalid-date" },
                { "end", "invalid-date" }
            });

            var context = new DefaultHttpContext();
            context.Request.Query = queryCollection;
            var request = context.Request;

            var getLogFunction = new GetLog();

            // Act
            var result = await getLogFunction.Run(request, loggerMock.Object) as BadRequestObjectResult;

            // Assert

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(result.Value, "Invalid date range");
        } 
    }
}
