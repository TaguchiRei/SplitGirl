using UnityEngine;

public class Lever : InteractObjectBase
{
    private static readonly int Use = Animator.StringToHash("Use");
    [SerializeField] Animator animator;
    public override void Interact()
    {
        animator.SetTrigger(Use);
        interactObject.Interact();
    }
}
