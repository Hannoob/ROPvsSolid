module ROP

let bind f m =
    match m with
    | Ok res -> f res
    | Error e -> Result.Error(e)

let tee f m =
    match bind f m with
    | Ok res -> m
    | Error e -> Result.Error(e)

let tryWith f m =
    try
        bind f m
    with 
    | e -> Error e 

let unwrap m =
    match m with
    | Ok res -> res
    | Error e -> Result.Error(e)