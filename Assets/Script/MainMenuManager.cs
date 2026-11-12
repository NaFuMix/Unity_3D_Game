using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    [Header("Panels")]
    public GameObject howToPlayPanel;

    void Start()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                {
                    if (button.onClick.GetPersistentTarget(i) != this || button.targetGraphic == null)
                        continue;

                    Outline border = button.targetGraphic.GetComponent<Outline>();
                    if (border == null)
                        border = button.targetGraphic.gameObject.AddComponent<Outline>();

                    border.effectColor = new Color(0.85f, 0.69f, 0.78f, 0.65f);
                    border.effectDistance = new Vector2(0.7f, -0.7f);
                    break;
                }
            }
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}