using UnityEngine;

public class Door : InteractedObjectBase
{
    private static readonly int Open = Animator.StringToHash("Open");
    [SerializeField] Animator animator;
    public override void Interact()
    {
        _isInteracted = true;
        animator.SetBool(Open,true);
    }

    public override void Cancel()
    {
        _isInteracted = false;
        animator.SetBool(Open,false);
    }
}
