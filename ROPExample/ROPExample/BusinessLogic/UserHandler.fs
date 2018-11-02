module UserHandler

open System
open DomainObjects

let ``Authenticate user`` (username:string ,password:string) =
    let validate (username:string ,password:string) =
        if String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) then
            Ok((username,password))
        else
            Error("Invalid params")

    let getUserDetails (username:string ,password:string) =
        Ok (new User("","",""), password)
    
    let comparePasswords (user:User,password:string) = 
        Ok ()

    let impl  = 
        validate
        >> ROP.bind getUserDetails
        >> ROP.tee comparePasswords
        
    
    impl (username ,password)

