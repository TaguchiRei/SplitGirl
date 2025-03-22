using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class PlayerMove : MonoBehaviour
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
    [SerializeField] float _moveSpeed;
    [SerializeField] float _jumpForce;
    [SerializeField] float _walkBaseAnimationSpeed;
    [SerializeField] float _runBaseAnimationSpeed;

    [SerializeField] Animator _animator;
    [SerializeField] Rigidbody _rigidBody;

    private bool _canMove;
    private bool _moving = false;
    private bool _onGround;

    public bool MoveMode;
    public Vector3 MoveDirection;

    //トータル画面長押し時間、距離
    private bool _isTouch = false;
    private float _splitTimer = 0f;
    private Vector2 _moveToTouch = Vector2.zero;
    private Collider[] _interactColliders = new Collider[10];

    private InputSystem_Actions _inputSystem;
    private InGameManager _inGameManager;

    private void Start()
    {
        MoveMode = false;
        _onGround = true;
        _canMove = true;
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMove;
        _inputSystem.Player.Move.canceled += CanselMove;
        _inputSystem.Player.Interact.started += OnInteract;
        _inputSystem.Enable();

        _inGameManager = FindAnyObjectByType<InGameManager>();

        //アニメーションを設定
        if (gameObject.CompareTag("MainPlayer"))
        {
            FindAnyObjectByType<PlayerAnimationManager>()._mainPlayerAnimator = _animator;
        }
        else
        {
            FindAnyObjectByType<PlayerAnimationManager>()._subPlayerAnimator = _animator;
        }
    }

    private void Update()
    {
        if (_moving && _onGround && _canMove)
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
        if (MoveMode)
            MoveDirection = new Vector3(input.x, 0, input.y) * _moveSpeed;
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
            _animator.SetFloat(Speed, Mathf.Max(magnitude * _walkBaseAnimationSpeed, 0.5f));
        }
        else
        {
            _animator.SetBool(Run, true);
            _animator.SetFloat(Speed, magnitude * _runBaseAnimationSpeed);
        }
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        Physics.OverlapSphereNonAlloc(transform.position + transform.forward * 1.05f + Vector3.up, 1.2f, _interactColliders);
        GameObject interactObject = null;

        bool check = false;
        foreach (var col in _interactColliders)
        {
            if (col == null)
                break;
            if (col.gameObject.CompareTag("Interactive"))
            {
                interactObject = col.gameObject;
                check = true;
            }
        }
        if (interactObject == null) return;
        
        _canMove = false;
        Vector3 movePosition = interactObject.transform.position + interactObject.transform.forward * -0.5f;
        movePosition.y = transform.position.y;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(movePosition, 0.5f));
        sequence.Append(transform.DORotate(movePosition - transform.position, 0.2f));
        sequence.OnComplete(() =>
        {
            _animator.SetTrigger(UseLeverR);
            StartCoroutine(DelayAction(5f, () => _canMove = true));
            StartCoroutine(TouchLever(interactObject));
        });
        sequence.Play();
    }

    IEnumerator TouchLever(GameObject LeverObject)
    {
        var I = LeverObject.GetComponent<InteractObjectBase>();
        yield return new WaitForSeconds(1.7f);
        I.Interact();
    }

    IEnumerator DelayAction(float delay, Action action = null)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    
    void Interacted()
    {
        _canMove = true;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.05f + Vector3.up, 1.2f);
    }
}