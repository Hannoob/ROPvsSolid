module UserRepo

open DataAccessObjects

let GenerateUser =
    new UserDataObject("1","Hanno Brink","test@test")

let GetAllWorkouts =
    [|  
        GenerateUser;
        GenerateUser
    |]

let GetUserForId id = 
    GenerateUser
