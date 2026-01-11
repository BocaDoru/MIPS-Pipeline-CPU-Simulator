using System;
using UnityEngine;

public class ExtensionUnit : MonoBehaviour, IReset
{
    public Signal input;
    public Signal output;

    public Signal signed;

    [Hex]
    public int dataIn;
    [Hex]
    public int dataOut;

    public void ResetComponent()
    {
        if (output is null)
            output = ScriptableObject.CreateInstance<Signal>();

        output.ResetSignal(32);
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (input is null || signed is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        dataIn = (int)input.Value;
        if (BitArray.ToBool(signed.Value))
        {
            Extend((short)input.Value);
        }
        else
        {
            Extend((ushort)input.Value);
        }
    }

    public void Extend(short value)
    {
        output.Value = (BitArray)(int)value;
        dataOut = (int)output.Value;
    }
    public void Extend(ushort value)
    {
        output.Value = (BitArray)(uint)value;
        dataOut = (int)output.Value;
    }
}
