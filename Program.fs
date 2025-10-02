open System
open System.CommandLine
open System.CommandLine.Parsing

// Game state types
type GameState = {
    Grid: int[][]
    N: int
    M: int
    P: int
    Q: int
    HasWon: bool
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

// Random number generator
let rnd = Random()

// Modern ANSI color system with RGB support
module Colors =
    // ANSI escape codes for 24-bit RGB colors
    let rgb r g b = sprintf "\x1b[38;2;%d;%d;%dm" r g b
    let bgRgb r g b = sprintf "\x1b[48;2;%d;%d;%dm" r g b
    let reset = "\x1b[0m"
    
    // Interpolate between two RGB colors
    let interpolateRgb (r1, g1, b1) (r2, g2, b2) t =
        let lerp a b t = int (float a + t * (float b - float a))
        (lerp r1 r2 t, lerp g1 g2 t, lerp b1 b2 t)
    
    // Color palette for different value ranges
    let getColorCode value n m =
        if value = 0 then rgb 64 64 64  // Dark gray for empty cells
        else
            let target = pown n m
            let doubleTarget = pown n (2 * m)
            
            if value <= target then
                // Gradient from beige to red for values up to n^m
                let logValue = Math.Log(float value) / Math.Log(float n)
                let progress = logValue / (float m)
                let t = max 0.0 (min 1.0 progress)
                
                // Color stops: Beige → Light Orange → Orange → Red → Dark Red
                let (r, g, b) = 
                    if t <= 0.25 then
                        interpolateRgb (245, 245, 220) (255, 218, 185) (t * 4.0)  // Beige to Peach
                    elif t <= 0.5 then
                        interpolateRgb (255, 218, 185) (255, 165, 0) ((t - 0.25) * 4.0)  // Peach to Orange
                    elif t <= 0.75 then
                        interpolateRgb (255, 165, 0) (255, 69, 0) ((t - 0.5) * 4.0)  // Orange to Red-Orange
                    else
                        interpolateRgb (255, 69, 0) (139, 0, 0) ((t - 0.75) * 4.0)  // Red-Orange to Dark Red
                
                rgb r g b
            elif value <= doubleTarget then
                // Gradient from violet to deep purple for values above n^m
                let logValue = Math.Log(float value) / Math.Log(float n)
                let progress = (logValue - (float m)) / (float m)
                let t = max 0.0 (min 1.0 progress)
                
                let (r, g, b) = interpolateRgb (148, 0, 211) (75, 0, 130) t  // Violet to Indigo
                rgb r g b
            else
                // Electric colors for extremely high values
                let logValue = Math.Log(float value) / Math.Log(float n)
                let progress = (logValue - (float (2 * m))) / (float m)
                let t = max 0.0 (min 1.0 progress)
                
                let (r, g, b) = interpolateRgb (0, 191, 255) (0, 255, 255) t  // Deep Sky Blue to Cyan
                rgb r g b

// Enhanced color function that returns ANSI codes
let getColorAnsi value n m = Colors.getColorCode value n m

// Calculate appropriate cell width based on the largest number in the grid
let getCellWidth (grid: int[][]) =
    let maxValue = 
        grid 
        |> Array.collect id 
        |> Array.max
    if maxValue = 0 then 3
    else
        let digits = maxValue.ToString().Length
        max 3 digits

// Helper functions for immutable array operations
let updateAt index value (arr: 'a[]) =
    arr |> Array.mapi (fun i x -> if i = index then value else x)

let update2D row col value (grid: int[][]) =
    grid |> Array.mapi (fun i arr -> 
        if i = row then updateAt col value arr else arr)

// Spawn a new number in a random empty cell
let spawnNumber (state: GameState) =
    let emptyCells = [
        for i in 0 .. state.P - 1 do
            for j in 0 .. state.Q - 1 do
                if state.Grid.[i].[j] = 0 then yield (i, j)
    ]
    
    if emptyCells.IsEmpty then state
    else
        let (row, col) = emptyCells.[rnd.Next(emptyCells.Length)]
        let value = 
            if rnd.NextDouble() < (1.0 / (float state.N ** float state.N)) then 
                state.N * state.N 
            else 
                state.N
        { state with Grid = update2D row col value state.Grid }

// Rotate grid 90 degrees clockwise
let rotate (grid: int[][]) =
    let rows = Array.length grid
    let cols = Array.length grid.[0]
    Array.init cols (fun j -> 
        Array.init rows (fun i -> grid.[rows - 1 - i].[j]))

// Merge a single row according to game rules
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

// Merge entire grid
let merge n (grid: int[][]) =
    grid |> Array.map (mergeRow n)

// Render a single cell with borders and colors using ANSI
let renderCell value n m cellWidth =
    let colorCode = getColorAnsi value n m
    let displayValue = 
        if value = 0 then 
            String.replicate cellWidth " " 
        else 
            let valueStr = value.ToString()
            valueStr.PadLeft(cellWidth)
    
    // Render border in medium grey, content with RGB color
    printf "%s│%s%s%s" (Colors.rgb 128 128 128) colorCode displayValue Colors.reset

// Render the entire grid with modern ANSI colors
let renderGrid (state: GameState) =
    Console.Clear()
    
    let cellWidth = getCellWidth state.Grid
    let borderWidth = String.replicate cellWidth "─"
    let greyColor = Colors.rgb 128 128 128  // Medium grey for grid borders
    
    // Top border
    printf "%s┌" greyColor
    for j in 0 .. state.Q - 1 do
        printf "%s" borderWidth
        if j < state.Q - 1 then printf "┬" else printf "┐"
    printfn "%s" Colors.reset
    
    // Grid content
    for i in 0 .. state.P - 1 do
        for j in 0 .. state.Q do
            if j < state.Q then
                renderCell state.Grid.[i].[j] state.N state.M cellWidth
            else
                printf "%s│%s" greyColor Colors.reset
        printfn ""
        
        // Horizontal separator (except for last row)
        if i < state.P - 1 then
            printf "%s├" greyColor
            for j in 0 .. state.Q - 1 do
                printf "%s" borderWidth
                if j < state.Q - 1 then printf "┼" else printf "┤"
            printfn "%s" Colors.reset
    
    // Bottom border
    printf "%s└" greyColor
    for j in 0 .. state.Q - 1 do
        printf "%s" borderWidth
        if j < state.Q - 1 then printf "┴" else printf "┘"
    printfn "%s" Colors.reset

// Animated render (simplified - shows before and after states)
let renderAnimated oldState newState =
    renderGrid oldState
    System.Threading.Thread.Sleep(150)
    renderGrid newState
    System.Threading.Thread.Sleep(150)

// Check if two grids are equal
let gridsEqual (grid1: int[][]) (grid2: int[][]) =
    Array.forall2 (Array.forall2 (=)) grid1 grid2

// Transform state based on direction
let transformState direction state =
    let grid = state.Grid
    match direction with
    | Left -> 
        { state with Grid = merge state.N grid }
    | Right ->
        let rotated = rotate >> rotate
        { state with Grid = grid |> rotated |> merge state.N |> rotated }
    | Down ->
        let transform = rotate >> merge state.N >> rotate >> rotate >> rotate
        { state with Grid = transform grid }
    | Up ->
        let transform = rotate >> rotate >> rotate >> merge state.N >> rotate
        { state with Grid = transform grid }

// Check if game is over (no moves possible)
let isGameOver state =
    let directions = [Left; Right; Up; Down]
    directions |> List.forall (fun dir -> 
        let newGrid = (transformState dir state).Grid
        gridsEqual state.Grid newGrid)

// Check win condition
let checkWin state =
    let target = pown state.N state.M
    state.Grid |> Array.exists (Array.exists ((=) target))

// Input result type
type InputResult =
    | Move of Direction
    | Quit
    | Continue
    | Invalid

// Clear any buffered input from the console
let clearInputBuffer () =
    while Console.KeyAvailable do
        Console.ReadKey(true) |> ignore

// Get user input (clears buffer first to prevent input accumulation)
let getUserInput () =
    // Clear any buffered keys first
    clearInputBuffer()
    
    printf "Use arrow keys to move, 'q' to quit: "
    let key = Console.ReadKey(true)
    
    // Clear buffer again after reading to prevent double input
    clearInputBuffer()
    
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

// Main game loop (recursive)
let rec gameLoop state =
    // Clear any residual input at start of each loop iteration
    clearInputBuffer()
    
    // Spawn number
    let stateWithNumber = spawnNumber state
    
    // Render
    renderGrid stateWithNumber
    
    // Check win condition
    let hasWonNow = checkWin stateWithNumber
    let updatedState = { stateWithNumber with HasWon = stateWithNumber.HasWon || hasWonNow }
    
    if hasWonNow && not stateWithNumber.HasWon then
        Console.ForegroundColor <- ConsoleColor.Green
        printfn "\nCONGRATULATIONS! You reached %d! You won the game!" (pown updatedState.N updatedState.M)
        Console.ResetColor()
        printfn "You can continue playing..."
        System.Threading.Thread.Sleep(2000)
    
    // Check game over
    if isGameOver updatedState then
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "\nGAME OVER! No more moves possible."
        Console.ResetColor()
    else
        // Get user input
        match getUserInput() with
        | Quit -> printfn "\nThanks for playing!"
        | Continue -> gameLoop updatedState
        | Invalid -> 
            printfn "\nInvalid input. Use arrow keys or 'q' to quit."
            gameLoop updatedState
        | Move direction ->
            // Transform state
            let oldGrid = updatedState.Grid
            let newState = transformState direction updatedState
            
            // Only proceed if the move actually changed something
            if not (gridsEqual oldGrid newState.Grid) then
                // Animate transformation
                renderAnimated updatedState newState
                // Clear any accumulated input after animation
                clearInputBuffer()
                // Continue game loop
                gameLoop newState
            else
                printfn "\nNo change possible in that direction."
                gameLoop updatedState

// Initialize game state
let initializeGame n m p q =
    let emptyGrid = Array.init p (fun _ -> Array.create q 0)
    let initialState = {
        Grid = emptyGrid
        N = n
        M = m 
        P = p
        Q = q
        HasWon = false
    }
    // Spawn two initial numbers
    initialState |> spawnNumber |> spawnNumber

// Main entry point
[<EntryPoint>]
let main args =
    let nOption = Option<int>("-n", getDefaultValue = (fun () -> 2), description = "Base number (default: 2)")
    let mOption = Option<int>("-m", getDefaultValue = (fun () -> 11), description = "Winner exponent (default: 11)")
    let pOption = Option<int>("-p", getDefaultValue = (fun () -> 4), description = "Grid height (default: 4)")
    let qOption = Option<int>("-q", getDefaultValue = (fun () -> 4), description = "Grid width (default: 4)")
    
    let rootCommand = RootCommand("MNPQ - A customizable 2048-like game")
    rootCommand.AddOption(nOption)
    rootCommand.AddOption(mOption) 
    rootCommand.AddOption(pOption)
    rootCommand.AddOption(qOption)
    
    rootCommand.SetHandler(Action<int, int, int, int>(fun n m p q ->
        printfn "Starting MNPQ game with n=%d, m=%d, p=%d, q=%d" n m p q
        printfn "Goal: Reach %d to win!" (pown n m)
        printfn "Press any key to start..."
        Console.ReadKey() |> ignore
        
        let initialState = initializeGame n m p q
        gameLoop initialState
    ), nOption, mOption, pOption, qOption)
    
    rootCommand.Invoke(args)
