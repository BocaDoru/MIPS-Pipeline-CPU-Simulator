using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class CircuitNode : MonoBehaviour
{
    public Vector2 normalizedPortPosition = new Vector2(0f, 0f);

    private RectTransform _rectTransform;

    public RectTransform RectTrans
    {
        get
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    public Vector3 GetAnchoredPosition()
    {
        RectTransform parentRT = transform.parent as RectTransform;

        if (parentRT is null) 
            return transform.localPosition;

        Rect rect = RectTrans.rect;

        float internalX = normalizedPortPosition.x * rect.width * .5f;
        float internalY = normalizedPortPosition.y * rect.height * .5f;
        Vector3 internalOffset = new Vector3(internalX, internalY, 0);

        Vector3 posInGate = transform.localPosition + (transform.localRotation * Vector3.Scale(internalOffset, transform.localScale));

        return parentRT.anchoredPosition3D + (parentRT.localRotation * Vector3.Scale(posInGate, parentRT.localScale));
    }

    public Vector3 GetPortWorldPosition()
    {
        RectTransform rt = RectTrans;
        if (rt == null) 
            return transform.position;

        Rect rect = rt.rect;
        float x = normalizedPortPosition.x * rect.width * .5f;
        float y = normalizedPortPosition.y * rect.height * .5f;

        return rt.TransformPoint(new Vector3(x, y, 0));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetPortWorldPosition(), 0.05f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawSphere(GetPortWorldPosition(), 0.05f);
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Circuit Port", false, 10)]
    private static void CreateFromMenu(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Port", typeof(RectTransform));

        CircuitNode node = go.AddComponent<CircuitNode>();

        Image debugVisual = go.AddComponent<Image>();
        debugVisual.color = new Color(1, 1, 1, 0.2f);

        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

        RectTransform rt = node.GetComponent<RectTransform>();
        rt.sizeDelta = go.GetComponentInParent<RectTransform>().sizeDelta;

        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}