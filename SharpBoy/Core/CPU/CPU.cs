using System;
using System.Collections.Generic;

namespace GameBoyEmulator.Core;

public class CPU
{
    private readonly Registers _registers;
    private readonly Memory _memory;
    private long _cycles;
    private bool _running;
    private readonly Dictionary<byte, Action> _instructionTable;
    private bool _ime;

    public CPU(Memory memory)
    {
        _memory = memory;
        _registers = new Registers();
        _registers.LoadPostBootState();
        _cycles = 0;
        _running = false;
        _ime = false;
        _instructionTable = new Dictionary<byte, Action>(256);
        BuildInstructionTable();
    }

    public Registers Registers => _registers;
    public long Cycles => _cycles;
    public bool Running { get => _running; set => _running = value; }
    public bool IME { get => _ime; set => _ime = value; }

    private void BuildInstructionTable()
    {
        _instructionTable[0x00] = Nop; _instructionTable[0x01] = LdBC_nn; _instructionTable[0x02] = Ld_BC_A; _instructionTable[0x03] = IncBC;
        _instructionTable[0x04] = IncB; _instructionTable[0x05] = DecB; _instructionTable[0x06] = LdB_n; _instructionTable[0x07] = Rlca;
        _instructionTable[0x08] = Ld_nn_SP; _instructionTable[0x09] = AddHL_BC; _instructionTable[0x0A] = LdA_BC; _instructionTable[0x0B] = DecBC;
        _instructionTable[0x0C] = IncC; _instructionTable[0x0D] = DecC; _instructionTable[0x0E] = LdC_n; _instructionTable[0x0F] = Rrca;

        _instructionTable[0x10] = Stop; _instructionTable[0x11] = LdDE_nn; _instructionTable[0x12] = Ld_DE_A; _instructionTable[0x13] = IncDE;
        _instructionTable[0x14] = IncD; _instructionTable[0x15] = DecD; _instructionTable[0x16] = LdD_n; _instructionTable[0x17] = Rla;
        _instructionTable[0x18] = Jr_n; _instructionTable[0x19] = AddHL_DE; _instructionTable[0x1A] = LdA_DE; _instructionTable[0x1B] = DecDE;
        _instructionTable[0x1C] = IncE; _instructionTable[0x1D] = DecE; _instructionTable[0x1E] = LdE_n; _instructionTable[0x1F] = Rra;

        _instructionTable[0x20] = Jr_NZ_n; _instructionTable[0x21] = LdHL_nn; _instructionTable[0x22] = Ld_HLI_A; _instructionTable[0x23] = IncHL;
        _instructionTable[0x24] = IncH; _instructionTable[0x25] = DecH; _instructionTable[0x26] = LdH_n; _instructionTable[0x27] = Daa;
        _instructionTable[0x28] = Jr_Z_n; _instructionTable[0x29] = AddHL_HL; _instructionTable[0x2A] = LdA_HLI; _instructionTable[0x2B] = DecHL;
        _instructionTable[0x2C] = IncL; _instructionTable[0x2D] = DecL; _instructionTable[0x2E] = LdL_n; _instructionTable[0x2F] = Cpl;

        _instructionTable[0x30] = Jr_NC_n; _instructionTable[0x31] = LdSP_nn; _instructionTable[0x32] = Ld_HLD_A; _instructionTable[0x33] = IncSP;
        _instructionTable[0x34] = Inc_HL; _instructionTable[0x35] = Dec_HL; _instructionTable[0x36] = Ld_HL_n; _instructionTable[0x37] = Scf;
        _instructionTable[0x38] = Jr_C_n; _instructionTable[0x39] = AddHL_SP; _instructionTable[0x3A] = LdA_HLD; _instructionTable[0x3B] = DecSP;
        _instructionTable[0x3C] = IncA; _instructionTable[0x3D] = DecA; _instructionTable[0x3E] = LdA_n; _instructionTable[0x3F] = Ccf;

        _instructionTable[0x40] = LdB_B; _instructionTable[0x41] = LdB_C; _instructionTable[0x42] = LdB_D; _instructionTable[0x43] = LdB_E;
        _instructionTable[0x44] = LdB_H; _instructionTable[0x45] = LdB_L; _instructionTable[0x46] = LdB_HL; _instructionTable[0x47] = LdB_A;
        _instructionTable[0x48] = LdC_B; _instructionTable[0x49] = LdC_C; _instructionTable[0x4A] = LdC_D; _instructionTable[0x4B] = LdC_E;
        _instructionTable[0x4C] = LdC_H; _instructionTable[0x4D] = LdC_L; _instructionTable[0x4E] = LdC_HL; _instructionTable[0x4F] = LdC_A;

        _instructionTable[0x50] = LdD_B; _instructionTable[0x51] = LdD_C; _instructionTable[0x52] = LdD_D; _instructionTable[0x53] = LdD_E;
        _instructionTable[0x54] = LdD_H; _instructionTable[0x55] = LdD_L; _instructionTable[0x56] = LdD_HL; _instructionTable[0x57] = LdD_A;
        _instructionTable[0x58] = LdE_B; _instructionTable[0x59] = LdE_C; _instructionTable[0x5A] = LdE_D; _instructionTable[0x5B] = LdE_E;
        _instructionTable[0x5C] = LdE_H; _instructionTable[0x5D] = LdE_L; _instructionTable[0x5E] = LdE_HL; _instructionTable[0x5F] = LdE_A;

        _instructionTable[0x60] = LdH_B; _instructionTable[0x61] = LdH_C; _instructionTable[0x62] = LdH_D; _instructionTable[0x63] = LdH_E;
        _instructionTable[0x64] = LdH_H; _instructionTable[0x65] = LdH_L; _instructionTable[0x66] = LdH_HL; _instructionTable[0x67] = LdH_A;
        _instructionTable[0x68] = LdL_B; _instructionTable[0x69] = LdL_C; _instructionTable[0x6A] = LdL_D; _instructionTable[0x6B] = LdL_E;
        _instructionTable[0x6C] = LdL_H; _instructionTable[0x6D] = LdL_L; _instructionTable[0x6E] = LdL_HL; _instructionTable[0x6F] = LdL_A;

        _instructionTable[0x70] = Ld_HL_B; _instructionTable[0x71] = Ld_HL_C; _instructionTable[0x72] = Ld_HL_D; _instructionTable[0x73] = Ld_HL_E;
        _instructionTable[0x74] = Ld_HL_H; _instructionTable[0x75] = Ld_HL_L; _instructionTable[0x76] = Halt; _instructionTable[0x77] = Ld_HL_A;
        _instructionTable[0x78] = LdA_B; _instructionTable[0x79] = LdA_C; _instructionTable[0x7A] = LdA_D; _instructionTable[0x7B] = LdA_E;
        _instructionTable[0x7C] = LdA_H; _instructionTable[0x7D] = LdA_L; _instructionTable[0x7E] = LdA_HL; _instructionTable[0x7F] = LdA_A;

        _instructionTable[0x80] = AddA_B; _instructionTable[0x81] = AddA_C; _instructionTable[0x82] = AddA_D; _instructionTable[0x83] = AddA_E;
        _instructionTable[0x84] = AddA_H; _instructionTable[0x85] = AddA_L; _instructionTable[0x86] = AddA_HL; _instructionTable[0x87] = AddA_A;
        _instructionTable[0x88] = AdcA_B; _instructionTable[0x89] = AdcA_C; _instructionTable[0x8A] = AdcA_D; _instructionTable[0x8B] = AdcA_E;
        _instructionTable[0x8C] = AdcA_H; _instructionTable[0x8D] = AdcA_L; _instructionTable[0x8E] = AdcA_HL; _instructionTable[0x8F] = AdcA_A;

        _instructionTable[0x90] = SubA_B; _instructionTable[0x91] = SubA_C; _instructionTable[0x92] = SubA_D; _instructionTable[0x93] = SubA_E;
        _instructionTable[0x94] = SubA_H; _instructionTable[0x95] = SubA_L; _instructionTable[0x96] = SubA_HL; _instructionTable[0x97] = SubA_A;
        _instructionTable[0x98] = SbcA_B; _instructionTable[0x99] = SbcA_C; _instructionTable[0x9A] = SbcA_D; _instructionTable[0x9B] = SbcA_E;
        _instructionTable[0x9C] = SbcA_H; _instructionTable[0x9D] = SbcA_L; _instructionTable[0x9E] = SbcA_HL; _instructionTable[0x9F] = SbcA_A;

        _instructionTable[0xA0] = AndA_B; _instructionTable[0xA1] = AndA_C; _instructionTable[0xA2] = AndA_D; _instructionTable[0xA3] = AndA_E;
        _instructionTable[0xA4] = AndA_H; _instructionTable[0xA5] = AndA_L; _instructionTable[0xA6] = AndA_HL; _instructionTable[0xA7] = AndA_A;
        _instructionTable[0xA8] = XorA_B; _instructionTable[0xA9] = XorA_C; _instructionTable[0xAA] = XorA_D; _instructionTable[0xAB] = XorA_E;
        _instructionTable[0xAC] = XorA_H; _instructionTable[0xAD] = XorA_L; _instructionTable[0xAE] = XorA_HL; _instructionTable[0xAF] = XorA_A;

        _instructionTable[0xB0] = OrA_B; _instructionTable[0xB1] = OrA_C; _instructionTable[0xB2] = OrA_D; _instructionTable[0xB3] = OrA_E;
        _instructionTable[0xB4] = OrA_H; _instructionTable[0xB5] = OrA_L; _instructionTable[0xB6] = OrA_HL; _instructionTable[0xB7] = OrA_A;
        _instructionTable[0xB8] = CpA_B; _instructionTable[0xB9] = CpA_C; _instructionTable[0xBA] = CpA_D; _instructionTable[0xBB] = CpA_E;
        _instructionTable[0xBC] = CpA_H; _instructionTable[0xBD] = CpA_L; _instructionTable[0xBE] = CpA_HL; _instructionTable[0xBF] = CpA_A;

        _instructionTable[0xC0] = Ret_NZ; _instructionTable[0xC1] = PopBC; _instructionTable[0xC2] = Jp_NZ_nn; _instructionTable[0xC3] = Jp_nn;
        _instructionTable[0xC4] = Call_NZ_nn; _instructionTable[0xC5] = PushBC; _instructionTable[0xC6] = AddA_n; _instructionTable[0xC7] = Rst_00;
        _instructionTable[0xC8] = Ret_Z; _instructionTable[0xC9] = Ret; _instructionTable[0xCA] = Jp_Z_nn; _instructionTable[0xCB] = CBPrefix;
        _instructionTable[0xCC] = Call_Z_nn; _instructionTable[0xCD] = Call_nn; _instructionTable[0xCE] = AdcA_n; _instructionTable[0xCF] = Rst_08;

        _instructionTable[0xD0] = Ret_NC; _instructionTable[0xD1] = PopDE; _instructionTable[0xD2] = Jp_NC_nn; _instructionTable[0xD3] = Unknown;
        _instructionTable[0xD4] = Call_NC_nn; _instructionTable[0xD5] = PushDE; _instructionTable[0xD6] = SubA_n; _instructionTable[0xD7] = Rst_10;
        _instructionTable[0xD8] = Ret_C; _instructionTable[0xD9] = Reti; _instructionTable[0xDA] = Jp_C_nn; _instructionTable[0xDB] = Unknown;
        _instructionTable[0xDC] = Call_C_nn; _instructionTable[0xDD] = Unknown; _instructionTable[0xDE] = SbcA_n; _instructionTable[0xDF] = Rst_18;

        _instructionTable[0xE0] = Ld_n_A; _instructionTable[0xE1] = PopHL; _instructionTable[0xE2] = Ld_C_A; _instructionTable[0xE3] = Unknown;
        _instructionTable[0xE4] = Unknown; _instructionTable[0xE5] = PushHL; _instructionTable[0xE6] = AndA_n; _instructionTable[0xE7] = Rst_20;
        _instructionTable[0xE8] = AddSP_n; _instructionTable[0xE9] = JpHL; _instructionTable[0xEA] = Ld_nn_A; _instructionTable[0xEB] = Unknown;
        _instructionTable[0xEC] = Unknown; _instructionTable[0xED] = Unknown; _instructionTable[0xEE] = XorA_n; _instructionTable[0xEF] = Rst_28;

        _instructionTable[0xF0] = LdA_n; _instructionTable[0xF1] = PopAF; _instructionTable[0xF2] = LdA_C; _instructionTable[0xF3] = Di;
        _instructionTable[0xF4] = Unknown; _instructionTable[0xF5] = PushAF; _instructionTable[0xF6] = OrA_n; _instructionTable[0xF7] = Rst_30;
        _instructionTable[0xF8] = LdHL_SP_n; _instructionTable[0xF9] = LdSP_HL; _instructionTable[0xFA] = LdA_nn; _instructionTable[0xFB] = Ei;
        _instructionTable[0xFC] = Unknown; _instructionTable[0xFD] = Unknown; _instructionTable[0xFE] = CpA_n; _instructionTable[0xFF] = Rst_38;
    }

    private byte FetchByte() => _memory.Read(_registers.PC++);
    private ushort FetchWord() { byte l = FetchByte(); byte h = FetchByte(); return (ushort)(l | (h << 8)); }

    public void Step()
    {
        if (!_running) return;
        byte opcode = FetchByte();
        if (_instructionTable.TryGetValue(opcode, out var instruction)) instruction();
        else UnknownOpcode(opcode);
    }

    public void Run() { _running = true; while (_running) Step(); }
    public void Stop() { _running = false; }

    private void UnknownOpcode(byte opcode) { Console.WriteLine($"Unknown opcode: 0x{opcode:X2} at PC: 0x{_registers.PC - 1:X4}"); _running = false; }
    public void Reset() { _registers.Reset(); _cycles = 0; _running = false; _ime = false; }
    public void LoadPostBootState() { _registers.LoadPostBootState(); _cycles = 0; _running = false; _ime = false; }

    private void Nop() { _cycles += 4; }
    private void LdBC_nn() { _registers.BC = FetchWord(); _cycles += 12; }
    private void Ld_BC_A() { _memory.Write(_registers.BC, _registers.A); _cycles += 8; }
    private void IncBC() { _registers.BC = ALU.IncWord(_registers.BC); _cycles += 8; }
    private void IncB() { var r = ALU.Inc(_registers.B); _registers.B = r.Value; r.Apply(_registers); _cycles += 4; }
    private void DecB() { var r = ALU.Dec(_registers.B); _registers.B = r.Value; r.Apply(_registers); _cycles += 4; }
    private void LdB_n() { _registers.B = FetchByte(); _cycles += 8; }
    private void Rlca() { var r = ALU.RotateLeftCircular(_registers.A); _registers.A = r.Value; r.ApplyWithoutZero(_registers); _registers.ZeroFlag = false; _cycles += 4; }
    private void Ld_nn_SP() { ushort a = FetchWord(); _memory.WriteWord(a, _registers.SP); _cycles += 20; }
    private void AddHL_BC() { int r = _registers.HL + _registers.BC; _registers.SubtractFlag = false; _registers.HalfCarryFlag = ((_registers.HL & 0x0FFF) + (_registers.BC & 0x0FFF)) > 0x0FFF; _registers.CarryFlag = r > 0xFFFF; _registers.HL = (ushort)r; _cycles += 8; }
    private void LdA_BC() { _registers.A = _memory.Read(_registers.BC); _cycles += 8; }
    private void DecBC() { _registers.BC = ALU.DecWord(_registers.BC); _cycles += 8; }
    private void IncC() { var r = ALU.Inc(_registers.C); _registers.C = r.Value; r.Apply(_registers); _cycles += 4; }
    private void DecC() { var r = ALU.Dec(_registers.C); _registers.C = r.Value; r.Apply(_registers); _cycles += 4; }
    private void LdC_n() { _registers.C = FetchByte(); _cycles += 8; }
    private void Rrca() { var r = ALU.RotateRightCircular(_registers.A); _registers.A = r.Value; r.ApplyWithoutZero(_registers); _registers.ZeroFlag = false; _cycles += 4; }
    private void Stop() { _cycles += 4; }
    private void LdDE_nn() { _registers.DE = FetchWord(); _cycles += 12; }
    private void Ld_DE_A() { _memory.Write(_registers.DE, _registers.A); _cycles += 8; }
    private void IncDE() { _registers.DE = ALU.IncWord(_registers.DE); _cycles += 8; }
    private void IncD() { var r = ALU.Inc(_registers.D); _registers.D = r.Value; r.Apply(_registers); _cycles += 4; }
    private void DecD() { var r = ALU.Dec(_registers.D); _registers.D = r.Value; r.Apply(_registers); _cycles += 4; }
    private void LdD_n() { _registers.D = FetchByte(); _cycles += 8; }
    private void Rla() { bool c = _registers.CarryFlag; var r = ALU.RotateLeftThroughCarry(_registers.A, c); _registers.A = r.Value; r.ApplyWithoutZero(_registers); _registers.ZeroFlag = false; _cycles += 4; }
    private void Jr_n() { sbyte o = (sbyte)FetchByte(); _registers.PC = (ushort)(_registers.PC + o); _cycles += 12; }
    private void AddHL_DE() { int r = _registers.HL + _registers.DE; _registers.SubtractFlag = false; _registers.HalfCarryFlag = ((_registers.HL & 0x0FFF) + (_registers.DE & 0x0FFF)) > 0x0FFF; _registers.CarryFlag = r > 0xFFFF; _registers.HL = (ushort)r; _cycles += 8; }
    private void LdA_DE() { _registers.A = _memory.Read(_registers.DE); _cycles += 8; }
    private void DecDE() { _registers.DE = ALU.DecWord(_registers.DE); _cycles += 8; }
    private void IncE() { var r = ALU.Inc(_registers.E); _registers.E = r.Value; r.Apply(_registers); _cycles += 4; }
    private void DecE() { var r = ALU.Dec(_registers.E); _registers.E = r.Value; r.Apply(_registers); _cycles += 4; }
    private void LdE_n() { _registers.E = FetchByte(); _cycles += 8; }
    private void Rra() { bool c = _registers.CarryFlag; var r = ALU.RotateRightThroughCarry(_registers.A, c); _registers.A = r.Value; r.ApplyWithoutZero(_registers); _registers.ZeroFlag = false; _cycles += 4; }
    private void Jr_NZ_n() { sbyte o = (sbyte)FetchByte(); if (!_registers.ZeroFlag) { _registers.PC = (ushort)(_registers.PC + o); _cycles += 12; } else _cycles += 8; }
    private void LdHL_nn() { _registers.HL = FetchWord(); _cycles += 12; }
    private void Ld_HLI_A() { _memory.Write(_registers.HL, _registers.A); _registers.HL = ALU.IncWord(_registers.HL); _cycles += 8; }
    private void IncHL() { _registers.HL = ALU.IncWord(_registers.HL); _cycles += 8; }
    private void IncH() { var r = ALU.Inc(_registers.H); _registers.H = r.Value; r.Apply(_registers); _cycles += 4; }
    private void DecH() { var r = ALU.Dec(_registers.H); _registers.H = r.Value; r.Apply(_registers); _cycles += 4; }
    private void LdH_n() { _registers.H = FetchByte(); _cycles += 8; }
    private void Daa() { _cycles += 4; }
    private void Jr_Z_n() { sbyte o = (sbyte)FetchByte(); if (_registers.ZeroFlag) { _registers.PC = (ushort)(_registers.PC + o); _cycles += 12; } else _cycles += 8; }
    private void AddHL_HL() { int r = _registers.HL + _registers.HL; _registers.SubtractFlag = false; _registers.HalfCarryFlag = ((_registers.HL & 0x0FFF) + (_registers.HL & 0x0FFF)) > 0x0FFF; _registers.CarryFlag = r > 0xFFFF; _registers.HL = (ushort)r; _cycles += 8; }
    private void LdA_HLI() { _registers.A = _memory.Read(_registers.HL); _registers.HL = ALU.IncWord(_registers.HL); _cycles += 8; }
    private void DecHL() { _registers.HL = ALU.DecWord(_registers.HL); _cycles += 8; }
    private void IncL() { var r = ALU.Inc(_registers.L); _registers.L = r.Value; r.Apply(_registers); _cycles += 4; }
    private void DecL() { var r = ALU.Dec(_registers.L); _registers.L = r.Value; r.Apply(_registers); _cycles += 4; }
    private void LdL_n() { _registers.L = FetchByte(); _cycles += 8; }
    private void Cpl() { _registers.A = (byte)~_registers.A; _registers.SubtractFlag = true; _registers.HalfCarryFlag = true; _cycles += 4; }
    private void Jr_NC_n() { sbyte o = (sbyte)FetchByte(); if (!_registers.CarryFlag) { _registers.PC = (ushort)(_registers.PC + o); _cycles += 12; } else _cycles += 8; }
    private void LdSP_nn() { _registers.SP = FetchWord(); _cycles += 12; }
    private void Ld_HLD_A() { _memory.Write(_registers.HL, _registers.A); _registers.HL = ALU.DecWord(_registers.HL); _cycles += 8; }
    private void IncSP() { _registers.SP = ALU.IncWord(_registers.SP); _cycles += 8; }
    private void Inc_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.Inc(v); _memory.Write(_registers.HL, r.Value); r.Apply(_registers); _cycles += 12; }
    private void Dec_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.Dec(v); _memory.Write(_registers.HL, r.Value); r.Apply(_registers); _cycles += 12; }
    private void Ld_HL_n() { byte v = FetchByte(); _memory.Write(_registers.HL, v); _cycles += 12; }
    private void Scf() { _registers.CarryFlag = true; _registers.SubtractFlag = false; _registers.HalfCarryFlag = false; _cycles += 4; }
    private void Jr_C_n() { sbyte o = (sbyte)FetchByte(); if (_registers.CarryFlag) { _registers.PC = (ushort)(_registers.PC + o); _cycles += 12; } else _cycles += 8; }
    private void AddHL_SP() { int r = _registers.HL + _registers.SP; _registers.SubtractFlag = false; _registers.HalfCarryFlag = ((_registers.HL & 0x0FFF) + (_registers.SP & 0x0FFF)) > 0x0FFF; _registers.CarryFlag = r > 0xFFFF; _registers.HL = (ushort)r; _cycles += 8; }
    private void LdA_HLD() { _registers.A = _memory.Read(_registers.HL); _registers.HL = ALU.DecWord(_registers.HL); _cycles += 8; }
    private void DecSP() { _registers.SP = ALU.DecWord(_registers.SP); _cycles += 8; }
    private void IncA() { var r = ALU.Inc(_registers.A); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void DecA() { var r = ALU.Dec(_registers.A); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void LdA_n() { _registers.A = FetchByte(); _cycles += 8; }
    private void Ccf() { _registers.CarryFlag = !_registers.CarryFlag; _registers.SubtractFlag = false; _registers.HalfCarryFlag = false; _cycles += 4; }

    private void LdB_B() { _registers.B = _registers.B; _cycles += 4; }
    private void LdB_C() { _registers.B = _registers.C; _cycles += 4; }
    private void LdB_D() { _registers.B = _registers.D; _cycles += 4; }
    private void LdB_E() { _registers.B = _registers.E; _cycles += 4; }
    private void LdB_H() { _registers.B = _registers.H; _cycles += 4; }
    private void LdB_L() { _registers.B = _registers.L; _cycles += 4; }
    private void LdB_HL() { _registers.B = _memory.Read(_registers.HL); _cycles += 8; }
    private void LdB_A() { _registers.B = _registers.A; _cycles += 4; }
    private void LdC_B() { _registers.C = _registers.B; _cycles += 4; }
    private void LdC_C() { _registers.C = _registers.C; _cycles += 4; }
    private void LdC_D() { _registers.C = _registers.D; _cycles += 4; }
    private void LdC_E() { _registers.C = _registers.E; _cycles += 4; }
    private void LdC_H() { _registers.C = _registers.H; _cycles += 4; }
    private void LdC_L() { _registers.C = _registers.L; _cycles += 4; }
    private void LdC_HL() { _registers.C = _memory.Read(_registers.HL); _cycles += 8; }
    private void LdC_A() { _registers.C = _registers.A; _cycles += 4; }
    private void LdD_B() { _registers.D = _registers.B; _cycles += 4; }
    private void LdD_C() { _registers.D = _registers.C; _cycles += 4; }
    private void LdD_D() { _registers.D = _registers.D; _cycles += 4; }
    private void LdD_E() { _registers.D = _registers.E; _cycles += 4; }
    private void LdD_H() { _registers.D = _registers.H; _cycles += 4; }
    private void LdD_L() { _registers.D = _registers.L; _cycles += 4; }
    private void LdD_HL() { _registers.D = _memory.Read(_registers.HL); _cycles += 8; }
    private void LdD_A() { _registers.D = _registers.A; _cycles += 4; }
    private void LdE_B() { _registers.E = _registers.B; _cycles += 4; }
    private void LdE_C() { _registers.E = _registers.C; _cycles += 4; }
    private void LdE_D() { _registers.E = _registers.D; _cycles += 4; }
    private void LdE_E() { _registers.E = _registers.E; _cycles += 4; }
    private void LdE_H() { _registers.E = _registers.H; _cycles += 4; }
    private void LdE_L() { _registers.E = _registers.L; _cycles += 4; }
    private void LdE_HL() { _registers.E = _memory.Read(_registers.HL); _cycles += 8; }
    private void LdE_A() { _registers.E = _registers.A; _cycles += 4; }
    private void LdH_B() { _registers.H = _registers.B; _cycles += 4; }
    private void LdH_C() { _registers.H = _registers.C; _cycles += 4; }
    private void LdH_D() { _registers.H = _registers.D; _cycles += 4; }
    private void LdH_E() { _registers.H = _registers.E; _cycles += 4; }
    private void LdH_H() { _registers.H = _registers.H; _cycles += 4; }
    private void LdH_L() { _registers.H = _registers.L; _cycles += 4; }
    private void LdH_HL() { _registers.H = _memory.Read(_registers.HL); _cycles += 8; }
    private void LdH_A() { _registers.H = _registers.A; _cycles += 4; }
    private void LdL_B() { _registers.L = _registers.B; _cycles += 4; }
    private void LdL_C() { _registers.L = _registers.C; _cycles += 4; }
    private void LdL_D() { _registers.L = _registers.D; _cycles += 4; }
    private void LdL_E() { _registers.L = _registers.E; _cycles += 4; }
    private void LdL_H() { _registers.L = _registers.H; _cycles += 4; }
    private void LdL_L() { _registers.L = _registers.L; _cycles += 4; }
    private void LdL_HL() { _registers.L = _memory.Read(_registers.HL); _cycles += 8; }
    private void LdL_A() { _registers.L = _registers.A; _cycles += 4; }
    private void Ld_HL_B() { _memory.Write(_registers.HL, _registers.B); _cycles += 8; }
    private void Ld_HL_C() { _memory.Write(_registers.HL, _registers.C); _cycles += 8; }
    private void Ld_HL_D() { _memory.Write(_registers.HL, _registers.D); _cycles += 8; }
    private void Ld_HL_E() { _memory.Write(_registers.HL, _registers.E); _cycles += 8; }
    private void Ld_HL_H() { _memory.Write(_registers.HL, _registers.H); _cycles += 8; }
    private void Ld_HL_L() { _memory.Write(_registers.HL, _registers.L); _cycles += 8; }
    private void Halt() { _cycles += 4; }
    private void Ld_HL_A() { _memory.Write(_registers.HL, _registers.A); _cycles += 8; }
    private void LdA_B() { _registers.A = _registers.B; _cycles += 4; }
    private void LdA_C() { _registers.A = _registers.C; _cycles += 4; }
    private void LdA_D() { _registers.A = _registers.D; _cycles += 4; }
    private void LdA_E() { _registers.A = _registers.E; _cycles += 4; }
    private void LdA_H() { _registers.A = _registers.H; _cycles += 4; }
    private void LdA_L() { _registers.A = _registers.L; _cycles += 4; }
    private void LdA_HL() { _registers.A = _memory.Read(_registers.HL); _cycles += 8; }
    private void LdA_A() { _registers.A = _registers.A; _cycles += 4; }

    private void AddA_B() { var r = ALU.Add(_registers.A, _registers.B); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AddA_C() { var r = ALU.Add(_registers.A, _registers.C); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AddA_D() { var r = ALU.Add(_registers.A, _registers.D); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AddA_E() { var r = ALU.Add(_registers.A, _registers.E); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AddA_H() { var r = ALU.Add(_registers.A, _registers.H); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AddA_L() { var r = ALU.Add(_registers.A, _registers.L); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AddA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.Add(_registers.A, v); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void AddA_A() { var r = ALU.Add(_registers.A, _registers.A); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AdcA_B() { var r = ALU.AddWithCarry(_registers.A, _registers.B, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AdcA_C() { var r = ALU.AddWithCarry(_registers.A, _registers.C, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AdcA_D() { var r = ALU.AddWithCarry(_registers.A, _registers.D, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AdcA_E() { var r = ALU.AddWithCarry(_registers.A, _registers.E, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AdcA_H() { var r = ALU.AddWithCarry(_registers.A, _registers.H, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AdcA_L() { var r = ALU.AddWithCarry(_registers.A, _registers.L, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AdcA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.AddWithCarry(_registers.A, v, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void AdcA_A() { var r = ALU.AddWithCarry(_registers.A, _registers.A, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SubA_B() { var r = ALU.Sub(_registers.A, _registers.B); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SubA_C() { var r = ALU.Sub(_registers.A, _registers.C); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SubA_D() { var r = ALU.Sub(_registers.A, _registers.D); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SubA_E() { var r = ALU.Sub(_registers.A, _registers.E); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SubA_H() { var r = ALU.Sub(_registers.A, _registers.H); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SubA_L() { var r = ALU.Sub(_registers.A, _registers.L); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SubA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.Sub(_registers.A, v); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void SubA_A() { var r = ALU.Sub(_registers.A, _registers.A); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SbcA_B() { var r = ALU.SubWithCarry(_registers.A, _registers.B, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SbcA_C() { var r = ALU.SubWithCarry(_registers.A, _registers.C, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SbcA_D() { var r = ALU.SubWithCarry(_registers.A, _registers.D, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SbcA_E() { var r = ALU.SubWithCarry(_registers.A, _registers.E, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SbcA_H() { var r = ALU.SubWithCarry(_registers.A, _registers.H, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SbcA_L() { var r = ALU.SubWithCarry(_registers.A, _registers.L, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void SbcA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.SubWithCarry(_registers.A, v, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void SbcA_A() { var r = ALU.SubWithCarry(_registers.A, _registers.A, _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AndA_B() { var r = ALU.And(_registers.A, _registers.B); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AndA_C() { var r = ALU.And(_registers.A, _registers.C); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AndA_D() { var r = ALU.And(_registers.A, _registers.D); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AndA_E() { var r = ALU.And(_registers.A, _registers.E); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AndA_H() { var r = ALU.And(_registers.A, _registers.H); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AndA_L() { var r = ALU.And(_registers.A, _registers.L); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void AndA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.And(_registers.A, v); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void AndA_A() { var r = ALU.And(_registers.A, _registers.A); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void XorA_B() { var r = ALU.Xor(_registers.A, _registers.B); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void XorA_C() { var r = ALU.Xor(_registers.A, _registers.C); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void XorA_D() { var r = ALU.Xor(_registers.A, _registers.D); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void XorA_E() { var r = ALU.Xor(_registers.A, _registers.E); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void XorA_H() { var r = ALU.Xor(_registers.A, _registers.H); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void XorA_L() { var r = ALU.Xor(_registers.A, _registers.L); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void XorA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.Xor(_registers.A, v); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void XorA_A() { var r = ALU.Xor(_registers.A, _registers.A); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void OrA_B() { var r = ALU.Or(_registers.A, _registers.B); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void OrA_C() { var r = ALU.Or(_registers.A, _registers.C); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void OrA_D() { var r = ALU.Or(_registers.A, _registers.D); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void OrA_E() { var r = ALU.Or(_registers.A, _registers.E); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void OrA_H() { var r = ALU.Or(_registers.A, _registers.H); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void OrA_L() { var r = ALU.Or(_registers.A, _registers.L); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void OrA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.Or(_registers.A, v); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void OrA_A() { var r = ALU.Or(_registers.A, _registers.A); _registers.A = r.Value; r.Apply(_registers); _cycles += 4; }
    private void CpA_B() { var r = ALU.Cp(_registers.A, _registers.B); r.Apply(_registers); _cycles += 4; }
    private void CpA_C() { var r = ALU.Cp(_registers.A, _registers.C); r.Apply(_registers); _cycles += 4; }
    private void CpA_D() { var r = ALU.Cp(_registers.A, _registers.D); r.Apply(_registers); _cycles += 4; }
    private void CpA_E() { var r = ALU.Cp(_registers.A, _registers.E); r.Apply(_registers); _cycles += 4; }
    private void CpA_H() { var r = ALU.Cp(_registers.A, _registers.H); r.Apply(_registers); _cycles += 4; }
    private void CpA_L() { var r = ALU.Cp(_registers.A, _registers.L); r.Apply(_registers); _cycles += 4; }
    private void CpA_HL() { byte v = _memory.Read(_registers.HL); var r = ALU.Cp(_registers.A, v); r.Apply(_registers); _cycles += 8; }
    private void CpA_A() { var r = ALU.Cp(_registers.A, _registers.A); r.Apply(_registers); _cycles += 4; }

    private void Ret_NZ() { if (!_registers.ZeroFlag) { _registers.PC = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 20; } else { _cycles += 8; } }
    private void PopBC() { _registers.BC = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 12; }
    private void Jp_NZ_nn() { if (!_registers.ZeroFlag) { _registers.PC = FetchWord(); _cycles += 16; } else { _registers.PC += 2; _cycles += 12; } }
    private void Jp_nn() { _registers.PC = FetchWord(); _cycles += 16; }
    private void Call_NZ_nn() { ushort a = FetchWord(); if (!_registers.ZeroFlag) { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = a; _cycles += 24; } else { _cycles += 12; } }
    private void PushBC() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.BC); _cycles += 16; }
    private void AddA_n() { var r = ALU.Add(_registers.A, FetchByte()); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void Rst_00() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0000; _cycles += 16; }
    private void Ret_Z() { if (_registers.ZeroFlag) { _registers.PC = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 20; } else { _cycles += 8; } }
    private void Ret() { _registers.PC = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 16; }
    private void Jp_Z_nn() { if (_registers.ZeroFlag) { _registers.PC = FetchWord(); _cycles += 16; } else { _registers.PC += 2; _cycles += 12; } }
    private void Call_Z_nn() { ushort a = FetchWord(); if (_registers.ZeroFlag) { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = a; _cycles += 24; } else { _cycles += 12; } }
    private void Call_nn() { ushort a = FetchWord(); _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = a; _cycles += 24; }
    private void AdcA_n() { var r = ALU.AddWithCarry(_registers.A, FetchByte(), _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void Rst_08() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0008; _cycles += 16; }
    private void Ret_NC() { if (!_registers.CarryFlag) { _registers.PC = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 20; } else { _cycles += 8; } }
    private void PopDE() { _registers.DE = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 12; }
    private void Jp_NC_nn() { if (!_registers.CarryFlag) { _registers.PC = FetchWord(); _cycles += 16; } else { _registers.PC += 2; _cycles += 12; } }
    private void Call_NC_nn() { ushort a = FetchWord(); if (!_registers.CarryFlag) { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = a; _cycles += 24; } else { _cycles += 12; } }
    private void PushDE() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.DE); _cycles += 16; }
    private void SubA_n() { var r = ALU.Sub(_registers.A, FetchByte()); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void Rst_10() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0010; _cycles += 16; }
    private void Ret_C() { if (_registers.CarryFlag) { _registers.PC = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 20; } else { _cycles += 8; } }
    private void Reti() { _registers.PC = _memory.ReadWord(_registers.SP); _registers.SP += 2; _ime = true; _cycles += 16; }
    private void Jp_C_nn() { if (_registers.CarryFlag) { _registers.PC = FetchWord(); _cycles += 16; } else { _registers.PC += 2; _cycles += 12; } }
    private void Call_C_nn() { ushort a = FetchWord(); if (_registers.CarryFlag) { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = a; _cycles += 24; } else { _cycles += 12; } }
    private void SbcA_n() { var r = ALU.SubWithCarry(_registers.A, FetchByte(), _registers.CarryFlag); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void Rst_18() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0018; _cycles += 16; }
    private void Ld_n_A() { _memory.Write((ushort)(0xFF00 + FetchByte()), _registers.A); _cycles += 12; }
    private void PopHL() { _registers.HL = _memory.ReadWord(_registers.SP); _registers.SP += 2; _cycles += 12; }
    private void Ld_C_A() { _memory.Write((ushort)(0xFF00 + _registers.C), _registers.A); _cycles += 8; }
    private void PushHL() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.HL); _cycles += 16; }
    private void AndA_n() { var r = ALU.And(_registers.A, FetchByte()); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void Rst_20() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0020; _cycles += 16; }
    private void AddSP_n() { sbyte v = (sbyte)FetchByte(); int r = _registers.SP + v; _registers.SubtractFlag = false; _registers.HalfCarryFlag = ((_registers.SP & 0x0FFF) + (v & 0x0FFF)) > 0x0FFF; _registers.CarryFlag = r > 0xFFFF; _registers.SP = (ushort)r; _cycles += 16; }
    private void JpHL() { _registers.PC = _registers.HL; _cycles += 4; }
    private void Ld_nn_A() { ushort a = FetchWord(); _memory.Write(a, _registers.A); _cycles += 16; }
    private void XorA_n() { var r = ALU.Xor(_registers.A, FetchByte()); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void Rst_28() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0028; _cycles += 16; }
    private void LdA_n() { _registers.A = _memory.Read((ushort)(0xFF00 + FetchByte())); _cycles += 12; }
    private void PopAF() { _registers.AF = _memory.ReadWord(_registers.SP); _registers.SP += 2; _registers.F = (byte)(_registers.F & 0xF0); _cycles += 12; }
    private void LdA_C() { _registers.A = _memory.Read((ushort)(0xFF00 + _registers.C)); _cycles += 8; }
    private void Di() { _ime = false; _cycles += 4; }
    private void PushAF() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.AF); _cycles += 16; }
    private void OrA_n() { var r = ALU.Or(_registers.A, FetchByte()); _registers.A = r.Value; r.Apply(_registers); _cycles += 8; }
    private void Rst_30() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0030; _cycles += 16; }
    private void LdHL_SP_n() { sbyte v = (sbyte)FetchByte(); int r = _registers.SP + v; _registers.SubtractFlag = false; _registers.HalfCarryFlag = ((_registers.SP & 0x0FFF) + (v & 0x0FFF)) > 0x0FFF; _registers.CarryFlag = r > 0xFFFF; _registers.HL = (ushort)r; _cycles += 12; }
    private void LdSP_HL() { _registers.SP = _registers.HL; _cycles += 8; }
    private void LdA_nn() { _registers.A = _memory.Read(FetchWord()); _cycles += 16; }
    private void Ei() { _ime = true; _cycles += 4; }
    private void CpA_n() { var r = ALU.Cp(_registers.A, FetchByte()); r.Apply(_registers); _cycles += 8; }
    private void Rst_38() { _registers.SP -= 2; _memory.WriteWord(_registers.SP, _registers.PC); _registers.PC = 0x0038; _cycles += 16; }

    private void CBPrefix()
    {
        byte cb = FetchByte();
        _cycles += 4;
        ExecuteCB(cb);
    }

    private void ExecuteCB(byte cb)
    {
        byte r = (byte)(cb & 0x07);
        byte bit = (byte)((cb & 0x38) >> 3);

        switch (cb >> 6)
        {
            case 0:
                switch (cb & 0x07)
                {
                    case 0x00: RLC_r(r); break;
                    case 0x01: RRC_r(r); break;
                    case 0x02: RL_r(r); break;
                    case 0x03: RR_r(r); break;
                    case 0x04: SLA_r(r); break;
                    case 0x05: SRA_r(r); break;
                    case 0x06: SWAP_r(r); break;
                    case 0x07: SRL_r(r); break;
                }
                break;
            case 1: BIT_b_r(bit, r); break;
            case 2: RES_b_r(bit, r); break;
            case 3: SET_b_r(bit, r); break;
        }
    }

    private byte GetRegister(byte r) => r switch
    {
        0 => _registers.B,
        1 => _registers.C,
        2 => _registers.D,
        3 => _registers.E,
        4 => _registers.H,
        5 => _registers.L,
        6 => _memory.Read(_registers.HL),
        7 => _registers.A,
        _ => 0
    };

    private void SetRegister(byte r, byte value)
    {
        switch (r)
        {
            case 0: _registers.B = value; break;
            case 1: _registers.C = value; break;
            case 2: _registers.D = value; break;
            case 3: _registers.E = value; break;
            case 4: _registers.H = value; break;
            case 5: _registers.L = value; break;
            case 6: _memory.Write(_registers.HL, value); break;
            case 7: _registers.A = value; break;
        }
    }

    private void RLC_r(byte r) { byte v = GetRegister(r); var res = ALU.RotateLeftCircular(v); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    private void RRC_r(byte r) { byte v = GetRegister(r); var res = ALU.RotateRightCircular(v); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    private void RL_r(byte r) { byte v = GetRegister(r); var res = ALU.RotateLeftThroughCarry(v, _registers.CarryFlag); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    private void RR_r(byte r) { byte v = GetRegister(r); var res = ALU.RotateRightThroughCarry(v, _registers.CarryFlag); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    private void SLA_r(byte r) { byte v = GetRegister(r); var res = ALU.ShiftLeft(v); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    private void SRA_r(byte r) { byte v = GetRegister(r); var res = ALU.ShiftRightArithmetic(v); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    private void SWAP_r(byte r) { byte v = GetRegister(r); var res = ALU.Swap(v); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    private void SRL_r(byte r) { byte v = GetRegister(r); var res = ALU.ShiftRight(v); SetRegister(r, res.Value); res.Apply(_registers); _cycles += r == 6 ? 16 : 8; }
    
    private void BIT_b_r(byte bit, byte r) 
    { 
        byte v = GetRegister(r); 
        _registers.ZeroFlag = (v & (1 << bit)) == 0; 
        _registers.SubtractFlag = false; 
        _registers.HalfCarryFlag = true; 
        _cycles += r == 6 ? 12 : 8; 
    }
    
    private void RES_b_r(byte bit, byte r) 
    { 
        byte v = GetRegister(r); 
        SetRegister(r, (byte)(v & ~(1 << bit))); 
        _cycles += r == 6 ? 16 : 8; 
    }
    
    private void SET_b_r(byte bit, byte r) 
    { 
        byte v = GetRegister(r); 
        SetRegister(r, (byte)(v | (1 << bit))); 
        _cycles += r == 6 ? 16 : 8; 
    }

    private void Unknown() { UnknownOpcode(0x00); }
}