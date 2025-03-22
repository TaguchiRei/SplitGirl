using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Back = Animator.StringToHash("Back");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int BlendLr = Animator.StringToHash("BlendLR");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int OnGround = Animator.StringToHash("OnGround");
    private static readonly int UseLeverR = Animator.StringToHash("UseLeverR");
    private static readonly int UseLeverL = Animator.StringToHash("UseLeverL");
    
    public Animator _mainPlayerAnimator;
    public Animator _subPlayerAnimator;

    void UseLever(LeverLR leverLR = LeverLR.left)
    {
        if (leverLR == LeverLR.left)
        {
            _mainPlayerAnimator.SetTrigger(UseLeverL);
            _subPlayerAnimator.SetTrigger(UseLeverL);
            _mainPlayerAnimator.SetTrigger(UseLeverL);
        }
        else
        {
            
        }
    }

    public enum LeverLR
    {
        left,
        right,
    }
}
