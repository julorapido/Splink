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
    [HideInInspector] public Transform plyr_target;

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
    private MeshRenderer[] bullet_msh;
    private LineRenderer[] bullet_lr;
    private Collider[] bullet_cldrs;

    private ParticleSystem[] bullet_explosions_;

    private Rigidbody bullet_04_rb;
    private Vector3 qtrn_blt04;

    [Header ("Projectile_Speeds")]
    private float speed = 20f;
    private float turnSpeed = 14f;


    [Header ("Stored directions & rotations")]
    private Vector3 l_dir;
    private int z_ = 0;

    [Header ("Player Passed Bools")]
    private bool is_behind = false;
    private bool plyr_passed = false;

    [Header ("ExplosionAnimation Position")]
    private Vector3 expl_offset;
    private bool exploded = false;

    // Start is called before the first frame update
    private void Start()
    {
        ParticleSystem[] ps_arr = gameObject.GetComponentsInChildren<ParticleSystem>();
        bullet_explosions_ = ps_arr;
        
        bullet_msh  = gameObject.GetComponentsInChildren<MeshRenderer>();
        bullet_lr = gameObject.GetComponentsInChildren<LineRenderer>();
        bullet_cldrs = gameObject.GetComponentsInChildren<Collider>();


        bullet_04_rb = gameObject.GetComponent<Rigidbody>();


        Vector3 dir = (plyr_target.position) - transform.position;
        if(dir.z < 0 ) is_behind = true;

        qtrn_blt04 = Vector3.RotateTowards(transform.forward, (plyr_target.position - transform.position), Time.deltaTime * 100, 0.0f);

        LeanTween.scale(gameObject, transform.localScale * 0.5f, 1f).setEaseInCubic();
    }

    // Update is called once per frame
    private void Update()
    {
        float dst_ = Vector3.Distance(transform.position, plyr_target.position);
        if( (dst_ < 3) && !plyr_passed) {plyr_passed = true; speed *= 1.3f;}

        if(!exploded)
        {
            try{
                Vector3 dir = (plyr_target.position + new Vector3(0f, 1f, 0f) ) - transform.position;
                if(!plyr_passed) l_dir = dir;
                if(plyr_passed &&  (dst_ > 40) && !exploded) bullet_explode();

                Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * turnSpeed, 0.0f);
                switch(blt_type)
                {
                    case turret_Type.Normal:
                    case turret_Type.Double:
                        //Debug.DrawRay(transform.position, newDirection, Color.red);

                        transform.Translate(Vector3.forward * Time.deltaTime * speed);
                        Vector3 l_r  = plyr_passed ? l_dir : newDirection;
                        if( l_r != new Vector3(0, 0, 0) ){
                            transform.rotation = Quaternion.LookRotation(plyr_passed ? l_dir : newDirection);
                        }
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
                        bullet_04_rb.AddForce(2 * shoot_dir, ForceMode.VelocityChange);

                        break;
                    case turret_Type.Catapult:
                        Vector3 p = CalculateCatapult(plyr_target.position, transform.position, 1f);
                        break;
                }

                //if( (transform.position.z - plyr_target.position.z ) < 4f) bullet_explode();

            }catch(Exception err){
                Debug.Log(err);
            }
        }else
        {
            transform.position = plyr_target.position + expl_offset;

            Vector3 dir = (plyr_target.position + new Vector3(0f, 1f, 0f) ) - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * turnSpeed, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

    }

    private void FixedUpdate()
    {
        transform.Rotate(0,0,10f, Space.Self);
    }

    // private void OnCollisionEnter(Collision other)
    // {
    //     if(other.collider.gameObject.tag == "player_hitbx")
    //     {
    //         Debug.Log("player boum!");
    //         bullet_explode();
    //     } 
    //     if(other.collider.gameObject.tag == "ground") bullet_explode();
    // }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "player_hitbx" && !exploded)
        {
            bullet_explode();
        } 
        if(other.gameObject.tag == "ground" && !exploded) bullet_explode();
    }

    private void bullet_explode()
    {
        // Destroy(gameObject);
        exploded = true;
        expl_offset = ( transform.position - plyr_target.transform.position );


        for (int i = 0; i < bullet_msh.Length; i ++) bullet_msh[i].enabled = false;
        for (int j = 0; j < bullet_lr.Length; j ++) bullet_lr[j].enabled = false;
        for (int k = 0; k < bullet_cldrs.Length; k ++) bullet_cldrs[k].enabled = false;

        if(bullet_explosions_.Length > 0)
        {
            for (int l = 0; l < bullet_explosions_.Length; l ++)
                bullet_explosions_[l].Play();
        }
   
        // if (explosion_) explosion_.Play();

        Invoke("destry", 1f);
    }
    private void destry(){ Destroy(gameObject); }


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
