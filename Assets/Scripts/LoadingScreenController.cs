using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadingScreenController : MonoBehaviour
{
    // Укажите в инспекторе индекс сцены, которую нужно загрузить (например, 2)
    public int sceneToLoadIndex = 1;

    private ProgressBar _bar;
    private Label _label;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _bar = root.Q<ProgressBar>("loading-bar");
        _label = root.Q<Label>("progress-text");

        // Устанавливаем диапазон прогресс бара от 0 до 1
        _bar.lowValue = 0f;
        _bar.highValue = 1f;
        _bar.value = 0f;

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
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
        
        // Небольшая задержка перед загрузкой сцены
        yield return new WaitForSeconds(0.2f);

        // Теперь загружаем сцену
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoadIndex);

        while (!operation.isDone)
        {
            yield return null; // Ждем следующего кадра
        }
    }
}
