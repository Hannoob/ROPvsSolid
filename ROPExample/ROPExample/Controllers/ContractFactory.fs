module ContractFactory

open DomainObjects
open ContractObjects

let createExerciseContract (exercise:Exercise):ExerciseContract  = 
    new ExerciseContract(exercise.Number,exercise.Name)

let createWorkoutContract (workout:Workout):WorkoutContract= 
    let exercises = workout.Exercises |> Seq.map createExerciseContract |> ResizeArray
    new WorkoutContract(workout.Name,exercises, workout.Sets, workout.Rest, workout.Work, workout.Warmup)

let createUserContract (user:User):UserContract  = 
    new UserContract(user.Id,user.Name,user.Email)