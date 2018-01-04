// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;

namespace FirstFloor.ModernUI.Windows.Controls.BBCode
{
    internal class TokenBuffer
    {
        private readonly List<Token> tokens = new List<Token>();
        private int position;

        public TokenBuffer(Lexer lexer)
        {
            if(lexer == null)
            {
                throw new ArgumentNullException("lexer");
            }

            Token token;
            do
            {
                token = lexer.NextToken();
                tokens.Add(token);
            }
            while(token.TokenType != Lexer.TokenEnd);
        }

        public Token LA(int count)
        {
            var index = position + count - 1;
            if(index < tokens.Count)
            {
                return tokens[index];
            }

            return Token.End;
        }

        public void Consume()
        {
            position++;
        }
    }
}