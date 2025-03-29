using System;
using Unity.VisualScripting;
using UnityEngine;

public class FootSwitch : InteractObjectBase
{
    public override void Interact()
    {
        interactObject.Interact();
    }

    private void CancelInteract()
    {
        interactObject.Cancel();
    }

    private void OnCollisionEnter(Collision other)
    {
        Vector3 pos = other.contacts[0].point;
        bool main = other.gameObject.CompareTag("MainPlayer");
        Debug.Log(other.contacts.Length);
        if (InGameManager.Instance.CheckInScreen(main, pos))
        {
            Interact();
        }
        else
        {
            CancelInteract();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        Vector3 pos = other.contacts[0].point;
        bool main = other.gameObject.CompareTag("MainPlayer");
        if (InGameManager.Instance.CheckInScreen(main, pos))
        {
            Interact();
        }
        else
        {
            CancelInteract();
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