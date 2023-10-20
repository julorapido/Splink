using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header ("Weapon Damage")]
    private const float damage = 20f;

    [Header ("Weapon Precision")]
    private const float precision_ = 20f;

    [Header ("Player Movement Script")]
    private PlayerMovement pm;

    [Header ("Weapon FireRate")]
    private const float fireRate = 0.30f;
    private bool canShoot = true;

    [Header ("Weapon Recoil")]
    private const float recoil_strength = 6.25f;
    private const float recoil_speed = 40.0f;
    private float em_recoil = 0.0f;

    private float em_x_recoil = 0.0f;
    private float recoilX_strength = 0f;


    public enum GunLevel
    {
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE
    };

    [Header ("Weapon Shot PartSystems")]
    private  ParticleSystem[] ps_shots;

    [Header ("Weapon Bullet")]
    [SerializeField] private GameObject[] weapon_bullets;  


    [Header ("Weapon FirePoint")]
    [SerializeField] private Transform[] fire_point;
    private int point_indx = 0;

    [Header ("Updated Target")]
    private Transform target_;

    [Header ("Bullet Speed")]
    private float turnSpeed = 14f;
    private float speed = 14f;

    

    // Start is called before the first frame update
    private void Start()
    {
        pm = FindObjectOfType<PlayerMovement>();
        ps_shots = gameObject.GetComponentsInChildren<ParticleSystem>();
    }


    //bullets collisions
    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.gameObject.tag == "TURRET" || other.collider.gameObject.tag == "enemy" ) 
        {
            GameObject enemy = other.collider.gameObject;

            foreach (ContactPoint c in other.contacts)
            {
                //Debug.Log(c.thisCollider.name);
            }

            // get index of bullet child in parents 

            // split target arr 
            // delete from parent
        }
    }


    // Update is called for lerp values
    private void Update()
    {
        // if(fire_point[point_indx].childCount > 0)
        // {
        //     for (int i = 0; i < fire_point[point_indx].childCount; i ++)
        //     {
        //         GameObject blt_ = fire_point[point_indx].GetChild(i).gameObject;
        //         Rigidbody blt_rb = blt_.GetComponent<Rigidbody>();

        //         Vector3 dir = (bullets_targets[i].position + new Vector3(0f, 1f, 0f) ) - transform.localPosition;

        //     }
        // }
    }


    public void Shoot(Transform target_transform, bool horizontal_enm)
    {
        if(!canShoot) return;
        // if(target_ == target_transform) target_ = target_transform;

        em_recoil = 0f;
        pm.set_recoil = 0f;
        StartCoroutine(shoot_recoil());

        GameObject new_bullet =  Instantiate(weapon_bullets[0], fire_point[point_indx].position, fire_point[point_indx].rotation);
        A_T_Projectile proj_scrpt = new_bullet.GetComponent<A_T_Projectile>();
        proj_scrpt.plyr_target = target_transform;
        proj_scrpt.horitzontal_target = horizontal_enm;
        proj_scrpt.weapon_dmg = damage;
        proj_scrpt.player_ = transform.root;

        float x_ = UnityEngine.Random.Range(-1*(100 - precision_) / 2, (100 - precision_) / 2);
        float y_ = UnityEngine.Random.Range(-1*(100 - precision_) / 4, (100 - precision_) / 4);
        x_ /= 20; y_ /= 20;

        Vector3 randomized_aim = new Vector3(x_, y_, 0);
        proj_scrpt.weapon_precision = randomized_aim;

        LeanTween.scale(gameObject, new Vector3(gameObject.transform.localScale.x * 2, gameObject.transform.localScale.y * 2, gameObject.transform.localScale.z * 2), fireRate).setEasePunch();
 
        for(int i = 0; i < ps_shots.Length; i ++) ps_shots[i].Play();
    }

    private IEnumerator shoot_recoil()
    {
        StartCoroutine(delay_shoot());
        float recoil_time = 0.1f;
        float recoil_tick_ = recoil_strength / 30;
        
        while(em_recoil < recoil_strength)
        {
            em_recoil += recoil_tick_ * 4f;
            pm.set_recoil = em_recoil;
            yield return new WaitForSeconds( (recoil_time/2) / 30 ); // 0.15f per transition

            if(em_recoil >= recoil_strength) break;
        }

        while(em_recoil > 0)
        {
            em_recoil -= recoil_tick_ * 4f;
            pm.set_recoil = em_recoil;
            yield return new WaitForSeconds(  (recoil_time/2) / 30); // 0.15f
        }
    }

    private IEnumerator delay_shoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
}
