using System;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC2
{
    private readonly byte[] _rom;
    private readonly byte[] _ram;
    private readonly int _romSize;

    private int _romBank;
    private bool _ramEnabled;

    private const int ROM_BANK_SIZE = 0x4000;
    private const int RAM_SIZE = 0x200;

    public MBC2(byte[] romData)
    {
        _rom = new byte[Math.Max(romData.Length, 0x4000)];
        Array.Copy(romData, _rom, romData.Length);
        _romSize = romData.Length;
        _ram = new byte[RAM_SIZE];
        _romBank = 1;
        _ramEnabled = false;
    }

    public byte Read(ushort address)
    {
        if (address < 0x4000)
        {
            return _rom[address];
        }

        if (address < 0x8000)
        {
            int bank = _romBank & 0x0F;
            if (bank == 0) bank = 1;

            int offset = (bank << 14) | (address & 0x3FFF);
            if (offset >= _romSize)
            {
                return 0xFF;
            }
            return _rom[offset];
        }

        if (address >= 0xA000 && address <= 0xBFFF)
        {
            if (!_ramEnabled)
            {
                return 0xFF;
            }

            int ramIndex = (address - 0xA000) & 0x1FF;
            return (byte)(_ram[ramIndex] | 0xF0);
        }

        return 0xFF;
    }

    public void Write(ushort address, byte value)
    {
        if (address < 0x2000)
        {
            if ((address & 0x0100) == 0)
            {
                _ramEnabled = (value & 0x0F) == 0x0A;
            }
            return;
        }

        if (address < 0x4000)
        {
            if ((address & 0x0100) != 0)
            {
                _romBank = value & 0x0F;
                if (_romBank == 0) _romBank = 1;
            }
            return;
        }

        if (address >= 0xA000 && address <= 0xBFFF)
        {
            if (!_ramEnabled)
            {
                return;
            }

            int ramIndex = (address - 0xA000) & 0x1FF;
            _ram[ramIndex] = (byte)(value & 0x0F);
        }
    }

    public void Reset()
    {
        _romBank = 1;
        _ramEnabled = false;
    }

    public int RomSize => _romSize;

    public byte[] GetRamData()
    {
        byte[] result = new byte[RAM_SIZE];
        Array.Copy(_ram, result, RAM_SIZE);
        return result;
    }

    public void SetRamData(byte[] data)
    {
        if (data == null) return;

        int length = Math.Min(data.Length, RAM_SIZE);
        Array.Copy(data, _ram, length);
    }
}