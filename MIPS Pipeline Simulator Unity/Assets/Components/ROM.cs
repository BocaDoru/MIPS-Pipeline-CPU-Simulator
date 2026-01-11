using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class ROM : MonoBehaviour, IReset
{
    public Signal adr;
    public Signal data;
    public uint[] memory;

    public TextAsset file;

    public void ResetComponent()
    {
        if (data is null)
            data = ScriptableObject.CreateInstance<Signal>();

        data.ResetSignal(32);

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

    void Update()
    {
        if (adr is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        if (memory is null)
        {
            Debug.LogError($"There is no data in the memmory");
            return;
        }

        Read();
    }
    public void Read()
    {
        uint address = (uint)adr.Value >> 2;

        if (address < 0 || address >= memory.Length)
            Debug.LogError($"Address={address << 2} was outside the memory boarder, memory lenght=[0:{memory.Length << 2}]");

        data.Value = (BitArray)memory[address];
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
        catch(Exception ex)
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
