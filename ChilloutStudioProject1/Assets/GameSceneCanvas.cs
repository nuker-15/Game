using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneCanvas : MonoBehaviour
{
    public void OnHomeButtonClicked()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }
}
