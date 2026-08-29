using UnityEngine;

namespace FabricHelper
{
    /// <summary>
    /// Fabrika bilgilerini tanımlayan veri sınıfı.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFactoryData", menuName = "FabricHelper/Factory Data")]
    public class FactoryData : ScriptableObject
    {
        [Header("Kimlik Bilgileri")]
        [Tooltip("Benzersiz fabrika kimliği")]
        public string factoryId;

        [Tooltip("Görünen fabrika adı (Örn: Ana Fabrika)")]
        public string factoryName;

        [Header("Detaylar")]
        [Tooltip("Fabrika hakkında açıklama")]
        [TextArea(3, 5)]
        public string description;

        [Tooltip("Fabrika veya şirket logosu")]
        public Sprite logo;

        [Header("Kat Bilgileri")]
        [Tooltip("Toplam kat sayısı")]
        public int totalFloors;

        [Tooltip("Kat isimleri (Örn: Zemin Kat, 1. Kat)")]
        public string[] floorNames;
    }
}
