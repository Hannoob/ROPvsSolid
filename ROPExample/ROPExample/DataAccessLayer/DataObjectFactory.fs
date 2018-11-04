module DataObjectFactory

open DomainObjects
open DataAccessObjects

let createExerciseDomainObject (exercise:ExerciseDataObject):Exercise  = 
    new Exercise(exercise.Number,exercise.Name)

let createWorkoutDomainObject (workout:WorkoutDataObject):Workout = 
    let exercises = workout.Exercises |> Seq.map createExerciseDomainObject |> ResizeArray
    new Workout(workout.Name,exercises, workout.Sets, workout.Rest, workout.Work, workout.Warmup)

let createUserDomainObject (user:UserDataObject):User  = 
    new User(user.Id,user.Name,user.Email,user.Password)