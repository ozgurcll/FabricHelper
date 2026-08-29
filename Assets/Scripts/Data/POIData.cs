using UnityEngine;

namespace FabricHelper
{
    /// <summary>
    /// Fabrikadaki ilgi noktalarının (POI) kategorilerini belirler.
    /// </summary>
    public enum POICategory
    {
        UretimBandi,
        Ofis,
        Yemekhane,
        InsanKaynaklari,
        Depo,
        Toplanti,
        Teknik,
        Diger
    }

    /// <summary>
    /// Fabrikadaki bir ilgi noktasını (POI) tanımlayan veri sınıfı.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPOIData", menuName = "FabricHelper/POI Data")]
    public class POIData : ScriptableObject
    {
        [Header("Temel Bilgiler")]
        [Tooltip("İlgi noktasının görünen adı (Örn: Üretim Bandı 3)")]
        public string poiName;

        [Tooltip("İlgi noktası kategorisi")]
        public POICategory category;

        [Tooltip("Bu POI'nin ait olduğu fabrikanın ID'si")]
        public string factoryId;

        [Tooltip("Bulunduğu katın indeksi (0 tabanlı)")]
        public int floorIndex;

        [Header("Detaylar")]
        [Tooltip("Detaylı açıklama")]
        [TextArea(3, 5)]
        public string description;

        [Tooltip("Kapasite (Uygulanamaz ise 0)")]
        public int capacity;

        [Tooltip("Çalışma saatleri (Örn: 08:00 - 17:00)")]
        public string workingHours;

        [Tooltip("Ek bilgi veya notlar")]
        public string detailText;

        [Header("Görsel Öğeler")]
        [Tooltip("Fotoğraf (Varsa)")]
        public Sprite photo;

        [Tooltip("Özel ikon (Varsa)")]
        public Sprite icon;

        [Tooltip("Kategoriye göre otomatik atanan renk")]
        public Color categoryColor;

        [Header("Arama")]
        [Tooltip("Arama için etiketler")]
        public string[] tags;

        /// <summary>
        /// Kategoriye karşılık gelen rengi döndürür.
        /// </summary>
        public Color GetCategoryColor()
        {
            switch (category)
            {
                case POICategory.UretimBandi: return new Color(0.2f, 0.6f, 1f);
                case POICategory.Ofis: return new Color(1f, 0.6f, 0.2f);
                case POICategory.Yemekhane: return new Color(0.2f, 0.8f, 0.3f);
                case POICategory.InsanKaynaklari: return new Color(1f, 0.85f, 0.2f);
                case POICategory.Depo: return new Color(0.5f, 0.5f, 0.5f);
                case POICategory.Toplanti: return new Color(0.6f, 0.3f, 0.8f);
                case POICategory.Teknik: return new Color(0.8f, 0.2f, 0.2f);
                case POICategory.Diger:
                default: return Color.white;
            }
        }

        private void OnValidate()
        {
            categoryColor = GetCategoryColor();
        }

        /// <summary>
        /// Statik olarak kategori rengini döndürür (UI scriptlerinde kullanım için).
        /// </summary>
        public static Color GetCategoryColorStatic(POICategory cat)
        {
            switch (cat)
            {
                case POICategory.UretimBandi: return new Color(0.2f, 0.6f, 1f);
                case POICategory.Ofis: return new Color(1f, 0.6f, 0.2f);
                case POICategory.Yemekhane: return new Color(0.2f, 0.8f, 0.3f);
                case POICategory.InsanKaynaklari: return new Color(1f, 0.85f, 0.2f);
                case POICategory.Depo: return new Color(0.5f, 0.5f, 0.5f);
                case POICategory.Toplanti: return new Color(0.6f, 0.3f, 0.8f);
                case POICategory.Teknik: return new Color(0.8f, 0.2f, 0.2f);
                case POICategory.Diger:
                default: return Color.white;
            }
        }
    }
}
