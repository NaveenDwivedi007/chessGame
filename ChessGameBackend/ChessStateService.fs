namespace ChessGameBackend.Services

open System
open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Pieces.MoveDispatcher

type GameState = {
    Board: Board
    MoveHistory: MoveRecord list
    CurrentTurn: Side
    Status: GameStatus
    DrawOffer: Side option
}

type GameRecord = {
    Id: string
    Board: Board
}

type MoveResponse = {
    Board: Board
    IsCheck: bool
    IsCheckmate: bool
    IsStalemate: bool
}

type GameStateResponse = {
    Id: string
    Board: Board
    CurrentTurn: string
    Status: string
    Winner: string
    IsCheck: bool
    MoveCount: int
}

type ValidMovesResponse = {
    ValidSquares: Coordinate list
}

type IChessStateService =
    abstract member GameInit: unit -> GameRecord
    abstract member GetGameBoard: string -> Board option
    abstract member MakeMove: string -> TCoordinate -> TCoordinate -> Piece option -> Result<MoveResponse, string>
    abstract member GetGameState: string -> GameStateResponse option
    abstract member GetMoveHistory: string -> MoveRecord list option
    abstract member GetValidMoves: string -> int -> int -> Result<ValidMovesResponse, string>
    abstract member ResignGame: string -> string -> Result<GameStateResponse, string>
    abstract member OfferDraw: string -> string -> Result<string, string>
    abstract member AcceptDraw: string -> string -> Result<GameStateResponse, string>

type ChessStateService() =
    let square side piece : option<Square>= Some { Piece = piece; Side = side }

    let initializeBoard () =
        let backRank = [ Rook; Knight; Bishop; Queen; King; Bishop; Knight; Rook ]
        let backRankFor side = backRank |> List.map (square side)
        let pawnRankFor side = List.replicate 8 (square side Pawn)
        let emptyRow : option<Square> list = List.replicate 8 None

        [
            backRankFor Black
            pawnRankFor Black
            emptyRow; emptyRow; emptyRow; emptyRow
            pawnRankFor White
            backRankFor White
        ]

    let generateId () = Guid.NewGuid().ToString()

    let mutable gameStates = Map.empty<string, GameState>

    let sideToString s = match s with White -> "White" | Black -> "Black"
    let oppSideStr  s = match s with White -> "Black" | Black -> "White"

    let parseSide (s: string) =
        match s.ToLowerInvariant() with
        | "white" -> Some White
        | "black" -> Some Black
        | _       -> None

    let buildResponse (id: string) (state: GameState) : GameStateResponse =
        let statusStr, winner =
            match state.Status with
            | InProgress       -> "InProgress", null
            | Checkmate loser  -> "Checkmate",  oppSideStr loser
            | Stalemate        -> "Stalemate",  null
            | Resigned loser   -> "Resigned",   oppSideStr loser
            | Draw             -> "Draw",        null
        let isCheck = ChessGameBackend.Pieces.CheckValidator.isKingInCheck state.Board state.CurrentTurn state.MoveHistory
        { Id          = id
          Board       = state.Board
          CurrentTurn = sideToString state.CurrentTurn
          Status      = statusStr
          Winner      = winner
          IsCheck     = isCheck
          MoveCount   = state.MoveHistory.Length }

    interface IChessStateService with
        member _.GameInit() =
            let board = initializeBoard()
            let id = generateId()
            gameStates <- gameStates |> Map.add id { Board = board; MoveHistory = []; CurrentTurn = White; Status = InProgress; DrawOffer = None }
            { Id = id; Board = board }

        member _.GetGameBoard(id) =
            gameStates.TryFind(id) |> Option.map (fun gs -> gs.Board)

        member _.MakeMove (gameId: string) (from: TCoordinate) (target: TCoordinate) (promotionPiece: Piece option) =
            match gameStates.TryFind(gameId) with
            | None -> Error "Game not found"
            | Some state ->
                match state.Status with
                | InProgress ->
                    let side = state.CurrentTurn
                    match tryExecuteMove state.Board from target side state.MoveHistory promotionPiece with
                    | NoPieceAtSource -> Error "No piece at source position"
                    | InvalidMove -> Error "Invalid move for this piece"
                    | WouldLeaveKingInCheck -> Error "Move would leave your king in check"
                    | Success newBoard ->
                        let piece =
                            match ChessGameBackend.Utils.getPieceAt state.Board from with
                            | Some sq -> sq.Piece
                            | None -> Pawn // should not happen

                        let moveRecord : MoveRecord = {
                            Piece = piece
                            Side = side
                            From = { X = from.x; Y = from.y }
                            To = { X = target.x; Y = target.y }
                            MoveNumber = state.MoveHistory.Length + 1
                        }

                        let nextSide = match side with White -> Black | Black -> White
                        let newHistory = moveRecord :: state.MoveHistory

                        let isCheck = ChessGameBackend.Pieces.CheckValidator.isKingInCheck newBoard nextSide newHistory
                        let isCheckmate = ChessGameBackend.Pieces.CheckValidator.isCheckmate newBoard nextSide newHistory
                        let isStalemate = ChessGameBackend.Pieces.CheckValidator.isStalemate newBoard nextSide newHistory

                        let newStatus =
                            if isCheckmate then Checkmate nextSide
                            elif isStalemate then Stalemate
                            else InProgress

                        let newState = { Board = newBoard; MoveHistory = newHistory; CurrentTurn = nextSide; Status = newStatus; DrawOffer = state.DrawOffer }
                        gameStates <- gameStates |> Map.add gameId newState

                        Ok { Board = newBoard; IsCheck = isCheck; IsCheckmate = isCheckmate; IsStalemate = isStalemate }
                | _ -> Error "Game is already over"

        member _.GetGameState (gameId: string) =
            gameStates.TryFind(gameId) |> Option.map (buildResponse gameId)

        member _.GetMoveHistory (gameId: string) =
            gameStates.TryFind(gameId) |> Option.map (fun s -> List.rev s.MoveHistory)

        member _.GetValidMoves (gameId: string) (x: int) (y: int) =
            match gameStates.TryFind(gameId) with
            | None -> Error "Game not found"
            | Some state ->
                let pos = { x = x; y = y }
                match ChessGameBackend.Utils.getPieceAt state.Board pos with
                | None -> Error "No piece at that square"
                | Some sq when sq.Side <> state.CurrentTurn -> Error "It is not that piece's turn"
                | _ ->
                    let squares =
                        getValidMovesForPiece state.Board pos state.CurrentTurn state.MoveHistory
                        |> List.map (fun c -> { X = c.x; Y = c.y })
                    Ok { ValidSquares = squares }

        member _.ResignGame (gameId: string) (sideStr: string) =
            match gameStates.TryFind(gameId) with
            | None -> Error "Game not found"
            | Some state ->
                match state.Status with
                | InProgress ->
                    match parseSide sideStr with
                    | None -> Error "Invalid side. Use 'White' or 'Black'"
                    | Some side ->
                        let newState = { state with Status = Resigned side }
                        gameStates <- gameStates |> Map.add gameId newState
                        Ok (buildResponse gameId newState)
                | _ -> Error "Game is already over"

        member _.OfferDraw (gameId: string) (sideStr: string) =
            match gameStates.TryFind(gameId) with
            | None -> Error "Game not found"
            | Some state ->
                match state.Status with
                | InProgress ->
                    match parseSide sideStr with
                    | None -> Error "Invalid side. Use 'White' or 'Black'"
                    | Some side ->
                        match state.DrawOffer with
                        | Some _ -> Error "A draw offer is already pending"
                        | None ->
                            gameStates <- gameStates |> Map.add gameId { state with DrawOffer = Some side }
                            Ok (sprintf "Draw offered by %s" (sideToString side))
                | _ -> Error "Game is already over"

        member _.AcceptDraw (gameId: string) (sideStr: string) =
            match gameStates.TryFind(gameId) with
            | None -> Error "Game not found"
            | Some state ->
                match state.Status with
                | InProgress ->
                    match parseSide sideStr with
                    | None -> Error "Invalid side. Use 'White' or 'Black'"
                    | Some side ->
                        match state.DrawOffer with
                        | None -> Error "No draw offer is pending"
                        | Some offeringSide when offeringSide = side -> Error "Cannot accept your own draw offer"
                        | Some _ ->
                            let newState = { state with Status = Draw; DrawOffer = None }
                            gameStates <- gameStates |> Map.add gameId newState
                            Ok (buildResponse gameId newState)
                | _ -> Error "Game is already over"
