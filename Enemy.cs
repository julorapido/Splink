using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header ("Animator")]
    [SerializeField] private Animator am_;


    [Header ("Stats")]
    [SerializeField, Range(20, 90)] private int e_movementSpeed;
    [SerializeField, Range(20, 90)] private int e_range;
    [SerializeField, Range(50, 600)] private int e_health;
    [SerializeField, Range(10, 40)] private int e_damage;
    [SerializeField, Range(0.01f, 3f)] private float e_fireRate;

    private enum IA_Type
    {
        Dashes,
        Stationary,
        Jumps
    };
    IA_Type weapon_type = new IA_Type();

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
