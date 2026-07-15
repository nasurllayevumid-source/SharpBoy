using System;

namespace GameBoyEmulator.Core;

public class PPU
{
    private readonly Memory _memory;
    private readonly Registers _registers;

    private int _scanline;
    private int _cycles;
    private int _mode;
    private int _modeCycles;
    private bool _frameReady;
    private bool _lcdEnabled;
    private int _windowLineCounter;

    private const int WIDTH = 160;
    private const int HEIGHT = 144;
    private const int CYCLES_PER_SCANLINE = 456;
    private const int SCANLINES_VBLANK = 144;
    private const int SCANLINES_TOTAL = 154;

    private byte _lcdc;
    private byte _stat;
    private byte _scy;
    private byte _scx;
    private byte _ly;
    private byte _lyc;
    private byte _bgp;
    private byte _obp0;
    private byte _obp1;
    private byte _wy;
    private byte _wx;

    private byte[,] _frameBuffer;
    private byte[] _bgColors;
    private byte[] _windowColors;
    private byte[] _spriteColors;
    private byte[] _spritePalettes;
    private bool[] _spritePriorities;
    private int[] _spriteXCache;

    private bool _dmaTransfer;
    private int _dmaCycles;
    private byte _dmaStart;

    public bool FrameReady => _frameReady;
    public byte[,] FrameBuffer => _frameBuffer;

    public PPU(Memory memory, Registers registers)
    {
        _memory = memory;
        _registers = registers;
        _frameBuffer = new byte[HEIGHT, WIDTH];
        _bgColors = new byte[WIDTH];
        _windowColors = new byte[WIDTH];
        _spriteColors = new byte[WIDTH];
        _spritePalettes = new byte[WIDTH];
        _spritePriorities = new bool[WIDTH];
        _spriteXCache = new int[40];
        _scanline = 0;
        _cycles = 0;
        _mode = 0;
        _modeCycles = 0;
        _frameReady = false;
        _dmaTransfer = false;
        _lcdEnabled = true;
        _windowLineCounter = 0;
        UpdateRegisters();
    }

    public void Step(int cycles)
    {
        UpdateRegisters();

        if (!_lcdEnabled)
        {
            _cycles += cycles;
            return;
        }

        if (_dmaTransfer)
        {
            _dmaCycles += cycles;
            if (_dmaCycles >= 160)
            {
                _dmaTransfer = false;
                _dmaCycles = 0;
                PerformDMA();
            }
        }

        _cycles += cycles;

        while (_cycles > 0)
        {
            int modeTime = 0;

            switch (_mode)
            {
                case 0: modeTime = 51; break;
                case 1: modeTime = 114; break;
                case 2: modeTime = 20; break;
                case 3: modeTime = 43; break;
            }

            int remaining = Math.Min(_cycles, modeTime - _modeCycles);
            _modeCycles += remaining;
            _cycles -= remaining;

            if (_modeCycles >= modeTime)
            {
                _modeCycles -= modeTime;
                UpdateMode();
            }
        }

        if (_scanline >= SCANLINES_TOTAL)
        {
            _scanline = 0;
            _windowLineCounter = 0;
            _frameReady = true;
        }

        _ly = (byte)_scanline;
        _memory.WriteIO(0x44, _ly);

        if (_ly == _lyc)
        {
            _stat |= 0x04;
            if ((_stat & 0x40) != 0)
            {
                _memory.SetInterruptFlag((byte)(_memory.GetInterruptFlag() | 0x02));
            }
        }
        else
        {
            _stat &= 0xFB;
        }

        _stat = (byte)((_stat & 0xF8) | (_mode & 0x03));
        _memory.WriteIO(0x41, _stat);
    }

    private void UpdateMode()
    {
        switch (_mode)
        {
            case 0:
                _scanline++;
                if (_scanline == HEIGHT)
                {
                    _mode = 1;
                    _memory.SetInterruptFlag((byte)(_memory.GetInterruptFlag() | 0x01));
                    if ((_stat & 0x10) != 0)
                    {
                        _memory.SetInterruptFlag((byte)(_memory.GetInterruptFlag() | 0x02));
                    }
                }
                else
                {
                    _mode = 2;
                    if ((_stat & 0x20) != 0)
                    {
                        _memory.SetInterruptFlag((byte)(_memory.GetInterruptFlag() | 0x02));
                    }
                }
                break;

            case 2:
                _mode = 3;
                break;

            case 3:
                _mode = 0;
                RenderScanline(_scanline);
                if ((_stat & 0x08) != 0)
                {
                    _memory.SetInterruptFlag((byte)(_memory.GetInterruptFlag() | 0x02));
                }
                break;

            case 1:
                _scanline++;
                if (_scanline >= SCANLINES_TOTAL)
                {
                    _mode = 2;
                    _scanline = 0;
                    _windowLineCounter = 0;
                    if ((_stat & 0x20) != 0)
                    {
                        _memory.SetInterruptFlag((byte)(_memory.GetInterruptFlag() | 0x02));
                    }
                }
                else
                {
                    _mode = 1;
                }
                break;
        }

        if (_scanline >= HEIGHT && _mode != 1)
        {
            _mode = 1;
        }

        if (_scanline < HEIGHT && _mode == 1)
        {
            _mode = 0;
        }

        _modeCycles = 0;
    }

    private void UpdateRegisters()
    {
        byte newLcdc = _memory.ReadIO(0x40);
        if ((newLcdc & 0x80) != (_lcdc & 0x80))
        {
            if ((newLcdc & 0x80) == 0)
            {
                _lcdEnabled = false;
                _scanline = 0;
                _mode = 0;
                _modeCycles = 0;
                _windowLineCounter = 0;
                _memory.WriteIO(0x44, 0);
            }
            else
            {
                _lcdEnabled = true;
                _windowLineCounter = 0;
            }
        }

        _lcdc = newLcdc;
        _stat = _memory.ReadIO(0x41);
        _scy = _memory.ReadIO(0x42);
        _scx = _memory.ReadIO(0x43);
        _ly = _memory.ReadIO(0x44);
        _lyc = _memory.ReadIO(0x45);
        _bgp = _memory.ReadIO(0x47);
        _obp0 = _memory.ReadIO(0x48);
        _obp1 = _memory.ReadIO(0x49);
        _wy = _memory.ReadIO(0x4A);
        _wx = _memory.ReadIO(0x4B);

        if (_ly == _lyc)
        {
            _stat |= 0x04;
        }
        else
        {
            _stat &= 0xFB;
        }
        _memory.WriteIO(0x41, _stat);
    }

    public void RefreshRegisters()
    {
        UpdateRegisters();
    }

    public void ResetFrameReady()
    {
        _frameReady = false;
    }

    private void RenderScanline(int scanline)
    {
        if (!_lcdEnabled || !IsLcdEnabled())
        {
            for (int x = 0; x < WIDTH; x++)
            {
                _frameBuffer[scanline, x] = 0;
            }
            return;
        }

        bool bgEnabled = IsBGEnabled();
        bool windowEnabled = IsWindowEnabled();
        bool spriteEnabled = IsSpriteEnabled();

        Array.Clear(_bgColors, 0, WIDTH);
        Array.Clear(_windowColors, 0, WIDTH);
        Array.Clear(_spriteColors, 0, WIDTH);
        Array.Clear(_spritePalettes, 0, WIDTH);
        Array.Clear(_spritePriorities, 0, WIDTH);

        if (bgEnabled)
        {
            RenderBackground(scanline, _bgColors);
        }

        if (windowEnabled)
        {
            RenderWindow(scanline, _windowColors);
        }

        if (spriteEnabled)
        {
            RenderSprites(scanline, _spriteColors, _spritePalettes, _spritePriorities);
        }

        for (int x = 0; x < WIDTH; x++)
        {
            byte finalColor = _bgColors[x];

            if (windowEnabled && scanline >= _wy && x >= _wx - 7)
            {
                finalColor = _windowColors[x];
            }

            if (spriteEnabled && _spriteColors[x] != 0)
            {
                if (!_spritePriorities[x] || finalColor == 0)
                {
                    finalColor = _spriteColors[x];
                }
            }

            _frameBuffer[scanline, x] = finalColor;
        }
    }

    private void RenderBackground(int scanline, byte[] output)
    {
        int mapY = (scanline + _scy) & 0xFF;
        int tileY = mapY / 8;
        int offsetY = mapY % 8;

        ushort tileMapBase = GetTileMapBase();

        for (int x = 0; x < WIDTH; x++)
        {
            int mapX = (x + _scx) & 0xFF;
            int tileX = mapX / 8;
            int offsetX = mapX % 8;

            int tileIndex = _memory.Read((ushort)(tileMapBase + tileY * 32 + tileX));
            output[x] = ReadTilePixel(tileIndex, offsetX, offsetY);
        }
    }

    private void RenderWindow(int scanline, byte[] output)
    {
        if (scanline < _wy) return;

        int mapY = _windowLineCounter;
        int tileY = mapY / 8;
        int offsetY = mapY % 8;

        ushort tileMapBase = GetWindowTileMapBase();

        for (int x = 0; x < WIDTH; x++)
        {
            if (x + 7 < _wx) continue;

            int mapX = x + 7 - _wx;
            int tileX = mapX / 8;
            int offsetX = mapX % 8;

            int tileIndex = _memory.Read((ushort)(tileMapBase + tileY * 32 + tileX));
            output[x] = ReadTilePixel(tileIndex, offsetX, offsetY);
        }

        _windowLineCounter++;
    }

    private byte ReadTilePixel(int tileIndex, int offsetX, int offsetY)
    {
        ushort tileDataBase = GetTileDataBase();

        if (tileDataBase == 0x8800)
        {
            tileIndex = (sbyte)tileIndex;
            int tileAddress = 0x9000 + tileIndex * 16 + offsetY * 2;
            byte tileLow = _memory.Read((ushort)tileAddress);
            byte tileHigh = _memory.Read((ushort)(tileAddress + 1));
            int bit = 7 - offsetX;
            int pixel = ((tileHigh >> bit) & 1) << 1;
            pixel |= (tileLow >> bit) & 1;
            return GetColorFromPalette(pixel, _bgp);
        }
        else
        {
            int tileAddress = tileDataBase + tileIndex * 16 + offsetY * 2;
            byte tileLow = _memory.Read((ushort)tileAddress);
            byte tileHigh = _memory.Read((ushort)(tileAddress + 1));
            int bit = 7 - offsetX;
            int pixel = ((tileHigh >> bit) & 1) << 1;
            pixel |= (tileLow >> bit) & 1;
            return GetColorFromPalette(pixel, _bgp);
        }
    }

    private void RenderSprites(int scanline, byte[] output, byte[] palettes, bool[] priorities)
    {
        int spriteHeight = IsSpriteDoubleSize() ? 16 : 8;
        int spriteCount = 0;
        int[] spriteOrder = new int[40];

        for (int i = 0; i < 40; i++)
        {
            spriteOrder[i] = i;
            byte spriteX = _memory.Read((ushort)(0xFE00 + i * 4 + 1));
            _spriteXCache[i] = spriteX;
        }

        Array.Sort(spriteOrder, (a, b) =>
        {
            if (_spriteXCache[a] != _spriteXCache[b])
                return _spriteXCache[a].CompareTo(_spriteXCache[b]);
            return a.CompareTo(b);
        });

        for (int idx = 0; idx < 40 && spriteCount < 10; idx++)
        {
            int i = spriteOrder[idx];
            int address = 0xFE00 + i * 4;
            byte spriteY = _memory.Read((ushort)address);
            byte spriteX = _memory.Read((ushort)(address + 1));
            byte tileIndex = _memory.Read((ushort)(address + 2));
            byte attributes = _memory.Read((ushort)(address + 3));

            int yPos = spriteY - 16;
            int xPos = spriteX - 8;

            int yOffset = scanline - yPos;
            if (yOffset < 0 || yOffset >= spriteHeight) continue;

            spriteCount++;

            bool yFlip = (attributes & 0x40) != 0;
            bool xFlip = (attributes & 0x20) != 0;
            byte palette = (attributes & 0x10) != 0 ? _obp1 : _obp0;
            bool priority = (attributes & 0x80) != 0;

            int realYOffset = yFlip ? (spriteHeight - 1 - yOffset) : yOffset;

            if (spriteHeight == 16)
            {
                tileIndex &= 0xFE;
                if (realYOffset >= 8)
                {
                    tileIndex++;
                    realYOffset -= 8;
                }
            }

            int tileAddress = 0x8000 + tileIndex * 16 + realYOffset * 2;
            byte tileLow = _memory.Read((ushort)tileAddress);
            byte tileHigh = _memory.Read((ushort)(tileAddress + 1));

            for (int x = 0; x < 8; x++)
            {
                int screenX = xPos + x;
                if (screenX < 0 || screenX >= WIDTH) continue;

                int realXOffset = xFlip ? (7 - x) : x;
                int bit = 7 - realXOffset;

                int pixel = ((tileHigh >> bit) & 1) << 1;
                pixel |= (tileLow >> bit) & 1;

                if (pixel == 0) continue;

                if (priority && output[screenX] != 0) continue;

                output[screenX] = GetColorFromPalette(pixel, palette);
                priorities[screenX] = priority;
                palettes[screenX] = palette;
            }
        }
    }

    private int GetColorFromPalette(int pixel, byte palette)
    {
        return pixel == 0 ? 0 : (palette >> ((pixel - 1) * 2)) & 0x03;
    }

    public void WriteDMA(byte value)
    {
        _dmaStart = value;
        _dmaTransfer = true;
        _dmaCycles = 0;
    }

    private void PerformDMA()
    {
        ushort source = (ushort)(_dmaStart << 8);
        for (int i = 0; i < 0xA0; i++)
        {
            byte data = _memory.Read((ushort)(source + i));
            _memory.Write((ushort)(0xFE00 + i), data);
        }
    }

    private bool IsLcdEnabled() => (_lcdc & 0x80) != 0;
    private bool IsBGEnabled() => (_lcdc & 0x01) != 0;
    private bool IsSpriteEnabled() => (_lcdc & 0x02) != 0;
    private bool IsSpriteDoubleSize() => (_lcdc & 0x04) != 0;
    private bool IsWindowEnabled() => (_lcdc & 0x20) != 0;

    private ushort GetTileMapBase() => (_lcdc & 0x08) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
    private ushort GetWindowTileMapBase() => (_lcdc & 0x40) != 0 ? (ushort)0x9C00 : (ushort)0x9800;

    private ushort GetTileDataBase()
    {
        if ((_lcdc & 0x10) != 0)
            return 0x8000;
        return 0x8800;
    }
}