namespace ChessGameBackend.Pieces

open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Utils

type KnightMovement() =
    let offsets = [
        (1, 2); (2, 1); (2, -1); (1, -2)
        (-1, -2); (-2, -1); (-2, 1); (-1, 2)
    ]

    interface IPieceMovement with
        member _.GetValidMoves (board: Board) (from: TCoordinate) (side: Side) (_history: MoveRecord list) =
            offsets
            |> List.map (fun (dx, dy) -> { x = from.x + dx; y = from.y + dy })
            |> List.filter (fun pos -> isInsideBoard pos && not (isOccupiedByAlly board pos side))

        member this.IsValidMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            (this :> IPieceMovement).GetValidMoves board from side history |> List.contains target

        member this.ExecuteMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            if not ((this :> IPieceMovement).IsValidMove board from target side history) then None
            else
                let piece = getPieceAt board from
                Some (applyMove board from target piece)
