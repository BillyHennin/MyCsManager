// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using FirstFloor.ModernUI.Annotations;

namespace FirstFloor.ModernUI.Windows.Controls.BBCode
{
    internal class BbCodeParser : Parser<Span>
    {
        private const string TagBold = "b";
        private const string TagColor = "color";
        private const string TagItalic = "i";
        private const string TagSize = "size";
        private const string TagUnderline = "u";
        private const string TagUrl = "url";

        private readonly FrameworkElement _source;

        public BbCodeParser(string value, FrameworkElement source) : base(new BBCodeLexer(value))
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
            }
            _source = source;
        }

        public CommandDictionary Commands { get; set; }

        private void ParseTag(string tag, bool start, ParseContext context)
        {
            if(tag == TagBold)
            {
                context.FontWeight = null;
                if(start)
                {
                    context.FontWeight = FontWeights.Bold;
                }
            }
            else if(tag == TagColor)
            {
                if(start)
                {
                    var token = La(1);
                    if(token.TokenType == BBCodeLexer.TokenAttribute)
                    {
                        var color = (Color) ColorConverter.ConvertFromString(token.Value);
                        context.Foreground = new SolidColorBrush(color);

                        Consume();
                    }
                }
                else
                {
                    context.Foreground = null;
                }
            }
            else if(tag == TagItalic)
            {
                if(start)
                {
                    context.FontStyle = FontStyles.Italic;
                }
                else
                {
                    context.FontStyle = null;
                }
            }
            else if(tag == TagSize)
            {
                if(start)
                {
                    var token = La(1);
                    if(token.TokenType == BBCodeLexer.TokenAttribute)
                    {
                        context.FontSize = Convert.ToDouble(token.Value);

                        Consume();
                    }
                }
                else
                {
                    context.FontSize = null;
                }
            }
            else if(tag == TagUnderline)
            {
                context.TextDecorations = start ? TextDecorations.Underline : null;
            }
            else if(tag == TagUrl)
            {
                if(start)
                {
                    var token = La(1);
                    if(token.TokenType == BBCodeLexer.TokenAttribute)
                    {
                        context.NavigateUri = token.Value;
                        Consume();
                    }
                }
                else
                {
                    context.NavigateUri = null;
                }
            }
        }

        private void Parse(Span span)
        {
            var context = new ParseContext(span);

            while(true)
            {
                var token = La(1);
                Consume();

                if(token.TokenType == BBCodeLexer.TokenStartTag)
                {
                    ParseTag(token.Value, true, context);
                }
                else if(token.TokenType == BBCodeLexer.TokenEndTag)
                {
                    ParseTag(token.Value, false, context);
                }
                else if(token.TokenType == BBCodeLexer.TokenText)
                {
                    var parent = span;
                    if(context.NavigateUri != null)
                    {
                        var uriStr = context.NavigateUri;
                        string parameter = null;
                        string targetName = null;

                        var parts = uriStr.Split(new[] {'|'}, 3);
                        if(parts.Length == 3)
                        {
                            uriStr = parts[0];
                            parameter = Uri.UnescapeDataString(parts[1]);
                            targetName = Uri.UnescapeDataString(parts[2]);
                        }
                        else if(parts.Length == 2)
                        {
                            uriStr = parts[0];
                            parameter = Uri.UnescapeDataString(parts[1]);
                        }

                        var uri = new Uri(uriStr, UriKind.RelativeOrAbsolute);
                        var link = new Hyperlink();

                        ICommand command;
                        if(Commands != null && Commands.TryGetValue(uri, out command))
                        {
                            link.Command = command;
                            link.CommandParameter = parameter;
                            if(targetName != null)
                            {
                                link.CommandTarget = _source.FindName(targetName) as IInputElement;
                            }
                        }
                        else
                        {
                            link.NavigateUri = uri;
                            link.TargetName = parameter;
                        }
                        parent = link;
                        span.Inlines.Add(parent);
                    }
                    var run = context.CreateRun(token.Value);
                    parent.Inlines.Add(run);
                }
                else if(token.TokenType == BBCodeLexer.TokenLineBreak)
                {
                    span.Inlines.Add(new LineBreak());
                }
                else if(token.TokenType == BBCodeLexer.TokenAttribute)
                {
                    throw new ParseException(Resources.UnexpectedToken);
                }
                else if(token.TokenType == Lexer.TokenEnd)
                {
                    break;
                }
                else
                {
                    throw new ParseException(Resources.UnknownTokenType);
                }
            }
        }

        public override Span Parse()
        {
            var span = new Span();

            Parse(span);

            return span;
        }

        private class ParseContext
        {
            public ParseContext(Span parent)
            {
                Parent = parent;
            }

            private Span Parent { [UsedImplicitly] get; set; }
            public double? FontSize { private get; set; }
            public FontWeight? FontWeight { private get; set; }
            internal FontStyle? FontStyle { private get; set; }
            public Brush Foreground { private get; set; }
            public TextDecorationCollection TextDecorations { private get; set; }
            public string NavigateUri { get; set; }

            public Run CreateRun(string text)
            {
                var run = new Run {Text = text};
                if(FontSize.HasValue)
                {
                    run.FontSize = FontSize.Value;
                }
                if(FontWeight.HasValue)
                {
                    run.FontWeight = FontWeight.Value;
                }
                if(FontStyle.HasValue)
                {
                    run.FontStyle = FontStyle.Value;
                }
                if(Foreground != null)
                {
                    run.Foreground = Foreground;
                }
                run.TextDecorations = TextDecorations;

                return run;
            }
        }
    }
}