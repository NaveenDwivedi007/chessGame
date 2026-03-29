namespace ChessGameBackend.Game

type Piece = Pawn | King | Queen | Knight | Rook | Bishop
type Side = White | Black

type Square = { Piece: Piece; Side: Side }
type Board = option<Square> list list

type Coordinate = { X: int; Y: int }

type MoveRecord = {
    Piece: Piece
    Side: Side
    From: Coordinate
    To: Coordinate
    MoveNumber: int
}

type GameBoard = {
    Board: Board
    Id: string
}