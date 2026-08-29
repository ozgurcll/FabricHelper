using UnityEngine;
using System.Collections;

namespace FabricHelper
{
    /// <summary>
    /// Fabrika 3D modelini izometrik/kuşbakışı açıyla incelemek için kullanılan kamera kontrolcüsü.
    /// </summary>
    public class IsometricCameraController : MonoBehaviour
    {
        public static IsometricCameraController Instance { get; private set; }
        [Header("Hedef ve Sınırlar")]
        [Tooltip("Etrafında dönülecek merkez hedef")]
        [SerializeField] private Transform target;
        [Tooltip("Kaydırma (Pan) sınırları")]
        [SerializeField] private Rect panBounds = new Rect(-50, -50, 100, 100);

        [Header("Hız Ayarları")]
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float panSpeed = 0.5f;
        [SerializeField] private float smoothTime = 0.15f;

        [Header("Limitler")]
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 50f;
        [SerializeField] private float minPitch = 20f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Mevcut Durum")]
        [SerializeField] private float currentDistance = 20f;
        [SerializeField] private float currentYaw = 45f;
        [SerializeField] private float currentPitch = 45f;
        [SerializeField] private Vector3 panOffset;

        private Vector3 currentVelocity;
        private Camera cam;

        private Coroutine focusCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            // Başlangıç değerleri
            currentYaw = 45f;
            currentPitch = 45f;
            UpdateCameraPosition(true);
        }

        private void LateUpdate()
        {
            if (focusCoroutine == null)
            {
                UpdateCameraPosition(false);
            }
        }

        /// <summary>
        /// Kamerayı döndürür.
        /// </summary>
        public void Rotate(float deltaX, float deltaY)
        {
            currentYaw += deltaX * rotationSpeed;
            currentPitch -= deltaY * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }

        /// <summary>
        /// Kamerayı yakınlaştırıp uzaklaştırır.
        /// </summary>
        public void Zoom(float delta)
        {
            currentDistance -= delta * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minZoom, maxZoom);
            
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = currentDistance;
            }
        }

        /// <summary>
        /// Kamerayı kaydırır.
        /// </summary>
        public void Pan(Vector3 delta)
        {
            if (cam != null)
            {
                // Pan yönünü kameranın baktığı yöne göre ayarla
                Vector3 right = cam.transform.right;
                Vector3 forward = cam.transform.up; // İzometrikte yukarı yönde kaydırma ileri gibi etki eder
                right.y = 0;
                forward.y = 0;
                right.Normalize();
                forward.Normalize();

                panOffset += (right * -delta.x + forward * -delta.y) * panSpeed;

                // Sınırla
                panOffset.x = Mathf.Clamp(panOffset.x, panBounds.xMin, panBounds.xMax);
                panOffset.z = Mathf.Clamp(panOffset.z, panBounds.yMin, panBounds.yMax);
            }
        }

        /// <summary>
        /// Belirtilen hedefe kamerayı odaklar.
        /// </summary>
        public void FocusOn(Vector3 worldPosition, float duration = 0.5f)
        {
            if (focusCoroutine != null) StopCoroutine(focusCoroutine);
            focusCoroutine = StartCoroutine(FocusRoutine(worldPosition, duration));
        }

        /// <summary>
        /// Kamerayı başlangıç ayarlarına döndürür.
        /// </summary>
        public void ResetView()
        {
            currentDistance = 20f;
            currentYaw = 45f;
            currentPitch = 45f;
            panOffset = Vector3.zero;
        }

        /// <summary>
        /// Orbit hedefini değiştirir.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void UpdateCameraPosition(bool instant)
        {
            if (target == null) return;

            // Küresel koordinatlardan pozisyon hesapla
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
            Vector3 direction = rotation * new Vector3(0, 0, -currentDistance);
            
            Vector3 desiredPosition = target.position + panOffset + direction;

            if (instant)
            {
                transform.position = desiredPosition;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
            }

            transform.LookAt(target.position + panOffset);
        }

        private IEnumerator FocusRoutine(Vector3 worldPosition, float duration)
        {
            Vector3 startPanOffset = panOffset;
            Vector3 targetPanOffset = worldPosition - (target != null ? target.position : Vector3.zero);
            
            // Sınırla
            targetPanOffset.x = Mathf.Clamp(targetPanOffset.x, panBounds.xMin, panBounds.xMax);
            targetPanOffset.z = Mathf.Clamp(targetPanOffset.z, panBounds.yMin, panBounds.yMax);
            targetPanOffset.y = 0;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                panOffset = Vector3.Lerp(startPanOffset, targetPanOffset, t);
                UpdateCameraPosition(false);
                yield return null;
            }

            panOffset = targetPanOffset;
            focusCoroutine = null;
        }
    }
}
