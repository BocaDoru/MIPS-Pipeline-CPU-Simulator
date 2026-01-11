using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum OperationType { ADD, ISZERO, OR, AND, NOT, SHIFT }

public class OperationUnit : MonoBehaviour, IReset
{
    public OperationType operation;

    public List<Signal> inputs;
    public Signal r;

    public bool useConst = false;
    public Signal constSignal;

    [Hex]
    public uint rValue;
    [Hex]
    public uint constValue;

    public void ResetComponent()
    {
        if (r is null)
            r = ScriptableObject.CreateInstance<Signal>();

        if (inputs is not null && inputs.Any())
        {
            int length = inputs.Max(i => i.Value.Length);

            r.ResetSignal(length);
        }
        else
        {
            r.ResetSignal();
        }
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (inputs is null || !inputs.All(i => i is not null))
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        switch (operation)
        {
            case OperationType.ADD:
                if (inputs.Count < 2 && constSignal is null)
                {
                    Debug.LogError($"The operation:{operation} requires at least 2 Input Signals or 1 Input Signal & Const Signal");
                    return;
                }

                if (inputs.Count == 1)
                    r.Value = constSignal.Value;
                else
                    r.Value = new BitArray(0, r.Value.Length);
                
                foreach (var input in inputs)
                {
                    r.Value = r.Value + input.Value;
                }
                break;
            case OperationType.OR:
                if (inputs.Count < 2)
                {
                    Debug.LogError($"The operation:{operation} requires at least 2 Input Signals");
                    return;
                }
                
                r.Value = new BitArray(false);
                
                foreach (var input in inputs)
                {
                    r.Value = r.Value | input.Value;
                }
                break;
            case OperationType.AND:
                if (inputs.Count < 2)
                {
                    Debug.LogError($"The operation:{operation} requires at least 2 Input Signals");
                    return;
                }
                
                r.Value = new BitArray(true);
                
                foreach (var input in inputs)
                {
                    r.Value = r.Value & input.Value;
                }
                break;
            case OperationType.NOT:
                r.Value = !inputs[0].Value;
                break;
            case OperationType.SHIFT:
                if (inputs.Count < 1 || constSignal is null)
                {
                    Debug.LogError($"The operation:{operation} requires a Const Signal");
                    return;
                }

                r.Value = inputs[0].Value << (int)constSignal.Value;
                break;
            case OperationType.ISZERO:
                r.Value = new BitArray(BitArray.IsZero(inputs[0].Value));
                break;
        }

        rValue = (uint)r.Value;
    }
    

}
