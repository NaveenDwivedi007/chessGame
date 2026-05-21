namespace ChessGameBackend.Move

[<CLIMutable>]
type TCoordinate = 
    {
        x: int
        y: int
    }

[<CLIMutable>]
type TPieceMove = 
    {
        gameId: string
        pieceName: string
        moveForm: TCoordinate
        moveTo: TCoordinate
        promotionPiece: string  // null when not a promotion move
    }

[<CLIMutable>]
type ResignRequest = { side: string }

[<CLIMutable>]
type DrawOfferRequest = { side: string }
