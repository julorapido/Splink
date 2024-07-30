using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public class Weapon : MonoBehaviour
{
    [Header ("Weapon Ammo")]
    private int ammo = 0, ammo_inMagazine = 0;
    [HideInInspector] public int get_ammo {
        set {return; } get{ return ammo;}
    }
    [HideInInspector] public int get_ammoInMag {
        set {return; } get{ return ammo_inMagazine;}
    }
    private bool ammo_fixed  = true;

    [Header ("Weapon Stats")]
    private const int damage = 60;
    private const int precision_ = 80;
    private const float fireRate = 0.4f;
    private const int criticalChance = 5; // /100
    private const int range_ = 40; // max range 70-ish
    private const int magSize = 12;
    private const float reloadTime = 1.30f;
    private const float bullet_speed = 10f;

    [Header ("Player Movement/Collision/UI/Camera Scripts")]
    private GameUI g_ui;
    private CameraMovement cm_movement;
    private PlayerMovement pm;
    private PlayerCollisions pm_cls;
    public int get_attRange {
        set {return; }
        get{ return range_;}
    }


    [Header ("Can Weapon Shoot")]
    private bool canShoot = true;

    [Header ("Weapon Recoil")]
    private bool recoil_side = false;
    private Vector3 recoil_v = new Vector3(0f, 0f, 0f);
    [HideInInspector] public Vector3 get_WeaponRecoil {
        set { return ; }
        get { return (recoil_v); }
    }
    private Vector3 arm_recoil = new Vector3(0f, 0f, 0f);
    [HideInInspector] public Vector3 get_ArmRecoil {
        set { return ; }
        get { return (arm_recoil); }
    }

    private enum WeaponType
    {
        PISTOL,
        SMG,
        RIFLE,
        SEMIAUTO,
        HEAVY,
        SHOTGUN,
        LIGHTGUN,
        SNIPER
    };
    WeaponType weapon_type = new WeaponType();
    public enum GunLevel
    {
        COMMON,
        RARE,
        SUPER_RARE,
        EPIC,
        LEGENDARY,
        MYTHIC,
        ETERNAL
    };
    [SerializeField]
    GunLevel gun_level = new GunLevel();
    public GunLevel set_GunLevel{
        get {return 0;}
        set { if(value.GetType() == typeof(GunLevel)) gun_level = value;}
    }
    public GunLevel get_GunLvl{
        get {return gun_level;} set { return;}
    }



    [Header ("Weapon Shot PartSystems")]
    private  ParticleSystem[] ps_shots;

    [Header ("Bullet")]
    [SerializeField] private GameObject[] weapon_bullets;

    [Header ("FirePoint")]
    [SerializeField] private Transform[] fire_point;
    private int point_indx = 0;

    [Header ("Pocket WeaponSlot")]
    [SerializeField] private GameObject[] pocket_slot;

    [Header ("Updated Target")]
    private Transform target_;
    private Transform[] m_r;


    [Header ("Weapon Prefabs")]
    private const string gun_name = "usps";
    private GameObject[] weaponsResourcesPrefab_buffer = new GameObject[7]{null, null, null, null, null, null, null};
    private GameObject[] set_weaponsResourcesPrefab_buffer
    {
        set { if(value.GetType() == typeof(GameObject[])) weaponsResourcesPrefab_buffer = value;}
        get{return null;}
    }
    public GameObject[] get_weaponsResourcesPrefab_buffer
    {
        set { return; }
        get{return weaponsResourcesPrefab_buffer;}
    }

    [Header ("Weapon 3D-UI Slot")]
    private GameObject wpn_prefab;


    [Header ("Hitmarker")]
    private GameObject hitmarker_;



    // Awake
    private async void Awake()
    {
        m_r = gameObject.GetComponentsInChildren<Transform>();
        pm = FindObjectOfType<PlayerMovement>();

        g_ui = FindObjectOfType<GameUI>();
        cm_movement = FindObjectOfType<CameraMovement>();

        // await first to get gun name from database

        // init all guns (Assets/Resources/PlayerWeapon/0)
        // load gun prefabs
        UnityEngine.Object[] guns = Resources.LoadAll("PlayerWeapon/" + gun_name + "/",  typeof(GameObject));
        int i = 0;
        foreach(var t in guns)
        {
            weaponsResourcesPrefab_buffer[i] = (t) as GameObject;
            i++;
        }
        wpn_prefab = weaponsResourcesPrefab_buffer[0];

        // init weapon stats

        // set weapon type
        weapon_type = WeaponType.PISTOL;

        if( weapon_type == WeaponType.PISTOL || weapon_type == WeaponType.SMG )
        {
            pm.set_weaponHandedMode = true;
            pm.set_weaponReloadType = 0;
        }else
        {
            pm.set_weaponHandedMode = false;
            if(weapon_type == WeaponType.SHOTGUN || weapon_type == WeaponType.HEAVY || weapon_type == WeaponType.SNIPER){
                pm.set_weaponReloadType = 2;
            }else{
                pm.set_weaponReloadType = 1;
            }
        }

        pm.set_weaponReloadTime = reloadTime;

    }


    // Start
    private void Start()
    {

        pm_cls = FindObjectOfType<PlayerCollisions>();
        pm_cls.set_AttackRange = range_;

        ps_shots = gameObject.GetComponentsInChildren<ParticleSystem>();

        equip_Weapon(null);

        hitmarker_ = transform.parent.GetChild(1).gameObject;
    }


    // Update
    private void Update()
    {
        if(ammo == 0)
        {
            if(!ammo_fixed) equip_Weapon(false);
        }else
        {
            ammo_fixed = false;
        }
    }




    // ----------------------------------------
    // ------------------ SHOOT ---------------
    public void Shoot(Transform target_transform)
    {
        if( (!canShoot) || (target_transform == null)
            || (target_transform.GetType() != typeof(Transform))
        )
            return;
        
        if(target_ != target_transform)
            target_ = target_transform;

        StartCoroutine(shoot_recoil());
        StartCoroutine(delay_shoot());

        ammo--;

        cm_movement.shoot_recoil(recoil_side);
        recoil_side = !recoil_side;

        GameObject new_bullet =  Instantiate(
            weapon_bullets[0], fire_point[point_indx].position,
            Quaternion.LookRotation(pm.transform.forward) //fire_point[point_indx].rotation
        );

        float cr = UnityEngine.Random.Range(0, 100);

        AutoTurret AT_ = target_transform.gameObject.GetComponent<AutoTurret>();
        A_T_Projectile proj_scrpt = new_bullet.GetComponent<A_T_Projectile>();
        if(AT_ != null)
            proj_scrpt.target_isLeft = AT_.is_left;
        else
            proj_scrpt.target_isLeft = false;

        proj_scrpt.is_crticial = ((cr) <= (criticalChance));
        proj_scrpt.horitzontal_target = (
            (AT_ != null && AT_.is_horizontal) ? true : false 
        );
        proj_scrpt.bullet_type = (A_T_Projectile.Bullet_Type.Direct);
        proj_scrpt.set_projSpeed = (bullet_speed);
        proj_scrpt.set_target = (target_transform);
        proj_scrpt.set_damage = (damage);
        proj_scrpt.player_bullet = true;

        float x_ = UnityEngine.Random.Range(
            -1f * (100f - (float)(precision_)) / 1.7f, (100f - (float)(precision_)) / 1.7f
        ) * 0.1f;
        float y_ = UnityEngine.Random.Range(
            -1f * (100f - (float)(precision_)) / 3f, (100f - (float)(precision_)) / 3f
        ) * 0.1f;

        if( Vector3.Distance(target_transform.position, transform.position) < 16f)
        {
            x_ /= 2f;
            y_ /= 5f;
        }

        proj_scrpt.weapon_precision = new Vector3(x_, y_, 0);

        // LeanTween.scale( gameObject,
        //     new Vector3(gameObject.transform.localScale.x * 1.2f, gameObject.transform.localScale.y * 1.2f, gameObject.transform.localScale.z * 1.2f),
        //     fireRate - 0.02f
        // ).setEasePunch();

        for(int i = 0; i < ps_shots.Length; i ++)
        {
            if(ps_shots[i] != null)
                ps_shots[i].Play();
        }

        // reload
        if(ammo == 0)
        {
            reload();
        }
    }



    // recoil
    private IEnumerator shoot_recoil()
    {
        // em_recoil = 0f;
        // float recoil_time = (fireRate/2) - 0.05f;
        // float recoil_tick_ = recoil_strength / 30;
        // while(em_recoil < recoil_strength)
        // {
        //     em_recoil += recoil_tick_ * 4f;
        //     pm.set_recoil = em_recoil;
        //     yield return new WaitForSeconds( (recoil_time/2) / 30 ); // 0.15f per transition

        //     if(em_recoil >= recoil_strength) break;
        //}

        // while(em_recoil > 0)
        // {
        //     em_recoil -= recoil_tick_ * 4f;
        //     pm.set_recoil = em_recoil;
        //     yield return new WaitForSeconds(  (recoil_time/2) / 30); // 0.15f
        //}



        // ----------------------------------------
        //                 RECOIL
        // ----------------------------------------
        // Target recoil
        if(!(LeanTween.isTweening(gameObject)))
        {
            switch(weapon_type)
            {
                case WeaponType.PISTOL:
                case WeaponType.SHOTGUN:
                case WeaponType.SEMIAUTO:
                case WeaponType.SNIPER:
                    LeanTween.value( gameObject, 
                            new Vector3(0f, 0f, 0f), 
                            new Vector3(
                                (target_.position.z - transform.position.z) * 0.15f, 
                                (target_.position.y > transform.position.y + 5f ? 0.5f : 1.25f)
                                    + ((target_.position.z - transform.position.z) * 0.1f),
                                (target_.position.z - transform.position.z) * -0.2f
                            ), 
                            (fireRate - 0.01f) // * (UnityEngine.Random.Range(1.30f, 2.5f))
                        )
                        .setOnUpdate(
                            (Vector3 val) =>  {recoil_v = val;}
                        )
                        .setEasePunch();
                    break;
                case WeaponType.RIFLE:
                case WeaponType.SMG:
                case WeaponType.HEAVY:
                    Debug.Log("ret");
                    break;
            }
        }
        // Body and Arm recoil
        if(arm_recoil == Vector3.zero)
        {
            LeanTween.value( gameObject, 
                new Vector3(0f, 0f, 0f), 
                new Vector3(
                    UnityEngine.Random.Range(-5f, -15f), 
                    (UnityEngine.Random.Range(-40f, -60f)) + (damage * 0.1f), 
                    0f
                ), 
                (fireRate - 0.01f)
            ).setOnUpdate(
                (Vector3 v) =>  {arm_recoil = v;}
            ).setEasePunch();
        }
        
    
        yield break;

    }


    // shoot mechanic
    private IEnumerator delay_shoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }




    public void equip_Weapon(bool? pocket_weapon)
    {
        // turn off meshes, partsystms, etc..


        if(pocket_weapon != null)
        {
            // +3 mag && full actual mag
            ammo_inMagazine += 3 * (magSize);
            ammo = (magSize);
        }

        // nullable bool for Start
        if(pocket_weapon == null)
        {
            pocket_slot[0].SetActive(false);
            for(int k = 0; k < m_r.Length; k++){
                if(m_r[k] == gameObject.transform) continue;
                m_r[k].gameObject.SetActive(false);
            }
        }
        else if ((bool?)pocket_weapon == true)
        {
            for(int j = 0; j < m_r.Length; j++){
                if(m_r[j] == gameObject.transform) continue;
                m_r[j].gameObject.SetActive(false);
            }
            pocket_slot[0].SetActive(true);
        }
        else if ((bool?)pocket_weapon == false)
        {
            for(int i = 0; i < m_r.Length; i++){
                if(m_r[i] == gameObject.transform) continue;
                m_r[i].gameObject.SetActive(true);
            }
            pocket_slot[0].SetActive(false);
            ammo_fixed = true;
        }
    }





    // private method to get enum position number
    private int GetEnumPosition<T>(T src) where T : struct
    {
        if (!typeof(T).IsEnum) 
            throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src);
        return j;
    }
    // private method to get next LevelEnum Value
    private T Next<T>(T src) where T : struct
    {
        if (!typeof(T).IsEnum) 
            throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length==j) ? Arr[0] : Arr[j];
    }





    // --------------
    //  GUN LEVEL UP
    // --------------
    public void GunLevelUp()
    {

        pm_cls = FindObjectOfType<PlayerCollisions>();
        pm_cls.set_AttackRange = range_;

        gun_level =  Next(gun_level);

        wpn_prefab = weaponsResourcesPrefab_buffer[GetEnumPosition(gun_level)];
        g_ui.Gun_levelUp(GetEnumPosition(gun_level));
    }



    // --------------
    //     RELOAD
    // --------------
    public void reload()
    {
        ammo = (
            ammo_inMagazine < magSize ? 
                (ammo_inMagazine) :  (magSize)
        );

        ammo_inMagazine = (
            ammo_inMagazine < magSize ?  
                (0) :  (ammo_inMagazine - magSize)
        );

        pm.player_reload();
        g_ui.ui_reload(reloadTime);
    }


    // ----------------
    //   THROW WEAPON 
    //    (LEVEL-UP)
    // ----------------
    public IEnumerator throw_weapon(float t_)
    {
        yield return new WaitForSeconds(t_);
        GameObject dropping_gun = Instantiate(
            pocket_slot[0], transform.localPosition, transform.localRotation,
            transform.parent
        );
        // for(int i = 0; i < dropping_gun.transform.childCount; i++)
        // {
        //     GameObject ex_ = pocket_slot[0].transform.GetChild(i).gameObject;
        //     GameObject gun_part = Instantiate(
        //         ex_,
        //         ex_.transform.position, ex_.transform.rotation,
        //         dropping_gun.transform
        //     );
        //     gun_part.transform.localScale = ex_.transform.localScale;
        // }
        dropping_gun.transform.localPosition = Vector3.zero;
        dropping_gun.transform.localScale = Vector3.one;
        dropping_gun.AddComponent<Rigidbody>();
        Rigidbody rb = dropping_gun.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 0.1f;
        dropping_gun.transform.parent = null;
        dropping_gun.transform.position = transform.position + new Vector3(0.3f, -0.35f, 0.65f);
        dropping_gun.transform.rotation = Quaternion.Euler(-70f, 50f, -70f);
        dropping_gun.SetActive(true);
        rb.velocity = new Vector3(0f, 5f, 2f);
        rb.AddForce(new Vector3(-4f, 0f, 2f), ForceMode.VelocityChange);
        rb.AddTorque(new Vector3(0f, 
            UnityEngine.Random.Range(0f, 40f), 
        0f), ForceMode.VelocityChange);

        // turn off weapon in character's hand
        equip_Weapon(true);
    }


    // -----------
    //   UNEQUIP
    // -----------
    public IEnumerator weapon_unequip(float t_)
    {
        yield return new WaitForSeconds(t_);
        equip_Weapon(true);
    }


    // -------------------
    //     AMMO PICKUP
    // -------------------
    public void ammo_pickup()
    {
    }



}
