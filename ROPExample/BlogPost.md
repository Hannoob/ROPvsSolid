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

```csharp

``

### BDD tests
Behaviour driven development tests is the idea that testing should be done in a way that allows tests to act as documentation of the expected behaviour of

##Test induced damage
There is also some debate around a concept called "test induced damage", whereby code is made more complicated in order to make it more testable.
This concept ususally goes hand-in-hand with some of the arguments for SOLID design.
The 5 principals of solid design are intended to serve as guidelines to writing more testable and maintainable code.

## Throwing all that we know about good design out of the window.
The solid pricipals are patterns that have been developed in order to deal with some of the common problems one encounters when writing large scale object oriented code.
When I was first introduced to these concepts, and I heard that I had to throw all the nice SOLID habits that I had spent so much time learning and advocating for, out of the window, I was resistant to say the least.
Especially because of the way it was advertised.
Insted of "Inversion of controll", functions.
Instead of "interface separation principal".. functions.

Another problem, was that there are lots of code snipets that demonstrate many of these principals, however, there was nothing that I could look at and see the benefit on a large system with lots of complexity.

### ROP programming and parameterize all the things

### Better BDD in F# by exploiting "Parameterize all the things" principal
