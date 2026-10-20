
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("따라갈 플레이어")]
    public Transform target;

    [Header("카메라 위치 설정")]
    public Vector3 offset = new Vector3(0f, 8f, 8f);

    [Header("카메라 회전 설정")]
    public Vector3 cameraRotation = new Vector3(40f, 180f, 0f);

    [Header("카메라 추적 속도")]
    [Range(1f, 20f)]
    public float followSpeed = 5f;

    [Header("카메라 화면 크기")]
    [Range(3f, 12f)]
    public float cameraSize = 5.5f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        // 쿼터뷰 카메라 설정
        cam.orthographic = true;
        cam.orthographicSize = cameraSize;

        transform.rotation = Quaternion.Euler(cameraRotation);

        // 시작 시 플레이어 위치로 카메라 이동
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // 플레이어를 따라갈 목표 위치
        Vector3 desiredPosition = target.position + offset;

        // 부드러운 카메라 이동
        float t = 1f - Mathf.Exp(
            -followSpeed * Time.deltaTime
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            t
        );

        // 카메라 회전 고정
        transform.rotation = Quaternion.Euler(cameraRotation);

        // 화면 크기 적용
        cam.orthographicSize = cameraSize;
    }
}