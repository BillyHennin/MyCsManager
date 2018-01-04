// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace FirstFloor.ModernUI.Windows.Controls.BBCode
{
    internal class Token
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Token End = new Token(string.Empty, Lexer.TokenEnd);

        private readonly int tokenType;
        private readonly string value;

        public Token(string value, int tokenType)
        {
            this.value = value;
            this.tokenType = tokenType;
        }

        public string Value { get { return value; } }

        public int TokenType { get { return tokenType; } }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", tokenType, value);
        }
    }
}