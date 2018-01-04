// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;

namespace FirstFloor.ModernUI.Windows.Controls.BBCode
{
    internal class CharBuffer
    {
        private readonly string value;
        private int mark;
        private int position;

        public CharBuffer(string value)
        {
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.value = value;
        }

        public char LA(int count)
        {
            var index = position + count - 1;
            if(index < value.Length)
            {
                return value[index];
            }

            return char.MaxValue;
        }

        public void Mark()
        {
            mark = position;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetMark()
        {
            if(mark < position)
            {
                return value.Substring(mark, position - mark);
            }
            return string.Empty;
        }

        public void Consume()
        {
            position++;
        }
    }
}