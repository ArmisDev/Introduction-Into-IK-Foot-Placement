using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook_scr : MonoBehaviour
{
    [Header("Look Parameters")]
    [SerializeField]
    private Transform mainCamera;
    [SerializeField]
    private GameObject player;

    [Header("Mouse Look Settings")]
    //Handles the current axis point of the camera.
    private float cameraPitch = 0.0f;
    //Handles the speed of the camera rotation.
    [SerializeField]
    private float mouseSensitivity;
    //A check on if the player wants to have the cursor visable
    [SerializeField] private bool cursorLock;

    //Here we will being to impliment the mouse smoothing
    [SerializeField][Range(0.0f, 0.5f)] float mouseSmoothTime = 0.1f;

    Vector2 currentMouseDelta = Vector2.zero;
    Vector2 currentMouseDeltaVelocity = Vector2.zero;

    [Header("Camera Zoom Properties")]
    public float targetFOV = 60f;
    public float lerpSpeed = 1f;

    public Camera _camera;
    public float _currentFOV;
    public float _currentFOVReset;
    private bool _isLerping = false;

    void Start()
    {
        _currentFOV = _camera.fieldOfView;
        _currentFOVReset = _camera.fieldOfView;
    }

    void Update()
    {
        HandleMouseLook();

        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch -= currentMouseDelta.y * mouseSensitivity;

        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        mainCamera.localEulerAngles = Vector3.right * cameraPitch;

        player.transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }
}
