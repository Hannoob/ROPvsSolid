# Writing testable code with F#

In this blog post we look at how functional and OOP code differ in the way that testable code is written.
We will also try to make an argument for why functional programming lends itself much better to writing, not only clean, testable code, but also writing clean DRY tests.

## Disclaimers
I am by no means an F# or a functional programming expert.
Most of the opinions discussed in this post is my own, and most of the wisdom contained within this post was blatantly stolen (and probably mis-understood) from people that are way smarter than I.

In this post, we will use F# and C# in order to demonstrate the examples, however, the idea is that the principals demonstrated should apply to any functional language.

## A quick note about the assumptions made in this post.
We work from the assumnption that if code is not testable, it is not good code.
We assume that if code is testable, but not tested, it is not good code.
Finally, we assume that if tests are brittle or difficult to write, they will not be written or maintained.

This post does (poorly) explain some of the aspects of SOLID, BDD and ROP, however it would probably be better if you have some idea of what these things are bnefore you start reading this. 


## A classic SOLID example, how to write good code.
There is also some debate around a concept called "test induced damage", whereby code is made more complicated in order to make it more testable.
This concept ususally goes hand-in-hand with some of the arguments for SOLID design.
The 5 principals of solid design are intended to serve as guidelines to writing more testable and maintainable code.
### SOLID Design

### BDD tests
Behaviour driven development tests is the idea that testing should be done in a way that allows tests to act as documentation of the expected behaviour of

## Throwing all that we know about good design out of the window.
The solid pricipals are patterns that have been developed in order to deal with some of the common problems one encounters when writing large scale object oriented code.
When I was first introduced to these concepts, and I heard that I had to throw all the nice SOLID habits that I had spent so much time learning and advocating for, out of the window, I was resistant to say the least.
Especially because of the way it was advertised.
Insted of "Inversion of controll", functions.
Instead of "interface separation principal".. functions.

Another problem, was that there are lots of code snipets that demonstrate many of these principals, however, there was nothing that I could look at and see the benefit on a large system with lots of complexity.

### ROP programming and parameterize all the things

### Better BDD in F# by exploiting "Parameterize all the things" principal