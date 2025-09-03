using System.Runtime.Serialization;

namespace GalaxyObfuscator
{
    internal class UnexpectedEndOfFileException : SyntaxErrorException
    {
        public UnexpectedEndOfFileException() : base("Unexpected end of file")
        {
        }

        protected UnexpectedEndOfFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
