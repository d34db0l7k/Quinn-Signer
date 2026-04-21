using UnityEngine;

public class ShipRotation : MonoBehaviour
{
    public float sensitivity = 0.5f;
    public float smoothing = 5f;
    
    private float _yRotation;
    private float _currentYRotation;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            _yRotation -= mouseX * sensitivity * 100f;
        }

        _currentYRotation = Mathf.Lerp(_currentYRotation, _yRotation, Time.deltaTime * smoothing);
        transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);
    }
}