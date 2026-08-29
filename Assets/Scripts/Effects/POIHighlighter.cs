using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FabricHelper
{
    /// <summary>
    /// Önemli Nokta (POI) alanlarında görsel vurgulama efektleri sağlar.
    /// </summary>
    public class POIHighlighter : MonoBehaviour
    {
        [Header("Hedefler")]
        [Tooltip("Eğer boş bırakılırsa alt objelerdeki rendererlar otomatik bulunur.")]
        [SerializeField] private Renderer[] targetRenderers;

        [Header("Efekt Ayarları")]
        [SerializeField] private Color highlightColor = new Color(0.3f, 0.7f, 1f, 0.5f);
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private bool useOutline = true;

        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> highlightMaterials = new Dictionary<Renderer, Material[]>();
        private Coroutine pulseCoroutine;
        private bool isHighlighted = false;

        private void Awake()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<Renderer>();
            }

            // Orijinal materyalleri kaydet
            foreach (Renderer r in targetRenderers)
            {
                if (r != null)
                {
                    originalMaterials[r] = r.sharedMaterials;
                }
            }
        }

        /// <summary>
        /// Vurgulama durumunu açar veya kapatır.
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            if (isHighlighted == highlighted) return;
            isHighlighted = highlighted;

            if (highlighted)
            {
                ApplyHighlightMaterials();
                if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
                pulseCoroutine = StartCoroutine(PulseCoroutine());
            }
            else
            {
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                }
                RestoreOriginalMaterials();
            }
        }

        private void ApplyHighlightMaterials()
        {
            foreach (Renderer r in targetRenderers)
            {
                if (r == null) continue;

                if (!highlightMaterials.ContainsKey(r))
                {
                    Material[] newMats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        if (highlightMaterial != null)
                        {
                            // Belirtilen materyalin bir kopyasını oluştur (örnek: Outline materyali)
                            newMats[i] = new Material(highlightMaterial);
                        }
                        else
                        {
                            // Kendi materyalinin bir kopyasını oluştur (URP uyumlu varsayılarak Emission ayarlanacak)
                            newMats[i] = new Material(r.sharedMaterials[i]);
                            newMats[i].EnableKeyword("_EMISSION");
                        }
                        
                        if (newMats[i].HasProperty("_BaseColor"))
                        {
                            newMats[i].SetColor("_BaseColor", highlightColor);
                        }
                    }
                    highlightMaterials[r] = newMats;
                }

                r.materials = highlightMaterials[r];
            }
        }

        private void RestoreOriginalMaterials()
        {
            foreach (Renderer r in targetRenderers)
            {
                if (r != null && originalMaterials.ContainsKey(r))
                {
                    r.materials = originalMaterials[r];
                }
            }
        }

        private IEnumerator PulseCoroutine()
        {
            float timer = 0f;

            while (true)
            {
                timer += Time.deltaTime * pulseSpeed;
                
                // Sinüs dalgası (0 ile 1 arasında dalgalanma)
                float pulse = (Mathf.Sin(timer) + 1f) / 2f; 
                float currentIntensity = pulse * pulseIntensity;

                foreach (Renderer r in targetRenderers)
                {
                    if (r == null) continue;

                    Material[] mats = r.sharedMaterials;
                    foreach (Material mat in mats)
                    {
                        if (mat != null)
                        {
                            // URP Lit Shader Emission özelliği "_EmissionColor" olarak isimlendirilir
                            if (mat.HasProperty("_EmissionColor"))
                            {
                                Color finalEmission = highlightColor * currentIntensity;
                                mat.SetColor("_EmissionColor", finalEmission);
                            }
                            // Opsiyonel Outline veya farklı shader özellikleri burada güncellenebilir
                            if (useOutline && mat.HasProperty("_OutlineColor"))
                            {
                                mat.SetColor("_OutlineColor", highlightColor * (0.5f + currentIntensity));
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        private void OnDisable()
        {
            if (isHighlighted)
            {
                SetHighlighted(false);
            }
        }

        private void OnDestroy()
        {
            // Bellek sızıntılarını önlemek için oluşturulan materyalleri yok et
            foreach (var kvp in highlightMaterials)
            {
                foreach (Material mat in kvp.Value)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
            }
        }
    }
}
