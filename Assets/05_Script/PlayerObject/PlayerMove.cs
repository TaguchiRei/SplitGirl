using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Back = Animator.StringToHash("Back");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int BlendLr = Animator.StringToHash("BlendLR");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int OnGround = Animator.StringToHash("OnGround"); 
    [SerializeField] float _moveSpeed;
    [SerializeField] float _jumpForce;
    [SerializeField] float _walkBaseAnimationSpeed;
    [SerializeField] float _runBaseAnimationSpeed;

    [SerializeField] PlayerInput _playerInput;
    [SerializeField] Animator _animator;
    [SerializeField] Rigidbody _rigidBody;

    private bool _moving = false;
    private bool _onGround;
    public Vector3 MoveDirection;
    
    
    private InputSystem_Actions _inputSystem;

    private void Start()
    {
        _onGround = true;
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMove;
        _inputSystem.Player.Move.canceled += CanselMove;
        
        _inputSystem.Enable();
    }

    private void Update()
    {
        if (_moving && _onGround)
        {
            _rigidBody.linearVelocity = new Vector3(MoveDirection.x, _rigidBody.linearVelocity.y, MoveDirection.z);
        }
    }


    //仮想パッドを動かしたときに呼び出される。
    private void OnMove(InputAction.CallbackContext context)
    {
        _moving = true;
        Vector2 input = context.ReadValue<Vector2>();
        float magnitude = input.magnitude;
        float lrWaight = input.x + 1;
        MoveDirection = new Vector3(input.x,0,input.y) * _moveSpeed;
        //前後移動で別のアニメーションにし、左右移動アニメーションとブレンドする
        if (input.y > 0)
        {
            _animator.SetBool(Back, false);
            _animator.SetBool(Forward, true);
        }
        else
        {
            _animator.SetBool(Forward, false);
            _animator.SetBool(Back, true);
        }

        _animator.SetFloat(BlendLr, lrWaight);
        //走っているか歩いているか
        if (magnitude < 0.5f)
        {
            _animator.SetBool(Run, false);
            _animator.SetFloat(Speed, Mathf.Max(magnitude * _walkBaseAnimationSpeed,0.5f));
        }
        else
        {
            _animator.SetBool(Run, true);
            _animator.SetFloat(Speed, magnitude * _runBaseAnimationSpeed);
        }
    }
    
    
    
    private void CanselMove(InputAction.CallbackContext context)
    {
        _moving = false;
        _animator.SetBool(Run, false);
        _animator.SetBool(Forward, false);
        _animator.SetBool(Back, false);
        _animator.SetFloat(BlendLr, 1);
        _animator.SetFloat(Speed, 1);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _onGround = true;
        }
    }
}