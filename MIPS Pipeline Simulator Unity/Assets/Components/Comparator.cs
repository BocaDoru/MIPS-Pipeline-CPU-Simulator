using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Comparator : MonoBehaviour, IReset
{
    public Signal a;
    public Signal b;

    public Signal greater;
    public Signal greaterOrEqual;
    public Signal notEqual;
    public Signal equal;

    public void ResetComponent()
    {
        if (greater is null)
            greater = ScriptableObject.CreateInstance<Signal>();

        if (greaterOrEqual is null)
            greaterOrEqual = ScriptableObject.CreateInstance<Signal>();

        if (notEqual is null)
            notEqual = ScriptableObject.CreateInstance<Signal>();

        if (equal is null)
            equal = ScriptableObject.CreateInstance<Signal>();

        greater.ResetSignal(1);
        greaterOrEqual.ResetSignal(1);
        notEqual.ResetSignal(1);
        equal.ResetSignal(1);
    }

    void Start()
    {
        ResetComponent();
    }

    // Update is called once per frame
    void Update()
    {
        if (a is null || b is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        var r = a.Value - b.Value;

        equal.Value = new BitArray(BitArray.IsZero(r));
        notEqual.Value = new BitArray(!BitArray.IsZero(r));
        greater.Value = new BitArray(BitArray.IsGreaterThanZero(r));
        greaterOrEqual.Value = new BitArray(BitArray.IsGreaterThanZero(r) || BitArray.IsZero(r));
    }
}
