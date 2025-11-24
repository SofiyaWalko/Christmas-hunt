using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InventoryUIController : MonoBehaviour
{
    private VisualElement inventoryPanel;
    private VisualElement slotsContainer;
    private PlayerControls playerControls;

    private ProgressBar healthBar;
    private Label healthText;
    private ProgressBar staminaBar;
    private Label staminaText;

    // ---← НОВОЕ: ПОДСКАЗКА 
    private VisualElement interactHint;
    private Label interactText;
    //----←
    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Gameplay.Inventory.performed += ToggleInventory;
    }

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        inventoryPanel = root.Q<VisualElement>("inventory-panel");
        slotsContainer = root.Q<VisualElement>("slots-container");
        healthBar = root.Q<ProgressBar>("health-bar");
        healthText = root.Q<Label>("health-text");
        staminaBar = root.Q<ProgressBar>("stamina-bar");
        staminaText = root.Q<Label>("stamina-text");

        // ← НОВОЕ: ПОДСКАЗКА
        interactHint = root.Q<VisualElement>("interact-hint");
        interactText = root.Q<Label>("interact-text");
        //  ←
        // ← ИНИЦИАЛИЗАЦИЯ ДИАЛОГА ←
        DialogueManager.Instance.Initialize(root); //так как у нас два вида диалогов я один закомментировал, который ниже, если нужно будет показать, комментируешь этот и разкомментируешь что ниже
        //InkDialogueManager.Instance.InitializeUI(root);
        // ← ПОДПИСКА НА СОБЫТИЕ ИЗ PlayerController 
        PlayerController.OnInteractableFocusChanged += UpdateInteractHint;// подписались на событие по выводу подсказки
        // ---- ←
        playerControls.Gameplay.Enable();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateUI;
            CharacterStats.OnHealthChanged += UpdateHealthUI;
            CharacterStats.OnStaminaChanged += UpdateStaminaUI;
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        playerControls.Gameplay.Disable();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= UpdateUI;

        CharacterStats.OnHealthChanged -= UpdateHealthUI;
        CharacterStats.OnStaminaChanged -= UpdateStaminaUI;
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthBar == null) return;

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
        if (staminaBar == null) return;

        staminaBar.highValue = maxStamina;
        staminaBar.value = currentStamina;

        if (staminaText != null)
            staminaText.text = $"{Mathf.RoundToInt(currentStamina)} / {Mathf.RoundToInt(maxStamina)}";
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        if (inventoryPanel == null) return;
        bool isVisible = inventoryPanel.style.display == DisplayStyle.Flex;
        inventoryPanel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void UpdateUI()
    {
        if (slotsContainer == null) return;
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
    }
}
