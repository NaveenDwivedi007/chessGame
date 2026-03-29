namespace ChessGameBackend.Pieces

open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Utils

type BishopMovement() =
    let directions = [ (1, 1); (1, -1); (-1, 1); (-1, -1) ]

    interface IPieceMovement with
        member _.GetValidMoves (board: Board) (from: TCoordinate) (side: Side) (_history: MoveRecord list) =
            directions
            |> List.collect (fun (dx, dy) -> slidingMoves board from side dx dy)

        member this.IsValidMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            (this :> IPieceMovement).GetValidMoves board from side history |> List.contains target

        member this.ExecuteMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            if not ((this :> IPieceMovement).IsValidMove board from target side history) then None
            else
                let piece = getPieceAt board from
                Some (applyMove board from target piece)
