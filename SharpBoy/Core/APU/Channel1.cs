using System;

namespace GameBoyEmulator.Core.APU;

public class Channel1
{
    private int _sweepTime;
    private int _sweepDirection;
    private int _sweepShift;
    private int _duty;
    private int _lengthCounter;
    private int _envelopeVolume;
    private int _envelopeDirection;
    private int _envelopePeriod;
    private int _frequency;
    private bool _enabled;
    private int _timer;
    private int _dutyPosition;
    private int _volume;
    private int _envelopeTimer;
    private int _sweepTimer;
    private int _shadowFrequency;
    private bool _sweepEnabled;
    private bool _lengthEnabled;
    private int _envelopeFrameCounter;

    private static readonly byte[,] DutyPatterns =
    {
        {0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,1},
        {1,0,0,0,0,1,1,1},
        {0,1,1,1,1,1,1,0}
    };

    public Channel1()
    {
        Reset();
    }

    public void Reset()
    {
        _sweepTime = 0;
        _sweepDirection = 0;
        _sweepShift = 0;
        _duty = 0;
        _lengthCounter = 64;
        _envelopeVolume = 0;
        _envelopeDirection = 0;
        _envelopePeriod = 0;
        _frequency = 0;
        _enabled = false;
        _timer = 0;
        _dutyPosition = 0;
        _volume = 0;
        _envelopeTimer = 0;
        _sweepTimer = 0;
        _shadowFrequency = 0;
        _sweepEnabled = false;
        _lengthEnabled = false;
        _envelopeFrameCounter = 0;
    }

    public void WriteRegister(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF10:
                _sweepTime = (value >> 4) & 0x07;
                _sweepDirection = (value >> 3) & 0x01;
                _sweepShift = value & 0x07;
                break;
            case 0xFF11:
                _duty = (value >> 6) & 0x03;
                _lengthCounter = 64 - (value & 0x3F);
                _lengthEnabled = true;
                break;
            case 0xFF12:
                _envelopeVolume = (value >> 4) & 0x0F;
                _envelopeDirection = (value >> 3) & 0x01;
                _envelopePeriod = value & 0x07;
                break;
            case 0xFF13:
                _frequency = (_frequency & 0x0700) | value;
                break;
            case 0xFF14:
                _frequency = (_frequency & 0x00FF) | ((value & 0x07) << 8);
                _lengthEnabled = (value & 0x40) != 0;
                if ((value & 0x80) != 0) Trigger();
                break;
        }
    }

    public byte ReadRegister(ushort address)
    {
        return address switch
        {
            0xFF10 => (byte)((_sweepTime << 4) | (_sweepDirection << 3) | _sweepShift),
            0xFF11 => (byte)((_duty << 6) | (64 - _lengthCounter)),
            0xFF12 => (byte)((_envelopeVolume << 4) | (_envelopeDirection << 3) | _envelopePeriod),
            0xFF13 => (byte)(_frequency & 0x00FF),
            0xFF14 => (byte)((_frequency >> 8) & 0x07),
            _ => 0xFF
        };
    }

    private void Trigger()
    {
        _enabled = true;
        _timer = (2048 - _frequency) * 4;
        _dutyPosition = 0;
        _volume = _envelopeVolume;
        _envelopeTimer = 0;
        _envelopeFrameCounter = 0;
        _shadowFrequency = _frequency;
        _sweepTimer = 0;
        _sweepEnabled = _sweepTime > 0 || _sweepShift > 0;
        if (_sweepShift > 0) _sweepTimer = _sweepTime;
        if (_lengthCounter == 0) _lengthCounter = 64;
    }

    public void Step(int cycles)
    {
        if (!_enabled) return;

        _timer -= cycles;
        while (_timer <= 0)
        {
            _timer += (2048 - _frequency) * 4;
            _dutyPosition = (_dutyPosition + 1) & 0x07;
        }

        if (_sweepEnabled && _sweepShift > 0)
        {
            _sweepTimer -= cycles;
            while (_sweepTimer <= 0 && _sweepTime > 0)
            {
                _sweepTimer += _sweepTime * 8;
                int newFreq = _shadowFrequency;
                if (_sweepDirection == 1)
                    newFreq -= _shadowFrequency >> _sweepShift;
                else
                    newFreq += _shadowFrequency >> _sweepShift;

                if (newFreq > 2047) { _enabled = false; break; }
                _shadowFrequency = newFreq;
                _frequency = newFreq;
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
        int bit = DutyPatterns[_duty, _dutyPosition];
        return bit == 1 ? _volume : -_volume;
    }
}