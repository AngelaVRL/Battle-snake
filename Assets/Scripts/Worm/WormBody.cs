using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBody : MonoBehaviour
{
    private const float BodyBrightness = 0.8f;

    [SerializeField] private Transform head;
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private Transform segmentPrefab;
    [SerializeField] private float segmentSpacing = 0.3f;
    [SerializeField] private float maxThickness = 2.5f;
    [SerializeField] private int segmentsForMaxSize = 100;

    private readonly List<Transform> segments = new List<Transform>();
    private Vector3 headBaseScale;
    private Sprite bodySprite;
    private Color bodyColor = Color.white;

    public float Thickness { get; private set; } = 1f;
    public float GrowthProgress { get; private set; }

    private void Awake()
    {
        headBaseScale = headRenderer.transform.localScale;
    }

    private void LateUpdate()
    {
        FollowHead();
    }

    private void OnDestroy()
    {
        foreach (Transform segment in segments)
        {
            if (segment != null)
            {
                Destroy(segment.gameObject);
            }
        }
    }

    public void ApplyStyle(Sprite headSprite, Sprite newBodySprite, Color headColor)
    {
        headRenderer.sprite = headSprite;
        headRenderer.color = headColor;
        bodySprite = newBodySprite;

        Color.RGBToHSV(headColor, out float hue, out float saturation, out float value);
        bodyColor = Color.HSVToRGB(hue, saturation, value * BodyBrightness);

        foreach (Transform segment in segments)
        {
            StyleSegment(segment);
        }
    }

    public void SetHeadVisible(bool isVisible)
    {
        if (headRenderer.gameObject.activeSelf != isVisible)
        {
            headRenderer.gameObject.SetActive(isVisible);
        }
    }

    public void SetSegmentCount(int targetCount)
    {
        while (segments.Count < targetCount)
        {
            Transform last = segments.Count > 0 ? segments[segments.Count - 1] : head;
            Transform segment = Instantiate(segmentPrefab, last.position, Quaternion.identity);
            StyleSegment(segment);
            segments.Add(segment);
        }

        while (segments.Count > targetCount)
        {
            int lastIndex = segments.Count - 1;
            Destroy(segments[lastIndex].gameObject);
            segments.RemoveAt(lastIndex);
        }

        UpdateThickness(targetCount);
    }

    public bool OverlapsPoint(Vector2 point, float radius)
    {
        float sqrRadius = radius * radius;

        for (int i = 0; i < segments.Count; i++)
        {
            if (((Vector2)segments[i].position - point).sqrMagnitude <= sqrRadius)
            {
                return true;
            }
        }

        return false;
    }

    public List<Vector2> GetSegmentPositions(int step)
    {
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < segments.Count; i += step)
        {
            positions.Add(segments[i].position);
        }

        return positions;
    }

    private void StyleSegment(Transform segment)
    {
        if (bodySprite == null)
        {
            return;
        }

        SpriteRenderer segmentRenderer = segment.GetComponent<SpriteRenderer>();
        segmentRenderer.sprite = bodySprite;
        segmentRenderer.color = bodyColor;
    }

    private void UpdateThickness(int segmentCount)
    {
        GrowthProgress = Mathf.Clamp01(segmentCount / (float)segmentsForMaxSize);
        Thickness = Mathf.Lerp(1f, maxThickness, GrowthProgress);

        headRenderer.transform.localScale = headBaseScale * Thickness;

        foreach (Transform segment in segments)
        {
            segment.localScale = segmentPrefab.localScale * Thickness;
        }
    }

    private void FollowHead()
    {
        float spacing = segmentSpacing * Thickness;

        for (int i = 0; i < segments.Count; i++)
        {
            Transform previous = i == 0 ? head : segments[i - 1];
            Vector3 offset = segments[i].position - previous.position;
            segments[i].position = previous.position + offset.normalized * spacing;
        }
    }
}