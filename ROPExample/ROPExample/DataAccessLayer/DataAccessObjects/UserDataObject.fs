namespace DataAccessObjects

open System

type UserDataObject (id:string, name:string, email:string, password:string) = 
    member this.Id = id
    member this.Name = name
    member this.Email = email
    member this.Password = password
