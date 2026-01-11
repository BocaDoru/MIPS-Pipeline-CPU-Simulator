using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ComponentFactory : EditorWindow
{
    [MenuItem("GameObject/Components/Main Control", false, 11)]
    public static void CreateMainControl(MenuCommand cmd) => CreateEntity<MainControl>("Main Control", cmd.context, SetupMainControl);

    [MenuItem("GameObject/Components/ALU", false, 11)]
    public static void CreateALU(MenuCommand cmd) => CreateEntity<ALU>("ALU", cmd.context, SetupALU);

    [MenuItem("GameObject/Components/ALU Control", false, 11)]
    public static void CreateAluControl(MenuCommand cmd) => CreateEntity<AluControl>("ALU Control", cmd.context, SetupAluControl);

    [MenuItem("GameObject/Components/Clock", false, 11)]
    public static void CreateClock(MenuCommand cmd) => CreateEntity<Clock>("Clock", cmd.context, SetupClock);

    [MenuItem("GameObject/Components/Extension Unit", false, 11)]
    public static void CreateExtensionUnit(MenuCommand cmd) => CreateEntity<ExtensionUnit>("Extension Unit", cmd.context, SetupExtensionUnit);

    [MenuItem("GameObject/Components/Mux", false, 11)]
    public static void CreateMux(MenuCommand cmd) => CreateEntity<Mux>("Mux", cmd.context, SetupMux);

    [MenuItem("GameObject/Components/Operation Unit", false, 11)]
    public static void CreateOperationUnit(MenuCommand cmd) => CreateEntity<OperationUnit>("Operation Unit", cmd.context, SetupOperationUnit);

    [MenuItem("GameObject/Components/RAM", false, 11)]
    public static void CreateRAM(MenuCommand cmd) => CreateEntity<RAM>("RAM", cmd.context, SetupRAM);

    [MenuItem("GameObject/Components/ROM", false, 11)]
    public static void CreateROM(MenuCommand cmd) => CreateEntity<ROM>("ROM", cmd.context, SetupROM);

    [MenuItem("GameObject/Components/Register", false, 11)]
    public static void CreateRegister(MenuCommand cmd) => CreateEntity<Register>("Register", cmd.context, SetupRegister);

    [MenuItem("GameObject/Components/Register File", false, 11)]
    public static void CreateRegisterFile(MenuCommand cmd) => CreateEntity<RegisterFile>("Register File", cmd.context, SetupRegisterFile);

    [MenuItem("GameObject/Components/Signal Adder", false, 11)]
    public static void CreateSignalAdder(MenuCommand cmd) => CreateEntity<SignalAdder>("Signal Adder", cmd.context, SetupSignalAdder);

    [MenuItem("GameObject/Components/Signal Splitter", false, 11)]
    public static void CreateSignalSplitter(MenuCommand cmd) => CreateEntity<SignalSplitter>("Signal Splitter", cmd.context, SetupSignalSplitter);

    [MenuItem("GameObject/Components/Forwarding Unit", false, 11)]
    public static void CreateForwardingUnit(MenuCommand cmd) => CreateEntity<ForwardingUnit>("Forwarding Unit", cmd.context, SetupForwardingUnit);
    
    [MenuItem("GameObject/Components/Comparator", false, 11)]
    public static void CreateComparator(MenuCommand cmd) => CreateEntity<Comparator>("Comparator", cmd.context, SetupComparator);
    
    [MenuItem("GameObject/Components/Hazard Unit", false, 11)]
    public static void CreateHazardUnit(MenuCommand cmd) => CreateEntity<HazardUnit>("Hazard Unit", cmd.context, SetupHazardUnit);

    [MenuItem("GameObject/Components/Add Input Port", false, 20)]
    public static void AddInputPort(MenuCommand cmd)
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null) return;

        Mux mux = selected.GetComponent<Mux>();
        SignalAdder adder = selected.GetComponent<SignalAdder>();

        if (mux != null)
        {
            if (mux.inputs == null) mux.inputs = new Signal[0];

            int oldCount = mux.inputs.Length;
            int newCount = oldCount + 1;

            Array.Resize(ref mux.inputs, newCount);

            mux.selectionNumber = (uint)newCount;

            for (int i = 0; i < newCount; i++)
            {
                float y = (newCount > 1) ? 0.5f - (i * (1.0f / (newCount - 1))) : 0f;
                string portName = $"Input {i}";
                CircuitNode node = CreatePort(selected, portName, new Vector2(-1, y), true);
                UpdatePortPosition(node, new Vector2(-1, y));
            }

            Debug.Log($"Added Input {newCount - 1} to Mux. Rearranged {newCount} ports.");
        }
        else if (adder != null)
        {
            if (adder.inputs == null) adder.inputs = new Signal[0];

            int oldCount = adder.inputs.Length;
            int newCount = oldCount + 1;

            Array.Resize(ref adder.inputs, newCount);

            for (int i = 0; i < newCount; i++)
            {
                float y = (newCount > 1) ? 0.5f - (i * (1.0f / (newCount - 1))) : 0f;
                string portName = $"Input {i}";
                CircuitNode node = CreatePort(selected, portName, new Vector2(-1, y), true);
                UpdatePortPosition(node, new Vector2(-1, y));
            }

            Debug.Log($"Added Input {newCount - 1} to Signal Adder. Rearranged {newCount} ports.");
        }
        else
        {
            Debug.LogWarning("Selected object is not a Mux or Signal Adder.");
        }
    }

    [MenuItem("GameObject/Components/Add Output Port", false, 20)]
    public static void AddOutputPort(MenuCommand cmd)
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null) return;

        SignalSplitter splitter = selected.GetComponent<SignalSplitter>();

        if (splitter != null)
        {
            if (splitter.outputs == null) splitter.outputs = new Signal[0];
            if (splitter.intervals == null) splitter.intervals = new Interval[0];

            int oldCount = splitter.outputs.Length;
            int newCount = oldCount + 1;

            Array.Resize(ref splitter.outputs, newCount);
            Array.Resize(ref splitter.intervals, newCount);
            splitter.intervals[newCount - 1] = new Interval();

            for (int i = 0; i < newCount; i++)
            {
                float y = (newCount > 1) ? 0.5f - (i * (1.0f / (newCount - 1))) : 0f;
                string portName = $"Output {i}";

                CircuitNode node = CreatePort(selected, portName, new Vector2(1, y), false);
                UpdatePortPosition(node, new Vector2(1, y));

                if (i >= oldCount)
                {
                    splitter.outputs[i] = CreateWireForOutput(node);
                }
            }

            Debug.Log($"Added Output {newCount - 1} to Signal Splitter. Rearranged {newCount} ports.");
        }
        else
        {
            Debug.LogWarning("Selected object is not a Signal Splitter.");
        }
    }

    private static void UpdatePortPosition(CircuitNode node, Vector2 normalizedPos)
    {
        RectTransform rt = node.GetComponent<RectTransform>();
        Vector2 anchor = normalizedPos * 0.75f + new Vector2(0.75f, 0.5f);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
    }

    private static void SetupForwardingUnit(GameObject go)
    {
        ForwardingUnit script = go.AddComponent<ForwardingUnit>();

        CreatePort(go, "ID Rs", new Vector2(-1, 0.5f), true);
        CreatePort(go, "ID Rt", new Vector2(-1, -0.5f), true);
        CreatePort(go, "EX Rd", new Vector2(1f, 0.5f), true);
        CreatePort(go, "MEM Rd", new Vector2(1f, -0.5f), true);
        CreatePort(go, "EX RegWrite", new Vector2(-0.5f, -1f), true);
        CreatePort(go, "MEM RegWrite", new Vector2(0.5f, -1f), true);

        var fwdA = CreatePort(go, "Fwd A", new Vector2(0.5f, 1f), false);
        var fwdB = CreatePort(go, "Fwd B", new Vector2(-0.5f, 1f), false);

        script.forwardA = CreateWireForOutput(fwdA);
        script.forwardB = CreateWireForOutput(fwdB);

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
    }

    private static void SetupHazardUnit(GameObject go)
    {
        HazardUnit script = go.AddComponent<HazardUnit>();

        CreatePort(go, "RT IF/ID", new Vector2(-0.6f, -1f), true);
        CreatePort(go, "RS IF/ID", new Vector2(-0.2f, -1f), true);
        CreatePort(go, "RT ID/EX", new Vector2(0.2f, -1f), true);
        CreatePort(go, "RT EX/MEM", new Vector2(0.6f, -1f), true);
        CreatePort(go, "Mem Read ID/EX", new Vector2(-0.5f, 1f), true);
        CreatePort(go, "Mem Read EX/MEM", new Vector2(0.5f, 1f), true);

        var pcWrite = CreatePort(go, "PC Write", new Vector2(-1f, 0.5f), false);
        var ifIdWrite = CreatePort(go, "IF/ID Write", new Vector2(-1f, -0.5f), false);
        var bubble = CreatePort(go, "Bubble", new Vector2(1f, 0f), false);

        script.pcWrite = CreateWireForOutput(pcWrite);
        script.ifIdWrite = CreateWireForOutput(ifIdWrite);
        script.bubble = CreateWireForOutput(bubble);

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
    }

    private static void SetupComparator(GameObject go)
    {
        Comparator script = go.AddComponent<Comparator>();

        CreatePort(go, "A", new Vector2(-1, 0.5f), true);
        CreatePort(go, "B", new Vector2(-1, -0.5f), true);

        var equal = CreatePort(go, "ZERO", new Vector2(1f, 0.6f), false);
        var notEqual = CreatePort(go, "NE", new Vector2(1f, 0.2f), false);
        var greater = CreatePort(go, "GTZ", new Vector2(1f, -0.2f), false);
        var greaterOrEqual = CreatePort(go, "GEZ", new Vector2(1f, -0.6f), false);

        script.equal = CreateWireForOutput(equal);
        script.notEqual = CreateWireForOutput(notEqual);
        script.greater = CreateWireForOutput(greater);
        script.greaterOrEqual = CreateWireForOutput(greaterOrEqual);

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
    }

    private static void SetupMainControl(GameObject go)
    {
        MainControl script = go.AddComponent<MainControl>();
        CreatePort(go, "OpCode", new Vector2(-1, 0), true);
        var outNode = CreatePort(go, "Control Signal", new Vector2(1, 0), false);
        script.controlSignal = CreateWireForOutput(outNode);
    }

    private static void SetupALU(GameObject go)
    {
        ALU script = go.AddComponent<ALU>();
        CreatePort(go, "A", new Vector2(-1, 0.5f), true);
        CreatePort(go, "B", new Vector2(-1, -0.5f), true);
        CreatePort(go, "ALU Ctrl", new Vector2(0, 1f), true);
        CreatePort(go, "SA", new Vector2(-0.5f, 1f), true);
        var rNode = CreatePort(go, "Result", new Vector2(1, 0), false);
        var flagsNode = CreatePort(go, "Flags", new Vector2(1, -0.5f), false);
        script.r = CreateWireForOutput(rNode);
    }

    private static void SetupAluControl(GameObject go)
    {
        AluControl script = go.AddComponent<AluControl>();
        CreatePort(go, "ALU Op", new Vector2(-1, 0.5f), true);
        CreatePort(go, "Func", new Vector2(-1, -0.5f), true);
        var outNode = CreatePort(go, "ALU Ctrl", new Vector2(1, 0), false);
        script.aluCtrl = CreateWireForOutput(outNode);
    }

    private static void SetupClock(GameObject go)
    {
        Clock script = go.AddComponent<Clock>();
        var outNode = CreatePort(go, "Clock Signal", new Vector2(1, 0), false);
        script.clock = CreateWireForOutput(outNode);

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
    }

    private static void SetupExtensionUnit(GameObject go)
    {
        ExtensionUnit script = go.AddComponent<ExtensionUnit>();
        CreatePort(go, "Input", new Vector2(-1, 0), true);
        CreatePort(go, "Signed", new Vector2(0, 1), true);
        var outNode = CreatePort(go, "Output", new Vector2(1, 0), false);
        script.output = CreateWireForOutput(outNode);
    }

    private static void SetupRegister(GameObject go)
    {
        Register script = go.AddComponent<Register>();
        CreatePort(go, "Data Input", new Vector2(-1, 0), true);
        CreatePort(go, "Clock", new Vector2(0, -1), true);
        CreatePort(go, "Flush", new Vector2(-1, -0.75f), true);
        CreatePort(go, "Write Enable", new Vector2(-1, 0.75f), true);

        var outNode = CreatePort(go, "Data Output", new Vector2(1, 0), false);
        script.dataOutput = CreateWireForOutput(outNode);
    }

    private static void SetupMux(GameObject go)
    {
        Mux script = go.AddComponent<Mux>();
        script.selectionNumber = 2;
        CreatePort(go, "Input 0", new Vector2(-1, 0.5f), true);
        CreatePort(go, "Input 1", new Vector2(-1, -0.5f), true);
        CreatePort(go, "Selection", new Vector2(0, -1), true);
        var outNode = CreatePort(go, "Output", new Vector2(1, 0), false);
        script.output = CreateWireForOutput(outNode);
        script.inputs = new Signal[2];
    }

    private static void SetupOperationUnit(GameObject go)
    {
        OperationUnit script = go.AddComponent<OperationUnit>();
        CreatePort(go, "A", new Vector2(-1, 0.5f), true);
        CreatePort(go, "B", new Vector2(-1, -0.5f), true);
        CreatePort(go, "Const", new Vector2(0, -1), true);
        var rNode = CreatePort(go, "R", new Vector2(1, 0), false);
        script.r = CreateWireForOutput(rNode);
    }

    private static void SetupRAM(GameObject go)
    {
        RAM script = go.AddComponent<RAM>();
        CreatePort(go, "Address", new Vector2(-1, 0.7f), true);
        CreatePort(go, "Write Data", new Vector2(-1, 0.2f), true);
        CreatePort(go, "Write Enable", new Vector2(-1, -0.3f), true);
        CreatePort(go, "Clock", new Vector2(0, -1), true);
        var outNode = CreatePort(go, "Read Data", new Vector2(1, 0.5f), false);
        script.readData = CreateWireForOutput(outNode);
    }

    private static void SetupROM(GameObject go)
    {
        ROM script = go.AddComponent<ROM>();
        CreatePort(go, "Address", new Vector2(-1, 0), true);
        var outNode = CreatePort(go, "Data", new Vector2(1, 0), false);
        script.data = CreateWireForOutput(outNode);
    }

    private static void SetupRegisterFile(GameObject go)
    {
        RegisterFile script = go.AddComponent<RegisterFile>();
        CreatePort(go, "Read Adr 1", new Vector2(-1, 0.8f), true);
        CreatePort(go, "Read Adr 2", new Vector2(-1, 0.6f), true);
        CreatePort(go, "Write Adr", new Vector2(-1, 0.6f), true);
        CreatePort(go, "Write Data", new Vector2(-1, -0.8f), true);
        CreatePort(go, "Reg Write", new Vector2(0, 1), true);
        CreatePort(go, "Clock", new Vector2(0, -1), true);
        var d1Node = CreatePort(go, "Read Data 1", new Vector2(1, 0.5f), false);
        var d2Node = CreatePort(go, "Read Data 2", new Vector2(1, -0.5f), false);
        script.readData1 = CreateWireForOutput(d1Node);
        script.readData2 = CreateWireForOutput(d2Node);
    }

    private static void SetupSignalAdder(GameObject go)
    {
        SignalAdder script = go.AddComponent<SignalAdder>();
        CreatePort(go, "Input 0", new Vector2(-1, 0.5f), true);
        CreatePort(go, "Input 1", new Vector2(-1, -0.5f), true);
        script.inputs = new Signal[2];
        var outNode = CreatePort(go, "Output", new Vector2(1, 0), false);
        script.output = CreateWireForOutput(outNode);
    }

    private static void SetupSignalSplitter(GameObject go)
    {
        SignalSplitter script = go.AddComponent<SignalSplitter>();
        CreatePort(go, "Input", new Vector2(-1, 0), true);
        var out0 = CreatePort(go, "Output 0", new Vector2(1, 0.5f), false);
        var out1 = CreatePort(go, "Output 1", new Vector2(1, -0.5f), false);
        script.outputs = new Signal[2];
        script.outputs[0] = CreateWireForOutput(out0);
        script.outputs[1] = CreateWireForOutput(out1);
        script.intervals = new Interval[2];
        script.intervals[0] = new Interval();
        script.intervals[1] = new Interval();
    }

    private delegate void SetupDelegate(GameObject go);

    private static void CreateEntity<T>(string name, UnityEngine.Object context, SetupDelegate setupLogic) where T : MonoBehaviour
    {
        GameObject obj = CreateBaseBody(name, context as GameObject, new Color(0.25f, 0.25f, 0.25f));
        setupLogic(obj);
        FinalizeCreation(obj, name);
    }

    private static GameObject CreateBaseBody(string name, GameObject parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        GameObjectUtility.SetParentAndAlign(obj, parent);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 120);
        obj.GetComponent<Image>().color = color;
        return obj;
    }

    private static void FinalizeCreation(GameObject obj, string name)
    {
        Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        Selection.activeObject = obj;
    }

    public static CircuitNode CreatePort(GameObject parent, string name, Vector2 normalizedPos, bool isInput)
    {
        if (HasChild(parent.transform, name)) return parent.transform.Find(name).GetComponent<CircuitNode>();

        GameObject portObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        portObj.transform.SetParent(parent.transform, false);

        portObj.GetComponent<Image>().color = isInput ? Color.yellow : Color.cyan;
        RectTransform rt = portObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(15, 15);

        CircuitNode node = portObj.AddComponent<CircuitNode>();
        node.normalizedPortPosition = Vector2.zero;

        Vector2 anchor = normalizedPos * 0.5f + new Vector2(0.5f, 0.5f);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;

        return node;
    }

    public static Signal CreateWireForOutput(CircuitNode outputNode)
    {
        WireVisual wire = WireVisual.Create(outputNode, null);
        wire.gameObject.name = "Output Wire";

        Signal newSignal = ScriptableObject.CreateInstance<Signal>();
        newSignal.name = "Auto Signal";
        newSignal.Value = new BitArray(new bool[] { false });

        wire.signalToWatch = newSignal;
        return newSignal;
    }

    private static bool HasChild(Transform parent, string name)
    {
        foreach (Transform t in parent) if (t.name == name) return true;
        return false;
    }
}