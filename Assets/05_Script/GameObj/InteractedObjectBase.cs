using System;
using UnityEngine;

public abstract class InteractedObjectBase : MonoBehaviour
{
    protected bool isInteracted = false;
    public abstract void Interact();

    private void OnDrawGizmos()
    {
        if(isInteracted)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
