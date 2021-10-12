using System;

namespace NeoSmart.Unicode
{
    public class UnsupportedCodepointException : Exception
    {
        public UnsupportedCodepointException()
        {}

        public UnsupportedCodepointException(string message) : base(message)
        {}
    }

    public class InvalidRangeException : Exception
    { }

    public class InvalidEncodingException : Exception
    { }
}
