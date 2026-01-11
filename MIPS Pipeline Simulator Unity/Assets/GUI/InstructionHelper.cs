using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EncodingException : Exception
{
    public EncodingException(string msg) : base(msg){}
}

public static class InstructionHelper
{
    private static readonly string[] RegNames = new string[]
    {
        "$zero", "$at", "$v0", "$v1", "$a0", "$a1", "$a2", "$a3",
        "$t0", "$t1", "$t2", "$t3", "$t4", "$t5", "$t6", "$t7",
        "$s0", "$s1", "$s2", "$s3", "$s4", "$s5", "$s6", "$s7",
        "$t8", "$t9", "$k0", "$k1", "$gp", "$sp", "$fp", "$ra"
    };

    public static string Disassemble(uint instruction)
    {
        if (instruction == 0)
            return "NOP";

        uint op = (instruction >> 26) & 0x3F;
        uint rs = (instruction >> 21) & 0x1F;
        uint rt = (instruction >> 16) & 0x1F;
        uint rd = (instruction >> 11) & 0x1F;
        uint sa = (instruction >> 6) & 0x1F;
        uint func = instruction & 0x3F;
        short imm = (short)(instruction & 0xFFFF);
        uint adr = instruction & 0x3FFFFF;

        switch (op)
        {
            case 0x00:  // R-type
                switch (func)
                {
                    case 0x20:  //ADD
                        return $"ADD {RegNames[rd]}, {RegNames[rs]}, {RegNames[rt]}";
                    case 0x22:  //SUB
                        return $"SUB {RegNames[rd]}, {RegNames[rs]}, {RegNames[rt]}";
                    case 0x00:  //SLL
                        return $"SLL {RegNames[rd]}, {RegNames[rt]}, {sa}";
                    case 0x02:  //SRL
                        return $"SRL {RegNames[rd]}, {RegNames[rt]}, {sa}";
                    case 0x03:  //SRA
                        return $"SRA {RegNames[rd]}, {RegNames[rt]}, {sa}";
                    case 0x24:  //AND
                        return $"AND {RegNames[rd]}, {RegNames[rs]}, {RegNames[rt]}";
                    case 0x25:  //OR
                        return $"OR {RegNames[rd]}, {RegNames[rs]}, {RegNames[rt]}";
                    case 0x26:  //XOR
                        return $"XOR {RegNames[rd]}, {RegNames[rs]}, {RegNames[rt]}";
                    default:
                        throw new EncodingException($"The func={Convert.ToString(func, 2)} is not suported");
                }
            case 0x1A:  // JR
                return $"JR {RegNames[rs]}";
            case 0x08:  // ADDI
                return $"ADDI {RegNames[rt]}, {RegNames[rs]}, {imm}";
            case 0x23:  // LW
                return $"LW {RegNames[rt]}, {imm}({RegNames[rs]})";
            case 0x2B:  // SW
                return $"SW {RegNames[rt]}, {imm}({RegNames[rs]})";
            case 0x04:  // BEQ
                return $"BEQ {RegNames[rs]}, {RegNames[rt]}, {imm}";
            case 0x05:  // BNE
                return $"BNE {RegNames[rs]}, {RegNames[rt]}, {imm}";
            case 0x01:  // BGEZ
                return $"BGEZ {RegNames[rs]}, {imm}";
            case 0x07:  // BGTZ
                return $"BGTZ {RegNames[rs]}, {imm}";
            case 0x02:  // J
                return $"J {adr}";
            case 0x03:  // JAL
                return $"JAL {adr}";
            default:
                throw new EncodingException($"The opCode={Convert.ToString(op, 2)} is not suported");
        }
    }

    public static uint Assemble(string instruction)
    {
        instruction = instruction.Trim().Replace(",", "").Replace("(", " ").Replace(")", "");
        string[] parts = instruction.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return 0;

        string mnemonic = parts[0].ToUpper();

        switch (mnemonic)
        {
            case "NOP":
                return 0;
            case "ADD":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRType(instruction, parts[2], parts[3], parts[1], 0x20);
                }
                return 0;
            case "SUB":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRType(instruction, parts[2], parts[3], parts[1], 0x22);
                }
                return 0;
            case "SLL":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRTypeShiftOperation(instruction, parts[2], parts[1], parts[3], 0x00);
                }
                return 0;
            case "SRL":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRTypeShiftOperation(instruction, parts[2], parts[1], parts[3], 0x02);
                }
                return 0;
            case "SRA":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRTypeShiftOperation(instruction, parts[2], parts[1], parts[3], 0x03);
                }
                return 0;
            case "AND":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRType(instruction, parts[2], parts[3], parts[1], 0x24);
                }
                return 0;
            case "OR":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRType(instruction, parts[2], parts[3], parts[1], 0x25);
                }
                return 0;
            case "XOR":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeRType(instruction, parts[2], parts[3], parts[1], 0x26);
                }
                return 0;
            case "JR":
                if (IsValidInstruction(parts, 2))
                {
                    return EncodeIType(instruction, 0x1A, parts[1], "$zero", "0");
                }
                return 0;
            case "ADDI":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeIType(instruction, 0x08, parts[2], parts[1], parts[3]);
                }
                return 0;
            case "LW":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeIType(instruction, 0x23, parts[3], parts[1], parts[2]);
                }
                return 0;
            case "SW":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeIType(instruction, 0x2B, parts[3], parts[1], parts[2]);
                }
                return 0;
            case "BEQ":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeIType(instruction, 0x04, parts[1], parts[2], parts[3]);
                }
                return 0;
            case "BNE":
                if (IsValidInstruction(parts, 4))
                {
                    return EncodeIType(instruction, 0x05, parts[1], parts[2], parts[3]);
                }
                return 0;
            case "BGEZ":
                if (IsValidInstruction(parts, 3))
                {
                    return EncodeIType(instruction, 0x01, parts[1], "$zero", parts[2]);
                }
                return 0;
            case "BGTZ":
                if (IsValidInstruction(parts, 3))
                {
                    return EncodeIType(instruction, 0x07, parts[1], "$zero", parts[2]);
                }
                return 0;
            case "J":
                if (IsValidInstruction(parts, 2))
                {
                    return EncodeJType(instruction, 0x02, parts[1]);
                }
                return 0;
            case "JAL":
                if (IsValidInstruction(parts, 2))
                {
                    return EncodeJType(instruction, 0x03, parts[1]);
                }
                return 0;
            default:
                throw new EncodingException($"Mnemonic:{mnemonic} was not recognized");
        }
    }

    private static bool IsValidInstruction(string[] parts, int nr)
    {
        return parts.Length == nr;
    }

    private static uint EncodeRType(string instruction, string rsName, string rtName, string rdName, uint funct)
    {
        try
        {
            uint rs = GetRegNumber(rsName);
            uint rt = GetRegNumber(rtName);
            uint rd = GetRegNumber(rdName);

            return (rs << 21) | (rt << 16) | (rd << 11) | funct;
        }
        catch (EncodingException ex)
        {
            Debug.LogError($"Encoding failed for {instruction} because of:{ex.Message}, 0x00000000 will be returned");
            return 0x00000000;
        }
    }

    private static uint EncodeRTypeShiftOperation(string instruction, string rtName, string rdName, string saStr, uint funct)
    {
        try
        {
            uint rt = GetRegNumber(rtName);
            uint rd = GetRegNumber(rdName);
            uint sa = GetShiftAmount(saStr);

            return (rt << 16) | (rd << 11) | (sa << 6) | funct;
        }
        catch (EncodingException ex)
        {
            Debug.LogError($"Encoding failed for {instruction} because of:{ex.Message}, 0x00000000 will be returned");
            return 0x00000000;
        }
    }

    private static uint EncodeIType(string instruction, uint op, string rsName, string rtName, string immStr)
    {
        try
        {
            uint rs = GetRegNumber(rsName);
            uint rt = GetRegNumber(rtName);
            short imm = GetImm(immStr);

            return (op << 26) | (rs << 21) | (rt << 16) | (ushort)imm;
        }
        catch (EncodingException ex)
        {
            Debug.LogError($"Encoding failed for {instruction} because of:{ex.Message}, 0x00000000 will be returned");
            return 0x00000000;
        }
    }

    private static uint EncodeJType(string instruction, uint op, string adrStr)
    {
        try
        {
            uint imm = GetAdr(adrStr);

            return (op << 26) | imm;
        }
        catch (EncodingException ex)
        {
            Debug.LogError($"Encoding failed for {instruction} because of:{ex.Message}, 0x00000000 will be returned");
            return 0x00000000;
        }
    }

    private static uint GetRegNumber(string name)
    {
        name = name.ToLower();
        int index = Array.IndexOf(RegNames, name);

        if (index != -1)
            return (uint)index;

        if (name.StartsWith("$") && int.TryParse(name.Substring(1), out int num))
            return (uint)num;

        throw new EncodingException($"Register name:{name} is not valid");
    }

    private static uint GetShiftAmount(string saStr)
    {
        if (uint.TryParse(saStr, out uint sa) && sa < 32)
            return sa;

        throw new EncodingException($"Invalid Shift Amount value:{sa}");
    }

    private static short GetImm(string immStr)
    {
        if (immStr.StartsWith("0x") || immStr.StartsWith("0X"))
            return Convert.ToInt16(immStr.Substring(2), 16);

        if (short.TryParse(immStr, out short imm))
            return imm;

        throw new EncodingException($"Invalid Immediate value:{imm}");
    }

    private static uint GetAdr(string adrStr)
    {
        if (uint.TryParse(adrStr, out uint adr) && adr <= 67108863)
            return adr;

        throw new EncodingException($"Invalid Immediate value:{adr}");
    }
}
