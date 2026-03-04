namespace ChessGameBackend.Game

type Piece = Pawn | King | Queen | Knight | Rook | Bishop
type Side = White | Black

type Square = { Piece: Piece; Side: Side }
type Board = option<Square> list list

type GameBoard = {
    Board: Board
    Id: string
}