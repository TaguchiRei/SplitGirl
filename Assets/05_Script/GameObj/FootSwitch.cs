using System;
using UnityEngine;

public class FootSwitch : InteractObjectBase
{
    public override void Interact()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y-0.3f, transform.position.z);
        interactObject.Interact();
    }

    private void CancelInteract()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y+0.3f, transform.position.z);
        interactObject.Cancel();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("MainPlayer")|| other.gameObject.CompareTag("SubPlayer"))
        {
            Interact();
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
