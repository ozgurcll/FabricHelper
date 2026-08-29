using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace FabricHelper
{
    /// <summary>
    /// Fabrika değiştirme UI bileşeni.
    /// Bir alanda birden fazla fabrika olduğunda aralarında geçiş sağlar.
    /// Üst kısımda dropdown veya buton grubu olarak gösterilir.
    /// </summary>
    public class FactorySwitcherUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private TMP_Dropdown factoryDropdown;
        [SerializeField] private TMP_Text currentFactoryNameText;
        [SerializeField] private Image factoryLogo;

        [Header("Buton Grubu (Opsiyonel)")]
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject factoryButtonPrefab;
        [SerializeField] private bool useButtons = false;

        [Header("Görsel")]
        [SerializeField] private Color activeColor = new Color(0.15f, 0.4f, 0.7f);
        [SerializeField] private Color inactiveColor = new Color(0.9f, 0.9f, 0.9f);

        private List<Button> _factoryButtons = new List<Button>();

        private void Start()
        {
            if (FactoryManager.Instance != null)
            {
                FactoryManager.Instance.OnFactoryChanged += OnFactoryChanged;
                BuildUI();
            }
        }

        private void OnDestroy()
        {
            if (FactoryManager.Instance != null)
                FactoryManager.Instance.OnFactoryChanged -= OnFactoryChanged;
        }

        private void BuildUI()
        {
            if (FactoryManager.Instance == null) return;

            var factories = FactoryManager.Instance.GetAllFactories();
            if (factories == null || factories.Count == 0) return;

            if (useButtons)
                BuildButtons(factories);
            else
                BuildDropdown(factories);

            // Mevcut fabrikayı göster
            UpdateDisplay(FactoryManager.Instance.CurrentFactory);
        }

        private void BuildDropdown(List<FactoryData> factories)
        {
            if (factoryDropdown == null) return;

            factoryDropdown.ClearOptions();
            var options = new List<string>();

            foreach (var factory in factories)
            {
                options.Add(factory.factoryName);
            }

            factoryDropdown.AddOptions(options);
            factoryDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void BuildButtons(List<FactoryData> factories)
        {
            if (buttonContainer == null || factoryButtonPrefab == null) return;

            foreach (var btn in _factoryButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            _factoryButtons.Clear();

            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                var btnObj = Instantiate(factoryButtonPrefab, buttonContainer);
                var btn = btnObj.GetComponent<Button>();
                var text = btnObj.GetComponentInChildren<TMP_Text>();

                if (text != null) text.text = factory.factoryName;

                int capturedIndex = i;
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnFactoryButtonClicked(capturedIndex));
                    _factoryButtons.Add(btn);
                }
            }
        }

        private void OnDropdownChanged(int index)
        {
            FactoryManager.Instance?.SwitchToFactory(index);
        }

        private void OnFactoryButtonClicked(int index)
        {
            FactoryManager.Instance?.SwitchToFactory(index);
        }

        private void OnFactoryChanged(FactoryData factory)
        {
            UpdateDisplay(factory);
        }

        private void UpdateDisplay(FactoryData factory)
        {
            if (factory == null) return;

            if (currentFactoryNameText != null)
                currentFactoryNameText.text = factory.factoryName;

            if (factoryLogo != null && factory.logo != null)
                factoryLogo.sprite = factory.logo;

            // Dropdown güncelle
            if (factoryDropdown != null)
            {
                var factories = FactoryManager.Instance?.GetAllFactories();
                if (factories != null)
                {
                    int index = factories.FindIndex(f => f.factoryId == factory.factoryId);
                    if (index >= 0) factoryDropdown.SetValueWithoutNotify(index);
                }
            }
        }
    }
}
