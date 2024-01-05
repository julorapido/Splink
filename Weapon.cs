using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public class Weapon : MonoBehaviour
{
    [Header ("Weapon Ammo")]
    private int ammo = 0;
    public int get_ammo {
        set {return; }
        get{ return ammo;}
    }
    private bool ammo_fixed  = true;

    [Header ("Weapon Statistics")]
    private const int damage = 40;
    private const int precision_ = 50;
    private const float fireRate = 0.4f;
    private const int criticalChance = 7; // /100
    private const int range_ = 30;
    private const int magSize = 12;
    private const float reloadTime = 2f;

    [Header ("Weapon Precision")]

    [Header ("Player Movement/Collision Script")]
    private PlayerMovement pm;
    private PlayerCollisions pm_cls;

    public int get_attRange {
        set {return; }
        get{ return range_;}
    }

    [Header ("Weapon FireRate")]
    private bool canShoot = true;

    [Header ("Weapon Recoil")]
    private const float recoil_strength = 6.25f;
    private const float recoil_speed = 40.0f;
    private float em_recoil = 0.0f;
    private bool recoil_side = false;


    // [Header ("Weapon Level")]
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


    [Header ("Game-UI")]
    private GameUI g_ui;

    [Header ("CameraMovement")]
    private CameraMovement cm_movement;

    // [Header ("Weapon-Type")]
    private enum GunType
    {
        PISTOL,
        SHOTGUN,
        SMG,
        RIFLE,
        SNIPER,
        MYTHIC,
    };



    // Awake is called even if the script is disabled
    private void Awake()
    {
        m_r = gameObject.GetComponentsInChildren<Transform>();

        g_ui = FindObjectOfType<GameUI>();
        cm_movement = FindObjectOfType<CameraMovement>();

        // init all guns (Assets/Resources/CollectibleGuns/0)
        // load gun prefabs
        UnityEngine.Object[] guns = Resources.LoadAll("CollectibleGuns/" + gun_name + "/",  typeof(GameObject));
        int i = 0;
        foreach(var t in guns)
        {
            weaponsResourcesPrefab_buffer[i] = (t) as GameObject;
            i++;
        }
        wpn_prefab = weaponsResourcesPrefab_buffer[0];
    }



    private void Start()
    {
    
        pm = FindObjectOfType<PlayerMovement>();

        pm_cls = FindObjectOfType<PlayerCollisions>();
        pm_cls.set_AttackRange = range_;

        ps_shots = gameObject.GetComponentsInChildren<ParticleSystem>();

        equip_Weapon(null);
    }



    //collisions??
    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.gameObject.tag == "TURRET" || other.collider.gameObject.tag == "enemy" ) 
        {
            GameObject enemy = other.collider.gameObject;

            // get index of bullet child in parents 
            // split target arr 
            // delete from parent
        }
    }


    // Update is called for lerp values
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
        if(!canShoot) return;
        // if(target_ == target_transform) target_ = target_transform;

        em_recoil = 0f;
        pm.set_recoil = 0f;
        StartCoroutine(shoot_recoil());

        // ammo--;
        cm_movement.shoot_recoil(recoil_side);
        recoil_side = !recoil_side;

        GameObject new_bullet =  Instantiate(weapon_bullets[0], fire_point[point_indx].position, fire_point[point_indx].rotation);
        A_T_Projectile proj_scrpt = new_bullet.GetComponent<A_T_Projectile>();
        
        float cr = UnityEngine.Random.Range(0, 100);
        bool is_criticalHit = cr <= criticalChance ? true : false;

        AutoTurret AT_ = target_transform.gameObject.GetComponent<AutoTurret>();
        if(AT_ != null) proj_scrpt.target_isLeft = AT_.is_left;

        proj_scrpt.is_crticial = is_criticalHit;
        proj_scrpt.plyr_target = target_transform;
        proj_scrpt.horitzontal_target = horizontal_enm;
        proj_scrpt.weapon_dmg = damage;
        proj_scrpt.player_ = transform.root;

        float x_ = UnityEngine.Random.Range(-1f *(100f - (float)(precision_)) / 2f, (100f - (float)(precision_)) / 2f);
        float y_ = UnityEngine.Random.Range(-1f *(100f - (float)(precision_)) / 3f, (100f - (float)(precision_)) / 3f);

        x_ /= 15f; y_ /= 12f;

        if( Vector3.Distance(target_transform.position, transform.position) < 16f){
            x_ /= 20f;
            y_ /= 30;
        }
        if( Vector3.Distance(target_transform.position, transform.position) < 7f) 
            y_ /= 10;
        

        Vector3 randomized_aim = new Vector3(x_, y_, 0);
        proj_scrpt.weapon_precision = randomized_aim;

        LeanTween.scale( gameObject, 
            new Vector3(gameObject.transform.localScale.x * 2, gameObject.transform.localScale.y * 2, gameObject.transform.localScale.z * 2),
            fireRate - 0.02f
        ).setEasePunch();
 
        for(int i = 0; i < ps_shots.Length; i ++) ps_shots[i].Play();
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

    // public reload method
    public void reload()
    {
        ammo = magSize;
    }

    // turn off meshes, partsystms, etc..
    public void equip_Weapon(bool? pocket_weapon)
    {   

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
}
