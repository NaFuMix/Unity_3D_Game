
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    public Camera camera1;
    public Camera camera2;

    [Header("Player References")]
    public Transform player1;
    public Transform player2;

    [Header("Camera Angle")]
    [Range(30f, 80f)]
    public float cameraPitch = 55f;

    public float followDistance = 24f;
    public float followHeight = 0.5f;

    [Header("Camera Movement")]
    public float followSmoothness = 8f;
    public float rotationSmoothness = 8f;

    [Header("Zoom Settings")]
    public float baseZoom = 10f;
    public float zoomFactor = 0.35f;
    public float minZoom = 10f;
    public float maxZoom = 17f;
    public float splitZoom = 10f;

    [Header("Split Screen Settings")]
    public float splitDistance = 12f;
    public float mergeDistance = 9f;

    private bool isSplitScreen = false;

    private void Start()
    {
        if (camera1 == null || camera2 == null ||
            player1 == null || player2 == null)
        {
            Debug.LogError(
                "CameraManager: กรุณากำหนด Camera และ Player ใน Inspector"
            );

            enabled = false;
            return;
        }

        camera1.orthographic = true;
        camera2.orthographic = true;

        camera1.enabled = true;
        camera2.enabled = false;

        camera1.rect = new Rect(0f, 0f, 1f, 1f);
        camera1.orthographicSize = baseZoom;
        camera2.orthographicSize = splitZoom;
    }

    private void LateUpdate()
    {
        if (player1 == null || player2 == null)
            return;

        float distance = Vector3.Distance(
            new Vector3(player1.position.x, 0f, player1.position.z),
            new Vector3(player2.position.x, 0f, player2.position.z)
        );

        // ตรวจสอบการแบ่งและรวมหน้าจอ
        if (!isSplitScreen && distance >= splitDistance)
        {
            SetSplitScreen(true);
        }
        else if (isSplitScreen && distance <= mergeDistance)
        {
            SetSplitScreen(false);
        }

        if (isSplitScreen)
        {
            // แต่ละกล้องติดตามผู้เล่นของตัวเอง
            FollowCamera(camera1, player1, splitZoom);
            FollowCamera(camera2, player2, splitZoom);
        }
        else
        {
            // คำนวณจุดกึ่งกลางระหว่างผู้เล่น
            Vector3 midpoint =
                (player1.position + player2.position) / 2f;

            midpoint.y = followHeight;

            // ซูมออกตามระยะห่าง
            float targetZoom = Mathf.Clamp(
                baseZoom + distance * zoomFactor,
                minZoom,
                maxZoom
            );

            FollowCamera(camera1, midpoint, targetZoom);
        }
    }

    private void SetSplitScreen(bool split)
    {
        isSplitScreen = split;

        if (split)
        {
            // แบ่งหน้าจอซ้ายและขวา
            camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
            camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);

            camera1.orthographicSize = splitZoom;
            camera2.orthographicSize = splitZoom;

            camera2.enabled = true;
        }
        else
        {
            // กลับมาใช้กล้องเดียว
            camera1.rect = new Rect(0f, 0f, 1f, 1f);

            camera2.enabled = false;
        }
    }

    private void FollowCamera(
        Camera cam,
        Transform target,
        float targetZoom)
    {
        FollowCamera(cam, target.position, targetZoom);
    }

    private void FollowCamera(
        Camera cam,
        Vector3 targetPosition,
        float targetZoom)
    {
        Vector3 offset = Quaternion.Euler(
            cameraPitch, 0f, 0f
        ) * new Vector3(0f, 0f, -followDistance);

        Vector3 desiredPosition = targetPosition + offset;

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            desiredPosition,
            followSmoothness * Time.deltaTime
        );

        Quaternion desiredRotation = Quaternion.LookRotation(
            targetPosition - cam.transform.position,
            Vector3.up
        );

        cam.transform.rotation = Quaternion.Slerp(
            cam.transform.rotation,
            desiredRotation,
            rotationSmoothness * Time.deltaTime
        );

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            followSmoothness * Time.deltaTime
        );
    }
}
