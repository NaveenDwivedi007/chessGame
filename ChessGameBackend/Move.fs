namespace ChessGameBackend.Move

open System



type WeatherForecast1 =
    { 
      TemperatureC: int
      Summary: string
      id: int
      }

    member this.TemperatureF =
        32.0 + (float this.TemperatureC / 0.5556)