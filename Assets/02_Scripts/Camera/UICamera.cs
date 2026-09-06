using UnityEngine;

public class UICamera : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        transform.rotation = mainCam.transform.rotation;
    }
}
