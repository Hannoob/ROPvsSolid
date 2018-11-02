namespace ROPExample.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open WorkoutHandler
open ContractFactory
open ContractObjects

[<Route("api/[controller]")>]
[<ApiController>]
type WorkoutController () =
    inherit ControllerBase()

    [<HttpGet>]
    member this.Get() =
        let values = WorkoutHandler.``Get All Workouts Implementation`` |> Seq.map createWorkoutContract
        ActionResult<WorkoutContract seq>(values)

    [<HttpGet("{id}")>]
    member this.Get(id:int) =
        let value = "value"
        ActionResult<string>(value)

    [<HttpPost>]
    member this.Post([<FromBody>] value:string) =
        ()

    [<HttpPut("{id}")>]
    member this.Put(id:int, [<FromBody>] value:string ) =
        ()

    [<HttpDelete("{id}")>]
    member this.Delete(id:int) =
        ()
