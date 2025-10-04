# MNPQ - A Customizable 2048-like Game

A terminal-based puzzle game written in F# with beautiful RGB colors and full-screen gameplay experience.

## Quick Start

### Download Pre-built Binary (Recommended)

Choose your platform and download and run the latest release:

#### Linux x64
```bash
curl -L -o mnpg https://github.com/picosmos/mnpq/releases/download/release/mnpq.linux.x64
chmod +x mnpg
./mnpg
```

#### Linux ARM64
```bash
curl -L -o mnpg https://github.com/picosmos/mnpq/releases/download/release/mnpq.linux.arm64
chmod +x mnpg
./mnpg
```

#### macOS x64 (Intel)
```bash
curl -L -o mnpg https://github.com/picosmos/mnpq/releases/download/release/mnpq.osx.x64
chmod +x mnpg
./mnpg
```

#### macOS ARM64 (Apple Silicon)
```bash
curl -L -o mnpg https://github.com/picosmos/mnpq/releases/download/release/mnpq.osx.arm64
chmod +x mnpg
./mnpg
```

#### Windows x64
```powershell
# Using PowerShell
Invoke-WebRequest -Uri "https://github.com/picosmos/mnpq/releases/download/release/mnpq.win.x64.exe" -OutFile "mnpg.exe"
./mnpg.exe
```

#### Using winget (Windows)
```bash
# Coming soon - winget package pending
winget install picosmos.mnpq
```

### Build from Source

```bash
# Clone the repository
git clone https://github.com/picosmos/mnpq.git
cd mnpq

# Build and run with default settings
dotnet run
```

### Default Game (2048-like)
```bash
./mnpg
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
./mnpg -n 2 -n 11 -p 4 -q 4
```
**Target**: 2048 on a 4×4 grid

This is the same as running just `./mnpg`.

### Easy Mode (Smaller Target)
```bash
./mnpg -m 2 -n 6 -p 4 -q 4
```
**Target**: 64 on a 4×4 grid

### Hard Mode (Larger Grid)
```bash
./mnpg -m 2 -n 11 -p 6 -q 6
```
**Target**: 2048 on a 6×6 grid

### Custom Game (Base 3)
```bash
./mnpg -m 3 -n 6 -p 5 -q 5
```
**Target**: 81 (3⁶) on a 5×5 grid with base-3 merging

### Extreme Challenge
```bash
./mnpg -m 4 -n 4 -p 4 -q 4
```
**Target**: 256 (4⁴) on a 4×4 grid

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
./mnpg --help
```

Shows all available options and their descriptions.

## File Integrity

You can verify the downloaded files using their SHA256 checksums:

- **mnpq.linux.arm64**: `3cd265374cee6f8c36d2209b488ca96f9eee288e6e432954de330c194f02d78d`
- **mnpq.linux.x64**: `4066590a37862e0feb105557ded2e57d48c310d918d3f259bef90524ca9010ab`
- **mnpq.osx.arm64**: `1c667fca606bccf85c00ebbcd738ebc05f4c549a214d4e7298938d35de5129ed`
- **mnpq.osx.x64**: `906b6ec955f309f6023d80d51542d5acde2379023ee41864dd15ad6a641f97dd`
- **mnpq.win.x64.exe**: `7403ae6c517b513164b6807d2d9588080bc1aa97a51cade2d031f4da93b29414`

```bash
# Verify checksum on Linux/macOS
sha256sum mnpg

# Verify checksum on Windows (PowerShell)
Get-FileHash mnpg.exe -Algorithm SHA256
```

---

**Enjoy the game!** 🎮✨