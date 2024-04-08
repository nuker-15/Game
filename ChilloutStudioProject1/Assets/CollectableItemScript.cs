using Unity.Netcode;
using UnityEngine;

public class CollectableItemScript : NetworkBehaviour
{
    private Vector3 rotationVector = new Vector3(15, 30, 45);

    private void Update()
    {
        transform.Rotate(rotationVector * Time.deltaTime);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TransmitBubbleDestroyStateToServerRpc()
    {
        Destroy(gameObject);
    }
}
