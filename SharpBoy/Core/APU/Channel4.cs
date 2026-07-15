using System;

namespace GameBoyEmulator.Core.APU;

public class Channel4
{
    private int _lengthCounter;
    private int _envelopeVolume;
    private int _envelopeDirection;
    private int _envelopePeriod;
    private bool _enabled;
    private bool _lengthEnabled;
    private int _volume;
    private int _envelopeFrameCounter;
    private ushort _lfsr;
    private int _clockShift;
    private int _clockWidth;
    private int _divisorRatio;

    private readonly int[] _divisorCodes = { 8, 16, 32, 48, 64, 80, 96, 112 };

    public Channel4()
    {
        Reset();
    }

    public void Reset()
    {
        _lengthCounter = 64;
        _envelopeVolume = 0;
        _envelopeDirection = 0;
        _envelopePeriod = 0;
        _enabled = false;
        _lengthEnabled = false;
        _volume = 0;
        _envelopeFrameCounter = 0;
        _lfsr = 0x7FFF;
        _clockShift = 0;
        _clockWidth = 0;
        _divisorRatio = 0;
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF20:
                _lengthCounter = 64 - (value & 0x3F);
                _lengthEnabled = true;
                break;
            case 0xFF21:
                _envelopeVolume = (value >> 4) & 0x0F;
                _envelopeDirection = (value >> 3) & 0x01;
                _envelopePeriod = value & 0x07;
                break;
            case 0xFF22:
                _clockShift = (value >> 4) & 0x0F;
                _clockWidth = (value >> 3) & 0x01;
                _divisorRatio = value & 0x07;
                break;
            case 0xFF23:
                _lengthEnabled = (value & 0x40) != 0;
                if ((value & 0x80) != 0) Trigger();
                break;
        }
    }

    public byte ReadRegister(ushort address)
    {
        return address switch
        {
            0xFF20 => (byte)(64 - _lengthCounter),
            0xFF21 => (byte)((_envelopeVolume << 4) | (_envelopeDirection << 3) | _envelopePeriod),
            0xFF22 => (byte)((_clockShift << 4) | (_clockWidth << 3) | _divisorRatio),
            0xFF23 => 0x00,
            _ => 0xFF
        };
    }

    private void Trigger()
    {
        _enabled = true;
        _volume = _envelopeVolume;
        _envelopeFrameCounter = 0;
        _lfsr = 0x7FFF;
        if (_lengthCounter == 0) _lengthCounter = 64;
    }

    public void Step(int cycles)
    {
        if (!_enabled) return;

        int divisor = _divisorCodes[_divisorRatio];
        int frequency = divisor << (_clockShift + 1);

        int steps = cycles / frequency;
        for (int i = 0; i < Math.Min(steps, 10); i++)
        {
            int bit = (_lfsr & 1) ^ ((_lfsr >> 1) & 1);
            _lfsr >>= 1;
            if (_clockWidth == 0)
                _lfsr |= (ushort)(bit << 14);
            else
            {
                _lfsr |= (ushort)(bit << 6);
                _lfsr &= 0xFFBF;
            }
        }
    }

    public void FrameStep(int frameStep)
    {
        if (!_enabled) return;

        if (frameStep == 0 || frameStep == 4)
        {
            if (_lengthEnabled)
            {
                _lengthCounter--;
                if (_lengthCounter <= 0) _enabled = false;
            }
        }

        if (frameStep == 0 || frameStep == 2 || frameStep == 4 || frameStep == 6)
        {
            if (_envelopePeriod > 0)
            {
                _envelopeFrameCounter++;
                if (_envelopeFrameCounter >= _envelopePeriod)
                {
                    _envelopeFrameCounter = 0;
                    if (_envelopeDirection == 1)
                    {
                        if (_volume < 15) _volume++;
                    }
                    else
                    {
                        if (_volume > 0) _volume--;
                    }
                }
            }
        }
    }

    public int GetSample()
    {
        if (!_enabled || _volume == 0) return 0;
        return (_lfsr & 1) == 0 ? _volume : -_volume;
    }
}