using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SignalAdder : MonoBehaviour, IReset
{
    public Signal[] inputs;
    public Signal output;

    public void ResetComponent()
    {
        if (output is null)
            output = ScriptableObject.CreateInstance<Signal>();

        if (inputs is not null && inputs.All(i => i is not null))
        {
            int length = inputs.Sum(i => i.Value.Length);
            output.ResetSignal(length);
        }
        else
        {
            output.ResetSignal();
        }
    }

    void Start()
    {
        ResetComponent();
    }
    void Update()
    {
        if (inputs is null || inputs.Length < 1 || !inputs.All(i => i is not null))
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        List<bool> bits = new List<bool>();
        foreach (Signal input in inputs)
            bits.AddRange(input.Value.Bits);

        output.Value = new BitArray(bits.ToArray());
    }
}
