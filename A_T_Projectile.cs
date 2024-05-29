using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_T_Projectile : MonoBehaviour
{
    [Header ("WeaponBullet Specifics")]
    [HideInInspector] public bool horitzontal_target;
    [HideInInspector] public Vector3 weapon_precision;
    [HideInInspector] public bool target_isLeft;
    [HideInInspector] public bool is_crticial;

    [Header ("Target")]
    private Transform target_;
    [HideInInspector] public Transform set_target{
        get {  return null; }
        set { if(value.GetType() == typeof(Transform)) target_ = value; }
    }


    [Header ("Bullet Type & Damage")]
    [HideInInspector] public bool player_bullet = false;
    [HideInInspector] public Bullet_Type bullet_type;
    [HideInInspector] public enum Bullet_Type{
        Direct,
        Tracking,
        Ricochet,
        Grenade,
    };
    private int bullet_damage = 0;
    [HideInInspector] public int set_damage{
        get {  return 0; }
        set { if(value.GetType() == typeof(int)) bullet_damage = value; }
    }

    [Header ("Rocket Near-Inprecision")]
    private float rocket_near_inPrecision = 0.0f;
    
    [Header ("Attached Objects")]
    private ParticleSystem blt_expl;
    private Rigidbody bullet_rb;
    private Vector3 bullet_qtrn;

    [Header ("Projectile Speed")]
    private const float turnSpeed = 5f;
    private float speed = 13f;
    [HideInInspector] public float set_projSpeed {
        set { if(value.GetType() == typeof(float)) speed = value; }
        get { return 0f; }
    }

    [Header ("Stored direction")]
    private Vector3 l_dir;
    private int z_ = 0;

    [Header ("Bools")]
    private Vector3 expl_offset;
    private bool is_behind = false;
    private bool target_passed = false;
    private bool exploded = false;

    [Header ("ExplosionAnimation Position")]
    private const string turret_parts = "tr_Barrel tr_Stand tr_Plate tr_Radar tr_Shootp tr_BarrelHz";



    // Start
    private void Start()
    {
        if(!player_bullet)
        {  
            // float dir =  transform.position.z - (target_.position.z);
            is_behind = (transform.position.z < target_.position.z);

            LeanTween.scale(gameObject, transform.localScale * 0.5f, 2f).setEaseInCubic();
        }

        bullet_rb = gameObject.GetComponent<Rigidbody>();
        ParticleSystem[] ps_0 = gameObject.GetComponentsInChildren<ParticleSystem>();
        blt_expl = ps_0.Length > 0 ? ps_0[0] : null;


        // direct 
        if(bullet_type == Bullet_Type.Direct)
            bullet_qtrn = Vector3.RotateTowards(
                transform.forward,
                (target_.position + 
                    (player_bullet ?
                        ( 
                        (horitzontal_target ? 
                            new Vector3(target_isLeft ? 2.0f : -3f, 0f, 0f) : new Vector3(0f, 2.25f, 0f) 
                        ) + (weapon_precision) 
                    )
                    : (Vector3.zero) ) - transform.position
                ), 
                Time.deltaTime * 100, 0.0f
        );

        // tracking
        if(bullet_type == Bullet_Type.Tracking)
            rocket_near_inPrecision = UnityEngine.Random.Range(0.2f, 3f);

    }


    
    // Update
    private void Update()
    {


        // NON-PLAYER BULLET
        if(!player_bullet)
        {
            float passedNear_inPrecision =  (bullet_type == Bullet_Type.Tracking ? rocket_near_inPrecision : 0f);
            float dst_ = (is_behind) ? 
                (transform.position.z - target_.position.z) : (target_.position.z - transform.position.z);

            // detect player passed
            if(
                ((bullet_type != Bullet_Type.Ricochet) && (bullet_type != Bullet_Type.Grenade))
                && (dst_ > passedNear_inPrecision) 
                && !target_passed
                && !exploded
            ){
                target_passed = true; 
                speed *= 1.30f;
                Invoke("bullet_explode", 2.25f);
            }

            if(!exploded)
            {
                try{
                    Vector3 dir = (target_.position + new Vector3(0f, 1f, 0f) ) - transform.position;
                        
                    if(target_passed && bullet_type == Bullet_Type.Tracking)
                        dir = l_dir;

                    switch(bullet_type)
                    {
                        case Bullet_Type.Tracking:
                            Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * turnSpeed, 0.0f);

                            transform.Translate(Vector3.forward * Time.deltaTime * speed);
                            transform.rotation = Quaternion.LookRotation(newDirection);

                            if(!target_passed)
                                l_dir = new Vector3(dir.x, dir.y / 2, dir.z);

                            z_+= 3;
                            break;

                        case Bullet_Type.Direct:
                            // Vector3 shoot_dir = dir.normalized;

                            // transform.rotation = Quaternion.LookRotation(bullet_qtrn);
                            bullet_rb.AddForce( (speed / 15) * bullet_qtrn, ForceMode.VelocityChange);
                            break;

                        case Bullet_Type.Grenade:
                            Vector3 p = CalculateGrenadeJump(target_.position, transform.position, 1f);
                            break;
                    }
                    
                }catch(Exception err){
                    Debug.Log(err);
                }
            }
        }



        // PLAYER BULLET
        else
        {
            if(!exploded)
            {
                try{
                    if (transform.position.z > target_.position.z)
                        target_passed = true;

                    Vector3 dir = (target_passed == true) ? 
                        (l_dir) : 
                        (target_.position +
                                (
                                    horitzontal_target ? 
                                        new Vector3(target_isLeft ? 2.0f : -3f, 2.5f, 0f) : new Vector3(0f, 2.25f, 0f) 
                                )
                            - transform.position 
                    ) + (weapon_precision);

                    switch(bullet_type)
                    {
                        case Bullet_Type.Tracking:
                            
                            if(!target_passed)
                                l_dir = dir;

                            Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * turnSpeed, 0.0f);

                            transform.rotation = Quaternion.LookRotation(newDirection);
                            transform.Translate(Vector3.forward * Time.deltaTime * speed);

                            if(target_passed)
                            {
                                if(Vector3.Distance(transform.position, target_.position) > 60f)
                                    destry();
                            }
                            break;
                        case Bullet_Type.Direct:
                            // Vector3 shoot_dir = dir.normalized;

                            // transform.rotation = Quaternion.LookRotation(bullet_qtrn);
                            bullet_rb.AddForce( (speed / 15) * (bullet_qtrn), ForceMode.VelocityChange);
                            break;
                        case Bullet_Type.Grenade:
                            Vector3 p = CalculateGrenadeJump(target_.position, transform.position, 1f);

                            bullet_rb.AddForce(p, ForceMode.VelocityChange);
                            break;
                    }   
                }catch(Exception err){
                    Debug.Log(err);
                }
            }
            else
            {
                transform.position = target_.position + expl_offset;
            }
        }


    }

    private void FixedUpdate()
    {
       transform.Rotate(0,0,10f, Space.Self);
    }


    // Player bullet
    private void OnCollisionEnter(Collision other)
    {
        if(player_bullet)
        {
            GameObject member_gmObj_replaced = other.gameObject; // force initialization beacuse it's a GameObj
            string hit_tag = other.gameObject.tag;
            Transform parent_ = other.gameObject.transform.parent;


            // hit a turret
            if( turret_parts.Contains(hit_tag) )
            {
                for(int i = 0; i < 50; i ++)
                {
                    if(parent_.parent != null)
                    {
                        if(hit_tag == "Untagged" && parent_.gameObject.tag != "Untagged")
                        {
                            hit_tag = parent_.gameObject.tag;
                            member_gmObj_replaced = parent_.gameObject;
                        }


                        parent_ = parent_.parent;
                        if( (parent_.gameObject.tag == "TURRET" || parent_.gameObject.tag == "ENEMY") && !exploded)
                        {
                            AutoTurret turret = parent_.GetComponent<AutoTurret>();
                                 
                            turret.turret_damage(
                                hit_tag,
                                bullet_damage, 
                                other.gameObject.tag != "Untagged" ?  other.gameObject : member_gmObj_replaced,
                                is_crticial,
                                transform.position - target_.position
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


    // Enemy Bullet
    private void OnTriggerEnter(Collider other)
    {
        if(!player_bullet)
        {
            if(other.gameObject.tag == "player_hitbx" && !exploded)
                bullet_explode(other.gameObject);
            if(other.gameObject.tag == "ground" || other.gameObject.tag == "obstacle" || 
                other.gameObject.tag == "slide" && !exploded)
                bullet_explode();
            // if(other.gameObject.tag == "obstacle" && !exploded) bullet_explode();
        }
    }


    // TURRETS hit
    private void bullet_explode(GameObject go_ = null)
    {
        if(exploded) 
            return;

        CancelInvoke("bullet_explode");
        exploded = true;
        
        if(go_ != null)
        {
            GameUI g_ui = FindObjectOfType<GameUI>();
            g_ui.player_damage(bullet_damage);
        }
        Destroy(gameObject);
    }


    // PLAYER hit
    private void enemy_hit()
    {
        if(exploded) 
            return;

        exploded = true;
        Invoke("destry", 0.75f);
        if(blt_expl != null) 
            blt_expl.Play();
        TrailRenderer tr = gameObject.GetComponent<TrailRenderer>();
        if(tr != null) 
            tr.enabled = false;
            
        // if(bullet_explosions_.Length > 0)
        // {
        //     for (int l = 0; l < bullet_explosions_.Length; l ++)
        //         bullet_explosions_[l].Play();
        // }

        bool is_left =  ((target_.rotation.eulerAngles.y >= 170 ) ? true : false);  
    }

    private void destry(){ Destroy(gameObject); }



    // grenade throw
    private Vector3 CalculateGrenadeJump(Vector3 target, Vector3 origen, float time)
    {
        Vector3 distance = target - origen;
        Vector3 distanceXZ = new Vector3(distance.x, 0, distance.z);
        // distanceXZ.y = 0;

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
