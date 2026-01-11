using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataWindow : MonoBehaviour
{
    public RAM dataMemory;

    public GameObject rowPrefab;
    public int initialRowCount = 16;

    public RectTransform content;

    public Signal address;
    public Signal write;
    public Signal writeData;

    public Color readColor = Color.green;
    public Color writeColor = Color.blue;

    public bool Modified => spawnedRows.Select(sr => sr.GetComponent<DataRow>().modified).Any(m => m);

    private List<GameObject> spawnedRows = new List<GameObject>();
    private int currentAdr = 0;
    private int accessAdr = -1;

    // StartButton is called before the first frame update
    void Start()
    {
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (accessAdr >= 0)
        {
            int adr = accessAdr / 16;
            int idx = (accessAdr % 16) / 4;

            if (adr < spawnedRows.Count)
            {
                DataRow row = spawnedRows[adr].GetComponent<DataRow>();
                row.SetColor(idx, Color.white);
            }
        }

        accessAdr = (int)address.Value;

        int newAdr = accessAdr / 16;
        int newIdx = (accessAdr % 16) / 4;

        if (newAdr >= 0 && newAdr < spawnedRows.Count)
        {
            DataRow newRow = spawnedRows[newAdr].GetComponent<DataRow>();

            if (BitArray.ToBool(write.Value))
            {
                newRow.SetColor(newIdx, writeColor);
                newRow.SetMemory(newIdx, (uint)writeData.Value);
            }
            else
                newRow.SetColor(newIdx, readColor);
        }
    }

    public void AddRows(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnRow(currentAdr);
            currentAdr += 16;
        }
    }

    public void UploadMemory()
    {
        uint[] memory = new uint[spawnedRows.Count * 4];
        int adr = 0;

        foreach (var row in spawnedRows)
        {
            DataRow dataRow = row.GetComponent<DataRow>();

            for (int i = 0; i < 4; i++)
                memory[adr++] = dataRow.GetMemory(i);
        }

        dataMemory.SetMemory(memory);
    }
    public void SetModified(bool modified)
    {
        spawnedRows.ForEach(sr => sr.GetComponent<DataRow>().modified = true);
    }
    private void SpawnRow(int adr)
    {
        GameObject newRow = Instantiate(rowPrefab, content.transform);
        spawnedRows.Add(newRow);

        DataRow dataRow = newRow.GetComponent<DataRow>();
        if (dataRow is not null)
        {
            dataRow.Initialize(adr, new uint[] { 0, 0, 0, 0 });

            for (int i = 0; i < 4; i++)
            {
                dataRow.memoryInput[i].onSelect.AddListener((val) => CheckAutoExpand(newRow));
            }
        }
    }

    private void CheckAutoExpand(GameObject rowObject)
    {
        if (spawnedRows.IndexOf(rowObject) >= spawnedRows.Count - 1)
        {
            AddRows(4);
        }
    }

    public void ResetWindow()
    {
        foreach (var rowObj in spawnedRows)
        {
            DataRow regRow = rowObj.GetComponent<DataRow>();
            for (int i = 0; i < 4; i++)
            {
                regRow.SetMemory(i, 0);
                regRow.SetColor(i, Color.white);
            }
        }
    }
    public void Restart()
    {
        if (spawnedRows.Count > 0)
        {
            spawnedRows.ForEach(row => Destroy(row));
            spawnedRows.Clear();
        }

        currentAdr = 0;
        AddRows(initialRowCount);
        SetModified(true);
    }
}
