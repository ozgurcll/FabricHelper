using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace FabricHelper
{
    /// <summary>
    /// POI noktası üzerinde 3D dünyada görünen etiket.
    /// Kameraya doğru döner (billboard efekti) ve odanın adını, kategorisini gösterir.
    /// </summary>
    public class POILabel : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Etiket metnini gösteren TMP bileşeni")]
        [SerializeField] private TextMeshProUGUI _nameText;

        [Tooltip("Kategori renk göstergesi")]
        [SerializeField] private Image _categoryIndicator;

        [Tooltip("Arka plan paneli")]
        [SerializeField] private Image _background;

        [Tooltip("Alt ok işareti (isteğe bağlı)")]
        [SerializeField] private Image _arrow;

        [Tooltip("Pin ikonu (isteğe bağlı)")]
        [SerializeField] private Image _pinIcon;

        [Header("Ayarlar")]
        [Tooltip("Kameraya doğru dönme (billboard)")]
        [SerializeField] private bool _billboardEnabled = true;

        [Tooltip("Minimum görünürlük mesafesi")]
        [SerializeField] private float _minVisibleDistance = 5f;

        [Tooltip("Maksimum görünürlük mesafesi")]
        [SerializeField] private float _maxVisibleDistance = 80f;

        [Tooltip("Mesafeye göre ölçek ayarı")]
        [SerializeField] private bool _scaleWithDistance = true;

        [Tooltip("Seçildiğinde büyüme miktarı")]
        [SerializeField] private float _selectedScaleMultiplier = 1.3f;

        private Camera _mainCamera;
        private POIMarker _marker;
        private CanvasGroup _canvasGroup;
        private Vector3 _baseScale;
        private bool _isSelected;
        private float _targetAlpha = 1f;
        private float _currentAlpha = 1f;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _marker = GetComponentInParent<POIMarker>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _baseScale = transform.localScale;
        }

        private void Start()
        {
            if (_marker != null && _marker.POIData != null)
            {
                SetupFromPOIData(_marker.POIData);

                // Seçim event'lerine abone ol
                _marker.OnSelected += OnMarkerSelected;
                _marker.OnDeselected += OnMarkerDeselected;
            }
        }

        private void OnDestroy()
        {
            if (_marker != null)
            {
                _marker.OnSelected -= OnMarkerSelected;
                _marker.OnDeselected -= OnMarkerDeselected;
            }
        }

        /// <summary>
        /// POIData bilgisinden etiket içeriğini oluşturur.
        /// </summary>
        public void SetupFromPOIData(POIData data)
        {
            if (_nameText != null)
            {
                _nameText.text = data.poiName;
            }

            Color catColor = data.GetCategoryColor();

            if (_categoryIndicator != null)
            {
                _categoryIndicator.color = catColor;
            }

            if (_pinIcon != null)
            {
                _pinIcon.color = catColor;
            }

            // Arka plan rengini hafifçe kategori rengine boyama
            if (_background != null)
            {
                Color bgColor = Color.Lerp(Color.white, catColor, 0.08f);
                bgColor.a = 0.95f;
                _background.color = bgColor;
            }
        }

        /// <summary>
        /// Etiket metnini elle ayarlar.
        /// </summary>
        public void SetLabel(string text)
        {
            if (_nameText != null)
            {
                _nameText.text = text;
            }
        }

        /// <summary>
        /// Kategori renk göstergesini ayarlar.
        /// </summary>
        public void SetCategoryColor(Color color)
        {
            if (_categoryIndicator != null)
            {
                _categoryIndicator.color = color;
            }
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            // Billboard — Canvas'ın ön yüzünü kameraya döndür
            // World Space Canvas, -Z yönünde render eder, bu yüzden kameranın
            // tam tersi yöne bakmalıyız
            if (_billboardEnabled)
            {
                Vector3 dirFromCamera = transform.position - _mainCamera.transform.position;
                dirFromCamera.y = 0;
                if (dirFromCamera.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(dirFromCamera.normalized, Vector3.up);
                }
            }

            // Mesafe bazlı görünürlük ve ölçek
            float distance = Vector3.Distance(_mainCamera.transform.position, transform.position);

            // Alpha hesapla
            if (distance < _minVisibleDistance)
            {
                _targetAlpha = 0f;
            }
            else if (distance > _maxVisibleDistance)
            {
                _targetAlpha = 0f;
            }
            else
            {
                // Kenarlarında yumuşak geçiş
                float fadeInRange = _minVisibleDistance + 3f;
                float fadeOutRange = _maxVisibleDistance - 10f;

                if (distance < fadeInRange)
                {
                    _targetAlpha = Mathf.InverseLerp(_minVisibleDistance, fadeInRange, distance);
                }
                else if (distance > fadeOutRange)
                {
                    _targetAlpha = Mathf.InverseLerp(_maxVisibleDistance, fadeOutRange, distance);
                }
                else
                {
                    _targetAlpha = 1f;
                }
            }

            // Yumuşak alpha geçişi
            _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, Time.deltaTime * 5f);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _currentAlpha;
            }

            // Mesafe bazlı ölçekleme
            if (_scaleWithDistance)
            {
                float scaleFactor = Mathf.Clamp(distance * 0.03f, 0.5f, 2f);
                float selectedMultiplier = _isSelected ? _selectedScaleMultiplier : 1f;
                transform.localScale = _baseScale * scaleFactor * selectedMultiplier;
            }
            else if (_isSelected)
            {
                transform.localScale = _baseScale * _selectedScaleMultiplier;
            }
        }

        private void OnMarkerSelected(POIMarker marker)
        {
            _isSelected = true;

            // Seçildiğinde arka plan rengini vurgula
            if (_background != null)
            {
                Color catColor = _marker.POIData != null ? _marker.POIData.GetCategoryColor() : Color.white;
                Color selectedBg = Color.Lerp(Color.white, catColor, 0.25f);
                selectedBg.a = 1f;
                _background.color = selectedBg;
            }
        }

        private void OnMarkerDeselected(POIMarker marker)
        {
            _isSelected = false;

            // Orijinal renge dön
            if (_marker != null && _marker.POIData != null)
            {
                Color catColor = _marker.POIData.GetCategoryColor();
                Color bgColor = Color.Lerp(Color.white, catColor, 0.08f);
                bgColor.a = 0.95f;
                if (_background != null) _background.color = bgColor;
            }
        }
    }
}
