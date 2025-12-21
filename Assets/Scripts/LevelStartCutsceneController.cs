using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class LevelStartCutsceneController : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [Tooltip("Ссылка на PlayableDirector, который проигрывает катсцену.")]
    public PlayableDirector cutsceneDirector;

    [Tooltip("Камера, которая используется во время катсцены.")]
    public Camera cutsceneCamera;

    [Header("Gameplay Settings")]
    [Tooltip("Камера персонажа, на которую нужно переключиться после катсцены.")]
    public Camera characterCamera;

    [Tooltip("Объект игрока (Character Controller), чтобы отключить управление во время катсцены.")]
    public GameObject playerObject;

    [Tooltip("UIDocument с интерфейсом игры (HUD), чтобы скрыть его во время катсцены.")]
    public UIDocument gameHUD;

    void Start()
    {
        // Если загружаемся из сохранения, не проигрываем катсцену
        if (SaveManager.Instance != null && SaveManager.Instance.isLoadingFromSave)
        {
            // Убедимся, что камеры и игрок в правильном состоянии
            if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(false);
            if (characterCamera != null) characterCamera.gameObject.SetActive(true);
            if (gameHUD != null) gameHUD.rootVisualElement.style.display = DisplayStyle.Flex;
            if (playerObject != null)
            {
                var controller = playerObject.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = true;

                var playerScript = playerObject.GetComponent<PlayerController>();
                if (playerScript != null) playerScript.enabled = true;
            }
            return;
        }

        StartCoroutine(PlayCutsceneRoutine());
    }

    IEnumerator PlayCutsceneRoutine()
    {
        // 1. Подготовка: отключаем игрока и включаем камеру катсцены
        if (playerObject != null) 
        {
            // Если на игроке висит скрипт управления, лучше отключать его, а не весь объект
            // Но для простоты можно отключить объект, если это не ломает логику
            // playerObject.SetActive(false); 
            
            // Лучше отключить компонент управления (например, CharacterController или ваш скрипт движения)
            var controller = playerObject.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            // Установим Animator в состояние "на земле" и вернёмся в Locomotion перед отключением управления,
            // чтобы избежать проигрывания анимации падения во время катсцены.
            var animator = playerObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("IsGrounded", true);
                animator.Play("Locomotion");
            }

            // Отключаем скрипт управления игроком, чтобы он не пытался двигать контроллер
            var playerScript = playerObject.GetComponent<PlayerController>();
            if (playerScript != null) playerScript.enabled = false;
        }

        if (gameHUD != null) gameHUD.rootVisualElement.style.display = DisplayStyle.None;

        if (characterCamera != null) characterCamera.gameObject.SetActive(false);
        if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(true);

        // 2. Запуск катсцены
        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();

            // Ждем пару кадров для обновления состояния
            yield return null;
            yield return null;

            // Ждем окончания проигрывания
            while (cutsceneDirector.state == PlayState.Playing)
            {
                yield return null;
            }
        }

        // 3. Завершение: включаем игрока и его камеру
        if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(false);
        if (characterCamera != null) characterCamera.gameObject.SetActive(true);

        if (gameHUD != null) gameHUD.rootVisualElement.style.display = DisplayStyle.Flex;

        if (playerObject != null)
        {
            // playerObject.SetActive(true);
            var controller = playerObject.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = true;
            
            var playerScript = playerObject.GetComponent<PlayerController>();
            if (playerScript != null) playerScript.enabled = true;
        }
        
        // Опционально: уничтожаем этот объект или отключаем скрипт, чтобы не мешал
        // Destroy(gameObject); 
    }
}
