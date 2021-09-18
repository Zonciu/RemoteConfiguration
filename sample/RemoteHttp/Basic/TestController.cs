using System;
using Microsoft.AspNetCore.Mvc;

namespace RemoteConfiguration.Tests
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly TestData _testData;

        public TestController(TestData testData)
        {
            _testData = testData;
        }

        [HttpGet("interval")]
        public IActionResult GetInterval()
        {
            _testData.TestInterval.A = Guid.NewGuid().ToString();
            _testData.TestInterval.B++;
            _testData.TestInterval.C = DateTime.Now;
            return Ok(_testData.TestInterval);
        }
    }
}