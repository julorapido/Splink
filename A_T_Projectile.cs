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
    [Header ("Weapon Bullet Specifics")]
    [HideInInspector] public bool horitzontal_target;
    [HideInInspector] public Vector3 weapon_precision;
    [HideInInspector] public float weapon_dmg;
    [HideInInspector] public Transform player_;
    [HideInInspector] public bool target_isLeft;
    [HideInInspector] public bool is_crticial;
    private ParticleSystem blt_expl;

    [Header ("Player_")]
    [HideInInspector] public Transform plyr_target;

    [HideInInspector] public enum turret_Type{
        Normal,
        Double,
        Catapult,
        Heavy,
        Sniper,
        Gattling,
        Military,
        Blast,
        Robot,
        WeaponBullet
    };
    [SerializeField]
    public turret_Type blt_type;

    [Header ("Rocket Near-Inprecision")]
    private float near_inPrecision = 0.0f;
    
    [Header ("Attached Objects")]
    private MeshRenderer[] bullet_msh;
    private LineRenderer[] bullet_lr;
    private Collider[] bullet_cldrs;

    private ParticleSystem[] bullet_explosions_;

    private Rigidbody bullet_rb;
    private Vector3 bullet_qtrn;

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


    [Header ("ExplosionAnimation Position")]
    private const string turret_parts = "tr_Barrel tr_Stand tr_Plate tr_Radar tr_Shootp tr_BarrelHz";



    // Start is called before the first frame update
    private void Start()
    {
        if(blt_type != turret_Type.WeaponBullet)
        {
            ParticleSystem[] ps_arr = gameObject.GetComponentsInChildren<ParticleSystem>();
            bullet_explosions_ = ps_arr;
            
            bullet_msh  = gameObject.GetComponentsInChildren<MeshRenderer>();
            bullet_lr = gameObject.GetComponentsInChildren<LineRenderer>();
            bullet_cldrs = gameObject.GetComponentsInChildren<Collider>();




            float dir =  transform.position.z - (plyr_target.position.z);
            if(dir < 0 ) is_behind = true;


            LeanTween.scale(gameObject, transform.localScale * 0.5f, 1f).setEaseInCubic();
        }

        ParticleSystem[] ps_0 = gameObject.GetComponentsInChildren<ParticleSystem>();
        blt_expl = ps_0.Length > 0 ? ps_0[0] : null;

        bullet_rb = gameObject.GetComponent<Rigidbody>();
        bullet_qtrn = Vector3.RotateTowards(transform.forward, (plyr_target.position - transform.position), Time.deltaTime * 100, 0.0f);


        near_inPrecision = UnityEngine.Random.Range(0.5f, 4f);

        if(blt_type == turret_Type.Robot)
        {
            speed =  UnityEngine.Random.Range(14f, 22f);
        }

    }


    

    // Update is called once per frame
    private void Update()
    {
        if(blt_type != turret_Type.WeaponBullet)
        {
            float passedNear_inPrecision =  ( blt_type == turret_Type.Normal || blt_type == turret_Type.Double ?
                near_inPrecision : 0f
            );
            
            float dst_ = (transform.position.z - plyr_target.position.z);
            if( (is_behind ? (dst_ > passedNear_inPrecision) : (dst_ < passedNear_inPrecision) ) && !plyr_passed)
            {
                plyr_passed = true; speed *= 1.3f;
                Invoke("bullet_explode", 0.35f);
            }

            if(!exploded)
            {
                try{
                    Vector3 dir = (plyr_target.position + new Vector3(0f, 1f, 0f) ) - transform.position;

                    if(!plyr_passed)
                        l_dir = dir;
                        
                    if(plyr_passed &&  (dst_ > 12) && !exploded)
                        bullet_explode();

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
                        case turret_Type.Blast:
                        case turret_Type.Robot:
                            Vector3 shoot_dir = dir.normalized;

                            // transform.rotation = Quaternion.LookRotation(bullet_qtrn); // Quaternion.Euler(bullet_qtrn.x, bullet_qtrn.y, bullet_qtrn.z);
                            // transform.LookAt(plyr_target);
                            transform.rotation = Quaternion.LookRotation(bullet_qtrn);
                            bullet_rb.AddForce( (speed / 15) * shoot_dir, ForceMode.VelocityChange);

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
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * 10f, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirection);
            }


        }
        else
        {
            if(!exploded)
            {
                if( (plyr_target.position.z - transform.position.z) < -2f){ plyr_passed = true;}

                bool is_left =  ((plyr_target.rotation.eulerAngles.y >= 170 ) ? true : false);  
                is_left = target_isLeft;

                Vector3 dir = (plyr_passed == true) ? 
                    (l_dir) : 
                    (plyr_target.position +
                            (horitzontal_target ? new Vector3(is_left ? 2.0f : -3f, 2.5f, 0f) :
                             new Vector3(0f, 2.25f, 0f) )
                         - transform.position 
                    ) + (weapon_precision);

                if(!plyr_passed) l_dir = new Vector3(dir.x, 0, dir.z);

                Vector3 shoot_dir = dir.normalized;

                transform.rotation = Quaternion.LookRotation(bullet_qtrn);
                bullet_rb.AddForce((plyr_passed ? 10f : 6f) * shoot_dir, ForceMode.VelocityChange);

                if(plyr_passed)
                {
                    float dst = Vector3.Distance(transform.position, plyr_target.position);
                    if(dst > 40) destry();
                }
            }
            else
            {
                transform.position = plyr_target.position + expl_offset;
            }
        }
    }

    private void FixedUpdate()
    {
       transform.Rotate(0,0,10f, Space.Self);
    }


    private void OnCollisionEnter(Collision other)
    {
        if(blt_type == turret_Type.WeaponBullet)
        {
            GameObject member_gmObj_replaced = other.gameObject; // force initialization beacuse it's a GameObj
            string member_hit = other.gameObject.tag;
            Transform parent_ = other.gameObject.transform.parent;

            if( turret_parts.Contains(member_hit) )
            {
                // while(parent_ != null)
                for(int i = 0; i < 50; i ++)
                {
                    if(parent_.parent != null)
                    {
                        if(member_hit == "Untagged" && parent_.gameObject.tag != "Untagged")
                        {
                            member_hit = parent_.gameObject.tag;
                            member_gmObj_replaced = parent_.gameObject;
                        }

                        if(member_hit == "rocket"){
                            Debug.Log(parent_.gameObject);
                        }

                        parent_ = parent_.parent;
                        if( (parent_.gameObject.tag == "TURRET" || parent_.gameObject.tag == "ENEMY") && !exploded)
                        {
                            AutoTurret turret = parent_.GetComponent<AutoTurret>();
                            turret.turret_damage(
                                member_hit,
                                weapon_dmg, other.gameObject.tag != "Untagged" ?  other.gameObject : member_gmObj_replaced,
                                player_,
                                is_crticial
                            );
                            enemy_hit();
                            parent_ = null;
                            break;
                        }
                    }else
                    {
                        break;
                    }
                }
            }

       }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(blt_type != turret_Type.WeaponBullet)
        {
            if(other.gameObject.tag == "player_hitbx" && !exploded) bullet_explode();
            if(other.gameObject.tag == "ground" && !exploded) bullet_explode();
        }
    }

    // TURRETS AMMO EXPLODE
    private void bullet_explode()
    {
        // Destroy(gameObject);
        exploded = true;
        expl_offset = ( transform.position - plyr_target.transform.position );

        if(bullet_msh.Length > 0)
        {
            for (int i = 0; i < bullet_msh.Length; i ++)
            {
                bullet_msh[i].enabled = false;
            }
        }
      
        for (int j = 0; j < bullet_lr.Length; j ++) bullet_lr[j].enabled = false;
        for (int k = 0; k < bullet_cldrs.Length; k ++) bullet_cldrs[k].enabled = false;

        if(blt_type == turret_Type.Normal || blt_type == turret_Type.Double)
        {
            if(bullet_explosions_.Length > 0 && bullet_explosions_[0] != null)
            {
                if(bullet_explosions_[0].gameObject != null) Destroy(bullet_explosions_[0].gameObject);
                if(bullet_explosions_[1].gameObject != null) Destroy(bullet_explosions_[1].gameObject);
            }
            // bullet_explosions_[0].Stop();
            // bullet_explosions_[1].Stop();
        }else
        {
            if(bullet_explosions_.Length > 0)
            {
                for (int l = 0; l < bullet_explosions_.Length; l ++)
                    bullet_explosions_[l].Stop();
            }
        }

   
        // if (explosion_) explosion_.Play();

        Invoke("destry", 0.75f);
    }


    // PLAYER WEAPON HIT
    private void enemy_hit()
    {

        expl_offset = ( transform.position - plyr_target.transform.position );
        exploded = true;
        Invoke("destry", 0.75f);
        if(blt_expl != null) blt_expl.Play();
        TrailRenderer tr = gameObject.GetComponent<TrailRenderer>();
        if(tr != null) tr.enabled = false;
        // if(bullet_explosions_.Length > 0)
        // {
        //     for (int l = 0; l < bullet_explosions_.Length; l ++)
        //         bullet_explosions_[l].Play();
        // }
        bool is_left =  ((plyr_target.rotation.eulerAngles.y >= 170 ) ? true : false);  
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
