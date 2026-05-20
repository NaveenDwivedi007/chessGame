namespace ChessGameBackend.GameControllers



open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Services

[<ApiController>]
[<Route("api/[controller]")>]
type GameController (logger : ILogger<GameController>, ser: IChessStateService) =
    inherit ControllerBase()

    [<HttpGet("start-game")>]
    member _.StartGame() =
        ser.GameInit()

    [<HttpGet("{id}")>]
    member this.GetById(id: string) : IActionResult =
        match ser.GetGameState(id) with
        | None       -> this.NotFound({| error = "Game not found" |}) :> IActionResult
        | Some state -> this.Ok(state) :> IActionResult

    [<HttpGet("{id}/history")>]
    member this.GetHistory(id: string) : IActionResult =
        match ser.GetMoveHistory(id) with
        | None    -> this.NotFound({| error = "Game not found" |}) :> IActionResult
        | Some hs -> this.Ok(hs) :> IActionResult

    [<HttpGet("{id}/valid-moves")>]
    member this.GetValidMoves(id: string, x: int, y: int) : IActionResult =
        match ser.GetValidMoves id x y with
        | Error "Game not found" -> this.NotFound({| error = "Game not found" |}) :> IActionResult
        | Error msg              -> this.BadRequest({| error = msg |}) :> IActionResult
        | Ok result              -> this.Ok(result) :> IActionResult

    [<HttpPost("{id}/resign")>]
    member this.Resign(id: string, [<FromBody>] body: ResignRequest) : IActionResult =
        match ser.ResignGame id body.side with
        | Error "Game not found" -> this.NotFound({| error = "Game not found" |}) :> IActionResult
        | Error msg              -> this.BadRequest({| error = msg |}) :> IActionResult
        | Ok state               -> this.Ok(state) :> IActionResult

    [<HttpPost("{id}/draw-offer")>]
    member this.DrawOffer(id: string, [<FromBody>] body: DrawOfferRequest) : IActionResult =
        match ser.OfferDraw id body.side with
        | Error "Game not found" -> this.NotFound({| error = "Game not found" |}) :> IActionResult
        | Error msg              -> this.BadRequest({| error = msg |}) :> IActionResult
        | Ok msg                 -> this.Ok({| message = msg |}) :> IActionResult

    [<HttpPost("{id}/accept-draw")>]
    member this.AcceptDraw(id: string, [<FromBody>] body: DrawOfferRequest) : IActionResult =
        match ser.AcceptDraw id body.side with
        | Error "Game not found" -> this.NotFound({| error = "Game not found" |}) :> IActionResult
        | Error msg              -> this.BadRequest({| error = msg |}) :> IActionResult
        | Ok state               -> this.Ok(state) :> IActionResult
