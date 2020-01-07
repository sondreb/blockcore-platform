using System;

namespace Blockcore.Platform.Networking.Exceptions
{
    public class PlatformException : Exception
    {
        public PlatformException(string? message) : base(message)
        {

        }

        public PlatformException(string? message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}
