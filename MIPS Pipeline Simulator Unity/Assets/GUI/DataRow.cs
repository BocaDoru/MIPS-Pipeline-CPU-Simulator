using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DataRow : MonoBehaviour
{
    public bool modified = false;
    public int address = 0;

    public TMP_Text addressLabel;

    public TMP_InputField[] memoryInput = new TMP_InputField[4];
    private uint[] memory = new uint[4];

    private void Start()
    {
        if (memoryInput == null || memoryInput.Length < 4)
        {
            Debug.LogError("DataRow Error: 'Memory Input' array must have 4 elements in the Inspector.");
        }
    }

    public void Initialize(int adr, uint[] initialValues)
    {
        address = adr;

        if (addressLabel != null)
            addressLabel.text = $"{adr:X5}";

        for (int i = 0; i < 4; i++)
        {
            if (i >= memoryInput.Length || memoryInput[i] == null) continue;

            memory[i] = (i < initialValues.Length) ? initialValues[i] : 0;

            memoryInput[i].onEndEdit.RemoveAllListeners();

            int index = i;
            memoryInput[i].onEndEdit.AddListener((val) => OnInputChange(index, val));

            memoryInput[i].text = Convert.ToString(memory[i], 16).PadLeft(8, '0');
        }
    }

    public uint GetMemory(int index)
    {
        if (index >= 0 && index < memory.Length)
            return memory[index];
        return 0;
    }

    public void SetMemory(int index, uint data)
    {
        if (index >= 0 && index < memory.Length)
        {
            memory[index] = data;

            if (index < memoryInput.Length && memoryInput[index] != null)
            {
                memoryInput[index].text = Convert.ToString(memory[index], 16).PadLeft(8, '0');
            }
        }
    }

    public void SetColor(int index, Color color)
    {
        if (memoryInput == null || index < 0 || index >= memoryInput.Length)
            return;

        TMP_InputField input = memoryInput[index];
        if (input != null)
        {
            Image bgImage = input.GetComponent<Image>();
            if (bgImage == null && input.targetGraphic != null)
                bgImage = input.targetGraphic as Image;

            if (bgImage != null)
            {
                bgImage.color = color;
            }
        }
    }

    private void OnInputChange(int index, string value)
    {
        try
        {
            memory[index] = Convert.ToUInt32(value, 16);

            if (index < memoryInput.Length && memoryInput[index] != null)
            {
                memoryInput[index].text = Convert.ToString(memory[index], 16).PadLeft(8, '0');
            }

            modified = true;
        }
        catch (Exception)
        {
            if (index < memoryInput.Length && memoryInput[index] != null)
            {
                memoryInput[index].text = Convert.ToString(memory[index], 16).PadLeft(8, '0');
            }
            Debug.LogWarning($"Invalid Hex Input at index {index}");
        }
    }
}
