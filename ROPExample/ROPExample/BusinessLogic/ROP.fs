module ROP

let bind f m =
    match m with
    | Ok res -> f res
    | Error e -> Error e

let tee f m =
    match bind f m with
    | Ok res -> m
    | Error e -> Error e

let map f m =
    match m with
    | Ok res -> Ok (f res)
    | Error e -> Error e

let tryWith f m =
    try
        map f m
    with 
    | e -> Error e.Message 

let unwrap f m =
    match m with
    | Ok res -> 
        f res |> ignore
        m
    | Error e -> Error e