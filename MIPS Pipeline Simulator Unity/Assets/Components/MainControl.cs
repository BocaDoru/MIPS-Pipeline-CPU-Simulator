using System;
using System.Collections.Generic;
using UnityEngine;

public class MainControl : MonoBehaviour, IReset
{
    public Signal opCode;

    public Signal controlSignal;
    // |<-- ID -->|<------ EX ------->|<--- MEM --->|<-- WB -->|
    // | 15 14 13 | 12 11 | 10 | 9  8 | 7 6 5 4 | 3 | 2 | 1  0 |
    // | E  J  J  | R     | A  | A    | B B B B | M | R | M    |
    // | X  U  U  | E     | L  | L    | R R R R | E | E | E    |
    // | T  M  M  | G     | U  | U    | A _ _ _ | M | G | M    |
    // | O  P  P  | D     | S  | O    | N N G G | W | W | 2    |
    // | P     R  | S     | R  | P    | C E E T | R | R | R    |
    // |       E  | T     | C  |      | H   Z Z |   |   | E    |
    // |       G  |       |    |      |         |   |   | G    |

    public void ResetComponent()
    {
        if (controlSignal is null)
        {
            controlSignal = ScriptableObject.CreateInstance<Signal>();
        }

        controlSignal.ResetSignal(16);
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (opCode is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        byte opCodeValue = (byte)opCode.Value;
        ushort controlValue;

        switch (opCodeValue)
        {
            case 0x00:  // R-type
                controlValue = 0x0804;
                break;
            case 0x1A:  // JR
                controlValue = 0x2000;
                break;
            case 0x08:  // ADDI
                controlValue = 0x8504;
                break;
            case 0x23:  // LW
                controlValue = 0x8505;
                break;
            case 0x2B:  // SW
                controlValue = 0x8508;
                break;
            case 0x04:  // BEQ
                controlValue = 0x8280;
                break;
            case 0x05:  // BNE
                controlValue = 0x8240;
                break;
            case 0x01:  // BGEZ
                controlValue = 0x8220;
                break;
            case 0x07:  // BGTZ
                controlValue = 0x8210;
                break;
            case 0x02:  // J
                controlValue = 0x4300;
                break;
            case 0x03:  // JAL
                controlValue = 0x5306;
                break;
            default:
                throw new InvalidOperationException($"The opCode={Convert.ToString(opCodeValue, 2)} is not suported");
        }

        controlSignal.Value = (BitArray)controlValue;
    }
}
