using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBody : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform segmentPrefab;
    [SerializeField] private float segmentSpacing = 0.35f;
    [SerializeField] private int initialSegments = 5;

    private readonly List<Transform> segments = new List<Transform>();

    private void Start()
    {
        Grow(initialSegments);
    }

    private void LateUpdate()
    {
        FollowHead();
    }

    public void Grow(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Transform last = segments.Count > 0 ? segments[segments.Count - 1] : head;
            Transform segment = Instantiate(segmentPrefab, last.position, Quaternion.identity);
            segments.Add(segment);
        }
    }

    private void FollowHead()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            Transform previous = i == 0 ? head : segments[i - 1];
            Vector3 offset = segments[i].position - previous.position;
            segments[i].position = previous.position + offset.normalized * segmentSpacing;
        }
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
}