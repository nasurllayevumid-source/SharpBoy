namespace GameBoyEmulator.Core;

public struct AluResult
{
    public byte Value;
    public bool Zero;
    public bool Subtract;
    public bool HalfCarry;
    public bool Carry;

    public void Apply(Registers regs)
    {
        regs.ZeroFlag = Zero;
        regs.SubtractFlag = Subtract;
        regs.HalfCarryFlag = HalfCarry;
        regs.CarryFlag = Carry;
    }

    public void ApplyWithoutZero(Registers regs)
    {
        regs.SubtractFlag = Subtract;
        regs.HalfCarryFlag = HalfCarry;
        regs.CarryFlag = Carry;
    }

    public void ApplyWithoutSubtract(Registers regs)
    {
        regs.ZeroFlag = Zero;
        regs.HalfCarryFlag = HalfCarry;
        regs.CarryFlag = Carry;
    }

    public void ApplyWithoutCarry(Registers regs)
    {
        regs.ZeroFlag = Zero;
        regs.SubtractFlag = Subtract;
        regs.HalfCarryFlag = HalfCarry;
    }
}