using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System;
using PathCreation.Utility;
using PathCreation;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    [Header ("Transforms & Rb")]
    public GameObject plyr_;
    public Rigidbody plyr_rb;
    public Transform plyr_trsnfm;
    public Transform plyr_cam;
    public Transform aimed_enemy;
    private Transform lastAimed_enemy;
    
    [Header ("Movement Values")]
    private const float jumpAmount = 32f;
    private const float strafe_speed = 0.225f;

    [Header ("Character Animators")]
    [SerializeField] private GameObject pico_character;
    private Animator _anim = null;
    private CharacterController _controller = null;

    [Header ("Animation Center Fixes")]
    private Vector3[] colliders_references = new Vector3[10];

    [Header ("Stored Last Obstacle")]
    private GameObject last_ObstJumped;

    [Header ("Player Movements Status")]
    private bool plyr_flying = false;
    [HideInInspector] 
    public bool plyr_sliding = false;
    private bool plyr_wallRninng = false;
    private bool plyr_obstclJmping = false;
    private bool plyr_swnging = false;
    [HideInInspector] 
    public bool plyr_tyro = false;
    private bool plyr_intro_tyro= false;

    
    [Header ("Authorized Movements")]
    private bool jump_auth = true;
    private bool movement_auth = true;

    [Header ("Player Available Jumps")]
    private int jumpCnt = 2;

    [Header ("Player Attached Partcl")]
    public ParticleSystem jump_prtcl;
    public ParticleSystem doubleJump_prtcl;
    public ParticleSystem   slide_prtcl;


    [Header ("Rotate booleans")]
    private bool rt_auth = false;


    [Header ("Tyro Vars")]
    private float last_tyro_trvld = 0.05f;
    private Transform tyro_handler_child;
    private PathCreator actual_path;
    private const float tyro_speed = 11.5f;
    private Vector3 end_tyroPos;

    [Header ("Grapple Vars")]
    private Vector3 grap_pnt = new Vector3(0,0,0);
    private bool grpl_sns = false;

    [Space(10)] // 10 pixels of spacing here.

    [Header ("Game State")]
    private bool gameOver_ = false;

    [Header ("WallRun Forced Strafes")]
    private bool lft_Straf = false;
    private bool rght_Straf = false;

    [Space(10)] // 10 pixels of spacing here.

    [Header ("Player Gun")]
    [SerializeField] private GameObject player_hidden_gunSlot;
    [SerializeField] private GameObject player_hand_gunSlot;
    private int ammo = 0;

    [Space(10)]

    [Header ("Player Rigs & Constraints")]
    [SerializeField] private MultiAimConstraint[] player_aims;
    [SerializeField] private Animator rig_animController;
    private float[] noAim_aimsWeigths = new float[4]{0.80f, 1.0f, 0.0f, 0.0f};
    private float[] autoAim_aimsWeigths = new float[4]{1.0f,  0.50f, 0.0f, 0.0f};
    [SerializeField] private Transform[] headAndNeck;


    [Header ("Authorized Shooting Animations")]
    private string[] authorizedShooting_ = new string[4] { "gunRun", "flying", "slide", "wallRun" };

    [Header ("Player Targets")]
    [SerializeField] private Transform body_TARGET; 
    [SerializeField] private Transform head_TARGET; 




    private void Start()
    {
       _anim = GetComponentInChildren<Animator>();
       if (_anim == null) Debug.Log("nul animtor");


       Collider[] colList = gameObject.transform.GetComponentsInChildren<Collider>();
       for (int i = 0; i < colList.Length; i ++)
       {
            string[] coldr_type = colList[i].GetType().ToString().Split('.');
            BoxCollider b_cldr; SphereCollider s_cldr; //MeshCollider m_cldr;

            switch(coldr_type[1])
            {
                case "BoxCollider" :
                    b_cldr = colList[i].GetComponent<BoxCollider>();
                    colliders_references[i] =  b_cldr.center; 
                    break;
                case "SphereCollider" : 
                    s_cldr = colList[0].GetComponent<SphereCollider>();
                    colliders_references[i] = s_cldr.center;
                    break;
                case "MeshCollider" : break;
            }
            
        }


        rig_animController.enabled = false;
    }





    // Update is called once per frame
    private void Update()
    {

        // get jump input
        // movement auth && jump auth
        if(movement_auth && jump_auth)
        {
            if ( Input.GetKeyDown(KeyCode.Space) && (jumpCnt > 0) )
            {

                // JUMP
                if(jumpCnt == 2)
                {
                    StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.55f ) );

                    StartCoroutine(Dly_bool_anm(0.52f, "Jump"));
                    FindObjectOfType<CameraMovement>().jmp(false);
                    jump_prtcl.Play();
                }

                // DOUBLE JUMP
                if(jumpCnt == 1)
                {
                    int randFlip =  UnityEngine.Random.Range(1, 3);
                    _anim.SetInteger("dblJmp", randFlip);
                    _anim.SetBool("Flying", false);

                    StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.70f ) );

                    StartCoroutine(Dly_bool_anm(0.65f, "DoubleJump"));
                    FindObjectOfType<CameraMovement>().jmp(true);
                    doubleJump_prtcl.Play();
                }

                // ForceMode.VelocityChange 
                plyr_rb.AddForce( new Vector3(0, (jumpCnt == 1 ? (jumpAmount * 1.40f) : jumpAmount), 0), ForceMode.VelocityChange);


                jumpCnt--;        
            }
        }



        // TYRO MVMNT
        if(plyr_tyro)
        {
            last_tyro_trvld += (tyro_speed) * Time.fixedDeltaTime;

            Quaternion e = actual_path.path.GetRotationAtDistance(last_tyro_trvld);

            tyro_handler_child.position = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, 0.06f, 0);
            tyro_handler_child.rotation = new Quaternion(tyro_handler_child.rotation.x, e.y, tyro_handler_child.rotation.z, tyro_handler_child.rotation.w);

            plyr_trsnfm.position = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, -2.1f, -0.1f);
            plyr_trsnfm.rotation = new Quaternion(plyr_trsnfm.rotation.x, e.y, plyr_trsnfm.rotation.z, plyr_trsnfm.rotation.w);

            
            float d_end = Vector3.Distance(plyr_trsnfm.position, end_tyroPos);
            if(d_end < 6f)
            {
                plyr_rb.AddForce( new Vector3(0, 20, 10), ForceMode.VelocityChange);
                _anim.SetBool("tyro", false);
                plyr_tyro = false;

                // turn back on all movements
                movement_auth = true;

                // turn back gravity
                plyr_rb.useGravity = true;

                FindObjectOfType<CameraMovement>().tyro_offset(true);
            }

        }


    }



    private void setAimSettings(bool isAutoAim, Transform target = null)
    {
        // player_rig.weight = is_active ? 1.0f : 0.0f;
        if(!isAutoAim) lastAimed_enemy = null;
        else lastAimed_enemy = target;
        for(int i = 0; i < player_aims.Length; i ++)
        {
            player_aims[i].weight = isAutoAim ? autoAim_aimsWeigths[i] : noAim_aimsWeigths[i];

            // switch between head & neck
            if(i == 1) player_aims[i].data.constrainedObject = isAutoAim ?  headAndNeck[0] : headAndNeck[1];
        }
    }


    private void FixedUpdate()
    {
        // Nothing to aim settings 
        if (aimed_enemy == null && ammo > 0)
        {
            if(lastAimed_enemy != null) setAimSettings(false);

            // running & wallRunning aim settings [Running with gun]
            if( (rig_animController.enabled == false) &&
                (_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "wallRun" || _anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "gunRun") 
            ){
                rig_animController.enabled = true;
                // setAimSettings(false);
            }

            // all others aim settings  
            if( (rig_animController.enabled == true) &&
                (_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "wallRun" && _anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "gunRun") 
            ){
                rig_animController.enabled = false;
            }
        }
 
        // Auto aim settings
        bool canShot = false;
        if (aimed_enemy != null && ammo > 0)
        {
            // force aimed enemy to be recognized once (so it doesn't get called infintely)
            if(aimed_enemy != lastAimed_enemy)
            {
                // can only auto-aim while [flying, running, wallrunnning, sliding, grappling]
                for(int s = 0; s < authorizedShooting_.Length; s ++)
                {
                    if( authorizedShooting_[s].Contains(_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name) )
                    {
                        rig_animController.enabled = true;
                        setAimSettings(true, aimed_enemy);
                        canShot = true;
                        break;
                    }
                }
                // turnOff
                if(!canShot)
                {
                    rig_animController.enabled = false;
                }
            }
        }





        if( (ammo > 0) && !_anim.GetBool("gunEquipped") ) _anim.SetBool("gunEquipped", true); 
        else if( (ammo == 0) && _anim.GetBool("gunEquipped") )  _anim.SetBool("gunEquipped", false); 
        

        if (!gameOver_ && !plyr_tyro)
        {

            if(!plyr_tyro && !plyr_intro_tyro && !plyr_wallRninng)
            {
                if (!Input.GetKey("q") && !Input.GetKey("d"))  rotate_bck();
                if(Input.GetKey("q") || Input.GetKey("d"))  rt_auth = false; 

                // rotate back [Quaternion Slerp]
                if(rt_auth)
                {
                    if(plyr_trsnfm.rotation.eulerAngles.y >= 0.1f || plyr_trsnfm.rotation.eulerAngles.y <= -0.1f)
                    {
                        transform.localRotation = Quaternion.Slerp(plyr_trsnfm.rotation, new Quaternion(0,0,0,1), 3.0f * Time.deltaTime);
                    }
                }
            }

            

            // movement auth disabled for [ObstacleHit, Swinging, Tyro, GrappleJump]
            if (movement_auth)
            {
                // MAIN MOVEMENT SPEED //
                    
                // SLIDING SPEED
                if(plyr_sliding) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 15);
                
                // WALL RUN 
                else if(plyr_wallRninng) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 3.5f, 12);
                
                // RUNNING SPEED
                else
                {

                    // DEFAULT SPEED
                    if (!Input.GetKey("q") && !Input.GetKey("d")) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 2f);
                    
                    // STRAFE SPEED
                    if (Input.GetKey("q") || Input.GetKey("d")) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 3);
                       
                    if(aimed_enemy != null ) plyr_rb.velocity = new Vector3(0, 0, 0.4f);
                }


                // STRAFE FORCES
                if (Input.GetKey("q"))
                {
                    // LEFT ROTATION
                    if(!plyr_wallRninng)
                        if ( (plyr_trsnfm.rotation.eulerAngles.y >= 315.0f && plyr_trsnfm.rotation.eulerAngles.y <= 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y <= 47.0f) )
                        {
                            plyr_.transform.Rotate(0, -2.50f, 0, Space.Self);
                        } 
                    
                    
                    // LEFT STRAFE
                    if(plyr_rb.velocity.x > -11)  plyr_rb.AddForce((-4 * (Vector3.right * strafe_speed) ), ForceMode.VelocityChange);
                    else  plyr_rb.velocity = new Vector3(-11, plyr_rb.velocity.y, plyr_rb.velocity.z);
                }

                if (Input.GetKey("d"))
                { 
                    
                    // RIGHT ROTATION
                    if(!plyr_wallRninng)
                        if ( (plyr_trsnfm.rotation.eulerAngles.y >= 311.0f) || ( Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) >= 0.0f && Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) <= 44.0f) )
                        {
                            plyr_.transform.Rotate(0, 2.50f, 0, Space.Self);
                        }
                    

                    // RIGHT STRAFE
                    if(plyr_rb.velocity.x < 11)  plyr_rb.AddForce((-4 * (Vector3.left * strafe_speed) ), ForceMode.VelocityChange);
                    else  plyr_rb.velocity = new Vector3(11, plyr_rb.velocity.y, plyr_rb.velocity.z);
                }
            }






            // Swinging Forces 
            if(!movement_auth && plyr_swnging)
            {
                plyr_trsnfm.rotation = Quaternion.Euler(plyr_trsnfm.rotation.eulerAngles.x, plyr_trsnfm.rotation.eulerAngles.y, grpl_sns ? -10 : 10);

                // swing strafe
                if (Input.GetKey("q"))  plyr_rb.AddForce((2.6f * (Vector3.left * strafe_speed)), ForceMode.VelocityChange); pico_character.transform.Rotate(0.4f, 0, 0, Space.Self);
                if (Input.GetKey("d"))  plyr_rb.AddForce((2.6f * (Vector3.right * strafe_speed)), ForceMode.VelocityChange); pico_character.transform.Rotate(-0.4f, 0, 0, Space.Self);

                if (Input.GetKey("d") || Input.GetKey("q"))  plyr_rb.AddForce( new Vector3(0f, 0.32f, 0f), ForceMode.VelocityChange);

                if (grap_pnt != new Vector3(0,0,0))
                {
                    if(plyr_trsnfm.position.z < grap_pnt.z - 3.5f)
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 17);
                        plyr_rb.AddForce( new Vector3(0, -0.20f, 0), ForceMode.VelocityChange);
                    }
                    else
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 23);
                        plyr_rb.AddForce( new Vector3(0, -0.20f,  0), ForceMode.VelocityChange);
                    }
                }
            }

        }


  
    }


    // Public fnc for cllision 
    public void animateCollision(string cls_type, Vector3 cls_size,  GameObject optional_gm = null)
    {   

        float jumpForce = Mathf.Sqrt(4.5f * -2 * (Physics.gravity.y));
        List<string> interact_Jmps = new List<string>(new string[3] {"tapTapJump", "bumperJump", "launcherHit"} );

        if(interact_Jmps.Contains(cls_type)) {
            FindObjectOfType<CameraMovement>().special_jmp();
            StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.0f));
        };
        
        switch(cls_type)
        {
            case "groundLeave":
                _anim.SetBool("Flying", true);
                _anim.SetBool("GroundHit", false);
                plyr_flying = true;

                fix_Cldrs_pos(0.12f, true);
                break;

            case "groundHit":
                _anim.SetBool("Flying", false);
                _anim.SetBool("GroundHit", true);

                jumpCnt = 2;
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.6f));

                if (plyr_flying)
                {           
                    StartCoroutine(Dly_bool_anm(0.3f, "GroundHit"));
                    fix_Cldrs_pos(-0.12f, false);

                    plyr_flying = false;
                }
                break; 

            case "obstacleHit":
                rotate_bck();

                if(!plyr_obstclJmping && optional_gm.GetComponent<Rigidbody>() == null)
                {
                    last_ObstJumped = optional_gm;

                    StartCoroutine(Dly_bool_anm(plyr_flying ? 1.75f : 1.25f, "obstacleJump"));
                    plyr_rb.AddForce( new Vector3(0, 2, 0), ForceMode.VelocityChange);
                    StartCoroutine(obstcl_anim(cls_size, optional_gm));
                }else
                {
                    if(last_ObstJumped != optional_gm) kickObst(optional_gm);
                }
                break;

            case "obstacleLeave":
                // TODO : ADD RIGIDBODT TO OBJ AND THROW IT AWAY
                break;

            case "wallRunHit":
                if(!gameOver_ && !_anim.GetBool("GroundHit"))
                {
                    _anim.SetBool("Flying", false);
                    _anim.SetBool("wallRun", true);
                    plyr_wallRninng = true;

                    GameObject hitWall = optional_gm;
                    float sns = (hitWall.transform.position.x - gameObject.transform.position.x);

                    Vector3 p_ = new Vector3( sns < 0 ? -0.30f : 0.4f, 0, 0);
                    Quaternion q_ = Quaternion.Euler(0,  sns <  0 ? -30f : -41f, sns <  0 ? -47f : 47f);

                    pico_character.transform.localRotation = q_;
                    pico_character.transform.localPosition = p_;

                }
                break;

            case "wallRunExit":
                if(!gameOver_)
                {
                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.5f));
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 2, plyr_rb.velocity.y, plyr_rb.velocity.z);
                    plyr_rb.AddForce( new Vector3(0, 17f, 0), ForceMode.VelocityChange);
                    _anim.SetBool("Flying", true);
                    _anim.SetBool("wallRun", false);
                    plyr_wallRninng = false;

                    pico_character.transform.rotation = new Quaternion(0, 0, 0, 0);
                    pico_character.transform.localPosition = new Vector3(0, 0, 0);

                }
                break;

            case "frontWallHit":
                if(!plyr_wallRninng)
                {
                    gameOver_ = true;
                    _anim.SetBool("frontWallHit", true);
                    FindObjectOfType<CameraMovement>().cam_GamerOver_();
                    // Collider[] colList = gameObject.transform.GetComponentsInChildren<Collider>();
                    // for (int i = 0; i < colList.Length; i ++){
                        
                    //     if (i != 0){
                    //         colList[i].enabled = false;
                    //     }else{
                    //         colList[i].center = new Vector3(colList[i].center.x, colList[i].center.y + 0.5f, colList[i].center.z);
                    //     }
                    // }
                }
                break;

            case "sliderHit":
                plyr_sliding = true;
                _anim.SetBool("slide", true);
                rotate_bck();
         
                slide_prtcl.Play();

                jumpCnt = 2;
                
                plyr_rb.AddForce( new Vector3(0, jumpForce * 0.15f, 5), ForceMode.VelocityChange);
                fix_Cldrs_pos( 0.42f, true);
                break;

            case "sliderLeave":
                plyr_sliding = false;
                _anim.SetBool("slide", false);
                
                plyr_rb.AddForce( new Vector3(0, jumpForce * 1.2f, -6), ForceMode.VelocityChange);
                fix_Cldrs_pos(-0.42f, false);    

                slide_prtcl.Stop();

                break;

            // case string jmp when interact_Jmps.Contains(jmp):
            //     StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.0f));
            //     return;

            case "launcherHit":
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.10f, 16), ForceMode.VelocityChange);
                StartCoroutine(Dly_bool_anm(0.90f, "launcherJump"));
                break;

            case "bumperJump":
                rotate_bck();
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.4f, 14), ForceMode.VelocityChange);
                StartCoroutine(Dly_bool_anm(0.60f, "bumperJump"));
                break;

            case "tapTapJump":
                _anim.SetBool("Flying", true); 
                rotate_bck();
                StartCoroutine(Dly_bool_anm(0.85f, "tapTapJump"));
                if(optional_gm)
                {
                    LeanTween.moveY(gameObject,plyr_trsnfm.position.y + (optional_gm.GetComponent<Collider>().bounds.size.y  + 0.1f), 0.320f).setEaseInSine();    
                    plyr_rb.AddForce( new Vector3(0, 0, 8), ForceMode.VelocityChange);  
                }
                break;

            case "gun":
                if(player_hand_gunSlot.activeSelf == false) player_hand_gunSlot.SetActive(true);
                LeanTween.scale(player_hand_gunSlot, player_hand_gunSlot.transform.localScale * 1.3f, 0.7f).setEasePunch();    

                ammo = 12; 

                if(aimed_enemy == null)
                {
                    head_TARGET.parent = transform; body_TARGET.parent = transform;
                    body_TARGET.localPosition = new Vector3(2.99f,-5.13f,-0.40f);
                    head_TARGET.localPosition = new Vector3(6.80f,-10.57f,16.06f);

               
                }else
                {
                    Vector3 adjustedAimG  = (
                        aimed_enemy.transform.rotation.eulerAngles.z > 45f ?
                        new Vector3(-4.5f, 0, 0) : new Vector3(0, -4.5f, 0)
                    );

                    head_TARGET.parent = aimed_enemy.transform; body_TARGET.parent = aimed_enemy.transform;
                    head_TARGET.localPosition = adjustedAimG;
                    body_TARGET.localPosition = adjustedAimG;
                }
                break;

            case "newEnemyAim":
                Vector3 adjustedAim  = (
                    optional_gm.transform.rotation.eulerAngles.z > 45f ?
                    new Vector3(-4.5f, 0, 0) : new Vector3(0, -4.5f, 0)
                );

                head_TARGET.parent = optional_gm.transform; body_TARGET.parent = optional_gm.transform;
                head_TARGET.localPosition = adjustedAim; 
                body_TARGET.localPosition = adjustedAim;

                aimed_enemy = optional_gm.transform;
                break;

            case "emptyEnemyAim":                
                head_TARGET.parent = transform; body_TARGET.parent = transform;
                body_TARGET.localPosition = new Vector3(2.99f,-5.13f,-0.40f);
                head_TARGET.localPosition = new Vector3(6.80f,-10.57f,16.06f);
              
                aimed_enemy = null;
                break;
            default:
                break;
        }
    }



    // TYRO
    public void tyro_movement(GameObject path_obj)
    {
        if(plyr_tyro) return;

        // turn off all movements
        movement_auth = false;
        // turn off gravity
        plyr_rb.useGravity = false;
        plyr_rb.velocity = new Vector3(0, 0, 0);

        last_tyro_trvld = 1.25f;   

  
        Transform prnt_ = path_obj.transform.parent;
        PathCreator[] paths_ = prnt_.GetComponentsInChildren<PathCreator>();
        actual_path = paths_[0];
        end_tyroPos = actual_path.path.GetPoint(actual_path.path.NumPoints - 1);

        
        // TYRO HANDLER Find
        foreach(Transform child_trsf in prnt_)
        {
            if(child_trsf.gameObject.tag == "tyro_handler")
            {

                // Assign transform
                tyro_handler_child = child_trsf.gameObject.GetComponent<Transform>();

                // Disable Collectible.cs
                Collectible c_;      
                c_ = tyro_handler_child.gameObject.GetComponent<Collectible>();
                c_.isAnimated = false;

                // 2.6 scale for tyro handler
                tyro_handler_child.localScale = new Vector3(2.6f, 2.6f, 2.6f);
                break;
            }
        };

        _anim.SetBool("tyro", true);

        plyr_intro_tyro = true;

        LeanTween.scale(tyro_handler_child.gameObject, tyro_handler_child.localScale * 2f, 1.5f).setEasePunch();

        Vector3 moveTo_ = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, -2.1f, -0.1f);
        LeanTween.move(gameObject, moveTo_, 0.6f).setEaseInSine(); 
        LeanTween.move(tyro_handler_child.gameObject, actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, 0.05f, 0), 0.6f).setEaseInSine(); 

        LeanTween.rotate(tyro_handler_child.gameObject, new Vector3(0, actual_path.path.GetRotationAtDistance(1f).eulerAngles.y / 2f, 0), 0.6f).setEaseInSine();
        LeanTween.rotate(gameObject, new Vector3(0, actual_path.path.GetRotationAtDistance(1f).eulerAngles.y / 4.30f, 0), 0.65f).setEaseInSine();
        
        Invoke("activateTyro", 1.0f);
        FindObjectOfType<CameraMovement>().tyro_offset(false);
        //plyr_tyro = true;
        
    }
    private void activateTyro(){plyr_tyro = true; plyr_intro_tyro = false;}



    // THROW AN ANIMATION BOOL 
    private IEnumerator Dly_bool_anm(float delay, string anim_bool)
    {
        _anim.SetBool(anim_bool, true);

        //yield on a new YieldInstruction that waits for "delay" seconds.
        yield return new WaitForSeconds(delay);
                
        _anim.SetBool(anim_bool, false);
    }



    
    private void fix_Cldrs_pos(float y_off_pos, bool default_)
    {
        Collider[] colList = gameObject.transform.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colList.Length; i ++)
        {
            if(y_off_pos < 0 && colList[i].isTrigger)
            {
                StartCoroutine(disbl_cldr(colList[i], 0.1f));
            }

            string[] coldr_type = colList[i].GetType().ToString().Split('.');
            BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr; CapsuleCollider c_cldr;

            switch(coldr_type[1])
            {
                case "BoxCollider" :
                    b_cldr = (colList[i] as BoxCollider);
                    b_cldr.center = !default_ ? colliders_references[i] : new Vector3(b_cldr.center.x, b_cldr.center.y + y_off_pos, b_cldr.center.z);
                    break;
                case "SphereCollider" : 
                    s_cldr = (colList[i] as SphereCollider);
                    s_cldr.center = !default_ ? colliders_references[i] : new Vector3(s_cldr.center.x, s_cldr.center.y + y_off_pos, s_cldr.center.z);
                    break;
                // case "CapsuleCollider" : 
                //     c_cldr = (colList[i] as CapsuleCollider);
                //     c_cldr.center = !default_ ? colliders_references[i] : new Vector3(c_cldr.center.x, c_cldr.center.y + y_off_pos, c_cldr.center.z);
                //     break;
                case "MeshCollider" : 
                    m_cldr = (colList[i] as MeshCollider);
                    break;
            }
        }
    }

    private IEnumerator disbl_cldr(Collider cld, float t_)
    {
        cld.enabled = false;
        yield return new WaitForSeconds(t_);
        cld.enabled = true;
    }




    // OBSTACLE HIT ANIMATION
    private IEnumerator obstcl_anim(Vector3 cls_size, GameObject obstacl_gm)
    {
        float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
        plyr_obstclJmping = true;
        // disable all movements
        movement_auth = false;

        // reset X and Y velocity
        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 20, 0, 0);
        plyr_rb.useGravity = false;

        Vector3 randTorque = new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-30, 30f), UnityEngine.Random.Range(15f, -15f));

        if (plyr_flying)
        {
            // 1 : vertical fix
            // if land behind obstacle
            if(plyr_trsnfm.position.y < (obstacl_gm.transform.position.y + (cls_size.y / 2f) ) + 0.05f)
            {
                LeanTween.moveY(gameObject,  (obstacl_gm.transform.position.y + (cls_size.y / 2f) + 0.2f), 0.10f).setEaseInSine(); 
                yield return new WaitForSeconds(0.10f);     
            }

            // 2 : slide
            LeanTween.moveZ(gameObject, obstacl_gm.transform.position.z + (cls_size.z / 2f), 0.95f ).setEaseInSine(); 
            yield return new WaitForSeconds(0.95f);   

            // 3 : jump
            plyr_rb.useGravity = true;
            plyr_rb.AddForce( new Vector3(0, jumpForce * 0.70f, 0), ForceMode.VelocityChange);

            yield return new WaitForSeconds(0.30f); 

            // make obstacle fly 
            Rigidbody obst_rb = obstacl_gm.AddComponent<Rigidbody>();
            obst_rb.mass = 0.01f;
            obst_rb.AddForce(new Vector3(0, 3, -6), ForceMode.VelocityChange);
            obst_rb.AddTorque(randTorque, ForceMode.VelocityChange);
        }
        else
        {
            
            // 1 : vertical fix
            plyr_rb.AddForce( new Vector3(0, 3, -1), ForceMode.VelocityChange);             
            LeanTween.moveY(gameObject,plyr_trsnfm.position.y + (cls_size.y  + 0.1f), 0.320f).setEaseInSine();      
            yield return new WaitForSeconds(0.325f); 


            // 2 : HORIZONTAL SLIDE of Precise [ 7 * cls_size.z ] jump dist
            plyr_rb.AddForce( new Vector3(0, -1  * (jumpForce * 0.15f), 7f * cls_size.z), ForceMode.VelocityChange);

            // make obstacle fly 
            Rigidbody obst_rb = obstacl_gm.AddComponent<Rigidbody>();
            obst_rb.mass = 0.01f;
            obst_rb.AddForce(new Vector3(0, 6, -6), ForceMode.VelocityChange);
            obst_rb.AddTorque(randTorque, ForceMode.VelocityChange);

            yield return new WaitForSeconds(0.45f); 
                // 3 : Reset velocity & JUMP++
                plyr_rb.useGravity = true;
                plyr_rb.AddForce( new Vector3(0, jumpForce * 1.30f, 0), ForceMode.VelocityChange);
                FindObjectOfType<CameraMovement>().obs_offset();


            yield return new WaitForSeconds(0.15f); 
                // FORWARD FORCE ++
                plyr_rb.AddForce( new Vector3(0, 0, 9), ForceMode.VelocityChange);
        }



        yield return new WaitForSeconds(0.25f); 
            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0, 0);
            _anim.SetBool("Flying", true);
            plyr_flying = true;


        plyr_obstclJmping = false;
        movement_auth = true;
    }

    // kick obj when alr obst jumping
    private void kickObst(GameObject obst)
    {
        Rigidbody obst_rb = obst.GetComponent<Rigidbody>() == null ? obst.AddComponent<Rigidbody>() : obst.GetComponent<Rigidbody>();
        obst_rb.mass = 0.01f;
        Vector3 randTorque = new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-30, 30f), UnityEngine.Random.Range(15f, -15f));
        Vector3 kickForce = new Vector3(
            plyr_trsnfm.position.x - obst.transform.position.x,
            plyr_trsnfm.position.y - obst.transform.position.y,
            plyr_trsnfm.position.z - obst.transform.position.z
        );
        obst_rb.AddForce(new Vector3(kickForce.x * 2, 10, 16 ), ForceMode.VelocityChange);
        obst_rb.AddTorque(randTorque, ForceMode.VelocityChange);
    }




    private IEnumerator delay_jumpInput(float t_)
    {
        jump_auth = false; yield return new WaitForSeconds(t_); jump_auth = true;
    }
    
    public void rotate_bck()
    {
        if(plyr_tyro || plyr_intro_tyro)
        {
            return;
        }
        // trn_back_Lean_id = LeanTween.rotate(gameObject, new Vector3(plyr_trsnfm.rotation.eulerAngles.x , 0.0f, plyr_trsnfm.rotation.eulerAngles.z), 1f).setEaseInOutCubic().id;
        // LeanTween.rotate(gameObject, new Vector3(plyr_trsnfm.eulerAngles.x, 0, plyr_trsnfm.eulerAngles.z), 0.8f);
        // rt_done = true;
        rt_auth = true;
    }


    // Swing Animation
    public void swing_anm(bool is_ext, Vector3 grapl_pnt)
    {
  

        _anim.SetBool("flying", true);
        if(is_ext)
        {
            fix_Cldrs_pos(0.0f, false);

            movement_auth = true;

            grap_pnt = new Vector3(0,0,0);
            _anim.SetBool("swing", false);
            StartCoroutine(Dly_bool_anm(0.65f, "exitSwing"));
            plyr_swnging = false;
            return;
        }
        else
        {
            float sns =  plyr_trsnfm.position.x - grap_pnt.x;
            grpl_sns = sns < 0 ? false : true;

            fix_Cldrs_pos(-0.52f, true);
            grap_pnt = grapl_pnt;
            plyr_rb.AddForce( new Vector3(0, 4f, 0f), ForceMode.VelocityChange);
            
            movement_auth = false;

            _anim.SetBool("swing", true);
            plyr_swnging = true; 
            return;
        }
    }

}
