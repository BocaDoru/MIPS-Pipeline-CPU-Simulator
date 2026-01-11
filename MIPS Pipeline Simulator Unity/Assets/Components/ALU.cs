using System;
using System.Collections.Generic;
using UnityEngine;

public class ALU : MonoBehaviour, IReset
{
    public Signal aluCtrl;

    public Signal a;
    public Signal b;
    public Signal r;

    public Signal sa;

    public void ResetComponent()
    {
        if (r is null)
            r = ScriptableObject.CreateInstance<Signal>();

        r.ResetSignal(32);
    }

    void Start()
    {
        ResetComponent();
    }

    // Update is called once per frame
    void Update()
    {
        if (a is null || b is null || sa is null || aluCtrl is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        byte aluCtrlValue = (byte)aluCtrl.Value;
        int saValue = (int)sa.Value;

        switch(aluCtrlValue)
        {
            case 0x0:
                r.Value = a.Value + b.Value;
                break;
            case 0x1:
                r.Value = a.Value - b.Value;
                break;
            case 0x2:
                r.Value = b.Value << saValue;
                break;
            case 0x3:
                r.Value = b.Value >> saValue;
                break;
            case 0x4:
                r.Value = BitArray.ShiftRightArithmetic(b.Value, saValue);
                break;
            case 0x5:
                r.Value = a.Value & b.Value;
                break;
            case 0x6:
                r.Value = a.Value | b.Value;
                break;
            case 0x7:
                r.Value = a.Value ^ b.Value;
                break;
            default:
                throw new InvalidOperationException($"ALU does not support this operation");
        }
    }
}
