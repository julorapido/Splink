using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using PathCreation.Utility;
using System;

public class PlayerCollisions : MonoBehaviour
{

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
    private const int player_attackRange = 50;

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

    private void Start()
    {
        m_Collider = gameObject.GetComponent<Collider>();
        StartCoroutine(delay_trgrs(strt_delay));
    }
  
 

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

            DisplayBox(transform.position,  
                new Vector3(3.8f, 8, player_attackRange), 
                wallRun_aimBox ? Quaternion.Euler(z_wallRun_aimRotation, transform.rotation.y, 0) : transform.rotation
            );

            Collider[] hitColliders = Physics.OverlapBox(transform.position,
                new Vector3(3.8f, 8, player_attackRange),
                wallRun_aimBox ? Quaternion.Euler(0, transform.rotation.y, z_wallRun_aimRotation) : transform.rotation
            );

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

                        if(zDist < 1f) continue;

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

    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
        // Gizmos.DrawWireCube(transform.position,  new Vector3(2 * 2, 6 * 2, 100 * 2) );
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

    private IEnumerator delay_trgrs(float dl)
    {
        yield return new WaitForSeconds(dl);
        can_trgr = true;
    }


}
