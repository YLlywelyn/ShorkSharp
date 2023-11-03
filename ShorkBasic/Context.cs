﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkBasic
{
    public class Context
    {
        public string displayName { get; protected set; }
        public Context parent { get; protected set; }
        public SymbolTable symbolTable { get; protected set; }

        public Context(string displayName,Context parent = null, SymbolTable symbolTable = null)
        {
            this.displayName = displayName;
            this.parent = parent;
            this.symbolTable = (symbolTable != null) ? symbolTable : ((parent != null) ? parent.symbolTable : new SymbolTable());
        }
    }
}