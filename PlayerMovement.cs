using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using System;
using PathCreation.Utility;
using PathCreation;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    [Header ("Transforms & Rb")]
    [SerializeField] private GameObject plyr_;
    [SerializeField] private Rigidbody plyr_rb;
    [SerializeField] private Transform plyr_trsnfm;
    [SerializeField] private Transform plyr_cam;


    [Header ("Combo System")]
    private float combo_timer = 0f;
    private int combo_ = 0;
    [HideInInspector] public int get_Combo{
        get{ return combo_;}
        set{ return; }
    }
    private string last_comboId_;

    [Header ("Momentums/Action/Ult")]
    [SerializeField] private float action_momentum = 0f;
    [SerializeField] private float momentum_ = 0f;
    [SerializeField] private float ultimate_ = 0f;
    [HideInInspector] public float get_Momentum {
        get{ return momentum_; }
        set{ return; }
    }

    
    [Header ("Movement Values")]
    private const float jumpAmount = 28f;
    private const float strafe_speed = 0.225f;
    private const float player_speed = 7f;
    private const float tyro_speed = 15f;
    private float railSlide_speed = 4f;



    [Header ("Character Animators")]
    [SerializeField] private GameObject pico_character;
    private Animator _anim;
    private CharacterController _controller = null;

    [Header ("Animation Center Fixes")]
    private Vector3[] colliders_references = new Vector3[10];

    [Header ("Stored Last Obstacle")]
    private GameObject last_ObstJumped;

    [Header ("Player Movements Status")]
    private bool plyr_flying = false;
    [HideInInspector] public bool plyr_sliding = false;
    private bool plyr_wallRninng = false, plyr_obstclJmping = false, plyr_tapTapJumping = false, plyr_swnging = false;
    [HideInInspector] public bool plyr_tyro = false;
    private bool plyr_intro_tyro= false;
    private bool plyr_shooting = false;
    private bool plyr_jumping = false;
    private bool plyr_animKilling = false;
    private bool plyr_wallExiting = false;
    [HideInInspector] private bool plyr_railSliding = false;
    [HideInInspector] public bool plyr_hanging = false;
    private bool plyr_rampSliding = false, plyr_boxFalling = false, plyr_climbing = false;
   [HideInInspector] public bool plyr_climbingLadder = false;
    private bool plyr_sideHanging = false;
    private bool plyr_bumping = false;

    [Header ("Authorized Movements")]
    private bool jump_auth = true;
    private bool movement_auth = true;

    [Header ("Player Available Jumps")]
    private int jumpCnt = 2;


    [Header ("Rotate booleans")]
    private bool rt_auth = false;


    [Header ("Tyro/SlideRail Vars")]
    private float last_tyro_trvld = 0.05f;
    private Transform tyro_handler_child;
    private PathCreator actual_path;
    private Vector3 end_tyroPos;
    private GameObject slideRail_ref;

    [Header ("Grapple Vars")]
    private Vector3 grap_pnt = new Vector3(0,0,0);
    private bool grpl_sns = false;

    [Header ("Hang Vars")]
    private GameObject hang_obj;
    private GameObject moving_hangTraveler; // hang
    private GameObject moving_camTraveler; // ladder

    [Header ("SideHang Var")]
    private bool sHng_leftSide = false;

    [Space(10)] 

    [Header ("Game State")]
    private bool gameOver_ = false;

    [Header ("WallRun Forced Strafes")]
    private bool lft_Straf = false;
    private bool rght_Straf = false;

    [Space(10)] 

    [Header ("Player Gun")]
    [HideInInspector] public int ammo = 0;
    private Weapon player_weaponScrpt;

    [Space(10)]

    [Header ("Player Left-Arm Rig")]
    [SerializeField] private MultiAimConstraint[] arm_aims;
    [SerializeField] private Transform[] arm_transforms;
    private Vector3[] adjustFly_vectors = new Vector3[4] {
        new Vector3(0,-40f,-10f), new Vector3(120f,0,-25f), new Vector3(0,0,0), new Vector3(-75f,0,0) 
    };
    private Vector3[] adjustSlide_vectors = new Vector3[4]{
        new Vector3(0f, 0f, 0f), new Vector3(180f, 0f, 0f), new Vector3(0f, 0f, -0f), new Vector3(0, 0f, 0f) 
    };


    [Header ("Player Rigs & Constraints")]
    [SerializeField] private RigBuilder player_Rig;
    [SerializeField] private MultiAimConstraint[] player_aims;
    [SerializeField] private Animator rig_animController;
    private float[] noAim_aimsWeigths = new float[4]{0.80f, 1.0f, 0.0f, 0.0f};
    private float[] autoAim_aimsWeigths = new float[4]{1.0f,  0.90f, 0f, 0.0f};
    [SerializeField] private Transform[] headAndNeck;
    private bool lastAimSettings_isArm;
    private string lastArmSetting_type;
    private bool wasLastAim_AutoAim = false;

    [Header ("Authorized Shooting Animations")]
    private string[] authorizedShooting_ = new string[4] { "pistolRun", "flying", "slide", "wallRun" };
    private const string authorizedShooting_s = "pistolRun flying slide wallRun railSlide hang slideDown";
    private bool special_rampDelay = true;
    
    [Header ("Player Targets")]
    [SerializeField] private Transform body_TARGET; 
    [SerializeField] private Transform head_TARGET; 
    [SerializeField] private Transform leftArm_TARGET; 


    [Header ("Enemy Data")]
    [HideInInspector] public Transform aimed_enemy;
    private bool horizontal_enemy = false;
    private Transform lastAimed_enemy;
    private Vector3 saved_WorldPos_armTarget;

    [Header ("Aim Setting Called")]
    private bool aimSettingCalled = false;


    [Header ("Aim Recoil")]
    private float aim_Recoil = 0.0f;
    public float set_recoil  
    {
        get { return 0f; } 
        set { if(value.GetType() == typeof(float)) aim_Recoil = value; }  // set method
    }
    private bool FLYYY = false;


    [Header ("Camera Movement")]
    private CameraMovement cm_movement;
    [Header ("Main PlayerCollisions")]
    private PlayerCollisions psCollisions_movement;



    [Header ("Touch Variables")]
    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered [5% width of the screen]


    [Header ("GameUI")]
    private GameUI g_ui;


    [Header ("Camera")]
    private GameObject game_cam;

    // Start method
    private void Start()
    {
        game_cam = GameObject.FindGameObjectsWithTag("MainCamera")[0];

        player_weaponScrpt = FindObjectOfType<Weapon>();
        g_ui = FindObjectOfType<GameUI>();
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
                        s_cldr = colList[i].GetComponent<SphereCollider>();
                        colliders_references[i] = s_cldr.center;
                        break;
                    case "MeshCollider" : break;
                }
                
            }

        rig_animController.enabled = false;

        cm_movement = FindObjectOfType<CameraMovement>();

        PlayerCollisions ar =  GameObject.FindGameObjectsWithTag("mainHitbox")[0].GetComponent<PlayerCollisions>();
        psCollisions_movement = ar;

        dragDistance = Screen.width * 4 / 100; //dragDistance is 5% width of the screen
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
                    cm_movement.jmp(false);
                    psCollisions_movement.player_paricleArray(null, true, "jump") ;
                }

                // DOUBLE JUMP
                if(jumpCnt == 1)
                {
                    int randFlip =  UnityEngine.Random.Range(1, 3);
                    _anim.SetInteger("dblJmp", randFlip);
                    _anim.SetBool("Flying", false);

                    StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.70f ) );

                    StartCoroutine(Dly_bool_anm(0.65f, "DoubleJump"));
                    cm_movement.jmp(true);
                    psCollisions_movement.player_paricleArray(null, true, "dblJump"); 
                }

                // ForceMode.VelocityChange 
                plyr_rb.AddForce( new Vector3(0, (jumpCnt == 1 ? (jumpAmount * 1.30f) : 
                    (plyr_flying ? jumpAmount : jumpAmount/1.5f )),
                     0),
                ForceMode.VelocityChange);


                jumpCnt--;        
            }
        }



        // Tyro/SlideRail
        if(plyr_tyro || plyr_railSliding)
        {
            last_tyro_trvld += (plyr_tyro ? tyro_speed : railSlide_speed) * Time.deltaTime;

            Quaternion e = actual_path.path.GetRotationAtDistance(last_tyro_trvld + (plyr_railSliding ? 0f : 0.05f) );

            if(tyro_handler_child != null)
            {
                tyro_handler_child.position = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, 0.06f, 0);
                tyro_handler_child.rotation = new Quaternion(tyro_handler_child.rotation.x, e.y, tyro_handler_child.rotation.z, tyro_handler_child.rotation.w);
            }
 
            plyr_trsnfm.position = (plyr_tyro ? 
                actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, -2.1f, -0.1f)
                    :
                actual_path.path.GetPointAtDistance(last_tyro_trvld)+ new Vector3(0, 0.6f, -0.1f)
            );
            plyr_trsnfm.rotation = new Quaternion(plyr_trsnfm.rotation.x, e.y, plyr_trsnfm.rotation.z, plyr_trsnfm.rotation.w);

            
            // stop
            float d_end = Vector3.Distance(plyr_trsnfm.position, end_tyroPos);
            if(d_end < 3f)
            {
                
                plyr_rb.AddForce( new Vector3(0, 20, 10), ForceMode.VelocityChange);
                _anim.SetBool(plyr_tyro ? "tyro" : "slideRail", false);

                if(plyr_tyro) plyr_tyro = false;
                if(plyr_railSliding)
                {
                    plyr_railSliding = false;
                    cm_movement.railSlide_offset(true);
                }

                // turn back on all movements
                movement_auth = true;

                // turn back gravity
                plyr_rb.useGravity = true;

                cm_movement.tyro_offset(true);

                g_ui.set_countBonus_ = false;
            }

            if(plyr_railSliding)
            { if (pico_character.transform.localRotation.eulerAngles.y != 85f)
                {
                    pico_character.transform.localRotation = Quaternion.Euler(0f, 85f, 0f);
                }
            }
        }
        

        // momentum
        if(momentum_ > 0) momentum_ -= 0.5f * Time.deltaTime;
        else momentum_ = 0;

        // action momentum
        if(action_momentum > 2f && action_momentum > 0) action_momentum -= 10f * Time.deltaTime;
        else if(action_momentum < 2f && action_momentum < 0) action_momentum += 10f * Time.deltaTime;
        else (action_momentum) = 0f;
    }   


    
    private enum Axis{
            X,
            X_NEG,
            Y,
            Y_NEG,
            Z,
            Z_NEG
    };

    static Vector3 Convert(Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return Vector3.right;
            case Axis.X_NEG:
                return Vector3.left;
            case Axis.Y:
                return Vector3.up;
            case Axis.Y_NEG:
                return Vector3.down;
            case Axis.Z:
                return Vector3.forward;
            case Axis.Z_NEG:
                return Vector3.back;
            default:
                return Vector3.up;
        }
    }

    private UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis[] all_axisS = new UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis[6]
    {
        UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis.X,
        UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis.X_NEG, 
        UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis.Y,
        UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis.Y_NEG,
        UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis.Z,
        UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis.Z_NEG  
    };

    private IEnumerator CheckAllArmRigAxies()
    {
        Vector3[] all_axis = new Vector3[6]{Vector3.left, Vector3.right, Vector3.down, Vector3.up, Vector3.back, Vector3.forward};

        while(true)
        {
    
            int rdm_ = UnityEngine.Random.Range(0, all_axis.Length - 1);
            int rdm_2 = UnityEngine.Random.Range(0, all_axis.Length - 1);

            for(int m = 0; m < arm_aims.Length; m ++)
            {
                UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis m_AimAxis;
                UnityEngine.Animations.Rigging.MultiAimConstraintData.Axis m_UpAxis;
          

                // UnityEngine.Animations.Rigging.IMultiAimConstraintData upAxis = all_axisS[rdm_];
                m_AimAxis = all_axisS[rdm_];
                m_UpAxis = all_axisS[rdm_2];

                arm_aims[m].data.aimAxis = m_AimAxis;
                arm_aims[m].data.upAxis = m_UpAxis;
            //    arm_aims[m].data.constrainedObject = arm_transforms[m];
            //    arm_aims[m].weight = 1.0f;
            }
            player_Rig.Build();
            yield return new WaitForSeconds(3f);
        }
    }


    private void setAimSettings(bool isAutoAim, Transform target = null, bool toogleArmRig = false, string armRig_ClipInfo = "")
    {
        if(aimSettingCalled) return;

        aimSettingCalled = true;
        wasLastAim_AutoAim = isAutoAim;

        if(!isAutoAim)
        {
            // clear target on CamerMovement.cs
            cm_movement.rst_aimedTarget();

            lastAimed_enemy = null; 

            // if(arm_aims[0].data.constrainedObject != null)
            // {
                for(int m = 0; m < arm_aims.Length; m ++)
                {
                   arm_aims[m].data.offset = new Vector3(0, 0, 0);
                   arm_aims[m].data.maintainOffset = false;

                   arm_aims[m].data.constrainedObject = arm_transforms[m];
                   arm_aims[m].weight = 0f;

                   // arm_aims[m].data.aimAxis = all_axisS[0];
                   // arm_aims[m].data.upAxis = all_axisS[0];
                }
               // StopCoroutine(CheckAllArmRigAxies());    
            // }
        }
        else
        {  
            lastAimed_enemy = target; 
            rig_animController.enabled = false;

            // Adjust HAND for autoAim
            arm_aims[3].data.constrainedObject = arm_transforms[3];
            arm_aims[3].data.offset = new Vector3(0, 0, -95f);
            arm_aims[3].weight = 1.0f;
            arm_aims[3].data.aimAxis = all_axisS[2]; // Y
            arm_aims[3].data.upAxis = all_axisS[4]; // Z
        }

     
        for(int i = 0; i < player_aims.Length; i ++)
        {
            // body running autoAim offset
            if(i == 0 && isAutoAim) player_aims[i].data.offset = new Vector3(20, 0, 0);

            // switch between head & neck
            if(i == 1)
            {
                player_aims[i].data.sourceObjects.Clear();
                player_aims[i].data.constrainedObject = isAutoAim ?  headAndNeck[0] : headAndNeck[1]; 
                player_Rig.Build();    
            }

            player_aims[i].weight = 0f;

            player_aims[i].weight = isAutoAim ? autoAim_aimsWeigths[i] : noAim_aimsWeigths[i];
        }
        

        // if(plyr_rampSliding) StartCoroutine(CheckAllArmRigAxies());

        if(toogleArmRig)
        {
            for(int m = 0; m < arm_aims.Length; m ++)
            {
                arm_aims[m].weight = 0f;
                arm_aims[m].data.sourceObjects.Clear();

                arm_aims[m].data.constrainedObject = arm_transforms[m];

                if(armRig_ClipInfo == "flying")
                {
                    arm_aims[m].data.offset = new Vector3(0, 0, 0);

                    arm_aims[m].data.aimAxis = all_axisS[1]; // -X
                    arm_aims[m].data.upAxis = all_axisS[3]; // -Y

                }
                else if (armRig_ClipInfo == "slide"|| armRig_ClipInfo == "railSlide" || armRig_ClipInfo == "slideDown")
                {
                    
                    arm_aims[m].data.offset = adjustSlide_vectors[m];

                    arm_aims[m].data.aimAxis = armRig_ClipInfo == "railSlide" ?  all_axisS[2] : (all_axisS[1]); // Y or -X 
                    arm_aims[m].data.upAxis = armRig_ClipInfo == "slideDown" ?  all_axisS[2] : all_axisS[4]; // Y or Z

                }

                arm_aims[m].weight = 1.0f;
            }

             if(armRig_ClipInfo == "slide" || armRig_ClipInfo == "railSlide" || armRig_ClipInfo == "slideDown") player_aims[0].weight = 0f;
        }

        
        rig_animController.enabled = true;
        aimSettingCalled = false;
        player_Rig.Build();
        //_anim.Rebind();
    }



    private void FixedUpdate()
    {
        ammo = player_weaponScrpt.get_ammo;

        if (!gameOver_ && !plyr_tyro)
        {


            if( (ammo > 0) && !_anim.GetBool("gunEquipped") ) _anim.SetBool("gunEquipped", true); 
            // else if( (ammo == 0) && _anim.GetBool("gunEquipped") )  _anim.SetBool("gunEquipped", false); 


            // Nothing-To-Aim 
            if ( (aimed_enemy == null && ammo > 0) || ammo == 0)
            {
                if(lastAimed_enemy != null) setAimSettings(false);

                // running & wallRunning aim settings [Running with gun]
                if( (rig_animController.enabled == false) &&
                    (_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "gunRun") 
                ){
                    rig_animController.enabled = true;
                }

                // others aim settings  
                if( (rig_animController.enabled == true) &&
                    (_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "gunRun") 
                ){
                    rig_animController.enabled = false;
                }

                // reset pico y previous rotations 
                if(!plyr_wallRninng && !plyr_hanging && !plyr_rampSliding && !plyr_sideHanging && !plyr_bumping)
                {
                    pico_character.transform.localRotation = Quaternion.Euler(
                        pico_character.transform.localRotation.eulerAngles.x, 0f, pico_character.transform.localRotation.eulerAngles.z
                    );
                }
            }
    


            // Auto-aim
            bool canShot = false;
            if (aimed_enemy != null && ammo > 0)
            {
                string animClip_info = _anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                bool armRigMaybe = ((animClip_info == "slide") || (animClip_info == "flying") 
                    || (animClip_info == "railSlide") ||  (animClip_info == "slideDown")
                );
                
                // force aimed enemy to be recognized once (so it doesn't get called infintely)
                // optional re-call for differents aims types settings [arm || not-arm]
                if( (aimed_enemy != lastAimed_enemy) || (lastAimSettings_isArm != armRigMaybe )  || (lastArmSetting_type != animClip_info))
                {
                    // can only auto-aim while [flying, running, wallrunnning, sliding, grappling, slideRail]
                    if( (authorizedShooting_s.Contains(animClip_info) && animClip_info != "slideDown") ||
                        (animClip_info == "slideDown" && !special_rampDelay)
                    )
                    {
                        lastAimSettings_isArm = armRigMaybe;
                        lastArmSetting_type = animClip_info;
                        setAimSettings(true, aimed_enemy, armRigMaybe, animClip_info);
                        aim_Recoil = 0.0f;
                        canShot = true;
                        // break;
                    }

                    // Enemy aimed but can't auto-aim !
                    if(!canShot)
                    {
                        // prevent from non-autoAim setting to be called too much
                        if(wasLastAim_AutoAim == true)
                        {
                            setAimSettings(false);
                            rig_animController.enabled = false;
                        }
          
                    }
                }
            }


            // Aim Shooting adjustments [Flying & Sliding & RampSliding]
            if( (aimed_enemy != null) && (ammo > 0) && (plyr_flying || plyr_sliding || plyr_rampSliding)) 
            {
                if(plyr_flying && !plyr_sliding && !plyr_rampSliding) // flying
                {

                    if(horizontal_enemy)
                    {
                        head_TARGET.position = aimed_enemy.position + 
                            new Vector3(12, 
                                (aimed_enemy.position.x - transform.position.x > 0 ) ? -2 : 2, 
                                (aimed_enemy.position.x - transform.position.x > 0 ) ? -2 : 2
                            );

                        body_TARGET.position = aimed_enemy.position + 
                            new Vector3(1.9f,
                                (aimed_enemy.position.x - transform.position.x > 0 ? -4 : 4),
                            + 5.4f);
                     
                    }else
                    {
                        float dst = Vector3.Distance(transform.position, aimed_enemy.position);

                        head_TARGET.position = new Vector3(
                            aimed_enemy.position.x  + (dst < 12 ? +2.5f : +7f), 
                            aimed_enemy.position.y + 1f,
                            aimed_enemy.position.z + ((aimed_enemy.position.x - transform.position.x > 0 ) ? 1f : -1f)
                        );

                        body_TARGET.position = new Vector3(aimed_enemy.position.x + 4f, aimed_enemy.position.y + 1.5f, aimed_enemy.position.z + 4f);
                    }



                    // rotate model [flying]
                    if(!plyr_wallRninng && !plyr_railSliding && !plyr_hanging && !plyr_rampSliding)
                    {
                        Vector3 relativePos = aimed_enemy.position - transform.position;
                        Quaternion y_aim = Quaternion.LookRotation(relativePos);
                        float y_fl = (y_aim.eulerAngles.y > 180f) ? (360f - y_aim.eulerAngles.y) : (y_aim.eulerAngles.y);

                        pico_character.transform.localRotation = Quaternion.Euler(
                            0f, 
                            Math.Abs(y_fl) > 25f ? 
                                (y_fl > 0 ? 25f : -25f) : y_fl,
                            // + transform.rotation.eulerAngles.y / 6, 
                            0f
                        );
                    }

                }
                else // sliding & rampsliding
                {   
                    head_TARGET.position = aimed_enemy.position + (plyr_rampSliding ? new Vector3(6f, -8f, 0f) : new Vector3(0, -2f, 0) );
                }
    
            }


            // Aim Shooting [Running]
            if( (aimed_enemy != null) && (ammo > 0)  && (!plyr_flying && !plyr_sliding) )
            {
                float dst = Vector3.Distance(transform.position, aimed_enemy.position);
                if(dst < 12)
                {
                    body_TARGET.position = aimed_enemy.position  + new Vector3(0f, 0.5f, 0f);
                    head_TARGET.position = aimed_enemy.position  + new Vector3(horizontal_enemy ? 0.9f : 1.2f, 0f, 0f);
                }else
                {
                    Vector3 running_v3Pos = aimed_enemy.position + (
                        horizontal_enemy ?
                        new Vector3(1.9f, 0, -1f) : new Vector3(2.4f, -1.2f, -1f)
                    );

                    body_TARGET.position = running_v3Pos;
                    head_TARGET.position = running_v3Pos + new Vector3(4.35f, 0f, 0f);
                }
         
            }

            // Aim Not-Shooting [Running]
            if( (aimed_enemy == null) && (ammo > 0)  && (!plyr_flying && !plyr_sliding) )
            {
                body_TARGET.localPosition = new Vector3(5f, -10f, 0f);
                head_TARGET.localPosition = new Vector3(-5f, 5f, 10f);     
            }

            // apply recoil
            leftArm_TARGET.position = new Vector3(saved_WorldPos_armTarget.x, saved_WorldPos_armTarget.y + aim_Recoil, saved_WorldPos_armTarget.z);
            



            if(!plyr_tyro && !plyr_intro_tyro && !plyr_wallRninng && !plyr_railSliding && 
                !plyr_hanging && !plyr_climbing && !plyr_sideHanging)
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
                // MAIN MOVEMENT SPEED // disabled for slideRail & ladder
                
                if(!plyr_railSliding && !plyr_climbingLadder)
                {
                    // SLIDING SPEED
                    if(plyr_sliding) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 
                        (player_speed + momentum_ + action_momentum) * 1.75f
                    );

                    // WALL RUN 
                    else if(plyr_wallRninng) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 5f, 
                        (player_speed + momentum_ + action_momentum) * 1.4f
                    );

                    // RAMP SLIDING
                    else if(plyr_rampSliding) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 
                        (player_speed + momentum_ + action_momentum) * 1.25f
                    );

                    // RUNNING SPEED
                    else
                    {

                        // DEFAULT SPEED
                        // if (!Input.GetKey("q") && !Input.GetKey("d")) 
                            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, (player_speed + momentum_ + action_momentum) );
                        
                        // STRAFE SPEED
                        // if (Input.GetKey("q") || Input.GetKey("d")) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, (player_speed + momentum_ + action_momentum) * 0.5f );
                        
                
                        if(Input.GetKeyDown("f")) FLYYY = !(FLYYY);
                        if(FLYYY) plyr_rb.velocity = new Vector3(0f, plyr_rb.velocity.y < 0.60f ? 0.60f : plyr_rb.velocity.y, 0f);
                    }


                    // STRAFE FORCES
                    if ( (Input.GetKey("q") || lft_Straf ) )
                    {
                        // LEFT
                        if(!plyr_wallRninng && !plyr_bumping){
                            if ( (plyr_trsnfm.rotation.eulerAngles.y >= 311.0f && plyr_trsnfm.rotation.eulerAngles.y <= 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y <= 43.0f) )
                            {
                                plyr_.transform.Rotate(0, plyr_animKilling ? -3f : -2.50f, 0, Space.Self);
                            } 
                        }
                        
                        if(plyr_rb.velocity.x > -9)  plyr_rb.AddForce((-4 * (Vector3.right * strafe_speed) ), ForceMode.VelocityChange);
                        else  plyr_rb.velocity = new Vector3(-9, plyr_rb.velocity.y, plyr_rb.velocity.z);
                    }

                    if ( (Input.GetKey("d") || rght_Straf ) )
                    { 
                        
                        // RIGHT
                        if(!plyr_wallRninng && !plyr_bumping){
                            if ( (plyr_trsnfm.rotation.eulerAngles.y >= 309.0f) || ( Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) >= 0.0f && Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) <= 41.0f) )
                            {
                                plyr_.transform.Rotate(0, plyr_animKilling ? 3f : 2.50f, 0, Space.Self);
                            }
                        }

                        if(plyr_rb.velocity.x < 9)  plyr_rb.AddForce((-4 * (Vector3.left * strafe_speed) ), ForceMode.VelocityChange);
                        else  plyr_rb.velocity = new Vector3(9, plyr_rb.velocity.y, plyr_rb.velocity.z);
                    }



                    // shotting y velocity fix
                    if(plyr_shooting && plyr_flying && !plyr_animKilling && !plyr_wallExiting)
                    {
                        if(plyr_rb.velocity.y < -8f) plyr_rb.velocity =  new Vector3(plyr_rb.velocity.x, -8f, plyr_rb.velocity.z);
                    }

                    // freeze y anim kill anim
                    if(plyr_animKilling)
                    {
                        plyr_rb.velocity =  new Vector3(plyr_wallExiting ? plyr_rb.velocity.x/3 : plyr_rb.velocity.x/2, plyr_rb.velocity.y, plyr_rb.velocity.z * 0.8f);
                    }


                }


                // railSlide Cancel
                if ((Input.GetKey("q") || Input.GetKey("d")) && plyr_railSliding)
                { 
                    plyr_railSliding = false;
                    plyr_rb.AddForce(( (Input.GetKey("q") ? 100 : -100 )
                        * (Vector3.left * strafe_speed) ),
                        ForceMode.VelocityChange
                    );
                    plyr_rb.AddForce( (Vector3.up * 20), ForceMode.VelocityChange );
                    
                    jump_auth = true;
                    _anim.SetBool("slideRail", false);
                    plyr_rb.useGravity = true;
                    plyr_railSliding = false;

                    // cam exit
                    cm_movement.railSlide_offset(true);
                    slideRail_ref = null;
                }


                // shoot
                if ( 
                    (!plyr_obstclJmping && !plyr_jumping && !plyr_animKilling) 
                                    &&
                    (ammo > 0) && (aimed_enemy != null) )
                {
                    StopCoroutine(shooting_());StartCoroutine(shooting_());
                    player_weaponScrpt.Shoot(aimed_enemy, aimed_enemy.GetComponent<AutoTurret>().is_horizontal);
                };
                

                // ladder
                if(plyr_climbingLadder)
                {
                    if(plyr_rb.velocity.y < 0.6f) plyr_rb.velocity =  new Vector3(plyr_rb.velocity.x, 0.6f, plyr_rb.velocity.z);

                    // ladder cancel
                    if ((Input.GetKey("q") || Input.GetKey("d")))
                    { 
                        plyr_rb.AddForce(  (Input.GetKey("q") ? 30 : -30) * (Vector3.left * strafe_speed) , ForceMode.VelocityChange);
                    }
                }

          
                
            }


            // side hang
            if(plyr_sideHanging)
            {
                if(plyr_rb.velocity.y < 0.6f) plyr_rb.velocity =  new Vector3(plyr_rb.velocity.x, 0.6f, plyr_rb.velocity.z);
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





            // tactile inputs
            if (Input.touchCount == 1 && (true == false)) // user is touching the screen with a single touch
            {
                Touch touch = Input.GetTouch(0); // get the touch
                if (touch.phase == TouchPhase.Began) //check for the first touch
                {
                    fp = touch.position;
                    lp = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
                {
                    lp = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
                {
                    lp = touch.position;  //last touch position. Ommitted if you use list
    
                    //Check if drag distance is greater than 20% of the screen height
                    if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                    {//It's a drag
                    
                        //check if the drag is vertical or horizontal
                        if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                        {   
                            //If the horizontal movement is greater than the vertical movement...
                            if ((lp.x > fp.x))  //If the movement was to the right)
                            {   //Right swipe
                                Debug.Log("Right Swipe");
                            }
                            else
                            {   //Left swipe
                                Debug.Log("Left Swipe");
                            }
                        }

                        else
                        {   
                            //the vertical movement is greater than the horizontal movement
                            if (lp.y > fp.y)  //If the movement was up
                            {   //Up swipe
                                Debug.Log("Up Swipe");
                            }
                            else
                            {   //Down swipe
                                Debug.Log("Down Swipe");
                            }
                        }
                    }
                    else
                    {   //It's a tap as the drag distance is less than 5% of the screen width
                        Debug.Log("Tap");
                    }
                }
            }




        }
    }


    // Public fnc for cllision 
    public void animateCollision(string cls_type, Vector3 cls_size,  GameObject optional_gm = null)
    {   

        float jumpForce = Mathf.Sqrt(4.5f * -2 * (Physics.gravity.y));
        List<string> interact_Jmps = new List<string>(new string[3] {"tapTapJump", "bumper", "launcherHit"} );
        List<string> combo_hits = new List<string>(new string[9]{
            "ladderHit", "sideHangHit", "fallBoxHit", "hang", "railSlide", "slideHit", "tapTapJump", "bumper", "launcherHit"
        } );
        List<string> combo_breakers = new List<string>(new string[1] {"obstacleHit"} );

        if(gameOver_) return;

        if(interact_Jmps.Contains(cls_type))
        {
            FindObjectOfType<CameraMovement>().special_jmp();
            StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.0f));
        };

        if(combo_hits.Contains(cls_type))
        {
            combo_++;
        };

        if(cls_type == "slideHit") momentum_ += 4f;
        if(cls_type == "railSlide") momentum_ += 2f;
        if(cls_type == "tapTapJump") momentum_ += 6f;
        if(cls_type == "bumper" || cls_type == "launcherHit") momentum_ += 3f;

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
                if(!_anim.GetBool("GroundHit") && !plyr_climbing && !plyr_sideHanging)
                {
                    if (plyr_animKilling || plyr_hanging) return;

                    _anim.SetBool("Flying", false);
                    _anim.SetBool("wallRun", true);
                    plyr_wallRninng = true;

                    GameObject hitWall = optional_gm;
                    float sns = (hitWall.transform.position.x - gameObject.transform.position.x);
                    
                    float h_y = hitWall.transform.rotation.eulerAngles.y;
                    int quarter = ((int)(h_y) / 90);
                    float y_bonus = quarter > 0 ? (h_y
                        - (quarter * 90)
                        + (
                            (h_y/ 90) - (quarter) 
                        ) > 0.75f ? 
                        (h_y/ 90) - (quarter) : 0
                    ) * 10 : (h_y <= 30 ? 
                        h_y : 0
                    );

                    cm_movement.wal_rn_offset(false, hitWall.transform, y_bonus);

                    if(sns < 0){ 
                        StartCoroutine(force_wallRn(true));
                        plyr_rb.AddForce(Vector3.left * 10, ForceMode.VelocityChange);
                    }else{
                        StartCoroutine(force_wallRn(false));
                        plyr_rb.AddForce(Vector3.right * 10, ForceMode.VelocityChange);
                    }
                    // Debug.Log(gameObject.transform.rotation.eulerAngles.y);
                    Vector3 p_ = new Vector3( sns < 0 ? -0.30f : 0.4f, 0, 0);
                    Quaternion q_ = Quaternion.Euler(0,  (sns <  0 ? -30f : -41f) + y_bonus, sns <  0 ? -47f : 47f);
        
                    pico_character.transform.localRotation = q_;
                    pico_character.transform.localPosition = p_;

                    psCollisions_movement.wallRun_aimBox = true;
                    psCollisions_movement.z_wallRun_aimRotation = ( (sns <  0) ? -47f : 47f );
                }
                break;

            case "wallRunExit": 
                if(!plyr_climbing && !plyr_sideHanging)
                {
                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.5f));
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 2, plyr_rb.velocity.y, plyr_rb.velocity.z);
                    plyr_rb.AddForce( new Vector3(0, 23f, 0), ForceMode.VelocityChange);
                    _anim.SetBool("Flying", true);
                    _anim.SetBool("wallRun", false);
                    plyr_wallRninng = false;

                    pico_character.transform.rotation = new Quaternion(0, 0, 0, 0);
                    pico_character.transform.localPosition = new Vector3(0, 0, 0);

                    psCollisions_movement.wallRun_aimBox = false;

                    StartCoroutine(wall_exit());

                    StopCoroutine(shooting_());
                    lft_Straf = rght_Straf = false;
                }           
                break;

            case "frontWallHit":
                if(!plyr_wallRninng && !plyr_climbing && !plyr_climbingLadder)
                {
                    // if is a rebord hit
                    bool is_rebord = false;

                    Mesh m_contact = optional_gm.GetComponent<MeshFilter>().sharedMesh;

                    float msh_y2 = (m_contact.bounds.size.y/2)* (
                        optional_gm.transform.lossyScale.y
                    ); // good
                    Matrix4x4 p = optional_gm.transform.localToWorldMatrix;

                    float top_y2 = ((optional_gm.transform.position.y)
                        + (msh_y2) + 
                        ((float) (m_contact.bounds.center.y * optional_gm.transform.lossyScale.y))
                    ); // good

                    // Debug.Log("center " + optional_gm.transform.position.y + 
                    //     "  mshSize? " + msh_y2+
                    //     "   centerMsh " + m_contact.bounds.center.y 
                    // );

                    is_rebord = (top_y2 - transform.position.y) <= 10f ? true : false;
                    if(is_rebord)
                    {
                        movement_auth = false;
                        plyr_rb.useGravity = false;
                        plyr_climbing = true;
                        

                        float r_y = optional_gm.transform.rotation.eulerAngles.y;
                        int q = ((int)(r_y) / 45);
                        float y_bonus = q > 0 ? (r_y
                            - (q * 45)
                            + ( (r_y/ 45) - (q) 
                        ) > 0.75f ? 
                            (r_y/ 45) - (q) : 0
                        ) * 10 : ( r_y <= 40f ? r_y : 0);
             
                        transform.rotation = Quaternion.Euler(0, y_bonus, 0f);
                        plyr_rb.velocity = new Vector3(0, 0, 0);

                        cm_movement.climbUp();

                        rotate_bck();
                        StartCoroutine(Dly_bool_anm(1.1f, "climb"));
                          
                        LeanTween.move(gameObject, 
                            new Vector3(transform.position.x, top_y2 - 0.4f , transform.position.z - 0.02f), 
                        0.9f).setEaseInOutCubic();

                        plyr_flying = true;
                    }else
                    {
                        gameOver_ = true;
                        _anim.SetBool("frontWallHit", true);
                        FindObjectOfType<CameraMovement>().cam_GamerOver_();
                    }
                }
                break;

            case "sliderHit":
                plyr_sliding = true;
                _anim.SetBool("slide", true);
                rotate_bck();
         
                jumpCnt = 2;
                
                plyr_rb.AddForce( new Vector3(0, jumpForce * 0.15f, 5), ForceMode.VelocityChange);
                fix_Cldrs_pos( 0.42f, true);
                break;

            case "sliderLeave":
                plyr_sliding = false;
                _anim.SetBool("slide", false);
                
                if(!plyr_jumping) plyr_rb.AddForce( new Vector3(0, jumpForce * 1.2f, -6), ForceMode.VelocityChange);

                fix_Cldrs_pos(-0.42f, false);    

                break;


            case "launcherHit":
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2f, 0), ForceMode.VelocityChange);
                StartCoroutine(Dly_bool_anm(0.90f, "launcherJump"));
                action_momentum += 10f;
                break;

            case "tapTapJump":
                if(plyr_tapTapJumping) return;
                _anim.SetBool("Flying", true); 
                rotate_bck();
                StartCoroutine(Dly_bool_anm(1.5f, "tapTapJump"));
                StartCoroutine(tapTapJumpAnim(optional_gm));
                break;

            case "gun":
                player_weaponScrpt.reload();
                player_weaponScrpt.equip_Weapon(false);
                
                if(aimed_enemy == null)
                {
                    head_TARGET.parent = transform; body_TARGET.parent = transform;
                    body_TARGET.localPosition = new Vector3(1.4f, -5.13f, 4.0f);
                    head_TARGET.localPosition = new Vector3(6.80f,-10.57f,16.06f);
                }
                break;

            case "newEnemyAim":
                // horizontal_enemy = optional_gm.transform.rotation.eulerAngles.z > 45f ? true : false;
                horizontal_enemy = optional_gm.GetComponent<AutoTurret>().is_horizontal;

                // Vector3 adjusteBodyAim  = optional_gm.transform.position + (
                //     horizontal_enemy ?
                //     new Vector3(1.9f, 0, -2.4f) : new Vector3(2.4f, -1.2f,-2.4f)
                // );

                // Vector3 adjusteHeadAim  = (
                //     horizontal_enemy ?
                //     new Vector3(-3.5f, 0f, -2.5f) : new Vector3(0, 1f, 0)
                // );

                head_TARGET.parent = optional_gm.transform;body_TARGET.parent = optional_gm.transform;
                leftArm_TARGET.parent = optional_gm.transform;

                leftArm_TARGET.localPosition =  (horizontal_enemy ?
                    new Vector3(0.37f,1.15f,-0.56f) : new Vector3(0, 1.5f, 0) );

                // saved_bodyTarget = adjusteBodyAim; 
                // saved_headTarget = adjusteHeadAim;
                saved_WorldPos_armTarget = leftArm_TARGET.position;


                aimed_enemy = optional_gm.transform;
                if(ammo > 0) cm_movement.set_aimedTarget = optional_gm.transform;

                g_ui.newEnemy_UI(false, aimed_enemy);
                break;

            case "emptyEnemyAim":      
                head_TARGET.parent = transform; body_TARGET.parent = transform; leftArm_TARGET.parent = transform;
                leftArm_TARGET.localPosition = new Vector3(0, 0, 0);
                
                int rdm_ = UnityEngine.Random.Range(0, 3);
                body_TARGET.localPosition = rdm_ == 1 ? new Vector3(1.4f, -5.13f, 4.0f) : new Vector3(1.09f,-5.13f,1.74f);

                head_TARGET.localPosition = new Vector3(6.80f,-10.57f,16.06f);
              
                aimed_enemy = null;
                cm_movement.set_aimedTarget = null;
                g_ui.newEnemy_UI(true);
                break;

            case "railSlide":
                StopCoroutine(delay_jumpInput(0.0f));
                if(slideRail_ref == optional_gm) { return; };

                jump_auth = false;
                cm_movement.railSlide_offset(false);

                _anim.SetBool("slideRail", true);
                tyro_handler_child = null;


                PathCreator[] paths_ = optional_gm.GetComponentsInChildren<PathCreator>();
                actual_path = paths_[0];

                float r_speed =  paths_[0].path.GetPointAtTime(0.99f).z - paths_[0].path.GetPointAtTime(0f).z;
                // adaptive rail speed
                // railSlide_speed = (r_speed / 100) < 0.16f ? 0.16f : r_speed / 100;
                railSlide_speed = (2 / (paths_[0].gameObject.transform.lossyScale.z * 1.2f) );
                // Debug.Log(paths_[0].gameObject.transform.lossyScale + " vs " + railSlide_speed);

                end_tyroPos = actual_path.path.GetPoint(actual_path.path.NumPoints - 1);
                last_tyro_trvld = actual_path.path.GetClosestDistanceAlongPath(transform.position);   

                // turn off gravity
                plyr_rb.useGravity = false;
                plyr_rb.velocity = new Vector3(0, 0, 0);

                pico_character.transform.localRotation = Quaternion.Euler(0f, 85f, 0f);
                plyr_railSliding = true;

                // ui bonus call
                g_ui.set_countBonus_= true;

                slideRail_ref = optional_gm;

                break;
            case "railSlideExit":
                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 2, plyr_rb.velocity.y, plyr_rb.velocity.z);
                break;
            case "hang":
                movement_auth = false;
                hang_obj = optional_gm;

                // turn off gravity
                plyr_rb.useGravity = false;
                plyr_rb.velocity = new Vector3(0, 0, 0);

                Collider c_ = optional_gm.GetComponent<Collider>();

                float x_ps = transform.position.x;
                float x_sz = ( optional_gm.transform.position.x - transform.position.x > 0f ? 
                    ((c_.bounds.size.x/2)): -1 * ((c_.bounds.size.x/2))
                    );

                Vector3 pos_ =  optional_gm.transform.position + new Vector3(0f, 0.0f, 0.05f);
                pos_ = new Vector3( ( Math.Abs(( optional_gm.transform.position.x + 
                        x_sz) - x_ps)
                        < (0.2f)
                     ? 3 * (x_sz/4) / 4 : x_ps), 
                    pos_.y, pos_.z
                );
                c_.enabled = false;
                transform.position = pos_;

                Quaternion forced_y = Quaternion.Euler(0f, optional_gm.transform.rotation.eulerAngles.y > 180f ? 
                    -1 * (360f - optional_gm.transform.rotation.eulerAngles.y) :
                    optional_gm.transform.rotation.eulerAngles.y
                ,0f);
                transform.rotation = forced_y;

                Transform pico_p = pico_character.transform.parent;
                pico_character.transform.localPosition = new Vector3(0, -1.050f, -0.532f);
                plyr_hanging = true;
                pico_character.transform.localRotation = Quaternion.Euler(0, -13.2f, 0);

                Transform root_ = game_cam.transform.parent;

                LTDescr rt_animation = LeanTween.rotateLocal(pico_p.gameObject,new Vector3(-60f, 0f, 0f), 3.5f).setEaseInOutCubic();
                cm_movement.stored_picoParent = pico_p;
    
                moving_hangTraveler = new GameObject("moving_hangTraveler");
                moving_hangTraveler.transform.parent = pico_p;

                Vector3 init_camPos = new Vector3(transform.position.x, optional_gm.transform.position.y - 0.35f, 
                    optional_gm.transform.position.z - 5f
                );

                moving_hangTraveler.transform.position = init_camPos;
                moving_hangTraveler.transform.localPosition = new Vector3(0, moving_hangTraveler.transform.localPosition.y, moving_hangTraveler.transform.localPosition.z);

                game_cam.transform.position = init_camPos;
                game_cam.transform.position =  new Vector3(moving_hangTraveler.transform.position.x, game_cam.transform.position.y, game_cam.transform.position.z);

                cm_movement.hang_point = moving_hangTraveler.transform;
                LeanTween.moveLocal(moving_hangTraveler, new Vector3(0, -2f, 2f), 4.75f).setEaseInOutCubic();
         
                game_cam.transform.rotation = Quaternion.Euler( game_cam.transform.rotation.eulerAngles.x, 0f, game_cam.transform.rotation.eulerAngles.z);

                pico_character.transform.localRotation = Quaternion.Euler(0, -6.2f, 0);
                StartCoroutine(Dly_bool_anm(1.7f, "hang"));
                StartCoroutine(hang_transformFixes_andReset(pico_p, moving_hangTraveler, rt_animation));
                break;

            case "rampSlide":
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1f));
                rotate_bck();
                fix_Cldrs_pos( 0.42f, true);

                plyr_rampSliding = true;
                _anim.SetBool("rampSlide", true);

                pico_character.transform.localRotation = Quaternion.Euler(0f, 92f, 
                    (optional_gm.transform.rotation.eulerAngles.x > 180f ? (360f - optional_gm.transform.rotation.eulerAngles.x ) : optional_gm.transform.rotation.eulerAngles.x ) * 0.6f
                );
                pico_character.transform.localPosition = new Vector3(0f, 0.14f, -0.45f);

                Invoke("ramp_bool", 1f);
                jumpCnt = 2;
                break;
            case "rampSlideExit":   
                _anim.SetBool("rampSlide", false);
                plyr_rampSliding = false;

                fix_Cldrs_pos(-0.42f, false);    

                pico_character.transform.localRotation = Quaternion.identity;
                pico_character.transform.localPosition = Vector3.zero;

                special_rampDelay = true;
                break;
            case "fallBoxHit":
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.7f));
                StartCoroutine(Dly_bool_anm(1.75f, "fallBox"));
                _anim.SetBool("Flying", false);
                rotate_bck();
                movement_auth = false;

                jumpCnt = 2;
                if (plyr_flying)
                {           
                    fix_Cldrs_pos(-0.12f, false);
                    plyr_flying = false;
                }
                break;
            case "fallBoxExit": 
                fix_Cldrs_pos(0.12f, true);
                _anim.SetBool("Flying", true);
                plyr_flying = true;
                break;

            case "ladderHit":
                _anim.SetBool("ladder", true);

                transform.position = new Vector3(optional_gm.transform.position.x -0.15f, transform.position.y, transform.position.z);

                plyr_rb.velocity = new Vector3(0, 0, 0);
                plyr_climbingLadder = true;

                cm_movement.ladderClimb_offst(false);


                // moving_camTraveler = new GameObject("moving_camTraveler");
                // moving_camTraveler.transform.parent = pico_character.transform;
                // moving_camTraveler.transform.localPosition = Vector3.zero;

                // game_cam.transform.rotation = Quaternion.Euler(-30f, 0f, 0f);

                // cm_movement.hang_point = moving_camTraveler.transform;

                StartCoroutine(ladder_climb(moving_camTraveler));

                break;

            case "ladderLeave":
                _anim.SetBool("ladder", false);
                plyr_climbingLadder = false;

                plyr_rb.AddForce( new Vector3(0, 25f, -10f), ForceMode.VelocityChange);

                StopCoroutine(ladder_climb());

                cm_movement.ladderClimb_offst(true);
                cm_movement.special_jmp();
                break;

            case "sideHangHit":
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.3f));
                StartCoroutine(Dly_bool_anm(1.5f, "sideHang"));

                plyr_sideHanging = true;

                sHng_leftSide = (optional_gm.transform.position.x < transform.position.x ) ? true : false;

                Vector3 pos = new Vector3(
                    optional_gm.transform.position.x + 0.25f,
                    optional_gm.transform.position.y - 1.6f,
                    optional_gm.transform.position.z
                );

                transform.position = pos;
                // LeanTween.move(gameObject, pos, 1.5f).setEaseInOutCubic();
     
                pico_character.transform.localRotation = Quaternion.Euler(
                    0, optional_gm.transform.rotation.eulerAngles.y, 0
                );
                plyr_rb.velocity = new Vector3(0, 0, 0);
                // plyr_rb.useGravity = false;
                movement_auth = false;

                LeanTween.rotate(pico_character, Vector3.zero, 1.5f).setEaseInOutCubic();
                Invoke("sideHng_force", 0.5f);
                break;

            case "bumper":

                float y_ = ( Math.Abs(optional_gm.transform.rotation.eulerAngles.y) > 42f ? 
                    (optional_gm.transform.rotation.eulerAngles.y > 0 ?
                         42f : -42f
                    ) : optional_gm.transform.rotation.eulerAngles.y
                );
                plyr_rb.AddTorque( new Vector3(0, y_ * 50, 0), ForceMode.VelocityChange);

                plyr_rb.AddForce( new Vector3(y_/4, jumpForce * 1.5f, 0), ForceMode.VelocityChange);

                StartCoroutine(Dly_bool_anm(0.8f, "bumper"));
                plyr_bumping = true;

                cm_movement.bump(y_);
                action_momentum += 7f;
                break;

            default:
                break;
        }
    }

    // sideHang delayed force
    private void sideHng_force()
    { 
        plyr_rb.AddForce( new Vector3(sHng_leftSide ? 7f : -7f, 15f, 8f), ForceMode.VelocityChange);
        cm_movement.sideHg(sHng_leftSide);
    }


    // ramp delay
    private void ramp_bool(){ special_rampDelay = false;}


    // ladder
    private IEnumerator ladder_climb(GameObject moving_camTraveler = null)
    {
        LTDescr[] lts_ = new LTDescr[2];
        for(int i = 0; i < 100; i ++)
        {
            if(!plyr_climbingLadder)
            {
                // if(lts_[0] != null) lts_[0].cancel(gameObject);
                // if(lts_[1] != null) lts_[1].cancel(moving_camTraveler); 
                // Destroy(moving_camTraveler);
                break;
            }

            // LTDescr a =  LeanTween.move(gameObject, new Vector3(
            //     transform.position.x, transform.position.y + 1.1f, transform.position.z),
            // i % 2 == 0 ? 0.9f : 0.75f).setEaseInOutCubic();
            
            // LTDescr b = LeanTween.move(moving_camTraveler, new Vector3(
            //     moving_camTraveler.transform.position.x, moving_camTraveler.transform.position.y + 1.1f, moving_camTraveler.transform.position.z),
            // i % 2 == 0 ? 0.9f : 0.75f).setEaseInOutCubic();

            // lts_[0] = a; lts_[1] = b;
            plyr_rb.AddForce( new Vector3(0, i % 2 == 0 ? 14f : 13f, 0), ForceMode.VelocityChange);
            yield return new WaitForSeconds(i % 2 == 0 ? 0.7f : 0.6f);
        }
        yield break;
    }

    // hang
    private IEnumerator hang_transformFixes_andReset(Transform pico_p, GameObject hang_traveler, LTDescr rt_animToStop)
    {
        yield return new WaitForSeconds(0.9f);
        LeanTween.moveLocal(pico_p.gameObject, new Vector3(0f, -0.1f,0f), 0.1f);
        yield return new WaitForSeconds(0.1f);
        LeanTween.moveLocal(pico_p.gameObject, new Vector3(-0.001f, 0.2f,0.60f), 0.4f);
        yield return new WaitForSeconds(0.45f);

        plyr_hanging = false;
        pico_character.transform.localRotation = Quaternion.identity;
        pico_p.transform.localRotation = Quaternion.identity;

        plyr_rb.useGravity = true;
        plyr_rb.velocity = new Vector3(0, 5, 0);
        plyr_rb.AddForce( new Vector3(0, (jumpAmount * 1.2f), 0f), ForceMode.VelocityChange);

        momentum_ += 2f;
        // LeanTween.rotateLocal(pico_p.gameObject,new Vector3(0f, 0f, 0f), 0.4f).setEaseInOutCubic();
        rt_animToStop.cancel(pico_p.gameObject);
        pico_p.transform.localPosition = Vector3.zero;
        pico_character.transform.localPosition = Vector3.zero;

        cm_movement.hang_jumpOut();
        Destroy(hang_traveler);

        movement_auth = true;
    }



    // TYRO
    public void tyro_movement(GameObject path_obj)
    {
        if(plyr_tyro) return;

        // ui bonus
        g_ui.set_countBonus_ = true;

        // turn off all movements
        movement_auth = false;
        // turn off gravity
        plyr_rb.useGravity = false;
        plyr_rb.velocity = new Vector3(0, 0, 0);

        last_tyro_trvld = 1.25f;   

   
        Transform prnt_ = path_obj.transform.parent;
        PathCreator[] paths_ = prnt_.GetComponentsInChildren<PathCreator>();

        for(int i =0; i < paths_.Length; i ++)
            if(paths_[i].gameObject.transform.parent.tag != "slideRail")
                actual_path = paths_[i]; 
        
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

        //LeanTween.scale(tyro_handler_child.gameObject, tyro_handler_child.localScale * 2f, 1.5f).setEasePunch();

        Vector3 moveTo_ = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, -2.1f, -0.1f);
        LeanTween.move(gameObject, moveTo_, 0.6f).setEaseInSine(); 
        LeanTween.move(tyro_handler_child.gameObject, actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, 0.05f, 0), 0.6f).setEaseInSine(); 

        LeanTween.rotate(tyro_handler_child.gameObject, new Vector3(0, actual_path.path.GetRotationAtDistance(2f).eulerAngles.y / 2f, 0), 0.6f).setEaseInSine();
        LeanTween.rotate(gameObject, new Vector3(0, actual_path.path.GetRotationAtDistance(2f).eulerAngles.y / 4.30f, 0), 0.65f).setEaseInSine();
        
        Invoke("activateTyro", 1.0f);
        cm_movement.tyro_offset(false);
        //plyr_tyro = true;
        
    }
    private void activateTyro(){plyr_tyro = true; plyr_intro_tyro = false;}



    // THROW AN ANIMATION BOOL 
    private IEnumerator Dly_bool_anm(float delay, string anim_bool)
    {
        _anim.SetBool(anim_bool, true);
        if(anim_bool == "Jump" || anim_bool == "DoubleJump") plyr_jumping = true;
        if(anim_bool == "animKill") plyr_animKilling = true;

        //yield on a new YieldInstruction that waits for "delay" seconds.
        yield return new WaitForSeconds(delay);

        if(anim_bool == "Jump" || anim_bool == "DoubleJump") plyr_jumping = false; 

        if(anim_bool == "animKill")
        {
            plyr_animKilling = false; 
            plyr_rb.AddForce( new Vector3(0f, 12f, 0f), ForceMode.VelocityChange);
        }

        if(anim_bool == "fallBox") movement_auth = true;

        if(anim_bool == "climb")
        {
            plyr_climbing = false;
            movement_auth = true;
            plyr_rb.useGravity = true;
            plyr_rb.AddForce( new Vector3(0f, 15f, 0f), ForceMode.VelocityChange);
        }

        if(anim_bool == "sideHang")
        {
            movement_auth = true;
            plyr_sideHanging = false;
        }

        if(anim_bool == "bumper") { plyr_bumping  = false;} 
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

    // TAPTAPJUMP ANIMATION
    private IEnumerator tapTapJumpAnim(GameObject obstacl_gm)
    {
        movement_auth = false;
        plyr_rb.useGravity = false;
        plyr_tapTapJumping = true;

        plyr_rb.velocity = new Vector3(0, 0, 0);
        Collider[] boxes = obstacl_gm.transform.parent.GetComponentsInChildren<Collider>();

        bool fst_box_touch = (obstacl_gm == obstacl_gm.transform.parent.GetChild(0).gameObject) ? true : false;

        if(fst_box_touch)
        {
            bool front_of_box = ( Math.Abs(transform.position.z 
                - (boxes[0].gameObject.transform.position.z - (boxes[0].bounds.size.z / 2) )) < 0.4f ? true : false);
            // FirstBox jump
            // y
            if(front_of_box)
            {
                LeanTween.moveY(gameObject, 
                    ((boxes[0].bounds.size.y/2) * boxes[0].gameObject.transform.localScale.y) + boxes[0].gameObject.transform.position.y, 
                    0.14f
                ).setEaseInSine();
                yield return new WaitForSeconds(0.15f); 
            }
            // z
            LeanTween.moveZ(gameObject, 
                ((boxes[0].bounds.size.z/2) * boxes[0].gameObject.transform.localScale.z) + boxes[0].gameObject.transform.position.z, front_of_box ? 0.29f : 0.44f).setEaseInSine();
            yield return new WaitForSeconds(front_of_box ? 0.3f : 0.45f); 
            // y [2nd box]
            LeanTween.moveY(gameObject, 
                boxes[1].bounds.size.y/2 + boxes[1].gameObject.transform.position.y, 
                0.20f
            ).setEaseInSine();
            yield return new WaitForSeconds(0.20f); 
            
        }
        

        movement_auth = true;
        plyr_rb.useGravity = true;
        plyr_tapTapJumping = false;
    }




    // kick obj when alr obst jumping
    private void kickObst(GameObject obst)
    {
        Rigidbody obst_rb = obst.GetComponent<Rigidbody>() == null ? obst.AddComponent<Rigidbody>() : obst.GetComponent<Rigidbody>();
        obst_rb.mass = 0.01f;
        Vector3 randTorque = new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-30, 30f), UnityEngine.Random.Range(15f, -15f));
        Vector3 kickForce = new Vector3(
            plyr_trsnfm.position.x - obst.transform.position.x,
            plyr_trsnfm.position.y - obst.transform.position.y, plyr_trsnfm.position.z - obst.transform.position.z
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

    // quit wall 
    private IEnumerator wall_exit()
    {
        plyr_wallExiting = true;
        yield return new WaitForSeconds(0.6f);
        plyr_wallExiting = false;
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


    // Coroutine to keep Shooting boolean
    private IEnumerator shooting_()
    {
        plyr_shooting = true;
        yield return new WaitForSeconds(1.1f);
        plyr_shooting = false;
    }


    // force wallrun
    private IEnumerator force_wallRn(bool side_)
    {
        if(side_)
        {
            lft_Straf = true;
        }else
        {
            rght_Straf = true;
        }
        yield return new WaitForSeconds(0.25f);
        lft_Straf = rght_Straf = false;
    }

    // Player Kill
    public void player_kill()
    {
        int rdm_a =  UnityEngine.Random.Range(1, 11); int rdm_m =  UnityEngine.Random.Range(-11, -1);
        int rdm_e =  UnityEngine.Random.Range(1, 3);

        int rdm_ = rdm_e == 1 ? rdm_a : Math.Abs(rdm_m);

        if(plyr_flying && !plyr_sliding && !plyr_wallRninng && !plyr_railSliding && !plyr_jumping
            && !plyr_animKilling
        )
        {
            cm_movement.kill_am();
            _anim.SetInteger("killAm", rdm_);
            StartCoroutine(Dly_bool_anm(1f, "animKill"));

            plyr_rb.AddForce( new Vector3(0f, 
                (24f - (plyr_rb.velocity.y / 2) ),
            0f), ForceMode.VelocityChange);

            StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.2f));
        }
        if(plyr_jumping) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y * 1.5f, plyr_rb.velocity.z);
        // rig_animController.enabled = false;

        // kill_prtcl.Play();
        psCollisions_movement.player_paricleArray(null, true, "animKill"); 
    }

}

