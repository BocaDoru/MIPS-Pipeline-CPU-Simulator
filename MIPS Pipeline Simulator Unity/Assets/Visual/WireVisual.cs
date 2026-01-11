using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(RectTransform))]
public class WireVisual : MonoBehaviour
{
    public Signal signalToWatch;

    public CircuitNode inputNode;
    public CircuitNode outputNode;

    public Color colorOne = Color.green;
    public Color colorZero = Color.black;
    [Range(0.01f, 1f)] 
    public float lineWidth = 0.1f;

    public int lineNodes = 2;

    public int sortingOrder = 0;

    public bool autoSize = true;
    public static float baseWidth = 0.03f;
    public static float widthPerBit = 0.004f;

    private LineRenderer lineRenderer;
    private RectTransform rectTransform;

    private Texture2D lineTexture;
    private Material lineMaterial;
    private Color[] texturePixels;

    private void OnValidate()
    {
        InitializeLineRenderer();
        UpdateLinePosition();
        if (signalToWatch is not null)
            UpdateWidth(signalToWatch.Value.Length);
    }

    void OnDestroy()
    {
        if (lineTexture != null) DestroyImmediate(lineTexture);
        if (lineMaterial != null) DestroyImmediate(lineMaterial);
    }

    void Start() => InitializeLineRenderer();

    void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateLinePosition();
            return;
        }

        if (signalToWatch is null || signalToWatch.Value is null)
            return;

        bool[] bits = signalToWatch.Value.Bits;
        int bitCount = bits.Length;

        if (bitCount == 0) 
            return;

        if (lineTexture is null || lineTexture.width != bitCount)
        {
            if (lineTexture is not null) 
                Destroy(lineTexture);

            lineTexture = new Texture2D(1, bitCount, TextureFormat.RGBA32, false);

            lineTexture.filterMode = FilterMode.Point;
            lineTexture.wrapMode = TextureWrapMode.Clamp;

            texturePixels = new Color[bitCount];

            if (lineMaterial is null)
                InitializeLineRenderer();
            if (lineMaterial is not null)
                lineMaterial.mainTexture = lineTexture;

            if (autoSize)
            {

                if (signalToWatch is not null)
                    UpdateWidth(bitCount);
            }
        }

        for (int i = 0; i < bitCount; i++)
        {
            texturePixels[i] = bits[i] ? colorOne : colorZero;
        }

        lineTexture.SetPixels(texturePixels);
        lineTexture.Apply();
    }

    private void InitializeLineRenderer()
    {
        if (lineRenderer == null) 
            lineRenderer = GetComponent<LineRenderer>();
        if (rectTransform == null) 
            rectTransform = GetComponent<RectTransform>();

        if (lineRenderer is not null && (Application.isPlaying || lineMaterial is not null))
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = lineNodes;

            lineRenderer.positionCount = lineNodes;

            lineRenderer.sortingOrder = sortingOrder;

            float currentWidth = autoSize ? baseWidth : lineWidth;
            lineRenderer.widthMultiplier = currentWidth;
            lineRenderer.widthCurve = AnimationCurve.Linear(0, 1, 1, 1);

            if (lineMaterial is null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader is null) 
                    shader = Shader.Find("UI/Default");
                if (shader is null) 
                    shader = Shader.Find("Unlit/Texture");

                lineMaterial = new Material(shader);
                lineRenderer.sharedMaterial = lineMaterial;
            }
        }
    }

    private void UpdateLinePosition()
    {
        if (lineRenderer is not null)
        {
            if (!Application.isPlaying || !autoSize)
                lineRenderer.widthMultiplier = lineWidth;

            lineRenderer.sortingOrder = sortingOrder;
        }

        if (inputNode is not null)
        {
            Vector3 startAnchored = inputNode.GetAnchoredPosition();
            Vector3 endAnchored = (outputNode is not null) ? outputNode.GetAnchoredPosition() : startAnchored + new Vector3(50, 0, 0);

            if (rectTransform is not null)
                rectTransform.anchoredPosition3D = new Vector3(startAnchored.x, startAnchored.y, 0);

            Vector3 localStart = Vector3.zero;
            Vector3 localEnd = endAnchored - startAnchored;
            localEnd.z = 0;

            if (lineRenderer is not null)
            {
                lineRenderer.positionCount = lineNodes;
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPosition(0, localStart);
                lineRenderer.SetPosition(lineNodes - 1, localEnd);
            }

            if (signalToWatch is not null)
                UpdateWidth(signalToWatch.Value.Length);
        }
    }

    private void UpdateWidth(int bitCount)
    {
        if (lineRenderer == null) 
            return;

        float calculatedWidth = baseWidth + (Mathf.Max(0, bitCount - 1) * widthPerBit);

        lineRenderer.widthMultiplier = calculatedWidth;
    }

    public static WireVisual Create(CircuitNode input, CircuitNode output)
    {
        GameObject wireObj = new GameObject("Wire", typeof(RectTransform));

        WireVisual wireVisual = wireObj.AddComponent<WireVisual>();

        wireVisual.inputNode = input;
        wireVisual.outputNode = output;

        if (input is not null && input.transform.parent is not null && input.transform.parent.parent is not null)
        {
            wireObj.transform.SetParent(input.transform.parent.parent, false);
        }
        else if (input is not null && input.transform.parent is not null)
        {
            wireObj.transform.SetParent(input.transform.parent, false);
        }

        wireVisual.InitializeLineRenderer();
        wireVisual.UpdateLinePosition();

        return wireVisual;
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Wire Connection", false, 10)]
    private static void CreateFromMenu(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Wire", typeof(RectTransform));
        WireVisual visual = go.AddComponent<WireVisual>();

        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}
