namespace GameBoyEmulator.Core;

public class Registers
{
    private ushort _af;
    private ushort _bc;
    private ushort _de;
    private ushort _hl;
    private ushort _sp;
    private ushort _pc;
    private bool _ime;

    public ushort AF { get => _af; set => _af = value; }
    public ushort BC { get => _bc; set => _bc = value; }
    public ushort DE { get => _de; set => _de = value; }
    public ushort HL { get => _hl; set => _hl = value; }
    public ushort SP { get => _sp; set => _sp = value; }
    public ushort PC { get => _pc; set => _pc = value; }
    public bool IME { get => _ime; set => _ime = value; }

    public byte A
    {
        get => (byte)(_af >> 8);
        set => _af = (ushort)((value << 8) | (_af & 0x00FF));
    }

    public byte F
    {
        get => (byte)(_af & 0xF0);
        set => _af = (ushort)((_af & 0xFF00) | (value & 0xF0));
    }

    public byte B
    {
        get => (byte)(_bc >> 8);
        set => _bc = (ushort)((value << 8) | (_bc & 0x00FF));
    }

    public byte C
    {
        get => (byte)(_bc & 0x00FF);
        set => _bc = (ushort)((_bc & 0xFF00) | value);
    }

    public byte D
    {
        get => (byte)(_de >> 8);
        set => _de = (ushort)((value << 8) | (_de & 0x00FF));
    }

    public byte E
    {
        get => (byte)(_de & 0x00FF);
        set => _de = (ushort)((_de & 0xFF00) | value);
    }

    public byte H
    {
        get => (byte)(_hl >> 8);
        set => _hl = (ushort)((value << 8) | (_hl & 0x00FF));
    }

    public byte L
    {
        get => (byte)(_hl & 0x00FF);
        set => _hl = (ushort)((_hl & 0xFF00) | value);
    }

    public bool ZeroFlag
    {
        get => (F & 0x80) != 0;
        set => F = (byte)(value ? F | 0x80 : F & ~0x80);
    }

    public bool SubtractFlag
    {
        get => (F & 0x40) != 0;
        set => F = (byte)(value ? F | 0x40 : F & ~0x40);
    }

    public bool HalfCarryFlag
    {
        get => (F & 0x20) != 0;
        set => F = (byte)(value ? F | 0x20 : F & ~0x20);
    }

    public bool CarryFlag
    {
        get => (F & 0x10) != 0;
        set => F = (byte)(value ? F | 0x10 : F & ~0x10);
    }

    public void Reset()
    {
        _af = 0;
        _bc = 0;
        _de = 0;
        _hl = 0;
        _sp = 0;
        _pc = 0;
        _ime = false;
    }

    public void LoadPostBootState()
    {
        _af = 0x01B0;
        _bc = 0x0013;
        _de = 0x00D8;
        _hl = 0x014D;
        _sp = 0xFFFE;
        _pc = 0x0100;
        _ime = false;
    }
}