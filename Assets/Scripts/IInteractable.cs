
public interface IInteractable
{
    // Метод, который будет вызываться при взаимодействии
    void Interact();
    
    // Метод, который вернет строку для отображения подсказки (например, "Нажать Е, чтобы Открыть")
    string GetInteractText();
    
}

