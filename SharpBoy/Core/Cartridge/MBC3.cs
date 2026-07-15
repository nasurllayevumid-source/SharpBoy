using System;

namespace GameBoyEmulator.Core.Cartridge;

public class MBC3
{
    private readonly byte[] _rom;
    private readonly byte[] _ram;
    private readonly int _romSize;
    private readonly int _ramSize;
    private readonly bool _hasRtc;

    private int _romBank;
    private int _ramBank;
    private bool _ramEnabled;
    private bool _rtcLatched;

    private readonly RTC _rtc;

    private const int ROM_BANK_SIZE = 0x4000;
    private const int RAM_BANK_SIZE = 0x2000;

    public MBC3(byte[] romData, int ramSize = 0x8000, bool hasRtc = true)
    {
        _rom = new byte[Math.Max(romData.Length, 0x8000)];
        Array.Copy(romData, _rom, romData.Length);
        _romSize = romData.Length;

        _ramSize = Math.Min(ramSize, 0x8000);
        _ram = new byte[_ramSize];

        _hasRtc = hasRtc;

        _romBank = 1;
        _ramBank = 0;
        _ramEnabled = false;
        _rtcLatched = false;

        _rtc = new RTC();
    }

    public byte Read(ushort address)
    {
        if (address < 0x4000)
        {
            return _rom[address];
        }

        if (address < 0x8000)
        {
            int bank = _romBank & 0x7F;
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

            if (_ramBank <= 0x03)
            {
                if (_ramSize == 0) return 0xFF;
                int offset = (_ramBank << 13) | (address & 0x1FFF);
                if (offset >= _ramSize) return 0xFF;
                return _ram[offset];
            }

            if (_hasRtc && _ramBank >= 0x08 && _ramBank <= 0x0C)
            {
                return ReadRtcRegister(_ramBank - 0x08);
            }
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
            int bank = value & 0x7F;
            if (bank == 0) bank = 1;
            _romBank = bank;
            return;
        }

        if (address < 0x6000)
        {
            _ramBank = value;
            return;
        }

        if (address < 0x8000)
        {
            if (!_hasRtc) return;

            if (value == 0x00)
            {
                _rtcLatched = false;
            }
            else if (value == 0x01 && !_rtcLatched)
            {
                _rtcLatched = true;
                _rtc.Latch();
            }
            return;
        }

        if (address >= 0xA000 && address <= 0xBFFF)
        {
            if (!_ramEnabled) return;

            if (_ramBank <= 0x03)
            {
                if (_ramSize == 0) return;
                int offset = (_ramBank << 13) | (address & 0x1FFF);
                if (offset < _ramSize)
                {
                    _ram[offset] = value;
                }
                return;
            }

            if (_hasRtc && _ramBank >= 0x08 && _ramBank <= 0x0C)
            {
                WriteRtcRegister(_ramBank - 0x08, value);
                return;
            }
        }
    }

    private byte ReadRtcRegister(int index)
    {
        if (!_rtcLatched) return 0x00;
        return _rtc.ReadRegister(index);
    }

    private void WriteRtcRegister(int index, byte value)
    {
        _rtc.WriteRegister(index, value);
    }

    public void Reset()
    {
        _romBank = 1;
        _ramBank = 0;
        _ramEnabled = false;
        _rtcLatched = false;
    }

    public void Reset(bool preserveRam)
    {
        _romBank = 1;
        _ramBank = 0;
        _ramEnabled = false;
        _rtcLatched = false;

        if (!preserveRam)
        {
            Array.Clear(_ram, 0, _ram.Length);
        }
    }

    public void TickRtc()
    {
        _rtc.Tick();
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

    public byte[] GetRtcData()
    {
        return _rtc.Serialize();
    }

    public void SetRtcData(byte[] data)
    {
        _rtc.Deserialize(data);
    }
}