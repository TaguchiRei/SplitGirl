using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    [SerializeField] float _jumpForce;
    
    [SerializeField] Rigidbody _rigidBody;

    private bool _onGround;

    private void Start()
    {
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
#endif
    }

    private void Update()
    {
        //最終的にはスマホでリリースするので仮のパソコン用スクリプト
#if UNITY_EDITOR
        //unityEditor上ではキーボード操作できるようにする
        if (Input.GetKeyDown(KeyCode.Space) && _onGround)
        {
            //ジャンプの処理
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _onGround = false;
        }
        //視点を左右に回転させる
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + Input.GetAxisRaw("Mouse X"), 0);
        //移動する
        Vector3 move = transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized);
        _rigidBody.linearVelocity = new Vector3(move.x * _moveSpeed, _rigidBody.linearVelocity.y, move.z * _moveSpeed);
#endif
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _onGround = true;
        }
    }
}
