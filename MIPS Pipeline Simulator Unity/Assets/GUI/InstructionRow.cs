using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public enum InputMode { BINARY, ASSEMBLY };

public class InstructionRow : MonoBehaviour
{
    public bool modified = false;

    public InputMode input = InputMode.ASSEMBLY;

    public int address = 0;

    public TMP_Text addressLabel;
    public TMP_InputField instructionInput;
    public TMP_Text hexLabel;
    public TMP_Text stageLabel;

    private uint instruction;

    public void Initialize(int adr, uint initialValue, InputMode mode)
    {
        address = adr;
        instruction = initialValue;
        input = mode;

        addressLabel.text = $"{adr:X3}";

        instructionInput.onEndEdit.AddListener(OnInputChange);

        RefresDisplay();
    }

    public void SetMode(InputMode mode)
    {
        input = mode;
        RefresDisplay();
    }

    public uint GetInstruction()
    {
        return instruction;
    }

    private void RefresDisplay()
    {
        hexLabel.text = "0x" + instruction.ToString("X8");

        if (input == InputMode.BINARY)
        {
            instructionInput.contentType = TMP_InputField.ContentType.Standard;
            instructionInput.characterLimit = 32;
            instructionInput.text = Convert.ToString(instruction, 2).PadLeft(32, '0');
        }
        else
        {
            instructionInput.contentType = TMP_InputField.ContentType.Standard;
            instructionInput.characterLimit = 0;
            try
            {
                instructionInput.text = InstructionHelper.Disassemble(instruction);
            }
            catch
            {
                instructionInput.text = "INVALID";
            }
        }
    }

    private void OnInputChange(string value)
    {
        try
        {
            if (input == InputMode.BINARY)
            {
                instruction = Convert.ToUInt32(value);
            }
            else
            {
                instruction = InstructionHelper.Assemble(value);
            }

            modified = true;
            RefresDisplay();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Input exception:{ex.Message}");
        }
    }
}
