using System;
using UnityEngine;

namespace FabricHelper
{
    /// <summary>
    /// 3D sahnedeki her bir oda/alan (POI) üzerine eklenen bileşen.
    /// </summary>
    public class POIMarker : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Bu işaretçinin temsil ettiği POI verisi")]
        [SerializeField] private POIData _poiData;

        [Tooltip("Pin ikonunun gösterileceği pozisyon (Boş bırakılırsa bu transform kullanılır)")]
        [SerializeField] private Transform _markerPosition;

        [Header("Durum")]
        [Tooltip("Geçerli seçim durumu")]
        [SerializeField] private bool _isSelected;

        /// <summary>
        /// POI seçildiğinde tetiklenen olay.
        /// </summary>
        public event Action<POIMarker> OnSelected;

        /// <summary>
        /// POI seçimi kaldırıldığında tetiklenen olay.
        /// </summary>
        public event Action<POIMarker> OnDeselected;

        /// <summary>
        /// İlgili POI verisine erişim sağlar.
        /// </summary>
        public POIData PoiData => _poiData;

        /// <summary>
        /// POIData property alias (diğer scriptlerle uyumluluk için).
        /// </summary>
        public POIData POIData => _poiData;

        /// <summary>
        /// Seçim durumunu döndürür.
        /// </summary>
        public bool IsSelected => _isSelected;

        private POIHighlighter _highlighter;

        private void Awake()
        {
            _highlighter = GetComponent<POIHighlighter>();
            
            if (POIManager.Instance != null)
            {
                POIManager.Instance.RegisterMarker(this);
            }
        }

        private void OnDestroy()
        {
            if (POIManager.Instance != null)
            {
                POIManager.Instance.UnregisterMarker(this);
            }
        }

        /// <summary>
        /// İşaretçiyi seçer, durumu günceller ve olayları tetikler.
        /// </summary>
        public void Select()
        {
            _isSelected = true;
            OnSelected?.Invoke(this);
            if (_highlighter != null) _highlighter.SetHighlighted(true);
        }

        /// <summary>
        /// İşaretçinin seçimini kaldırır, durumu günceller ve olayları tetikler.
        /// </summary>
        public void Deselect()
        {
            _isSelected = false;
            OnDeselected?.Invoke(this);
            if (_highlighter != null) _highlighter.SetHighlighted(false);
        }

        /// <summary>
        /// İşaretçinin dünya pozisyonunu döndürür.
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            return _markerPosition != null ? _markerPosition.position : transform.position;
        }

        /// <summary>
        /// İşaretçinin görünürlüğünü ayarlar.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}
