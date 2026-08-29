using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace FabricHelper
{
    /// <summary>
    /// NavMesh kullanarak rota hesaplamasını yöneten Singleton MonoBehaviour.
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }

        [Header("References")]
        [Tooltip("Yolu çizecek LineRenderer referansı")]
        [SerializeField] private NavigationLineRenderer lineRenderer;

        private NavMeshPath currentPath;
        private bool hasActiveRoute;
        private float routeDistance;
        private float estimatedTime;

        private const float WalkingSpeed = 1.4f; // m/s (ortalama yürüme hızı)

        public event Action<Vector3[]> OnRouteCalculated;
        public event Action<string> OnRouteFailed;
        public event Action OnRouteCleared;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            currentPath = new NavMeshPath();
        }

        /// <summary>
        /// İki Vector3 pozisyonu arasında rota hesaplar.
        /// </summary>
        public bool CalculateRoute(Vector3 startPos, Vector3 endPos)
        {
            if (NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, currentPath))
            {
                if (currentPath.status == NavMeshPathStatus.PathComplete)
                {
                    CalculatePathMetrics();
                    hasActiveRoute = true;
                    
                    if (lineRenderer != null)
                    {
                        lineRenderer.DrawPath(currentPath.corners);
                    }
                    
                    OnRouteCalculated?.Invoke(currentPath.corners);
                    return true;
                }
            }
            
            hasActiveRoute = false;
            OnRouteFailed?.Invoke("Rota hesaplanamadı veya hedefe ulaşılamıyor.");
            return false;
        }

        /// <summary>
        /// İki POI işareti (marker) arasında rota hesaplar.
        /// </summary>
        public bool CalculateRoute(POIMarker from, POIMarker to)
        {
            if (from == null || to == null)
            {
                OnRouteFailed?.Invoke("Başlangıç veya bitiş noktası geçersiz.");
                return false;
            }
            
            return CalculateRoute(from.transform.position, to.transform.position);
        }

        /// <summary>
        /// Sahnedeki kullanıcı konumundan hedefe rota hesaplar.
        /// </summary>
        public bool CalculateRouteFromUserLocation(POIMarker destination)
        {
            if (destination == null)
            {
                OnRouteFailed?.Invoke("Hedef nokta geçersiz.");
                return false;
            }

            // Unity 6'da FindFirstObjectByType kullanımı tercih edilmeli
            UserLocationMarker userLocation = FindFirstObjectByType<UserLocationMarker>();
            
            if (userLocation == null)
            {
                OnRouteFailed?.Invoke("Kullanıcı konumu bulunamadı. Lütfen konumunuzu belirleyin.");
                return false;
            }
            
            return CalculateRoute(userLocation.transform.position, destination.transform.position);
        }

        /// <summary>
        /// Aktif rotayı temizler.
        /// </summary>
        public void ClearRoute()
        {
            currentPath.ClearCorners();
            hasActiveRoute = false;
            routeDistance = 0f;
            estimatedTime = 0f;
            
            if (lineRenderer != null)
            {
                lineRenderer.ClearPath();
            }
            
            OnRouteCleared?.Invoke();
        }

        private void CalculatePathMetrics()
        {
            routeDistance = 0f;
            
            if (currentPath.corners.Length < 2)
            {
                estimatedTime = 0f;
                return;
            }
            
            for (int i = 0; i < currentPath.corners.Length - 1; i++)
            {
                routeDistance += Vector3.Distance(currentPath.corners[i], currentPath.corners[i + 1]);
            }
            
            estimatedTime = routeDistance / WalkingSpeed;
        }

        /// <summary>
        /// Rota mesafesini metre cinsinden döndürür.
        /// </summary>
        public float GetRouteDistance()
        {
            return routeDistance;
        }

        /// <summary>
        /// Tahmini varış süresini saniye cinsinden döndürür.
        /// </summary>
        public float GetEstimatedTime()
        {
            return estimatedTime;
        }

        /// <summary>
        /// Formatlı tahmini varış süresini döndürür (Örn: "2 dk 30 sn").
        /// </summary>
        public string GetFormattedTime()
        {
            if (!hasActiveRoute) return "0 sn";
            
            int minutes = Mathf.FloorToInt(estimatedTime / 60f);
            int seconds = Mathf.FloorToInt(estimatedTime % 60f);
            
            if (minutes > 0)
            {
                return $"{minutes} dk {seconds} sn";
            }
            
            return $"{seconds} sn";
        }

        /// <summary>
        /// Formatlı rota mesafesini döndürür (Örn: "150 m").
        /// </summary>
        public string GetFormattedDistance()
        {
            if (!hasActiveRoute) return "0 m";
            
            return $"{Mathf.RoundToInt(routeDistance)} m";
        }
    }
}
