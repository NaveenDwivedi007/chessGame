namespace ChessGameBackend.Services

open System
open ChessGameBackend.Move
type Pieces = Pawn = 'p' | King ='K' | Queen = 'q'|Knight = 'k' | Rook = 'r' | Bishop = 'b'
type Sides = White = 'l' | Black = 'd'
type TBoard = ((string|null) list) list

type GameRecord = {
    id: string
    board:TBoard
}

type IChessStateService =
    abstract member ValidateMove: string -> bool
    abstract member GetAvailableMoves: unit -> string list
    abstract member GameInit: unit -> GameRecord
    abstract member GetGameBoard: string -> TBoard


type ChessStateService() =
    let initialize_board () = 
        let pawn_list = [0..7] |> List.map(fun _ -> Pieces.Pawn)
        let empty_space = [0..7] |> List.map(fun _ -> null)
        let piece_list = [for i in 0..7 do
                            if i = 0 || i = 7 then yield Pieces.Rook
                            if i=1 || i = 6 then yield Pieces.Knight
                            if i = 2 || i = 5 then yield Pieces.Bishop
                            if i = 4 then yield Pieces.Knight
                            if i = 3 then yield Pieces.Queen
                            ]
        let make_piece (side:Sides,row: Pieces List) =row |> List.map(fun piece -> side.ToString()+"_"+piece.ToString())
        let game_init_state = [0..7] |> List.map(function 
                                                        | row when row = 0 -> make_piece(Sides.Black, piece_list)
                                                        | row when row = 1 -> make_piece(Sides.Black, pawn_list)
                                                        | row when row = 6 -> make_piece(Sides.White, pawn_list)
                                                        | row when row = 7 -> make_piece(Sides.White, piece_list)
                                                        | row -> empty_space
                                                        )
                                                          
        printfn "len %i" (game_init_state.Length)
        game_init_state

    let random_id_gen () = Guid.NewGuid().ToString()

    let mutable Game_Record = Map.empty<string,TBoard>

    interface IChessStateService with
        member _.ValidateMove(move: string) =
            true
            
        member _.GetAvailableMoves() =
            []
        member _.GameInit() = 
            let board = initialize_board()
            let id = random_id_gen()
            Game_Record <- Game_Record |> Map.add id board
            {
                id=id
                board = board
            }

        member _.GetGameBoard(move: string) =
            initialize_board()
