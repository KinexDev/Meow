using System;
using System.Collections.Generic;
using MeowLang.Internal.Parser.AST;
using MeowLang.Internal.Tokenizer;

namespace MeowLang.Internal.Parser
{
    public class Script
    {
        private Dictionary<string, object> Globals = new();
        private ProgramAST program;

        public void SetGlobal(string name, object value)
        {
            Globals[name] = value;
        }

        public object GetGlobal(string name)
        {
            if (Globals.TryGetValue(name, out object value))
                return value;
            return null;
        }

        public void LoadString(string script, bool printTokens = false)
        {
            Tokenizer.Tokenizer.FindTokens(script, out Token[] tokenList);

            if (printTokens)
            {
                foreach (var token in tokenList)
                {
                    Console.WriteLine($"{token.TokenType} : {token.Value}");
                }
            }

            program = Parser.Parse(tokenList);
        }

        public object DoString(string script)
        {
            LoadString(script);
            return program.Visit(this);
        }
    }
}