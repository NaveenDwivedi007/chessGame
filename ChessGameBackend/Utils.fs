namespace ChessGameBackend.Utils

open System
open ChessGameBackend.Game
open ChessGameBackend.Move

interface IUtils = 
    abstract member getItemFromBoard: Board*TCoordinate -> string option


type Utils () =
    interface IUtils with
        member _.getItemFromBoard(board, coordinate) =
            let inline outside v = v < 0 || v > 7
            if outside coordinate.x || outside coordinate.y then
                invalidArg (nameof coordinate) "Invalid coordinate"
            else
                board.[coordinate.y].[coordinate.x] |> Option.ofObj
                


