using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace FabricHelper
{
    /// <summary>
    /// Bir katın yapılandırmasını içerir.
    /// </summary>
    [Serializable]
    public class FloorConfig
    {
        [Tooltip("Katın görünen adı (Örn: Zemin Kat)")]
        public string floorName;
        
        [Tooltip("Kat indeksi (0 tabanlı)")]
        public int floorIndex;
        
        [Tooltip("Bu kata ait tüm objelerin bulunduğu kök obje")]
        public GameObject floorRoot;
        
        [Tooltip("Bu kat için oluşturulan NavMesh yüzeyi")]
        public NavMeshSurface navMeshSurface;
    }

    /// <summary>
    /// Fabrikadaki katların görünürlüğünü ve geçişlerini yöneten singleton sınıf.
    /// </summary>
    public class FloorManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton örneği.
        /// </summary>
        public static FloorManager Instance { get; private set; }

        [Header("Kat Ayarları")]
        [Tooltip("Sahnedeki kat yapılandırmalarının listesi")]
        [SerializeField] private List<FloorConfig> _floors = new List<FloorConfig>();

        [Tooltip("Şu an aktif olan katın indeksi")]
        [SerializeField] private int _currentFloorIndex = -1;

        /// <summary>Toplam kat sayısı.</summary>
        public int FloorCount => _floors.Count;

        /// <summary>Mevcut aktif kat indeksi.</summary>
        public int CurrentFloorIndex => _currentFloorIndex;

        /// <summary>
        /// Kat değiştirildiğinde tetiklenir (Yeni kat indeksi ile).
        /// </summary>
        public event Action<int> OnFloorChanged;

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
        /// Belirtilen indekse sahip kata geçiş yapar, diğer katları gizler.
        /// </summary>
        public void SwitchToFloor(int index)
        {
            _currentFloorIndex = index;

            foreach (var floor in _floors)
            {
                if (floor.floorRoot != null)
                {
                    floor.floorRoot.SetActive(floor.floorIndex == index);
                }
            }

            OnFloorChanged?.Invoke(index);
        }

        /// <summary>
        /// Mevcut kat indeksini döndürür.
        /// </summary>
        public int GetCurrentFloor()
        {
            return _currentFloorIndex;
        }

        /// <summary>
        /// İndeksi verilen katın adını döndürür.
        /// </summary>
        public string GetFloorName(int index)
        {
            var floor = _floors.Find(f => f.floorIndex == index);
            return floor != null ? floor.floorName : string.Empty;
        }

        /// <summary>
        /// Genel bakış (Overview) modu için tüm katları görünür yapar.
        /// </summary>
        public void ShowAllFloors()
        {
            _currentFloorIndex = -1; // -1 genel bakışı temsil eder

            foreach (var floor in _floors)
            {
                if (floor.floorRoot != null)
                {
                    floor.floorRoot.SetActive(true);
                }
            }

            OnFloorChanged?.Invoke(-1);
        }
    }
}
