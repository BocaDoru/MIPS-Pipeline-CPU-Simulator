using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterFileWindow : MonoBehaviour, IReset
{
    public RegisterFile regFile;
    public GameObject rowPrefab;
    public RectTransform content;

    public Signal ra1;
    public Signal ra2;
    public Signal wa;

    public Signal writeData;
    public Signal writeEnable;

    public Color ra1Color = Color.green;
    public Color ra2Color = Color.cyan;
    public Color waColor = Color.blue;

    private List<GameObject> spawnedRows = new List<GameObject>();
    private static readonly string[] RegNames = new string[]
    {
        "$zero", "$at", "$v0", "$v1", "$a0", "$a1", "$a2", "$a3",
        "$t0", "$t1", "$t2", "$t3", "$t4", "$t5", "$t6", "$t7",
        "$s0", "$s1", "$s2", "$s3", "$s4", "$s5", "$s6", "$s7",
        "$t8", "$t9", "$k0", "$k1", "$gp", "$sp", "$fp", "$ra"
    };
    private int ra1Adr = 0;
    private int ra2Adr = 0;
    private int waAdr = 0;

    void Start()
    {
        AddRows(32);
    }

    private void Update()
    {
        spawnedRows[ra1Adr].GetComponent<RegisterFileRow>().SetColor(Color.white);
        spawnedRows[ra2Adr].GetComponent<RegisterFileRow>().SetColor(Color.white);
        spawnedRows[waAdr].GetComponent<RegisterFileRow>().SetColor(Color.white);

        ra1Adr = (int)ra1.Value;
        ra2Adr = (int)ra2.Value;

        spawnedRows[ra1Adr].GetComponent<RegisterFileRow>().SetColor(ra1Color);
        spawnedRows[ra2Adr].GetComponent<RegisterFileRow>().SetColor(ra2Color);

        if(BitArray.ToBool(writeEnable.Value))
        {
            waAdr = (int)wa.Value;
            RegisterFileRow row = spawnedRows[waAdr].GetComponent<RegisterFileRow>();

            row.SetColor(waColor);
            row.SetValue((uint)writeData.Value);
        }
    }

    public void AddRows(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnRow(i);
        }
    }

    private void SpawnRow(int adr)
    {
        GameObject newRow = Instantiate(rowPrefab, content.transform);
        spawnedRows.Add(newRow);

        RegisterFileRow regRow = newRow.GetComponent<RegisterFileRow>();
        if (regRow is not null)
        {
            regRow.Initialize(adr, RegNames[adr]);
        }
    }

    public void ResetComponent()
    {
        foreach (var rowObj in spawnedRows)
        {
            RegisterFileRow regRow = rowObj.GetComponent<RegisterFileRow>();
            regRow.SetValue(0);
            regRow.SetColor(Color.white);
        }
    }
}
