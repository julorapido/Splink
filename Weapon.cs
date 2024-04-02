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
    private const int damage = 10;
    private const int precision_ = 70;
    private const float fireRate = 0.6f;
    private const int criticalChance = 20; // /100
    private const int range_ = 40;
    private const int magSize = 12;
    private const float reloadTime = 2f;
    private const float bullet_speed = 30f;

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
    private const float recoil_strength = 5.25f;
    private const float recoil_speed = 40.0f;
    private float em_recoil = 0.0f;
    private bool recoil_side = false;


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

    [Header ("Weapon Bullet")]
    [SerializeField] private GameObject[] weapon_bullets;  


    [Header ("Weapon FirePoint")]
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


    // private method to get enum position number
    private int GetEnumPosition<T>(T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src);
        return j;            
    }


    // private method to get next LevelEnum Value
    private T Next<T>(T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length==j) ? Arr[0] : Arr[j];            
    }

    public void GunLevelUp()
    {

        pm_cls = FindObjectOfType<PlayerCollisions>();
        pm_cls.set_AttackRange = range_;

        gun_level =  Next(gun_level);

        wpn_prefab = weaponsResourcesPrefab_buffer[GetEnumPosition(gun_level)];
        g_ui.Gun_levelUp(GetEnumPosition(gun_level));
    }





    public void Shoot(Transform target_transform, bool horizontal_enm)
    {
        if(!canShoot || target_transform == null) return;

        em_recoil = 0f;
        pm.set_recoil = 0f;
        StartCoroutine(shoot_recoil());

        // ammo--;

        cm_movement.shoot_recoil(recoil_side);
        recoil_side = !recoil_side;

        GameObject new_bullet =  Instantiate(
            weapon_bullets[0], fire_point[point_indx].position, 
            Quaternion.LookRotation(pm.transform.forward) //fire_point[point_indx].rotation
        );
        
        float cr = UnityEngine.Random.Range(0, 100);
        bool is_criticalHit = cr <= criticalChance ? true : false;

        AutoTurret AT_ = target_transform.gameObject.GetComponent<AutoTurret>();
        A_T_Projectile proj_scrpt = new_bullet.GetComponent<A_T_Projectile>();
        if(AT_ != null) 
            proj_scrpt.target_isLeft = AT_.is_left;
            
        proj_scrpt.player_bullet = true;
        proj_scrpt.bullet_type = A_T_Projectile.Bullet_Type.Direct;
        proj_scrpt.is_crticial = is_criticalHit;
        proj_scrpt.set_target = target_transform;
        proj_scrpt.horitzontal_target = horizontal_enm;
        proj_scrpt.weapon_dmg = damage;
        proj_scrpt.set_projSpeed = bullet_speed;

        float x_ = UnityEngine.Random.Range(
            -1f * (100f - (float)(precision_)) / 2f, (100f - (float)(precision_)) / 2f
        ) * 0.1f;
        float y_ = UnityEngine.Random.Range(
            -1f * (100f - (float)(precision_)) / 3f, (100f - (float)(precision_)) / 3f
        ) * 0.1f;

        if( Vector3.Distance(target_transform.position, transform.position) < 16f)
        {
            x_ /= 2f;
            y_ /= 2f;
        }
        
        proj_scrpt.weapon_precision = new Vector3(x_, y_, 0);


        LeanTween.scale( gameObject, 
            new Vector3(gameObject.transform.localScale.x * 1.2f, gameObject.transform.localScale.y * 1.2f, gameObject.transform.localScale.z * 1.2f),
            fireRate - 0.02f
        ).setEasePunch();
 
        for(int i = 0; i < ps_shots.Length; i ++) 
            ps_shots[i].Play();


        // reload
        if(ammo == 0)
        {
            reload();
        }

    }



    private IEnumerator shoot_recoil()
    {
        StartCoroutine(delay_shoot());
        float recoil_time = (fireRate/2) - 0.05f;
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




    // turn off meshes, partsystms, etc..
    public void equip_Weapon(bool? pocket_weapon)
    {   

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




    // public reload method
    public void reload()
    {
        ammo = ((ammo_inMagazine < magSize) ?  ammo_inMagazine :  magSize);
        ammo_inMagazine = ((ammo_inMagazine < magSize) ?  0 :  (ammo_inMagazine - magSize));

        pm.player_reload();
        g_ui.ui_reload(reloadTime);
    }



}
