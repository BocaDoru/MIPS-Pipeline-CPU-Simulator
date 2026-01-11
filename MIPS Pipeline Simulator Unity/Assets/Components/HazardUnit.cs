using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HazardUnit : MonoBehaviour, IReset
{
    public Signal rs;
    public Signal rt;
    public Signal rtIdEx;
    public Signal memReadIdEx;

    public Signal rtExMem;
    public Signal memReadExMem;

    public Signal pcWrite;
    public Signal ifIdWrite;
    public Signal bubble;

    public void ResetComponent()
    {
        if (pcWrite is null)
            pcWrite = ScriptableObject.CreateInstance<Signal>();
        if (ifIdWrite is null)
            ifIdWrite = ScriptableObject.CreateInstance<Signal>();
        if (bubble is null)
            bubble = ScriptableObject.CreateInstance<Signal>();

        pcWrite.ResetSignal(1);
        ifIdWrite.ResetSignal(1);
        bubble.ResetSignal(1);
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (rs is null || rt is null || rtIdEx is null || rtExMem is null || memReadIdEx is null || memReadExMem is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        bool memReadEx = BitArray.ToBool(memReadIdEx.Value);
        bool memReadMem = BitArray.ToBool(memReadExMem.Value);

        uint rsValue = (uint)rs.Value;
        uint rtValue = (uint)rt.Value;

        uint rtIdExValue = (uint)rtIdEx.Value;
        uint rtExMemValue = (uint)rtExMem.Value;

        bool pcWriteValue = true;
        bool ifIdWriteValue = true;
        bool bubbleValue = false;

        if (memReadEx && (rsValue == rtIdExValue || rtValue == rtIdExValue))
        {
            pcWriteValue = false;
            ifIdWriteValue = false;
            bubbleValue = true;
        }

        if (memReadMem && (rsValue == rtExMemValue || rtValue == rtExMemValue))
        {
            pcWriteValue = false;
            ifIdWriteValue = false;
            bubbleValue = true;
        }

        pcWrite.Value = new BitArray(pcWriteValue);
        ifIdWrite.Value = new BitArray(ifIdWriteValue);
        bubble.Value = new BitArray(bubbleValue);
    }
}
