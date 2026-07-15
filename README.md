# SharpBoy

**Game Boy Emulator written in C#**

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)

> 🎮 A lightweight, accurate Game Boy emulator built from scratch in C#.

---

## ✨ Features

- ✅ Full Game Boy CPU emulation (LR35902) — 500+ instructions
- ✅ PPU with Background, Window, and Sprites
- ✅ APU with 4 sound channels (Pulse 1/2, Wave, Noise)
- ✅ MBC1, MBC2, MBC3 (with RTC), MBC5 support
- ✅ Timer, Joypad, Interrupts, DMA
- ✅ Boot ROM support (optional)
- ✅ Save/Load States (binary, versioned)
- ✅ 60 FPS limiter
- ✅ Clean, modular architecture

---

## 🖥️ How to Use

### Download
### Run

Drag and drop a `.gb` or `.gbc` ROM file onto `SharpBoy.exe`, or use the command line:

```bash
SharpBoy.exe pokemon.gb
