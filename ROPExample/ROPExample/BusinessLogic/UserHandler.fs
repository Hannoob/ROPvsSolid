module UserHandler

open System
open DomainObjects
open Microsoft.Extensions.Logging;

let AuthenticateUserImplementation
    (getUserDetailsI:string -> Result<User,_>)
    (comparePasswordsI:string -> string -> Result<unit,_>)
    (emailClientI:string -> string -> Result<unit,_>)
    (username:string ,password:string) =

    let ``Validate that the input is not emply`` (username:string ,password:string) =
        if String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) then
            Error("Invalid params")
        else
            Ok((username,password))

    let ``Get user details from the DB`` (username:string, password:string) =
        username
        |> getUserDetailsI 
        |> ROP.map (fun (user) -> (user, password))
    
    let ``Compare provided password with user password`` (user:User, password:string) = 
        comparePasswordsI user.Password password

    let ``Email login confirmation to the client`` (user:User, password:string) = 
        emailClientI user.Email "You have successfully logged in to your new awesome fitness app!"

    let ``Log authentication to client history`` m =
        match m with
        | Ok (user:User,_) -> 
            user.Id
            |> printf "User %s has logged in successfully"
            |> Log.Debug
            m
        | Error error -> 
            Log.Error (printf "An error occurred during login of username %s:" username) error
            m

    let ``Build authentication response`` (user:User, password:string) =
        user
 
    (username ,password)
    |>          ``Validate that the input is not emply``
    |> ROP.bind ``Get user details from the DB``
    |> ROP.tee  ``Compare provided password with user password``
    |> ROP.tee  ``Email login confirmation to the client``
    |>          ``Log authentication to client history``
    |> ROP.map  ``Build authentication response``

let AuthenticateUser (username:string ,password:string) =
    AuthenticateUserImplementation 
        UserRepo.GetUserForDetails
        PasswordUtils.ComparePasswords
        EmailService.EmailClient
        (username ,password)