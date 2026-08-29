using System;
using System.Collections.Generic;
using UnityEngine;

namespace FabricHelper
{
    /// <summary>
    /// Bir fabrikanın yapılandırmasını içerir.
    /// </summary>
    [Serializable]
    public class FactoryConfig
    {
        [Tooltip("Fabrika verisi (ScriptableObject)")]
        public FactoryData factoryData;
        
        [Tooltip("Bu fabrikaya ait tüm objelerin bulunduğu kök obje")]
        public GameObject factoryRoot;
        
        [Tooltip("Bu fabrika seçildiğinde kameranın odaklanacağı nokta")]
        public Transform cameraFocusPoint;
    }

    /// <summary>
    /// Bölgedeki birden fazla fabrikayı yöneten singleton sınıf.
    /// </summary>
    public class FactoryManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton örneği.
        /// </summary>
        public static FactoryManager Instance { get; private set; }

        [Header("Fabrika Ayarları")]
        [Tooltip("Sahnedeki fabrika yapılandırmalarının listesi")]
        [SerializeField] private List<FactoryConfig> _factories = new List<FactoryConfig>();

        [Tooltip("Şu an aktif olan fabrika")]
        [SerializeField] private FactoryConfig _currentFactory;

        /// <summary>Şu an aktif olan fabrika verisi.</summary>
        public FactoryData CurrentFactory => _currentFactory?.factoryData;

        /// <summary>
        /// Fabrika değiştirildiğinde tetiklenir (Yeni fabrika verisi ile).
        /// </summary>
        public event Action<FactoryData> OnFactoryChanged;

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
        /// Belirtilen ID'ye sahip fabrikaya geçiş yapar, diğerlerini gizler.
        /// </summary>
        public void SwitchToFactory(string factoryId)
        {
            var factory = _factories.Find(f => f.factoryData != null && f.factoryData.factoryId == factoryId);
            if (factory != null)
            {
                SetCurrentFactory(factory);
            }
        }

        /// <summary>
        /// Belirtilen indekse sahip fabrikaya geçiş yapar.
        /// </summary>
        public void SwitchToFactory(int index)
        {
            if (index >= 0 && index < _factories.Count)
            {
                SetCurrentFactory(_factories[index]);
            }
        }

        /// <summary>
        /// Geçerli fabrikayı belirler ve görünürlüklerini günceller.
        /// </summary>
        private void SetCurrentFactory(FactoryConfig newFactory)
        {
            _currentFactory = newFactory;

            foreach (var factory in _factories)
            {
                if (factory.factoryRoot != null)
                {
                    factory.factoryRoot.SetActive(factory == _currentFactory);
                }
            }

            if (_currentFactory != null && _currentFactory.factoryData != null)
            {
                OnFactoryChanged?.Invoke(_currentFactory.factoryData);
            }
        }

        /// <summary>
        /// Mevcut fabrikanın verisini döndürür.
        /// </summary>
        public FactoryData GetCurrentFactory()
        {
            return _currentFactory != null ? _currentFactory.factoryData : null;
        }

        /// <summary>
        /// Genel bakış modu için tüm fabrikaları görünür yapar.
        /// </summary>
        public void ShowAllFactories()
        {
            _currentFactory = null;

            foreach (var factory in _factories)
            {
                if (factory.factoryRoot != null)
                {
                    factory.factoryRoot.SetActive(true);
                }
            }

            OnFactoryChanged?.Invoke(null);
        }

        /// <summary>
        /// Tüm fabrika verilerinin listesini döndürür.
        /// </summary>
        public List<FactoryData> GetAllFactories()
        {
            var result = new List<FactoryData>();
            foreach (var config in _factories)
            {
                if (config.factoryData != null)
                    result.Add(config.factoryData);
            }
            return result;
        }

        /// <summary>
        /// Tüm fabrika yapılandırmalarının listesini döndürür.
        /// </summary>
        public List<FactoryConfig> GetAllConfigs()
        {
            return new List<FactoryConfig>(_factories);
        }
    }
}
