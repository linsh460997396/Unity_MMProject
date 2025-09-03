using System;

namespace GalaxyObfuscator
{
    internal struct Sequence : IEquatable<Sequence>, IEquatable<string>
    {
        public Sequence(string str)
        {
            this.String = str;
            this.Start = 0;
            this.End = str.Length;
        }

        public Sequence(string str, int start, int end)
        {
            this.String = str;
            this.End = Math.Min(Math.Max(end, 0), str.Length);
            this.Start = Math.Min(Math.Max(start, 0), this.End);
        }

        public Sequence(Sequence seq, int start, int end)
        {
            this = new Sequence(seq.String, start, end);
        }

        public override string ToString()
        {
            return this.String.Substring(this.Start, this.End - this.Start);
        }

        public int Length
        {
            get
            {
                return this.End - this.Start;
            }
        }

        public char this[int index]
        {
            get
            {
                return this.String[this.Start + index];
            }
        }

        public bool Equals(Sequence seq)
        {
            if (this.Length != seq.Length)
            {
                return false;
            }
            for (int i = 0; i < this.Length; i++)
            {
                if (this[i] != seq[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool Equals(string str)
        {
            if (this.Length != str.Length)
            {
                return false;
            }
            for (int i = 0; i < this.Length; i++)
            {
                if (this[i] != str[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                return this.Equals((string)obj);
            }
            return obj is Sequence && this.Equals((Sequence)obj);
        }

        public override int GetHashCode()
        {
            int num = 0;
            for (int i = this.Start; i < this.End; i++)
            {
                num ^= this.String[i].GetHashCode();
            }
            return num;
        }

        public static bool operator ==(Sequence a, Sequence b)
        {
            return a.Equals(b);
        }

        public static bool operator ==(Sequence a, string b)
        {
            return a.Equals(b);
        }

        public static bool operator ==(string a, Sequence b)
        {
            return b.Equals(a);
        }

        public static bool operator !=(Sequence a, Sequence b)
        {
            return !a.Equals(b);
        }

        public static bool operator !=(Sequence a, string b)
        {
            return !a.Equals(b);
        }

        public static bool operator !=(string a, Sequence b)
        {
            return !b.Equals(a);
        }

        public string String;

        public int Start;

        public int End;
    }
}
