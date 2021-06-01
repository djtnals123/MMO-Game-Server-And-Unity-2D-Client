using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;
    public static bool IsLoading { get; set; } = true;

    [SerializeField]
    Image progressBar;

    private void Start() => StartCoroutine(LoadScene());

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        InitObject.Player.IsLoading = true;
        IsLoading = true;
        SceneManager.LoadScene("Loading", LoadSceneMode.Additive);
    }

    IEnumerator LoadScene()
    {
        yield return null;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main"));
        SceneManager.UnloadSceneAsync(MainScene.CurrentSceneName);
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        op.allowSceneActivation = false;
        float timer = 0.0f;

        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                if (progressBar.fillAmount >= op.progress) timer = 0f;
            }
            else
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                if (progressBar.fillAmount == 1.0f)
                {
                    op.allowSceneActivation = true;
                    SceneManager.UnloadSceneAsync("Loading");
                    yield break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        InitObject.Player.IsLoading = false;
        MainScene.CurrentSceneName = nextScene;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextScene));
        IsLoading = false;
        PacketManager.Instance.CallRequestObjects();
    }
}