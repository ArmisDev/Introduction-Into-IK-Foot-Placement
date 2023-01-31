using UnityEngine;

public class WeaponSway_scr : MonoBehaviour
{

    [SerializeField] private float smoothingTime = 1.0f;
    [SerializeField] private float swayMultiplier = 1.0f;

    void HandleWeaponSway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayMultiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothingTime * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        HandleWeaponSway();
    }
}
