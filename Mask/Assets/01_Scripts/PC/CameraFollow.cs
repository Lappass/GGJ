using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class CameraFollow: MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smooth = 12f;

    [Header("Bounds")]
    [SerializeField] private BoxCollider2D bounds;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindBounds();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindBounds();
    }

    private void FindBounds()
    {
        GameObject boundsObj = GameObject.Find("CameraBounds");
        if (boundsObj != null)
        {
            bounds = boundsObj.GetComponent<BoxCollider2D>();
        }
        else
        {
            bounds = null;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        desired.z = offset.z;

        //ƽ��
        if (smooth > 0f)
            desired = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);

        //�߽�
        if (bounds != null && cam.orthographic)
        {
            Bounds b = bounds.bounds;
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            float minX = b.min.x + halfW;
            float maxX = b.max.x - halfW;
            float minY = b.min.y + halfH;
            float maxY = b.max.y - halfH;
            if (minX > maxX) desired.x = b.center.x;
            else desired.x = Mathf.Clamp(desired.x, minX, maxX);
            if (minY > maxY) desired.y = b.center.y;
            else desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        transform.position = desired;
    }
}
