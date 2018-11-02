namespace ContractObjects

type UserContract (id:string, name:string, email:string) = 
    member this.Id = id
    member this.Name = name
    member this.Email = email
