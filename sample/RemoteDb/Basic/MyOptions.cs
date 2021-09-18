using System;

namespace RemoteDb.Basic
{
    public class MyOptions
    {
        public string A { get; set; }

        public int B { get; set; }

        public DateTime C { get; set; }
    }

    public class MyOptionsModel
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}