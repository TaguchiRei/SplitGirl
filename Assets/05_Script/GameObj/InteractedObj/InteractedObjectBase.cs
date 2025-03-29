using System;
using UnityEngine;

public abstract class InteractedObjectBase : MonoBehaviour
{
    protected bool _isInteracted = false;

    public virtual void Interact()
    {
        _isInteracted = true;
    }

    public virtual void Cancel()
    {
        _isInteracted = false;
    }

    private void OnDrawGizmos()
    {
        if(_isInteracted)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
