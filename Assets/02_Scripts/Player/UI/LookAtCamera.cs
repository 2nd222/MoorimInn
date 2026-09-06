using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        this.transform.forward = Camera.main.transform.forward;
    }
}
