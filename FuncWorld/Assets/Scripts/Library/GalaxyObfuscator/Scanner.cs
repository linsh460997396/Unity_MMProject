using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyObfuscator
{
    internal class Scanner : IEnumerator<Token>, IDisposable, IEnumerator
    {
        public Scanner(string str)
        {
            this.token.Sequence = new Sequence(str, 0, 0);
            this.token.Type = TokenType.None;
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            this.token.Sequence.Start = (this.token.Sequence.End = 0);
            this.token.Type = TokenType.None;
        }

        public bool End
        {
            get
            {
                return this.position >= this.length;
            }
        }

        public Token Current
        {
            get
            {
                return this.token;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.token;
            }
        }

        public bool MoveNext()
        {
            if (this.End)
            {
                return false;
            }
            this.readToken();
            return true;
        }

        public Token Read()
        {
            this.readToken();
            return this.token;
        }

        public Token ReadExpectedToken()
        {
            if (!this.MoveNext())
            {
                throw new UnexpectedEndOfFileException();
            }
            return this.token;
        }

        public void ReadExpectedSymbol(string symbol)
        {
            this.Read();
            if (this.token.Type != TokenType.Symbol || this.token.Sequence != symbol)
            {
                throw new SyntaxErrorException("Expected " + symbol);
            }
        }

        public void ReadExpectedToken(TokenType type)
        {
            this.Read();
            if (this.token.Type != type)
            {
                throw new SyntaxErrorException("Unexpected " + this.token);
            }
        }

        public void SkipBlock(string terminate)
        {
            while (!this.End)
            {
                if (this.Read().Type == TokenType.Symbol && this.token.Sequence == terminate)
                {
                    return;
                }
            }
            throw new SyntaxErrorException("Missing " + terminate);
        }

        public void SkipBlock(string o, string e)
        {
            int num = 1;
            while (!this.End)
            {
                if (this.Read().Type == TokenType.Symbol)
                {
                    if (this.token.Sequence == o)
                    {
                        num++;
                    }
                    else if (this.token.Sequence == e && --num == 0)
                    {
                        return;
                    }
                }
            }
            throw new SyntaxErrorException("Missing " + e);
        }

        private int position
        {
            get
            {
                return this.token.Sequence.End;
            }
            set
            {
                this.token.Sequence.End = value;
            }
        }

        private int length
        {
            get
            {
                return this.token.Sequence.String.Length;
            }
        }

        private char get()
        {
            return this.token.Sequence.String[this.position];
        }

        private char read()
        {
            string @string = this.token.Sequence.String;
            int end;
            this.token.Sequence.End = (end = this.token.Sequence.End) + 1;
            return @string[end];
        }

        private bool forward()
        {
            return (this.token.Sequence.End = this.token.Sequence.End + 1) < this.length;
        }

        private void setNil()
        {
            this.token.Sequence.Start = this.token.Sequence.End;
            this.token.Type = TokenType.None;
        }

        private char this[int index]
        {
            get
            {
                return this.token.Sequence.String[index];
            }
        }

        private void readToken()
        {
            this.readWhite();
            if (this.End)
            {
                return;
            }
            char c = this.get();
            if (char.IsLetter(c) || c == '_')
            {
                this.readIdentifier();
                return;
            }
            if (char.IsDigit(c) || (c == '.' && this.position + 1 < this.length && char.IsDigit(this[this.position + 1])))
            {
                this.readNumberLiteral();
                return;
            }
            if (c == '\'' || c == '"')
            {
                this.readTextLiteral();
                return;
            }
            this.readSymbol();
        }

        private void readWhite()
        {
            while (!this.End)
            {
                if (char.IsWhiteSpace(this.get()))
                {
                    this.forward();
                }
                else
                {
                    if (this.get() != '/' || this.position + 1 >= this.length || (this[this.position + 1] != '/' && this[this.position + 1] != '*'))
                    {
                        return;
                    }
                    this.readComment();
                }
            }
            this.setNil();
        }

        private void readComment()
        {
            this.forward();
            if (this.read() == '*')
            {
                while (this.position < this.length - 1)
                {
                    if (this.read() == '*' && this.get() == '/')
                    {
                        this.forward();
                        return;
                    }
                }
                throw new SyntaxErrorException("End of file in comment");
            }
            while (!this.End)
            {
                char c = this.read();
                if (c == '\n')
                {
                    return;
                }
                if (c == '\\')
                {
                    if (!this.End)
                    {
                        this.forward();
                    }
                }
            }
        }

        private void readIdentifier()
        {
            this.token.Type = TokenType.Identifier;
            this.token.Sequence.Start = this.position;
            while (this.forward())
            {
                if (!char.IsLetterOrDigit(this.get()) && this.get() != '_')
                {
                    return;
                }
            }
        }

        private static bool IsHexDigit(char ch)
        {
            return char.IsDigit(ch) || "abcdefABCDEF".Contains(ch);
        }

        private void readNumberLiteral()
        {
            this.token.Sequence.Start = this.position;
            Func<char, bool> func;
            if (this.get() == '0' && this.position + 2 < this.length && this[this.position + 1] == 'x')
            {
                this.position += 2;
                this.token.Type = TokenType.HexLiteral;
                func = new Func<char, bool>(Scanner.IsHexDigit);
            }
            else
            {
                this.token.Type = TokenType.IntegerLiteral;
                func = new Func<char, bool>(char.IsDigit);
            }
            for (; ; )
            {
                if (this.get() == '.' && this.token.Type == TokenType.IntegerLiteral)
                {
                    this.token.Type = TokenType.RealLiteral;
                }
                else if (!func(this.get()))
                {
                    break;
                }
                if (!this.forward())
                {
                    return;
                }
            }
        }

        private void readTextLiteral()
        {
            this.token.Sequence.Start = this.position;
            char c = this.read();
            this.token.Type = ((c == '"') ? TokenType.StringLiteral : TokenType.CharLiteral);
            while (!this.End)
            {
                char c2 = this.read();
                if (c2 == c)
                {
                    return;
                }
                if (c2 == '\\' && !this.End)
                {
                    this.forward();
                }
                else if (c2 == '\n')
                {
                    throw new SyntaxErrorException("New line in constant");
                }
            }
            throw new UnexpectedEndOfFileException();
        }

        private void readSymbol()
        {
            this.token.Sequence.Start = this.position;
            this.token.Type = TokenType.Symbol;
            char c = this.read();
            if (!this.End)
            {
                char c2 = this.get();
                if (c2 <= '-')
                {
                    if (c2 != '&')
                    {
                        switch (c2)
                        {
                            case '+':
                            case '-':
                                break;
                            case ',':
                                return;
                            default:
                                return;
                        }
                    }
                }
                else
                {
                    switch (c2)
                    {
                        case '<':
                            break;
                        case '=':
                            if ("+-*/%&^|=!<>".Contains(c))
                            {
                                this.forward();
                                return;
                            }
                            return;
                        case '>':
                            if (c == '-')
                            {
                                this.forward();
                            }
                            break;
                        default:
                            if (c2 != '|')
                            {
                                return;
                            }
                            goto IL_8B;
                    }
                    if (this.get() == c && this.forward() && this.get() == '=')
                    {
                        this.forward();
                        return;
                    }
                    return;
                }
            IL_8B:
                if (this.get() == c)
                {
                    this.forward();
                    return;
                }
            }
        }

        private Token token;
    }
}
