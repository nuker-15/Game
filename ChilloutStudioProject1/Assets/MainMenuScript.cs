using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void OnClickPlayButton(bool isServer)
    {
        StaticController.isServer = isServer;
        SceneManager.LoadScene(1);
    }
}
