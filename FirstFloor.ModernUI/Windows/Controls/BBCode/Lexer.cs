// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FirstFloor.ModernUI.Windows.Controls.BBCode
{
    internal abstract class Lexer
    {
        public const int TokenEnd = int.MaxValue;

        private readonly CharBuffer buffer;
        private readonly Stack<int> states;

        protected Lexer(string value)
        {
            buffer = new CharBuffer(value);
            states = new Stack<int>();
        }

        protected abstract int DefaultState { get; }

        protected int State
        {
            get
            {
                if(states.Count > 0)
                {
                    return states.Peek();
                }
                return DefaultState;
            }
        }

        private static void ValidateOccurence(int count, int minOccurs, int maxOccurs)
        {
            if(count < minOccurs || count > maxOccurs)
            {
                throw new ParseException("Invalid number of characters");
            }
        }

        protected void PushState(int state)
        {
            states.Push(state);
        }

        protected int PopState()
        {
            return states.Pop();
        }

        protected char LA(int count)
        {
            return buffer.LA(count);
        }

        protected void Mark()
        {
            buffer.Mark();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected string GetMark()
        {
            return buffer.GetMark();
        }

        protected void Consume()
        {
            buffer.Consume();
        }

        protected bool IsInRange(char first, char last)
        {
            var la = LA(1);
            return la >= first && la <= last;
        }

        protected bool IsInRange(char[] value)
        {
            if(value == null)
            {
                return false;
            }
            var la = LA(1);
            foreach(var t in value)
            {
                if(la == t)
                {
                    return true;
                }
            }

            return false;
        }

        protected void Match(char value)
        {
            if(LA(1) == value)
            {
                Consume();
            }
            else
            {
                throw new ParseException("Character mismatch");
            }
        }

        protected void Match(char value, int minOccurs, int maxOccurs)
        {
            var i = 0;
            while(LA(1) == value)
            {
                Consume();
                i++;
            }
            ValidateOccurence(i, minOccurs, maxOccurs);
        }

        protected void Match(string value)
        {
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            foreach(var t in value)
            {
                if(LA(1) == t)
                {
                    Consume();
                }
                else
                {
                    throw new ParseException("String mismatch");
                }
            }
        }

        protected void MatchRange(char[] value)
        {
            if(IsInRange(value))
            {
                Consume();
            }
            else
            {
                throw new ParseException("Character mismatch");
            }
        }

        protected void MatchRange(char[] value, int minOccurs, int maxOccurs)
        {
            var i = 0;
            while(IsInRange(value))
            {
                Consume();
                i++;
            }
            ValidateOccurence(i, minOccurs, maxOccurs);
        }

        protected void MatchRange(char first, char last)
        {
            if(IsInRange(first, last))
            {
                Consume();
            }
            else
            {
                throw new ParseException("Character mismatch");
            }
        }

        protected void MatchRange(char first, char last, int minOccurs, int maxOccurs)
        {
            var i = 0;
            while(IsInRange(first, last))
            {
                Consume();
                i++;
            }
            ValidateOccurence(i, minOccurs, maxOccurs);
        }

        public abstract Token NextToken();
    }
}