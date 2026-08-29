using System.Collections;
using UnityEngine;

namespace FabricHelper
{
    /// <summary>
    /// Navigasyon rotasını 3 boyutlu uzayda hareketli bir çizgi olarak çizen MonoBehaviour.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class NavigationLineRenderer : MonoBehaviour
    {
        [Header("Line Settings")]
        [Tooltip("Çizgi bileşeni")]
        [SerializeField] private LineRenderer lineRenderer;
        
        [Tooltip("Çizgi genişliği")]
        [SerializeField] private float lineWidth = 0.3f;
        
        [Tooltip("Z-fighting'i önlemek için yerden yüksekliği")]
        [SerializeField] private float lineHeightOffset = 0.05f;
        
        [Tooltip("Başlangıç rengi")]
        [SerializeField] private Color startColor = new Color(0.2f, 0.6f, 1f, 0.8f);
        
        [Tooltip("Bitiş rengi")]
        [SerializeField] private Color endColor = new Color(1f, 0.4f, 0.1f, 0.8f);
        
        [Header("Animation Settings")]
        [Tooltip("UV kaydırma hızı")]
        [SerializeField] private float scrollSpeed = 2f;
        
        [Tooltip("Parlama şiddeti")]
        [SerializeField] private float glowIntensity = 2f;
        
        [Tooltip("Çizgi için kullanılacak materyal (UV kaydırmayı desteklemeli)")]
        [SerializeField] private Material lineMaterial;
        
        [Tooltip("Çizginin yavaşça çizilme animasyonu olsun mu?")]
        [SerializeField] private bool animateLine = true;
        
        [Tooltip("Kesik çizgi uzunluğu")]
        [SerializeField] private float dashLength = 1f;

        private Coroutine drawingCoroutine;
        private bool isPathActive = false;

        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            ConfigureLineRenderer();
        }

        private void ConfigureLineRenderer()
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(startColor.a, 0.0f), new GradientAlphaKey(endColor.a, 1.0f) }
            );
            lineRenderer.colorGradient = gradient;

            if (lineMaterial == null)
            {
                lineMaterial = CreateDefaultMaterial();
            }
            
            if (lineMaterial != null)
            {
                // Material'ın kopyasını oluşturarak diğer objelerin etkilenmesini önle
                lineRenderer.material = new Material(lineMaterial);
            }
            
            lineRenderer.enabled = false;
        }

        private Material CreateDefaultMaterial()
        {
            // Unity 6 URP uyumlu basit bir materyal bulmaya çalış
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }
            
            if (shader != null)
            {
                Material mat = new Material(shader);
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", Color.white * glowIntensity);
                }
                return mat;
            }
            
            return null;
        }

        /// <summary>
        /// Verilen noktaları kullanarak rotayı çizer.
        /// </summary>
        public void DrawPath(Vector3[] pathPoints)
        {
            if (pathPoints == null || pathPoints.Length < 2) return;

            ClearPath();
            isPathActive = true;
            
            // Noktaları z-fighting olmaması için ofsetle
            Vector3[] offsetPoints = new Vector3[pathPoints.Length];
            for (int i = 0; i < pathPoints.Length; i++)
            {
                offsetPoints[i] = pathPoints[i] + Vector3.up * lineHeightOffset;
            }

            lineRenderer.enabled = true;

            if (animateLine)
            {
                drawingCoroutine = StartCoroutine(AnimateAppearance(offsetPoints));
            }
            else
            {
                lineRenderer.positionCount = offsetPoints.Length;
                lineRenderer.SetPositions(offsetPoints);
            }
        }

        /// <summary>
        /// Çizgiyi temizler ve animasyonları durdurur.
        /// </summary>
        public void ClearPath()
        {
            if (drawingCoroutine != null)
            {
                StopCoroutine(drawingCoroutine);
                drawingCoroutine = null;
            }
            
            isPathActive = false;
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }

        private IEnumerator AnimateAppearance(Vector3[] points)
        {
            float duration = 1.0f;
            float totalLength = GetTotalLength(points);
            
            // Sıfıra bölünmeyi engelle
            if (totalLength <= 0.001f)
            {
                lineRenderer.positionCount = points.Length;
                lineRenderer.SetPositions(points);
                yield break;
            }
            
            float speed = totalLength / duration;
            
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, points[0]);
            
            float distanceCovered = 0f;
            int currentIndex = 0;

            while (currentIndex < points.Length - 1)
            {
                distanceCovered += speed * Time.deltaTime;
                
                float segmentLength = Vector3.Distance(points[currentIndex], points[currentIndex + 1]);
                
                if (distanceCovered >= segmentLength)
                {
                    distanceCovered -= segmentLength;
                    currentIndex++;
                    lineRenderer.positionCount = currentIndex + 1;
                    lineRenderer.SetPosition(currentIndex, points[currentIndex]);
                }
                else
                {
                    lineRenderer.positionCount = currentIndex + 2;
                    Vector3 currentPos = Vector3.Lerp(points[currentIndex], points[currentIndex + 1], distanceCovered / segmentLength);
                    lineRenderer.SetPosition(currentIndex + 1, currentPos);
                    yield return null;
                }
            }
            
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        private float GetTotalLength(Vector3[] points)
        {
            float length = 0f;
            for (int i = 0; i < points.Length - 1; i++)
            {
                length += Vector3.Distance(points[i], points[i + 1]);
            }
            return length;
        }

        private void Update()
        {
            if (isPathActive && animateLine && lineRenderer.material != null)
            {
                // URP için genellikle "_BaseMap", eski pipeline için "_MainTex"
                string textureProp = lineRenderer.material.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
                
                if (lineRenderer.material.HasProperty(textureProp))
                {
                    Vector2 currentOffset = lineRenderer.material.GetTextureOffset(textureProp);
                    lineRenderer.material.SetTextureOffset(textureProp, currentOffset + new Vector2(Time.deltaTime * scrollSpeed, 0));
                }
            }
        }
    }
}
