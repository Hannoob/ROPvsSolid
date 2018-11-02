namespace DomainObjects

open System

type Exercise (number:int, name:string) =
    member this.Number = number
    member this.Name = name

type Workout (name:string, exercises:Exercise ResizeArray, sets: int, rest:int, work:int, warmup:int) = 
    member this.Name = name
    member this.Exercises:Exercise ResizeArray = exercises
    member this.Sets = sets
    member this.Rest = rest
    member this.Work = work
    member this.Warmup = warmup
