using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;

public class Weapon : NetworkBehaviour
{
    public float power = 500;
    public Transform tip;
    public GameObject hitObject;
    public Camera cam;
    public WeaponData[] weapons;
    public NetworkVariable<int> selectedWeapon = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
    public TMPro.TMP_Text selectedWeaponText;
    public GameObject scopeTexture;
    private RaycastHit hit;
    private bool scopedIn;
    private float cooldown;
    private WeaponData swd;
    private List<GameObject> weaponPrefabs;
    private float hipFireFOV;
    private float recoilSpeed = 38f;
    private Vector2 nextRecoilRot;
    private GameObject hitFx;
    private void Start()
    {
        weaponPrefabs = new List<GameObject>();
        for (int i = 0; i < weapons.Length; i++)
        {
            var w = Instantiate(weapons[i].weaponPrefab,transform);
            weaponPrefabs.Add(w);
        }
        hipFireFOV = cam.fieldOfView;
    }
    void Update()
    {
        foreach (var w in weaponPrefabs)
        {
            w.SetActive(false);
        }
        weaponPrefabs[selectedWeapon.Value].SetActive(true);
        tip = weaponPrefabs[selectedWeapon.Value].transform.Find("Tip");
        swd = weapons[selectedWeapon.Value];
        if (!transform.root.GetComponent<NetworkObject>().IsLocalPlayer) return;
        if (Input.GetButtonDown("Fire2"))
        {
            scopedIn = !scopedIn;
        }
        scopeTexture.SetActive(scopedIn && swd.hasScope);
        cam.fieldOfView = hipFireFOV / (scopedIn ? swd.zoom : 1);
        if (Input.GetButtonDown("Swap Weapon")) { selectedWeapon.Value += Mathf.RoundToInt(Input.GetAxis("Swap Weapon")); selectedWeapon.Value = Mathf.Clamp(selectedWeapon.Value, 0, weapons.Length - 1); }
        selectedWeaponText.text = swd.name;
        cam.GetComponent<PlayerCamera>().yAngle -= nextRecoilRot.y * Time.deltaTime * recoilSpeed;
        transform.root.localEulerAngles += new Vector3(0,nextRecoilRot.x * Time.deltaTime * recoilSpeed,0);
        nextRecoilRot = Vector2.Lerp(nextRecoilRot,Vector2.zero,Time.deltaTime * recoilSpeed);
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
        cam.GetComponent<PlayerCamera>().yAngle -= nextRecoilRot.y * Time.deltaTime * recoilSpeed;
        transform.root.eulerAngles += Vector3.up * nextRecoilRot.x * Time.deltaTime * recoilSpeed;
        nextRecoilRot = Vector2.Lerp(nextRecoilRot, Vector2.zero, Time.deltaTime * recoilSpeed);
        if ((swd.automatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1")) && cooldown <= 0)
        {
            
            var rbias = swd.recoilBias;
            nextRecoilRot = new Vector2(Random.Range(-swd.recoil, swd.recoil),Random.Range(-swd.recoil, swd.recoil)) + Vector2.one * rbias * swd.recoil;
            Fire(1000ul);
            FireServerRpc(OwnerClientId);
            cooldown = 60f / swd.firerate;

        }
    }
    [ServerRpc]
    void FireServerRpc(ulong senderId)
    {
        FireClientRpc();
        Fire(senderId);
        hitFx.GetComponent<NetworkObject>().Spawn();
        if (hit.transform.GetComponent<Health>())
        {
            hit.transform.GetComponent<Health>().health.Value -= swd.damage;
        }
    }
    [ClientRpc]
    void FireClientRpc()
    {
        var a = new GameObject().AddComponent<AudioSource>();
        a.AddComponent<NetworkObject>();
        a.clip = swd.audioClip;
        a.Play();
        Destroy(a.gameObject, 3);
    }
    [ClientRpc]
    void CheckHitFXClientRpc()
    {
        foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.name == OwnerClientId.ToString())
            {
                Destroy(obj);
            }
        }
    }
    void Fire(ulong id)
    {
        Physics.Raycast(tip.transform.position, tip.transform.forward, out hit, 1000);
        if (hit.transform == null) return;
        hitFx = Instantiate(hitObject, hit.point, Quaternion.identity);
        hitFx.name = id.ToString();
        hitFx.transform.position = hit.point;
        CheckHitFXClientRpc();
        Destroy(hitFx, 0.05f);
    }
    
}
public class NetworkTransformData : INetworkSerializable
{
    Transform t;

    public NetworkTransformData()
    {
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter { }

    public NetworkTransformData(Transform t)
    {
        this.t = t;
    }

    public Transform ReturnData()
    {
        return t;
    }
}

