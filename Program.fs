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

// Random number generator
let rnd = Random()

// Color codes for console output
let getColor value n =
    if value = 0 then ConsoleColor.Black
    else
        let logValue = Math.Log(float value) / Math.Log(float n)
        match int logValue with
        | x when x <= 1 -> ConsoleColor.White        // beige
        | x when x <= 2 -> ConsoleColor.Yellow       
        | x when x <= 3 -> ConsoleColor.DarkYellow   // orange
        | x when x <= 4 -> ConsoleColor.Red          
        | _ -> ConsoleColor.Magenta                   // violet

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
                2 * state.N 
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

// Render a single cell with borders and colors
let renderCell value n =
    let color = getColor value n
    let displayValue = if value = 0 then "   " else sprintf "%3d" value
    Console.ForegroundColor <- color
    printf "│%s" displayValue
    Console.ResetColor()

// Render the entire grid
let renderGrid (state: GameState) =
    Console.Clear()
    
    // Top border
    printf "┌"
    for _ in 1 .. state.Q do printf "───┬"
    printfn "───┐" |> ignore
    printf "└" |> ignore
    for _ in 1 .. state.Q - 1 do printf "───┴"
    printfn "───┘" |> ignore
    printf "┌"
    for _ in 1 .. state.Q do printf "───┬"
    printfn "───┐"
    
    // Grid content
    for i in 0 .. state.P - 1 do
        for j in 0 .. state.Q do
            if j < state.Q then
                renderCell state.Grid.[i].[j] state.N
            else
                printf "│"
        printfn ""
        
        // Horizontal separator (except for last row)
        if i < state.P - 1 then
            printf "├"
            for j in 0 .. state.Q - 1 do
                printf "───"
                if j < state.Q - 1 then printf "┼" else printf "┤"
            printfn ""
    
    // Bottom border
    printf "└"
    for j in 0 .. state.Q - 1 do
        printf "───"
        if j < state.Q - 1 then printf "┴" else printf "┘"
    printfn ""

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
    | "left" -> 
        { state with Grid = merge state.N grid }
    | "right" ->
        let rotated = rotate >> rotate
        { state with Grid = grid |> rotated |> merge state.N |> rotated }
    | "down" ->
        let transform = rotate >> merge state.N >> rotate >> rotate >> rotate
        { state with Grid = transform grid }
    | "up" ->
        let transform = rotate >> rotate >> rotate >> merge state.N >> rotate
        { state with Grid = transform grid }
    | _ -> state

// Check if game is over (no moves possible)
let isGameOver state =
    let directions = ["left"; "right"; "up"; "down"]
    directions |> List.forall (fun dir -> 
        let newGrid = (transformState dir state).Grid
        gridsEqual state.Grid newGrid)

// Check win condition
let checkWin state =
    let target = pown state.N state.M
    state.Grid |> Array.exists (Array.exists ((=) target))

// Get user input
let getUserInput () =
    printf "Use arrow keys to move, 'q' to quit: "
    let key = Console.ReadKey(true)
    match key.Key with
    | ConsoleKey.LeftArrow -> Some "left"
    | ConsoleKey.RightArrow -> Some "right"
    | ConsoleKey.UpArrow -> Some "up"
    | ConsoleKey.DownArrow -> Some "down"
    | ConsoleKey.Q -> 
        printf "\nAre you sure you want to quit? (y/N): "
        let confirm = Console.ReadLine()
        if confirm.ToLower() = "y" then None else Some "continue"
    | _ -> Some "invalid"

// Main game loop (recursive)
let rec gameLoop state =
    // Spawn number
    let stateWithNumber = spawnNumber state
    
    // Render
    renderGrid stateWithNumber
    
    // Check win condition
    let hasWonNow = checkWin stateWithNumber
    let updatedState = { stateWithNumber with HasWon = stateWithNumber.HasWon || hasWonNow }
    
    if hasWonNow && not stateWithNumber.HasWon then
        Console.ForegroundColor <- ConsoleColor.Green
        printfn "\n🎉 CONGRATULATIONS! You reached %d! You won the game! 🎉" (pown updatedState.N updatedState.M)
        Console.ResetColor()
        printfn "You can continue playing..."
        System.Threading.Thread.Sleep(2000)
    
    // Check game over
    if isGameOver updatedState then
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "\n💀 GAME OVER! No more moves possible."
        Console.ResetColor()
    else
        // Get user input
        match getUserInput() with
        | None -> printfn "\nThanks for playing!"
        | Some "continue" -> gameLoop updatedState
        | Some "invalid" -> 
            printfn "\nInvalid input. Use arrow keys or 'q' to quit."
            gameLoop updatedState
        | Some direction ->
            // Transform state
            let oldGrid = updatedState.Grid
            let newState = transformState direction updatedState
            
            // Only proceed if the move actually changed something
            if not (gridsEqual oldGrid newState.Grid) then
                // Animate transformation
                renderAnimated updatedState newState
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
