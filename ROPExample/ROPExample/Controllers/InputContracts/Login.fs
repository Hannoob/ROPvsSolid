module InputContracts

open System

type LoginContract (username:string,password:string) =
    member this.Username = username
    member this.Password = password
