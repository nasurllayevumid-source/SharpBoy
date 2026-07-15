using System;

namespace GameBoyEmulator.Core.APU;

public class Channel3
{
    private int _lengthCounter;
    private int _frequency;
    private bool _enabled;
    private bool _lengthEnabled;
    private bool _dacEnabled;
    private int _timer;
    private int _samplePosition;
    private readonly byte[] _waveRam;

    public Channel3()
    {
        _waveRam = new byte[16];
        Reset();
    }

    public void Reset()
    {
        _lengthCounter = 256;
        _frequency = 0;
        _enabled = false;
        _lengthEnabled = false;
        _dacEnabled = true;
        _timer = 0;
        _samplePosition = 0;
        Array.Clear(_waveRam, 0, _waveRam.Length);
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF1A:
                _dacEnabled = (value & 0x80) != 0;
                if (!_dacEnabled) _enabled = false;
                break;
            case 0xFF1B:
                _lengthCounter = 256 - value;
                _lengthEnabled = true;
                break;
            case 0xFF1C:
                break;
            case 0xFF1D:
                _frequency = (_frequency & 0x0700) | value;
                break;
            case 0xFF1E:
                _frequency = (_frequency & 0x00FF) | ((value & 0x07) << 8);
                _lengthEnabled = (value & 0x40) != 0;
                if ((value & 0x80) != 0) Trigger();
                break;
            case >= 0xFF30 and <= 0xFF3F:
                _waveRam[address - 0xFF30] = value;
                break;
        }
    }

    public byte ReadRegister(ushort address)
    {
        return address switch
        {
            0xFF1A => (byte)(_dacEnabled ? 0x80 : 0x00),
            0xFF1B => (byte)(256 - _lengthCounter),
            0xFF1C => 0x00,
            0xFF1D => (byte)(_frequency & 0x00FF),
            0xFF1E => (byte)((_frequency >> 8) & 0x07),
            >= 0xFF30 and <= 0xFF3F => _waveRam[address - 0xFF30],
            _ => 0xFF
        };
    }

    public void WriteWaveRam(byte index, byte value)
    {
        if (index < 16) _waveRam[index] = value;
    }

    public byte ReadWaveRam(byte index)
    {
        return index < 16 ? _waveRam[index] : (byte)0xFF;
    }

    private void Trigger()
    {
        if (!_dacEnabled) return;
        _enabled = true;
        _timer = (2048 - _frequency) * 2;
        _samplePosition = 0;
        if (_lengthCounter == 0) _lengthCounter = 256;
    }

    public void Step(int cycles)
    {
        if (!_enabled || !_dacEnabled) return;

        _timer -= cycles;
        while (_timer <= 0)
        {
            _timer += (2048 - _frequency) * 2;
            _samplePosition = (_samplePosition + 1) & 0x1F;
        }
    }

    public void FrameStep(int frameStep)
    {
        if (!_enabled || !_dacEnabled) return;

        if (frameStep == 0 || frameStep == 4)
        {
            if (_lengthEnabled)
            {
                _lengthCounter--;
                if (_lengthCounter <= 0) _enabled = false;
            }
        }
    }

    public int GetSample()
    {
        if (!_enabled || !_dacEnabled) return 0;

        int byteIndex = _samplePosition >> 1;
        int nibble = (_samplePosition & 1) == 0
            ? (_waveRam[byteIndex] >> 4)
            : (_waveRam[byteIndex] & 0x0F);
        return nibble;
    }
}