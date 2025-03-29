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
    

    private void OnCollisionStay(Collision other)
    {
        Vector3 pos = Vector3.zero;
        bool main = false;
        foreach (var obj in other.contacts)
        {
            if (obj.thisCollider.gameObject.CompareTag("MainPlayer"))
            {
                pos = obj.point;
                main = true;
            }
            else if(obj.thisCollider.gameObject.CompareTag("SubPlayer"))
            {
                pos = obj.point;
                main = false;
            }
        }

        if (pos != Vector3.zero)
        {
            if (InGameManager.Instance.CheckInScreen(main, pos))
            {
                Interact();
            }
            else
            {
                CancelInteract();
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("MainPlayer") || other.gameObject.CompareTag("SubPlayer"))
        {
            CancelInteract();
        }
    }
}