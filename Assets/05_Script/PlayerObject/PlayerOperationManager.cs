using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOperationManager : MonoBehaviour
{
    
    [SerializeField] private InGameManager _inGameManager;
    private InputSystem_Actions _inputSystem;
    
    public Animator _mainPlayerAnimator;
    public Animator _subPlayerAnimator;
        
    /// <summary>移動に使う</summary>
    private Action _moveAction;
    private Action _cancelMoveAction;
    
    private Action _mainInteractAction;
    private Action _subInteractAction;
    
    
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
    
    public void SetMoveAction(Action moveAction)
    {
        _moveAction += moveAction;
    }

    public void SetInteractAction(Action interactAction)
    {
        
    }
}
