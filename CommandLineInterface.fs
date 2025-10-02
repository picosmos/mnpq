namespace picosmos.CommandLineInterface

// Modern ANSI color system with RGB support
module Colors =
    // ANSI escape codes for 24-bit RGB colors
    let rgb r g b = sprintf "\x1b[38;2;%d;%d;%dm" r g b
    let bgRgb r g b = sprintf "\x1b[48;2;%d;%d;%dm" r g b
    let reset = "\x1b[0m"
    
    // Interpolate between two RGB colors
    let interpolateRgb (r1, g1, b1) (r2, g2, b2) t =
        let calcComponent a b t = int (float a + t * (float b - float a))
        (calcComponent r1 r2 t, calcComponent g1 g2 t, calcComponent b1 b2 t)
    
    
module Screen =
    open System
    // ANSI escape sequences for alternate screen buffer
    let enterAlternateScreen = "\x1b[?1049h"  // Enter alternate screen buffer
    let exitAlternateScreen = "\x1b[?1049l"   // Exit alternate screen buffer
    let clearScreen = "\x1b[2J\x1b[H"         // Clear screen and move cursor to top-left
    let hideCursor = "\x1b[?25l"              // Hide cursor
    let showCursor = "\x1b[?25h"              // Show cursor
    
    // Initialize full-screen mode
    let enterFullScreen () =
        printf "%s%s%s" enterAlternateScreen clearScreen hideCursor
        Console.Out.Flush()
    
    // Exit full-screen mode and restore terminal
    let exitFullScreen () =
        printf "%s%s" showCursor exitAlternateScreen
        Console.Out.Flush()
