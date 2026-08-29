using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FabricHelper
{
    /// <summary>
    /// Rota hesaplama sonuç paneli.
    /// Güzergah hesaplandığında sol üstte gösterilir.
    /// Başlangıç/bitiş noktası, mesafe, süre ve rotayı başlat butonu içerir.
    /// </summary>
    public class RoutePanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Rota Bilgileri")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text startPointText;
        [SerializeField] private TMP_Text endPointText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text estimatedTimeText;

        [Header("Başlangıç/Bitiş İkonları")]
        [SerializeField] private Image startIcon;
        [SerializeField] private Image endIcon;
        [SerializeField] private Color startColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color endColor = new Color(1f, 0.4f, 0.1f);

        [Header("Butonlar")]
        [SerializeField] private Button startRouteButton;
        [SerializeField] private Button cancelRouteButton;
        [SerializeField] private TMP_Text startRouteButtonText;

        private Vector3[] _currentRouteCorners;

        private void Start()
        {
            if (startRouteButton != null)
                startRouteButton.onClick.AddListener(OnStartRouteClicked);

            if (cancelRouteButton != null)
                cancelRouteButton.onClick.AddListener(OnCancelRouteClicked);

            Hide();
        }

        /// <summary>Rota panelini gösterir ve bilgileri doldurur.</summary>
        public void Show(Vector3[] routeCorners)
        {
            _currentRouteCorners = routeCorners;

            if (panelRoot != null) panelRoot.SetActive(true);

            PopulateRouteInfo();
        }

        /// <summary>Paneli gizler.</summary>
        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            _currentRouteCorners = null;
        }

        private void PopulateRouteInfo()
        {
            if (NavigationManager.Instance == null) return;

            // Durum metni
            if (statusText != null)
                statusText.text = "Güzergah Hesaplandı";

            // Başlangıç noktası
            if (startPointText != null)
            {
                startPointText.text = "Konumunuz";
            }

            // Bitiş noktası
            if (endPointText != null)
            {
                if (POIManager.Instance != null && POIManager.Instance.CurrentSelected != null)
                {
                    var data = POIManager.Instance.CurrentSelected.POIData;
                    endPointText.text = data != null ? data.poiName : "Hedef";
                }
                else
                {
                    endPointText.text = "Hedef";
                }
            }

            // Mesafe
            if (distanceText != null)
                distanceText.text = NavigationManager.Instance.GetFormattedDistance();

            // Tahmini süre
            if (estimatedTimeText != null)
                estimatedTimeText.text = NavigationManager.Instance.GetFormattedTime();

            // İkon renkleri
            if (startIcon != null) startIcon.color = startColor;
            if (endIcon != null) endIcon.color = endColor;

            // Buton metni
            if (startRouteButtonText != null)
                startRouteButtonText.text = "Rotayı Başlat";
        }

        #region Buton Olayları

        private void OnStartRouteClicked()
        {
            // Rotayı takip etme modunu başlat
            // Şimdilik sadece kamerayı rotanın başlangıç noktasına odaklar
            if (_currentRouteCorners != null && _currentRouteCorners.Length > 0)
            {
                if (IsometricCameraController.Instance != null)
                {
                    IsometricCameraController.Instance.FocusOn(_currentRouteCorners[0]);
                }
            }
        }

        private void OnCancelRouteClicked()
        {
            NavigationManager.Instance?.ClearRoute();
            UIManager.Instance?.CloseRoutePanel();
        }

        #endregion
    }
}
