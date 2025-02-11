using System;
using System.Diagnostics;
using System.Threading;
using MeowLang.Internal.Parser;
using MeowLang.Internal.Parser.AST;
using MeowLang.Internal.Tokenizer;

namespace MeowLang
{
    class Program
    {
        public static object PrintMeow(object[] arguments, Script context)
        {
            Console.Write("[MEOW] : ");
            foreach (var arg in arguments)
            {
                Console.Write((arg is null ? "null" : arg.ToString()) + " ");
            }

            Console.Write("\n");
            return null;
        }

        public static object InputMeow(object[] arguments, Script context)
        {
            Console.Write("[MEOW] : ");
            if (arguments.Length > 0)
            {
                if (arguments[0] is null)
                    Console.Write("null");
                else
                    Console.Write(arguments[0]);
            }

            return Console.ReadLine();
        }

        public static object WaitMeow(object[] arguments, Script context)
        {
            if (arguments.Length > 0)
            {
                float time = (float)NumberMeow(new [] {arguments[0]}, context);
                time *= 1000;
                Thread.Sleep((int)time);
            }

            return null;
        }

        public static object IntMeow(object[] arguments, Script context)
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

        public static object NumberMeow(object[] arguments, Script context)
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

        public static object StringMeow(object[] arguments, Script context)
        {
            if (arguments.Length > 0)
            {
                return arguments[0].ToString();
            }

            return null;
        }

        public static object TypeMeow(object[] arguments, Script context)
        {
            if (arguments.Length > 0)
            {
                if (arguments[0] is null)
                    return "null";

                var type = arguments[0].GetType().Name.ToLower();

                if (type == "single")
                    return "number";
                else if (type == "meowfunction")
                    return "function";

                return type;
            }

            return null;
        }
        
        static void Main(string[] args)
        {
            var script = new Script();

            script.SetGlobal("print", (MeowFunction)PrintMeow);
            script.SetGlobal("input", (MeowFunction)InputMeow);
            script.SetGlobal("wait", (MeowFunction)WaitMeow);
            script.SetGlobal("int", (MeowFunction)IntMeow);
            script.SetGlobal("number", (MeowFunction)NumberMeow);
            script.SetGlobal("string", (MeowFunction)StringMeow);
            script.SetGlobal("type", (MeowFunction)TypeMeow);
            try
            {
                script.DoString(File.ReadAllText("Script.meow"));
            }
            catch (Exception e)
            {
                if (e is InterpreterException interpreterException)
                {
                    Console.WriteLine(interpreterException.FullMessage);
                }
                else
                {
                    Console.WriteLine(e.Message);
                }
                
                Console.ReadKey();
                return;
            }

            var update = script.GetGlobal("update");
            
            if (update is Function func)
            {
                while (true)
                {
                    try
                    {
                        func.Call(script);
                    }
                    catch (Exception e)
                    {
                        if (e is InterpreterException interpreterException)
                        {
                            Console.WriteLine(interpreterException.FullMessage);
                        }
                        else
                        {
                            Console.WriteLine(e.Message);
                        }
                
                        Console.ReadKey();
                        return;
                    }
                }
            }
            
            Console.ReadKey();
        }
    }
}