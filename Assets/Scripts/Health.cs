using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    public int maxHealth = 100;
    public bool respawn;
    public List<Transform> respawnPoints;
    public NetworkVariable<int> health = new NetworkVariable<int>();
    private void Start()
    {
        SetHealthServerRpc(0);
        foreach (var point in GameObject.FindGameObjectsWithTag("Respawn"))
        {
            respawnPoints.Add(point.transform);
        }
    }
    void LateUpdate()
    {
        if (!IsOwner) return;
        if (transform.position.y <= -20)
        {
            SetHealthServerRpc(0);
        }
        if (health.Value <= 0)
        {
            if (respawn)
            {
                transform.position = respawnPoints[Random.Range(0,respawnPoints.Count)].position;
                SetHealthServerRpc(maxHealth);
            }
        }
    }
    [ServerRpc]
    void SetHealthServerRpc(int amount)
    {
        health.Value = amount;
    }
}
