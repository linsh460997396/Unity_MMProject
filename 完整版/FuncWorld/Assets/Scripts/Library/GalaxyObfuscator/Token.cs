using System;
using System.Globalization;
using System.Text;

namespace GalaxyObfuscator
{
    internal struct Token
    {
        public override string ToString()
        {
            return this.Sequence.ToString();
        }

        public bool IsLiteral
        {
            get
            {
                return this.Type == TokenType.StringLiteral || this.Type == TokenType.CharLiteral || this.Type == TokenType.IntegerLiteral || this.Type == TokenType.RealLiteral || this.Type == TokenType.HexLiteral;
            }
        }

        public object ParseLiteral()
        {
            switch (this.Type)
            {
                case TokenType.StringLiteral:
                    return this.ParseStringLiteral();
                case TokenType.CharLiteral:
                    return this.ParseCharLiteral();
                case TokenType.IntegerLiteral:
                case TokenType.HexLiteral:
                    return this.ParseIntegerLiteral();
                case TokenType.RealLiteral:
                    return this.ParseRealLiteral();
            }
            return null;
        }

        public char ParseCharLiteral()
        {
            char c = this.Sequence[1];
            char c2 = c;
            if (c2 == '\'')
            {
                return '\0';
            }
            if (c2 != '\\')
            {
                return c;
            }
            return this.SpecialCharacter(this.Sequence[2]);
        }

        public string ParseStringLiteral()
        {
            StringBuilder stringBuilder = new StringBuilder(this.Sequence.Length - 2);
            for (int i = 1; i < this.Sequence.Length - 1; i++)
            {
                if (this.Sequence[i] != '\\')
                {
                    stringBuilder.Append(this.Sequence[i]);
                }
                else
                {
                    stringBuilder.Append(this.SpecialCharacter(this.Sequence[++i]));
                }
            }
            return stringBuilder.ToString();
        }

        public int ParseIntegerLiteral()
        {
            if (this.Type == TokenType.IntegerLiteral)
            {
                return int.Parse(this.Sequence.ToString());
            }
            return Convert.ToInt32(this.Sequence.ToString(), 16);
        }

        public double ParseRealLiteral()
        {
            return double.Parse(this.Sequence.ToString(), Token.Culture);
        }

        public char SpecialCharacter(char ch)
        {
            if (ch == 'n')
            {
                return '\n';
            }
            if (ch != 't')
            {
                return ch;
            }
            return '\t';
        }

        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-US");

        public Sequence Sequence;

        public TokenType Type;
    }
}
