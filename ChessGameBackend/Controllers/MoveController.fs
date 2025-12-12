namespace ChessGameBackend.MoveControllers


open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open ChessGameBackend.Move
open ChessGameBackend.Services

[<ApiController>]
[<Route("api/[controller]")>]
type MoveController (logger : ILogger<MoveController>,ser:IChessStateService) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        let rng = System.Random()
        ser.GameInit()

        
        
