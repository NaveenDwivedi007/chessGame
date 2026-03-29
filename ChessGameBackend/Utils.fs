module ChessGameBackend.Utils

open ChessGameBackend.Game
open ChessGameBackend.Move

let isInsideBoard (coord: TCoordinate) =
    coord.x >= 0 && coord.x <= 7 && coord.y >= 0 && coord.y <= 7

let getPieceAt (board: Board) (coord: TCoordinate) : option<Square> =
    if not (isInsideBoard coord) then None
    else board.[coord.y].[coord.x]

let isEmpty (board: Board) (coord: TCoordinate) =
    isInsideBoard coord && board.[coord.y].[coord.x] = None

let isOccupiedByAlly (board: Board) (coord: TCoordinate) (side: Side) =
    match getPieceAt board coord with
    | Some sq -> sq.Side = side
    | None -> false

let isOccupiedByEnemy (board: Board) (coord: TCoordinate) (side: Side) =
    match getPieceAt board coord with
    | Some sq -> sq.Side <> side
    | None -> false

let oppositeSide (side: Side) =
    match side with White -> Black | Black -> White

let applyMove (board: Board) (from: TCoordinate) (target: TCoordinate) (piece: option<Square>) : Board =
    board
    |> List.mapi (fun y row ->
        row |> List.mapi (fun x square ->
            if x = from.x && y = from.y then None
            elif x = target.x && y = target.y then piece
            else square
        )
    )

let setPieceAt (board: Board) (coord: TCoordinate) (piece: option<Square>) : Board =
    board
    |> List.mapi (fun y row ->
        row |> List.mapi (fun x square ->
            if x = coord.x && y = coord.y then piece
            else square
        )
    )

let findKing (board: Board) (side: Side) : TCoordinate option =
    board
    |> List.indexed
    |> List.tryPick (fun (y, row) ->
        row
        |> List.indexed
        |> List.tryPick (fun (x, sq) ->
            match sq with
            | Some { Piece = King; Side = s } when s = side -> Some { x = x; y = y }
            | _ -> None
        )
    )

/// Generate moves along a direction until blocked
let slidingMoves (board: Board) (from: TCoordinate) (side: Side) (dx: int) (dy: int) : TCoordinate list =
    let rec loop x y acc =
        let pos = { x = x; y = y }
        if not (isInsideBoard pos) then acc
        elif isOccupiedByAlly board pos side then acc
        elif isOccupiedByEnemy board pos side then pos :: acc
        else loop (x + dx) (y + dy) (pos :: acc)
    loop (from.x + dx) (from.y + dy) []
