using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraTargetController : MonoBehaviour
{
    [Header("시작 설정")]
    [SerializeField] private float startOrbitRotationY = 45f;
    [SerializeField] private float startOrbitRotationX = 45f;

    [Header("카메라 이동 (WASD)")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("카메라 회전 (우클릭 드래그)")]
    [SerializeField] private float orbitSpeed = 5f;
    [SerializeField] private float minOrbitAngleY = 10f;
    [SerializeField] public float maxOrbitAngleY = 85f;

    [Header("카메라 줌 (마우스 휠)")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minOrbitDistance = 2f;
    [SerializeField] private float maxOrbitDistance = 20f;
    [SerializeField] private float orbitDistance = 10f; 

    [Header("이동 제한")]
    [SerializeField] private Collider groundCollider;

    public GameObject anchorObj;
    private Transform cameraFollowAnchor;

    private float currentOrbitRotationY = 0f;
    private float currentOrbitRotationX = 0f;
    private bool isOrbiting = false;

    void Start()
    {
        anchorObj.transform.SetParent(this.transform);
        cameraFollowAnchor = anchorObj.transform;

        CameraPositionInit();
    }

    void LateUpdate()
    {
        MoveFocusPoint();
        OrbitMovement();
        ZoomCamera(); 
        ClampToBounds(); 
        UpdateCameraFollowAnchor(); 
    }

    private void CameraPositionInit()
    {
        if (groundCollider != null)
        {
            this.transform.position = groundCollider.bounds.center;
        }

        this.currentOrbitRotationY = this.startOrbitRotationY;
        this.currentOrbitRotationX = this.startOrbitRotationX;

        UpdateCameraFollowAnchor();
    }

    private void MoveFocusPoint()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = Quaternion.Euler(0, currentOrbitRotationX, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, currentOrbitRotationX, 0) * Vector3.right;

        Vector3 moveDir = (forward * v + right * h).normalized;

        this.transform.position += moveDir * this.moveSpeed * Time.deltaTime;
    }

    private void OrbitMovement()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                this.isOrbiting = false;
                return;
            }

            this.isOrbiting = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            this.isOrbiting = false;
        }

        if (Input.GetMouseButton(1) && this.isOrbiting)
        {
            float mouseX = Input.GetAxis("Mouse X") * this.orbitSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * this.orbitSpeed;

            this.currentOrbitRotationX += mouseX;
            this.currentOrbitRotationY -= mouseY;

            this.currentOrbitRotationY = Mathf.Clamp(this.currentOrbitRotationY, this.minOrbitAngleY, this.maxOrbitAngleY);
        }
    }

    
    private void ZoomCamera()
    {
        // UI 클릭 시 줌 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            orbitDistance -= scroll * zoomSpeed * 0.5f;
            
            // 너무 가까워지거나 너무 멀어지지 않게 제한
            orbitDistance = Mathf.Clamp(orbitDistance, minOrbitDistance, maxOrbitDistance);
        }
    }
    

    private void ClampToBounds()
    {
        if (this.groundCollider == null) return;
        Bounds bounds = this.groundCollider.bounds;

        Quaternion rotation = Quaternion.Euler(this.currentOrbitRotationY, this.currentOrbitRotationX, 0f);
        Vector3 desiredAnchorPos = this.transform.position + rotation * Vector3.back * this.orbitDistance;
        
        Vector3 clampedAnchorPos = desiredAnchorPos;
        clampedAnchorPos.x = Mathf.Clamp(clampedAnchorPos.x, bounds.min.x, bounds.max.x);
        clampedAnchorPos.z = Mathf.Clamp(clampedAnchorPos.z, bounds.min.z, bounds.max.z);

        if (clampedAnchorPos != desiredAnchorPos)
        {
            this.transform.position += (clampedAnchorPos - desiredAnchorPos);
        }

        Vector3 currentPos = this.transform.position;
        currentPos.x = Mathf.Clamp(currentPos.x, bounds.min.x, bounds.max.x);
        currentPos.z = Mathf.Clamp(currentPos.z, bounds.min.z, bounds.max.z);
        this.transform.position = currentPos;
    }

    private void UpdateCameraFollowAnchor()
    {
        Quaternion rotation = Quaternion.Euler(this.currentOrbitRotationY, this.currentOrbitRotationX, 0f);
        cameraFollowAnchor.position = this.transform.position + rotation * Vector3.back * this.orbitDistance;
        cameraFollowAnchor.LookAt(this.transform.position);
    }
}