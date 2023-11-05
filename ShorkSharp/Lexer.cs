﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp
{
    internal class Lexer
    {
        static readonly string[] KEYWORDS =
        {
            "var",
            "if",
            "else"
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

        public (Token[], ShorkError?) Lex()
        {
            if (input.Length == 0)
                return (new Token[]{}, new ShorkError("No input given", null));
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

                // Single character tokens
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
                            tokens.Add(new Token(TokenType.MINUS, position));
                            Advance();
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

        (Token, ShorkError) MakeNumberToken()
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
                        return (null, new InvalidSyntaxError("Number cannot have more than one decimal point", position))
                    else
                        hasDecimalPoint = true;
                }
                numstring += currentChar;
                Advance();
			}
			
			return (new Token(TokenType.NUMBER, decimal.Parse(numstring), startPosition, position), null)
        }
    }
}
