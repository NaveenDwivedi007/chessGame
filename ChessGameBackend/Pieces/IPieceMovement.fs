namespace ChessGameBackend.Pieces

open ChessGameBackend.Game
open ChessGameBackend.Move

type IPieceMovement =
    abstract member GetValidMoves: Board -> TCoordinate -> Side -> MoveRecord list -> TCoordinate list
    abstract member IsValidMove: Board -> TCoordinate -> TCoordinate -> Side -> MoveRecord list -> bool
    abstract member ExecuteMove: Board -> TCoordinate -> TCoordinate -> Side -> MoveRecord list -> Board option
