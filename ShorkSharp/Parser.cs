﻿namespace ShorkSharp
{
    public class Parser
    {
        Token[] tokens;
        int tokenIndex = 0;
        Token currentToken;

        public Parser(Token[] tokens)
        {
            this.tokens = tokens;
            this.currentToken = this.tokens[0];
        }

        Token Advance()
        {
            tokenIndex++;
            currentToken = (tokenIndex < tokens.Length) ? this.tokens[tokenIndex] : null;
            return currentToken;
        }

        Token Reverse(int amount = 1)
        {
            tokenIndex -= amount;
            currentToken = (tokenIndex < tokens.Length) ? this.tokens[tokenIndex] : null;
            return currentToken;
        }

        public ParseResult Parse()
        {
            ParseResult result = ParseStatements();

            if (result.error != null && currentToken.type != TokenType.EOF)
                return result.Failure(new InvalidSyntaxError("Unexpected EOF", currentToken.startPosition));

            return result;
        }

        //##################################

        protected ParseResult ParseStatements()
        {
            ParseResult result = new ParseResult();
            List<NodeBase> statements = new List<NodeBase>();
            Position startPosition = currentToken.startPosition.Copy();

            while (currentToken.type != TokenType.NEWLINE)
            {
                result.RegisterAdvancement();
                Advance();
            }

            NodeBase statement = result.Register(ParseStatement());
            if (result.error != null)
                return result;
            statements.Add(statement);

            bool hasMoreStatements = true;
            while (true)
            {
                int newlineCount = 0;
                while (currentToken.type == TokenType.NEWLINE)
                {
                    result.RegisterAdvancement();
                    Advance();
                    newlineCount++;
                }
                if (newlineCount == 0)
                    hasMoreStatements = false;

                if (!hasMoreStatements)
                    break;

                statement = result.TryRegister(ParseStatement());
                if (statement == null)
                {
                    Reverse(result.toReverseCount);
                    hasMoreStatements = false;
                    continue;
                }
                statements.Add(statement);
            }

            return result.Success(new CodeBlockNode(statements, startPosition, currentToken.endPosition));
        }

        protected ParseResult ParseStatement()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseExpression()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseComparisonExpression()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseArithmaticExpression()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseTerm()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseFactor()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseExponent()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseCall()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseAtom()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseListExpression()
        {
            throw new NotImplementedException();
        }

        // TODO: ParseIfExpression

        protected ParseResult ParseStatement()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseForExpression()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseWhileExpression()
        {
            throw new NotImplementedException();
        }

        protected ParseResult ParseFunctionDefinition()
        {
            throw new NotImplementedException();
        }

        //######################################

        protected delegate ParseResult BinaryOperationDelegate();
        protected ParseResult ParseBinaryOperation(BinaryOperationDelegate leftFunc, TokenType[] operations)
        {
            return ParseBinaryOperation(leftFunc, operations, leftFunc);
        }
        protected ParseResult ParseBinaryOperation(BinaryOperationDelegate leftFunc, TokenType[] operations, BinaryOperationDelegate rightFunc)
        {
            ParseResult result = new ParseResult();

            NodeBase leftNode = result.Register(leftFunc());
            if (result.error != null)
                return result;

            while (operations.Contains(currentToken.type))
            {
                Token operatorToken = currentToken;
                result.RegisterAdvancement();
                Advance();

                NodeBase rightNode = result.Register(rightFunc());
                if (result.error != null)
                    return result;

                leftNode = new BinaryOperationNode(leftNode, operatorToken, rightNode);
            }

            return result.Success(leftNode);
        }
    }
}
