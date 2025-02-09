# Meow
Meow is a simple language (currently it isn't one) im making in C# as a side project written completely from scratch, i use a regex lexer and then generate an AST which i then walk through for execution.

example script.

```ts
add:function = function(a: number, b: number): number {
	return a + b;
}

result: number = add(5, 3);

print(result); // 8
```

# Features currently
number parsing `10 + 3 * 2` => 16

boolean parsing `true or false` -> true

string parsing `"hello" + 10` -> `"hello10"`, if you have a string in your expression the result will be string always

comparisons `1 == 2` -> `False`

unary operators `-10`, `not true` -> `False`

variables `x = 2` or with type hinting `x:number = 2` these are all currently global, there is no local variables yet. Currently the types are ignored so it doesn't throw an error if you do `x:nonexistanttype = 10`, this is an intentional design for the type hinting as it when parsing we do not know the types.

function calls `print("hello world!")` using delegate called `MeowFunction` for interacting with C# side, supports arguments in the form of `object[]`, it also has the script as context and a return type in the form `object`

the language is a semicolon based language, i don't have any error checking for semicolons so if you don't include them you start gettting a bunch of logic errors.

functions, they are declared as `variable = function() {}`, the functions are treated as first class citizens, so you can do stuff like

```ts
betterPrint: function = print;
betterPrint("Hello world!");
```

```ts
call = function(func: function): void {
	func();
}

call(function() {
	print("hello world!");
}); // prints hello world!
```

```ts
returnFunction = function(): function {
    return function() {
        print("hello world!");
    }
}

func = returnFunction();
func();  // prints hello world!
```

The if statement currently is a function that takes in the condition, a true anonymous function and a false (i haven't gotten to writing if statements yet)

```ts
if(true, 
function() {
    // true
}, function() {
    // false
});
```

it comes with some basic functions, `print`, `input`, `wait`, `int`, `number`, `string`, `if` and `type`


this is an example script using some of the basic functions.
```ts
name = input("What is your name? ");
print("hello", name);
```

more complex programs that rely on type hinting everywhere.

```ts
add: function = function(a: number, b: number): number {
	return a + b;
}

print("the add game, can you get over the limit?");

limit: number = number(input("set the limit. "));

number1: number = number(input("give me the first number. "));
number2: number = number(input("give me the second number. "));

result: number = add(number(number1), number(number2));

if(result > limit, 
function() { // if
    print("you won");
}, 
function() { // else
    print("you lost.");
});

print("total sum you got was", result, "you got", result - limit, "greater than", limit);
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
script.DoString("print(\"Hello world!\");"); // prints `Hello world!` to the console.
```
