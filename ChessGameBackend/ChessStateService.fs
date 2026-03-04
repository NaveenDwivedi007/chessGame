namespace ChessGameBackend.Services

open System
open ChessGameBackend.Game

type GameRecord = {
    Id: string
    Board: Board
}

type IChessStateService =
    abstract member ValidateMove: string -> bool
    abstract member GetAvailableMoves: unit -> string list
    abstract member GameInit: unit -> GameRecord
    abstract member GetGameBoard: string -> Board option


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

    let mutable gameRecords = Map.empty<string, Board>

    interface IChessStateService with
        member _.ValidateMove(_move) =
            true

        member _.GetAvailableMoves() =
            []

        member _.GameInit() =
            let board = initializeBoard()
            let id = generateId()
            gameRecords <- gameRecords |> Map.add id board
            { Id = id; Board = board }

        member _.GetGameBoard(id) =
            gameRecords.TryFind(id)
