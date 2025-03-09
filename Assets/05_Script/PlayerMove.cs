using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour
{
    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int OnGround = Animator.StringToHash("OnGround");
    [SerializeField] float _walkSpeed;
    [SerializeField] float _runSpeed;
    [SerializeField] float _jumpForce;
    
    [SerializeField] Animator _animator;
    [SerializeField] Rigidbody _rigidBody;

    private bool _onGround;

    private void Start()
    {
        _onGround = true;
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
#endif
    }

    private void Update()
    {
        //最終的にはスマホでリリースするので仮のパソコン用スクリプト
#if UNITY_EDITOR
        //unityEditor上ではキーボード操作できるようにする
        if (Input.GetKeyDown(KeyCode.Space) && _onGround)
        {
            _animator.SetTrigger(Jump);
            //ジャンプの処理
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _onGround = false;
        }
        //視点を左右に回転させる
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + Input.GetAxisRaw("Mouse X"), 0);
        //移動する
        Vector3 move = transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            move *= _runSpeed;
            _animator.SetBool(Run, true);
        }
        else
        {
            move *= _walkSpeed;
            _animator.SetBool(Run, false);
        }
        
        if (move.magnitude != 0)
        {
            _animator.SetBool(Move, true);
        }
        else
        {
            _animator.SetBool(Move, false);
        }

        if (Input.GetButtonDown("Jump"))
        {
            _animator.SetTrigger(Jump);
        }
        _rigidBody.linearVelocity = new Vector3(move.x * _walkSpeed, _rigidBody.linearVelocity.y, move.z * _walkSpeed);
#endif
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _onGround = true;
            _animator.SetTrigger(OnGround);
        }
    }
}
