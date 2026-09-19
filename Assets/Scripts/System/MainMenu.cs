using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuButtonGroup;

    [Header("Level Select")]
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Scenes")]
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string arenaSceneName = "Arena";

    private const string SelectedLevelKey = "SelectedLevel";

    private void Start()
    {
        Time.timeScale = 1f;

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }

        if (mainMenuButtonGroup != null)
        {
            mainMenuButtonGroup.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (
                levelSelectPanel != null &&
                levelSelectPanel.activeSelf
            )
            {
                CloseLevelSelect();
            }
        }
    }

    // =========================
    // PLAY
    // =========================

    public void Play()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            characterSelectSceneName
        );
    }

    // =========================
    // LEVEL
    // =========================

    public void OpenLevelSelect()
    {
        if (mainMenuButtonGroup != null)
        {
            mainMenuButtonGroup.SetActive(false);
        }

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(true);
        }
    }

    public void CloseLevelSelect()
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }

        if (mainMenuButtonGroup != null)
        {
            mainMenuButtonGroup.SetActive(true);
        }
    }

    // =========================
    // LEVEL SELECTION
    // =========================

    public void SelectGameLevel()
    {
        PlayerPrefs.SetString(
            SelectedLevelKey,
            gameSceneName
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Selected Level: " +
            gameSceneName
        );

        CloseLevelSelect();
    }

    public void SelectArenaLevel()
    {
        PlayerPrefs.SetString(
            SelectedLevelKey,
            arenaSceneName
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Selected Level: " +
            arenaSceneName
        );

        CloseLevelSelect();
    }

    // =========================
    // GET SELECTED LEVEL
    // =========================

    public static string GetSelectedLevel()
    {
        return PlayerPrefs.GetString(
            SelectedLevelKey,
            "Game"
        );
    }

    // =========================
    // QUIT
    // =========================

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}