using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI/Radial Layout Group")]
public class RadialLayoutGroup : LayoutGroup
{
    public float radius = 0f;
    public float startAngle = 90f;
    public float sweepAngle = 360f;
    public bool clockwise = true;
    public Vector2 childSize = new Vector2(200, 200);

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        DoLayout();
    }

    public override void CalculateLayoutInputVertical() => DoLayout();
    public override void SetLayoutHorizontal() { }
    public override void SetLayoutVertical() { }

    protected override void OnEnable()
    {
        base.OnEnable();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        EditorApplication.delayCall += () =>
        {
            if (this) LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        };
    }
#endif

    void DoLayout()
    {
        int n = rectChildren.Count;
        if (n == 0) return;

        float r = radius;
        if (r <= 0f)
        {
            float minSide = Mathf.Min(rectTransform.rect.width - padding.horizontal,
                                      rectTransform.rect.height - padding.vertical);
            r = Mathf.Max(0f, minSide * 0.5f);
        }

        float dir = clockwise ? -1f : 1f;
        float step = (n > 1) ? sweepAngle / n : 0f;

        for (int i = 0; i < n; i++)
        {
            RectTransform child = rectChildren[i];
            child.anchorMin = child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);

            if (childSize.x > 0) child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, childSize.x);
            if (childSize.y > 0) child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, childSize.y);

            float ang = startAngle + dir * (step * i + (n > 1 ? step * 0.5f : 0f));
            float rad = ang * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * r;

            child.anchoredPosition = pos;
            child.localRotation = Quaternion.identity;
        }
    }
}
