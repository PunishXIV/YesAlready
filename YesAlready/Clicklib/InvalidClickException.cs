using System;

namespace Clicklib
{
    public class InvalidClickException : InvalidOperationException
    {
        public InvalidClickException(string message) : base(message) { }
    }
}
