using System;

namespace GameBoyEmulator.Core;

public class SaveState
{
    public const int Version = 1;
    public const int Magic = 0x4742454D;

    public RegisterState Registers;
    public CpuState Cpu;
    public MemoryState Memory;
    public PpuState Ppu;
    public TimerState Timer;
    public JoypadState Joypad;
    public InterruptsState Interrupts;
    public ApuState Apu;
    public MbcState Mbc;
    public long Timestamp;
}

public struct RegisterState
{
    public ushort AF;
    public ushort BC;
    public ushort DE;
    public ushort HL;
    public ushort SP;
    public ushort PC;
}

public struct CpuState
{
    public bool IME;
    public bool Halted;
    public bool Stopped;
    public int Opcode;
    public int PrefixedOpcode;
    public long Cycles;
    public int ClockCounter;
}

public struct MemoryState
{
    public byte[] WRAM;
    public byte[] VRAM;
    public byte[] OAM;
    public byte[] HRAM;
    public byte[] IO;
    public byte InterruptEnable;
    public byte InterruptFlag;
}

public struct PpuState
{
    public int Scanline;
    public int Cycles;
    public int Mode;
    public int ModeCycles;
    public bool FrameReady;
    public byte Lcdc;
    public byte Stat;
    public byte Scy;
    public byte Scx;
    public byte Ly;
    public byte Lyc;
    public byte Bgp;
    public byte Obp0;
    public byte Obp1;
    public byte Wy;
    public byte Wx;
    public byte[] FrameBuffer;
    public bool DmaTransfer;
    public int DmaCycles;
    public byte DmaStart;
}

public struct TimerState
{
    public ushort DivCounter;
    public byte LastDiv;
    public byte LastTima;
    public int TimaOverflowPending;
    public byte LastTac;
}

public struct JoypadState
{
    public byte DirectionState;
    public byte ButtonState;
    public byte LastJoyp;
    public byte CurrentJoyp;
}

public struct InterruptsState
{
    public byte InterruptFlag;
    public byte InterruptEnable;
}

public struct ApuState
{
    public bool Powered;
    public int FrameSequencerStep;
    public int FrameSequencerTimer;
    public Channel1State Channel1;
    public Channel2State Channel2;
    public Channel3State Channel3;
    public Channel4State Channel4;
}

public struct Channel1State
{
    public int SweepTime;
    public int SweepDirection;
    public int SweepShift;
    public int Duty;
    public int LengthCounter;
    public int EnvelopeVolume;
    public int EnvelopeDirection;
    public int EnvelopePeriod;
    public int Frequency;
    public bool Enabled;
    public bool LengthEnabled;
    public int Timer;
    public int DutyPosition;
    public int Volume;
    public int EnvelopeTimer;
    public int SweepTimer;
    public int ShadowFrequency;
    public bool SweepEnabled;
    public int EnvelopeFrameCounter;
}

public struct Channel2State
{
    public int Duty;
    public int LengthCounter;
    public int EnvelopeVolume;
    public int EnvelopeDirection;
    public int EnvelopePeriod;
    public int Frequency;
    public bool Enabled;
    public bool LengthEnabled;
    public int Timer;
    public int DutyPosition;
    public int Volume;
    public int EnvelopeFrameCounter;
}

public struct Channel3State
{
    public int LengthCounter;
    public int Frequency;
    public bool Enabled;
    public bool LengthEnabled;
    public bool DacEnabled;
    public int Timer;
    public int SamplePosition;
    public byte[] WaveRam;
}

public struct Channel4State
{
    public int LengthCounter;
    public int EnvelopeVolume;
    public int EnvelopeDirection;
    public int EnvelopePeriod;
    public bool Enabled;
    public bool LengthEnabled;
    public int Volume;
    public int EnvelopeFrameCounter;
    public ushort Lfsr;
    public int ClockShift;
    public int ClockWidth;
    public int DivisorRatio;
}

public struct MbcState
{
    public byte Type;
    public byte[] Data;
}

public struct RtcState
{
    public int Seconds;
    public int Minutes;
    public int Hours;
    public int Days;
    public bool Halt;
    public bool Carry;
    public long LastUpdateTicks;
}