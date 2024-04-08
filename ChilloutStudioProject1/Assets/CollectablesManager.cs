using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CollectablesManager : NetworkBehaviour
{
    public int numCollectableItems = 0;
    [SerializeField] GameObject p_collectableItem;

    public static CollectablesManager instance;

    private void Start()
    {
        if(instance == null)
        { 
            CollectablesManager.instance = this; 
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        base.OnNetworkSpawn();

        StartCoroutine(WaitToSpawnCollectableItems());

    }

    IEnumerator WaitToSpawnCollectableItems()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count > 0);
        SpawnCollectableItems();
        StartCoroutine(WaitForGameOver());
    }
    IEnumerator WaitForGameOver()
    {
        yield return new WaitUntil(() => GameControllerScript.instance != null);
        StartCoroutine(GameControllerScript.instance.WaitForGameOver());
    }
    [ServerRpc(RequireOwnership = false)]
    public void CollectedServerRPC()
    {
        numCollectableItems -= 1;
    }

    private GameObject collectableItem;
    private float offsetRadius = 5f;
    private void SpawnCollectableItems()
    {
        SpawnCircle(20);
        numCollectableItems += 20;
    }

    void DefaultSpawnFunction()
    {
        collectableItem = Instantiate(p_collectableItem, this.transform);
        collectableItem.transform.position = transform.position + Vector3.forward * offsetRadius;
        collectableItem = Instantiate(p_collectableItem, this.transform);
        collectableItem.transform.position = transform.position + Vector3.back * offsetRadius;
        collectableItem = Instantiate(p_collectableItem, this.transform);
        collectableItem.transform.position = transform.position + Vector3.left * offsetRadius;
        collectableItem = Instantiate(p_collectableItem, this.transform);
        collectableItem.transform.position = transform.position + Vector3.right * offsetRadius;
    }

    void SpawnCircle( int numOfCollectables)
    {
        float angle = 0;
        Vector3 positionVector = Vector3.forward;
        for(int i = 0; i< numOfCollectables; i++)
        {
            angle += (360 / numOfCollectables);
            positionVector = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            collectableItem = Instantiate(p_collectableItem, this.transform);
            collectableItem.transform.position = transform.position + positionVector * offsetRadius;
            var instanceNetworkObject = collectableItem.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn();
        }

    }


}
