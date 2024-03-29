using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class WeaponData : ScriptableObject
{
    public int damage;
    public int firerate;
    public bool automatic;
    public float recoil = 5;
    public Vector2 recoilBias = new Vector2(0,1);
    public GameObject weaponPrefab;
    public bool hasScope;
    public float zoom = 1.25f;
    public AudioClip audioClip;
}
