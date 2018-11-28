# Writing testable code in a functional style

In this blog post we look at how functional and Object Oriented (OO) code differ in the way that testable code is written.
We will also try to make an argument for why functional programming (FP) lends itself much better to writing, not only clean, testable code, but also writing [DRY](https://dzone.com/articles/is-your-code-dry-or-wet) tests.

## Disclaimers

I am by no means an F# or a functional programming expert.
All of the opinions discussed in this post is my own, and most of the wisdom contained within this post was blatantly stolen (and probably mis-understood) from people who are way smarter than I am.

In this post, we will use F# and C# in order to demonstrate the examples, however, the idea is that the principals demonstrated should apply to any object oriented and functional language.

This is not meant to be an in-depth tutorial on [Railway Oriented Programming (ROP)](https://fsharpforfunandprofit.com/rop/) and [SOLID design](https://scotch.io/bar-talk/s-o-l-i-d-the-first-five-principles-of-object-oriented-design), or functional code in general.
It is aimed more at showcasing the readability and re-usability benefits of writing code in a functional style when it comes to unit testing specifically.
However, it is also very likely that I start going down a rabbit hole when explaining some of the concepts, which is fine by me because planning out a well written post is way too much effort!

## Audience

In order to get the most bang for your buck out of this work, I have decided to write most of it with two audiences in mind.
The first is a non-technical person, or an OO programmer just casually looking at some of the many benefits that F# has to offer, or a FP programmer just looking at some ideas on how to structure code and tests.
For this type of audience member, I would recommend skipping the following chapters as these are a bit more technical and not as relevant to the actual benefits of testing using functional code.

[Some functional programming basics](#Some-functional-programming-basics)

[Railway oriented programming](#Railway-orriented-programming)

For programmers interested in learning functional programming and who would like to try out some of these ideas in practice, I would highly recommend reading the entire document.
Please note: This post does (poorly) explain some of the aspects of SOLID, BDD and ROP, however it would probably be better if you look at some of the original sources for a proper explanation.

## A quick note about the assumptions made in this post

I am aware that there is some debate on the topic of testing.
I work from the assumption that if code is not testable, it is not good code.
I assume that if code is testable, but not tested, it is also not good code.
Finally, I assume that if tests are brittle or difficult to write, they will not be written or maintained, resulting in bad code.

## A classic SOLID example, how to write good code

In this post, we will not focus on any of the specific principals and their applications, but rather examine the structure and tests of a "typical" project designed according to some of these principals.

### 1. Typically external dependencies and large pieces of logic are hidden behind interfaces

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

Typically, during testing, we use tools such as [Moq](https://github.com/Moq/moq4) in order to do throwaway implementations of interfaces for testing purposes.

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

This application of interfaces has some obvious benefits, but how do we actually allow our code to not care about the specific implementation of the interface?
Well, this leads us to the second principal that matters in designing good software...

### 2. External dependencies are injected into the constructors of classes

The idea is to invert the dependencies of classes by adding dependencies of classes as parameters in the constructor of the class, rather than having to new up these dependencies in the business logic class itself.
(There are other ways of handling dependencies that is not "constructor injection" however this is the most common and also my favorite method)
This allows whatever is "newing up" the business logic class to choose the implementations of the dependency interfaces.

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

In practice, it is often some dependency injection framework that new up these classes, however, this pattern has the awesome advantage, in that it allows you to pass in your mock interface implementations into the business logic class when it is being set up for testing.
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
There are many ways of testing software, and many different schools of thought around what "good" tests are.
I personally like the idea of Behavior Driven Testing (BDT) or Behavior Driven Development (BDT), where the test cases are focused around how the user, or other parts of the system will interact with the code being tested.
This method of testing has a couple of advantages over the traditional way of writing tests.
One of the main advantages that I would like to focus on, is the fact that these tests are written in such a way that the structure of the test conveys information of what is being tested and what the expected behavior should be.
This has the implication of the test acting as a sort of "living documentation" that will remain up to date and that anyone can understand.
These tests are also very easy to maintain, because of the fact that each test is focused on a very specific piece of behavior.

The main principal behind this form of testing is that it should follow a "Given,When,Then" structure.
The "Given" part explains the state of the method being tested, "When" explains the action taken by the user, and the "Then" statements explain the expected behavior of the method.

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

## Throwing all that we know about "good code" out of the window

Everything that we have talked about up to this point is basically what I think of when I hear the words "Clean Code".
However, recently I started looking at large project written in F# which is a pretty cool functional .net language.
When looking at these projects, I realized that unfortunately, the developers that write much of these code bases, like me, also know how to write "good code".
This meant that we see a lot of the same design patterns from object oriented code in functional code bases.
It is also unfortunate that F# allows you to get away with using many of these patterns, which leads to a lot of weird looking functional code.
However, I came to realize, that the SOLID design principals, where really developed to deal with some of the issues specific to OOP, rather than just guidelines for good software.
Another way I started looking at it, was that the solid design principals are only a way to implement some set of deeper principals that make "good" code "good".
These deeper principals are not concerned with how you use your language paradigm in order to implement these principals.
Unfortunately, I am not smart enough to tell you exactly what these are, however, there are thoughts around this such as the principal of ["high cohesion, and low coupling"](https://stackoverflow.com/questions/3085285/difference-between-cohesion-and-coupling) as well as ideas focusing on [properties of good code](https://www.codementor.io/learn-development/what-makes-good-software-architecture-101) such as functionality, robustness, measurability, debuggability, maintainability, reusability, and extensibility.

I do not blame myself to harshly for conflating the two ideas of "good code" and SOLID design, as there really aren't that many good resources and examples how to achieve these properties in a functional paradigm.
There is also the reality that when I heard that I had to throw all the nice SOLID habits, that I had spent so much time learning and advocating for, out of the window, I was resistant to say the least.
Another problem, was that there are lots of code snippets that demonstrate ways to implement many of these principals, however, there was nothing that I could look at and see the benefit on a large system with lots of complexity.

### Some functional programming basics

There are some people that do make a huge effort to show us mere mortals how to write good code in functional languages, and being an F# guy, I have to give credit to Scott Wlaschin's [blog](https://fsharpforfunandprofit.com), although I am sure there are many more.
If you are familiar with the basics of functional programming you are more than welcome to skip this section, but for the reader who would like to get a deeper understanding; this part is for you.
The first concept I would like to touch on, is an idea called "piping"
If you can get around the F# syntax, the following example shows one example of this idea.

```fsharp
//This is a function that takes an X and a Y and adds them together
let someMathFunction x = x + 1

//This is how it is usually called
let normalAnswer = someMathFunction 2

//Piping allows us to change the order of parameters. We take two, and push it through the function to get an answer
let pipedAnswer = 2 |> someMathFunction

//This is often read as "take 2, and pipe it through 'someMathFunction'"

//If we want to combine these functions we would traditionally do the following
let combinedWay = someMathFunction (someMathFunction 2)

//But piping allows us to make this a bit prettier
let pipedCombinedWay = 2 |> someMathFunction |> someMathFunction
```

The last method can be seen as taking the value 2, using it as the input for the function "someMathFunction" and then taking the resulting value and using it as an input for the function "someMathFunction".
This gives you the freedom to structure your code in such a way that it is almost understandable by any non-programmer.
The only limitation is that the output type of the preceding function should match the input of the following it.

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

F# also allows for a thing called composition, where you get to glue functions together.
And same as before, the only requirement is that the output of the first function, should be the same as the input of the second function:

```fsharp
let addOne x = x + 1

let printAnswer x = printfn "%d" x

let printAnswer = 
    addOne 
    >> addOne
    >> printAnswer

//execute the code
printAnswer 2
```

This code demonstrates how we were able to glue these functions together, to give us a new function that expects an integer as an input, and then we can use it whenever we are ready.

There is also a simple idea in FP that allows us to pass in some functions as parameters to other functions.
This idea is actually also quite prevalent in many modern OO languages, although the syntax is sometimes a bit of a hassle.

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

There is also a concept called algebraic data types and monads which I am by far not clever enough to explain well, but here goes a quick introduction that should give you the basic idea.
For our examples, we will say that values in our software can be wrapped up in a type called a "Result".
A type is similar to a class in OO.
A value of Result type could be in an "OK" state, that contains some value, or it could be in an "ERROR" state that contains an error message.
Although the one state contains an int, and the other a string, they are still wrapped up in the same type.
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
It is also important to note that although these are ideas that functional languages do well, it is by no means impossible to achieve this same behavior in OO languages with a bit of clever programming.

### Railway oriented programming

Scott's blog introduced me to the idea of Railway Oriented Programming.
This is an idea that exploits one of the things that functional programming languages do well, called "composition", as explained earlier.
Although I highly recommend reading Scott's blog to get a great explanation of this, I will attempt to explain it briefly.

When working with algebraic types (types with "states") we find ourselves doing this bit of "matching" logic quite often.
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

Although this is now a function, it still won't help us in other places where we want to do things on functions where things have states.
This is where we will use that cool property of FP where we can pass in functions as parameters.
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

Now it is nice and generic, however we see that we have some default behavior on the ERROR case that will only print the error message.
We could take this default behavior out and pass in some other function, but for the purposes of this example, we won't really care about passing  custom behavior for the ERROR case.
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

Because our default behavior for errors, is only to propagate them, we are can be sure that if an error gets generated in the `divide2ByNumber` function, the rest of the binds in the railway, will only propagate the error instead of trying to execute the function.
This can almost be seen as a way to early exit the execution of the code without having to throw and catch exceptions.

Notice that `bind` as we defined it, only works with methods that return Result types.
We can also build a bunch of other "wrapper" functions to handle all kinds of things such as "dead-end" functions that do not return anything, or functions that wraps functions that do not return Result types, or could throw exceptions.
You can really get creative here and feel free to build your own crazy wrappers.
These are some of the ones I usually find myself creating the following functions.

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
        ERROR (error.Message)

//Define a wrapper that will wrap functions that do not return result types
let map functionToExecuteIfOK stateValue =
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

### A realistic example and parameterize all the things

In order to examine how this will look in practice, I will use an example of a fictional Authentication endpoint and draw comparisons between the OO way and ROP style.
But before we start, there is a little syntactic sugar that F# has that takes the railway style to the next level for me.
I call it the double back-tick trick, and it simply allows you to add spaces to the names of methods.
But this has some huge effects when it comes to code-readability.
Just look at the example below!

```fsharp
let ``take an input value 4`` = 4
let ``square it`` x = x*x
let ``and print the answer`` x = printfn "%d" x

``take an input value 4``
|> ``square it``
|> ``and print the answer``

```

Okay, now that we have that cool trick, let's look at some code.
The basic flow of the program will be described by the railway code:

```fsharp
``Validate that the input is not empty``
>> ROP.bind ``Get user details from the DB``
>> ROP.tee  ``Compare provided password with user password``
>> ROP.tee  ``Email login confirmation to the client``
>>          ``Log authentication to client history``
>> ROP.map  ``Build authentication response``
```

Now if this is not readable, then I do not know what is.
But let's look at some of the complexity that is hidden behind these deceptively simple method names.

```fsharp
let AuthenticateUser =
    let ``Validate that the input is not empty`` (username:string ,password:string) =
        if String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) then
            Error("Invalid params")
        else
            Ok((username,password))

    let ``Get user details from the DB`` (username:string, password:string) =
        username
        |> UserRepo.getUserDetails 
        |> ROP.map (fun (user) -> (user, password))
    
    let ``Compare provided password with user password`` (user:User, password:string) = 
        PasswordUtils.comparePasswords user.Password password

    let ``Email login confirmation to the client`` (user:User, password:string) = 
        EmailService.EmailClient user.Email "You have successfully logged in to your new awesome fitness app!"

    let ``Log authentication to client history`` m =
        match m with
        | Ok (user:User,_) -> 
            user.Id
            |> printf "User %s has logged in successfully"
            |> Log.Debug
            m
        | Error error -> 
            Log.Error (printf "An error occurred during login") error
            m

    let ``Build authentication response`` (user:User, password:string) =
        user
 
    ``Validate that the input is not empty``
    >> ROP.bind ``Get user details from the DB``
    >> ROP.tee  ``Compare provided password with user password``
    >> ROP.tee  ``Email login confirmation to the client``
    >>          ``Log authentication to client history``
    >> ROP.map  ``Build authentication response``
```

Now we can see that most of the functions that we define in this method, are simply functions that wrap other functions in order to make them accept the parameters needed to make them fit in the railway.
This is because the input of a method in the railway, needs to match the output of the previous method.
This is one of the drawbacks of ROP in my opinion, however, this is something that could possibly be addressed by the clever use of types.

When we look at how this code interacts with its external dependencies such as the EmailClient, we can see that is basically calls a method in a different module which is almost like a static method, but it is not contained within a class.
That does not look like testable code at all, furthermore, this method is also not in a class, and therefore we cannot use constructor injection as in the SOLID example. (I mean, we could write our code in classes, but then we are not really using the functional paradigm to our advantage.)
This is where we can use an idea by Scott called "Parameterize all the things".
In this context we will simply extract all the dependencies and pass them in as parameters.
This is a different way to achieve inversion of control, but this way needs no framework to inject things.
This is what the final, testable method would look like.

```fsharp
let AuthenticateUserImplementation
    (getUserDetailsI:string -> Result<User,_>)
    (comparePasswordsI:string -> string -> Result<unit,_>)
    (emailClientI:string -> string -> Result<unit,_>) =

    let ``Validate that the input is not empty`` (username:string ,password:string) =
        if String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) then
            Error("Invalid params")
        else
            Ok((username,password))

    let ``Get user details from the DB`` (username:string, password:string) =
        username
        |> getUserDetailsI
        |> ROP.map (fun (user) -> (user, password))
    
    let ``Compare provided password with user password`` (user:User, password:string) = 
        comparePasswordsI user.Password password

    let ``Email login confirmation to the client`` (user:User, password:string) = 
        emailClientI user.Email "You have successfully logged in to your new awesome fitness app!"

    let ``Log authentication to client history`` m =
        match m with
        | Ok (user:User,_) ->
            user.Id
            |> printf "User %s has logged in successfully"
            |> Log.Debug
            m
        | Error error -> 
            Log.Error (printf "An error occurred during login") error
            m

    let ``Build authentication response`` (user:User, password:string) =
        user

    ``Validate that the input is not emply``
    >> ROP.bind ``Get user details from the DB``
    >> ROP.tee  ``Compare provided password with user password``
    >> ROP.tee  ``Email login confirmation to the client``
    >>          ``Log authentication to client history``
    >> ROP.map  ``Build authentication response``

let AuthenticateUser (username:string ,password:string) =
    AuthenticateUserImplementation 
        UserRepo.GetUserForDetails
        PasswordUtils.ComparePasswords
        EmailService.EmailClient
        (username ,password)
```

This way of doing things has the benefit that you are now able to pass in any functions into the method, that has the same method signature.
That means, that when we look at our `UserRepo`, we see the following simple piece of code.

```fsharp
let GenerateUser =
    new UserDataObject("1","Hanno Brink","test@test","password")

let GetUserForDetails username = 
    GenerateUser 
    |> DataObjectFactory.createUserDomainObject
    |> Ok
```

And that is all, no interfaces, no abstractions, nothing.
The method signature that we use as a parameter in the method, acts like an interface.
Because of the strong type checking in F# we are guaranteed that the code will not compile if the functions that gets passed in, does not conform to the expected type.
This idea of not having an interface over my repo methods is one of the things that was very difficult for me to make peace with when I was first confronted with these ideas.
But after seeing it in action on a more realistic example of code, I can now start to see the benefits.

There are also some opinions around a concept called "test induced damage", whereby code is made more complicated in order to make it more testable.
This concept usually goes hand-in-hand with some of the arguments for SOLID design.
Mark Seemann has a [talk](https://www.infoq.com/presentations/mock-fsharp-tdd) about BDD with F# where he discusses this aspect of F# code development, and I feel like looking at the repo method, and comparing it to the C# code makes a very good case for why SOLID might not be "all that", in that we can achieve the same "high cohesion, low coupling" idea with a lot less code.
In fact, we achieve all these benefits using nothing but functions that we pass in as parameters.

Finally, when writing the C# example, using entirely fictional integrations, it was extremely difficult for me to know how and when to handle exceptions.
I was unsure of which errors can be thrown where, and how it would affect the variables I had defined in my method.
This was in complete contrast to the Railway method where every method is forced to have an explicit handler for both the OK and ERROR cases.
I found this much more intuitive, and comforting, knowing that the inputs to my method would be exactly as I expect them. 
No `null` values because of some improperly handled exception or some other unforeseen changes to the state of my class.

### Better BDD in F# by exploiting "Parameterize all the things" principal

Okay, so we are pretty excited about the awesome code we can write with Functional Programming, but what does this mean for testing?
This is actually the part that inspired me to write this project in the first place.
Since we don't have interfaces anymore, we have to come up with a new way to replace the dependencies.
Well, since the dependencies are only function calls that gets passed in as parameters, we can pass in our own little stubs, and then interrogate the result.
Because the result is a "wrapped" type, we have to extract the value in order to test it.

```fsharp
type ``User authentication tests`` () =
    [<Test>]
    member this.``Valid login test``() = 
        let result = 
            AuthenticateUserImplementation 
                fun input -> Ok (new User("1","username","test@test.com","password"))
                fun password1 password2 -> Ok ()
                fun string1 string2 -> Ok ()
                (username ,password)

        match result with
            | Ok response -> 
                response.Name |> should equal "username"
            | Error error -> 
                Assert.Fail("The response should have been successful:" + error)
                result
        
```

This is pretty cool, but we can make it better.
One of the problems, is that this way of doing things really does not give us the same readability as the BDD tests we saw in C#.
These are not that readable, and that is something we can improve by defining the parameters as functions and using the "double back-tick trick" to give them descriptive names.
As an added bonus, if we define these dependencies as function, we can re-use them in multiple test cases.
This is something that is often difficult to achieve with OO style tests.
Finally, we can use "piping" to pipe our result through a bunch of asserts with descriptive names, and this has the effect of making even our asserts re-usable!
As far as I know this is not something that is often done in testing, ever.
We can also extract the "unwrapping" logic into a method so that we do not have to repeat that on every assert.
This is what the resulting code would look like.

```fsharp
open FsUnit
type ``User authentication tests`` () =
    //Simply unwraps the result so that we can do asserts on the contents
    let inspect assertFunction result =
        match result with
        | Ok responseValue -> 
            assertFunction responseValue |> ignore
            result //Pass the original result to the next function
        | Error e -> Error e
    
    //---------------------------------------------------------------------------------------------    
    //Dependencies
    //---------------------------------------------------------------------------------------------
    let ``Given an authentication handler`` = UserHandler.AuthenticateUserImplementation 

    let ``and the user details exist in the db`` = fun input -> Ok (new User("1","username","test@test.com","password"))

    let ``and the password validation succeeds`` = fun password1 password2 -> Ok ()

    let ``and the email sends successfully`` = fun string1 string2 -> Ok ()

    let ``and the email does NOT send successfully`` = fun string1 string2 -> Error ("Some major problem occurred")

    let ``and a valid username and password is provided`` = ("username","password")

    //---------------------------------------------------------------------------------------------  
    //Asserts
    //---------------------------------------------------------------------------------------------
    let ``Then the result should be an OK response`` result =
        match result with
        | Ok response -> 
            result
        | Error error -> 
            Assert.Fail("The response should have been successful:" + error)
            result

    let ``Then the result should be an Error response`` result =
        match result with
        | Ok response -> 
            Assert.Fail("The response should have failed")
            result
        | Error error -> 
            result

    let ``and the response should have the correct username`` (result:User) =
        result.Name |> should equal "username"

    let ``and the response should have the correct email`` (result:User) =
        result.Email |> should equal "test@test.com"

    let ``and the response should have the correct user id`` (result:User) =
        result.Id |> should equal "1"

    let ``that is all`` = ignore //Stop the test from outputting anything
    
    //---------------------------------------------------------------------------------------------  
    //Tests
    //---------------------------------------------------------------------------------------------    

    [<Test>]
    member this.``Valid login test``() = 
        
        ``Given an authentication handler`` //This returns the function
         ``and the user details exist in the db`` //Use this function as the first parameter
         ``and the password validation succeeds`` //Use this function as the second parameter
         ``and the email sends successfully``//Use this function as the third parameter
         ``and a valid username and password is provided`` //Use this function as the final parameter
        
        |> ``Then the result should be an OK response`` //Take the output and pipe it into this function
        |> inspect ``and the response should have the correct username``
        |> inspect ``and the response should have the correct user id``
        |> inspect ``and the response should have the correct email``
        |> ``that is all``

    [<Test>]
    member this.``Email fail test``() = 
        
        ``Given an authentication handler``
         ``and the user details exist in the db``
         ``and the password validation succeeds``
         ``and the email does NOT send successfully``
         ``and a valid username and password is provided``
            
        |>``Then the result should be an Error response``
        |> ``that is all``
```

When I saw this, my mind was blown, by giving our dependencies nice names, we were able to define the behavior of our dependencies using words, and then giving our asserts nice names, we can describe the expected behavior in words.
This looks a lot like many of the tests written using BDD frameworks, but this does not use any fancy frameworks.
Just standard F# is used to achieve these readable re-usable tests.
I also believe that this is just the tip of the iceberg, our re-usable asserts could also accept additional parameters as input making it more re-usable.
Finally, coming back to one of our assumptions; "we assume that if tests are brittle or difficult to write, they will not be written or maintained, resulting in bad code".
Because of the high readability and re-usability of these test components, it is super easy to write and maintain.
This, in my opinion, would lead to better code in the long term.

## Some drawbacks or ROP and ROP style testing

Despite how much I love this new way of writing and testing code, I have three issues with this style of writing code and testing.

### 1. Tooling

The first is a pretty minor issue, and that is that the way the new ROP tests are structured, are not very compatible with how Visual Studio displays these tests.
As demonstrated in the example below, the traditional C# tests are displayed in such a way, that when any tests fail, it is clear from just looking at the test runner, which tests failed, however, when looking at the F# tests, the exact test case that failed is not obvious.
In this example, I broke the code that would return a "userId".
From just looking at the C# test results, it is immediately obvious what happened in the C# tests, as opposed to the F# tests that would take some debugging to find out what is actually broken.

![How traditional BDD tests look when Visual Studio runs them, vs. how they look in the new ROP style](https://github.com/Hannoob/ROPvsSolid/blob/master/ROPExample/Images/test.bmp "How traditional BDD tests look when Visual Studio runs them, vs. how they look in the new ROP style")

### 2. Speed

As I was writing this post and taking screenshots of the tests running, I noticed that the new and improved ROP tests seemed to be quite a bit faster than the corresponding OOP tests when there were failing tests.
The example above shows the performance of a single test case in both scenarios with one assert failing, and we see an improvement of about 30% in the speed in which the tests were executed.
I am not sure if this was due to the code being faster, or the tests requiring less setup, or because of the fact that the OO tests have 3 actual test methods vs the ROP tests were there is only one, or some other factor all together.
It gave me the idea that we might see more significant performance benefits as the number of tests increase, because of the re-usability of so many of the test dependencies.
Let's see how these tests perform when we add an additional test scenario.

![How traditional BDD tests performs , vs. how ROP tests perform](https://github.com/Hannoob/ROPvsSolid/blob/master/ROPExample/Images/testSpeeds.bmp "How traditional BDD tests performs , vs. how ROP tests perform")

All of a sudden, the tests were a lot slower.
This came as a bit of a surprise to me, and I am not sure why this is.
If anyone has any ideas on why this could be, or knows why my test might not be valid, please let me know.
I think it might have something to do with the fact that failing tests take longer to execute than successful tests.
(My colleague, Dean, pointed out to me that this may very well be related to the version of Visual Studio that I was using at the time. This will be an interesting experiment to repeat at some point in the future.)

### 3. Debugging

The final issue is more of a bother to me than the other two, and unfortunately, also more difficult to demonstrate.
This is the fact that in Visual Studio, ROP style code, is much more difficult to step through when it comes to debugging time.
This could be countered by saying that, generally, F# code requires a lot less debugging, but when you have to step through, it is not quite as easy as on the C# code.
This is because each piece of logic consists of a bunch of functions glued together, and not disjunct steps like C# code.
I guess it is possible that in future, someone comes up with a better tool to debug these composed functions, but as it stands right now, it is not that easy to step through the code.

## Closing arguments

I personally like the readability and how easy it is to write tests, as well as the way I hardly ever get surprized by the behavior of the code I write, more than I dislike some of the minor kinks this may have.
I am also a huge fan of the idea that I can basically copy and paste both my code, and my tests, and paste it into some document to serve as documentation for non-technical people.
(I have never actually tried this, but please let me know if it works).

So, in the end, just like any other language or paradigm that came before this, it is not the silver bullet that will solve all the issues we've had with code, but I do think there are a few things that we can learn from these ideas that could change software everywhere for the better.
Finally, I am hoping that this post furthers the discussion on the topic of good design principals for functional projects, with many people contributing to, or challenging these ideas, so that we can get to a point where we have established best-practices for writing functional code, with great examples to make it easy to understand.
