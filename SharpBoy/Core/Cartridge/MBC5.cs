using System;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC5
{
    private readonly byte[] _rom;
    private readonly byte[] _ram;
    private readonly int _romSize;
    private readonly int _ramSize;

    private int _romBank;
    private int _ramBank;
    private bool _ramEnabled;

    private const int ROM_BANK_SIZE = 0x4000;
    private const int RAM_BANK_SIZE = 0x2000;

    public MBC5(byte[] romData, int ramSize = 0x8000)
    {
        _rom = new byte[Math.Max(romData.Length, 0x8000)];
        Array.Copy(romData, _rom, romData.Length);
        _romSize = romData.Length;

        _ramSize = Math.Min(ramSize, 0x20000);
        _ram = new byte[_ramSize];

        _romBank = 1;
        _ramBank = 0;
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
            int bank = _romBank & 0x1FF;
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
            if (!_ramEnabled || _ramSize == 0)
            {
                return 0xFF;
            }

            int bank = _ramBank & 0x0F;
            int offset = (bank << 13) | (address & 0x1FFF);
            if (offset >= _ramSize)
            {
                return 0xFF;
            }
            return _ram[offset];
        }

        return 0xFF;
    }

    public void Write(ushort address, byte value)
    {
        if (address < 0x2000)
        {
            _ramEnabled = (value & 0x0F) == 0x0A;
            return;
        }

        if (address < 0x3000)
        {
            _romBank = (_romBank & 0x100) | value;
            return;
        }

        if (address < 0x4000)
        {
            _romBank = (_romBank & 0x0FF) | ((value & 0x01) << 8);
            return;
        }

        if (address >= 0x4000 && address < 0x6000)
        {
            _ramBank = value & 0x0F;
            return;
        }

        if (address >= 0xA000 && address <= 0xBFFF)
        {
            if (!_ramEnabled || _ramSize == 0)
            {
                return;
            }

            int bank = _ramBank & 0x0F;
            int offset = (bank << 13) | (address & 0x1FFF);
            if (offset < _ramSize)
            {
                _ram[offset] = value;
            }
        }
    }

    public void Reset()
    {
        _romBank = 1;
        _ramBank = 0;
        _ramEnabled = false;
    }

    public void Reset(bool preserveRam)
    {
        _romBank = 1;
        _ramBank = 0;
        _ramEnabled = false;

        if (!preserveRam)
        {
            Array.Clear(_ram, 0, _ram.Length);
        }
    }

    public int RomSize => _romSize;
    public int RamSize => _ramSize;

    public byte[] GetRamData()
    {
        byte[] result = new byte[_ramSize];
        Array.Copy(_ram, result, _ramSize);
        return result;
    }

    public void SetRamData(byte[] data)
    {
        if (data == null) return;
        int length = Math.Min(data.Length, _ramSize);
        Array.Copy(data, _ram, length);
    }
}