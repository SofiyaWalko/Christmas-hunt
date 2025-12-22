using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }
    public bool IsModalOpen { get; private set; }

    private VisualElement inventoryPanel;
    private VisualElement slotsContainer;
    private VisualElement rewardSlotElement;
    private VisualElement statSlotElement;
    private PlayerControls playerControls;

    private ProgressBar healthBar;
    private Label healthText;
    private ProgressBar staminaBar;
    private Label staminaText;

    private VisualElement interactHint;
    private Label interactText;

    private CinemachineInputAxisController[] inputProviders;

    // Notification modal fields
    private VisualElement notificationOverlay;
    private Label notificationTitle;
    private Label notificationMessage;
    private Button notificationClose;
    private Button notificationOk;
    private Button notificationNextLevel;

    //----←
    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Gameplay.Inventory.performed += ToggleInventory;
        Instance = this;
    }

    private InventoryManager inventoryManagerRef;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        inventoryPanel = root.Q<VisualElement>("inventory-panel");
        slotsContainer = root.Q<VisualElement>("slots-container");
        rewardSlotElement = root.Q<VisualElement>("reward-slot");
        statSlotElement = root.Q<VisualElement>("stat-slot");
        healthBar = root.Q<ProgressBar>("health-bar");
        healthText = root.Q<Label>("health-text");
        staminaBar = root.Q<ProgressBar>("stamina-bar");
        staminaText = root.Q<Label>("stamina-text");

        // ← НОВОЕ: ПОДСКАЗКА
        interactHint = root.Q<VisualElement>("interact-hint");
        interactText = root.Q<Label>("interact-text");

        notificationOverlay = root.Q<VisualElement>("notification-overlay");
        notificationTitle = root.Q<Label>("notification-title");
        notificationMessage = root.Q<Label>("notification-message");
        notificationOk = root.Q<Button>("notification-ok");
        notificationNextLevel = root.Q<Button>("notification-next-level");

        if (notificationOk != null)
            notificationOk.clicked += () => HideNotification();

        InkDialogueManager.Instance.InitializeUI(root);
        PlayerController.OnInteractableFocusChanged += UpdateInteractHint; // подписались на событие по выводу подсказки
        // ---- ←
        playerControls.Gameplay.Enable();

        // Подписываемся на InventoryManager, ожидая его, если он ещё не создан
        if (InventoryManager.Instance != null)
        {
            inventoryManagerRef = InventoryManager.Instance;
            inventoryManagerRef.OnInventoryChanged += UpdateUI;
            CharacterStats.OnHealthChanged += UpdateHealthUI;
            CharacterStats.OnStaminaChanged += UpdateStaminaUI;
            UpdateUI();
        }
        else
        {
            StartCoroutine(WaitForInventoryManager());
        }

        inputProviders = FindObjectsOfType<CinemachineInputAxisController>();
    }

    private void Update()
    {
        bool shouldBlock = IsInputBlocked();
        if (inputProviders != null)
        {
            foreach (var provider in inputProviders)
            {
                if (provider != null)
                {
                    provider.enabled = !shouldBlock;
                }
            }
        }
    }

    private bool IsInputBlocked()
    {
        if (InkDialogueManager.Instance != null && InkDialogueManager.Instance.IsDialogueActive)
        {
            return true;
        }

        if (IsModalOpen)
        {
            return true;
        }

        if (inventoryPanel != null && inventoryPanel.style.display == DisplayStyle.Flex)
        {
            return true;
        }

        return false;
    }

    public void ShowNotification(
        string message,
        string title = "Уведомление",
        float autoHideSeconds = 0f
    )
    {
        IsModalOpen = true;
        // Lazy initialize notification elements if Start hasn't set them yet
        if (notificationOverlay == null)
        {
            var uiDoc = FindObjectOfType<UIDocument>();
            if (uiDoc != null)
            {
                var root = uiDoc.rootVisualElement;
                notificationOverlay = root.Q<VisualElement>("notification-overlay");
                notificationTitle = root.Q<Label>("notification-title");
                notificationMessage = root.Q<Label>("notification-message");
                notificationOk = root.Q<Button>("notification-ok");

                if (notificationOk != null)
                    notificationOk.clicked += () => HideNotification();
            }
        }

        if (notificationOverlay == null)
        {
            Debug.LogWarning("ShowNotification: notification overlay not found in UI.");
            return;
        }

        if (notificationTitle != null)
            notificationTitle.text = title;
        if (notificationMessage != null)
            notificationMessage.text = message;

        // // Add class for USS-driven show, and set inline display as a fallback
        notificationOverlay.AddToClassList("show");
        notificationOverlay.style.display = DisplayStyle.Flex;

        if (autoHideSeconds > 0f)
        {
            StartCoroutine(HideNotificationAfter(autoHideSeconds));
        }
    }

    public void ShowVictoryNotification(string message, string title, string nextSceneName)
    {
        ShowNotification(message, title);
        if (notificationOk != null)
        {
            notificationOk.text = "Следующий уровень";
            notificationOk.clicked += () => {
                // SaveManager.Instance.SaveGame(); // Auto-save moved to SaveManager OnSceneLoaded
                SceneManager.LoadScene(nextSceneName);
                HideNotification();
            };
        }
    }

    public void HideNotification()
    {
        IsModalOpen = false;
        if (notificationOverlay == null)
            return;

        notificationOverlay.RemoveFromClassList("show");
        notificationOverlay.style.display = DisplayStyle.None;
        
        if (notificationNextLevel != null)
        {
            notificationNextLevel.style.display = DisplayStyle.None;
            // Remove all listeners to avoid stacking
            notificationNextLevel.clicked -= null; 
        }
    }

    private System.Collections.IEnumerator HideNotificationAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HideNotification();
    }

    private void OnDisable()
    {
        playerControls.Gameplay.Disable();

        if (inventoryManagerRef != null)
            inventoryManagerRef.OnInventoryChanged -= UpdateUI;

        CharacterStats.OnHealthChanged -= UpdateHealthUI;
        CharacterStats.OnStaminaChanged -= UpdateStaminaUI;
    }

    private System.Collections.IEnumerator WaitForInventoryManager()
    {
        yield return new WaitUntil(() => InventoryManager.Instance != null);
        inventoryManagerRef = InventoryManager.Instance;
        inventoryManagerRef.OnInventoryChanged += UpdateUI;
        CharacterStats.OnHealthChanged += UpdateHealthUI;
        CharacterStats.OnStaminaChanged += UpdateStaminaUI;
        UpdateUI();
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthBar == null)
            return;

        healthBar.highValue = maxHealth;
        healthBar.value = currentHealth;

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    // ← НОВЫЙ МЕТОД: ОБНОВЛЕНИЕ ПОДСКАЗКИ ←
    private void UpdateInteractHint(string hint)
    {
        if (string.IsNullOrEmpty(hint))
        {
            interactHint.RemoveFromClassList("visible");
        }
        else
        {
            interactText.text = $"[E] {hint}";
            interactHint.AddToClassList("visible");
        }
    }

    //Новый метод
    private void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        if (staminaBar == null)
            return;

        staminaBar.highValue = maxStamina;
        staminaBar.value = currentStamina;

        if (staminaText != null)
            staminaText.text =
                $"{Mathf.RoundToInt(currentStamina)} / {Mathf.RoundToInt(maxStamina)}";
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        if (inventoryPanel == null)
            return;
        bool isVisible = inventoryPanel.style.display == DisplayStyle.Flex;
        inventoryPanel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void UpdateUI()
    {
        // Update UI called
        if (slotsContainer == null)
            return;
        slotsContainer.Clear();

        foreach (InventorySlot inventorySlot in InventoryManager.Instance.slots)
        {
            VisualElement slot = new VisualElement();
            slot.AddToClassList("inventory-slot");

            if (inventorySlot.item.icon != null)
                slot.style.backgroundImage = new StyleBackground(inventorySlot.item.icon.texture);

            if (inventorySlot.item.isStackable && inventorySlot.quantity > 0)
            {
                Label quantityLabel = new Label(inventorySlot.quantity.ToString());
                quantityLabel.AddToClassList("slot-quantity-label");
                slot.Add(quantityLabel);
            }

            slotsContainer.Add(slot);
        }

        // Обновляем специальный слот reward (показывается всегда, даже если пустой)
        if (rewardSlotElement != null)
        {
            rewardSlotElement.Clear();

            if (InventoryManager.Instance != null && InventoryManager.Instance.HasReward())
            {
                var rs = InventoryManager.Instance.rewardSlot;
                if (rs.item != null && rs.item.icon != null)
                {
                    rewardSlotElement.style.backgroundImage = new StyleBackground(
                        rs.item.icon.texture
                    );
                }
                else
                {
                    rewardSlotElement.style.backgroundImage = new StyleBackground();
                }

                // Всегда показываем количество (даже 0)
                Label rq = new Label(rs.quantity.ToString());
                rq.AddToClassList("slot-quantity-label");
                rewardSlotElement.Add(rq);
            }
            else
            {
                // Пустой слот — можно показать прозрачный фон или иконку-заглушку
                rewardSlotElement.style.backgroundImage = new StyleBackground();
            }
            // reward slot updated
        }

        // Обновляем специальный слот stat (показывается всегда)
        if (statSlotElement != null)
        {
            statSlotElement.Clear();

            if (InventoryManager.Instance != null && InventoryManager.Instance.HasStat())
            {
                var ss = InventoryManager.Instance.statSlot;
                if (ss.item != null && ss.item.icon != null)
                {
                    statSlotElement.style.backgroundImage = new StyleBackground(
                        ss.item.icon.texture
                    );
                }
                else
                {
                    statSlotElement.style.backgroundImage = new StyleBackground();
                }

                // Всегда показываем количество (даже 0)
                Label sq = new Label(ss.quantity.ToString());
                sq.AddToClassList("slot-quantity-label");
                statSlotElement.Add(sq);
            }
            else
            {
                statSlotElement.style.backgroundImage = new StyleBackground();
            }
            // stat slot updated
        }
    }
}
