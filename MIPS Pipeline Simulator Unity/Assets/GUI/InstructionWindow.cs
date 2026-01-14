using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;

public class InstructionWindow : MonoBehaviour
{
    public ROM instructionMemory;

    public GameObject rowPrefab;
    public int initialRowCount = 16;

    public TMP_Dropdown modeDropdown;
    public RectTransform content;

    public TMP_Dropdown fileListDropdown;
    public TMP_InputField saveFileNameInput;

    public Signal[] pcIn;

    public bool Modified => spawnedRows.Select(sr => sr.GetComponent<InstructionRow>().modified).Any(m => m);

    private List<TMP_Text> stageLabels = new List<TMP_Text>(); 
    private string[] stages = { "IF", "ID", "EX", "MEM", "WB" };

    private List<GameObject> spawnedRows = new List<GameObject>();
    private int currentAdr = 0;
    private InputMode mode = InputMode.ASSEMBLY;

    private string UserSavePath => Path.Combine(Application.persistentDataPath, "SavedInstructions");
    private string DefaultSavePath => Path.Combine(Application.streamingAssetsPath, "DefaultInstructions");

    private void Start()
    {
        Restart();
    }

    private void Update()
    {
        stageLabels.ForEach(x => x.text = "");
        stageLabels.RemoveRange(0, stageLabels.Count);

        for (int i = 4; i >= 0; i--)
        {
            int index = (int)pcIn[i].Value / 4;
            if (index >= 0 && index < spawnedRows.Count)
            {
                GameObject rowObj = spawnedRows[index];
                InstructionRow instructionRow = rowObj.GetComponent<InstructionRow>();

                instructionRow.stageLabel.text = stages[i];

                stageLabels.Add(instructionRow.stageLabel);
            }
            else
                break;
        }
    }

    private void OnDropdownChange(int index)
    {
        mode = index == 0 ? InputMode.ASSEMBLY : InputMode.BINARY;

        foreach (var rowObj in spawnedRows)
        {
            InstructionRow instructionRow = rowObj.GetComponent<InstructionRow>();
            if (instructionRow is not null)
                instructionRow.SetMode(mode);
        }
    }

    public void SaveToFile()
    {
        string fileName = saveFileNameInput.text.Trim();
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("Save failed: File name is empty.");
            return;
        }

        if (!fileName.EndsWith(".txt")) 
            fileName += ".txt";

        string fullPath = Path.Combine(UserSavePath, fileName);
        List<string> lines = new List<string>();

        foreach (var row in spawnedRows)
        {
            InstructionRow iRow = row.GetComponent<InstructionRow>();
            uint val = iRow.GetInstruction();

            string binary = Convert.ToString(val, 2).PadLeft(32, '0');
            lines.Add(binary);
        }

        File.WriteAllLines(fullPath, lines);
        Debug.Log($"Saved {lines.Count} instructions to: {fullPath}");

        RefreshFileDropdown();
    }
    public void LoadFromFile()
    {
        if (fileListDropdown.options.Count == 0) return;

        string selectedName = fileListDropdown.options[fileListDropdown.value].text;
        string fullPath = "";

        if (selectedName.EndsWith(" (Default)"))
        {
            string realName = selectedName.Replace(" (Default)", "") + ".txt";
            fullPath = Path.Combine(DefaultSavePath, realName);
        }
        else
        {
            fullPath = Path.Combine(UserSavePath, selectedName + ".txt");
        }

        if (File.Exists(fullPath))
        {
            string[] lines = File.ReadAllLines(fullPath);

            foreach (var obj in spawnedRows) 
                Destroy(obj);
            spawnedRows.Clear();
            currentAdr = 0;

            foreach (string line in lines)
            {
                try
                {
                    uint val = Convert.ToUInt32(line, 2);
                    SpawnRow(currentAdr, val);
                    currentAdr += 4;
                }
                catch { }
            }

            if (spawnedRows.Count < initialRowCount)
            {
                AddRows(initialRowCount - spawnedRows.Count);
            }

            UploadMemory();
            Debug.Log($"Loaded from {fullPath}");
        }
        else
        {
            Debug.LogError($"File not found: {fullPath}");
        }
    }

    private void RefreshFileDropdown()
    {
        if (fileListDropdown == null) 
            return;

        fileListDropdown.ClearOptions();
        List<string> options = new List<string>();

        if (Directory.Exists(UserSavePath))
        {
            string[] userFiles = Directory.GetFiles(UserSavePath, "*.txt");
            foreach (string f in userFiles)
            {
                options.Add(Path.GetFileNameWithoutExtension(f));
            }
        }

        if (Directory.Exists(DefaultSavePath))
        {
            string[] defaultFiles = Directory.GetFiles(DefaultSavePath, "*.txt");
            foreach (string f in defaultFiles)
            {
                options.Add(Path.GetFileNameWithoutExtension(f) + " (Default)");
            }
        }

        if (options.Count == 0) 
            options.Add("No Files Found");

        fileListDropdown.AddOptions(options);
    }

    public void UploadMemory()
    {
        uint[] memory = new uint[spawnedRows.Count];
        int adr = 0;

        foreach (var row in spawnedRows)
        {
            InstructionRow instructionRow = row.GetComponent<InstructionRow>();

            memory[adr++] = instructionRow.GetInstruction();
        }

        instructionMemory.SetMemory(memory);
    }

    public void Restart()
    {
        if (spawnedRows.Count > 0)
        {
            spawnedRows.ForEach(row => Destroy(row));
            spawnedRows.Clear();
        }

        if (!Directory.Exists(UserSavePath))
        {
            Directory.CreateDirectory(UserSavePath);
        }

        RefreshFileDropdown();

        if (modeDropdown is not null)
        {
            modeDropdown.onValueChanged.AddListener(OnDropdownChange);

            OnDropdownChange(modeDropdown.value);
        }

        currentAdr = 0;

        AddRows(initialRowCount);
        SetModified(true);
    }

    public void SetModified(bool modified)
    {
        spawnedRows.ForEach(sr => sr.GetComponent<InstructionRow>().modified = true);
    }

    private void AddRows(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnRow(currentAdr, 0);
            currentAdr += 4;
        }
    }

    private void SpawnRow(int adr, uint val)
    {
        GameObject newRow = Instantiate(rowPrefab, content.transform);
        spawnedRows.Add(newRow);

        InstructionRow instructionRow = newRow.GetComponent<InstructionRow>();
        if (instructionRow is not null)
        {
            instructionRow.Initialize(adr, val, mode);
            instructionRow.instructionInput.onSelect.AddListener((val) => CheckAutoExpand(newRow));
        }
    }

    private void CheckAutoExpand(GameObject rowObject)
    {
        if (spawnedRows.IndexOf(rowObject) >= spawnedRows.Count - 1)
        {
            AddRows(4);
        }
    }
}
