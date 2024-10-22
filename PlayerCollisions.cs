using System.Collections;
using System.Collections.Generic;
//using System.Collections.Immutable;
using UnityEngine;
using PathCreation.Utility;
using System;
using System.Reflection;
using UnityEditor;

// [CustomEditor(typeof(PlayerCollisions)), CanEditMultipleObjects]
// public class PlayerCollisionsEditor : Editor 
// {
//     SerializedProperty isMainCollision_bool;
//     SerializedProperty m_part_arrays;

//     private void OnEnable()
//     {
//         isMainCollision_bool = serializedObject.FindProperty("isMainCollision");
//         // m_part_arrays = serializedObject.FindProperty("player_particls");
//     }

//     public override void OnInspectorGUI()
//     {
//         // keep default inspector
//         DrawDefaultInspector();
        
//         // logic
//         if (isMainCollision_bool.boolValue)
//         {
//             EditorGUILayout.HelpBox("Main Collision Particles Array", MessageType.Info);
//             EditorGUILayout.PropertyField(m_part_arrays, GUIContent.none);
//         }

//         // serializedObject.ApplyModifiedProperties();
//     }
// }

// [CustomEditor(typeof(PlayerCollisions))]
// public class PlayerCollisionsEditor : Editor
// {
//   void OnInspectorGUI()
//   {
//     var collision_script = target as PlayerCollisions;
//     SerializedProperty m_part_arrays;

//     m_part_arrays = serializedObject.FindProperty("player_particls");
//     collision_script.isMainCollision = GUILayout.Toggle(collision_script.isMainCollision, "player_particls");

//     //if(collision_script.flag)
//     //  collision_script.i = EditorGUILayout.IntSlider("I field:", collision_script.i , 1 , 100);
//   }
// }
public class PlayerCollisions : MonoBehaviour
{
    // [DrawIf("someFloat", 1f, ComparisonType.GreaterOrEqual)]
    public bool isMainCollision;
    [Serializable] public class Particl_List { 
        public ParticleSystem[] jump; 
        public ParticleSystem[] doubleJump; 
        public ParticleSystem[] slide; 
        public ParticleSystem[] kill; 
        // public ParticleSystem[] coin; 
        public ParticleSystem[] health; 
        public ParticleSystem[] armor; 
        public ParticleSystem[] deaths; 
        public ParticleSystem[] interacts_; 
        public ParticleSystem[] combo; 
        public ParticleSystem[] ammo; 
    } 
    [SerializeField] private Particl_List[] player_particls; 

    [Header ("Collisions Constants")]
    public string[] colsions_values = new string[7]{"ground", "frontwall","sidewall", "slider", "enemyProj", "collectibles", "boxAutoAim"};

    [Header ("SubCollisions Constants")]
    private string[] subcolsions_values = new string[5]{"obstacleHit", "launcherHit","tyro", "bumper", "tapTapJump"};

    [Header ("-> Selected Collision <-")]
    [SerializeField] private string slcted_clsion;

    [Header ("Ground Physical Mat")]
    [HideInInspector] public PhysicMaterial grnd_mat;

    [Header ("Start Delay")]
    private float strt_delay = 0.5f;
    private bool can_trgr = false;
    private Vector3 currentVelocity;

    [Header ("Sider Wall last_registered_gm")]
    private int lst_wall;

    [Header ("Currently Aimed Enemy (Sphere)")]
    private GameObject sphereStored_aimed_turret;
    private GameObject sphere_aimed_turret;
    private int turretInSight = 0;


    [Header ("Player Weapon")]
    private Weapon p_weapon;
    private int player_attackRange;
    private int player_ammo = 0;
    [HideInInspector] public int set_AttackRange {
        get {return player_attackRange;}
        set { if(value is (int) ){player_attackRange = value; }}
    }


    [Header ("Currently Auto-Aimed Enemy [Default]")]
    private Collider m_Collider;
    private RaycastHit[] m_Hits;
    private GameObject aimed_enemy;
    private GameObject storedAimed_enemy;
    private int enemy_inSight = 0;
    private bool firstEverDetectedEnemy = false;
    
    [Header ("Wall Run Aim Hitbox")]
    [HideInInspector] public bool wallRun_aimBox = false;
    [HideInInspector] public float z_wallRun_aimRotation = 0.0f;

    [Header ("Attached Scrtips PlayerMovement/CameraMovement/GameUi")]
    private PlayerMovement p_movement;
    private CameraMovement c_movement;
    private GameUI game_ui;
    private PlayerVectors p_vectors;

    [Header ("Main PlayerCollisions")]
    private PlayerCollisions psCollisions_movement;

    [Header ("Character Joints")]
    private Rigidbody[] character_connectedBodies;
    private CharacterJoint[] pico_characterJoints;

    // AWAKE
    private void Awake()
    {
        // disable ragdoll
        if(slcted_clsion == "boxAutoAim")
        {
            set_playerRagdoll(false);
        }

        p_movement =  FindObjectOfType<PlayerMovement>();
        c_movement = FindObjectOfType<CameraMovement>();
        p_weapon = FindObjectOfType<Weapon>();
        game_ui = FindObjectOfType<GameUI>();

        if(slcted_clsion == "sidewall")
            p_vectors = FindObjectOfType<PlayerVectors>();
    }


    // Start
    private void Start()
    {
        m_Collider = gameObject.GetComponent<Collider>();
        StartCoroutine(delay_trgrs(strt_delay));

  
        PlayerCollisions ar =  GameObject.FindGameObjectsWithTag("mainHitbox")[0].GetComponent<PlayerCollisions>();
        psCollisions_movement = ar;
    }
  
  
 
    // aim box
    private void DisplayBox(Vector3 center, Vector3 HalfExtend, Quaternion rotation, float Duration = 0)
    {
        Vector3[] Vertices = new Vector3[8];
        int i = 0;
        // Each loop from -1 to 1 [To extend behind and beyond] from the center
        for (int x = -1; x < 2; x += 2) // X-Axis
        {
            for (int y = -1; y < 2; y += 2)// Y-Axis
            {
                for (int z = -1; z < 2; z += 2) // Z-Axis
                {
                    Vertices[i] = center + new Vector3(HalfExtend.x * x, HalfExtend.y * y, HalfExtend.z * z);
                    i++;
                }
            }
        }

        Vertices = RotateObject(Vertices, rotation.eulerAngles, center);

        Debug.DrawLine(Vertices[0], Vertices[1], Color.white, Duration);
        Debug.DrawLine(Vertices[1], Vertices[3], Color.white, Duration);
        Debug.DrawLine(Vertices[2], Vertices[3], Color.white, Duration);
        Debug.DrawLine(Vertices[2], Vertices[0], Color.white, Duration);
        Debug.DrawLine(Vertices[4], Vertices[0], Color.white, Duration);
        Debug.DrawLine(Vertices[4], Vertices[6], Color.white, Duration);
        Debug.DrawLine(Vertices[2], Vertices[6], Color.white, Duration);
        Debug.DrawLine(Vertices[7], Vertices[6], Color.white, Duration);
        Debug.DrawLine(Vertices[7], Vertices[3], Color.white, Duration);
        Debug.DrawLine(Vertices[7], Vertices[5], Color.white, Duration);
        Debug.DrawLine(Vertices[1], Vertices[5], Color.white, Duration);
        Debug.DrawLine(Vertices[4], Vertices[5], Color.white, Duration);
    }


    private Vector3[] RotateObject(Vector3[] VerticesToRotate, Vector3 DegreesToRotate, Vector3 Around)//rotates a set of dots counterclockwise
    {
        for (int i = 0; i < VerticesToRotate.Length; i++)
        {
            VerticesToRotate[i] -= Around;
        }

        // Quaternion Euler Angles to Radians
        DegreesToRotate.z = Mathf.Deg2Rad * DegreesToRotate.z;
        DegreesToRotate.x = Mathf.Deg2Rad * DegreesToRotate.x;
        DegreesToRotate.y = -Mathf.Deg2Rad * DegreesToRotate.y;

        for(int j = 0; j < 3; j ++)
        {
            for (int i = 0; i < VerticesToRotate.Length; i++)
            {

                float CosA, cosB, SinA, SinB;
                CosA = cosB = SinA = SinB = 0.0f;
                float H = Vector3.Distance(Vector3.zero, VerticesToRotate[i]);

                float x_ratio = H * ((CosA * cosB) - (SinA * SinB));
                float y_ratio = H * ((SinA * cosB) + (CosA * SinB));

                if (H != 0)
                {
                    if(j == 0)
                    {
                        CosA = VerticesToRotate[i].x / H;
                        cosB = Mathf.Cos(DegreesToRotate.z);

                        SinA = VerticesToRotate[i].y / H;
                        SinB = Mathf.Sin(DegreesToRotate.z);

                        x_ratio = H * ((CosA * cosB) - (SinA * SinB));
                        y_ratio = H * ((SinA * cosB) + (CosA * SinB));
                        VerticesToRotate[i] = new Vector3(x_ratio, y_ratio, VerticesToRotate[i].z);
                        // Z-Axis
                    }
                    else if (j == 1)
                    {
                        CosA = VerticesToRotate[i].y / H;
                        cosB = Mathf.Cos(DegreesToRotate.x);

                        SinA = VerticesToRotate[i].z / H;
                        SinB = Mathf.Sin(DegreesToRotate.x);

                        x_ratio = H * ((CosA * cosB) - (SinA * SinB));
                        y_ratio = H * ((SinA * cosB) + (CosA * SinB));
                        VerticesToRotate[i] = new Vector3(VerticesToRotate[i].x, x_ratio, y_ratio);
                        // X-Axis
                    }
                    else if (j == 2)
                    {
                        CosA = VerticesToRotate[i].x / H;
                        cosB = Mathf.Cos(DegreesToRotate.y);

                        SinA = VerticesToRotate[i].z / H;
                        SinB = Mathf.Sin(DegreesToRotate.y);


                        x_ratio = H * ((CosA * cosB) - (SinA * SinB));
                        y_ratio = H * ((SinA * cosB) + (CosA * SinB));
                        VerticesToRotate[i] = new Vector3(x_ratio, VerticesToRotate[i].y, y_ratio);
                        // Y-Axis
                    }
                }
            }
        }

        for (int i = 0; i < VerticesToRotate.Length; i++)
            VerticesToRotate[i] += Around;

        return VerticesToRotate;
    }



    // fixedUpdate
    private void FixedUpdate()
    {
        // attribute attack range
        if(p_weapon != null)
        {
             player_attackRange = p_weapon.get_attRange;
             player_ammo = p_weapon.get_ammo;
        }

        if(player_ammo > 0)
        {
            // player shots auto-aim
            // if(slcted_clsion == "boxCastAutoAim" && (true == false))
            // {
            //     enemy_inSight = 0;
            //     RaycastHit[] top = Physics.BoxCastAll(m_Collider.bounds.center, transform.localScale, transform.forward, transform.rotation, 200f);
            //     RaycastHit[] bottom = Physics.BoxCastAll(m_Collider.bounds.center, transform.localScale + new Vector3(0, -1, 0), transform.forward, transform.rotation, 200f);
            //     m_Hits  = new RaycastHit[top.Length + bottom.Length];
            //     top.CopyTo(m_Hits, 0);
            //     bottom.CopyTo(m_Hits, top.Length);

            //     for(int i = 0; i < m_Hits.Length; i ++)
            //     {
            //         if ( (m_Hits[i].collider.tag == "TURRET") )
            //         {
            //             //Output the name of the enemy you hits
            //             Debug.Log("Hit : " + m_Hits[i].collider.name);
            //             aimed_enemy = m_Hits[i].transform.gameObject;
            //             if(storedAimed_enemy != aimed_enemy)
            //             {
            //                 storedAimed_enemy = aimed_enemy;
            //                 p_movement.animateCollision("newEnemyAim", new Vector3(0, 0, 0), storedAimed_enemy);
            //             }
            //             enemy_inSight++;
            //             break;
            //         }
            //     }
            //     if(enemy_inSight == 0 && (aimed_enemy != null) )
            //         p_movement.animateCollision("emptyEnemyAim", new Vector3(0, 0, 0));
            //         aimed_enemy = null;
            // }

            // Player Box Auto-Aim
            bool enemy_destroyed = false;
            if(slcted_clsion == "boxAutoAim")
            {
                float minDistance = float.MaxValue;

                // DisplayBox(transform.position,  
                //     new Vector3(4f, 10f, player_attackRange), 
                //     wallRun_aimBox ? Quaternion.Euler(z_wallRun_aimRotation, transform.rotation.y, 0) : transform.rotation
                // );

                int maxColliders = (player_attackRange / 10) * 100;
                // Debug.Log((player_attackRange / 10) * 100);
                Collider[] hitColliders = new Collider[maxColliders];
                int numColliders = Physics.OverlapBoxNonAlloc
                (
                    transform.position,
                    new Vector3(4f, 10f, player_attackRange), 
                    hitColliders,
                    //transform.rotation
                    (wallRun_aimBox) ? 
                        (Quaternion.Euler(z_wallRun_aimRotation, transform.rotation.y, 0)) : (transform.rotation)
                );

                if(numColliders > 0)
                {   
                    turretInSight = 0;
                    for (int i = 0; i < hitColliders.Length; i ++)
                    {   
                        if(hitColliders[i] == null ) 
                            continue;

                        if(hitColliders[i].tag == "TURRET" || hitColliders[i].tag == "ENEMY")
                        {
                            Vector3 possiblePosition = hitColliders[i].transform.position;
                        
                            float currDistance = Vector3.Distance(transform.position, possiblePosition);
                            float zDist = possiblePosition.z - transform.position.z;

                            if(zDist < 1f
                                || ((zDist <= 15) && ((possiblePosition.y - transform.position.y) > 10f))
                            ) continue;

                            // If the distance is smaller than the one before...
                            if ( (currDistance < minDistance) )
                            {
                                sphere_aimed_turret = hitColliders[i].transform.gameObject;
                                minDistance = currDistance;
                            }

                            turretInSight++;
                        }
                    }

                    if( (sphereStored_aimed_turret != sphere_aimed_turret)
                                                && 
                        (!firstEverDetectedEnemy || (sphere_aimed_turret != null) ) 
                    ){
                        p_movement.animateCollision("newEnemyAim", new Vector3(0, 0, 0), sphere_aimed_turret);
                        sphereStored_aimed_turret = sphere_aimed_turret;
                        firstEverDetectedEnemy = true;
                    }

                    if( turretInSight == 0  && (sphere_aimed_turret != null) )
                    {
                        p_movement.animateCollision("emptyEnemyAim", new Vector3(0, 0, 0));
                        sphere_aimed_turret = null;
                        sphereStored_aimed_turret = null;
                    }
                }

            }
        }


    }




    private void OnTriggerEnter(Collider collision)
    {
        if(slcted_clsion.Length > 0 && can_trgr)
        {
            Vector3 _size = collision.bounds.size;
            switch (slcted_clsion)
            {
                // ==================================================
                //                      GROUND
                // ==================================================
                case "ground":

                    // Groundroll
                    if(collision.gameObject.tag == "ground")
                    {
                        p_movement.animateCollision("groundHit", _size);
                        if(grnd_mat != null)
                        {
                            Collider p =  collision.gameObject.GetComponent<Collider>();
                            if(p){ p.sharedMaterial = grnd_mat;}
                        }
                    }


                    // Obstacle
                    if(collision.gameObject.tag == "obstacle")
                    {
                        p_movement.animateCollision("obstacleHit", _size, collision.gameObject);
                    }

                    // Launcher
                    if(collision.gameObject.tag == "launcher")
                        p_movement.animateCollision("launcherHit", _size, collision.gameObject);

                    // Tyro hit
                    if(collision.gameObject.tag == "tyro")
                        p_movement.tyro_movement(collision.gameObject);

                    // Bumper
                    if(collision.gameObject.tag == "bumper")
                        p_movement.animateCollision("bumper", _size, collision.gameObject);
                    

                    // TapTap Jump
                    if(collision.gameObject.tag == "tapTapJump") 
                        p_movement.animateCollision("tapTapJump", _size, collision.gameObject);
                    
                    // Land
                    if(collision.gameObject.tag == "fallBox")
                    {
                        p_movement.animateCollision("land", _size, collision.gameObject);
                    }  

                    // void & sidevoid
                    if(collision.gameObject.tag == "void" || collision.gameObject.tag == "sideVoid")
                    {
                        if(collision.gameObject.tag == "void")
                            p_movement.animateCollision("void", _size, collision.gameObject);
                        else
                            p_movement.animateCollision("sideVoid", _size, collision.gameObject);
                    }

                    if(collision.gameObject.tag == "bareer")
                    {
                        p_movement.animateCollision("bareerG", _size, collision.gameObject);       
                    }
                    break;

                // ==================================================
                //                      FRONT
                // ==================================================
                case "frontwall":
                    // Turret collision
                    List<string> t_parts = new List<string>(new string[6]
                    {
                    "tr_Barrel","tr_Shootp", "tr_BarrelHz", "tr_BarrelHz", "tr_Body", "tr_Stand"
                    });
                    if(t_parts.Contains(collision.gameObject.tag))
                    {
                        p_movement.animateCollision("frontTurret_Col_GameOver", _size, collision.gameObject);     
                    }

                    // FrontWall Gameover
                    if(collision.gameObject.tag == "ground"){
                        p_movement.animateCollision("frontWallHit", _size, collision.gameObject);
                    }

                    // fallBox front hit Gameover
                    if(collision.gameObject.tag == "fallBox"){
                        if( !collision.isTrigger )
                            p_movement.animateCollision("frontSpecialGameOver", _size, collision.gameObject);
                    }

                    // Ladder
                    if(collision.gameObject.tag == "ladder"){
                        p_movement.animateCollision("ladderHit", _size, collision.gameObject);
                    }

                    if(collision.gameObject.tag == "bareer"){
                        p_movement.animateCollision("bareerFrontDelay", _size, collision.gameObject);   
                    }

                    break;

                // ==================================================
                //                      SIDEWALL
                // ==================================================
                case "sidewall":
                    // WallRun hit
                    if(collision.gameObject.tag == "ground" || collision.gameObject.tag == "ramp" )
                    {
                        // if(lst_wall != collision.gameObject.GetInstanceID())
                        // {
                            lst_wall = (collision.gameObject.GetInstanceID());
                            p_movement.animateCollision("wallRunHit", _size, collision.gameObject);
                            // FindObjectOfType<PlayerVectors>().slippery_trigr(false, collision.gameObject);
                            p_vectors.slippery_trigr(false, collision.gameObject);
                        // }
                    }

                    // SlideWall hit
                    if(collision.gameObject.tag == "slider")
                    {
                        p_movement.animateCollision("sliderHit", _size, collision.gameObject);
                    }
                    break; 

                // ==================================================
                //                       SLIDER
                // ==================================================
                case "slider":
                    // Slider hit
                    if(collision.gameObject.tag == "slider")
                    {
                        p_movement.animateCollision("sliderHit", _size, collision.gameObject);
                        psCollisions_movement.player_paricleArray(psCollisions_movement.player_particls[0].slide);
                    } 

                    // Rail
                    if(collision.gameObject.tag == "slideRail")
                    {
                        p_movement.animateCollision("railSlide", _size, collision.gameObject);
                        // c_movement.railSlide_offset(false); moved to pm.cs
                    }

                    // Ramp   
                    if(collision.gameObject.tag == "ramp")
                    {
                        p_movement.animateCollision("rampSlide", _size, collision.gameObject);
                        c_movement.rmp_slid_offst(false, collision.gameObject.transform);
                    }

                    // Under
                    if(collision.gameObject.tag == "under")
                    {
                        p_movement.animateCollision("under", _size, collision.gameObject);
                        
                    }

                    // SideHang
                    if(collision.gameObject.tag == "sideHang")
                    {
                        p_movement.animateCollision("sideHang", _size, collision.gameObject);
                        
                    }

                    // hang
                    if(collision.gameObject.tag == "hang")
                        p_movement.animateCollision("hang", _size, collision.gameObject);

                    // Bareer
                    if(collision.gameObject.tag == "bareer")
                    {
                        p_movement.animateCollision("bareerW", _size, collision.gameObject);       
                    }
                    break;


                // ==================================================
                //                     COLLECTIBLE
                // ==================================================
                case "collectibles":
                    // collectible hit
                    switch(collision.gameObject.tag)
                    {
                        case "coin":
                            // psCollisions_movement.player_paricleArray(psCollisions_movement.player_particls[0].coin);
                            GameObject gm = collision.gameObject;
                            ParticleSystem[] ps = new ParticleSystem[2]{
                                gm.transform.GetChild(2).GetComponent<ParticleSystem>(),
                                gm.transform.GetChild(3).GetComponent<ParticleSystem>(),
                            };
                            MeshRenderer[] mr_ = gm.GetComponentsInChildren<MeshRenderer>();
                            for(int i = 0; i < mr_.Length; i++)
                                mr_[i].enabled = false;
                                
                            game_ui.gain_money(4);
                            ps[0].Stop();
                            if(ps[1] != null)
                                ps[1].Play();
                            break;

                        case "gun":
                            // player_paricleArray(player_particls[0].coin);
                            collision.gameObject.SetActive(false);

                            p_movement.animateCollision("gun", _size);
                            FindObjectOfType<Weapon>().GunLevelUp();
                            game_ui.ui_announcer("weapon_levelUp");
                            break;

                        case "healthSmall":
                            psCollisions_movement.player_paricleArray(
                                    psCollisions_movement.player_particls[0].health, false, "healthSmall");
                            collision.gameObject.SetActive(false);
                            game_ui.gain_health(100);
                            Destroy(collision.gameObject);
                            break;
                        case "healthBig":
                            psCollisions_movement.player_paricleArray(
                                    psCollisions_movement.player_particls[0].health, false, "healthBig");
                            collision.gameObject.SetActive(false);
                            game_ui.gain_health(500);
                            Destroy(collision.gameObject);
                            break;

                        case "ammoSmall":
                            psCollisions_movement.player_paricleArray(psCollisions_movement.player_particls[0].ammo, false, "ammo");
                            game_ui.gain_ammo(3);
                            Destroy(collision.gameObject);
                            break;
                        case "ammoBig":
                            psCollisions_movement.player_paricleArray(psCollisions_movement.player_particls[0].ammo, false, "ammo");
                            game_ui.gain_ammo(6);
                            Destroy(collision.gameObject);
                            break;
                    }
                    break;
                case "enemyProj":
                    
                    break;
                default:
                    break;
            }
        }
    }
    
    private void OnTriggerExit(Collider collision)
    {
        if(slcted_clsion.Length > 0 && can_trgr)
        {
            Vector3 _size = collision.bounds.size;
            switch (slcted_clsion)
            {
                case "ground":
                    // ground, land
                    if(collision.gameObject.tag == "ground")
                    {
                        p_movement.animateCollision("groundLeave", _size);
                    }

                    // obstacle
                    if(collision.gameObject.tag == "obstacle")
                    {
                        p_movement.animateCollision("obstacleLeave", _size, collision.gameObject);
                    }
                    

                    // taptap
                    if(collision.gameObject.tag == "tapTapJump")
                        p_movement.animateCollision("tapTapJumpExit", _size, collision.gameObject);

                    // Land
                    if(collision.gameObject.tag == "fallBox")
                    {
                        p_movement.animateCollision("landExit", _size, collision.gameObject);
                    }  

                    break;

                case "sidewall":
                    // Wall run exit
                    if(collision.gameObject.tag == "ground" || collision.gameObject.tag == "ramp")
                    {
                        p_movement.animateCollision("wallRunExit", _size, collision.gameObject);
                        // FindObjectOfType<PlayerVectors>().slippery_trigr(true, collision.gameObject);
                        p_vectors.slippery_trigr(true, collision.gameObject);
                        c_movement.wal_rn_offset(true, collision.gameObject.transform);
                    }

                    // SlideWall exit
                    if(collision.gameObject.tag == "slider")
                    {
                        p_movement.animateCollision("sliderLeave", _size, collision.gameObject);
                    }
                    break; 

                case "slider":
                    // slider
                    if(collision.gameObject.tag == "slider")
                    {   
                        p_movement.animateCollision("groundLeave", _size);
                        p_movement.animateCollision("sliderLeave", _size);
                        c_movement.sld_offset(true);

                        psCollisions_movement.player_paricleArray(psCollisions_movement.player_particls[0].slide, false, "", true);
                    }

                    // rail
                    if(collision.gameObject.tag == "slideRail")
                    {
                        p_movement.animateCollision("railSlideExit", _size);
                    }

                    // ramp   
                    if(collision.gameObject.tag == "ramp")
                    {
                        p_movement.animateCollision("rampSlideExit", _size, collision.gameObject);
                        c_movement.rmp_slid_offst(true, collision.gameObject.transform);
                    }
                    break;

                case "frontwall":
                    // ladder
                    if(collision.gameObject.tag == "ladder")
                    {
                        p_movement.animateCollision("ladderLeave", _size, collision.gameObject);
                    }
                    break;

                default:
                    break;
            }
        }
    }
    private void clearLastWall() { lst_wall  = 0; }
    private IEnumerator delay_trgrs(float dl)
    {
        yield return new WaitForSeconds(dl);
        can_trgr = true;
    }




    // ---------------------------------
    //          PLAY PARTICLES
    // ---------------------------------
    public void player_paricleArray( ParticleSystem[]? ps = null, bool fromPlyrMovement_Invoke = false, 
        string invoke_paremeter = "", bool is_leave = false, int optional_param = -1)
    {
        int limit_ps = 0;

        if(fromPlyrMovement_Invoke)
        {
            switch(invoke_paremeter)
            {
                case "dblJump":
                    ps = player_particls[0].doubleJump;
                    break;
                case "jump":
                    ps = player_particls[0].jump;
                    break;
                case "animKill":
                case "killstreak":
                    ps = player_particls[0].kill;
                    break;
                case "combo":
                    ps = player_particls[0].combo;
                    break;
                case "coin":
                    return;
                    // ps = player_particls[0].coin;
                    break;
                case "healthBig":
                case "healthSmall":
                    ps = player_particls[0].health;
                    break;
                case "bigAmmo":
                case "lightAmmo":
                    ps = player_particls[0].ammo;
                    break;
                case "death_FrontSmash":
                case "death_Damage":
                case "death_SideVoid":
                    ps = player_particls[0].deaths;
                    break;
            }
        }
        if(invoke_paremeter == "healthSmall" || invoke_paremeter == "animKill")
            limit_ps = 1;

        for(int i = 0; i < ((limit_ps != 0 ) ? limit_ps : ps.Length); i ++)
        {
            ParticleSystem p = ps[i];

            if(p != null)
            {
                if(is_leave)
                    p.Stop();
                else
                    p.Play();
            }
        }
    }



    // ---------------------------------
    //          PLAYER RAGDOLL
    // ---------------------------------
    public void set_playerRagdoll(bool v_)
    {
        if(slcted_clsion == "boxAutoAim")
        {
            Transform pico_chan  = GameObject.FindGameObjectsWithTag("player_character")[0].GetComponent<Transform>();
        
            Rigidbody[] pico_rb = pico_chan.GetComponentsInChildren<Rigidbody>();
            Collider[] pico_cldr = pico_chan.GetComponentsInChildren<Collider>();
            Animator a_ = transform.root.GetComponent<Animator>();
            // CharacterJoint[] pico_cjoint = pico_chan.GetComponentsInChildren<CharacterJoint>();

            // Colliders
            for(int c = 0; c < pico_cldr.Length; c++)
            {
                pico_cldr[c].enabled = v_;
            }   

            // Rigidbodies
            for(int r = 0; r < pico_rb.Length; r ++)
            {
                pico_rb[r].isKinematic = !(v_);
                pico_rb[r].useGravity = v_;
            }

            if(v_)
            {
                a_.enabled = false;
                //     for(int cj = 0; cj < ((v_) ? pico_characterJoints.Length : pico_cjoint.Length); cj ++)
                //     {
                //         // pico_cjoint[cj].enableProjection = v_;
                //         // pico_cjoint[cj].enablePreprocessing = v_;
                //         // pico_cjoint[cj].enableCollision = v_;
                //     }
            }
    }}

}
