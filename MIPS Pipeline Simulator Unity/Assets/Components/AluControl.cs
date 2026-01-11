using System;
using System.Collections.Generic;
using UnityEngine;

public class AluControl : MonoBehaviour, IReset
{
    public Signal func;
    public Signal aluOp;

    public Signal aluCtrl;

    public void ResetComponent()
    {
        if (aluCtrl is null)
            aluCtrl = ScriptableObject.CreateInstance<Signal>();

        aluCtrl.ResetSignal(3);
    }

    void Start()
    {
        ResetComponent();
    }

    void Update()
    {
        if (func is null || aluOp is null)
        {
            Debug.LogError($"Ensure that all input signals are connected to {this.name} before running");
            return;
        }

        byte funcValue = (byte)func.Value;
        byte aluOpValue = (byte)aluOp.Value;

        byte aluCtrlValue;

        switch(aluOpValue)
        {
            case 0x0:
                switch(funcValue)
                {
                    case 0x20:  //ADD
                        aluCtrlValue = 0x0; //(+)
                        break;
                    case 0x22:  //SUB
                        aluCtrlValue = 0x1; //(-)
                        break;
                    case 0x00:  //SLL
                        aluCtrlValue = 0x2; //(<<L)
                        break;
                    case 0x02:  //SRL
                        aluCtrlValue = 0x3; //(>>L)
                        break;
                    case 0x03:  //SRA
                        aluCtrlValue = 0x4; //(>>A)
                        break;
                    case 0x24:  //AND
                        aluCtrlValue = 0x5; //(&)
                        break;
                    case 0x25:  //OR
                        aluCtrlValue = 0x6; //(|)
                        break;
                    case 0x26:  //XOR
                        aluCtrlValue = 0x7; //(^)
                        break;
                    default:
                        Debug.LogError($"The func={Convert.ToString(funcValue, 2)} is not suported"); 
                        aluCtrlValue = 0x0;
                        return;
                }
                break;
            case 0x1:  //ADDI LW SW
                aluCtrlValue = 0x0; //(+)
                break;
            case 0x2:  //BEQ BNE BGTZ BGEZ
                aluCtrlValue = 0x1; //(-)
                break;
            case 0x3:  //J JAL JR
                aluCtrlValue = 0x0; //(Don't care)
                break;
            default:
                Debug.Log($"The ALUOp={Convert.ToString(aluOpValue, 2)} is not suported");
                aluCtrlValue = 0x0;
                return;
        }

        aluCtrl.Value = new BitArray(aluCtrlValue, 3);
    }
}
