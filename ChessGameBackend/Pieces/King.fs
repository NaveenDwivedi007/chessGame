namespace ChessGameBackend.Pieces

open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Utils

type KingMovement() =
    let directions = [
        (1, 0); (-1, 0); (0, 1); (0, -1)
        (1, 1); (1, -1); (-1, 1); (-1, -1)
    ]

    let kingStartPos (side: Side) = match side with White -> { x = 4; y = 7 } | Black -> { x = 4; y = 0 }
    let kingSideRookPos (side: Side) = match side with White -> { x = 7; y = 7 } | Black -> { x = 7; y = 0 }
    let queenSideRookPos (side: Side) = match side with White -> { x = 0; y = 7 } | Black -> { x = 0; y = 0 }

    let hasNotMoved (history: MoveRecord list) (pos: Coordinate) (piece: Piece) =
        history |> List.exists (fun m -> m.Piece = piece && m.From.X = pos.X && m.From.Y = pos.Y) |> not

    let toCoord (c: TCoordinate) : Coordinate = { X = c.x; Y = c.y }

    /// Check if any enemy piece can attack a square (lightweight, no recursion into king castling)
    let isSquareAttackedByNonKing (board: Board) (target: TCoordinate) (byEnemy: Side) =
        let enemy = byEnemy
        // Check pawns
        let pawnDir = match enemy with White -> 1 | Black -> -1 // pawns attack in their forward dir
        let pawnAttacks =
            [ { x = target.x - 1; y = target.y + pawnDir }; { x = target.x + 1; y = target.y + pawnDir } ]
            |> List.exists (fun p ->
                isInsideBoard p &&
                match getPieceAt board p with Some { Piece = Pawn; Side = s } when s = enemy -> true | _ -> false)
        if pawnAttacks then true
        else
        // Check knights
        let knightOffsets = [ (1,2);(2,1);(2,-1);(1,-2);(-1,-2);(-2,-1);(-2,1);(-1,2) ]
        let knightAttacks =
            knightOffsets
            |> List.exists (fun (dx,dy) ->
                let p = { x = target.x + dx; y = target.y + dy }
                isInsideBoard p &&
                match getPieceAt board p with Some { Piece = Knight; Side = s } when s = enemy -> true | _ -> false)
        if knightAttacks then true
        else
        // Check sliding: rook/queen on rank/file, bishop/queen on diagonals
        let rookDirs = [ (1,0);(-1,0);(0,1);(0,-1) ]
        let bishopDirs = [ (1,1);(1,-1);(-1,1);(-1,-1) ]
        let checkSliding dirs pieces =
            dirs |> List.exists (fun (dx,dy) ->
                let rec scan cx cy =
                    let p = { x = cx; y = cy }
                    if not (isInsideBoard p) then false
                    else
                        match getPieceAt board p with
                        | Some { Side = s } when s <> enemy -> false // blocked by friendly
                        | Some { Piece = pc; Side = s } when s = enemy -> List.contains pc pieces
                        | None -> scan (cx+dx) (cy+dy)
                        | _ -> false
                scan (target.x+dx) (target.y+dy))
        if checkSliding rookDirs [ Rook; Queen ] then true
        elif checkSliding bishopDirs [ Bishop; Queen ] then true
        else
        // Check enemy king (adjacent)
        directions |> List.exists (fun (dx,dy) ->
            let p = { x = target.x + dx; y = target.y + dy }
            isInsideBoard p &&
            match getPieceAt board p with Some { Piece = King; Side = s } when s = enemy -> true | _ -> false)

    interface IPieceMovement with
        member _.GetValidMoves (board: Board) (from: TCoordinate) (side: Side) (history: MoveRecord list) =
            let enemy = oppositeSide side
            let normalMoves =
                directions
                |> List.map (fun (dx, dy) -> { x = from.x + dx; y = from.y + dy })
                |> List.filter (fun pos ->
                    isInsideBoard pos
                    && not (isOccupiedByAlly board pos side)
                    && not (isSquareAttackedByNonKing (applyMove board from pos (getPieceAt board from)) pos enemy))

            let castlingMoves =
                let kStart = kingStartPos side
                if from.x <> kStart.x || from.y <> kStart.y then []
                elif not (hasNotMoved history (toCoord kStart) King) then []
                elif isSquareAttackedByNonKing board from enemy then [] // can't castle out of check
                else
                    let tryCastle rookPos targetX passThroughXs =
                        let rCoord = toCoord rookPos
                        if hasNotMoved history rCoord Rook then
                            // Path between king and rook must be empty
                            let betweenXs =
                                let minX = min (kStart.x + 1) (rookPos.x + 1)
                                let maxX = max (kStart.x - 1) (rookPos.x - 1)
                                [ minX .. maxX ] |> List.filter (fun x -> x <> kStart.x && x <> rookPos.x)
                            let pathClear = betweenXs |> List.forall (fun x -> isEmpty board { x = x; y = from.y })
                            if pathClear then
                                // King must not pass through or land on attacked square
                                let safe = passThroughXs |> List.forall (fun x ->
                                    let pos = { x = x; y = from.y }
                                    not (isSquareAttackedByNonKing (applyMove board from pos (getPieceAt board from)) pos enemy))
                                if safe then [ { x = targetX; y = from.y } ] else []
                            else []
                        else []

                    let kingSide = tryCastle (kingSideRookPos side) 6 [ 5; 6 ]
                    let queenSide = tryCastle (queenSideRookPos side) 2 [ 2; 3 ]
                    kingSide @ queenSide

            normalMoves @ castlingMoves

        member this.IsValidMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            (this :> IPieceMovement).GetValidMoves board from side history |> List.contains target

        member this.ExecuteMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            if not ((this :> IPieceMovement).IsValidMove board from target side history) then None
            else
                let piece = getPieceAt board from
                let newBoard = applyMove board from target piece
                // Handle castling: also move the rook
                let dx = target.x - from.x
                if abs dx = 2 then
                    let rookFrom, rookTo =
                        if dx > 0 then // kingside
                            { x = 7; y = from.y }, { x = 5; y = from.y }
                        else // queenside
                            { x = 0; y = from.y }, { x = 3; y = from.y }
                    let rook = getPieceAt board rookFrom
                    Some (applyMove newBoard rookFrom rookTo rook)
                else
                    Some newBoard
