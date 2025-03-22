using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOperationManager : MonoBehaviour
{
    [SerializeField] private PlayerAnimationManager playerAnimationManager;
    [SerializeField] private InGameManager _inGameManager;
    private InputSystem_Actions _inputSystem;
        
    /// <summary>移動に使う</summary>
    private Action<InputAction.CallbackContext> _moveAction;
    private Action _cancelMoveAction;
    
    private Action _mainInteractAction;
    private Action _subInteractAction;

    private GameObject _mainPlayerObject;
    private GameObject _subPlayerObject;
    
    private Collider[] _interactColliders = new Collider[10];
    
    private void Start()
    {
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMoveInput;
        _inputSystem.Player.Move.canceled += OnMoveInput;
        _inputSystem.Player.Interact.performed += OnInteractInput;
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

    /// <summary>
    /// インタラクト時に起動するメソッド
    /// </summary>
    /// <param name="context"></param>
    void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            var mainPlayerObject = playerAnimationManager._mainPlayerAnimator.gameObject;
            var SubPlayerObject = playerAnimationManager._subPlayerAnimator.gameObject;
            var mainPM = mainPlayerObject.GetComponent<PlayerMove>();
            var subPM = SubPlayerObject.GetComponent<PlayerMove>();
            if (_inGameManager.cameraMode is InGameManager.CameraMode.MainOnly or InGameManager.CameraMode.MainCameraMove)
            {
                Physics.OverlapSphereNonAlloc(mainPlayerObject.transform.position + mainPlayerObject.transform.forward * 1.05f + Vector3.up
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
                if (_inGameManager.cameraMode
                    is InGameManager.CameraMode.MainOnly or InGameManager.CameraMode.MainCameraMove)
                {
                    UseLeverSequence(mainPlayerObject,interactObject, movePosition, mainPM, subPM).Play();
                }
                else
                {
                    UseLeverSequence(SubPlayerObject, interactObject, movePosition, mainPM, subPM);
                }
            }
        }
    }

    /// <summary>
    /// レバーを起動するために移動する際に使用する。
    /// </summary>
    /// <param name="moveObject">動くプレイヤーオブジェクトを入れる</param>
    /// <param name="interactObject"></param>
    /// <param name="movePosition"></param>
    /// <param name="mainPM"></param>
    /// <param name="subPM"></param>
    /// <returns></returns>
    Sequence UseLeverSequence(GameObject moveObject,GameObject interactObject, Vector3 movePosition, PlayerMove mainPM, PlayerMove subPM)
    {
        return DOTween.Sequence()
            .Append(moveObject.transform.DOMove(movePosition, 0.5f))
            .Append(moveObject.transform.DORotate(movePosition - moveObject.transform.position, 0.2f))
            .OnComplete(() =>
        {
            //----------------ここに左右を見極めるスクリプトを挟む----------------------
            //---------------------------------------------------------------------
            playerAnimationManager.UseLever();
            StartCoroutine(DelayAction(5f, () =>
            { 
                mainPM._canMove = true;
                subPM._canMove = true;
            }));
            StartCoroutine(TouchLever(interactObject));
        });
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
        
    }

    public void SetInteractAction(Action interactAction)
    {
        
    }
}
