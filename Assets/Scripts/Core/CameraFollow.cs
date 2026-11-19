using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float baseSize = 5f;
    [SerializeField] private float maxSize = 12f;
    [SerializeField] private float zoomSmoothing = 2f;

    private Camera cameraComponent;
    private Transform target;
    private float growthProgress;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographicSize = baseSize;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetGrowthProgress(float progress)
    {
        growthProgress = progress;
    }

    private void LateUpdate()
    {
        float targetSize = Mathf.Lerp(baseSize, maxSize, growthProgress);
        cameraComponent.orthographicSize = Mathf.Lerp(
            cameraComponent.orthographicSize, targetSize, zoomSmoothing * Time.deltaTime);

        if (target == null)
        {
            return;
        }

        Vector3 position = target.position;
        position.z = transform.position.z;
        transform.position = position;
    }
}