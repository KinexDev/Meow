# Meow
Meow is a simple language im making in C# as a side project written completely from scratch, i use a regex lexer and then generate an AST which i then walk through for execution, you can download the [meow playground](https://github.com/KinexDev/Meow/releases/tag/0.0.2) here.

example script.

```luau
add = function(a: number, b: number): number {
	return a + b;
}

result: number = add(5, 3);

print(result); // 8
```

without type hinting (type hints are not enforced)

```luau
add = function(a, b) {
	return a + b;
}

result = add(5, 3);

print(result); // 8
```

Generated AST

```json
{
  "Statements": [
    {
      "Identifier": "add",
      "Value": {
        "FunctionNodes": [
          {
            "ReturnValue": {
              "Expression": "+",
              "InBracket": false,
              "Left": {
                "Identifier": "a"
              },
              "Right": {
                "Identifier": "b"
              }
            }
          }
        ],
        "parameters": [
          "a",
          "b"
        ]
      }
    },
    {
      "Identifier": "result",
      "Value": {
        "Identifier": "add",
        "Arguments": [
          {
            "Literal": 5.0
          },
          {
            "Literal": 3.0
          }
        ]
      }
    },
    {
      "Identifier": "print",
      "Arguments": [
        {
          "Identifier": "result"
        }
      ]
    }
  ]
}
```

this code executes the same as previous one

# Syntax
the language is a semicolon based language, i have some error checking for semicolons so if you don't include them you may start getting logic errors.

variables are declared like `x = 2` or with type hinting `x:number = 2` these are all currently global, there is no local variables yet, the types are not enforced but are just there for clarity and documentation.

function calls via C# side `print("hello world!")` using delegate called `MeowFunction` for interacting with C# side, supports arguments in the form of `object[]`, it also has the script as context and a return type in the form `object`

functions are declared as `variable = function() {}`, the functions are treated as first class citizens.

The if-else statements and while statements work, you cannot chain elifs yet, there is no for statements yet but you could make that with a while loop and a variable.

it comes with some basic functions, `print`, `input`, `wait`, `int`, `number`, `string`, `if` and `type`

the `program` calls the function `update` every frame if it can find one. (this was for testing)

# Examples 

setting a function as a variable and calling it
```luau
betterPrint: function = print;
betterPrint("Hello world!");
```

calling an anonymous function
```luau
call = function(func: function): void {
	func();
}

// calls the anonymous function, prints hello world!
call(function() {
	print("hello world!");
});
```

returning a function
```luau
returnFunc = function(): function {
	return function() {
		print("hello world!");
    }
}

func: function = returnFunc();
func();  // prints hello world!
```

current if functions (bound to change)
```luau
if(true) {
	print("condition was true!");
} else {
	print("condition was false!");
}
```

this is an example script using some of the basic functions.
```luau
name: string = input("What is your name? ");
print("hello", name);
```

the AST for this simple program.

```json
{
  "Statements": [
    {
      "Identifier": "name",
      "Value": {
        "Identifier": "input",
        "Arguments": [
          {
            "String": "What is your name? "
          }
        ]
      }
    },
    {
      "Identifier": "print",
      "Arguments": [
        {
          "String": "hello"
        },
        {
          "Identifier": "name"
        }
      ]
    }
  ]
}
```

more complex programs that rely on type hinting everywhere.

```luau
add = function(a: number, b: number): number {
	return a + b;
}

print("the add game, can you get over the limit?");

limit: number = number(input("set the limit. "));

number1: number = number(input("give me the first number. "));
number2: number = number(input("give me the second number. "));

result: number = add(number(number1), number(number2));

if(result > limit) {
	print("you won");
} else {
	print("you lost.");
}

print("total sum you got was", result, "you got", result - limit, "greater than", limit);
```

simple program for showing if-elses and a while loop.
```luau
while (true) {
	x = input("what do you want to try? true, false or exit? ");
	if (x == "exit") {
		// exits the program returns a value for c# runner, intended behaviour
		return;
	}
	if (x == "true") {
		print("got it true");
	} else {
		print("its false");
	}
}
```

a custom application that loops how many times the user inputted
```lua
forLoop = function(to: number, loop: function): void {
	i = 0;
	while (i < int(to)) {
		loop(i);
		i = i + 1;
	}
}

howManyCount: string = input("how do you want it to count? ");

forLoop(int(howManyCount), function(i: number) {
	print(i + 1);
});
```


# Planned
I plan on making it a language similar to python, lua and javascript with hints of miniscript to create a scripting language.
Maybe i plan on generating bytecode in the distant future, but rn im walking through ASTs.

# Why
I am making this as a side project for fun and i love doing it

# Embedding
to embed meow in your C# scripts, you need to create a new `Script`, then you can register all the globals you want, so for example you can register a function call for printing and then you just run the script via `DoString`, it takes in 1 argument which is the script content, heres an example script, currently its not unity friendly because im using some of the new c# features but this will be fixed in my next commit.

```cs
public static object PrintMeow(object[] arguments)
{
    foreach (var arg in arguments)
    {
        Console.Write((arg is null ? "null" : arg.ToString()) + " ");
    }
    return null;
}

...
var script = new Script();
script.SetGlobal("print", (MeowFunction)PrintMeow);
script.DoString("print(\"Hello world!\");"); // prints `Hello world!` to the console, do string returns what is returned from the script.
```

To call a function for example lets say on unity update, you can do the following after you've initalized the script.

```cs
var update = script.GetGlobal("update");

if (update is Function func) // check if its a function
{
	func.Call(script); // you need to reference the script in the call.
}           
```