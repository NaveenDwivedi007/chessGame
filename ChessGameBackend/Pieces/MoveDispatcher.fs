module ChessGameBackend.Pieces.MoveDispatcher

open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Utils

let private getPieceMovement (piece: Piece) : IPieceMovement =
    match piece with
    | Pawn   -> PawnMovement()   :> IPieceMovement
    | Knight -> KnightMovement() :> IPieceMovement
    | Bishop -> BishopMovement() :> IPieceMovement
    | Rook   -> RookMovement()   :> IPieceMovement
    | Queen  -> QueenMovement()  :> IPieceMovement
    | King   -> KingMovement()   :> IPieceMovement

type MoveResult =
    | Success of Board
    | InvalidMove
    | NoPieceAtSource
    | WouldLeaveKingInCheck

let tryExecuteMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) : MoveResult =
    match getPieceAt board from with
    | None -> NoPieceAtSource
    | Some sq when sq.Side <> side -> NoPieceAtSource
    | Some sq ->
        let handler = getPieceMovement sq.Piece
        if not (handler.IsValidMove board from target side history) then
            InvalidMove
        else
            match handler.ExecuteMove board from target side history with
            | None -> InvalidMove
            | Some newBoard ->
                if CheckValidator.isKingInCheck newBoard side history then
                    WouldLeaveKingInCheck
                else
                    Success newBoard

let getValidMovesForPiece (board: Board) (from: TCoordinate) (side: Side) (history: MoveRecord list) : TCoordinate list =
    match getPieceAt board from with
    | None -> []
    | Some sq when sq.Side <> side -> []
    | Some sq ->
        let handler = getPieceMovement sq.Piece
        handler.GetValidMoves board from side history
        |> List.filter (fun target ->
            not (CheckValidator.wouldMoveLeaveKingInCheck board from target side history))
