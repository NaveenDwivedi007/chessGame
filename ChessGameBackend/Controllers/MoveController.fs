namespace ChessGameBackend.MoveControllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Services

[<ApiController>]
[<Route("api/[controller]")>]
type MoveController (logger : ILogger<MoveController>, stateService: IChessStateService) =
    inherit ControllerBase()

    [<HttpPost>]
    member this.Post(arg: TPieceMove) : IActionResult =
        logger.LogInformation("Move request: {Move}", arg)
        let promotionPiece =
            if isNull arg.promotionPiece then None
            else
                match arg.promotionPiece.ToLowerInvariant() with
                | "queen"  -> Some Queen
                | "rook"   -> Some Rook
                | "bishop" -> Some Bishop
                | "knight" -> Some Knight
                | _        -> None
        match stateService.MakeMove arg.gameId arg.moveForm arg.moveTo promotionPiece with
        | Ok response ->
            this.Ok(response) :> IActionResult
        | Error "Game not found" ->
            this.NotFound({| error = "Game not found" |}) :> IActionResult
        | Error "Game is already over" ->
            this.Conflict({| error = "Game is already over" |}) :> IActionResult
        | Error msg ->
            this.BadRequest({| error = msg |}) :> IActionResult
