using UnityEngine;
using System;

namespace FabricHelper
{
    /// <summary>
    /// Tüm UI panellerini koordine eden merkezi yönetici.
    /// Paneller arası geçişleri, açma/kapama işlemlerini ve
    /// diğer sistemlerden gelen olaylara UI tepkilerini yönetir.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panel Referansları")]
        [SerializeField] private SearchPanel searchPanel;
        [SerializeField] private InfoPanel infoPanel;
        [SerializeField] private RoutePanel routePanel;
        [SerializeField] private FloorSwitcherUI floorSwitcherUI;
        [SerializeField] private FactorySwitcherUI factorySwitcherUI;

        [Header("Ayarlar")]
        [SerializeField] private GameObject headerBar;
        [SerializeField] private GameObject controlButtons;

        /// <summary>Herhangi bir panel açık mı?</summary>
        public bool IsAnyPanelOpen => _isSearchOpen || _isInfoOpen || _isRouteOpen;

        private bool _isSearchOpen;
        private bool _isInfoOpen;
        private bool _isRouteOpen;

        // Olaylar
        public event Action OnAllPanelsClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Başlangıçta tüm panelleri kapat
            CloseAllPanels();

            // Diğer sistemlerden gelen olaylara abone ol
            if (POIManager.Instance != null)
            {
                POIManager.Instance.OnPOISelected += HandlePOISelected;
                POIManager.Instance.OnPOIDeselected += HandlePOIDeselected;
            }

            if (NavigationManager.Instance != null)
            {
                NavigationManager.Instance.OnRouteCalculated += HandleRouteCalculated;
                NavigationManager.Instance.OnRouteCleared += HandleRouteCleared;
                NavigationManager.Instance.OnRouteFailed += HandleRouteFailed;
            }
        }

        private void OnDestroy()
        {
            if (POIManager.Instance != null)
            {
                POIManager.Instance.OnPOISelected -= HandlePOISelected;
                POIManager.Instance.OnPOIDeselected -= HandlePOIDeselected;
            }

            if (NavigationManager.Instance != null)
            {
                NavigationManager.Instance.OnRouteCalculated -= HandleRouteCalculated;
                NavigationManager.Instance.OnRouteCleared -= HandleRouteCleared;
                NavigationManager.Instance.OnRouteFailed -= HandleRouteFailed;
            }
        }

        #region Panel Kontrolleri

        /// <summary>Arama panelini aç/kapat (toggle).</summary>
        public void ToggleSearchPanel()
        {
            if (_isSearchOpen)
                CloseSearchPanel();
            else
                OpenSearchPanel();
        }

        public void OpenSearchPanel()
        {
            _isSearchOpen = true;
            if (searchPanel != null) searchPanel.Show();
        }

        public void CloseSearchPanel()
        {
            _isSearchOpen = false;
            if (searchPanel != null) searchPanel.Hide();
        }

        public void OpenInfoPanel(POIMarker marker)
        {
            _isInfoOpen = true;
            if (infoPanel != null) infoPanel.Show(marker);
        }

        public void CloseInfoPanel()
        {
            _isInfoOpen = false;
            if (infoPanel != null) infoPanel.Hide();
        }

        public void OpenRoutePanel(Vector3[] routeCorners)
        {
            _isRouteOpen = true;
            if (routePanel != null) routePanel.Show(routeCorners);
        }

        public void CloseRoutePanel()
        {
            _isRouteOpen = false;
            if (routePanel != null) routePanel.Hide();
        }

        /// <summary>Tüm panelleri kapatır.</summary>
        public void CloseAllPanels()
        {
            CloseSearchPanel();
            CloseInfoPanel();
            CloseRoutePanel();
            OnAllPanelsClosed?.Invoke();
        }

        #endregion

        #region Olay İşleyicileri

        private void HandlePOISelected(POIMarker marker)
        {
            // POI seçildiğinde bilgi panelini aç
            OpenInfoPanel(marker);

            // Kamerayı odakla
            if (IsometricCameraController.Instance != null)
            {
                IsometricCameraController.Instance.FocusOn(marker.GetWorldPosition());
            }
        }

        private void HandlePOIDeselected()
        {
            CloseInfoPanel();
        }

        private void HandleRouteCalculated(Vector3[] corners)
        {
            OpenRoutePanel(corners);
        }

        private void HandleRouteCleared()
        {
            CloseRoutePanel();
        }

        private void HandleRouteFailed(string errorMessage)
        {
            Debug.LogWarning($"[UIManager] Rota hesaplanamadı: {errorMessage}");
            // TODO: Kullanıcıya hata mesajı göster (toast notification)
        }

        #endregion

        #region Yardımcı Metodlar

        /// <summary>Input'u kilitlemek gerekip gerekmediğini döner.</summary>
        public bool ShouldBlockInput()
        {
            return _isSearchOpen;
        }

        #endregion
    }
}
