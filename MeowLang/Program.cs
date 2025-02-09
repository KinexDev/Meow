using System;
using System.Threading;
using MeowLang.Internal.Parser;
using MeowLang.Internal.Parser.AST;
using MeowLang.Internal.Tokenizer;

namespace MeowLang
{
    class Program
    {
        public static object PrintMeow(object[] arguments)
        {
            foreach (var arg in arguments)
            {
                Console.Write((arg is null ? "null" : arg.ToString()) + " ");
            }

            Console.Write("\n");
            return null;
        }

        public static object InputMeow(object[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (arguments[0] is null)
                    Console.Write("null");
                else
                    Console.Write(arguments[0]);
            }

            return Console.ReadLine();
        }

        public static object WaitMeow(object[] arguments)
        {
            if (arguments.Length > 0)
            {
                float time = (float)NumberMeow(new [] {arguments[0]});
                time *= 1000;
                Thread.Sleep((int)time);
            }

            return null;
        }

        public static object IntMeow(object[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (arguments[0] is string)
                {
                    return MathF.Floor(float.Parse(arguments[0].ToString()));
                }
                else if (arguments[0] is int intValue)
                {
                    return MathF.Floor(intValue);
                }
                else if (arguments[0] is float floatValue)
                {
                    return MathF.Floor(floatValue);
                }
                else if (arguments[0] is double doubleValue)
                {
                    return MathF.Floor((float)doubleValue);
                }

                return Convert.ToInt32(arguments[0]);
            }

            return null;
        }

        public static object NumberMeow(object[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (arguments[0] is string)
                {
                    return float.Parse(arguments[0].ToString());
                }
                else if (arguments[0] is int)
                {
                    return (float)((int)arguments[0]);
                }
                else if (arguments[0] is float)
                {
                    return arguments[0];
                }
                else if (arguments[0] is double)
                {
                    return (float)((double)arguments[0]);
                }

                return Convert.ToSingle(arguments[0]);
            }

            return null;
        }

        public static object StringMeow(object[] arguments)
        {
            if (arguments.Length > 0)
            {
                return arguments[0].ToString();
            }

            return null;
        }

        public static object TypeMeow(object[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (arguments[0] is null)
                    return "null";

                var type = arguments[0].GetType().Name.ToLower();

                if (type == "single")
                    return "number";

                return type;
            }

            return null;
        }

        static void Main(string[] args)
        {
            bool disableRepl = false;
            //string? filePath = Directory.GetCurrentDirectory() + "/Test.meow";

            //Tokenizer.FindTokens(File.ReadAllText(filePath), out Token[] tokenList);
            //foreach (var token in tokenList)
            //{
            //    Console.WriteLine($"{token.TokenType} : {token.Value}");
            //}

            var script = new Script();

            script.SetGlobal("print", (MeowDelegate)PrintMeow);
            script.SetGlobal("input", (MeowDelegate)InputMeow);
            script.SetGlobal("wait", (MeowDelegate)WaitMeow);
            script.SetGlobal("int", (MeowDelegate)IntMeow);
            script.SetGlobal("number", (MeowDelegate)NumberMeow);
            script.SetGlobal("string", (MeowDelegate)StringMeow);
            script.SetGlobal("type", (MeowDelegate)TypeMeow);

            if (disableRepl)
            {
                //string? filePath = Directory.GetCurrentDirectory() + "/Script.meow";
                try
                {
                    string Source =
                        @"name = input(""What is your name? "");
                    print(""hello"", name);";
                    script.DoString(Source);
                    //script.DoString(File.ReadAllText(filePath));
                }
                catch (Exception e)
                {
                    if (e is InterpreterException interpreterException)
                    {
                        Console.WriteLine(interpreterException.FullMessage);
                    }

                    Console.WriteLine(e.Message);
                }
            }

            if (disableRepl)
                return;

            while (true)
            {
                Console.Write($"> ");
                string? Input = Console.ReadLine();

                try
                {
                    Console.Write($"-> ");
                    script.DoString(Input);
                    Console.Write("\n");
                }
                catch (Exception e)
                {
                    if (e is InterpreterException interpreterException)
                    {
                        Console.WriteLine(interpreterException.FullMessage);
                        continue;
                    }

                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}