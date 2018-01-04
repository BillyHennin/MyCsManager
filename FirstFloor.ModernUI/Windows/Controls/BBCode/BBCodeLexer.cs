// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

namespace FirstFloor.ModernUI.Windows.Controls.BBCode
{
    internal class BBCodeLexer : Lexer
    {
        public const int TokenStartTag = 0;

        public const int TokenEndTag = 1;

        public const int TokenAttribute = 2;

        public const int TokenText = 3;

        public const int TokenLineBreak = 4;

        public const int StateNormal = 0;

        public const int StateTag = 1;
        private static readonly char[] QuoteChars = {'\'', '"'};
        private static readonly char[] WhitespaceChars = {' ', '\t'};
        private static readonly char[] NewlineChars = {'\r', '\n'};

        public BBCodeLexer(string value) : base(value) {}

        protected override int DefaultState { get { return StateNormal; } }

        private bool IsTagNameChar()
        {
            return IsInRange('A', 'Z') || IsInRange('a', 'z') || IsInRange(new[] {'*'});
        }

        private Token OpenTag()
        {
            Match('[');
            Mark();
            while(IsTagNameChar())
            {
                Consume();
            }

            return new Token(GetMark(), TokenStartTag);
        }

        private Token CloseTag()
        {
            Match('[');
            Match('/');

            Mark();
            while(IsTagNameChar())
            {
                Consume();
            }
            var token = new Token(GetMark(), TokenEndTag);
            Match(']');

            return token;
        }

        private Token Newline()
        {
            Match('\r', 0, 1);
            Match('\n');

            return new Token(string.Empty, TokenLineBreak);
        }

        private Token Text()
        {
            Mark();
            while(LA(1) != '[' && LA(1) != char.MaxValue && !IsInRange(NewlineChars))
            {
                Consume();
            }
            return new Token(GetMark(), TokenText);
        }

        private Token Attribute()
        {
            Match('=');
            while(IsInRange(WhitespaceChars))
            {
                Consume();
            }

            Token token;

            if(IsInRange(QuoteChars))
            {
                Consume();
                Mark();
                while(!IsInRange(QuoteChars))
                {
                    Consume();
                }
                token = new Token(GetMark(), TokenAttribute);
                Consume();
            }
            else
            {
                Mark();
                while(!IsInRange(WhitespaceChars) && LA(1) != ']')
                {
                    Consume();
                }

                token = new Token(GetMark(), TokenAttribute);
            }

            while(IsInRange(WhitespaceChars))
            {
                Consume();
            }
            return token;
        }

        public override Token NextToken()
        {
            if(LA(1) == char.MaxValue)
            {
                return Token.End;
            }

            if(State == StateNormal)
            {
                if(LA(1) == '[')
                {
                    if(LA(2) == '/')
                    {
                        return CloseTag();
                    }
                    var token = OpenTag();
                    PushState(StateTag);
                    return token;
                }
                if(IsInRange(NewlineChars))
                {
                    return Newline();
                }
                return Text();
            }
            if(State == StateTag)
            {
                if(LA(1) == ']')
                {
                    Consume();
                    PopState();
                    return NextToken();
                }

                return Attribute();
            }
            throw new ParseException("Invalid state");
        }
    }
}