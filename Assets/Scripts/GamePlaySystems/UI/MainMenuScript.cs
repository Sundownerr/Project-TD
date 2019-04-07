using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuScript : ExtendedMonoBehaviour
{
    public Button QuitButton, NewGameButton;

    protected override void Awake()
    {
        base.Awake();

        QuitButton.onClick.AddListener(QuitClick);
        NewGameButton.onClick.AddListener(NewGameClick);
    }
	
    void QuitClick()
    {
        Application.Quit();
    }

    void NewGameClick()
    {
        SceneManager.LoadScene("Game");
    }
}
