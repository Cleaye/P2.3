using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float
        Sensitivity = 1.0f,
        Speed       = 5.0f;

    private float rotationX, rotationY;
    private Quaternion originalRotation;

    private void Start() {
        ToggleCursorState();
        originalRotation = transform.localRotation;
    }

    public bool MouseLook
        => Cursor.lockState == CursorLockMode.Locked;

    private void ToggleCursorState()
        => Cursor.lockState
            = MouseLook
            ? CursorLockMode.None
            : CursorLockMode.Locked;

    void Update() {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            ToggleCursorState();

        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            movement += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            movement += Vector3.back;
        if (Input.GetKey(KeyCode.A))
            movement += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            movement += Vector3.right;
        if (Input.GetKey(KeyCode.Space))
            movement += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl))
            movement += Vector3.down;
        if (movement != Vector3.zero)
            movement.Normalize();
        movement = transform.TransformDirection(movement);
        transform.position += movement * Speed * Time.deltaTime;

        if (MouseLook) {
            rotationX += Input.GetAxis("Mouse X") * Sensitivity;
            rotationY += Input.GetAxis("Mouse Y") * Sensitivity;
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
    }

}
