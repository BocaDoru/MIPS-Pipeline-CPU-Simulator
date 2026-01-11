using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResizableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public RectTransform windowRect;
    public RectTransform contentRect;
    public Button collapseButton;
    public float maxHeight = 800f;
    public float minHeight = 100f;

    public int resizeDirection = -1;

    public bool isCollapsed = false;
    public float previousHeight;

    public void OnDrag(PointerEventData eventData)
    {
        if (isCollapsed)
            return;

        float scaleFactor = 1f;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas is not null) 
            scaleFactor = canvas.scaleFactor;

        float heightChange = resizeDirection * eventData.delta.y / scaleFactor;

        Vector2 size = windowRect.sizeDelta;
        float oldHeight = size.y;
        float newHeight = Mathf.Clamp(oldHeight - heightChange, minHeight, maxHeight);

        windowRect.sizeDelta = new Vector2(size.x, newHeight);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        windowRect.SetAsLastSibling();
    }

    // StartButton is called before the first frame update
    void Start()
    {
        if (collapseButton is not null)
            collapseButton.onClick.AddListener(ToggleCollapse);

        previousHeight = windowRect.sizeDelta.y;
    }

    private void ToggleCollapse()
    {
        isCollapsed = !isCollapsed;
        if (isCollapsed)
        {
            previousHeight = windowRect.sizeDelta.y;
            windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x, 50);

            if (contentRect)
                contentRect.gameObject.SetActive(false);
        }
        else
        {
            windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x, previousHeight);

            if (contentRect)
                contentRect.gameObject.SetActive(true);
        }
    }
}
