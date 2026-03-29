module ChessGameBackend.Pieces.CheckValidator

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

let isSquareAttacked (board: Board) (target: TCoordinate) (byEnemy: Side) (history: MoveRecord list) =
    board
    |> List.indexed
    |> List.exists (fun (y, row) ->
        row
        |> List.indexed
        |> List.exists (fun (x, sq) ->
            match sq with
            | Some { Piece = piece; Side = side } when side = byEnemy ->
                let from = { x = x; y = y }
                let handler = getPieceMovement piece
                handler.GetValidMoves board from side history |> List.contains target
            | _ -> false
        )
    )

let isKingInCheck (board: Board) (side: Side) (history: MoveRecord list) =
    match findKing board side with
    | Some kingPos -> isSquareAttacked board kingPos (oppositeSide side) history
    | None -> false

let wouldMoveLeaveKingInCheck (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
    let piece = getPieceAt board from
    let tempBoard = applyMove board from target piece
    isKingInCheck tempBoard side history

let getAllLegalMoves (board: Board) (side: Side) (history: MoveRecord list) =
    board
    |> List.indexed
    |> List.collect (fun (y, row) ->
        row
        |> List.indexed
        |> List.collect (fun (x, sq) ->
            match sq with
            | Some { Piece = piece; Side = s } when s = side ->
                let from = { x = x; y = y }
                let handler = getPieceMovement piece
                handler.GetValidMoves board from side history
                |> List.filter (fun target ->
                    not (wouldMoveLeaveKingInCheck board from target side history))
                |> List.map (fun target -> (from, target))
            | _ -> []
        )
    )

let isCheckmate (board: Board) (side: Side) (history: MoveRecord list) =
    isKingInCheck board side history && getAllLegalMoves board side history |> List.isEmpty

let isStalemate (board: Board) (side: Side) (history: MoveRecord list) =
    not (isKingInCheck board side history) && getAllLegalMoves board side history |> List.isEmpty
