using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] Animator _animator;
    
    public bool OnGround;
    

    private void Start()
    {
        OnGround = true;
        
        //アニメーション関連初期化
        var pam = FindAnyObjectByType<PlayerAnimationManager>();
        //アニメーションを設定
        if (gameObject.CompareTag("MainPlayer"))
        {
            pam._mainPlayerAnimator = _animator;
        }
        else
        {
            pam._subPlayerAnimator = _animator;
        }
    }

    void Update()
    {
        if (Physics.SphereCast(new Vector3(transform.position.x,transform.position.y + 1,transform.position.z)
                , 0.2f, Vector3.down, out RaycastHit hit,20f,LayerMask.GetMask("MainGround","SubGround")))
        {
            OnGround = hit.distance < 1f;
        }
    }

    private enum LookSplit
    {
        Look,
        Split
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.05f + Vector3.up, 1.2f);
    }
}