using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOperationManager : MonoBehaviour
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
    [SerializeField] private InGameManager _inGameManager;
    private InputSystem_Actions _inputSystem;
    
    public Animator _mainPlayerAnimator;
    public Animator _subPlayerAnimator;
        
    /// <summary>移動に使う</summary>
    private Action _moveAction;
    private Action _cancelMoveAction;
    
    private Action _mainInteractAction;
    private Action _subInteractAction;
    
    private Collider[] _interactColliders = new Collider[10];
    
    private void Start()
    {
        _inputSystem = new InputSystem_Actions();
        
        _inputSystem.Enable();
    }


    void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _moveAction?.Invoke();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            _cancelMoveAction?.Invoke();
        }
    }

    void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            var mainP = _mainPlayerAnimator.gameObject;
            var subP = _subPlayerAnimator.gameObject;
            var mainPM = mainP.GetComponent<PlayerMove>();
            var subPM = subP.GetComponent<PlayerMove>();
            if (_inGameManager.cameraMode is InGameManager.CameraMode.MainOnly or InGameManager.CameraMode.MainCameraMove)
            {
                Physics.OverlapSphereNonAlloc(mainP.transform.position + mainP.transform.forward * 1.05f + Vector3.up
                    , 1.2f
                    , _interactColliders);
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
        
                mainPM._canMove = false;
                subPM._canMove = false;
                Vector3 movePosition = interactObject.transform.position + interactObject.transform.forward * -0.42f;
                movePosition.y = transform.position.y;
                Sequence sequence = DOTween.Sequence();
                sequence.Append(transform.DOMove(movePosition, 0.5f));
                sequence.Append(transform.DORotate(movePosition - transform.position, 0.2f));
                sequence.OnComplete(() =>
                {
                    _mainPlayerAnimator.SetTrigger(UseLeverR);
                    _subPlayerAnimator.SetTrigger(UseLeverR);
                    _mainPlayerAnimator.SetTrigger(UseLeverR);
                    StartCoroutine(DelayAction(5f, () =>
                    { 
                        mainPM._canMove = true;
                        subPM._canMove = true;
                    }));
                    StartCoroutine(TouchLever(interactObject));
                });
                sequence.Play();
            }
            else
            {
                
            }
        }
    }
    IEnumerator DelayAction(float delay, Action action = null)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    
    IEnumerator TouchLever(GameObject LeverObject)
    {
        var I = LeverObject.GetComponent<InteractObjectBase>();
        yield return new WaitForSeconds(1.7f);
        I.Interact();
    }
    
    public void SetMoveAction(Action moveAction)
    {
        _moveAction += moveAction;
    }

    public void SetInteractAction(Action interactAction)
    {
        
    }
}
