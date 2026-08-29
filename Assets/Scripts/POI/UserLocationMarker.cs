using System;
using UnityEngine;

namespace FabricHelper
{
    /// <summary>
    /// Kullanıcının konumunu (Siz Buradasınız) gösteren işaretçi.
    /// </summary>
    public class UserLocationMarker : MonoBehaviour
    {
        [Header("Görsel Ayarlar")]
        [Tooltip("Konumu gösteren görsel öğe")]
        [SerializeField] private Transform _markerVisual;
        
        [Tooltip("Animasyon hızı")]
        [SerializeField] private float _pulseSpeed = 5f;
        
        [Tooltip("Animasyon büyüklüğü")]
        [SerializeField] private float _pulseAmount = 0.2f;

        [Header("Durum")]
        [Tooltip("Kullanıcının konumunu belirleyip belirlemediği")]
        [SerializeField] private bool _isPlaced;
        
        [Tooltip("Kullanıcının bulunduğu geçerli kat")]
        [SerializeField] private int _currentFloor;
        
        [Tooltip("Kullanıcının bulunduğu fabrika ID'si")]
        [SerializeField] private string _currentFactoryId;

        /// <summary>
        /// Konum ayarlandığında tetiklenir (Pozisyon bilgisi ile).
        /// </summary>
        public event Action<Vector3> OnLocationSet;

        /// <summary>
        /// Konum temizlendiğinde tetiklenir.
        /// </summary>
        public event Action OnLocationCleared;
        
        private Vector3 _initialScale;

        private void Awake()
        {
            if (_markerVisual != null)
            {
                _initialScale = _markerVisual.localScale;
            }
            
            if (!_isPlaced)
            {
                ClearLocation();
            }
        }

        private void Update()
        {
            if (_isPlaced && _markerVisual != null)
            {
                float scaleMultiplier = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
                _markerVisual.localScale = _initialScale * scaleMultiplier;
            }
        }

        /// <summary>
        /// Marker'ı belirtilen dünya pozisyonuna yerleştirir.
        /// </summary>
        public void SetLocation(Vector3 worldPos)
        {
            transform.position = worldPos;
            _isPlaced = true;
            
            if (_markerVisual != null)
            {
                _markerVisual.gameObject.SetActive(true);
            }
            
            OnLocationSet?.Invoke(worldPos);
        }

        /// <summary>
        /// Marker'ı kaldırır ve durumu sıfırlar.
        /// </summary>
        public void ClearLocation()
        {
            _isPlaced = false;
            
            if (_markerVisual != null)
            {
                _markerVisual.gameObject.SetActive(false);
            }
            
            OnLocationCleared?.Invoke();
        }

        /// <summary>
        /// Mevcut konumu döndürür.
        /// </summary>
        public Vector3 GetLocation()
        {
            return transform.position;
        }

        /// <summary>
        /// Kullanıcının bulunduğu katı ayarlar.
        /// </summary>
        public void SetFloor(int floor)
        {
            _currentFloor = floor;
        }

        /// <summary>
        /// Kullanıcının bulunduğu fabrikayı ayarlar.
        /// </summary>
        public void SetFactory(string factoryId)
        {
            _currentFactoryId = factoryId;
        }
    }
}
