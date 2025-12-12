namespace ChessGameBackend.GameControllers



open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open ChessGameBackend.Game
open ChessGameBackend.Services

[<ApiController>]
[<Route("api/[controller]")>]
type GameController (logger : ILogger<GameController>,ser:IChessStateService) =
    inherit ControllerBase()

    [<HttpGet("start-game")>]
    member _.Get() =
        ser.GameInit()

        
        
