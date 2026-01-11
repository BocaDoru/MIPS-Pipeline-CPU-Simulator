using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;

public class Mux : MonoBehaviour, IReset
{
    public Signal selection;
    public uint selectionNumber;

    public Signal[] inputs;
    public Signal output;

    public void ResetComponent()
    {
        if (output is null)
            output = ScriptableObject.CreateInstance<Signal>();

        if (inputs is not null && inputs.Any())
        {
            int length = inputs.Max(i => i.Value.Length);

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
        if (inputs is null || selection is null || !inputs.All(i => i is not null))
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        uint sel = (uint)selection.Value;
        if (sel >= selectionNumber)
            throw new AccessViolationException($"The selection was outside the range for this mux: sel={sel}; max={selectionNumber}");

        output.Value = inputs[sel].Value;
    }
}
