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

    /// <summary>
    /// 両方のレバー起動アニメーションを動かす
    /// </summary>
    /// <param name="leverLR">初期値はLeft</param>
    public void UseLever(LeverLR leverLR = LeverLR.left)
    {
        if (leverLR == LeverLR.left)
        {
            _mainPlayerAnimator.SetTrigger(UseLeverL);
            _subPlayerAnimator.SetTrigger(UseLeverL);
        }
        else
        {
            _mainPlayerAnimator.SetTrigger(UseLeverR);
            _subPlayerAnimator.SetTrigger(UseLeverR);
        }
    }

    /// <summary>
    /// 歩くアニメーションの前後を設定する
    /// </summary>
    /// <param name="forward"></param>
    public void MoveAnimation(bool forward)
    {
        _mainPlayerAnimator.SetBool(Forward, forward);
        _subPlayerAnimator.SetBool(Forward, forward);
        _mainPlayerAnimator.SetBool(Back, !forward);
        _subPlayerAnimator.SetBool(Back, !forward);
    }

    public enum LeverLR
    {
        left,
        right,
    }
}
