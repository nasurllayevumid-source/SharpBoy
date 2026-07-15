using System;

namespace GameBoyEmulator.Core;

public class Memory
{
    private byte[] _rom = new byte[0x8000];
    private byte[] _vram = new byte[0x2000];
    private byte[] _wram = new byte[0x2000];
    private byte[] _echoRam = new byte[0x1E00];
    private byte[] _oam = new byte[0xA0];
    private byte[] _ioRegisters = new byte[0x80];
    private byte[] _hram = new byte[0x7F];
    private byte _interruptEnable;
    private byte _interruptFlag;
    private bool _romWriteable;

    public Memory()
    {
        _romWriteable = false;
    }

    public byte Read(ushort address)
    {
        return address switch
        {
            >= 0x0000 and <= 0x3FFF => _rom[address],
            >= 0x4000 and <= 0x7FFF => _rom[address],
            >= 0x8000 and <= 0x9FFF => _vram[address - 0x8000],
            >= 0xA000 and <= 0xBFFF => _rom[address],
            >= 0xC000 and <= 0xDFFF => _wram[address - 0xC000],
            >= 0xE000 and <= 0xFDFF => _echoRam[address - 0xE000],
            >= 0xFE00 and <= 0xFE9F => _oam[address - 0xFE00],
            >= 0xFF00 and <= 0xFF7F => _ioRegisters[address - 0xFF00],
            >= 0xFF80 and <= 0xFFFE => _hram[address - 0xFF80],
            0xFFFF => _interruptEnable,
            _ => 0xFF
        };
    }

    public void Write(ushort address, byte value)
    {
        switch (address)
        {
            case >= 0x0000 and <= 0x3FFF:
                if (_romWriteable) _rom[address] = value;
                break;
            case >= 0x4000 and <= 0x7FFF:
                if (_romWriteable) _rom[address] = value;
                break;
            case >= 0x8000 and <= 0x9FFF:
                _vram[address - 0x8000] = value;
                break;
            case >= 0xA000 and <= 0xBFFF:
                _rom[address] = value;
                break;
            case >= 0xC000 and <= 0xDFFF:
                _wram[address - 0xC000] = value;
                break;
            case >= 0xE000 and <= 0xFDFF:
                _echoRam[address - 0xE000] = value;
                break;
            case >= 0xFE00 and <= 0xFE9F:
                _oam[address - 0xFE00] = value;
                break;
            case >= 0xFF00 and <= 0xFF7F:
                _ioRegisters[address - 0xFF00] = value;
                break;
            case >= 0xFF80 and <= 0xFFFE:
                _hram[address - 0xFF80] = value;
                break;
            case 0xFFFF:
                _interruptEnable = value;
                break;
        }
    }

    public ushort ReadWord(ushort address)
    {
        byte low = Read(address);
        byte high = Read((ushort)(address + 1));
        return (ushort)(low | (high << 8));
    }

    public void WriteWord(ushort address, ushort value)
    {
        Write(address, (byte)(value & 0x00FF));
        Write((ushort)(address + 1), (byte)(value >> 8));
    }

    public void LoadRom(byte[] rom)
    {
        int length = Math.Min(rom.Length, 0x8000);
        Array.Copy(rom, 0, _rom, 0, length);
        _romWriteable = false;
    }

    public void LoadBootRom(byte[] bootRom)
    {
        if (bootRom == null || bootRom.Length == 0) return;

        if (bootRom.Length > 0x100)
        {
            Array.Copy(bootRom, 0, _rom, 0x0000, 0x100);
        }
        else
        {
            Array.Copy(bootRom, 0, _rom, 0x0000, bootRom.Length);
        }
        _romWriteable = true;
    }

    public void EnableRomWrite(bool enable)
    {
        _romWriteable = enable;
    }

    public byte ReadIO(byte address)
    {
        return _ioRegisters[address];
    }

    public void WriteIO(byte address, byte value)
    {
        _ioRegisters[address] = value;
    }

    public void SetInterruptFlag(byte value)
    {
        _interruptFlag = value;
    }

    public byte GetInterruptFlag()
    {
        return _interruptFlag;
    }

    public byte GetInterruptEnable()
    {
        return _interruptEnable;
    }

    public void Reset()
    {
        Array.Clear(_vram, 0, _vram.Length);
        Array.Clear(_wram, 0, _wram.Length);
        Array.Clear(_echoRam, 0, _echoRam.Length);
        Array.Clear(_oam, 0, _oam.Length);
        Array.Clear(_ioRegisters, 0, _ioRegisters.Length);
        Array.Clear(_hram, 0, _hram.Length);
        _interruptEnable = 0;
        _interruptFlag = 0;
    }
}