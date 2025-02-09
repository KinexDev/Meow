# Meow
Meow is a simple language (currently it isn't one) im making in C# as a side project written completely from scratch, i use a regex lexer and then generate an AST which i then walk through for execution.

# Features currently
number parsing `10 + 3 * 2` => 16

boolean parsing `true or false` -> true

string parsing `"hello" + 10` -> `"hello10"`, if you have a string in your expression the result will be string always

comparisons `1 == 2` -> `False`

unary operators `-10`, `not true` -> `False`

variables `x = 2` or with type hinting `x:number = 2` these are all currently global, there is no local variables yet. Currently the types are ignored so it doesn't throw an error if you do `x:asiduasiudn = 10`, this is an intentional design for the type hinting as it when parsing we do not know the types.

function calls `print("hello world!")` using delegate called `MeowDelegate` for interacting with C# side, supports arguments in the form of `object[]` and a return type in the form `object`

this system has first class functions so you could do the following : `x = print; x("hello world!");`

this is an example script.
```py
name = input("What is your name? ");
print("hello", name);
```

the language is a semicolon based language, i don't have any error checking for semicolons so if you don't include them you start gettting a bunch of logic errors.

it comes with some basic functions, `print`, `input`, `wait`, `int`, `number`, `string` and `type`

# Planned
I plan on making it a language similar to python, lua and javascript to create a scripting language.
Maybe i plan on generating bytecode in the distant future, but rn im walking through ASTs.

# Why
I am making this as a side project for fun and i love doing it

# Embedding
to embed meow in your C# scripts, you need to create a new `Script`, then you can register all the globals you want, so for example you can register a function call for printing and then you just run the script via `DoString`, it takes in 1 argument which is the script content, heres an example script, currently its not unity friendly because im using some of the new c# features but this will be fixed in my next commit.

```cs
public static object PrintMeow(object[] arguments)
{
    var concatenatedString = string.Empty;
    foreach (var arg in arguments)
    {
        if (arg is null)
            concatenatedString += "null" + " ";   
        else
            concatenatedString += arg + " ";
    }
    Console.Write(concatenatedString);
    return null;
}

...
var script = new Script();
script.SetGlobal("print", (MeowDelegate)PrintMeow);
script.DoString("print(\"Hello world!\");"); // prints `Hello world!` to the console.
```