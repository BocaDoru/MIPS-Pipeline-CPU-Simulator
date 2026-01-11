using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RegisterFileRow : MonoBehaviour
{
    public int address = 0;
    public uint value = 0;

    public TMP_Text registerName;
    public TMP_Text hexValue;
    public TMP_Text binaryValue;

    public Image image;

    // StartButton is called before the first frame update
    public void Initialize(int adr, string name)
    {
        address = adr;
        value = 0;

        registerName.text = name;
        hexValue.text = "0x" + $"{0:X8}";
        binaryValue.text = $"{0:X32}";
    }

    public void SetValue(uint val)
    {
        value = val;

        hexValue.text = "0x" + Convert.ToString(val, 16).PadLeft(8, '0');
        binaryValue.text = Convert.ToString(val, 2).PadLeft(32, '0');
    }
    public void SetColor(Color color)
    {
        image.color = color;
    }
}
