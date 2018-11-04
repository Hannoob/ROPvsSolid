module UserHandler

open System
open DomainObjects

let AuthenticateUserImplementation 
    (getUserDetailsI:string -> Result<User,_>)
    (comparePasswordsI:string -> string -> Result<_,_>)
    (emailClientI:string -> string -> unit)
    (saveToHistoryLogI:User -> Action -> unit)
    (username:string ,password:string) =

    let validate (username:string ,password:string) =
        if String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) then
            Ok((username,password))
        else
            Error("Invalid params")

    let getUserDetails (username:string, password:string) =
        username
        |> getUserDetailsI 
        |> ROP.bind (fun (user) -> Ok (user, password))
    
    let comparePasswords (user:User, password:string) = 
        comparePasswordsI

    let emailClient (user) = 
        emailClientI

    let saveToHistoryLog (user) =
        saveToHistoryLogI

    let buildResponse m =
        new User()

    let impl  = 
        validate
        >> ROP.bind getUserDetails
        >> ROP.tee comparePasswords
        >> ROP.tee emailClient
        >> ROP.tee saveToHistoryLog
        >> buildResponse
    
    impl (username ,password)


let AuthenticateUser (username:string ,password:string) =
    let comparePasswords (providedPassword:string,password:string) = 
        Ok ()

    let emailClient user message = 
        Ok ()

    let saveToHistoryLog user action =
        Ok ()

    AuthenticateUserImplementation 
        UserRepo.GetUserForDetails
        comparePasswords
        emailClient
        saveToHistoryLog
        (username ,password)