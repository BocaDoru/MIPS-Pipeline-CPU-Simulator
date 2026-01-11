using System;
using System.Collections;
using UnityEngine;

public class Register : MonoBehaviour, IReset
{
    public Signal dataInput;
    public Signal dataOutput;

    public Signal flush;
    public Signal writeEnable;

    public Clock clock;

    [Hex]
    public uint dataIn;
    [Hex]
    public uint dataOut;

    private BitArray value;

    public void ResetComponent()
    {
        if (dataOutput is null)
            dataOutput = ScriptableObject.CreateInstance<Signal>();

        if (dataInput is not null)
            dataOutput.ResetSignal(dataInput.Value.Length);
        else
            dataOutput.ResetSignal();

        if (flush is null)
        {
            flush = ScriptableObject.CreateInstance<Signal>();
            flush.Value = new BitArray(false);
        }

        if (writeEnable is null)
        {
            writeEnable = ScriptableObject.CreateInstance<Signal>();
            writeEnable.Value = new BitArray(true);
        }

        value = new BitArray(0, dataInput.Value.Length);
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (dataInput is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        if (clock.RisingEdge)
        {
            if (BitArray.ToBool(flush.Value))
            {
                Write(new BitArray(0, dataInput.Value.Length));
            }
            else if (BitArray.ToBool(writeEnable.Value))
            {
                Write(value);
            }
        }
        Read();
    }

    public void Write(BitArray value)
    {
        dataOutput.Value = value;
        dataOut = (uint)value;
    }

    public void Read()
    {
        dataIn = (uint)dataInput.Value;
        value.Copy(dataInput.Value);
    }
}
