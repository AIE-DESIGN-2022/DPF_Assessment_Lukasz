using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Loading : MonoBehaviour
{
    private UI_Bar loadingBar;

    private void Awake()
    {
        loadingBar = GetComponentInChildren<UI_Bar>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsShowing())
        {
            ToggleShowing();
        }

        loadingBar.UpdatePercentage(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleShowing()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public bool IsShowing()
    {
        return gameObject.activeSelf;
    }

    public void Show(bool showUI = true)
    {
        if (!IsShowing() && showUI)
        {
            ToggleShowing();
        }
        else if (IsShowing() && !showUI)
        {
            ToggleShowing();
        }
    }

    public void LoadScene(int sceneId)
    {
        Show();
        StartCoroutine(LoadScreenAsync(sceneId));
    }

    private IEnumerator LoadScreenAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.UpdatePercentage(progressValue);
            yield return null;
        }
    }
}
