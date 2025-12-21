using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Индекс сцены для загрузки (File -> Build Settings).")]
    public int sceneToLoadIndex = 2;

    // Ссылка на корневой элемент UI
    private VisualElement _rootElement;
    private ProgressBar _bar;
    private Label _label;

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        _rootElement = uiDoc.rootVisualElement;
        _bar = _rootElement.Q<ProgressBar>("loading-bar");
        _label = _rootElement.Q<Label>("progress-text");

        // Устанавливаем диапазон прогресс бара от 0 до 1
        _bar.lowValue = 0f;
        _bar.highValue = 1f;
        _bar.value = 0f;

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Начинаем загрузку сцены в фоне
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoadIndex);
        operation.allowSceneActivation = false;

        // Анимация загрузки в течение 1 секунды
        float animationDuration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            _bar.value = progress;
            _label.text = (progress * 100f).ToString("F0") + "%";
            
            yield return null;
        }
        
        // Устанавливаем 100% после анимации
        _bar.value = 1f;
        _label.text = "100%";
        
        // Небольшая задержка перед переходом
        yield return new WaitForSeconds(0.5f);

        // Ждем, пока сцена загрузится (до 90%)
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // Разрешаем активацию сцены
        operation.allowSceneActivation = true;
    }
}
