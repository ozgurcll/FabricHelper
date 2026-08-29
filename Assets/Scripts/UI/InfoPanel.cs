using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FabricHelper
{
    /// <summary>
    /// POI bilgi kartı paneli. Bir POI seçildiğinde sağ tarafta açılır.
    /// İsim, kategori, kapasite, çalışma saatleri, detay ve görsel gösterir.
    /// Referans görseldeki YEMEKHANE kartı benzeri tasarım.
    /// </summary>
    public class InfoPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Başlık Alanı")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image titleBackground;
        [SerializeField] private Image categoryIcon;

        [Header("Bilgi Alanları")]
        [SerializeField] private TMP_Text capacityText;
        [SerializeField] private GameObject capacityRow;
        [SerializeField] private TMP_Text workingHoursText;
        [SerializeField] private GameObject workingHoursRow;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text floorText;

        [Header("Görsel")]
        [SerializeField] private Image poiPhoto;
        [SerializeField] private Sprite placeholderPhoto;

        [Header("Butonlar")]
        [SerializeField] private Button navigateButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button detailsButton;
        [SerializeField] private TMP_Text navigateButtonText;

        private POIMarker _currentMarker;

        private void Start()
        {
            // Buton olayları
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            if (navigateButton != null)
                navigateButton.onClick.AddListener(OnNavigateClicked);

            if (detailsButton != null)
                detailsButton.onClick.AddListener(OnDetailsClicked);

            // Başlangıçta gizle
            Hide();
        }

        /// <summary>Bilgi panelini belirtilen POI marker ile gösterir.</summary>
        public void Show(POIMarker marker)
        {
            if (marker == null || marker.POIData == null)
            {
                Debug.LogWarning("[InfoPanel] Marker veya POIData boş!");
                return;
            }

            _currentMarker = marker;
            PopulateData(marker.POIData);

            if (panelRoot != null) panelRoot.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>Bilgi panelini gizler.</summary>
        public void Hide()
        {
            _currentMarker = null;

            if (panelRoot != null) panelRoot.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>Panel verilerini doldurur.</summary>
        private void PopulateData(POIData data)
        {
            // Başlık
            if (titleText != null)
                titleText.text = data.poiName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));

            // Başlık arka plan rengi
            if (titleBackground != null)
            {
                Color catColor = data.GetCategoryColor();
                titleBackground.color = catColor;
            }

            // Kategori ikonu
            if (categoryIcon != null && data.icon != null)
                categoryIcon.sprite = data.icon;

            // Kapasite
            if (capacityRow != null)
            {
                bool hasCapacity = data.capacity > 0;
                capacityRow.SetActive(hasCapacity);
                if (hasCapacity && capacityText != null)
                    capacityText.text = $"{data.capacity} kişi";
            }

            // Çalışma saatleri
            if (workingHoursRow != null)
            {
                bool hasHours = !string.IsNullOrEmpty(data.workingHours);
                workingHoursRow.SetActive(hasHours);
                if (hasHours && workingHoursText != null)
                    workingHoursText.text = data.workingHours;
            }

            // Detay metni
            if (detailText != null)
            {
                detailText.text = !string.IsNullOrEmpty(data.detailText)
                    ? data.detailText
                    : "";
            }

            // Açıklama
            if (descriptionText != null)
            {
                descriptionText.text = !string.IsNullOrEmpty(data.description)
                    ? data.description
                    : "";
            }

            // Kat bilgisi
            if (floorText != null)
            {
                string floorName = FloorManager.Instance != null
                    ? FloorManager.Instance.GetFloorName(data.floorIndex)
                    : $"Kat {data.floorIndex}";
                floorText.text = floorName;
            }

            // Fotoğraf
            if (poiPhoto != null)
            {
                poiPhoto.sprite = data.photo != null ? data.photo : placeholderPhoto;
                poiPhoto.gameObject.SetActive(poiPhoto.sprite != null);
            }

            // Navigasyon buton metni
            if (navigateButtonText != null)
                navigateButtonText.text = "Buraya Git";
        }

        #region Buton Olayları

        private void OnCloseClicked()
        {
            POIManager.Instance?.DeselectAll();
            UIManager.Instance?.CloseInfoPanel();
        }

        private void OnNavigateClicked()
        {
            if (_currentMarker == null) return;

            // Kullanıcının konumundan bu POI'ya navigasyon başlat
            if (NavigationManager.Instance != null)
            {
                bool success = NavigationManager.Instance.CalculateRouteFromUserLocation(_currentMarker);
                if (!success)
                {
                    Debug.LogWarning("[InfoPanel] Rota hesaplanamadı. Kullanıcı konumu ayarlanmış mı?");
                }
            }
        }

        private void OnDetailsClicked()
        {
            // Detay sayfasına geçiş veya daha fazla bilgi gösterimi
            // Şimdilik sadece açıklama metnini toggle eder
            if (descriptionText != null)
            {
                descriptionText.gameObject.SetActive(!descriptionText.gameObject.activeSelf);
            }
        }

        #endregion
    }
}
