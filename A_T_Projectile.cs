using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_T_Projectile : MonoBehaviour
{
    
    /// <summary>
    /// An impassable tile is one which does not allow the player to move through
    /// it at all. It is completely solid.
    /// </summary>

    [Header ("Player_")]
    [HideInInspector] public Transform plyr_trnsfrm;

    [HideInInspector] public enum turret_Type{
        Normal,
        Double,
        Catapult,
        Heavy,
        Sniper,
        Gattling
    };
    [SerializeField]
    public turret_Type blt_type;

    [Header ("Attached Objects")]
    [HideInInspector] public Transform plyr_target;
    //[HideInInspector] public Transform prnt_turret;
    private MeshRenderer[] bullet_msh;
    private Rigidbody bullet_04_rb;
    private Vector3 qtrn_blt04;

    [Header ("Projectile_Speeds")]
    private float speed = 18f;
    private float turnSpeed = 14f;
    private bool plyr_passed = false;

    private Vector3 l_dir;
    private int z_ = 0;

    // Start is called before the first frame update
    private void Start()
    {
        bullet_msh  = gameObject.GetComponentsInChildren<MeshRenderer>();
        bullet_04_rb = gameObject.GetComponent<Rigidbody>();

        qtrn_blt04 = Vector3.RotateTowards(transform.forward, (plyr_target.position - transform.position), Time.deltaTime * 100, 0.0f);

        LeanTween.scale(gameObject, transform.localScale * 0.5f, 1f).setEaseInCubic();
    }

    // Update is called once per frame
    private void Update()
    {
        //gameObject.transform.LookAt(plyr_trnsfrm);
        float dst_ = Vector3.Distance(transform.position, plyr_target.position);
        if( (dst_ < 8) && !plyr_passed) {plyr_passed = true; speed *= 1.3f;}

        try{
            Vector3 dir = (plyr_target.position + new Vector3(0f, 1f, 0f) ) - transform.position;
            if(!plyr_passed) l_dir = dir;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * turnSpeed, 0.0f);
            switch(blt_type){
                case turret_Type.Normal:
                case turret_Type.Double:
                    //Debug.DrawRay(transform.position, newDirection, Color.red);

                    transform.Translate(Vector3.forward * Time.deltaTime * speed);
                    transform.rotation = Quaternion.LookRotation(plyr_passed ? l_dir : newDirection);
                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, z_);

                    z_+= 3;
                    break;

                case turret_Type.Heavy:
                case turret_Type.Sniper:
                case turret_Type.Gattling:
                    Vector3 shoot_dir = dir.normalized;

                    // transform.rotation = Quaternion.LookRotation(qtrn_blt04); // Quaternion.Euler(qtrn_blt04.x, qtrn_blt04.y, qtrn_blt04.z);
                    //transform.LookAt(plyr_target);
                    transform.rotation = Quaternion.LookRotation(qtrn_blt04);
                    bullet_04_rb.AddForce(9 * shoot_dir, ForceMode.VelocityChange);

                    break;
                case turret_Type.Catapult:
                    Vector3 p = CalculateCatapult(plyr_target.position, transform.position, 1f);
                    break;
            }

            if( (transform.position.z - plyr_target.position.z ) < 4f) bullet_explode();

        }catch(Exception err){
            Debug.Log(err);
            Debug.Log("No turret bullet type");
        }
    }

    private void FixedUpdate(){
        transform.Rotate(0,0,10f, Space.Self);
    }


    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "plaer_hitbx"){
            Debug.Log("zer");
            bullet_explode();
        } 
        if(other.gameObject.tag == "ground") bullet_explode();
    }

    private void bullet_explode(){
        for (int i = 0; i < bullet_msh.Length; i ++){
            bullet_msh[i].enabled = false;
        }

    }

    private Vector3 CalculateCatapult(Vector3 target, Vector3 origen, float time)
    {
        Vector3 distance = target - origen;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }
}
