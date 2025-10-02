# MNPQ - A Customizable 2048-like Game

A terminal-based puzzle game written in F# with beautiful RGB colors and full-screen gameplay experience.

## Quick Start

### Clone and Run

```bash
# Clone the repository
git clone https://github.com/picosmos/mnpq.git
cd mnpq

# Build and run with default settings
dotnet run
```

### Default Game (2048-like)
```bash
dotnet run
```
- **Goal**: Reach 2048 (2¹¹) to win
- **Grid**: 4×4 
- **Base number**: 2
- **Spawns**: 2 or 4 (rare)

## Game Parameters

The game accepts four customizable parameters:

| Parameter | Flag | Default | Description |
|-----------|------|---------|-------------|
| **Base Number** | `-m` | `2` | Number used for spawning and merging |
| **Winner Exponent** | `-n` | `11` | Target is n^m (default: 2¹¹ = 2048) |
| **Grid Height** | `-p` | `4` | Number of rows |
| **Grid Width** | `-q` | `4` | Number of columns |

### Parameter Rules
- `n >= 2` (minimum binary merge)
- `n >= 2` (meaningful target)  
- `p >= n` (fit merge sequences)
- `q >= n` (fit merge sequences)

## Game Examples

### Classic 2048
```bash
dotnet run -- -n 2 -n 11 -p 4 -q 4
```
**Target**: 2048 on a 4×4 grid

This is the same as running just `dotnet run`.

### Easy Mode (Smaller Target)
```bash
dotnet run -- -m 2 -n 6 -p 4 -q 4
```
**Target**: 64 on a 4×4 grid

### Hard Mode (Larger Grid)
```bash
dotnet run -- -m 2 -n 11 -p 6 -q 6
```
**Target**: 2048 on a 6×6 grid

### Custom Game (Base 3)
```bash
dotnet run -- -m 3 -n 6 -p 5 -q 5
```
**Target**: 81 (3⁶) on a 5×5 grid with base-3 merging

### Extreme Challenge
```bash
dotnet run -- -m 4 -n 4 -p 4 -q 4
```
**Target**: 4096 (4⁶) on a 4×4 grid

## Features

- **Full-Screen Mode**: Clean gameplay like `nano` or `vim`
- **RGB Colors**: Beautiful color gradients for different tile values
- **Animated Moves**: Smooth transitions between game states
- **Result Summary**: Detailed statistics after each game
- **Colored Final Board**: Game results with full color preservation
- **Input Buffer Management**: Prevents rapid key press issues

## Controls

- **Arrow Keys**: Move tiles
- **Q**: Quit game (with confirmation)
- **Ctrl+C**: Emergency exit (restores terminal)

## Requirements

- .NET 6.0 or later
- Terminal with ANSI color support
- Linux, macOS, or Windows with modern terminal

## Game Statistics

After each game, you'll see:
- **Victory status**: Win/quit/game over
- **Turn count**: Number of moves made
- **Highest tile**: Maximum value achieved
- **Target value**: The winning number
- **Grid dimensions**: Playing field size
- **Base number**: Merge multiplier used

## Help

```bash
dotnet run -- --help
```

Shows all available options and their descriptions.

---

**Enjoy the game!** 🎮✨