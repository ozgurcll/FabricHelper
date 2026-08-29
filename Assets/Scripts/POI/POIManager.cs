using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FabricHelper
{
    /// <summary>
    /// Sahnedeki tüm POI'leri yöneten singleton sınıf.
    /// </summary>
    public class POIManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton örneği.
        /// </summary>
        public static POIManager Instance { get; private set; }

        [Header("Durum")]
        [Tooltip("Kayıtlı tüm POI işaretçileri")]
        [SerializeField] private List<POIMarker> _allMarkers = new List<POIMarker>();

        [Tooltip("Şu an seçili olan POI işaretçisi")]
        [SerializeField] private POIMarker _currentSelected;

        /// <summary>Tüm kayıtlı POI işaretçileri.</summary>
        public List<POIMarker> AllMarkers => _allMarkers;

        /// <summary>Şu an seçili olan POI işaretçisi.</summary>
        public POIMarker CurrentSelected => _currentSelected;

        /// <summary>
        /// Yeni bir POI seçildiğinde tetiklenir.
        /// </summary>
        public event Action<POIMarker> OnPOISelected;

        /// <summary>
        /// POI seçimi kaldırıldığında tetiklenir.
        /// </summary>
        public event Action OnPOIDeselected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// POI işaretçisini sisteme kaydeder.
        /// </summary>
        public void RegisterMarker(POIMarker marker)
        {
            if (marker != null && !_allMarkers.Contains(marker))
            {
                _allMarkers.Add(marker);
            }
        }

        /// <summary>
        /// POI işaretçisinin sistemden kaydını siler.
        /// </summary>
        public void UnregisterMarker(POIMarker marker)
        {
            if (marker != null && _allMarkers.Contains(marker))
            {
                _allMarkers.Remove(marker);
            }
        }

        /// <summary>
        /// Yeni bir POI seçer, mevcut seçili olanı kaldırır.
        /// </summary>
        public void SelectPOI(POIMarker marker)
        {
            if (_currentSelected == marker) return;

            if (_currentSelected != null)
            {
                _currentSelected.Deselect();
            }

            _currentSelected = marker;

            if (_currentSelected != null)
            {
                _currentSelected.Select();
                OnPOISelected?.Invoke(_currentSelected);
            }
            else
            {
                OnPOIDeselected?.Invoke();
            }
        }

        /// <summary>
        /// Tüm POI seçimlerini kaldırır.
        /// </summary>
        public void DeselectAll()
        {
            SelectPOI(null);
        }

        /// <summary>
        /// Verilen sorguya göre POI'leri isim, etiketler ve açıklamaya göre arar.
        /// </summary>
        public List<POIMarker> SearchPOIs(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<POIMarker>();

            return _allMarkers.Where(m => m.PoiData != null && (
                (m.PoiData.poiName != null && m.PoiData.poiName.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (m.PoiData.description != null && m.PoiData.description.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (m.PoiData.tags != null && m.PoiData.tags.Any(t => t.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0))
            )).ToList();
        }

        /// <summary>
        /// Belirli bir kategoriye ait POI'leri döndürür.
        /// </summary>
        public List<POIMarker> GetPOIsByCategory(POICategory category)
        {
            return _allMarkers.Where(m => m.PoiData != null && m.PoiData.category == category).ToList();
        }

        /// <summary>
        /// Belirli bir kata ait POI'leri döndürür.
        /// </summary>
        public List<POIMarker> GetPOIsByFloor(int floorIndex)
        {
            return _allMarkers.Where(m => m.PoiData != null && m.PoiData.floorIndex == floorIndex).ToList();
        }

        /// <summary>
        /// Belirli bir fabrikaya ait POI'leri döndürür.
        /// </summary>
        public List<POIMarker> GetPOIsByFactory(string factoryId)
        {
            return _allMarkers.Where(m => m.PoiData != null && m.PoiData.factoryId == factoryId).ToList();
        }

        /// <summary>
        /// Belirli bir kat ve fabrikaya ait POI'leri döndürür.
        /// </summary>
        public List<POIMarker> GetPOIsByFloorAndFactory(int floorIndex, string factoryId)
        {
            return _allMarkers.Where(m => m.PoiData != null && m.PoiData.floorIndex == floorIndex && m.PoiData.factoryId == factoryId).ToList();
        }
    }
}
