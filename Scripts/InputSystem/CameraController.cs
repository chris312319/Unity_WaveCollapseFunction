using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private Vector3 currentMovementInput;
    private Vector2 currentRotationInput;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float rotateSpeed;
    private bool tryRotate;
    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Build.Enable();
        inputActions.Build.Move.performed += ctx => currentMovementInput = ctx.ReadValue<Vector3>();
        inputActions.Build.Move.canceled += ctx => currentMovementInput = Vector3.zero;
        inputActions.Build.Rotate.performed += ctx => currentRotationInput = ctx.ReadValue<Vector2>();
        inputActions.Build.Rotate.canceled += ctx => currentRotationInput = Vector2.zero;
        inputActions.Build.TryRotate.performed += ctx => tryRotate = true;
        inputActions.Build.TryRotate.canceled += ctx => tryRotate = false;
    }
    private void Update()
    {
        Move();
        Rotate();
    }
    private void Move()
    {
        Vector3 inputValue = currentMovementInput.x * transform.right + currentMovementInput.y * Vector3.up + currentMovementInput.z * transform.forward;
        transform.localPosition += inputValue * Time.deltaTime * speed;
    }
    private void Rotate()
    {
        if (tryRotate)
        {
            transform.rotation = Quaternion.Euler(-currentRotationInput.y * Time.deltaTime * rotateSpeed + transform.rotation.eulerAngles.x,currentRotationInput.x * Time.deltaTime * rotateSpeed + transform.rotation.eulerAngles.y,0f);
        }
    }
}
