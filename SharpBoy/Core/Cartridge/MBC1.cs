using System;

namespace GameBoyEmulator.Core;

public class MBC1
{
    private readonly byte[] _rom;
    private byte[] _ram;
    private readonly int _romSize;
    private readonly int _ramSize;

    private bool _ramEnabled;
    private int _romBankLow;     
    private int _romBankHigh;    
    private int _ramBank;        
    private bool _bankingMode;   

    private const int ROM_BANK_SIZE = 0x4000;
    private const int RAM_BANK_SIZE = 0x2000;
    private const int MAX_ROM_BANKS = 0x7F; 

    public MBC1(byte[] romData, int ramSize = 0x8000)
    {
        _rom = new byte[Math.Max(romData.Length, 0x8000)];
        Array.Copy(romData, _rom, romData.Length);

        _romSize = romData.Length;
        _ramSize = Math.Min(ramSize, 0x8000);
        _ram = new byte[_ramSize];

        _ramEnabled = false;
        _romBankLow = 1;
        _romBankHigh = 0;
        _ramBank = 0;
        _bankingMode = false;
    }

    public byte Read(ushort address)
    {
        if (address < 0x4000)
        {
            if (_bankingMode)
            {
                int bank = _romBankHigh << 5;
                int offset = (bank << 14) | address;
                if (offset >= _romSize) return 0xFF;
                return _rom[offset];
            }
            return _rom[address];
        }

        if (address < 0x8000)
        {
            int bank = (_romBankHigh << 5) | _romBankLow;
            if (bank == 0) bank = 1;
            int offset = (bank << 14) | (address & 0x3FFF);
            if (offset >= _romSize) return 0xFF;
            return _rom[offset];
        }

        if (address >= 0xA000 && address < 0xC000)
        {
            if (!_ramEnabled || _ramSize == 0) return 0xFF;

            int bank = _bankingMode ? _ramBank : 0;
            int offset = (bank << 13) | (address & 0x1FFF);
            if (offset >= _ramSize) return 0xFF;
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

        if (address < 0x4000)
        {
            int low = value & 0x1F;
            if (low == 0) low = 1;
            _romBankLow = low;
            return;
        }

        if (address < 0x6000)
        {
            int high = value & 0x03;
            _romBankHigh = high;
            _ramBank = high;
            return;
        }

        if (address < 0x8000)
        {
            _bankingMode = (value & 0x01) != 0;
            return;
        }

        if (address >= 0xA000 && address < 0xC000)
        {
            if (!_ramEnabled || _ramSize == 0) return;

            int bank = _bankingMode ? _ramBank : 0;
            int offset = (bank << 13) | (address & 0x1FFF);
            if (offset < _ramSize)
            {
                _ram[offset] = value;
            }
        }
    }

    public void Reset(bool preserveRam = true)
    {
        _ramEnabled = false;
        _romBankLow = 1;
        _romBankHigh = 0;
        _ramBank = 0;
        _bankingMode = false;

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
        if (data != null && data.Length == _ramSize)
        {
            Array.Copy(data, _ram, _ramSize);
        }
    }
}