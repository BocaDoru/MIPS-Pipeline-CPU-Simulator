using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Signal", menuName = "Signals/Signal")]
[Serializable]
public class Signal : ScriptableObject
{
    public BitArray _value;

    private readonly object _syncRoot = new object();

    public BitArray Value
    {
        set
        {
            if (_value == null)
            {
                _value = new BitArray(value.Bits);
            }
            lock (_syncRoot)
            {
                _value.Copy(value);
            }
        }
        get => _value;
    }

    public void ResetSignal()
    {
        Value = new BitArray(0, Value.Length);
    }
    public void ResetSignal(int length)
    {
        Value = new BitArray(0, length);
    }
}


[Serializable]
public class BitArray
{
    public bool[] _bits;
    public int Length;

    private readonly object _syncRoot = new object();

    public bool[] Bits
    {
        set
        {
            lock (_syncRoot)
            {
                Copy(value);
            }
        }
        get => _bits;
    }

    public BitArray(){
        this.Length = 0;
        _bits = new bool[0];
    }

    public BitArray(int length) {
        this.Length = length;
        _bits = new bool[length];
    }

    public BitArray(bool[] bits)
    {
        Length = bits.Length;
        Copy(bits);
    }

    public static bool IsZero(BitArray a) => (int)a == 0;

    public static bool IsGreaterThanZero(BitArray a) => (int)a > 0;

    public BitArray(int value, int length) => CreateBitArray(BitConverter.GetBytes(value), length);

    public BitArray(uint value, int length) => CreateBitArray(BitConverter.GetBytes(value), length);

    public BitArray(short value, int length) => CreateBitArray(BitConverter.GetBytes(value), length);

    public BitArray(ushort value, int length) => CreateBitArray(BitConverter.GetBytes(value), length);

    public BitArray(char value, int length) => CreateBitArray(BitConverter.GetBytes(value), length);

    public BitArray(byte value, int length) => CreateBitArray(new byte[] { value }, length);

    public BitArray(bool value)
    {
        Bits = new bool[] { value };
        Length = 1;
    }

    public static BitArray operator <<(BitArray bitArray, int shift)
    {
        BitArray result = new BitArray(bitArray.Length);
        for (int i = bitArray.Length - 1; i >= shift; i--)
            result.Bits[i] = bitArray.Bits[i - shift];

        return result;
    }
    public static BitArray operator >>(BitArray bitArray, int shift)
    {
        BitArray result = new BitArray(bitArray.Length);
        for (int i = 0; i < bitArray.Length - shift; i++)
            result.Bits[i] = bitArray.Bits[i + shift];

        for (int i = bitArray.Length - shift; i < bitArray.Length; i++)
            result.Bits[i] = false;

        return result;
    }

    public static BitArray ShiftRightArithmetic(BitArray bitArray, int shift)
    {
        BitArray result = new BitArray(bitArray.Length);
        for (int i = 0; i < bitArray.Length - shift; i++)
            result.Bits[i] = bitArray.Bits[i + shift];

        for (int i = bitArray.Length - shift; i < bitArray.Length - 1; i++)
            result.Bits[i] = bitArray.Bits[bitArray.Length - 1];

        return result;
    }

    public static BitArray operator +(BitArray a, BitArray b) => ExecuteOpeartion(a, b, (x, y) => x + y);

    public static BitArray operator -(BitArray a, BitArray b) => ExecuteOpeartion(a, b, (x, y) => x - y);

    public static BitArray operator &(BitArray a, BitArray b) => ExecuteOpeartion(a, b, (x, y) => x & y);

    public static BitArray operator |(BitArray a, BitArray b) => ExecuteOpeartion(a, b, (x, y) => x | y);

    public static BitArray operator ^(BitArray a, BitArray b) => ExecuteOpeartion(a, b, (x, y) => x ^ y);

    public static BitArray operator !(BitArray a)
    {
        BitArray result = new BitArray(a.Length);

        for (int i = 0; i < a.Length; i++)
        {
            result.Bits[i] = !a.Bits[i];
        }

        return result;
    }

    public static BitArray ExecuteOpeartion(BitArray a, BitArray b, Func<uint, uint, uint> operation)
    {
        uint aUint = (uint)a;
        uint bUint = (uint)b;

        int length = Math.Max(a.Length, b.Length);

        return new BitArray(operation(aUint, bUint), length);
    }

    public static explicit operator BitArray(uint value) => ConvertFromByteArray(BitConverter.GetBytes(value));

    public static explicit operator BitArray(int value) => ConvertFromByteArray(BitConverter.GetBytes(value));

    public static explicit operator BitArray(ushort value) => ConvertFromByteArray(BitConverter.GetBytes(value));

    public static explicit operator BitArray(short value) => ConvertFromByteArray(BitConverter.GetBytes(value));

    public static explicit operator BitArray(char value) => ConvertFromByteArray(BitConverter.GetBytes(value));

    public static explicit operator BitArray(byte value) => ConvertFromByteArray(new byte[] {value});

    public static explicit operator BitArray(bool value) => new BitArray(new bool[] {value});

    public static explicit operator int(BitArray value) => BitConverter.ToInt32(ConvertFromBitArray(value, 4));

    public static explicit operator uint(BitArray value) => BitConverter.ToUInt32(ConvertFromBitArray(value, 4));

    public static explicit operator short(BitArray value) => BitConverter.ToInt16(ConvertFromBitArray(value, 2));

    public static explicit operator ushort(BitArray value) => BitConverter.ToUInt16(ConvertFromBitArray(value, 2));

    public static explicit operator char(BitArray value) => BitConverter.ToChar(ConvertFromBitArray(value, 2));

    public static explicit operator byte(BitArray value) => ConvertFromBitArray(value, 1)[0];

    public static bool ToBool(BitArray value) => value.Bits[0];

    public void Copy(BitArray bitArray)
    {
        lock (_syncRoot)
        {
            if (bitArray == null || bitArray._bits == null)
            {
                _bits = new bool[0];
                Length = 0;
            }
            else
            {
                _bits = (bool[])bitArray._bits.Clone();
                Length = _bits.Length;
            }
        }
    }

    public void Copy(bool[] boolArray)
    {
        lock(_syncRoot)
        {
            if (boolArray == null)
            {
                _bits = new bool[0];
                Length = 0;
            }
            else
            {
                _bits = (bool[])boolArray.Clone();
                Length = _bits.Length;
            }
        }
    }
    private static byte[] ConvertFromBitArray(BitArray value, int lengthInBytes)
    {
        byte[] bits = new byte[lengthInBytes];
        int i = 0;

        for (int j = 0; j < value.Length; j++)
        {
            bits[(i++) / 8] |= (byte)((value.Bits[j] ? 1 : 0) << (j % 8));
        }

        return bits;
    }

    private static BitArray ConvertFromByteArray(byte[] bytes)
    {
        bool[] bits = new bool[bytes.Length * 8];
        int i = 0;

        foreach (var byteVar in bytes)
        {
            for (int j = 0; j < 8; j++)
            {
                bits[i++] = (byteVar & (1 << j)) != 0;
            }
        }

        return new BitArray(bits);
    }

    private void CreateBitArray(byte[] bytes, int length)
    {
        bool[] bits = new bool[length];
        int i = 0;

        foreach (var byteVar in bytes)
        {
            for (int j = 0; j < 8; j++)
            {
                bits[i++] = (byteVar & (1 << j)) != 0;
                if (i == length)
                    break;
            }
            if (i == length)
                break;
        }

        _bits = bits;
        Length = length;
    }
}
