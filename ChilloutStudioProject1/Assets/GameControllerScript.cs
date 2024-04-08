using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameControllerScript : NetworkBehaviour
{
    public static GameControllerScript instance;

    public GameObject waitingForPlayersPanel;
    public GameObject highScorePanel;

    public int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    public GameState gameState;

    public Dictionary<int, int> scores = new Dictionary<int, int>();

    public List<NetworkObject> toDespawn;

    private void Start()
    {
        if(instance == null)
            instance = this;

        gameState = GameState.waiting;

        if(StaticController.isServer)
        { 
            NetworkManager.Singleton.StartServer();
            StartCoroutine(Server_WaitingForPlayers());
            print("startServer");
        }
        else
        { 
            NetworkManager.Singleton.StartClient();
            StartCoroutine(Client_WaitingForPlayers());
            print("clientStarted");
        }
    }

    IEnumerator Server_WaitingForPlayers()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count >= 4);
        gameState = GameState.started;
        print("calledClientRPC");
        SetGameStateClientRPC(gameState);
    }

    [ClientRpc]
    void SetGameStateClientRPC(GameState _gameState)
    {
        print("receivedClientRPC");
        gameState = _gameState;
    }

    IEnumerator Client_WaitingForPlayers()
    {
        waitingForPlayersPanel.SetActive(true);
        score = 0;
        
        yield return new WaitUntil(() => gameState == GameState.started);
        print("gameState: started");
        waitingForPlayersPanel.SetActive(false);
    }
    //IEnumerator WaitingForPlayers()
    //{
    //    if (IsClient)
    //    {
    //        waitingForPlayersPanel.SetActive(true);
    //        score = 0;
    //    }
    //    yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count <= 2);
    //    gameState = GameState.started;
    //    if (IsClient)
    //    { 
    //        waitingForPlayersPanel.SetActive(false); 
    //    }

    //}


    public IEnumerator WaitForGameOver()
    {
        yield return new WaitUntil(() => CollectablesManager.instance.numCollectableItems <= 0);
        gameState = GameState.ended;
        GameOver();
    }

    void GameOver()
    {
        //NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerController>().
        SendScoreClientRPC();
        StartCoroutine(WaitToDisplayHighScore());
        //CalculateAndSendScores();
    }

    [ClientRpc]
    public void SendScoreClientRPC()
    {
        AddScoresServerRPC((int)(int)NetworkManager.Singleton.LocalClientId, score);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoresServerRPC(int id, int score)
    {
        scores.Add(id, score);
    }

    IEnumerator WaitToDisplayHighScore()
    {
        yield return new WaitUntil(() => scores.Count >= NetworkManager.Singleton.ConnectedClients.Count);
        
        DisplayHighScoreClientRPC(HighestScore(scores));
        gameState = GameState.ended;
        DespawnPlayersAndNetworkObjects();
    }

    void DespawnPlayersAndNetworkObjects()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
        {
            NetworkManager.Singleton.ConnectedClients[(ulong)i].PlayerObject.Despawn();
        }
        foreach(var item in toDespawn)
        {
            item.Despawn();
        }
    }

    [ClientRpc]
    void DisplayHighScoreClientRPC(string _highscoreText)
    {
        highScorePanel.SetActive(true);
        highScoreText.text = _highscoreText;
    }

    private void Update()
    {
        scoreText.text = "Score: " + score;
    }

    string HighestScore(Dictionary<int, int> _scores)
    {
        int highestScore = 0;
        int id = 0;
        foreach (var item in _scores.Keys)
        {
            if(highestScore < _scores[item])
            {
                highestScore = _scores[item];
                id = item;
            }
        }
        return playerColors[id] + "is the winner \n" + "Score: " + highestScore;
    }

    public Dictionary<int, string> playerColors = new Dictionary<int, string>
    {
        { 0, "white "},
        { 1, "yellow "},
        { 2, "red "},
        { 3, "green "},
        { 4, "blue "}
    };
}
public enum GameState
{
    waiting,
    started,
    ended
}