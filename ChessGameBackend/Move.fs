namespace ChessGameBackend.Move

type TCoordinate = 
    {
        x:int
        y:int
    }

type TPieceMove = 
    {
        gameId:string
        pieceName:string
        moveForm:TCoordinate
        moveTo:TCoordinate
    }
