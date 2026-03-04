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
type MoveController (logger : ILogger<MoveController>,stateService:IChessStateService) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        let rng = System.Random()
        stateService.GameInit()

    [<HttpPostAttribute>]
    member _.Post(arg: TPieceMove) =
        printfn "Post req : %A" arg
        let game_state = stateService.GetGameBoard(arg.gameId)
        let board_state = 
            match game_state with 
            | Some gameState-> gameState
            | None -> failwith("")
        
        board_state

        
        
