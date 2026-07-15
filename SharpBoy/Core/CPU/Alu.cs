using System;

namespace GameBoyEmulator.Core;

public static class ALU
{
    public static AluResult Add(byte a, byte b)
    {
        int result = a + b;
        return new AluResult
        {
            Value = (byte)result,
            Zero = (byte)result == 0,
            Subtract = false,
            HalfCarry = ((a & 0x0F) + (b & 0x0F)) > 0x0F,
            Carry = result > 0xFF
        };
    }

    public static AluResult AddWithCarry(byte a, byte b, bool carryIn)
    {
        int result = a + b + (carryIn ? 1 : 0);
        return new AluResult
        {
            Value = (byte)result,
            Zero = (byte)result == 0,
            Subtract = false,
            HalfCarry = ((a & 0x0F) + (b & 0x0F) + (carryIn ? 1 : 0)) > 0x0F,
            Carry = result > 0xFF
        };
    }

    public static AluResult Sub(byte a, byte b)
    {
        int result = a - b;
        return new AluResult
        {
            Value = (byte)result,
            Zero = (byte)result == 0,
            Subtract = true,
            HalfCarry = (a & 0x0F) < (b & 0x0F),
            Carry = a < b
        };
    }

    public static AluResult SubWithCarry(byte a, byte b, bool carryIn)
    {
        int result = a - b - (carryIn ? 1 : 0);
        return new AluResult
        {
            Value = (byte)result,
            Zero = (byte)result == 0,
            Subtract = true,
            HalfCarry = (a & 0x0F) < ((b & 0x0F) + (carryIn ? 1 : 0)),
            Carry = a < (b + (carryIn ? 1 : 0))
        };
    }

    public static AluResult And(byte a, byte b)
    {
        byte result = (byte)(a & b);
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = true,
            Carry = false
        };
    }

    public static AluResult Or(byte a, byte b)
    {
        byte result = (byte)(a | b);
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = false
        };
    }

    public static AluResult Xor(byte a, byte b)
    {
        byte result = (byte)(a ^ b);
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = false
        };
    }

    public static AluResult Inc(byte value)
    {
        int result = value + 1;
        return new AluResult
        {
            Value = (byte)result,
            Zero = (byte)result == 0,
            Subtract = false,
            HalfCarry = (value & 0x0F) == 0x0F,
            Carry = false
        };
    }

    public static AluResult Dec(byte value)
    {
        int result = value - 1;
        return new AluResult
        {
            Value = (byte)result,
            Zero = (byte)result == 0,
            Subtract = true,
            HalfCarry = (value & 0x0F) == 0x00,
            Carry = false
        };
    }

    public static AluResult RotateLeftCircular(byte value)
    {
        bool carry = (value & 0x80) != 0;
        byte result = (byte)((value << 1) | (carry ? 1 : 0));
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = carry
        };
    }

    public static AluResult RotateLeftThroughCarry(byte value, bool oldCarry)
    {
        bool newCarry = (value & 0x80) != 0;
        byte result = (byte)((value << 1) | (oldCarry ? 1 : 0));
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = newCarry
        };
    }

    public static AluResult RotateRightCircular(byte value)
    {
        bool carry = (value & 0x01) != 0;
        byte result = (byte)((value >> 1) | (carry ? 0x80 : 0));
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = carry
        };
    }

    public static AluResult RotateRightThroughCarry(byte value, bool oldCarry)
    {
        bool newCarry = (value & 0x01) != 0;
        byte result = (byte)((value >> 1) | (oldCarry ? 0x80 : 0));
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = newCarry
        };
    }

    public static AluResult ShiftLeft(byte value)
    {
        bool carry = (value & 0x80) != 0;
        byte result = (byte)(value << 1);
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = carry
        };
    }

    public static AluResult ShiftRight(byte value)
    {
        bool carry = (value & 0x01) != 0;
        byte result = (byte)(value >> 1);
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = carry
        };
    }

    public static AluResult ShiftRightArithmetic(byte value)
    {
        bool carry = (value & 0x01) != 0;
        byte result = (byte)((value >> 1) | (value & 0x80));
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = carry
        };
    }

    public static AluResult Swap(byte value)
    {
        byte result = (byte)(((value & 0x0F) << 4) | ((value & 0xF0) >> 4));
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = false
        };
    }

    public static AluResult Complement(byte value)
    {
        byte result = (byte)(~value);
        return new AluResult
        {
            Value = result,
            Zero = result == 0,
            Subtract = false,
            HalfCarry = false,
            Carry = false
        };
    }

    public static AluResult Cp(byte a, byte b)
    {
        int result = a - b;
        return new AluResult
        {
            Value = (byte)result,
            Zero = result == 0,
            Subtract = true,
            HalfCarry = (a & 0x0F) < (b & 0x0F),
            Carry = a < b
        };
    }

    public static ushort IncWord(ushort value) => (ushort)(value + 1);
    public static ushort DecWord(ushort value) => (ushort)(value - 1);
}