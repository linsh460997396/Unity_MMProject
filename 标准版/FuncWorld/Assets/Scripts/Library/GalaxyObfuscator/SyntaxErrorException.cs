using System;
using System.Runtime.Serialization;

namespace GalaxyObfuscator
{
    internal class SyntaxErrorException : Exception
    {
        public SyntaxErrorException()
        {
        }

        public SyntaxErrorException(Scanner scanner) : base("Unexpected " + scanner.Current.Sequence.ToString())
        {
            Sequence sequence = scanner.Current.Sequence;
        }

        public SyntaxErrorException(string message) : base(message)
        {
        }

        protected SyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
