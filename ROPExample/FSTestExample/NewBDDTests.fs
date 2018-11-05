namespace FSTestExample

open System
open NUnit.Framework
open FsUnit
open DomainObjects

type ``User authentication tests`` () =
    
    let ``Given an authentication handler`` =
        UserHandler.AuthenticateUserImplementation 

    let ``and the user details exist in the db`` =
        fun input -> Ok (new User("1","username","test@test.com","password"))

    let ``and the password validation succeeds`` =
        fun password1 password2 -> Ok ()

    let ``and the email sends successfully`` =
        fun string1 string2 -> Ok ()

    let ``and the email does NOT send successfully`` =
        fun string1 string2 -> Error ("Some major problem occurred")

    let ``and a valid username and password is provided`` =
        ("username","password")

    

    let ``Then the result should be an OK response`` result =
        match result with
        | Ok response -> 
            Assert.Pass("The response is OK")
            result
        | Error error -> 
            Assert.Fail("The response should have been successful:" + error)
            result

    let ``Then the result should be an Error response`` result =
        match result with
        | Ok response -> 
            Assert.Fail("The response should have failed")
            result
        | Error error -> 
            Assert.Pass("The response should was unsucessful" + error)
            result

    let ``and the response should have the correct username`` (result:User) =
        result.Name |> should equal "username"

    let ``and the response should have the correct email`` (result:User) =
        result.Email |> should equal "test@test.com"

    let ``and the response should have the correct user id`` (result:User) =
        result.Id |> should equal "1"

    [<Test>]
    member this.``Valid login test``() = 
        
        ``Given an authentication handler``
         ``and the user details exist in the db``
         ``and the password validation succeeds``
         ``and the email sends successfully``
         ``and a valid username and password is provided``
        
        |> ``Then the result should be an OK response``
        |> ROP.unwrap ``and the response should have the correct username``
        |> ROP.unwrap ``and the response should have the correct user id``
        |> ROP.unwrap ``and the response should have the correct email``
        |> ignore

    [<Test>]
    member this.``Email fail test``() = 
        
        ``Given an authentication handler``
         ``and the user details exist in the db``
         ``and the password validation succeeds``
         ``and the email does NOT send successfully``
         ``and a valid username and password is provided``
            
        |>``Then the result should be an Error response``
        |> ignore

