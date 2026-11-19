using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalWormController : MonoBehaviour
{
    [SerializeField] private WormHeadMover headMover;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (Vector2)mouseWorld - (Vector2)headMover.transform.position;
        headMover.Step(direction.normalized, Time.deltaTime);
    }
}