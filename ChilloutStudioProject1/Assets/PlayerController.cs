using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 3;


    public Dictionary<int, Color> playerColors = new Dictionary<int, Color>
    {
        { 0, Color.white },
        { 1, Color.yellow },
        { 2, Color.red },
        { 3, Color.green },
        { 4, Color.blue }
    };



    private Transform mainCameraTransform;
    public int playerID = 0;

    private float horizontalInput = 0;
    private float verticalInput = 0;

    private void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }
    private void Initialize()
    {
        //playerID = (int)NetworkManager.Singleton.LocalClientId;
        SetColor();
        SetInitialTransform();
    }

    private void SetColor()
    {
        playerID = (int)NetworkObject.OwnerClientId;
        Material material = new Material(Shader.Find("Standard"));
        material.color = playerColors[playerID];
        GetComponent<MeshRenderer>().material = material;
    }

    private void SetInitialTransform()
    {
        if (!IsOwner) return;
        int id = (int)NetworkManager.Singleton.LocalClientId;
        rb.isKinematic = true;
        Vector3 backLeft = new Vector3(-10, 0.5f, -10);
        transform.position = backLeft + (new Vector3(1, 0, 1) * (id-1) * 5);
        rb.isKinematic = false;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) {
            return; }
        Move();
    }

    private void Move()
    {

        if (!Application.isFocused) { return; } // added to make working with clone editors easier

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        rb.AddForce(new Vector3(horizontalInput, 0, verticalInput) * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner && other.tag == "CollectableItem")
        {
            //CollectablesManager.instance.numCollectableItems -= 1;
            CollectablesManager.instance.CollectedServerRPC();
            GameControllerScript.instance.score += 1;
            other.gameObject.GetComponent<CollectableItemScript>().TransmitBubbleDestroyStateToServerRpc();
            //Destroy(other.gameObject);
            //SetActive(false);
        }
    }

    private Vector3 offset = new Vector3(0, 9.5f, -10);

    private void LateUpdate()
    {
        if(!IsOwner) { return; }
        mainCameraTransform.position = transform.position + offset;
    }
}
