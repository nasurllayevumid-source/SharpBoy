using System;

namespace GameBoyEmulator.Core.Cartridge;

public class RTC
{
    private int _seconds;
    private int _minutes;
    private int _hours;
    private int _days;
    private bool _halt;
    private bool _carry;
    private DateTime _lastUpdate;

    public RTC()
    {
        Reset();
    }

    public void Reset()
    {
        _seconds = 0;
        _minutes = 0;
        _hours = 0;
        _days = 0;
        _halt = false;
        _carry = false;
        _lastUpdate = DateTime.Now;
    }

    public void Tick()
    {
        if (_halt) return;

        DateTime now = DateTime.Now;
        TimeSpan elapsed = now - _lastUpdate;
        _lastUpdate = now;

        int totalSeconds = (int)elapsed.TotalSeconds;
        if (totalSeconds <= 0) return;

        _seconds += totalSeconds;

        while (_seconds >= 60)
        {
            _seconds -= 60;
            _minutes++;

            if (_minutes >= 60)
            {
                _minutes = 0;
                _hours++;

                if (_hours >= 24)
                {
                    _hours = 0;
                    _days++;

                    if (_days >= 512)
                    {
                        _days = 0;
                        _carry = true;
                    }
                }
            }
        }
    }

    public void Latch()
    {
    }

    public byte ReadRegister(int index)
    {
        Tick();

        return index switch
        {
            0 => (byte)_seconds,
            1 => (byte)_minutes,
            2 => (byte)_hours,
            3 => (byte)(_days & 0xFF),
            4 => (byte)(((_days >> 8) & 0x01) | (_carry ? 0x80 : 0x00) | (_halt ? 0x40 : 0x00)),
            _ => 0x00
        };
    }

    public void WriteRegister(int index, byte value)
    {
        switch (index)
        {
            case 0:
                _seconds = value % 60;
                break;
            case 1:
                _minutes = value % 60;
                break;
            case 2:
                _hours = value % 24;
                break;
            case 3:
                _days = (_days & 0x100) | value;
                break;
            case 4:
                _carry = (value & 0x80) != 0;
                _halt = (value & 0x40) != 0;
                _days = (_days & 0xFF) | ((value & 0x01) << 8);
                break;
        }
    }

    public byte[] Serialize()
    {
        byte[] data = new byte[8];
        data[0] = (byte)_seconds;
        data[1] = (byte)_minutes;
        data[2] = (byte)_hours;
        data[3] = (byte)(_days & 0xFF);
        data[4] = (byte)(((_days >> 8) & 0x01) | (_carry ? 0x80 : 0x00) | (_halt ? 0x40 : 0x00));
        data[5] = (byte)(_lastUpdate.ToBinary() >> 56);
        data[6] = (byte)(_lastUpdate.ToBinary() >> 48);
        data[7] = (byte)(_lastUpdate.ToBinary() >> 40);
        return data;
    }

    public void Deserialize(byte[] data)
    {
        if (data == null || data.Length < 8) return;

        _seconds = data[0];
        _minutes = data[1];
        _hours = data[2];
        _days = data[3] | ((data[4] & 0x01) << 8);
        _carry = (data[4] & 0x80) != 0;
        _halt = (data[4] & 0x40) != 0;

        long ticks = ((long)data[5] << 56) | ((long)data[6] << 48) | ((long)data[7] << 40);
        if (ticks != 0)
        {
            _lastUpdate = DateTime.FromBinary(ticks);
        }
        else
        {
            _lastUpdate = DateTime.Now;
        }
    }
}