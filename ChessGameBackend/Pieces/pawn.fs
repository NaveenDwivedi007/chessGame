namespace ChessGameBackend.Pieces

open ChessGameBackend.Game
open ChessGameBackend.Move
open ChessGameBackend.Utils

type PawnMovement() =
    let forwardDir (side: Side) = match side with White -> -1 | Black -> 1
    let startRank (side: Side) = match side with White -> 6 | Black -> 1
    let promotionRank (side: Side) = match side with White -> 0 | Black -> 7

    interface IPieceMovement with
        member _.GetValidMoves (board: Board) (from: TCoordinate) (side: Side) (history: MoveRecord list) =
            let dir = forwardDir side
            let moves = ResizeArray<TCoordinate>()

            // Forward 1
            let oneStep = { x = from.x; y = from.y + dir }
            if isInsideBoard oneStep && isEmpty board oneStep then
                moves.Add(oneStep)
                // Double step from starting rank
                let twoStep = { x = from.x; y = from.y + 2 * dir }
                if from.y = startRank side && isEmpty board twoStep then
                    moves.Add(twoStep)

            // Diagonal captures
            for dx in [ -1; 1 ] do
                let diag = { x = from.x + dx; y = from.y + dir }
                if isInsideBoard diag && isOccupiedByEnemy board diag side then
                    moves.Add(diag)

            // En passant
            match history with
            | lastMove :: _ when lastMove.Piece = Pawn ->
                let movedTwoRanks = abs (lastMove.To.Y - lastMove.From.Y) = 2
                let landedBeside = lastMove.To.Y = from.y && abs (lastMove.To.X - from.x) = 1
                if movedTwoRanks && landedBeside then
                    let epTarget = { x = lastMove.To.X; y = from.y + dir }
                    moves.Add(epTarget)
            | _ -> ()

            moves |> Seq.toList

        member this.IsValidMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            let validMoves = (this :> IPieceMovement).GetValidMoves board from side history
            validMoves |> List.contains target

        member this.ExecuteMove (board: Board) (from: TCoordinate) (target: TCoordinate) (side: Side) (history: MoveRecord list) =
            if not ((this :> IPieceMovement).IsValidMove board from target side history) then None
            else
                let piece = getPieceAt board from
                // En passant capture: remove the captured pawn
                let boardAfterCapture =
                    match history with
                    | lastMove :: _ when lastMove.Piece = Pawn ->
                        let movedTwoRanks = abs (lastMove.To.Y - lastMove.From.Y) = 2
                        let isEnPassant = target.x = lastMove.To.X && target.y = from.y + forwardDir side && movedTwoRanks
                        if isEnPassant then
                            setPieceAt board { x = lastMove.To.X; y = lastMove.To.Y } None
                        else board
                    | _ -> board

                let newBoard = applyMove boardAfterCapture from target piece

                // Promotion: default to Queen
                if target.y = promotionRank side then
                    let promoted = Some { Piece = Queen; Side = side }
                    Some (setPieceAt newBoard target promoted)
                else
                    Some newBoard
