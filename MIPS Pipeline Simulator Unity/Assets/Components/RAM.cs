using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RAM : MonoBehaviour, IReset
{
    public Signal adr;
    public Signal writeData;
    public Signal readData;

    public Signal writeEnable;
    public Clock clock;

    public uint[] memory;

    public TextAsset file;

    public void ResetComponent()
    {
        if (readData is null)
            readData = ScriptableObject.CreateInstance<Signal>();

        readData.ResetSignal(32);

        Array.Clear(memory, 0, memory.Length);

        try
        {
            LoadMemory(file);
        }
        catch (Exception ex)
        {
            Debug.LogError($"ERROR:{ex.Message}");
        }
    }

    void Start()
    {
        ResetComponent();
    }

    // Update is called once per frame
    void Update()
    {
        if (adr is null || writeData is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        if (memory is null)
        {
            Debug.LogError($"There is no data in the memmory");
            return;
        }

        if (clock.RisingEdge)
        {
            if (BitArray.ToBool(writeEnable.Value))
            {
                Write(adr, writeData);
            }
        }

        Read();
    }
    public void Read()
    {
        uint address = (uint)adr.Value >> 2;

        if (memory is null)
        {
            Debug.LogError($"There is no data in the memmory");
            return;
        }

        if (address < 0 || address > memory.Length)
        {
            Debug.LogError($"Address={address << 2} was outside the memory boarder, memory lenght=[0:{memory.Length << 2}]. The memory data will be read as 0");
            readData.Value = (BitArray)0;
        }
        else
            readData.Value = (BitArray)memory[address];
    }

    public void Write(Signal adr, Signal data)
    {
        uint address = (uint)adr.Value >> 2;

        if (address < 0 || address > memory.Length)
        {
            Debug.Log($"Address={address} was outside the memory boarder, memory lenght=[0:{memory.Length << 2}]");
            return;
        }

        memory[address] = (uint)data.Value;
    }

    public void LoadMemory(TextAsset file)
    {
        int adr = 0;
        string[] lines = file.ToString().Split("\r\n");

        memory = new uint[lines.Length];

        try
        {
            foreach (string strData in lines)
            {
                string trimmedStrData = strData.Trim();
                if (!string.IsNullOrEmpty(trimmedStrData))
                {
                    memory[adr++] = Convert.ToUInt32(trimmedStrData, 2);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An exception occured when reading memmory: {ex}");
        }
    }


    public void SetMemory(uint[] newMemory)
    {
        if (memory == null)
        {
            memory = new uint[newMemory.Length];
        }
        else if (memory.Length != newMemory.Length)
        {
            memory = new uint[newMemory.Length];
        }

        Array.Copy(newMemory, memory, Math.Min(newMemory.Length, memory.Length));
    }
}
