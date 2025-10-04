module picosmos.MNPQ

open System
open System.CommandLine
open picosmos.CommandLineInterface

type GridArray = int[][]

type GameState = {
    Grid: GridArray
    N: int
    M: int
    P: int
    Q: int
    HasWon: bool
    TurnsPlayed: int
}

type GameStatus = 
    | Playing
    | Won
    | GameOver

type Direction =
    | Left
    | Right
    | Up
    | Down

let rnd = Random()

let safePowerLong m n =
    try
        let result = Math.Pow(float m, float n)
        if result > float Int64.MaxValue then Int64.MaxValue
        else int64 result
    with
    | _ -> Int64.MaxValue

let getColorCode value m n =
    if value = 0 then Colors.rgb 64 64 64  // Dark gray for empty cells
    else
        let target = safePowerLong m n
        let doubleTarget = safePowerLong m (2 * n)
        
        if int64 value <= target then
            // Gradient from beige to red for values up to m^n
            let logValue = Math.Log(float value) / Math.Log(float m)
            let progress = logValue / (float n)
            let t = max 0.0 (min 1.0 progress)
            
            // Color stops: Beige → Light Orange → Orange → Red → Dark Red
            let (r, g, b) = 
                if t <= 0.25 then
                    Colors.interpolateRgb (245, 245, 220) (255, 218, 185) (t * 4.0)  // Beige to Peach
                elif t <= 0.5 then
                    Colors.interpolateRgb (255, 218, 185) (255, 165, 0) ((t - 0.25) * 4.0)  // Peach to Orange
                elif t <= 0.75 then
                    Colors.interpolateRgb (255, 165, 0) (255, 69, 0) ((t - 0.5) * 4.0)  // Orange to Red-Orange
                else
                    Colors.interpolateRgb (255, 69, 0) (139, 0, 0) ((t - 0.75) * 4.0)  // Red-Orange to Dark Red
            
            Colors.rgb r g b
        elif int64 value <= doubleTarget then
            // Gradient from violet to deep purple for values above m^n
            let logValue = Math.Log(float value) / Math.Log(float m)
            let progress = (logValue - (float n)) / (float n)
            let t = max 0.0 (min 1.0 progress)

            let (r, g, b) = Colors.interpolateRgb (148, 0, 211) (75, 0, 130) t  // Violet to Indigo
            Colors.rgb r g b
        else
            // Electric colors for extremely high values
            let logValue = Math.Log(float value) / Math.Log(float m)
            let progress = (logValue - (float (2 * n))) / (float n)
            let t = max 0.0 (min 1.0 progress)
            
            let (r, g, b) = Colors.interpolateRgb (0, 191, 255) (0, 255, 255) t  // Deep Sky Blue to Cyan
            Colors.rgb r g b

let getColorAnsi value m n = getColorCode value m n

let getCellWidth (grid: GridArray) =
    let maxValue = 
        grid 
        |> Array.collect id 
        |> Array.max
    let digits = maxValue.ToString().Length
    max 3 digits

let updateAt index value (arr: 'a[]) : 'a[] =
    arr |> Array.mapi (fun i x -> if i = index then value else x)

let update2D row col value (grid: GridArray) =
    grid |> Array.mapi (fun i arr -> 
        if i = row then updateAt col value arr else arr)

let spawnNumber (state: GameState) : GameState =
    let emptyCells = 
        state.Grid
        |> Seq.mapi (fun i row ->
            row |> Seq.mapi (fun j value -> if value = 0 then Some (i, j) else None))
        |> Seq.collect (Seq.choose id)
        |> Seq.toList
    
    if emptyCells.IsEmpty then state
    else
        let (row, col) = emptyCells.[rnd.Next(emptyCells.Length)]
        let value = 
            if rnd.NextDouble() < (1.0 / (float state.N ** float state.N)) then 
                state.N * state.N 
            else 
                state.N
        { state with Grid = update2D row col value state.Grid }

let rotate (grid: GridArray) : GridArray =
    let rows = Array.length grid
    let cols = Array.length grid.[0]
    Array.init cols (fun j -> 
        Array.init rows (fun i -> grid.[rows - 1 - i].[j]))

let merge n (grid: GridArray) : GridArray =
    let mergeRow n (row: int[]) =
        let nonZeros = row |> Array.filter ((<>) 0)
        let rec merge acc remaining =
            match remaining with
            | [] -> acc
            | [x] -> acc @ [x]
            | x :: xs ->
                let consecutiveCount = xs |> List.takeWhile ((=) x) |> List.length
                if consecutiveCount + 1 >= n then
                    let newValue = x * n
                    let remainingAfterMerge = xs |> List.skip (n - 1)
                    merge (acc @ [newValue]) remainingAfterMerge
                else
                    merge (acc @ [x]) xs
        
        let merged = merge [] (Array.toList nonZeros)
        let result = Array.create row.Length 0
        merged |> List.iteri (fun i v -> result.[i] <- v)
        result
    grid |> Array.map (mergeRow n)

let borderColor = Colors.rgb 128 128 128

let renderGridAsString (state: GameState) : string = 
    let sb = Text.StringBuilder()
    let cellWidth = getCellWidth state.Grid
    let borderWidth = String.replicate cellWidth "─"
    let borderSegments = 
        state.Grid.[0] 
        |> Array.map (fun _ -> borderWidth)

    let renderTopBorder() =
        sb.Append(sprintf "%s┌%s┐%s" borderColor (String.Join("┬", borderSegments)) Colors.reset) |> ignore
        sb.AppendLine() |> ignore

    let renderGridContent() =
        let renderCellContent value m n cellWidth =
            let colorCode = getColorAnsi value m n
            let displayValue = 
                if value = 0 then 
                    String.replicate cellWidth " " 
                else 
                    let valueStr = value.ToString()
                    valueStr.PadLeft(cellWidth)
            sprintf "%s%s%s" colorCode displayValue Colors.reset

        let middleBorderLine = sprintf "%s├%s┤%s" borderColor (String.Join("┼", borderSegments)) Colors.reset
        
        let rowLines = 
            state.Grid
            |> Array.map (fun row ->
                let cellContents = row |> Array.map (fun value -> renderCellContent value state.M state.N cellWidth)
                sprintf "%s│%s%s│%s" borderColor (String.Join(sprintf "%s│%s" borderColor Colors.reset, cellContents)) borderColor Colors.reset)
        
        sb.Append(String.Join(sprintf "\n%s\n" middleBorderLine, rowLines)) |> ignore
        sb.AppendLine() |> ignore
    
    let renderBottomBorder() =
        sb.Append(sprintf "%s└%s┘%s" borderColor (String.Join("┴", borderSegments)) Colors.reset) |> ignore
        sb.AppendLine() |> ignore
    
    renderTopBorder()
    renderGridContent()
    renderBottomBorder()

    sb.ToString()

let renderGrid (state: GameState) : unit =
    let output = Screen.clearScreen + renderGridAsString state
    Console.Write output
    Console.Out.Flush()

let renderAnimated oldState newState =
    // todo: implement
    ()

let gridsEqual (grid1: GridArray) (grid2: GridArray) =
    Array.forall2 (Array.forall2 (=)) grid1 grid2

let transformState direction state =
    let transformation = 
        match direction with
        | Left -> merge state.N
        | Right -> rotate >> rotate >> merge state.N >> rotate >> rotate
        | Down -> rotate >> merge state.N >> rotate >> rotate >> rotate
        | Up -> rotate >> rotate >> rotate >> merge state.N >> rotate
    { state with Grid = transformation state.Grid }

let isGameOver state =
    let directions = [Left; Right; Up; Down]
    directions |> List.forall (fun dir -> 
        let newGrid = (transformState dir state).Grid
        gridsEqual state.Grid newGrid)

let checkWin state =
    let target = safePowerLong state.N state.M
    state.Grid |> Array.exists (Array.exists (fun v -> int64 v = target))

type InputResult =
    | Move of Direction
    | Quit
    | Continue
    | Invalid

type GameResult = {
    FinalState: GameState
    Outcome: GameOutcome
    MaxValue: int
    TurnsPlayed: int
}

and GameOutcome =
    | PlayerWon
    | PlayerQuit  
    | GameOver


// Get user input (clears buffer first to prevent input accumulation)
let getUserInput () =
    printf "Use arrow keys to move, 'q' to quit: "
    let key = Console.ReadKey(true)
        
    match key.Key with
    | ConsoleKey.LeftArrow -> Move Left
    | ConsoleKey.RightArrow -> Move Right
    | ConsoleKey.UpArrow -> Move Up
    | ConsoleKey.DownArrow -> Move Down
    | ConsoleKey.Q -> 
        printf "\nAre you sure you want to quit? (y/N): "
        let confirm = Console.ReadLine()
        if confirm.ToLower() = "y" then Quit else Continue
    | _ -> Invalid

// Input loop that repeats until a valid transformation occurs
let rec getValidMove state =
    match getUserInput() with
    | Quit -> None
    | Continue -> getValidMove state
    | Invalid -> 
        printfn "\nInvalid input. Use arrow keys or 'q' to quit."
        getValidMove state
    | Move direction ->
        // Transform state
        let oldGrid = state.Grid
        let newState = transformState direction state
        
        // Check if the move actually changed something
        if not (gridsEqual oldGrid newState.Grid) then
            Some newState  // Return the transformed state
        else
            printfn "\nNo change possible in that direction."
            getValidMove state  // Ask for input again

// Main game loop (recursive) - now returns GameResult
let rec gameLoop state =
    Input.clearInputBuffer()
    let stateWithNumber = spawnNumber state
    
    renderGrid stateWithNumber
    
    // Check win condition
    let hasWonNow = checkWin stateWithNumber
    let updatedState = { stateWithNumber with HasWon = stateWithNumber.HasWon || hasWonNow }
    
    if hasWonNow && not stateWithNumber.HasWon then
        Console.ForegroundColor <- ConsoleColor.Green
        printfn "\nCONGRATULATIONS! You reached %d! You won the game!" (safePowerLong updatedState.N updatedState.M)
        Console.ResetColor()
        printfn "You can continue playing..."
        printfn "Press any key to continue..."
        Console.ReadKey() |> ignore
    
    // Check game over
    if isGameOver updatedState then
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "\nGAME OVER! No more moves possible."
        Console.ResetColor()
        let maxValue = updatedState.Grid |> Array.collect id |> Array.max
        { FinalState = updatedState; Outcome = GameOver; MaxValue = maxValue; TurnsPlayed = updatedState.TurnsPlayed }
    else
        match getValidMove updatedState with
        | None -> 
            printfn "\nThanks for playing!"
            let maxValue = updatedState.Grid |> Array.collect id |> Array.max
            let outcome = if updatedState.HasWon then PlayerWon else PlayerQuit
            { FinalState = updatedState; Outcome = outcome; MaxValue = maxValue; TurnsPlayed = updatedState.TurnsPlayed }
        | Some newState ->
            renderAnimated updatedState newState
            gameLoop { newState with TurnsPlayed = newState.TurnsPlayed + 1 }

// Initialize game state
let initializeGame m n p q =
    let emptyGrid = Array.init p (fun _ -> Array.create q 0)
    let initialState = {
        Grid = emptyGrid
        N = m
        M = n 
        P = p
        Q = q
        HasWon = false
        TurnsPlayed = 0
    }
    // Spawn two initial numbers
    initialState |> spawnNumber |> spawnNumber

// Print game results to standard screen buffer
let printGameResult (result: GameResult) =
    printfn ""
    printfn "%s" (String.replicate 50 "=")
    printfn "MNPQ GAME COMPLETED"
    printfn "%s" (String.replicate 50 "=")
    
    match result.Outcome with
    | PlayerWon ->
        Console.ForegroundColor <- ConsoleColor.Green
        printfn "VICTORY! You reached the target!"
        Console.ResetColor()
    | PlayerQuit ->
        Console.ForegroundColor <- ConsoleColor.Yellow
        printfn "Game ended by player choice"
        Console.ResetColor()
    | GameOver ->
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "Game Over - No more moves possible"
        Console.ResetColor()
    
    printfn "\nFinal Game Board:"
    printf "%s" (renderGridAsString result.FinalState)
    
    printfn "\nGame Statistics:"
    printfn " - Turns played: %d" result.TurnsPlayed
    printfn " - Highest tile: %d" result.MaxValue
    printfn " - Target value: %d" (safePowerLong result.FinalState.N result.FinalState.M)
    printfn " - Grid size: %dx%d" result.FinalState.P result.FinalState.Q
    printfn " - Base number: %d" result.FinalState.N
    
    if result.FinalState.HasWon then
        printfn " - Achievement: Winner! 🏆"
    
    printfn "%s" (String.replicate 50 "=")
    printfn ""

// Validate command line parameters
let validateParameters m n p q =
    let errors = [
        if m < 2 then yield sprintf "Base number (m) must be >= 2, got %d" m
        if n < 2 then yield sprintf "Winner exponent (n) must be >= 2, got %d" n
        if p < m then yield sprintf "Grid height (p) must be >= m (%d), got %d" m p
        if q < m then yield sprintf "Grid width (q) must be >= m (%d), got %d" m q
    ]
    
    if not errors.IsEmpty then
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "Parameter validation failed:"
        errors |> List.iter (printfn " - %s")
        Console.ResetColor()
        printfn "\nGame requirements:"
        printfn " - Base number (m) >= 2 (need at least binary merge)"
        printfn " - Winner exponent (n) >= 2 (meaningful target)"
        printfn " - Grid height (p) >= m (fit merge sequences)"
        printfn " - Grid width (q) >= m (fit merge sequences)"
        false
    else
        true

[<EntryPoint>]
let main args =
    let mOption = Option<int>([|"--base"; "-b"; "-m"|], getDefaultValue = (fun () -> 2), description = "Base number")
    let nOption = Option<int>([|"--exponent"; "-e"; "-n"|], getDefaultValue = (fun () -> 11), description = "Winner exponent")
    let pOption = Option<int>([|"--height"; "-h"; "-p" |], getDefaultValue = (fun () -> 4), description = "Grid height")
    let qOption = Option<int>([|"--width"; "-w"; "-q"|], getDefaultValue = (fun () -> 4), description = "Grid width")

    let rootCommand = RootCommand("MNPQ - A customizable 2048-like game")
    rootCommand.AddOption(mOption) 
    rootCommand.AddOption(nOption)
    rootCommand.AddOption(pOption)
    rootCommand.AddOption(qOption)
    
    rootCommand.SetHandler(Action<int, int, int, int>(fun n m p q ->
        if validateParameters m n p q then
            printfn "Starting MNPQ game with n=%d, m=%d, p=%d, q=%d" n m p q
            printfn "Goal: Reach %d to win!" (safePowerLong m n)
            printfn "Press any key to start..."
            Console.ReadKey() |> ignore
            
            Screen.enterFullScreen()
            
            let mutable gameResult = None
            Console.CancelKeyPress.Add(fun args ->
                Screen.exitFullScreen()
                gameResult |> Option.iter printGameResult
                args.Cancel <- false
            )
            
            try
                let initialState = initializeGame m n p q
                let result = gameLoop initialState
                gameResult <- Some result
                result
            finally
                Screen.exitFullScreen()
            |> printGameResult
        else
            printfn "\nUse --help for more information about valid parameter ranges."
    ), nOption, mOption, pOption, qOption)
    
    rootCommand.Invoke(args)
