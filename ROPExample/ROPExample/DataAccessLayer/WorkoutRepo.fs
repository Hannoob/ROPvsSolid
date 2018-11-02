module WorkoutRepo

open DataAccessObjects

let GenerateWorkout =
    let exercises = [|new ExerciseDataObject(1,"sqauts") |] |> ResizeArray
    new WorkoutDataObject("workout1",exercises,4,20,40,180)

let GetAllWorkouts =
    [|  
        GenerateWorkout
        GenerateWorkout
    |] |> Seq.map DataObjectFactory.createWorkoutDomainObject

