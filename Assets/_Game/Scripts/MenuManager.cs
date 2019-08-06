using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private string menuSceneName = "";
    [SerializeField] private string gameSceneName = "";
    [SerializeField] private GameObject player = default;

    //******************//
    //    Properties    //
    //******************//

    public static MenuManager Instance { get; private set; }

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }
    }

    private void Start()
    {
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Additive);
    }

    private IEnumerator LoadGameCoroutine()
    {
        SceneManager.UnloadSceneAsync(menuSceneName);

        AsyncOperation loadSceneAsync = null;
        if (!SceneManager.GetSceneByName(gameSceneName).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);
        }

        while (loadSceneAsync != null && !loadSceneAsync.isDone) {
            Debug.LogFormat("Loading game scene: {0}%", loadSceneAsync.progress * 100);
            yield return null;
        }

        AreaController controller = FindObjectOfType<AreaController>();
        if (controller) {
            controller.InitializeArea(player, 0);
        } else {
            Debug.LogErrorFormat("{0}: Could not find AreaController!", name);
        }
        player.SetActive(true);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void LoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }
}
