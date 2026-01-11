using System;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour, IReset
{
    public Signal clock;
    public bool clockValue;
    public bool previousClockValue;
    public int numberOfUpdates = 25;

    public bool IsClockHigh => ClockValue;
    public bool IsClockLow => !ClockValue;
    public bool RisingEdge => clockValue != previousClockValue && clockValue == true;
    public bool FallingEdge => clockValue != previousClockValue && clockValue == false;
    public bool ClockValue => BitArray.ToBool(clock.Value);

    private bool start = false;

    private int count = 0;

    public void ResetComponent()
    {
        if (clock is null)
            clock = ScriptableObject.CreateInstance<Signal>();

        clock.ResetSignal(1);

        clockValue = false;
        previousClockValue = false;
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (start)
        {
            if (count == numberOfUpdates)
            {
                clock.Value = !clock.Value;
                count = 0;
            }

            count++;

            previousClockValue = clockValue;
            clockValue = ClockValue;
        }
    }

    public void ToggleStart(bool start)
    {
        this.start = start;
    }

    public void ChangeClockNumber(float number)
    {
        numberOfUpdates = (int)number;
    }
}
