using System;
using UnityEditor;
using UnityEngine;

public class RegisterFile : MonoBehaviour, IReset
{
    public Signal readAddress1;
    public Signal readAddress2;
    public Signal writeAddress;
    public Signal writeData;

    public Signal registerWrite;

    public Signal readData1;
    public Signal readData2;

    public Clock clock;

    public uint readAddress1Value;
    public uint readAddress2Value;
    public uint writeAddressValue;
    public uint writeDataValue;

    public uint registerWriteValue;

    public uint readData1Value;
    public uint readData2Value;

    public uint[] memory = new uint[32];
    public TextAsset file;

    public void ResetComponent()
    {
        if (readData1 is null)
            readData1 = ScriptableObject.CreateInstance<Signal>();

        readData1.ResetSignal(32);

        if (readData2 is null)
            readData2 = ScriptableObject.CreateInstance<Signal>();

        readData2.ResetSignal(32);

        memory = new uint[32];
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (readAddress1 is null || readAddress2 is null || writeAddress is null || writeData is null || registerWrite is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        readAddress1Value = (uint)readAddress1.Value;
        readAddress2Value = (uint)readAddress2.Value;
        writeAddressValue = (uint)writeAddress.Value;
        writeDataValue = (uint)writeData.Value;

        registerWriteValue = (uint)registerWrite.Value;

        readData1Value = (uint)readData1.Value;
        readData2Value = (uint)readData2.Value;

        if (clock.FallingEdge)
        {
            if (BitArray.ToBool(registerWrite.Value))
            {
                Write(writeAddress, writeData);
            }
        }

        Read(readAddress1, readData1);

        Read(readAddress2, readData2);
    }

    public void Read(Signal adr, Signal data)
    {
        uint address = (uint)adr.Value;

        if (address < 0 || address > 31)
            Debug.LogError($"Address={address} was outside the memory boarder, memory lenght=[0:31]");

        data.Value = (BitArray)memory[address];
    }

    public void Write(Signal adr, Signal data)
    {
        uint address = (uint)adr.Value;

        if (address < 0 || address > 31)
            Debug.LogError($"Address={address} was outside the memory boarder, memory lenght=[0:31]");

        memory[address] = (uint)data.Value;
    }
}
