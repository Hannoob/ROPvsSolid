# Writing testable code in a functional style

In this blog post we look at how functional and OOP code differ in the way that testable code is written.
We will also try to make an argument for why functional programming lends itself much better to writing, not only clean, testable code, but also writing [DRY](https://www.google.com) tests.

## Disclaimers
I am by no means an F# or a functional programming expert.
Most of the opinions discussed in this post is my own, and most of the wisdom contained within this post was blatantly stolen (and probably mis-understood) from people that are way smarter than I.

In this post, we will use F# and C# in order to demonstrate the examples, however, the idea is that the principals demonstrated should apply to any object oriented and functional language.

This is not meant to be an in-depth tutorial on ROP and SOLID design, or functional code in general.
It is aimed more at showcasing the readability benefits of writing code in a functional style when it comes to unit testing specifically.
However, it is also very likely that I start going down a rabbit hole when explaining some of the concepts, which is fine by me because planning out a well written post is way too much effort for me.

## A quick note about the assumptions made in this post.
We work from the assumnption that if code is not testable, it is not good code.
We assume that if code is testable, but not tested, it is not good code.
Finally, we assume that if tests are brittle or difficult to write, they will not be written or maintained.

This post does (poorly) explain some of the aspects of SOLID, BDD and ROP, however it would probably be better if you have some idea of what these things are before you start reading this. 

## A classic SOLID example, how to write good code.
In this post, we will not focus on any of the specific principals and their applications, but rather examine the structure and tests of a "typical" project designed according to these principals. (This is code for: "I don't quite understand every one of these principals but I feel like my code looks similar to the examples I have seen")

### 1. Typically external depenencies are and large pieces of logic are hiden behind interfaces.
In this example, we have a simple interface over a class that accesses some database.

```csharp
namespace SOLIDExample.Interfaces
{
    public interface IUserRepo
    {
        User GetUserDetails(string username);
    }
}
```
The exact implementation of this interface is not important, however it might be interesting to show that this specific implementation just returns a random object.
```csharp
namespace SOLIDExample.DataAccessLayer
{
    public class UserRepo : IUserRepo
    {
        private UserDataAccessObject GenerateUser()
        {
            return new UserDataAccessObject()
            {
                UserId = "",
                Email = "email@server.co.za",
                Username = "Username",
                Password = "Password"
            };
        }

        public User GetUserDetails(string username)
        {
            return DataAccessObjectFactory.create(GenerateUser());
        }
    }
}
```
This is the beauty of interfaces. It allows you to define the way of interacting with some resource, without bothering the calling code with all the implementation details, and also allows you to swap out implementations without having to change any of the calling code.
This is also the reason why having your code separated by interfaces is such an integral part of writing testable code; Because it allows you to write an implementation that suits the scenario you would like to test.
For example, you would prefer not to be bothered by an actual database when you are trying to test how your code will handle some error or edge when reading from the DB.

Typlically during testing, we use tools such as [Moq](https://www.google.com) in order to do thowaway implementations of interfaces for testing purposes.
```csharp
var fakeUser = new User()
            {
                UserId = "1",
                Username = "username",
                Password = "password",
                Email = "test@test.com"
            };

var userRepo = new Mock<IUserRepo>();
    userRepo
        .Setup(repo => repo.GetUserDetails("SomeUsername"))
        .Returns(fakeUser);
```

This application of interfaces has some obvious bennefits, but how do we actually allow our code to not care about the specific implementation of the interface?
Well, this leads us to the second principal that matters in designing good software...

### 2. External dependencies are injected into the constructors of classes.
The idea is to invert the dependencies of classes by adding dependencies of classes as parameters in the constructor of the class, rather than having to new up these dependencies in the business logic class itself.
(Disclaimer: There are other ways of handling dependencies that is not "constructor injection" however this is the most common and also my favorate method)
This allows whatever is newing up the business logic class to choose the implementations of the dependency interfaces.

```csharp
namespace SOLIDExample.BuisinessLogic
{
    public class UserHandler
    {
        private readonly IUserRepo _userRepo;
        
        public UserHandler(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public User AuthenticateUser(string username)
        {

            userDetails = _userRepo.GetUserDetails(username);
            return userDetails;
        }

    }
}
```

In practice, it is often some dependency injection framework that new up these classes, however, this pattern has the awesome advantage, in that it allows you to pass in your mock interface implementations into the business logic class when it it being set up for testing.
This is illustrated by this poor example of a test below.

```csharp
[Test]
public void This_is_just_an_example_test()
{
    var user = new User()
    {
        UserId = "1",
        Username = "username",
        Email = "test@test.com"
    };
    
    var userRepo = new Mock<IUserRepo>();
    userRepo
        .Setup(repo => repo.GetUserDetails(user.Username))
        .Returns(user);
        
    var handler = new UserHandler(userRepo.Object);
    
    response = handler.GetUser("username");
    
    response.Email.Should().Be("test@test.com");
}
```

### 3. BDD tests
Now that we know how to write good testable code, let us explore how to write good DRY tests.
There are many ways of testing software, and many different schools of thougth around what "good" tests are.
I personaly like the idea of Behaviour Driven Testing (BDT) or Behaviour Driven Development (BDT), where the test cases are focused around how the user, or other parts of the system will interact with the code being tested.
This method of testing has a couple of advantages over the traditional way of writing tests.
One of the main advantages that I would like to focus on, is the fact that these tests are writen in such a way that the structure of the test conveys information of what is being tested and what the expected behaviour should be.
This has the implication of the test acting as a sort of "living documentation" that will remain up to date and that anyone can understand.
These tests are also very easy to maintain, because of the fact that each test is focussed on a very specific piece of behaviour.

The main principal behind this form of testing is that it should follow a "Given,When,Then" structure.
The "Given" part explains the the state of the method being tested, "When" explains the action taken by the user, and the "Then" statements explain the expected behaviour of the method.

Below is an example of a fictional authentication method tested using this method:

```csharp
[TestFixture]
public class Given_a_valid_username_and_password_When_a_user_authenticates_and_the_db_is_available
{
    private User response;

    [OneTimeSetUp]
    public void setup()
    {
        var user = new User()
        {
            UserId = "1",
            Username = "username",
            Password = "password",
            Email = "test@test.com"
        };

        var logger = new Mock<ILogger<UserHandler>>();

        var userRepo = new Mock<IUserRepo>();
        userRepo
            .Setup(repo => repo.GetUserDetails(user.Username))
            .Returns(user);

        var passwordUtils = new Mock<IPasswordUtils>();
        passwordUtils
            .Setup(util => util.ComparePasswords("password", user.Password))
            .Returns(true);

        var emailService = new Mock<IEmailService>();
        emailService
            .Setup(service => service.EmailClient(user.Email, It.IsAny<string>()));

        var handler = new UserHandler(logger.Object, userRepo.Object, passwordUtils.Object, emailService.Object);

        response = handler.AuthenticateUser("username", "password");
    }

    [Test]
    public void Then_The_Response_Object_Should_Have_The_Correct_Username()
    {
        response.Username.Should().Be("username");
    }

    [Test]
    public void Then_The_Response_Object_Should_Have_The_Correct_Email()
    {
        response.Email.Should().Be("test@test.com");
    }

    [Test]
    public void Then_The_Response_Object_Should_Have_The_Correct_UserId()
    {
        response.UserId.Should().Be("1");
    }
}
```
## Throwing all that we know about "good code" out of the window.
Everything that we have talked about up to this point is basically what I think of when I hear the words "Clean Code". 
However, recently I started looking at large project written in F# which is a pretty cool functional .net language.
When looking at these projects, I realized that unfortunately, the developers that write much of these code bases, like me, also know how to write "good code".
This meant that we see a lot of the same design patterns from object oriented code in functional code bases.
It is also unfortunate that F# allows you to get away with using many of these patterns, which leads to a lot of weird looking functional code.
However, I came to realize, that the SOLID design principals, where really developed to deal with some of the issues specific to OOP, rather than just guidelines for good software.
Another way I started looking at it, was that the solid design principals are only a way to implement some set of deeper principals that make "good" code "good".
These deeper principals are not concerned with how you use your language paradigm in order to implement these principals.
Unfortunately, I am not smart enough to tell you exactly what these are, however, there are thoughts around this such as the principal of ["high cohesion, and low coupling"](https://stackoverflow.com/questions/3085285/difference-between-cohesion-and-coupling) as well as ideas focusing on [properties of good code](https://www.codementor.io/learn-development/what-makes-good-software-architecture-101) such as functionality, robustness, measurabilty, debuggability, maintainability, reusability, and extensibity.

I do not blame myself to harshly for conflating the two ideas of "good code" and SOLID design, as there realy aren't that many good resources and examples how to achieve these properties in a functional paradigm.
There is also the reality that when I heard that I had to throw all the nice SOLID habits, that I had spent so much time learning and advocating for, out of the window, I was resistant to say the least.
Another problem, was that there are lots of code snipets that demonstrate ways to implement many of these principals, however, there was nothing that I could look at and see the benefit on a large system with lots of complexity.

### Some functional programming basics

There are some people that do make a huge effort to show us mere mortals how to write good code in functional languages, and being an F# guy, I have to give credit to Scott Wlaschin's [blog](https://fsharpforfunandprofit.com), although I am sure there are many more.
If you are familiar with the basics of functional programming you are more than welcome to skip this section, but for the reader who would like to get a deeper understanding; this part is for you.
The first concept I would like to touch on, is an idea called "piping"
If you can get around the F# syntax, the following example shows one example of this idea.

```fsharp
//This is a function that takes an X and a Y and adds them together
let someMathFunction x = x + 1

//This is how it it usually called
let normalAnswer = someMathFunction 2

//Piping allows us to change the order of parameters. We take two, and push it through the function to get an answer
let pipedAnswer = 2 |> someMathFunction

//This is often read as "take 2, and pipe it through 'someMathFunction'"

//If we want to combine these functions we would traditionally do the following
let combinedWay = someMathFunction (someMathFunction 2)

//But piping allows us to make this a bit prettier
let pipedCominedWay = 2 |> someMathFunction |> someMathFunction
```

The last method can be seen as taking the value 2, using it as the input for the function "someMathFunction" and then taking the resulting value and using it as an input for the function "someMathFunction".
This gives you the freedom to structure your code in such a way that it is almost understandable by any non-programmer.

```fsharp
let addOne x = x + 1

let printAnswer x = printfn "%d" x

//Execute the code
2 
|> addOne 
|> addOne
|> printAnswer
```

These functions are very simple, and could be written in much shorter ways, however this does give you the freedom to structure your code in such a way that it would allow you to read the steps in the function almost as a paragraph:
"Take 2, Add one to it, add another one to that, and finally print the answer!"

F# also allows a thing called composition, where you get to glue functions together.
The only requirement is that the output of the first function, should be the same as the input of the second function:

```fsharp
let addOne x = x + 1

let printAnswer x = printfn "%d" x

let printAnswer = 
    >> addOne 
    >> addOne
    >> printAnswer

//execute the code
printAnswer 2
```

This code demonstrates how we were able to glue these functions together, to give us a new function that expects an integer as an input, and then we can use it whenever we are ready.

There is also a simple idea in FP that allows us to pass in some functions as parameters to other functions.
This idea is actually also quite prevalent in many modern OO languages, although the syntax is somethimes a bit of a haslte.
```fsharp
//Define a function that takes an integer and returns an integer
let someMathFunction x = x*x 

//Define a function that takes 1) a function that expects an integer and returns an integer, and 2) an integer
let printTheAnswerOfAMathFunction (mathFunction:int -> int) x 
    printfn "%d" (mathFunction x)

//Execute the code
printTheAnswerOfAMathFunction someMathFunction 2

//Execute the code using piping
2 |> (printTheAnswerOfAMathFunction someMathFunction)
```

There is also a concept called algabraic data types and monads which I am by far not clever enough to explain well, but here goes a quick introduction that should give you the basic idea.
For our examples, we will say that values in our software can be wrapped up in a type called a "Result".
A type is similar to a class in OO.
A value of Result type could be in an "OK" state, that contains some value, or it could be in an "ERROR" state that contains an error message.
Althogh the one state contains an int, and the other a string, they are still wrapped up in the same type.
Therefore, in order to work with the values in this type, we have to use a special type of "case" statement called a "match".

```fsharp
let happyValue = OK (1) //Take the value 1 and put it in an OK state
let errorValue = ERROR ("I am not happy about something") //Take an error message and put it in an unhappy state

match happyValue with
//If the type is in an OK state, take the int value out and let us use it for something
| OK ourUnwrappedVariable -> 
    printfn "%d" (ourUnwrappedVariable * 2)

//If the type is in an ERROR state, take the errorMessage string out and let us use it for something else
| ERROR ourErrorMessage -> 
    printfn ourErrorMessage
```

If you got this last part, then you are sorted for functional programming, as this is probably as complicated as it gets!
Admittedly, there are one or two other concepts that we could talk about, but really this is all we need to write these examples of "good code".
It is also important to note that although these are ideas that functional languages do well, it is by no means impossible to achieve this same behaviour in OO languages with a bit of clever programming.

### ROP programming and parameterize all the things
Scott introduced me to the idea of Railway Oriented Programming.
This is an idea that exploits one of the things that functional programming languages do well, called "composition", as explained earlier.
Although I highly recomend reading Scott's blog to get a great explanation of this, I will attempt to explain it briefly.

When working with algabraic types (types with "states") we find ourselves doing this bit of "matching" logic quite often.
That is why it may be a good idea to make a little function that takes all this boilerplate code and hides it from us.
We will use our "print" example from earlier and put it in a function.

```fsharp
let happyValue = OK (1) 
let errorValue = ERROR ("I am not happy about something") 

//Define our function that can handle things with states
let printSomeThingsFromValuesWithStates stateValue =
    match stateValue with
    | OK ourUnwrappedVariable -> 
        printfn "%d" (ourUnwrappedVariable * 2)
    | ERROR ourErrorMessage -> 
        printfn ourErrorMessage

//Execute the code
printSomeThingsFromValuesWithStates errorValue
```

Although this is now a function, it still won't help us in other places where we want to do things on functions wher things have states.
This is where we will use that cool property of FP where we can pass in functions as paramaters.
The example will now look like this:

```fsharp
let happyValue = OK (1) 
let errorValue = ERROR ("I am not happy about something") 

//Define the function that does something with an integer and returns nothing
let printSquareOfValue x = printfn "%d" (x * 2)

//Define our function that can handle things with states
let genericUnwrapFunction functionToExecuteIfOK stateValue =
    match stateValue with
    | OK x -> 
        functionToExecuteIfOK x
    | ERROR ourErrorMessage -> 
        printfn ourErrorMessage

//Execute the code
genericUnwrapFunction printSquareOfValue happyValue
```

Now it is nice and generic, however we see that we have some default behaviour on the ERROR case that will only print the errormessage.
We could take this default behavour out and pass in some other function, but for the puropses of this example, we won't really care about passing in custom behaviour for the ERROR case.
This function does not return anything, but when we want to start to chain these functions together using the pipe operator (|>) or the composition operator (>>), these functions will have to start returning outputs that we can use as inputs for the following functions.
One catch is that "matches" need to return the same type of output for all cases.
This is where the idea of railway oriented programming starts.
We will from now on, make sure that both the OK and the ERROR case returns another Result type.

```fsharp
//Define the functions that take integers and returns Result types
let SquareOfValue x = OK (x * 2)

let divide2ByNumber x = 
    if x = 0 then
        ERROR ("You cannot divide by 0")
    else
        OK (2/x)

//Define our function will take the int out of the Result type
let bind functionToExecuteIfOK stateValue =
    match stateValue with
    | OK x -> 
        functionToExecuteIfOK x
    | ERROR ourErrorMessage -> 
        //The default behaviour of the error case, is simply to output it again
        ERROR (ourErrorMessage)

//Execute the code using pipes
OK (0) 
|> bind divide2ByNumber
|> bind printSquareOfValue

//Or build a function that we can execute later
let functionToExecuteLater = 
    bind divide2ByNumber
    >> bind printSquareOfValue

//Execute the built-up function
functionToExecuteLater OK (0) 
```

Bercause our default behaviour for errors, is only to propagate them, we are can be sure that if an error gets generated in the `divide2ByNumber` function, the rest of the binds in the railway, will only propagate the error istead of trying to execute the fuction.
This can almost be seen as a way to early exit the execution of the code without having to throw and catch exceptions.

Notice that `bind` as we defined it, only works with methods that return Result types.
We can also build a bunch of other "wrapper" functions to handle all kinds of things such as "dead-end" functions that do not return anything, or functions that wraps functions that do not return Result tipes, or could throw exceptions.
You can really get creative here and feel free to build your own crazy wrappers.
These are some of the ones I usually find myself creating:

```fsharp
//Define our function wrap dead-end functions like database writes or even input validation
let tee functionToExecuteIfOK stateValue =
    //unwrap the input and Execute the function, 
    match (bind functionToExecuteIfOK) with
    | OK x -> 
        //and if the result is a OK, Propagate the original value
        stateValue
    | ERROR ourErrorMessage -> 
        //and if some error occurred tell us about it sothat we know skip the rest of the functions
        ERROR (ourErrorMessage)

//Wrap functions that could potentially throw exceptions in a type that will convert it to Result types
let tryWith functionToExecuteIfOK stateValue =
    try 
        //unwrap the input and Execute the function, 
        bind functionToExecuteIfOK stateValue
    with
    | error ->
        //and wrap errors in the error result state
        ERROR (error.message)

//Define a wrapper that will wrap functions that do not return result types
let tee functionToExecuteIfOK stateValue =
    match stateValue with
    | OK x -> 
        x //Take x
        |> functionToExecuteIfOK //Execute a function that return a normal value
        |> OK //And wrap that value in a Result type
    | ERROR ourErrorMessage -> 
        ERROR (ourErrorMessage)
```

That is all you need to know to fully understand how to write railway oriented programs.
In the next section, we will look at how this idea fits into big testable system

### Test induced damage
There is also some debate around a concept called "test induced damage", whereby code is made more complicated in order to make it more testable.
This concept ususally goes hand-in-hand with some of the arguments for SOLID design.
The 5 principals of solid design are intended to serve as guidelines to writing more testable and maintainable code.

### Better BDD in F# by exploiting "Parameterize all the things" principal
