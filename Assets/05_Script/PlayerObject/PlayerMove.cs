using System.Linq;
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
    
    [SerializeField] Animator _animator;
    [SerializeField] Rigidbody _rigidBody;

    private bool _moving = false;
    private bool _onGround;

    public bool MoveMode;
    public Vector3 MoveDirection;
    
    //トータル画面長押し時間、距離
    private bool _isTouch = false;
    private float _splitTimer = 0f;
    private Vector2 _moveToTouch = Vector2.zero;
    
    private InputSystem_Actions _inputSystem;
    private InGameManager _inGameManager;

    private void Start()
    {
        MoveMode = false;
        _onGround = true;
        
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMove;
        _inputSystem.Player.Move.canceled += CanselMove;
        _inputSystem.Player.Look.started += OnTouch;
        _inputSystem.Player.Look.performed += PreformedTouch;
        _inputSystem.Enable();

        _inGameManager = FindAnyObjectByType<InGameManager>();
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
        float lrWeight = input.x + 1;
        if(MoveMode)
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

        _animator.SetFloat(BlendLr, lrWeight);
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

    private void OnTouch(InputAction.CallbackContext context)
    {
        _isTouch = true;
        _splitTimer = 0;
        _moveToTouch = Vector2.zero;
    }

    private void PreformedTouch(InputAction.CallbackContext context)
    {
        if (!_isTouch)
        {
            return;
        }
        var value = context.ReadValue<Vector2>();
        Debug.Log(value.x + " " + value.y);
        //操作による変化が閾値を上回っていた場合のみ操作を実行
        if (value.magnitude <= _inGameManager.LookThreshold)
        {
            return;
        }
        //transform.rotation = 
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

    private enum LookSplit
    {
        Look,
        Split
    }
}