using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using PathCreation.Utility;
using System;

public class PlayerCollisions : MonoBehaviour
{
    //public Collider mmbr_collider;

    [Header ("Collisions Constants")]
    public string[] colsions_values = new string[7]{"ground", "frontwall","sidewall", "slider", "missiles", "collectibles", "boxAutoAim"};

    [Header ("SubCollisions Constants")]
    private string[] subcolsions_values = new string[5]{"obstacleHit", "launcherHit","tyro", "bumper", "tapTapJump"};

    [Header ("-> Selected Collision <-")]
    public string slcted_clsion;

    [Header ("Ground Physical Mat")]
    [HideInInspector] public PhysicMaterial grnd_mat;

    [Header ("Start Delay")]
    [SerializeField]  private Transform ply_transform;
    private float strt_delay = 0.5f;
    private bool can_trgr = false;
    private Vector3 currentVelocity;

    [Header ("Sider Wall last_registered_gm")]
    private int lst_wall;

    [Header ("Currently Aimed Enemy (Sphere)")]
    private GameObject sphereStored_aimed_turret;
    private GameObject sphere_aimed_turret;
    private int turretInSight = 0;
    private const int player_attackRange = 75;

    [Header ("Currently Auto-Aimed Enemy [Default]")]
    private Collider m_Collider;
    private RaycastHit[] m_Hits;
    private GameObject aimed_enemy;
    private GameObject storedAimed_enemy;
    private int enemy_inSight = 0;
    private bool firstEverDetectedEnemy = false;
    
    // private List<GameObject?> colliders_gm = new List<GameObject?>(new GameObject[7] {null, null, null, null, null, null, null});

    private void Start()
    {
        m_Collider = gameObject.GetComponent<Collider>();
        StartCoroutine(delay_trgrs(strt_delay));
    }
  
 

    private void FixedUpdate()
    {

        // player shots auto-aim
        if(slcted_clsion == "boxCastAutoAim")
        {
            enemy_inSight = 0;

            RaycastHit[] top = Physics.BoxCastAll(m_Collider.bounds.center, transform.localScale, transform.forward, transform.rotation, 200f);
            RaycastHit[] bottom = Physics.BoxCastAll(m_Collider.bounds.center, transform.localScale + new Vector3(0, -1, 0), transform.forward, transform.rotation, 200f);
            m_Hits  = new RaycastHit[top.Length + bottom.Length];
            top.CopyTo(m_Hits, 0);
            bottom.CopyTo(m_Hits, top.Length);

            for(int i = 0; i < m_Hits.Length; i ++)
            {
                // Debug.Log( (m_Hits[i].collider.name)  + " / " + m_Hits[i].collider.tag);

                if ( (m_Hits[i].collider.tag == "TURRET") )
                {
                    //Output the name of the enemy you hits
                    Debug.Log("Hit : " + m_Hits[i].collider.name);

                    aimed_enemy = m_Hits[i].transform.gameObject;
                    if(storedAimed_enemy != aimed_enemy)
                    {
                        storedAimed_enemy = aimed_enemy;
                        FindObjectOfType<PlayerMovement>().animateCollision("newEnemyAim", new Vector3(0, 0, 0), storedAimed_enemy);
                    }

                    enemy_inSight++;

                    break;
                }
            }

            if(enemy_inSight == 0 && (aimed_enemy != null) )
                FindObjectOfType<PlayerMovement>().animateCollision("emptyEnemyAim", new Vector3(0, 0, 0));
                aimed_enemy = null;
   
        }



        // Sphere around player auto-aim turrets
        if(slcted_clsion == "boxAutoAim")
        {
            float minDistance = float.MaxValue;
    
            // Detect Turrets & Enemies
            // Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, 50f);
            Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(m_Collider.bounds.size.x, m_Collider.bounds.size.y, player_attackRange), transform.rotation);
            if(hitColliders.Length > 0)
            {   
                turretInSight = 0;
                for (int i = 0; i < hitColliders.Length; i ++)
                {   
                    if(hitColliders[i].tag == "TURRET" || hitColliders[i].tag == "ENEMY")
                    {
                        Vector3 possiblePosition = hitColliders[i].transform.position;
                    
                        float currDistance = Vector3.Distance(transform.position, possiblePosition);
                        float zDist = possiblePosition.z - transform.position.z;

                        if(zDist < 3f) continue;

                        // If the distance is smaller than the one before...
                        if ( (currDistance < minDistance) )
                        {
                            sphere_aimed_turret = hitColliders[i].transform.gameObject;
                            minDistance = currDistance;
                        }

                        turretInSight++;
                    }
                }

                if( (sphereStored_aimed_turret != sphere_aimed_turret) && (!firstEverDetectedEnemy || (sphere_aimed_turret != null) ) )
                {
                    FindObjectOfType<PlayerMovement>().animateCollision("newEnemyAim", new Vector3(0, 0, 0), sphere_aimed_turret);
                    sphereStored_aimed_turret = sphere_aimed_turret;
                    firstEverDetectedEnemy = true;
                }

                if( turretInSight == 0  && (sphere_aimed_turret != null) )
                {
                    FindObjectOfType<PlayerMovement>().animateCollision("emptyEnemyAim", new Vector3(0, 0, 0));
                    sphere_aimed_turret = null;
                    sphereStored_aimed_turret = null;
                }
            }
        }



   
    }

//    private void OnDrawGizmos()
//     {
//         Gizmos.color = Color.red;

//         //Check if there has been a hit yet
//         if (aimed_enemy != null)
//         {
//             for(int j = 0; j < m_Hits.Length; j ++)
//             {
//                 //Draw a Ray forward from GameObject toward the hit
//                 Gizmos.DrawRay(transform.position, transform.forward * m_Hits[j].distance);
//                 //Draw a cube that extends to where the hit exists
//                 Gizmos.DrawWireCube(transform.position + transform.forward * m_Hits[j].distance, transform.localScale);
//             }
//         }
//     }

    // private void OnCollisionEnter(Collision other)
    // {
    //     Vector3 _size = other.collider.bounds.size;
    //     switch (slcted_clsion){
    //             case "ground":
    //                 // Grnd hit
    //                 if(other.collider.gameObject.tag == "ground")
    //                 {
    //                     FindObjectOfType<PlayerMovement>().animateCollision("groundHit", _size);
    //                     if(grnd_mat != null)
    //                     {
    //                         Collider p =  other.collider.gameObject.GetComponent<Collider>();
    //                         if(p){ p.sharedMaterial = grnd_mat;}
    //                     }
    //                 }
    //                 break;
    //             default:
    //                 break;
    //     }
    // }

    private void OnTriggerEnter(Collider collision)
    {
        if(slcted_clsion.Length > 0 && can_trgr)
        {
            Vector3 _size = collision.bounds.size;
            switch (slcted_clsion){
                case "ground":

                    // Grnd hit
                    if(collision.gameObject.tag == "ground")
                    {
                        FindObjectOfType<PlayerMovement>().animateCollision("groundHit", _size);
                        if(grnd_mat != null)
                        {
                            Collider p =  collision.gameObject.GetComponent<Collider>();
                            if(p){ p.sharedMaterial = grnd_mat;}
                        }
                    }
                    // Obstcl hit
                    if(collision.gameObject.tag == "obstacle") FindObjectOfType<PlayerMovement>().animateCollision("obstacleHit", _size, collision.gameObject);

                    // Launcher jmp
                    if(collision.gameObject.tag == "launcher") FindObjectOfType<PlayerMovement>().animateCollision("launcherHit", _size);

                    // Tyro hit
                    if(collision.gameObject.tag == "tyro") FindObjectOfType<PlayerMovement>().tyro_movement(collision.gameObject);

                    // Bumper jmp
                    if(collision.gameObject.tag == "bumper")
                    {
                        GameObject pr_gm = collision.gameObject.transform.parent.gameObject == null ? collision.gameObject.transform.parent.gameObject : collision.gameObject;
                        LeanTween.scale(pr_gm, pr_gm.transform.localScale * 1.2f, 0.4f).setEasePunch();
                        FindObjectOfType<PlayerMovement>().animateCollision("bumperJump", _size);
                    }

                    // Tap Tap Jump
                    if(collision.gameObject.tag == "tapTapJump") FindObjectOfType<PlayerMovement>().animateCollision("tapTapJump", _size, collision.gameObject);
                    
                    break;

                case "frontwall":

                    // front wall gameover
                    if(collision.gameObject.tag == "ground") FindObjectOfType<PlayerMovement>().animateCollision("frontWallHit", _size);

                    break;

                case "sidewall":
                    // Sidewall hit
                    if(collision.gameObject.tag == "ground")
                    {
                        if(lst_wall != collision.gameObject.GetInstanceID())
                        {
                            lst_wall = (collision.gameObject.GetInstanceID());
                            Vector3 targetDir = collision.gameObject.transform.position - ply_transform.position;
                            float angle = Vector3.Angle(targetDir, transform.forward);
                            FindObjectOfType<PlayerMovement>().animateCollision("wallRunHit", _size, collision.gameObject);
                            FindObjectOfType<PlayerVectors>().slippery_trigr(false, collision.gameObject);
                            FindObjectOfType<CameraMovement>().wal_rn_offset(false, collision.gameObject.transform);
                        }
                    }
                    break; 

                case "slider":
                    // Slider hit
                    if(collision.gameObject.tag == "slider")
                    {
                        Vector3 scaleV3 = new Vector3(collision.gameObject.transform.localScale.x * 1.4f, collision.gameObject.transform.localScale.y, collision.gameObject.transform.localScale.z * 1.4f);
                        LeanTween.scale(collision.gameObject, scaleV3, 0.85f).setEasePunch();

                        FindObjectOfType<PlayerMovement>().animateCollision("sliderHit", _size);
                        FindObjectOfType<CameraMovement>().sld_offset(false);
                    }  
                    break;


                case "collectibles":
                    // collectible hit
                    switch(collision.gameObject.tag)
                    {
                        case "coin":
                            FindObjectOfType<PlayerMovement>().animateCollision("sliderHit", _size);
                            break;
                        case "gun":
                            collision.gameObject.SetActive(false);
                            FindObjectOfType<PlayerMovement>().animateCollision("gun", _size);
                            break;
                    }
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
                    // Ground leave
                    if(collision.gameObject.tag == "ground") FindObjectOfType<PlayerMovement>().animateCollision("groundLeave", _size);
                    
                    // Obstacl leave
                    if(collision.gameObject.tag == "obstacle") FindObjectOfType<PlayerMovement>().animateCollision("obstacleLeave", _size, collision.gameObject);
                    
                    break;

                case "sidewall":
                    // Wall run exit
                    if(collision.gameObject.tag == "ground")
                    {
                        FindObjectOfType<PlayerMovement>().animateCollision("wallRunExit", _size);
                        FindObjectOfType<PlayerVectors>().slippery_trigr(true, collision.gameObject);
                        FindObjectOfType<CameraMovement>().wal_rn_offset(true, collision.gameObject.transform);
                    }
                    break; 

                case "slider":
                    // Slider leave
                    if(collision.gameObject.tag == "slider")
                    {
                        LeanTween.scale(collision.gameObject, collision.gameObject.transform.localScale * 1.08f, 1f).setEasePunch();
                        
                        FindObjectOfType<PlayerMovement>().animateCollision("groundLeave", _size);
                        FindObjectOfType<PlayerMovement>().animateCollision("sliderLeave", _size);
                        FindObjectOfType<CameraMovement>().sld_offset(true);
                    }  
                    break;

                default:
                    break;
            }
        }
    }

    private IEnumerator delay_trgrs(float dl){
        yield return new WaitForSeconds(dl);
        can_trgr = true;
    }
    
    private IEnumerator sid_wl_delay(){
        yield return new WaitForSeconds(0.6f);
    }
}
