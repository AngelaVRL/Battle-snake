using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkWorm : NetworkBehaviour
{
    [SerializeField] private WormHeadMover headMover;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            Vector2 direction = input.AimPoint - (Vector2)transform.position;
            headMover.Step(direction.normalized, Runner.DeltaTime);
        }
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Camera.main.GetComponent<CameraFollow>().SetTarget(transform);
        }
    }

}