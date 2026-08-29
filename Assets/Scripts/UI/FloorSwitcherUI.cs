using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace FabricHelper
{
    /// <summary>
    /// Kat değiştirme UI bileşeni.
    /// Kullanıcının fabrika katları arasında geçiş yapmasını sağlar.
    /// Dikey buton listesi veya dropdown olarak gösterilebilir.
    /// </summary>
    public class FloorSwitcherUI : MonoBehaviour
    {
        [Header("Mod")]
        [SerializeField] private DisplayMode displayMode = DisplayMode.ButtonList;

        [Header("Buton Listesi Modu")]
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject floorButtonPrefab;

        [Header("Dropdown Modu")]
        [SerializeField] private TMP_Dropdown floorDropdown;

        [Header("Görsel Ayarlar")]
        [SerializeField] private Color activeFloorColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color inactiveFloorColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color activeTextColor = Color.white;
        [SerializeField] private Color inactiveTextColor = new Color(0.3f, 0.3f, 0.3f);

        private List<FloorButtonInfo> _floorButtons = new List<FloorButtonInfo>();

        public enum DisplayMode
        {
            ButtonList,
            Dropdown
        }

        private class FloorButtonInfo
        {
            public GameObject buttonObject;
            public Button button;
            public TMP_Text text;
            public Image background;
            public int floorIndex;
        }

        private void Start()
        {
            if (FloorManager.Instance != null)
            {
                FloorManager.Instance.OnFloorChanged += OnFloorChanged;
                BuildUI();
            }
        }

        private void OnDestroy()
        {
            if (FloorManager.Instance != null)
                FloorManager.Instance.OnFloorChanged -= OnFloorChanged;
        }

        /// <summary>UI elemanlarını oluşturur.</summary>
        private void BuildUI()
        {
            if (FloorManager.Instance == null) return;

            int floorCount = FloorManager.Instance.FloorCount;

            switch (displayMode)
            {
                case DisplayMode.ButtonList:
                    BuildButtonList(floorCount);
                    break;
                case DisplayMode.Dropdown:
                    BuildDropdown(floorCount);
                    break;
            }

            // Mevcut katı işaretle
            UpdateVisuals(FloorManager.Instance.CurrentFloorIndex);
        }

        private void BuildButtonList(int floorCount)
        {
            if (buttonContainer == null || floorButtonPrefab == null) return;

            // Mevcut butonları temizle
            foreach (var info in _floorButtons)
            {
                if (info.buttonObject != null) Destroy(info.buttonObject);
            }
            _floorButtons.Clear();

            // Her kat için buton oluştur (üstten alta)
            for (int i = floorCount - 1; i >= 0; i--)
            {
                GameObject btnObj = Instantiate(floorButtonPrefab, buttonContainer);
                var info = new FloorButtonInfo
                {
                    buttonObject = btnObj,
                    button = btnObj.GetComponent<Button>(),
                    text = btnObj.GetComponentInChildren<TMP_Text>(),
                    background = btnObj.GetComponent<Image>(),
                    floorIndex = i
                };

                string floorName = FloorManager.Instance.GetFloorName(i);
                if (info.text != null)
                    info.text.text = floorName;

                int capturedIndex = i;
                if (info.button != null)
                    info.button.onClick.AddListener(() => OnFloorButtonClicked(capturedIndex));

                _floorButtons.Add(info);
            }
        }

        private void BuildDropdown(int floorCount)
        {
            if (floorDropdown == null) return;

            floorDropdown.ClearOptions();
            var options = new List<string>();

            for (int i = 0; i < floorCount; i++)
            {
                options.Add(FloorManager.Instance.GetFloorName(i));
            }

            floorDropdown.AddOptions(options);
            floorDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void OnFloorButtonClicked(int floorIndex)
        {
            FloorManager.Instance?.SwitchToFloor(floorIndex);
        }

        private void OnDropdownChanged(int index)
        {
            FloorManager.Instance?.SwitchToFloor(index);
        }

        private void OnFloorChanged(int newFloorIndex)
        {
            UpdateVisuals(newFloorIndex);
        }

        /// <summary>Aktif katın görselini günceller.</summary>
        private void UpdateVisuals(int activeFloor)
        {
            switch (displayMode)
            {
                case DisplayMode.ButtonList:
                    foreach (var info in _floorButtons)
                    {
                        bool isActive = info.floorIndex == activeFloor;
                        if (info.background != null)
                            info.background.color = isActive ? activeFloorColor : inactiveFloorColor;
                        if (info.text != null)
                            info.text.color = isActive ? activeTextColor : inactiveTextColor;
                    }
                    break;

                case DisplayMode.Dropdown:
                    if (floorDropdown != null)
                        floorDropdown.value = activeFloor;
                    break;
            }
        }
    }
}
