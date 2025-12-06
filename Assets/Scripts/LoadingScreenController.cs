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

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoadIndex);

        while (!operation.isDone)
        {
            // operation.progress изменяется от 0.0 до 0.9.
            // Чтобы получить значение от 0 до 1, делим на 0.9.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            _bar.value = progress;
            _label.text = (progress * 100f).ToString("F0") + "%";
            yield return null; // Ждем следующего кадра
        }
    }
}
