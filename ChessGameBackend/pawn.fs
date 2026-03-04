namespace ChessGameBackend.Pieces

open ChessGameBackend.Move
open ChessGameBackend.Game

type PawnControl() =

    member _.isValidMove(currentPos: TCoordinate, targetPos: TCoordinate, board: Board) =
        match board.[currentPos.y].[currentPos.x] with
        | Some { Piece = Pawn; Side = side } ->
            let expectedY =
                match side with
                | White -> currentPos.y - 1
                | Black -> currentPos.y + 1
            targetPos.x = currentPos.x && targetPos.y = expectedY
        | _ -> false
    
    member _.movePawnToNewLocation (currentPos: TCoordinate, targetPos: TCoordinate, board: Board) = 
        if(this.isValidMove(currentPos,targetPos,board)) then {
            let piece = board.[currentPos.y].[currentPos.x]
            let newBoard = board |> List.mapi (fun y row ->
                row |> List.mapi (fun x square ->
                    if x = currentPos.x && y = currentPos.y then None
                    else if x = targetPos.x && y = targetPos.y then piece
                    else square
                )
            )
            Some newBoard
        }
        else None
        






        
        
