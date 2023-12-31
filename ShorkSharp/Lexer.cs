namespace ShorkSharp
{
    /// <summary>
    /// The lexer takes in the input text and converts it into a series of tokens.
    /// </summary>
    public class Lexer
    {
        /// <summary>
        /// The words recognised as keywords.
        /// </summary>
        static readonly string[] KEYWORDS =
        {
            "var",
            "and",
            "or",
            "not",
            "if",
            "then",
            "elif",
            "else",
            "for",
            "to",
            "step",
            "func",
            "while",
            "do",
            "end",
            "return",
            "continue",
            "break"
        };
        static readonly char[] WHITESPACE = { ' ', '\t', '\r' };
        static readonly char[] DIGITS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static readonly char[] DIGITS_WITH_DOT = DIGITS.Concat(new char[] { '.' }).ToArray();
        static readonly char[] LETTERS = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        static readonly char[] LETTERS_WITH_UNDERSCORE = LETTERS.Concat(new char[] { '_' }).ToArray();

        public Position position { get; protected set; }
        public string input { get; protected set; }
        public char currentChar { get; protected set; } = '\0';

        public Lexer(string input)
        {
            this.input = input;
            this.position = new Position(input);
        }
        public Lexer(string input, string filename)
        {
            this.input = input;
            this.position = new Position(filename);
        }

        void Advance()
        {
            position.Advance(currentChar);

            if (position.index < input.Length)
                currentChar = input[position.index];
            else
                currentChar = '\0';
        }

        /// <summary>
        /// Runs the lexer and returns the result.
        /// </summary>
        /// <returns>If an error occured, Token[] will be null and ShorkError will contain the error.  Otherwise Token[] will contain the tokens and ShorkError will be null.</returns>
        public (Token[], ShorkError?) Lex()
        {
            if (input.Length == 0)
                return (new Token[] { }, new ShorkError("Empty Input", "Input text is empty", null));
            this.currentChar = input[0];
            
            List<Token> tokens = new List<Token>();

            while (currentChar != '\0')
            {
                if (WHITESPACE.Contains(currentChar))
                {
                    Advance();
                }

                // Number Tokens
                else if (DIGITS.Contains(currentChar))
                {
                    tokens.Add(MakeNumberToken());
                }

                // String Tokens
                else if (currentChar == '"')
                {
                    (Token token, ShorkError error) = MakeStringToken();
                    if (error != null)
                        return (null, error);
                    tokens.Add(token);
                }
                
                // Identifiers and Keywords
                else if (LETTERS.Contains(currentChar))
                {
                    tokens.Add(MakeIdentifierToken());
                }

                // Simple tokens
                else
                {
                    switch (currentChar)
                    {
                        default:
                            return (new Token[] { },
                                    new InvalidCharacterError(string.Format("'{0}'", currentChar), position));
                        case '+':
                            tokens.Add(new Token(TokenType.PLUS, position));
                            Advance();
                            break;
                        case '-':
                            TokenType ttype = TokenType.MINUS;
                            Position startPosition = position.Copy();
                            Advance();

                            if (currentChar == '>')
                            {
                                ttype = TokenType.ARROW;
                                Advance();
                            }

                            tokens.Add(new Token(ttype, startPosition, position));
                            break;
                        case '*':
                            tokens.Add(new Token(TokenType.MULTIPLY, position));
                            Advance();
                            break;
                        case '/':
                            tokens.Add(new Token(TokenType.DIVIDE, position));
                            Advance();
                            break;
                        case '^':
                            tokens.Add(new Token(TokenType.EXPONENT, position));
                            Advance();
                            break;

                        case '!':
                            (Token token, ShorkError error) = MakeNotEqualsToken();
                            if (error != null) return (null, error);
                            tokens.Add(token);
                            break;
                        case '=':
                            tokens.Add(MakeEqualsToken());
                            break;
                        case '<':
                            tokens.Add(MakeLessThanToken());
                            break;
                        case '>':
                            tokens.Add(MakeGreaterThanToken());
                            break;

                        case '.':
                            tokens.Add(new Token(TokenType.DOT, position));
                            Advance();
                            break;
                        case ',':
                            tokens.Add(new Token(TokenType.COMMA, position));
                            Advance();
                            break;

                        case '(':
                            tokens.Add(new Token(TokenType.LPAREN, position));
                            Advance();
                            break;
                        case ')':
                            tokens.Add(new Token(TokenType.RPAREN, position));
                            Advance();
                            break;
                        case '{':
                            tokens.Add(new Token(TokenType.LBRACE, position));
                            Advance();
                            break;
                        case '}':
                            tokens.Add(new Token(TokenType.RBRACE, position));
                            Advance();
                            break;
                        case '[':
                            tokens.Add(new Token(TokenType.LBRACKET, position));
                            Advance();
                            break;
                        case ']':
                            tokens.Add(new Token(TokenType.RBRACKET, position));
                            Advance();
                            break;
                    }
                }
            }

            return (tokens.ToArray(), null);
        }

        Token MakeNumberToken()
        {
            string numstring = string.Empty + currentChar;
            bool hasDecimalPoint = false;
            Position startPosition = position.Copy();
            
            Advance();
            while (DIGITS_WITH_DOT.Contains(currentChar))
            {
                if (currentChar == '.')
                {
                    if (hasDecimalPoint)
                        break;
                    else
                        hasDecimalPoint = true;
                }
                numstring += currentChar;
                Advance();
            }

            return new Token(TokenType.NUMBER, decimal.Parse(numstring), startPosition, position);
        }

        (Token, ShorkError) MakeStringToken()
        {
            Position startPosition = position.Copy();
            string str = string.Empty;
            Advance();

            bool escaping = false;
            while (true)
            {
                if (escaping)
                {
                    switch (currentChar)
                    {
                        default:
                            return (null, new InvalidEscapeSequenceError(string.Format("\\{0}", currentChar), position));
                        case '"':
                            str += '"';
                            break;
                        case '\\':
                            str += '\\';
                            break;
                        case 't':
                            str += '\t';
                            break;
                    }
                    escaping = false;
                }

                else if (currentChar == '"')
                {
                    Advance();
                    break;
                }

                else if (currentChar == '\\')
                    escaping = true;

                else
                    str += currentChar;

                Advance();
            }

            return (new Token(TokenType.STRING, str, startPosition, position), null);
        }
        
        Token MakeIdentifierToken()
        {
            Position startPosition = position.Copy();
            string idstr = string.Empty + currentChar;
            Advance();
            
            while (LETTERS_WITH_UNDERSCORE.Contains(currentChar))
            {
                idstr += currentChar;
                Advance();
            }
            
            if (idstr == "true")
                return new Token(TokenType.BOOL, true, startPosition, position);
            else if (idstr == "false")
                return new Token(TokenType.BOOL, false, startPosition, position);
            else if (idstr == "null")
                return new Token(TokenType.NULL, startPosition, position);
            else
            {
                TokenType ttype = KEYWORDS.Contains(idstr.ToLower()) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
                return new Token(ttype, idstr, startPosition, position);
            }
        }

        Token MakeEqualsToken()
        {
            Position startPosition = position.Copy();
            TokenType ttype = TokenType.EQUALS;
            Advance();
            if (currentChar == '=')
            {
                ttype = TokenType.DOUBLE_EQUALS;
                Advance();
            }
            return new Token(ttype, startPosition, position);
        }

        (Token, ShorkError) MakeNotEqualsToken()
        {
            Position startPosition = position.Copy();
            Advance();
            if (currentChar == '=')
            {
                Advance();
                return (new Token(TokenType.NOT_EQUALS, startPosition, position), null);
            }
            return (null, new InvalidCharacterError("", position));
        }

        Token MakeLessThanToken()
        {
            Position startPosition = position.Copy();
            TokenType ttype = TokenType.LESS_THAN;
            Advance();
            if (currentChar == '=')
            {
                ttype = TokenType.LESS_THAN_OR_EQUAL;
                Advance();
            }
            return new Token(ttype, startPosition, position);
        }

        Token MakeGreaterThanToken()
        {
            Position startPosition = position.Copy();
            TokenType ttype = TokenType.GREATER_THAN;
            Advance();
            if (currentChar == '=')
            {
                ttype = TokenType.GREATER_THAN_OR_EQUAL;
                Advance();
            }
            return new Token(ttype, startPosition, position);
        }
    }
}
