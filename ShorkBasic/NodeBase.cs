﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkBasic
{
    abstract internal class NodeBase
    {
        internal Position startPosition { get; private set; }
        internal Position endPosition { get; private set; }

        protected NodeBase(Position startPosition, Position endPosition)
        {
            this.startPosition = startPosition.Copy();
            this.endPosition = endPosition.Copy();
        }
    }

    internal class NumberNode : NodeBase
    {
        public Token numToken { get; protected set; }

        public NumberNode(Token numToken)
            : base(numToken.startPosition, numToken.endPosition)
        {
            this.numToken = numToken;
        }
    }

    internal class BinaryOperationNode : NodeBase
    {
        public NodeBase leftNode { get; protected set; }
        public Token opToken { get; protected set; }
        public NodeBase rightNode { get; protected set; }

        public BinaryOperationNode(NodeBase leftNode, Token opToken, NodeBase rightNode)
            : base(leftNode.startPosition, rightNode.endPosition)
        {
            this.leftNode = leftNode;
            this.opToken = opToken;
            this.rightNode = rightNode;
        }
    }
}