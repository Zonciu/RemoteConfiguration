using System;

namespace RemoteConfiguration.Tests
{
    public class TestData
    {
        public TestOptions TestInterval = new()
        {
            A = "Abcd",
            B = 98765,
            C = DateTime.Now
        };
    }
}