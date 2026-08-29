using UnityEngine;
using UnityEngine.EventSystems;

namespace FabricHelper
{
    /// <summary>
    /// Mouse ve dokunmatik girişleri işleyen sınıf. Kamera kontrolcüsüne ve POI sistemine komut gönderir.
    /// </summary>
    public class TouchInputHandler : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private IsometricCameraController cameraController;
        [SerializeField] private Camera mainCamera;

        [Header("Ayarlar")]
        [SerializeField] private LayerMask poiLayerMask;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float tapThreshold = 10f;
        [SerializeField] private float doubleTapTime = 0.3f;
        [SerializeField] private bool isPlacingUserLocation = false;

        private bool isInputEnabled = true;

        // Mouse takibi
        private Vector3 lastMousePos;
        private Vector3 mouseClickStartPos;
        private bool isDraggingMouse = false;

        // Dokunmatik takibi
        private float initialPinchDistance;
        private float initialPinchAngle;

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!isInputEnabled) return;

            // UI'a tıklanıyorsa işlemi iptal et
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Mobil cihazlarda dokunmatik kontrol
                if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;
                
                if (Input.touchCount == 0) // Sadece mouse ise ve UI üstündeyse
                    return;
            }

            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleMouseInput()
        {
            // Zoom (Scroll)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                cameraController.Zoom(scroll * 10f);
            }

            // Pan (Sol Tık Sürükle)
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePos = Input.mousePosition;
                mouseClickStartPos = Input.mousePosition;
                isDraggingMouse = false;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                if (Vector3.Distance(Input.mousePosition, mouseClickStartPos) > tapThreshold)
                {
                    isDraggingMouse = true;
                }

                if (isDraggingMouse && delta.magnitude > 0)
                {
                    cameraController.Pan(new Vector3(delta.x, delta.y, 0) * Time.deltaTime);
                }
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (!isDraggingMouse && Vector3.Distance(Input.mousePosition, mouseClickStartPos) <= tapThreshold)
                {
                    HandleTap(Input.mousePosition);
                }
                isDraggingMouse = false;
            }

            // Rotate (Sağ Tık Sürükle veya Orta Tuş)
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                cameraController.Rotate(delta.x * Time.deltaTime, delta.y * Time.deltaTime);
                lastMousePos = Input.mousePosition;
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        mouseClickStartPos = touch.position;
                        isDraggingMouse = false;
                        break;
                    case TouchPhase.Moved:
                        if (Vector3.Distance(touch.position, mouseClickStartPos) > tapThreshold)
                        {
                            isDraggingMouse = true;
                            cameraController.Pan(new Vector3(touch.deltaPosition.x, touch.deltaPosition.y, 0) * Time.deltaTime * 5f);
                        }
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (!isDraggingMouse && Vector3.Distance(touch.position, mouseClickStartPos) <= tapThreshold)
                        {
                            HandleTap(touch.position);
                        }
                        isDraggingMouse = false;
                        break;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                    initialPinchAngle = Mathf.Atan2(touch1.position.y - touch0.position.y, touch1.position.x - touch0.position.x) * Mathf.Rad2Deg;
                }
                else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    // Pinch to Zoom
                    float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                    float distanceDelta = currentDistance - initialPinchDistance;
                    if (Mathf.Abs(distanceDelta) > 5f)
                    {
                        cameraController.Zoom(distanceDelta * 0.01f);
                        initialPinchDistance = currentDistance;
                    }

                    // İki parmakla döndürme
                    float currentAngle = Mathf.Atan2(touch1.position.y - touch0.position.y, touch1.position.x - touch0.position.x) * Mathf.Rad2Deg;
                    float angleDelta = Mathf.DeltaAngle(initialPinchAngle, currentAngle);
                    
                    if (Mathf.Abs(angleDelta) > 1f)
                    {
                        cameraController.Rotate(-angleDelta * 0.1f, 0);
                        initialPinchAngle = currentAngle;
                    }
                }
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (isPlacingUserLocation)
            {
                if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
                {
                    var userMarker = FindAnyObjectByType<UserLocationMarker>();
                    if (userMarker != null)
                    {
                        userMarker.SetLocation(hit.point);
                    }
                    else
                    {
                        Debug.LogWarning("Kullanıcı konum işaretçisi (UserLocationMarker) sahnede bulunamadı.");
                    }
                    SetPlacingUserLocationMode(false);
                }
            }
            else
            {
                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    // POIMarker scriptini bul (objenin kendisinde veya parent'ta)
                    var marker = hit.collider.GetComponentInParent<POIMarker>();
                    if (marker != null)
                    {
                        if (POIManager.Instance != null)
                        {
                            POIManager.Instance.SelectPOI(marker);
                        }
                    }
                    else
                    {
                        // Boş alana tıklandı — seçimi kaldır
                        if (POIManager.Instance != null)
                        {
                            POIManager.Instance.DeselectAll();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Kullanıcı konumu belirleme modunu açar veya kapatır.
        /// </summary>
        public void SetPlacingUserLocationMode(bool enabled)
        {
            isPlacingUserLocation = enabled;
            Debug.Log($"Konum yerleştirme modu: {(enabled ? "Açık" : "Kapalı")}");
        }

        /// <summary>
        /// Girdiyi etkinleştirir veya devre dışı bırakır.
        /// </summary>
        public void EnableInput(bool enabled)
        {
            isInputEnabled = enabled;
        }
    }
}
