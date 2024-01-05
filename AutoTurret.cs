using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq; // Make 'Select' extension available

public class AutoTurret : MonoBehaviour
{
    private GameObject plyr_gm;
    private PlayerMovement plyr_pm;


    [Header ("Turret Type")]
    public static List<string> turret_types = new List<string>(new string[] {"Normal","Double",
    "Catapult", "Heavy", "Sniper", "Gattling", "LightGun", "Military"});
    private bool plyr_n_sight = false;


    [Header ("Turret Health")]
    [HideInInspector] public int turret_maxHealth = 0;
    private int turret_health = 0;
    public int get_health {
        get { return turret_health; } 
        set { }  // set method
    }
    private bool turret_exploded = false;

    private enum turret_Type{
        Normal,
        Double,
        Catapult,
        Heavy,
        Sniper,
        Gattling,
        LightGun,
        Military,
        Robot,
        Blast
    };
    [SerializeField]
    turret_Type t_type = new turret_Type();
    
    [Header ("Attached Turret Lists")]
    int i = 0, j = 0, k = 0, l = 0, p = 0;
    private List<GameObject> tr_barrels = new List<GameObject>(new GameObject[] {null, null, null});
    private List<GameObject> tr_sht_points = new List<GameObject>(new GameObject[] {null, null, null, null, null, null, null, null, null});
    private List<Transform> tr_body = new List<Transform>(new Transform[] {null, null, null});
    private List<Transform> tr_stand = new List<Transform>(new Transform[] {null, null, null});

    [Header ("Turret FireRate")]
    public float shootCoolDown;
    private float timer;

    [Header ("Turret Rotations")]
    private List<float> randomRot_a = new List<float>(new float[] {0.0f, 0.0f, 0.0f});
    private bool hz_sens = false;

    private List<float> randomRot_vrt = new List<float>(new float[] {0.0f, 0.0f, 0.0f});
    private bool vrt_sens = false;

    private float tr_aimSpeed = 60.2f;
    private List<Vector3> strt_rt = new List<Vector3>(new Vector3[] {new Vector3(0,0,0),new Vector3(0,0,0), new Vector3(0,0,0)} );

    [Header ("Turret Projectile")]
    public GameObject turret_bullet;


    [Header ("Turret Orientation")]
    public bool is_horizontal;
    public bool is_left;
    private bool reset_bl = false;

    [Header ("Turret Range")]
    public float rng_;

    [Header ("Turret PartSystems")]
    private ParticleSystem[] fire_ = new ParticleSystem[10];
    private ParticleSystem[] explosions_;


    [Header ("Turret Damages Text")]
    private Transform turret_textsParent;
    [SerializeField] private GameObject dmg_text_prefab;

    private Transform stored_player_trnsfrm;

    [Header ("Public Turret Informations")]
    [HideInInspector] public string turret_name;
    [HideInInspector] public int turret_level;

    [Header ("Stored Player Transform")]
    private Transform PLAYER_;

    [Header ("GameUI")]
    private GameUI g_ui;

    private void Start()
    {
        plyr_gm = GameObject.FindGameObjectsWithTag("Player")[0];
        PLAYER_ = plyr_gm.transform;
        plyr_pm = FindObjectOfType<PlayerMovement>();
        g_ui = FindObjectOfType<GameUI>();

        if(is_left != true) is_left = false;

        InvokeRepeating("target_check", 0, 0.60f);
        //InvokeRepeating("shoot_prjcle", 0, 0.50f);
        // ASSIGNEMENT OF BARRELS AND SHOOTS POINTS
        Transform[] tr_trsnfrms = GetComponentsInChildren<Transform>();

        foreach(Transform chld_ in tr_trsnfrms)
        {
            GameObject chld_g = chld_.gameObject;
            if(chld_g?.tag == "tr_Barrel")
            {
                tr_barrels[i] = chld_.gameObject;
                 i++;
            }  
            if(chld_g?.tag == "tr_Shootp")
            {
                tr_sht_points[j] = chld_g; 
                ParticleSystem o = chld_g.GetComponentInChildren<ParticleSystem>();
                if(o != null)
                {
                    fire_[p] = o;
                    p ++;
                }
                 j++;
            }
            if(t_type != turret_Type.Catapult)
            {
                if(is_horizontal ? chld_g?.tag == "tr_BarrelHz" : chld_g?.tag == "tr_BarrelHz")
                {
                    tr_body[k] = chld_;
                    k++; 
                }
            }else{
                if(chld_g?.tag == "tr_Body"){
                    tr_body[k] = chld_;
                    k++; 
                } 
            }
            if(chld_g?.tag == "tr_Stand"){
                tr_stand[l] = chld_;
                l ++;
            }

            if(chld_g?.tag == "Turret_Texts"){turret_textsParent = chld_;}
        }

        ParticleSystem[] tr_ps = GetComponentsInChildren<ParticleSystem>();
        ParticleSystem[] ps_map = tr_ps.Select((p_s) => (p_s.transform.parent.gameObject.tag == "tr_Shootp") ? null : p_s).ToArray();
        explosions_ = ps_map;

        // HORIZONTAL START X-AXIS DEGREE GAP
        float randm_flt_a =  UnityEngine.Random.Range(40f, 70f);
        float randm_flt_vrt =  UnityEngine.Random.Range(23f, 42f);
        for(int b = 0; b < k; b ++)
        {
            randomRot_a[b] = is_horizontal ? randm_flt_a : (tr_body[k-1].rotation.y + randm_flt_a);
            randomRot_vrt[b] = tr_body[b].rotation.eulerAngles.x + randm_flt_vrt;

            strt_rt[b] = tr_body[b].eulerAngles;
        }
        // Reset X-Axis 0deg rotation [Horizontal]
        if(is_horizontal && (t_type == turret_Type.Normal || t_type == turret_Type.Double) ) tr_body[k - 1].localRotation = Quaternion.Euler(0, tr_body[k - 1].localRotation.y, 0);

        assignTurret_attributes();
    }



    // FixedUpdate is called for physics (around 3x time per frame)
    private void  FixedUpdate()
    {
        if(plyr_n_sight){
            follow_target(false);
        }else{
            follow_target(true);
        }

        // BLOCK Z-AXIS
        //tr_body[k - 1].rotation = Quaternion.Euler(tr_body[k - 1].rotation .x, tr_body[k - 1].rotation.y, 0);

        timer += Time.deltaTime;
        if (timer >= shootCoolDown)
        {
            if (plyr_n_sight)
            {
                timer = 0;
                reset_bl = false;
                shoot_prjcle();
            }
        }

        if(!plyr_n_sight && ( (t_type == turret_Type.Normal || t_type == turret_Type.Double) ))
        {
            if(!reset_bl)
            {
                for(int c = 0; c < k; c ++)
                    tr_body[c].rotation = Quaternion.Euler(strt_rt[c].x , -180, strt_rt[c].z);
                reset_bl = true;
            }
        }


        if(turret_textsParent!= null) turret_textsParent.rotation = Quaternion.Euler(0, 0, 0);

        // dmg texts rotations
        if(turret_textsParent.childCount > 0 && turret_textsParent != null)
        {
            // for(int txt_r = 0; txt_r < (( turret_textsParent.childCount ) ); txt_r ++)
            // {
            //     if(stored_player_trnsfrm != null)
            //     {
            //         GameObject txt = turret_textsParent.GetChild(txt_r).gameObject;
            //         RectTransform txt_rect = txt.GetComponent<RectTransform>();

            //         // txt_rect.transform.LookAt(stored_player_trnsfrm, Vector3.left);

            //         Quaternion rt_ = Quaternion.LookRotation(stored_player_trnsfrm.position);
            //         // txt_rect.rotation = Quaternion.Slerp(
            //         //     txt_rect.rotation,
            //         //     Quaternion.Euler(0f, rt_.eulerAngles.y, 0f)
            //         //  , 0.1f
            //         // );
            //     }
            // }
            // Quaternion rt_a = Quaternion.LookRotation(stored_player_trnsfrm.position);
            // turret_textsParent.rotation = Quaternion.Slerp(
            //     turret_textsParent.rotation,
            //         Quaternion.Euler(0f, rt_a.eulerAngles.y, 0f)
            //     , 0.5f
            // );
            // turret_textsParent.LookAt(stored_player_trnsfrm);

        }
    }


    private void follow_target(bool is_idle) //todo : smooth rotate
    {
        for(int a = 0; a < k; a ++)
        {
            // IDLE TURRET ROTATION
            if(is_idle)
            {    

                // Y-AXIS ROTATION for Normal--Double
                // AND FOR VERTICAL => [Sniper, Heavy, Military]
                if( (t_type == turret_Type.Normal || t_type == turret_Type.Double) || (!is_horizontal) )
                {
                    float y_rot = !is_horizontal ? ((tr_body[a].localRotation.eulerAngles.y < 180f) ? 
                        (tr_body[a].localRotation.eulerAngles.y) : (-1 * (360 - tr_body[a].localRotation.eulerAngles.y)) )
                            : 
                        (tr_body[a].localRotation.eulerAngles.x > 270f) ? 
                            (-1* (360 - tr_body[a].localRotation.eulerAngles.x)) : (tr_body[a].localRotation.eulerAngles.x )
                    ;
                            
                    if(!hz_sens && (y_rot > randomRot_a[a]) ) hz_sens = true;
                    if(hz_sens && (y_rot < (is_horizontal ? -1* (randomRot_a[a] + 25) : -randomRot_a[a]) ) ) hz_sens = false; // HORIZONTAL GOT 35 DEGRES MORE ON HZ_SENS TRUE

                    float h_agl_add = hz_sens ? -0.45f : 0.45f; // HORIZONTAL

                    tr_body[a].Rotate(0f, h_agl_add, 0f, Space.World);

                    // rt_stand [NON-HORIZONTAL]
                    if(!is_horizontal && l > 0 && (t_type == turret_Type.Normal || t_type == turret_Type.Double) ) tr_stand[l - 1].Rotate(0f, h_agl_add, 0f, Space.World);
                }


                // VERT-AXIS ROTATION for NON Normal--Double
                // ONLY FOR HORIZONTAL => [Sniper, Heavy, Military]
                if(is_horizontal){
                    if(t_type != turret_Type.Normal && t_type != turret_Type.Double ){
                        
                        float rt_v = ( tr_body[a].rotation.eulerAngles.x < 250f ? 
                            (tr_body[a].rotation.eulerAngles.x + 360f) : tr_body[a].rotation.eulerAngles.x
                        );

                        float rot_rest = Math.Abs(strt_rt[a].x - (randomRot_vrt[a]) );
                        if(a == 0){
                            //  Debug.Log(rt_v );
                            //  Debug.Log("vs " + (randomRot_vrt[a]) );
                        }
                        if(!vrt_sens && (rt_v < randomRot_vrt[a] - 2 * rot_rest) ) { vrt_sens = true; }
                        if(vrt_sens && (rt_v > randomRot_vrt[a]) ) { vrt_sens = false; };

                        float v_agl_add = vrt_sens ? -0.45f : 0.45f; // VERT
                        tr_body[a].Rotate(v_agl_add, 0f, 0f, Space.World);
                    }
                }
                // break;
                // return;

            }else
            {  
                ////////////////////////////////////////////

                // TURRET AUTO-AIM
                // ONLY FOR 1ST TR_BARRELHZ !!
                if(a == 0)
                {
                    Vector3 target_Dir = new Vector3(0,0,0);
                    if(is_horizontal){
                        target_Dir = new Vector3(plyr_gm.transform.rotation.x, 
                                                plyr_gm.transform.rotation.y,
                                            0f) ;
                    }

                    Vector3 trgt = is_horizontal ? target_Dir :  plyr_gm.transform.position;
                    if(is_horizontal)
                    {
                        tr_body[a].LookAt(plyr_gm.transform, 
                            ( (120f < strt_rt[a].y) && (strt_rt[a].y <= 220f) ) ? Vector3.right : Vector3.left);
                    }else
                    {   
                        // FIX FOR VERTICAL MILITARY & BLAST
                        if(t_type == turret_Type.Military || t_type == turret_Type.Blast )
                        {
                            Vector3 relativePos = plyr_gm.transform.position - transform.position;
                            Quaternion r_= Quaternion.LookRotation(relativePos);

                            tr_body[a].localRotation =  Quaternion.Euler (
                                (t_type == turret_Type.Military ? 90f : -25f) + 
                                    (-1 * (r_.eulerAngles.x > 180 ? r_.eulerAngles.x - 360 : r_.eulerAngles.x) ),
                                180 +  (t_type == turret_Type.Military ?
                                r_.eulerAngles.y : 1.15f * r_.eulerAngles.y), 
                                0f
                            ); 
                        }else
                        {   tr_body[a].LookAt(plyr_gm.transform);   }
                    }   
                }else
                {
                    // just [VERT or HORIZONTAL] rotation for Stand
                    Vector3 newD_ = Vector3.RotateTowards(transform.forward, plyr_gm.transform.position, Time.deltaTime * 12f, 0.0f);
                    // tr_body[a].rotation = Quaternion.LookRotation(newD_);
                    // tr_body[a].rotation = Quaternion.Euler(tr_body[a].eulerAngles.x, tr_body[a].eulerAngles.y, 0f);
                    if(is_horizontal)
                    {
                      tr_body[a].LookAt(plyr_gm.transform, 
                            ( (120f < strt_rt[a].y) && (strt_rt[a].y <= 220f) ) ? Vector3.right : Vector3.left);
                    }else{
                        tr_body[a].LookAt(plyr_gm.transform);
                    }

                }
    
      
            }
        }
        
    }



    private void shoot_prjcle()
    {
        if(plyr_n_sight)
        {
            try{
                Vector3 msl_scale = turret_bullet.transform.localScale;
                if(fire_[0] != null && (t_type != turret_Type.Military) && (t_type != turret_Type.Double) && (t_type != turret_Type.Gattling)
                    && (t_type != turret_Type.Blast)
                 ) 
                {
                    fire_[0].Stop();
                    fire_[0].Play();
                };

                switch(t_type){
                    case turret_Type.Normal:
                        //Instantiate()
                        if(! (LeanTween.isTweening(tr_barrels[0]) ))
                        {
                        LeanTween.scale(tr_barrels[0], tr_barrels[0].transform.localScale * 1.6f, shootCoolDown - 0.4f).setEasePunch();
                        }
                        //Quaternion r_  = Quaternion.Euler(tr_body[0].rotation.eulerAngles.x + 180, tr_body[0].rotation.eulerAngles.y, tr_body[0].rotation.eulerAngles.z);
                        GameObject missle_Go = Instantiate(turret_bullet, tr_sht_points[0].transform.position, tr_sht_points[0].transform.rotation);
                        //typeof(GameObject) as GameObject;
                        missle_Go.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                            msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                        );
                        if(!missle_Go) return;

                        A_T_Projectile projectile_scrpt = missle_Go.GetComponent<A_T_Projectile>();
                        projectile_scrpt.plyr_target = plyr_gm.transform;
                        projectile_scrpt.blt_type = A_T_Projectile.turret_Type.Normal;
                        break;
                    case turret_Type.Double:
                        StartCoroutine(double_(msl_scale));
                        // for(int i = 0; i < j; i ++)
                        // {
                        //     LeanTween.scale(tr_barrels[i], tr_barrels[i].transform.localScale * 1.6f, shootCoolDown - 0.4f).setEasePunch();

                        //     GameObject m_Go = Instantiate(turret_bullet, tr_sht_points[i].transform.position, tr_sht_points[i].transform.rotation);
                        //     m_Go.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                        //         msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                        //     );

                        //     A_T_Projectile pj_scrpt = m_Go.GetComponent<A_T_Projectile>();
                        //     pj_scrpt.plyr_target = plyr_gm.transform;
                        //     pj_scrpt.blt_type = A_T_Projectile.turret_Type.Double;
                        // }
                        break;
                    case turret_Type.Heavy:
                        if(! (LeanTween.isTweening(tr_body[0].gameObject )) )
                        {
                            LeanTween.scale(tr_body[0].gameObject, tr_body[0].localScale * 1.35f, shootCoolDown - 0.03f).setEasePunch();
                        }
                        GameObject missle_Go_h = Instantiate(turret_bullet, tr_sht_points[0].transform.position, tr_sht_points[0].transform.rotation);
                        // missle_Go.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                        //     msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                        // );
                        if(!missle_Go_h) return;

                        A_T_Projectile proj_scrpt_h = missle_Go_h.GetComponent<A_T_Projectile>();
                        proj_scrpt_h.plyr_target = plyr_gm.transform;
                        proj_scrpt_h.blt_type = A_T_Projectile.turret_Type.Heavy;
                        break;
                    case turret_Type.Sniper:
                        if(! (LeanTween.isTweening(tr_body[0].gameObject) ))
                        {
                            LeanTween.scale(tr_body[0].gameObject, tr_body[0].localScale * 1.17f, shootCoolDown - 0.4f).setEasePunch();
                        }
                        GameObject missle_Go_s = Instantiate(turret_bullet, tr_sht_points[0].transform.position, tr_sht_points[0].transform.rotation);
                        if(!missle_Go_s) return;

                        missle_Go_s.transform.localScale = new Vector3( msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                            msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                        );

                        A_T_Projectile proj_scrpt_s = missle_Go_s.GetComponent<A_T_Projectile>();
                        proj_scrpt_s.plyr_target = plyr_gm.transform;
                        proj_scrpt_s.blt_type = A_T_Projectile.turret_Type.Sniper;
                        break;
                    case turret_Type.Gattling:
                        StartCoroutine(gtlng_(msl_scale));
                        break;
                    case turret_Type.Catapult:

                        //Vector3 throw_dst = CalculateCatapult(plyr_gm.transform.position, gameObject.transform.position, 1f);
                        break;
                    case turret_Type.LightGun:
                        
                        break;
                    case turret_Type.Military:
                        StartCoroutine(military_sh(msl_scale));
                        break;
                    case turret_Type.Blast:
                        StartCoroutine(blast_sh(msl_scale));
                        break;
                    case turret_Type.Robot:

                        break;
                }
            }catch(Exception err){
                Debug.Log(err);
                //Debug.Log("no turret type");
            }
        }
    }
    


    // double
    private IEnumerator double_(Vector3 msl_scale)
    {
        for(int i = 0; i < j; i ++)
        {
            fire_[i].Play();
            if(! (LeanTween.isTweening(tr_barrels[i]) ))
            {    
                LeanTween.scale(tr_barrels[i], tr_barrels[i].transform.localScale * 1.6f, shootCoolDown - 0.4f).setEasePunch();
            }
            GameObject m_Go = Instantiate(turret_bullet, tr_sht_points[i].transform.position, tr_sht_points[i].transform.rotation);
            m_Go.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
            );

            A_T_Projectile pj_scrpt = m_Go.GetComponent<A_T_Projectile>();
            pj_scrpt.plyr_target = plyr_gm.transform;
            pj_scrpt.blt_type = A_T_Projectile.turret_Type.Double;
            
            yield return new WaitForSeconds(0.65f);
        }
        yield break;
    }

    // gattling 
    private IEnumerator gtlng_(Vector3 msl_scale)
    {
        for(int i = 0; i < j; i ++)
        {
            tr_barrels[0].transform.Rotate(1,0,0, Space.World);
            fire_[i].Play();

            GameObject missle_Go_g = Instantiate(turret_bullet, tr_sht_points[i].transform.position, tr_sht_points[i].transform.rotation);
            if(!missle_Go_g) yield break;

            missle_Go_g.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                            msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                    );


            A_T_Projectile proj_scrpt_g = missle_Go_g.GetComponent<A_T_Projectile>();
            proj_scrpt_g.plyr_target = plyr_gm.transform; 
            proj_scrpt_g.blt_type = A_T_Projectile.turret_Type.Gattling;

            yield return new WaitForSeconds(shootCoolDown / 7);
        }
        yield break;
    }



    // military
    private IEnumerator military_sh(Vector3 msl_scale)
    {
        for(int i = 0; i < j/2; i ++) // 4 loop
        {
            for(int z = 0; z < 2; z ++)
            {
                fire_[3 * z + i].Play();

                GameObject missle_Go_g = Instantiate(turret_bullet, tr_sht_points[3 * z + i].transform.position, tr_sht_points[3 * z + i].transform.rotation);
                if(!missle_Go_g) yield break;

                missle_Go_g.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                                msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                );

                if(plyr_gm.transform == null) yield break;
                A_T_Projectile proj_scrpt_g = missle_Go_g.GetComponent<A_T_Projectile>();
                proj_scrpt_g.plyr_target = plyr_gm.transform; 
                proj_scrpt_g.blt_type = A_T_Projectile.turret_Type.Military;

            }
            yield return new WaitForSeconds((shootCoolDown / 2) / 4);
        }
        yield break;
    }


    // blast
    private IEnumerator blast_sh(Vector3 msl_scale)
    {
        for(int i = 0; i < j; i ++) // 4 loop
        {
                fire_[i].Play();

                LeanTween.scale(tr_barrels[0].gameObject, tr_barrels[0].transform.localScale * 1.1f, (shootCoolDown/ 3) / 4 ).setEasePunch();
                GameObject missle_Go_g = Instantiate(turret_bullet, tr_sht_points[i].transform.position, tr_sht_points[i].transform.rotation);
                if(!missle_Go_g) yield break;

                missle_Go_g.transform.localScale = new Vector3(msl_scale.x * (4*(gameObject.transform.localScale.x / 5)), msl_scale.y * (4*(gameObject.transform.localScale.y / 5)),
                                msl_scale.z * (4*(gameObject.transform.localScale.z / 5))
                );


                A_T_Projectile proj_scrpt_g = missle_Go_g.GetComponent<A_T_Projectile>();
                proj_scrpt_g.plyr_target = plyr_gm.transform; proj_scrpt_g.blt_type = A_T_Projectile.turret_Type.Gattling;

            
            yield return new WaitForSeconds((shootCoolDown /3) / 4);
        }
        yield break;
    }



    private void target_check()
    {
        // OvSphere => OvSphereNonAlloc
        const int maxColliders = 50;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, rng_, hitColliders);

        // Collider[] colls = Physics.OverlapSphere(transform.position, rng_);

        //float distAway = Mathf.Infinity;
        bool s = false;
        for (int i = 0; i < maxColliders; i++)
        {
            if(hitColliders[i] != null)
            {
                if (hitColliders[i].tag == "player_hitbx")
                {
                    plyr_gm = hitColliders[i].gameObject;
                    plyr_n_sight = true; s = true;
                    break;
                }
            }else{ break; }

        }
        if (!s){plyr_n_sight = false;}
    }

    private void turret_explode()
    {
        turret_exploded = true;
        Transform[] g_l = gameObject.GetComponentsInChildren<Transform>();
        // turn off instantly aim collider
        Collider p = gameObject.GetComponent<Collider>();
        if(p!=null && p.isTrigger && p.enabled == true) p.enabled = false;
        
        for (int i = 0; i < g_l.Length; i++)
        {
            GameObject go_ = g_l[i].gameObject;
            // Debug.Log(go_.name);
            if(go_.GetComponent<ParticleSystem>() != null || go_ == null
             || go_.name.Contains("T.A.R.G.E.T") || 
             go_.name.Contains("TurretDmgg") || go_.name.Contains("Turret_Texts")
             || (g_l[i] == transform)
            ){ continue; }

            if(g_l[i].parent != gameObject.transform) 
                g_l[i].SetParent(gameObject.transform);

            if(g_l[i].gameObject.tag == "tr_Shootp") g_l[i].gameObject.SetActive(false);
            
            Rigidbody alr = g_l[i].gameObject.GetComponent<Rigidbody>();
            Rigidbody turret_part = (alr != null) ? alr : g_l[i].gameObject.AddComponent<Rigidbody>();
            if(turret_part == null) continue;

            MeshCollider msh_cldr = g_l[i].gameObject.GetComponent<MeshCollider>();
            if(msh_cldr == null)
            {
                MeshFilter msh_fltr =  g_l[i].gameObject.GetComponent<MeshFilter>();
                if(msh_fltr != null)
                {
                    msh_cldr = g_l[i].gameObject.AddComponent<MeshCollider>();
                    Mesh msh_rndr = msh_fltr?.mesh;
                    if(msh_rndr != null)
                    {
                        msh_cldr.sharedMesh = msh_rndr; 
                        msh_cldr.convex = true;
                        msh_cldr.enabled = true;
                    }
                };
            }

            float x_ =  UnityEngine.Random.Range(-10f, 10f); float y_ =  UnityEngine.Random.Range(9f, 24f);
            float z_ =  UnityEngine.Random.Range(-7f, 7f);
            turret_part.AddForce(x_, y_, z_, ForceMode.Impulse);
            turret_part.AddTorque(-x_, -y_, -z_, ForceMode.Impulse);


            float exl_ = UnityEngine.Random.Range(0.5f, 0.7f);
            float exl_t = UnityEngine.Random.Range(1f, 3f);
            LeanTween.scale(go_, g_l[i].localScale * exl_, exl_t).setEaseInSine();

            if(go_.GetComponent<MeshFilter>() != null )
            {
                //LeanTween.alphaVertex(go_, 0.0f, exl_t + 0.3f).setEaseInSine();
            }
            // dont scale to absolute zero [physics can't handle 0 scaled meshes(Rigidbody) ]
            LeanTween.scale(go_, new Vector3(0.01f, 0.01f, 0.01f), 3f).setEaseInSine();
        }


        // explosions_[0].Play();
        // explosions_[1].Play();

        plyr_pm.player_kill();
        for(int j = 0; j < explosions_.Length; j ++)
        {
            if(explosions_[j] != null)
            {
                explosions_[j].Play();
            };
        }
    }


    public void turret_damage(string dmg_status, float wpn_damage, GameObject turret_part, Transform player_, bool is_crit)
    {
        if(turret_exploded) return;


        if(stored_player_trnsfrm == null) stored_player_trnsfrm = player_;
        LeanTween.scale(turret_part, turret_part.transform.localScale * 1.15f, 0.3f).setEasePunch();

        int dmg = 0;
        switch(dmg_status)
        {
            case "tr_Shootp":
                dmg = (int) (wpn_damage * 1.90f);
                break;
            case "tr_Stand":
                dmg = (int) (wpn_damage * 0.75f);
                break;
            case "TURRET":
            case "tr_Body":
                dmg = (int) (wpn_damage * 1f);
                break;
            case "tr_Barrel":
            case "tr_BarrelHz":
                float a_ = UnityEngine.Random.Range(0.85f, 1.18f);
                dmg = (int) (wpn_damage * a_);
                break;
            case "tr_Plate":
                dmg = (int) (wpn_damage * 0.6f);
                break;
            case "tr_Radar":
                float d_ = UnityEngine.Random.Range(1.4f, 1.70f);
                dmg = (int) (wpn_damage * d_);
                break;
            case "tr_Cannon":
                dmg = (int) (wpn_damage);
                break;
        }
        int b_ = UnityEngine.Random.Range(-7, 7);
        dmg += b_;

        float barrel_y = ( (tr_body[0] != null) ? 
            (is_horizontal) ? tr_body[0].transform.rotation.eulerAngles.x : (tr_body[0].transform.rotation.eulerAngles.y + 180f)
                : 
            tr_barrels[0].transform.rotation.eulerAngles.y
        );
        float barrel_x = ( (tr_body[0] != null) ? 
            (is_horizontal) ? tr_body[0].transform.rotation.eulerAngles.y : tr_body[0].transform.rotation.eulerAngles.x
                :  tr_barrels[0].transform.rotation.eulerAngles.x
        );

        if(dmg_text_prefab == null) return;

        GameObject dmg_textObj = Instantiate(dmg_text_prefab, Vector3.zero, Quaternion.Euler(0, barrel_y, 0), turret_textsParent);
        RectTransform m_RectTransform = dmg_textObj.GetComponent<RectTransform>();
        TextMeshPro dmg_text = dmg_textObj.GetComponent<TextMeshPro>();
        if(is_crit) dmg_text.color = new Color32(255, 128, 0, 255);

        float x_x = UnityEngine.Random.Range(-0.7f, 0.7f);

        bool plyr_belowTrt =  (transform.position.y - player_.position.y) > 6.5f ? true : false;

        if(m_RectTransform != null)
        {
            m_RectTransform.transform.localPosition = new Vector3 (
                ((is_horizontal) ? (transform.rotation.eulerAngles.y > 150f) ? (1.5f): (-1.5f) : 0f) + x_x,
                ((is_horizontal) ? (transform.rotation.eulerAngles.y > 150f) ?
                    (plyr_belowTrt ? -1.35f : 1.35f): (plyr_belowTrt ? -1f : 1f) 
                : (plyr_belowTrt) ? -2.30f : 2.30f),
                -1f
            ); 
        }


        dmg_text.text = dmg.ToString();
        float r_x = UnityEngine.Random.Range(-1.8f, 1.8f);
        float r_y = (plyr_belowTrt) ? UnityEngine.Random.Range(-0.6f, -1.8f) : UnityEngine.Random.Range(0.6f, 2.8f);
        Vector3 rdm_move = new Vector3(
            dmg_textObj.transform.localPosition.x + r_x,
            dmg_textObj.transform.localPosition.y + r_y,
            0f
        );
        
        float r_s = UnityEngine.Random.Range(1.3f, 2f);
        LeanTween.scale(dmg_textObj, dmg_textObj.transform.localScale * r_s, 0.8f).setEasePunch();
        LeanTween.moveLocal(dmg_textObj, rdm_move, 1.2f).setEaseInSine();
        //LeanTween.moveLocal(dmg_textObj, new Vector3(rdm_move.x, -1 * rdm_move.y, 0), 1.5f).setEaseInSine();

        StartCoroutine(dmg_textDestruction(dmg_textObj));
        // LeanTween.alpha(dmg_textObj, 0, 2f);


        if( (turret_health - dmg )< 0)
        {
            turret_explode();
            turret_health = 0;
            g_ui.kill_ui();

        }else
        {
            turret_health = turret_health - dmg;
            turret_health = turret_health - 2;
        }



        OutlinePatched p = turret_part.GetComponent<OutlinePatched>();
        if(p == null && (!false))
        { 
            p = turret_part.AddComponent<OutlinePatched>();
            p.OutlineColor = Color.white;
            p.OutlineMode = OutlinePatched.Mode.OutlineVisible;
            StartCoroutine(outline_effect(p));
        }

    }

    private IEnumerator outline_effect(OutlinePatched outline_instance)
    {
        const float effect_time = 0.5f;
        const float outlineTick = 6f / 30;
        // outline_instance.OutlineWidth = 0f;
        // for(int i = 0; i < 30; i ++)
        // {
        //     outline_instance.OutlineWidth += outlineTick;
        //     yield return new WaitForSeconds(effect_time / 30);
        // }
        // outline_instance.OutlineWidth = 0f;

        outline_instance.OutlineWidth = 5f;
        yield return new WaitForSeconds(0.4f);
        Destroy(outline_instance);
    }

    private IEnumerator dmg_textDestruction(GameObject text_obj)
    {
        yield return new WaitForSeconds(0.5f);
        LeanTween.scale(text_obj, new Vector3(0, 0, 0), 0.5f).setEaseInSine();
        yield return new WaitForSeconds(0.5f);
        Destroy(text_obj);
    }
    
    private void assignTurret_attributes()
    {
       int h = 0;
       try{
            switch(t_type){
                case turret_Type.Normal:
                    h += 100;
                    turret_name = "SIMPLE ROCKET";
                    break;
                case turret_Type.Double:
                    h += 150;
                    turret_name = "DOUBLE ROCKET";
                    break;
                case turret_Type.Heavy:
                    h += 120;
                    turret_name = "HEAVY TURRET";
                    break;
                case turret_Type.Sniper:
                    h += 120;
                    turret_name = "SNIPER TURRET";
                    break;
                case turret_Type.Gattling:
                    h += 120;
                    turret_name = "MILITARY Gattling";
                    break;
                case turret_Type.Catapult:
                    h += 175;
                    turret_name = "CATAPULT TURRET";
                    break;
                case turret_Type.LightGun:
                    h += 80;
                    turret_name = "SMG Turret";
                    break;
                case turret_Type.Military:
                    h += 100;
                    turret_name = "MILITARY TURRET";
                    break;
                case turret_Type.Blast:
                    h += 135;
                    turret_name = "BLAST TURRET";
                    break;
                case turret_Type.Robot:
                    h += 90;
                    turret_name = "ROBOT TURRET";
                    break;
            }
        }catch(Exception err){
            Debug.Log(err);
        }

        h += UnityEngine.Random.Range(-10, 10); 

        
        turret_health = turret_maxHealth =  h;
        turret_level = UnityEngine.Random.Range(1, 5); 
    }
}
