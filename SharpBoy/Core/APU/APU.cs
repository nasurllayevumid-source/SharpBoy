using System;

namespace GameBoyEmulator.Core.APU;

public class APU
{
    private readonly Channel1 _channel1;
    private readonly Channel2 _channel2;
    private readonly Channel3 _channel3;
    private readonly Channel4 _channel4;
    private readonly FrameSequencer _frameSequencer;

    private bool _powered;

    public APU()
    {
        _channel1 = new Channel1();
        _channel2 = new Channel2();
        _channel3 = new Channel3();
        _channel4 = new Channel4();
        _frameSequencer = new FrameSequencer();
        _powered = true;
    }

    public void Reset()
    {
        _channel1.Reset();
        _channel2.Reset();
        _channel3.Reset();
        _channel4.Reset();
        _frameSequencer.Reset();
        _powered = true;
    }

    public void Step(int cycles)
    {
        if (!_powered) return;

        _channel1.Step(cycles);
        _channel2.Step(cycles);
        _channel3.Step(cycles);
        _channel4.Step(cycles);

        _frameSequencer.Step(cycles, frameStep =>
        {
            _channel1.FrameStep(frameStep);
            _channel2.FrameStep(frameStep);
            _channel3.FrameStep(frameStep);
            _channel4.FrameStep(frameStep);
        });
    }

    public int GetSample()
    {
        if (!_powered) return 0;

        int sample = 0;
        sample += _channel1.GetSample();
        sample += _channel2.GetSample();
        sample += _channel3.GetSample();
        sample += _channel4.GetSample();

        if (sample > 31) sample = 31;
        if (sample < -31) sample = -31;

        return sample;
    }

    public bool IsPowered => _powered;
    public void Power(bool on) => _powered = on;

    public void WriteRegister(ushort address, byte value)
    {
        if (!_powered && address >= 0xFF10 && address <= 0xFF3F) return;

        if (address == 0xFF26)
        {
            _powered = (value & 0x80) != 0;
            if (!_powered)
            {
                _channel1.Reset();
                _channel2.Reset();
                _channel3.Reset();
                _channel4.Reset();
                _frameSequencer.Reset();
            }
            return;
        }

        if (!_powered) return;

        if (address >= 0xFF10 && address <= 0xFF14)
        {
            _channel1.WriteRegister(address, value);
            return;
        }

        if (address >= 0xFF16 && address <= 0xFF19)
        {
            _channel2.WriteRegister(address, value);
            return;
        }

        if (address >= 0xFF1A && address <= 0xFF1E)
        {
            _channel3.WriteRegister(address, value);
            return;
        }

        if (address >= 0xFF20 && address <= 0xFF23)
        {
            _channel4.WriteRegister(address, value);
            return;
        }

        if (address >= 0xFF30 && address <= 0xFF3F)
        {
            _channel3.WriteWaveRam((byte)(address - 0xFF30), value);
            return;
        }
    }

    public byte ReadRegister(ushort address)
    {
        if (!_powered && address >= 0xFF10 && address <= 0xFF3F) return 0xFF;

        if (address == 0xFF26)
        {
            byte result = (byte)(_powered ? 0x80 : 0x00);
            if (!_powered) return result;

            if (_channel1.GetSample() != 0) result |= 0x01;
            if (_channel2.GetSample() != 0) result |= 0x02;
            if (_channel3.GetSample() != 0) result |= 0x04;
            if (_channel4.GetSample() != 0) result |= 0x08;
            return result;
        }

        if (!_powered) return 0xFF;

        if (address >= 0xFF10 && address <= 0xFF14)
            return _channel1.ReadRegister(address);

        if (address >= 0xFF16 && address <= 0xFF19)
            return _channel2.ReadRegister(address);

        if (address >= 0xFF1A && address <= 0xFF1E)
            return _channel3.ReadRegister(address);

        if (address >= 0xFF20 && address <= 0xFF23)
            return _channel4.ReadRegister(address);

        if (address >= 0xFF30 && address <= 0xFF3F)
            return _channel3.ReadWaveRam((byte)(address - 0xFF30));

        return 0xFF;
    }
}