using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace FabricHelper
{
    /// <summary>
    /// Sol taraftaki arama ve kategori listesi paneli.
    /// Kullanıcı POI'ları isim veya kategoriye göre arayabilir.
    /// Referans görseldeki sol sidebar tasarımı.
    /// </summary>
    public class SearchPanel : MonoBehaviour
    {
        [Header("Arama")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button clearSearchButton;

        [Header("Liste")]
        [SerializeField] private Transform listContent;
        [SerializeField] private GameObject categoryHeaderPrefab;
        [SerializeField] private GameObject poiListItemPrefab;

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Animator panelAnimator;

        [Header("Filtre")]
        [SerializeField] private TMP_Dropdown floorFilterDropdown;
        [SerializeField] private TMP_Dropdown categoryFilterDropdown;

        private List<GameObject> _spawnedItems = new List<GameObject>();
        private string _currentSearchQuery = "";
        private int _selectedFloorFilter = -1; // -1 = tümü
        private POICategory? _selectedCategoryFilter = null;

        private void Start()
        {
            // Arama input olayları
            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener(OnSearchQueryChanged);
                searchInput.placeholder.GetComponent<TMP_Text>().text = "Ara: Bölüm veya alan adı...";
            }

            if (clearSearchButton != null)
                clearSearchButton.onClick.AddListener(ClearSearch);

            if (toggleButton != null)
                toggleButton.onClick.AddListener(() => UIManager.Instance?.ToggleSearchPanel());

            // Filtre dropdown'ları
            SetupFloorFilter();
            SetupCategoryFilter();

            // Kat değişikliğinde listeyi güncelle
            if (FloorManager.Instance != null)
                FloorManager.Instance.OnFloorChanged += OnFloorChanged;

            if (FactoryManager.Instance != null)
                FactoryManager.Instance.OnFactoryChanged += OnFactoryChanged;
        }

        private void OnDestroy()
        {
            if (FloorManager.Instance != null)
                FloorManager.Instance.OnFloorChanged -= OnFloorChanged;

            if (FactoryManager.Instance != null)
                FactoryManager.Instance.OnFactoryChanged -= OnFactoryChanged;
        }

        #region Panel Gösterimi

        /// <summary>Paneli gösterir ve listeyi günceller.</summary>
        public void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshList();
        }

        /// <summary>Paneli gizler.</summary>
        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        #endregion

        #region Arama

        private void OnSearchQueryChanged(string query)
        {
            _currentSearchQuery = query;
            RefreshList();
        }

        private void ClearSearch()
        {
            if (searchInput != null) searchInput.text = "";
            _currentSearchQuery = "";
            RefreshList();
        }

        #endregion

        #region Filtreler

        private void SetupFloorFilter()
        {
            if (floorFilterDropdown == null) return;

            floorFilterDropdown.ClearOptions();
            var options = new List<string> { "Tüm Katlar" };

            if (FloorManager.Instance != null)
            {
                for (int i = 0; i < FloorManager.Instance.FloorCount; i++)
                {
                    options.Add(FloorManager.Instance.GetFloorName(i));
                }
            }

            floorFilterDropdown.AddOptions(options);
            floorFilterDropdown.onValueChanged.AddListener(OnFloorFilterChanged);
        }

        private void SetupCategoryFilter()
        {
            if (categoryFilterDropdown == null) return;

            categoryFilterDropdown.ClearOptions();
            var options = new List<string> { "Tüm Kategoriler" };
            options.AddRange(System.Enum.GetNames(typeof(POICategory)).Select(GetCategoryDisplayName));

            categoryFilterDropdown.AddOptions(options);
            categoryFilterDropdown.onValueChanged.AddListener(OnCategoryFilterChanged);
        }

        private void OnFloorFilterChanged(int index)
        {
            _selectedFloorFilter = index - 1; // 0 = tümü, 1 = zemin kat, vb.
            RefreshList();
        }

        private void OnCategoryFilterChanged(int index)
        {
            if (index == 0)
                _selectedCategoryFilter = null;
            else
                _selectedCategoryFilter = (POICategory)(index - 1);
            RefreshList();
        }

        private void OnFloorChanged(int floorIndex)
        {
            if (floorFilterDropdown != null)
                floorFilterDropdown.value = floorIndex + 1;
        }

        private void OnFactoryChanged(FactoryData factory)
        {
            RefreshList();
        }

        #endregion

        #region Liste Oluşturma

        /// <summary>Listeyi filtreler ve yeniden oluşturur.</summary>
        public void RefreshList()
        {
            ClearList();

            if (POIManager.Instance == null) return;

            // Filtrelenmiş POI'ları al
            List<POIMarker> filteredMarkers = GetFilteredMarkers();

            // Kategoriye göre grupla
            var grouped = filteredMarkers
                .Where(m => m.POIData != null)
                .GroupBy(m => m.POIData.category)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                // Kategori başlığı oluştur
                CreateCategoryHeader(group.Key, group.Count());

                // Her POI için liste öğesi oluştur
                foreach (var marker in group.OrderBy(m => m.POIData.poiName))
                {
                    CreatePOIListItem(marker);
                }
            }
        }

        private List<POIMarker> GetFilteredMarkers()
        {
            if (POIManager.Instance == null) return new List<POIMarker>();

            List<POIMarker> markers;

            // Metin araması
            if (!string.IsNullOrEmpty(_currentSearchQuery))
            {
                markers = POIManager.Instance.SearchPOIs(_currentSearchQuery);
            }
            else
            {
                markers = POIManager.Instance.AllMarkers;
            }

            // Kat filtresi
            if (_selectedFloorFilter >= 0)
            {
                markers = markers.Where(m =>
                    m.POIData != null && m.POIData.floorIndex == _selectedFloorFilter).ToList();
            }

            // Kategori filtresi
            if (_selectedCategoryFilter.HasValue)
            {
                markers = markers.Where(m =>
                    m.POIData != null && m.POIData.category == _selectedCategoryFilter.Value).ToList();
            }

            // Fabrika filtresi
            if (FactoryManager.Instance != null && FactoryManager.Instance.CurrentFactory != null)
            {
                string factoryId = FactoryManager.Instance.CurrentFactory.factoryId;
                markers = markers.Where(m =>
                    m.POIData != null && m.POIData.factoryId == factoryId).ToList();
            }

            return markers;
        }

        private void CreateCategoryHeader(POICategory category, int count)
        {
            if (categoryHeaderPrefab == null || listContent == null) return;

            GameObject header = Instantiate(categoryHeaderPrefab, listContent);
            _spawnedItems.Add(header);

            // Başlık metnini ayarla
            var headerText = header.GetComponentInChildren<TMP_Text>();
            if (headerText != null)
            {
                headerText.text = $"{GetCategoryDisplayName(category.ToString())} ({count})";
            }

            // Kategori rengini ayarla
            var headerImage = header.GetComponent<Image>();
            if (headerImage != null)
            {
                Color catColor = POIData.GetCategoryColorStatic(category);
                headerImage.color = new Color(catColor.r, catColor.g, catColor.b, 0.2f);
            }
        }

        private void CreatePOIListItem(POIMarker marker)
        {
            if (poiListItemPrefab == null || listContent == null) return;

            GameObject item = Instantiate(poiListItemPrefab, listContent);
            _spawnedItems.Add(item);

            // İsim metnini ayarla
            var nameText = item.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = marker.POIData.poiName;
            }

            // Kategori renk göstergesi
            var colorIndicator = item.transform.Find("ColorIndicator");
            if (colorIndicator != null)
            {
                var img = colorIndicator.GetComponent<Image>();
                if (img != null)
                    img.color = marker.POIData.GetCategoryColor();
            }

            // Tıklama olayı
            var button = item.GetComponent<Button>();
            if (button != null)
            {
                POIMarker capturedMarker = marker; // Closure için
                button.onClick.AddListener(() => OnPOIItemClicked(capturedMarker));
            }
        }

        private void OnPOIItemClicked(POIMarker marker)
        {
            if (marker == null) return;

            // POI'yı seç
            POIManager.Instance?.SelectPOI(marker);

            // İlgili kata geç
            if (FloorManager.Instance != null && marker.POIData != null)
            {
                FloorManager.Instance.SwitchToFloor(marker.POIData.floorIndex);
            }
        }

        private void ClearList()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null) Destroy(item);
            }
            _spawnedItems.Clear();
        }

        #endregion

        #region Yardımcılar

        private string GetCategoryDisplayName(string categoryName)
        {
            return categoryName switch
            {
                "UretimBandi" => "Üretim Bandı",
                "Ofis" => "Ofisler",
                "Yemekhane" => "Yemekhane",
                "InsanKaynaklari" => "İnsan Kaynakları",
                "Depo" => "Depo",
                "Toplanti" => "Toplantı Odası",
                "Teknik" => "Teknik Alan",
                "Diger" => "Diğer",
                _ => categoryName
            };
        }

        #endregion
    }
}
