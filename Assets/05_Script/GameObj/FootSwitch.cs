using System;
using UnityEngine;

public class FootSwitch : InteractObjectBase
{
    private Vector3 _startPos;
    void Start()
    {
        _startPos = transform.position;
    }
    public override void Interact()
    {
        transform.position = new Vector3(_startPos.x, _startPos.y - 0.3f, _startPos.z);
        interactObject.Interact();
    }

    private void CancelInteract()
    {
        transform.position = _startPos;
        interactObject.Cancel();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("MainPlayer")|| other.gameObject.CompareTag("SubPlayer"))
        {
            Interact();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("MainPlayer"))
        {
            foreach (var obj in other.contacts)
            {
                
            }
        }
        else if (other.gameObject.CompareTag("SubPlayer"))
        {
            
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("MainPlayer")|| other.gameObject.CompareTag("SubPlayer"))
        {
            CancelInteract();
        }
    }
}
