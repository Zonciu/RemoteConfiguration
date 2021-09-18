using System;
using System.Collections.Generic;
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

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(
                new
                {
                    _testData.TestBasic
                });
        }

        [HttpGet("interval")]
        public IActionResult GetInterval()
        {
            _testData.TestInterval.A = Guid.NewGuid().ToString();
            _testData.TestInterval.B++;
            _testData.TestInterval.C = DateTime.Now;
            return Ok(
                new
                {
                    _testData.TestInterval
                });
        }
    }
}