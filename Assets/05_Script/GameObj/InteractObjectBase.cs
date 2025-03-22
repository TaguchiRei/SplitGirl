using UnityEngine;

public abstract class InteractObjectBase : MonoBehaviour
{
    [SerializeField] protected InteractedObjectBase interactObject;

    public abstract void Interact();

    private void OnDrawGizmos()
    {
        if (interactObject == null)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, interactObject.transform.position);
    }
}