using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse")]
public class MouseCameraControl : MonoBehaviour
{
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 5F;
    public float sensitivityY = 5F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationY = 0F;

    bool isEnabled = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isEnabled = true;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1)) {
            isEnabled = false;
            Cursor.visible = true;
        }
            

        if (axes == RotationAxes.MouseXAndY && isEnabled)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if (axes == RotationAxes.MouseX && isEnabled)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
        }
        else if (isEnabled)
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }
    }

    void Start()
    {

    }




}