using UnityEngine;

public class Door : InteractedObjectBase
{
    private static readonly int Open = Animator.StringToHash("Open");
    [SerializeField] Animator animator;
    public override void Interact()
    {
        animator.SetBool(Open,true);
    }

    public override void Cancel()
    {
        animator.SetBool(Open,false);
    }
}
