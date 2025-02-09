using System;

namespace MeowLang.Internal.Parser
{
    public class InterpreterException : Exception
    {
        public int Line { get; }
        public string Message { get; }
        public string FullMessage => $"Line {Line}: {Message}";

        public InterpreterException(int line, string message)
        {
            Line = line;
            Message = message;
        }
    }
}