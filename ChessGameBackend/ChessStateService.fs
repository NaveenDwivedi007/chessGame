namespace ChessGameBackend.Services

open System
open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Pieces.MoveDispatcher

type GameState = {
    Board: Board
    MoveHistory: MoveRecord list
    CurrentTurn: Side
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

type IChessStateService =
    abstract member GameInit: unit -> GameRecord
    abstract member GetGameBoard: string -> Board option
    abstract member MakeMove: string -> TCoordinate -> TCoordinate -> Result<MoveResponse, string>

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

    interface IChessStateService with
        member _.GameInit() =
            let board = initializeBoard()
            let id = generateId()
            gameStates <- gameStates |> Map.add id { Board = board; MoveHistory = []; CurrentTurn = White }
            { Id = id; Board = board }

        member _.GetGameBoard(id) =
            gameStates.TryFind(id) |> Option.map (fun gs -> gs.Board)

        member _.MakeMove (gameId: string) (from: TCoordinate) (target: TCoordinate) =
            match gameStates.TryFind(gameId) with
            | None -> Error "Game not found"
            | Some state ->
                let side = state.CurrentTurn
                match tryExecuteMove state.Board from target side state.MoveHistory with
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
                    let newState = { Board = newBoard; MoveHistory = newHistory; CurrentTurn = nextSide }
                    gameStates <- gameStates |> Map.add gameId newState

                    let isCheck = ChessGameBackend.Pieces.CheckValidator.isKingInCheck newBoard nextSide newHistory
                    let isCheckmate = ChessGameBackend.Pieces.CheckValidator.isCheckmate newBoard nextSide newHistory
                    let isStalemate = ChessGameBackend.Pieces.CheckValidator.isStalemate newBoard nextSide newHistory

                    Ok { Board = newBoard; IsCheck = isCheck; IsCheckmate = isCheckmate; IsStalemate = isStalemate }
