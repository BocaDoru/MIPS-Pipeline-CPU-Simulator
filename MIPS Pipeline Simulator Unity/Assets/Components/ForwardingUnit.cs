using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardingUnit : MonoBehaviour, IReset
{
    public Signal rsId;
    public Signal rtId;

    public Signal rdEx;
    public Signal rdMem;

    public Signal regWriteEx;
    public Signal regWriteMem;

    public Signal forwardA;
    public Signal forwardB;

    public void ResetComponent()
    {
        if (forwardA is null)
            forwardA = ScriptableObject.CreateInstance<Signal>();

        if (forwardB is null)
            forwardB = ScriptableObject.CreateInstance<Signal>();

        forwardA.ResetSignal(2);
        forwardB.ResetSignal(2);
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (rsId is null || rtId is null || rdEx is null || rdMem is null || regWriteEx is null || regWriteMem is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        uint fwdA = 0;
        uint fwdB = 0;

        uint rs = (uint)rsId.Value;
        uint rt = (uint)rtId.Value;

        uint dstEx = (uint)rdEx.Value;
        uint dstMem = (uint)rdMem.Value;

        bool writeEx = BitArray.ToBool(regWriteEx.Value);
        bool writeMem = BitArray.ToBool(regWriteMem.Value);

        if (writeMem && dstMem != 0 && dstMem == rs)
        {
            fwdA = 2;
        }
        if (writeMem && dstMem != 0 && dstMem == rt)
        {
            fwdB = 2;
        }

        if (writeEx && dstEx != 0 && dstEx == rs)
        {
            fwdA = 1;
        }
        if (writeEx && dstEx != 0 && dstEx == rt)
        {
            fwdB = 1;
        }

        forwardA.Value = new BitArray(fwdA, 2);
        forwardB.Value = new BitArray(fwdB, 2);
    }
}
