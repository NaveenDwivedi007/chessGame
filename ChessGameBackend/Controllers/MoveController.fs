namespace ChessGameBackend.MoveControllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open ChessGameBackend.Move
open ChessGameBackend.Services

[<ApiController>]
[<Route("api/[controller]")>]
type MoveController (logger : ILogger<MoveController>, stateService: IChessStateService) =
    inherit ControllerBase()

    [<HttpPost>]
    member this.Post(arg: TPieceMove) : IActionResult =
        logger.LogInformation("Move request: {Move}", arg)
        match stateService.MakeMove arg.gameId arg.moveForm arg.moveTo with
        | Ok response ->
            this.Ok(response) :> IActionResult
        | Error msg ->
            this.BadRequest({| error = msg |}) :> IActionResult
