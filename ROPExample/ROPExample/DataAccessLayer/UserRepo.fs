module UserRepo

open DataAccessObjects

let GenerateUser =
    new UserDataObject("1","Hanno Brink","test@test","password")

let GetUserForDetails username = 
    Ok GenerateUser