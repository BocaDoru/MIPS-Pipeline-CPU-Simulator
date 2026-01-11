using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SignalSplitter : MonoBehaviour, IReset
{
    public Signal input;
    public Interval[] intervals;
    public Signal[] outputs;

    public void ResetComponent()
    {
        if (outputs is null)
        {
            if (intervals is not null && intervals.Length > 0)
            {
                outputs = new Signal[intervals.Length];
                for (int i = 0; i < intervals.Length; i++)
                {
                    outputs[i] = ScriptableObject.CreateInstance<Signal>();
                    outputs[i].ResetSignal(intervals[i].y - intervals[i].x + 1);
                }
            }
            else
            {
                outputs = new Signal[0];
                outputs[0] = ScriptableObject.CreateInstance<Signal>();
                outputs[0].ResetSignal();
            }
        }
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (input is null || intervals is null || intervals.Length < 1)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        for (int i = 0; i < intervals.Length; i++)
        {
            outputs[i].Value = Split(input.Value, intervals[i]);
        }
    }

    public BitArray Split(BitArray input, Interval interval)
    {
        int size = interval.y - interval.x + 1;
        uint mask = (uint)(1 << (size)) - 1;
        uint shiftedInput = ((uint)input >> interval.x);

        return new BitArray(shiftedInput & mask, size);
    }
}

[Serializable]
public record Interval
{
    public int x;
    public int y;
}



