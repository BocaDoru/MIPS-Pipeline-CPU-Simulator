using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CircuitAutoFixer : EditorWindow
{
    [MenuItem("Tools/Circuit/Add Labels to Selected")]
    public static void AddLabelsToSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int count = 0;

        TMP_FontAsset customFont = Resources.Load<TMP_FontAsset>("CircuitFont");

        if (customFont == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset Arial");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                customFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
        }

        foreach (GameObject go in selectedObjects)
        {
            if (go.GetComponent<CircuitNode>() != null) continue;

            bool excludeCenterLabel = go.GetComponent<SignalAdder>() != null ||
                                      go.GetComponent<SignalSplitter>() != null ||
                                      go.GetComponent<Mux>() != null ||
                                      go.GetComponent<OperationUnit>() != null;

            AddLabelsToComponent(go, !excludeCenterLabel, customFont);
            count++;
        }

        Debug.Log($"Added labels to {count} Components!");
    }

    private static void AddLabelsToComponent(GameObject go, bool addCenterLabel, TMP_FontAsset font)
    {
        if (addCenterLabel)
        {
            Transform existingLabel = go.transform.Find("Label");
            if (existingLabel == null)
            {
                GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(go.transform, false);

                TextMeshProUGUI text = labelObj.GetComponent<TextMeshProUGUI>();

                if (go.GetComponent<Register>() != null)
                {
                    text.text = "REG";
                }
                else
                {
                    text.text = go.name.Replace("ID", "").Replace("(Clone)", "").Trim();
                }

                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 24;
                text.color = Color.black;
                text.enableWordWrapping = false;
                text.fontStyle = FontStyles.Bold;

                if (font != null) text.font = font;

                RectTransform rt = labelObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;

                labelObj.transform.SetSiblingIndex(1);
            }
        }

        CircuitNode[] ports = go.GetComponentsInChildren<CircuitNode>();
        foreach (CircuitNode port in ports)
        {
            Transform portTransform = port.transform;
            if (portTransform.parent != go.transform) continue;

            Transform existingPortLabel = portTransform.Find("PortLabel");

            GameObject labelObj;
            if (existingPortLabel == null)
            {
                labelObj = new GameObject("PortLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(portTransform, false);
            }
            else
            {
                labelObj = existingPortLabel.gameObject;
            }

            TextMeshProUGUI text = labelObj.GetComponent<TextMeshProUGUI>();

            string portName = port.name;
            string labelText = "";
            string[] words = portName.Split(' ');

            if (words.Length == 1)
            {
                labelText = portName.Length > 3 ? portName.Substring(0, 3) : portName;
            }
            else
            {
                foreach (string word in words)
                {
                    if (!string.IsNullOrEmpty(word))
                    {
                        labelText += word[0];
                    }
                }
            }

            text.text = labelText.ToUpper();
            text.fontSize = 20;
            text.color = Color.black;
            text.enableWordWrapping = false;
            text.fontStyle = FontStyles.Bold;
            if (font != null) text.font = font;

            RectTransform rt = labelObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 20);

            RectTransform portRect = port.GetComponent<RectTransform>();
            float anchorX = (portRect.anchorMin.x + portRect.anchorMax.x) / 2f;
            float anchorY = (portRect.anchorMin.y + portRect.anchorMax.y) / 2f;

            float padding = 10f;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            if (anchorX < 0.25f)
            {
                rt.pivot = new Vector2(0, 0.5f);
                rt.anchoredPosition = new Vector2(padding, 0);
                text.alignment = TextAlignmentOptions.Left;
            }
            else if (anchorX > 0.75f)
            {
                rt.pivot = new Vector2(1, 0.5f);
                rt.anchoredPosition = new Vector2(-padding, 0);
                text.alignment = TextAlignmentOptions.Right;
            }
            else
            {
                rt.pivot = new Vector2(0.5f, 0.5f);
                text.alignment = TextAlignmentOptions.Center;

                if (anchorY > 0.75f)
                    rt.anchoredPosition = new Vector2(0, -padding);
                else if (anchorY < 0.25f)
                    rt.anchoredPosition = new Vector2(0, padding);
                else
                    rt.anchoredPosition = Vector2.zero;
            }

            rt.localScale = Vector3.one;
        }
    }

    [MenuItem("Tools/Circuit/Upgrade Selected Components")]
    public static void UpgradeSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int count = 0;

        foreach (GameObject go in selectedObjects)
        {
            if (FixIfFound<Register>(go, FixRegister)) count++;
            else if (FixIfFound<HazardUnit>(go, FixHazardUnit)) count++;
            else if (FixIfFound<AluControl>(go, FixAluControl)) count++;
            else if (FixIfFound<Clock>(go, FixClock)) count++;
            else if (FixIfFound<ExtensionUnit>(go, FixExtensionUnit)) count++;
            else if (FixIfFound<MainControl>(go, FixMainControl)) count++;
            else if (FixIfFound<ALU>(go, FixALU)) count++;
            else if (FixIfFound<Mux>(go, FixMux)) count++;
            else if (FixIfFound<OperationUnit>(go, FixOperationUnit)) count++;
            else if (FixIfFound<RAM>(go, FixRAM)) count++;
            else if (FixIfFound<ROM>(go, FixROM)) count++;
            else if (FixIfFound<RegisterFile>(go, FixRegisterFile)) count++;
            else if (FixIfFound<SignalAdder>(go, FixSignalAdder)) count++;
            else if (FixIfFound<SignalSplitter>(go, FixSignalSplitter)) count++;
        }

        Debug.Log($"Successfully upgraded {count} Components!");
    }

    private static bool FixIfFound<T>(GameObject go, System.Action<T> fixAction) where T : MonoBehaviour
    {
        T component = go.GetComponent<T>();
        if (component != null)
        {
            EnsureImage(go);
            fixAction(component);
            EditorUtility.SetDirty(go);
            return true;
        }
        return false;
    }

    private static void EnsureImage(GameObject go)
    {
        if (go.GetComponent<Image>() == null)
        {
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.25f);
        }
    }

    private static void FixAluControl(AluControl script)
    {
        EnsurePort(script.gameObject, "ALU Op", new Vector2(-1, 0.5f), true);
        EnsurePort(script.gameObject, "Func", new Vector2(-1, -0.5f), true);
        var outNode = EnsurePort(script.gameObject, "ALU Ctrl", new Vector2(1, 0), false);

        script.aluCtrl = EnsureOutputWire(outNode, script.aluCtrl);
    }

    private static void FixClock(Clock script)
    {
        var outNode = EnsurePort(script.gameObject, "Clock Signal", new Vector2(1, 0), false);
        script.clock = EnsureOutputWire(outNode, script.clock);
    }

    private static void FixExtensionUnit(ExtensionUnit script)
    {
        EnsurePort(script.gameObject, "Input", new Vector2(-1, 0), true);
        EnsurePort(script.gameObject, "Signed", new Vector2(0, 1), true);
        var outNode = EnsurePort(script.gameObject, "Output", new Vector2(1, 0), false);
        script.output = EnsureOutputWire(outNode, script.output);
    }

    private static void FixRegister(Register script)
    {
        EnsurePort(script.gameObject, "Data Input", new Vector2(-1, 0), true);
        EnsurePort(script.gameObject, "Clock", new Vector2(0, -1), true);
        EnsurePort(script.gameObject, "Flush", new Vector2(-1, -0.75f), true);
        EnsurePort(script.gameObject, "Write Enable", new Vector2(-1, 0.75f), true);
        var outNode = EnsurePort(script.gameObject, "Data Output", new Vector2(1, 0), false);
        script.dataOutput = EnsureOutputWire(outNode, script.dataOutput);
    }

    private static void FixHazardUnit(HazardUnit script)
    {
        EnsurePort(script.gameObject, "RT IF/ID", new Vector2(-0.6f, -1f), true);
        EnsurePort(script.gameObject, "RS IF/ID", new Vector2(-0.2f, -1f), true);
        EnsurePort(script.gameObject, "RT ID/EX", new Vector2(0.2f, -1f), true);
        EnsurePort(script.gameObject, "RT EX/MEM", new Vector2(0.6f, -1f), true);
        EnsurePort(script.gameObject, "Mem Read ID/EX", new Vector2(-0.5f, 1f), true);
        EnsurePort(script.gameObject, "Mem Read EX/MEM", new Vector2(0.5f, 1f), true);
        var pcWrite = EnsurePort(script.gameObject, "PC Write", new Vector2(-1f, 0.5f), false);
        var ifIdWrite = EnsurePort(script.gameObject, "IF/ID Write", new Vector2(-1f, -0.5f), false);
        var bubble = EnsurePort(script.gameObject, "Bubble", new Vector2(1f, 0f), false);

        script.pcWrite = EnsureOutputWire(pcWrite, script.pcWrite);
        script.ifIdWrite = EnsureOutputWire(ifIdWrite, script.ifIdWrite);
        script.bubble = EnsureOutputWire(pcWrite, script.bubble);
    }

    private static void FixMainControl(MainControl script)
    {
        EnsurePort(script.gameObject, "OpCode", new Vector2(-1, 0), true);
        var outNode = EnsurePort(script.gameObject, "Control Signal", new Vector2(1, 0), false);
        script.controlSignal = EnsureOutputWire(outNode, script.controlSignal);
    }

    private static void FixALU(ALU script)
    {
        EnsurePort(script.gameObject, "A", new Vector2(-1, 0.5f), true);
        EnsurePort(script.gameObject, "B", new Vector2(-1, -0.5f), true);
        EnsurePort(script.gameObject, "ALU Ctrl", new Vector2(0, 1f), true);
        EnsurePort(script.gameObject, "SA", new Vector2(-0.5f, 1f), true);
        var rNode = EnsurePort(script.gameObject, "Result", new Vector2(1, 0), false);
        var fNode = EnsurePort(script.gameObject, "Flags", new Vector2(1, -0.5f), false);

        script.r = EnsureOutputWire(rNode, script.r);
    }

    private static void FixMux(Mux script)
    {
        int count = (script.inputs != null && script.inputs.Length > 0) ? script.inputs.Length : 2;

        if (script.inputs == null || script.inputs.Length != count)
        {
            var old = script.inputs ?? new Signal[0];
            script.inputs = new Signal[count];
            for (int i = 0; i < old.Length && i < count; i++) script.inputs[i] = old[i];
        }

        for (int i = 0; i < count; i++)
        {
            float t = (count > 1) ? (float)i / (count - 1) : 0.5f;
            float y = Mathf.Lerp(0.6f, -0.6f, t);

            var inNode = EnsurePort(script.gameObject, $"Input {i}", new Vector2(-1, y), true);

            RectTransform rt = inNode.GetComponent<RectTransform>();
            Vector2 anchor = new Vector2(-1, y) * 0.5f + new Vector2(0.5f, 0.5f);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;

            inNode.transform.SetSiblingIndex(i);
        }

        var selNode = EnsurePort(script.gameObject, "Selection", new Vector2(0, -1), true);
        selNode.transform.SetSiblingIndex(count);

        var outNode = EnsurePort(script.gameObject, "Output", new Vector2(1, 0), false);
        outNode.transform.SetSiblingIndex(count + 1);

        script.output = EnsureOutputWire(outNode, script.output);
    }

    private static void FixOperationUnit(OperationUnit script)
    {
        EnsurePort(script.gameObject, "A", new Vector2(-1, 0.5f), true);
        EnsurePort(script.gameObject, "B", new Vector2(-1, -0.5f), true);
        EnsurePort(script.gameObject, "Const", new Vector2(0, -1), true);
        var outNode = EnsurePort(script.gameObject, "R", new Vector2(1, 0), false);
        script.r = EnsureOutputWire(outNode, script.r);
    }

    private static void FixRAM(RAM script)
    {
        EnsurePort(script.gameObject, "Address", new Vector2(-1, 0.7f), true);
        EnsurePort(script.gameObject, "Write Data", new Vector2(-1, 0.2f), true);
        EnsurePort(script.gameObject, "Write Enable", new Vector2(-1, -0.3f), true);
        EnsurePort(script.gameObject, "Clock", new Vector2(0, -1), true);
        var outNode = EnsurePort(script.gameObject, "Read Data", new Vector2(1, 0.5f), false);
        script.readData = EnsureOutputWire(outNode, script.readData);
    }

    private static void FixROM(ROM script)
    {
        EnsurePort(script.gameObject, "Address", new Vector2(-1, 0), true);
        var outNode = EnsurePort(script.gameObject, "Data", new Vector2(1, 0), false);
        script.data = EnsureOutputWire(outNode, script.data);
    }

    private static void FixRegisterFile(RegisterFile script)
    {
        EnsurePort(script.gameObject, "Read Adr 1", new Vector2(-1, 0.8f), true);
        EnsurePort(script.gameObject, "Read Adr 2", new Vector2(-1, 0.6f), true);
        EnsurePort(script.gameObject, "Write Adr", new Vector2(-1, -0.6f), true);
        EnsurePort(script.gameObject, "Write Data", new Vector2(-1, -0.8f), true);
        EnsurePort(script.gameObject, "Reg Write", new Vector2(0, 1), true);
        EnsurePort(script.gameObject, "Clock", new Vector2(0, -1), true);
        var d1 = EnsurePort(script.gameObject, "Read Data 1", new Vector2(1, 0.5f), false);
        var d2 = EnsurePort(script.gameObject, "Read Data 2", new Vector2(1, -0.5f), false);

        script.readData1 = EnsureOutputWire(d1, script.readData1);
        script.readData2 = EnsureOutputWire(d2, script.readData2);
    }

    private static void FixSignalAdder(SignalAdder script)
    {
        int count = (script.inputs != null && script.inputs.Length > 0) ? script.inputs.Length : 2;

        if (script.inputs == null || script.inputs.Length != count)
        {
            var old = script.inputs ?? new Signal[0];
            script.inputs = new Signal[count];
            for (int i = 0; i < old.Length && i < count; i++) script.inputs[i] = old[i];
        }

        for (int i = 0; i < count; i++)
        {
            float t = (count > 1) ? (float)i / (count - 1) : 0.5f;
            float y = Mathf.Lerp(0.7f, -0.7f, t);

            var inNode = EnsurePort(script.gameObject, $"Input {i}", new Vector2(-1, y), true);

            RectTransform rt = inNode.GetComponent<RectTransform>();
            Vector2 anchor = new Vector2(-1, y) * 0.5f + new Vector2(0.5f, 0.5f);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;

            inNode.transform.SetSiblingIndex(i);
        }

        var outNode = EnsurePort(script.gameObject, "Output", new Vector2(1, 0), false);
        outNode.transform.SetSiblingIndex(count);

        script.output = EnsureOutputWire(outNode, script.output);
    }

    private static void FixSignalSplitter(SignalSplitter script)
    {
        var inputNode = EnsurePort(script.gameObject, "Input", new Vector2(-1, 0), true);
        inputNode.transform.SetAsFirstSibling();

        int count = (script.intervals != null && script.intervals.Length > 0) ? script.intervals.Length : 2;

        if (script.outputs == null || script.outputs.Length != count)
        {
            var old = script.outputs ?? new Signal[0];
            script.outputs = new Signal[count];
            for (int i = 0; i < old.Length && i < count; i++) script.outputs[i] = old[i];
        }

        for (int i = 0; i < count; i++)
        {
            float t = (count > 1) ? (float)i / (count - 1) : 0.5f;
            float y = Mathf.Lerp(0.7f, -0.7f, t);

            var outNode = EnsurePort(script.gameObject, $"Output {i}", new Vector2(1, y), false);

            RectTransform rt = outNode.GetComponent<RectTransform>();
            Vector2 anchor = new Vector2(1, y) * 0.5f + new Vector2(0.5f, 0.5f);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;

            outNode.transform.SetSiblingIndex(i + 1);

            script.outputs[i] = EnsureOutputWire(outNode, script.outputs[i]);
        }
    }

    private static CircuitNode EnsurePort(GameObject parent, string name, Vector2 normalizedPos, bool isInput)
    {
        return ComponentFactory.CreatePort(parent, name, normalizedPos, isInput);
    }

    private static Signal EnsureOutputWire(CircuitNode outputNode, Signal existingSignal = null)
    {
        WireVisual[] allWires = Object.FindObjectsOfType<WireVisual>();
        foreach (var w in allWires)
        {
            if (w.inputNode == outputNode)
            {
                if (existingSignal != null && w.signalToWatch != existingSignal)
                {
                    w.signalToWatch = existingSignal;
                    EditorUtility.SetDirty(w);
                }

                return w.signalToWatch != null ? w.signalToWatch : existingSignal;
            }
        }

        Signal createdSignal = ComponentFactory.CreateWireForOutput(outputNode);

        if (existingSignal != null)
        {
            WireVisual[] wires = Object.FindObjectsOfType<WireVisual>();
            foreach (var w in wires)
            {
                if (w.inputNode == outputNode)
                {
                    w.signalToWatch = existingSignal;
                    EditorUtility.SetDirty(w);
                    return existingSignal;
                }
            }
        }

        return createdSignal;
    }
}