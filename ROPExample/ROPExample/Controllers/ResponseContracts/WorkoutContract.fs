namespace ContractObjects

type ExerciseContract (number:int, name:string) =
    member this.Number = number
    member this.Name = name

type WorkoutContract (name:string, exercises:ExerciseContract ResizeArray, sets: int, rest:int, work:int, warmup:int) = 
    member this.Name = name
    member this.Exercises = exercises
    member this.Sets = sets
    member this.Rest = rest
    member this.Work = work
    member this.Warmup = warmup
