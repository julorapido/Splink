//
//  PlayerMovement.cs
//  PlayerMovement
//
//  Created by Jules Sainthorant on 01/08/2023.
//  Copyright © 2023 Sainthorant Jules. All rights reserved.
//
using System.Collections;
using System.Collections.Generic;
// using System.Collections.Immutable;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
#if UNITY_EDITOR
    using UnityEditor.Animations;
#endif
using System;
using PathCreation.Utility;
using PathCreation;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Game State")]
    private bool gameOver_ = false;

    [Header ("Transforms & Rb")]
    [SerializeField] private GameObject plyr_;
    [SerializeField] private Rigidbody plyr_rb;
    [SerializeField] private Transform plyr_trsnfm;
    [SerializeField] private Transform plyr_cam;


    [Header ("Attached Scripts/Camera")]
    private CameraMovement cm_movement;
    private PlayerCollisions psCollisions_movement;
    private GameUI g_ui;
    private GameObject game_cam;


    [Header ("Combo System")]
    private float combo_timer = 0f, combo_reset = 1.5f;
    private int combo_ = 0;
    [HideInInspector] public int get_Combo{
        get{ return combo_;}
        set{ return; }
    }
    private int last_comboId_ = 0;

    [Header ("Momentums/Action/Ult")]
    [SerializeField] private float action_momentum = 0f;
    [SerializeField] private float momentum_ = 0f;
    [SerializeField] private float ultimate_ = 0f;
    [HideInInspector] public float get_Momentum {
        get{ return momentum_; }
        set{ return; }
    }


    [Header ("Movement Values")]
    private const float fall_sub = 16f;
    private const float jumpAmount = 27f;
    private const float strafe_speed = 0.225f;
    private const float player_speed = 4.25f;
    private const float tyro_speed = 28f;
    private float railSlide_speed = 0.5f; // 1f


    [Header ("Player Movements Status")]
    // [#0 fly]
    private bool plyr_flying = false;

    // [#1 jumps]
    private bool plyr_jumping = false, plyr_downDashing = false;

    // #2 [wallrun, slide, rampslide, saveclimb ...]
    private bool plyr_wallSliding = false, plyr_rampSliding = false, plyr_wallRninng = false, plyr_wallExiting = false;
    [HideInInspector] public bool plyr_sliding = false;

    // #3 [railSlide, tyro, saveclimbing, obstacles]
    [HideInInspector] public bool plyr_saveClimbing = false, plyr_tyro = false, plyr_railSliding = false;
    private bool plyr_obstclJmping = false, plyr_swnging = false, plyr_intro_tyro = false;
    private bool plyr_groundWalling = false;

    // #4 interact jumps [boxFall, hang, sideHang, rails, underslide, bareer, ladder...]
    [HideInInspector] public bool plyr_hanging = false;
    private bool plyr_underSliding = false, plyr_boxFalling = false, plyr_launchJumping = false, plyr_climbingLadder = false;
    private bool plyr_bareerJumping = false, plyr_tapTapJumping = false;

    // #5 [animkill, shooting & reloading & equiping]
    private bool plyr_shooting = false, plyr_animKilling = false, plyr_reloading = false, plyr_equiping = false;

    // #6 SPECIAL [IS_PLAYER_INTERACTING]
    private bool plyr_interacting = false;



    [Header ("Authorized Movements")]
    private bool jump_auth = true;
    private bool movement_auth = true;

    [Header ("Player Available Jumps")]
    private int jumpCnt = 2;
    private int dashDownCnt = 1;

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


    [Header ("TapTap, SideHang, Ladder, Fallbox, Bareer, SaveClimbing, WallRun, Obstacle && Hang Vars")]
    /////////////////////   vars     //////////////////////////
    ///// Hang
    private bool sHng_leftSide = false;
    private bool anim_bool_hang = false;
    ///// TapTap
    private bool tapTap_exited = false;
    private Vector3 taptap_V3;
    ///// Fallbox
    private int last_landBoxId = 0;
    private int last_jumpedOut_landBoxId = 0;
    ///// Bareer
    private bool horizontal_breerJmping = false;
    private bool lateral_breerJmping = false;
    ///// Sidehang
    private GameObject side_hang_obj = null;
    ///// Hang
    private GameObject hang_bar = null;
    ///// Obstacle
    private GameObject last_ObstJumped;
    private bool side_obst_side = false;
    private int obst_type = -1;
    ///// SaveClimbing
    private float rot_y_saveClimbing = 0f;
    ///// WallRun
    private GameObject wall_running_wall = null;
    private bool lft_Straf = false, rght_Straf = false;
    private Quaternion saved_wallRun_quatn;
    private Vector3 saved_wallRun_picoPos;
    ///// Slide
    private GameObject wallSlide_wall = null;
    private float slide_timer = 0f;
    ///// Ladder [&& Hang]
    private GameObject moving_camTraveler; // ladder



    [Header ("WEAPON - AIM - ENEMY ")]
    // -------- WEAPON --------
    [HideInInspector] public int ammo = 0;
    private bool weapon_equipped = false, weapon_twoHanded = false;
    [HideInInspector] public bool set_weaponHandedMode {
        set { if(value.GetType() == typeof(bool)) weapon_twoHanded = value; }  // set_weaponHandedMode
        get {return false; }
    }
    [HideInInspector] public float set_weaponReloadTime {
        set { if(value.GetType() == typeof(float)) weapon_reloadTime = value; }  // set_weaponReloadTime
        get {return 0f; }
    }
    [HideInInspector] public int set_weaponReloadType {
        set { if(value.GetType() == typeof(int)) weapon_reloadTime = value; }  // set_weaponReloadType
        get {return 0; }
    }
    private Vector3 aim_Recoil = new Vector3(0f, 0f, 0f);
    private Vector3 arm_Recoil = new Vector3(0f, 0f, 0f);
    private float weapon_reloadTime = 0f;
    private int weapon_reloadType = 0;
    private Weapon player_weaponScrpt;
    // -------- ENEMY --------
    [HideInInspector] public Transform aimed_enemy;
    private bool horizontal_enemy = false;
    private Transform lastAimed_enemy;
    private Vector3 saved_WorldPos_armTarget;
    // -------- AIM --------
    private bool aimSettingCalled = false;
    private bool shoot_blocked = false;


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


    [Header ("Player Animator, Rigs & Constraints")]
    [SerializeField] private Animator rig_animController;
    [SerializeField] private AvatarMask lower_body_mask;
    [SerializeField] private RigBuilder player_Rig;
    [SerializeField] private MultiAimConstraint[] player_aims;
    private float[] noAim_aimsWeigths = new float[4]{0.80f, 1.0f, 0.0f, 0.0f};
    private float[] autoAim_aimsWeigths = new float[4]{1.0f,  0.90f, 0f, 0.0f};
    [SerializeField] private Transform[] headAndNeck;
    private bool lastAimSettings_isArm;
    private string lastArmSetting_type;
    private bool wasLastAim_AutoAim = false;
    private bool y_r = false;

    [Header ("Character Animators")]
    [SerializeField] private GameObject pico_character;
    private Animator _anim;

    [Header ("Animation Center Fixes")]
    private Vector3[] colliders_references = new Vector3[10];


    [Header ("Lerp Saved Coroutines")]
    private Coroutine[] arm_routines = new Coroutine[4]{null, null, null, null};
    private Coroutine[] body_routines = new Coroutine[4]{null, null, null, null};


    [Header ("Authorized Shooting Animations")]
    // private string[] authorizedShooting_ = new string[4] { "pistolRun", "gunFly", "slide", "wallRun" };
    private const string authorizedShooting_s = "rifleShoot jumpFly groundLeave pistolShoot flyShoot slideWall slide wallRun railSlideShoot hang slideDown bump";
    private bool special_rampDelay = true;

    [Header ("Player Targets")]
    [SerializeField] private Transform body_TARGET;
    [SerializeField] private Transform head_TARGET;
    [SerializeField] private Transform leftArm_TARGET;


    [Header ("Application State")]
    private bool is_build = true;
    [HideInInspector] public bool set_build_mode {
        set { if(value.GetType() == typeof(bool)) is_build = value; } 
        get {return false; }
    }

    [Header ("TACTILE-INPUT (TOUCH)")]
    // MAX AND MIN FORCES VALUES
    private const float t_maxForce = 5f;
    private const float t_minForce = 0.75f;
    // touch positions
    private Vector3 f_tch, l_tch;
    private Vector3 stored_tch;
    // dead zones && vertical drag
    private float vertical_dragDistance = 7f, horizontal_dragDistance = 1f;  // min dist for vert drag to be registered [8% height of the screen]
    private float swipe_area_width = 0f;
    // can register jmp
    private bool can_registerJump = true;
    // touchs additionnals data
    private bool t_leftStrafe = false, t_rightStrafe = false;
    private bool t_stationary = false;
    private bool t_untouched = true;
    // tick && swipe power calculation
    private float t_calculated_swipePower = 0f;
    private float t_rotation_tick = 0f;
    private float rt_delay = 0.5f;
    private float rotate_speed = 0f;
    // stored rigidbody force
    private float stored_strafeForce = 0f;

    public bool is_free_flying {
        get{ return (plyr_flying && !plyr_interacting); }
        set{ return; }
    }



    [Header ("EDITOR fly boolean")]
    private bool FLYYY = false;



    // [Start] [Routine]
    private IEnumerator start_game()
    {
        cm_movement.start_jump(1);
        plyr_rb.AddForce(new Vector3(0f, 17f, 16f), ForceMode.VelocityChange);

        yield return new WaitForSeconds(1f);
        g_ui.ui_announcer("start_");
        cm_movement.start_jump(2);

        yield return new WaitForSeconds(1.1f);
        movement_auth = true;
    }



    // Awake
    private void Awake()
    {
        PlayerCollisions ar =  GameObject.FindGameObjectsWithTag("mainHitbox")[0].GetComponent<PlayerCollisions>();
        psCollisions_movement = ar;
        game_cam = GameObject.FindGameObjectsWithTag("MainCamera")[0];
        player_weaponScrpt = FindObjectOfType<Weapon>();
        g_ui = FindObjectOfType<GameUI>();
        _anim = GetComponentInChildren<Animator>();
        cm_movement = FindObjectOfType<CameraMovement>();

        movement_auth = false;

        vertical_dragDistance = (Screen.height / 100) * 12; // 12% of screen height
        swipe_area_width = (Screen.width / 100) * 60f; // 60% of screen width

        t_rotation_tick = (t_maxForce) / (swipe_area_width / 2);

        // var runtimeController = _anim.runtimeAnimatorController;
        // if(runtimeController == null)
        // {
        //     Debug.LogErrorFormat("RuntimeAnimatorController must not be null.");
        //     return;
        // }else{
        //     // Debug.Log(runtimeController);
        //     _anim_controller = runtimeController;
        // }
        //
        // AnimatorController _anim_controller = _anim.runtimeAnimatorController as AnimatorController;
        //
        // var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(runtimeController));
        // if(controller == null)
        // {
        //     Debug.LogErrorFormat("AnimatorController must not be null.");
        //     return;
        // }else
        // {
        //     _anim_controller = controller;
        //     // Debug.Log(controller);
        //     // Debug.Log(controller.layers);
        // }
    }



    // Start
    private void Start()
    {
        if (_anim == null)
            Debug.Log("nul animtor");

        // _anim.SetLayerWeight(1, 1f);

        Collider[] colList = gameObject.transform.GetChild(3).GetComponentsInChildren<Collider>();
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

        StartCoroutine(start_game());
    }



    // Update
    private void Update()
    {
        if(!gameOver_)
        {
            // ======================= JUMPS =======================
            // get jump input
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

                        StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.67f ) );

                        StartCoroutine(Dly_bool_anm(0.65f, "DoubleJump"));
                        cm_movement.jmp(true);
                        psCollisions_movement.player_paricleArray(null, true, "dblJump");
                    }

                    plyr_rb.AddForce( new Vector3(
                            0, 
                            (jumpCnt == 1 ? 
                                (jumpAmount * 1.1f) 
                                :
                                (plyr_flying ? jumpAmount : jumpAmount / 1.65f)
                            ),
                            0),
                        ForceMode.VelocityChange
                    );

                    jumpCnt--;
                }


                // DASH DOWN
                if(Input.GetKey("s") && (dashDownCnt > 0) )
                {
                    plyr_downDashing = true;

                    if(!(plyr_flying))
                    {
                        StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.7f ) );
                        StopCoroutine(Dly_bool_anm(0.7f, "dashDown"));
                        StartCoroutine(Dly_bool_anm(0.7f, "dashDown"));

                        action_momentum += 4f;
                    }else
                    {
                        StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.7f ) );
                        StopCoroutine(Dly_bool_anm(0.85f, "dashDown"));
                        StartCoroutine(Dly_bool_anm(0.85f, "dashDown"));

                        plyr_rb.AddForce( new Vector3(
                                0f, 
                                (jumpAmount * -1f), 
                                0f), 
                            ForceMode.VelocityChange
                        );
                    }

                    cm_movement.dash_down();
                    // dashDownCnt --;
                }
            }
            if ( Input.GetKeyDown(KeyCode.Space))
            {
                // CLIMB-UP
                if(plyr_saveClimbing)
                {
                    plyr_saveClimbing = false;
                    StopCoroutine(Dly_bool_anm(1.2f, "climb"));
                    StartCoroutine(Dly_bool_anm(0.8f, "climbOut"));
                    _anim.SetBool("climb", false);
                }
            }
            // =====================================================================


            
            // ======================= WEAPON EQUIP =======================
            if(Input.GetKeyDown("e"))
            {
                if(!plyr_equiping)
                {
                    playerEquip_weapon(!(weapon_equipped));
                }
            }
            // =====================================================================


            // ======================= TYRO/SLIDERAIL =======================
            if(plyr_tyro || plyr_railSliding)
            {
                last_tyro_trvld +=   (plyr_tyro ? tyro_speed : ( railSlide_speed )) * Time.deltaTime;

                Quaternion e = actual_path.path.GetRotationAtDistance(last_tyro_trvld + (plyr_railSliding ? 0f : 0.05f) );
                float e_y = (e.eulerAngles.y > 270f) ? (e.eulerAngles.y - 360f) : e.eulerAngles.y;

                if(tyro_handler_child != null)
                {
                    tyro_handler_child.position = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, 0.06f, 0);
                    // tyro_handler_child.rotation = new Quaternion(tyro_handler_child.rotation.x, e.y, tyro_handler_child.rotation.z, tyro_handler_child.rotation.w);
                    plyr_trsnfm.rotation = Quaternion.Euler(tyro_handler_child.rotation.eulerAngles.x, e_y, tyro_handler_child.rotation.eulerAngles.z);
                }

                plyr_trsnfm.position = (plyr_tyro ?
                    actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, -2.1f, -0.1f)
                        :
                    actual_path.path.GetPointAtDistance(last_tyro_trvld)+ new Vector3(0, 0.45f, -0.1f)
                );
                plyr_trsnfm.rotation = Quaternion.Euler(plyr_trsnfm.rotation.eulerAngles.x, e_y, plyr_trsnfm.rotation.eulerAngles.z);


                // stop
                float d_end = Vector3.Distance(plyr_trsnfm.position, end_tyroPos);
                if(d_end < 3f)
                {

                    plyr_rb.AddForce( new Vector3(0, 10f, 0), ForceMode.VelocityChange);
                    _anim.SetBool(plyr_tyro ? "tyro" : "slideRail", false);

                    if(plyr_tyro) plyr_tyro = false;
                    if(plyr_railSliding)
                    {
                        plyr_railSliding = false;
                        cm_movement.railSlide_offset(true, false);
                    }

                    // turn back on all movements
                    movement_auth = true;

                    // turn back gravity
                    plyr_rb.useGravity = true;

                    cm_movement.tyro_offset(true);

                    g_ui.set_countBonus_ = false;
                }
            }
            // =====================================================================



            // ------------------ HANG ------------------
            if(plyr_hanging)
            {
                transform.RotateAround(hang_bar.transform.position, Vector3.left, 52 * Time.deltaTime);
            }
            // ------------------------------------------


            // ------------------ MOMENTUM ------------------
            if(momentum_ > 0)
                momentum_ -= 0.5f * Time.deltaTime;
            else
                momentum_ = 0;
            // ------------------------------------------


            // ------------------ ACTION MOMENTUM ------------------
            if(action_momentum > 2f && action_momentum > 0)
                action_momentum -= 10f * Time.deltaTime;
            else if(action_momentum < 2f && action_momentum < 0)
                action_momentum += 10f * Time.deltaTime;
            else
                (action_momentum) = 0f;
            // ------------------------------------------------------


            // ------------------ COMBO ------------------
            if(combo_timer > 0f)
            {
                combo_timer -= 5 * Time.deltaTime;
            }
            else if (combo_timer < 0f)
            { 
                combo_timer = 0f; 
            }
            else
            { }

            if(combo_reset > 0f)
            {
                combo_reset -= Time.deltaTime;
            }
            // ---------------------------------------------

            if(rt_delay > 0f)
                rt_delay -= Time.deltaTime;

            if(plyr_sliding)
                slide_timer += Time.deltaTime;


            if((is_build) && true == false && 1 == 0)
            {
                if(movement_auth)
                {
                    if(!plyr_railSliding && !plyr_climbingLadder)
                    {
                        if( !plyr_saveClimbing && !plyr_boxFalling && !plyr_tapTapJumping)
                            if(!(t_untouched))
                            {
                                
                                if(!(t_stationary))
                                    plyr_.transform.Rotate(
                                        0, 
                                        (
                                            (150f)
                                            * 
                                            (t_rightStrafe ? 1f : -1f)
                                            *
                                            (Time.deltaTime)
                                        ),
                                        0,  Space.Self);
                                else
                                    plyr_.transform.rotation = (plyr_.transform.rotation);
                                
                            }
                    }
                    // CLAMP ROTATIONS
                    if(transform.rotation.eulerAngles.y < 180f && transform.rotation.eulerAngles.y > 60f)
                        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 60f, transform.rotation.eulerAngles.z);

                    if(transform.rotation.eulerAngles.y > 180f && transform.rotation.eulerAngles.y < 300f)
                        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 300f, transform.rotation.eulerAngles.z);
                }        
            }
        }
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

        // aim transition settings [for LeanTween]
        const float aim_time = 0.35f;

        if(armRig_ClipInfo == "railSlideShoot" || armRig_ClipInfo == "slide"
            || armRig_ClipInfo == "jumpOut"
        )
            rig_animController.enabled = false;

        if(!isAutoAim)
        {
            // clear target on CamerMovement.cs
            cm_movement.rst_aimedTarget();

            lastAimed_enemy = null;


            for(int m = 0; m < arm_aims.Length; m ++)
            {
                arm_aims[m].data.offset = new Vector3(0, 0, 0);
                arm_aims[m].data.maintainOffset = false;

                arm_aims[m].data.constrainedObject = arm_transforms[m];

                if(arm_routines[m] != null)
                    StopCoroutine(arm_routines[m]);

                arm_routines[m] = StartCoroutine( lerpAimWeight(arm_aims[m], 0f, aim_time) );
                // arm_aims[m].weight = 0f;

                arm_aims[m].data.aimAxis = all_axisS[0];
                arm_aims[m].data.upAxis = all_axisS[0];
            }
        }
        else
        {
            lastAimed_enemy = target;
            // rig_animController.enabled = false;

            // Adjust HAND for autoAim
            if(armRig_ClipInfo != "railSlideShoot")
            {
                arm_aims[3].data.constrainedObject = arm_transforms[3];
                arm_aims[3].data.offset = new Vector3(0, 0, -95f);
                arm_aims[3].weight = 1.0f;
                arm_aims[3].data.aimAxis = all_axisS[2]; // Y
                arm_aims[3].data.upAxis = all_axisS[4]; // Z
            }else{
                arm_aims[3].data.offset = new Vector3(-160f, 0, 0f);
                arm_aims[3].weight = 0f;
            }
        }


        for(int i = 0; i < player_aims.Length; i ++)
        {
            // body offset
            if(i == 0 && isAutoAim)
            {
                player_aims[i].data.offset = new Vector3(armRig_ClipInfo == "flyShoot" ? 30f : 14f, 0, 0);
            }

            // head offset
            if(i == 1 && isAutoAim)
            {
                player_aims[i].data.offset = armRig_ClipInfo == "railSlideShoot" ? new Vector3(25f, 15f, 20f): new Vector3(35f, 0, 0);
            }


            player_aims[i].weight = 0f;

            if(body_routines[i] != null)
               StopCoroutine(body_routines[i]);

            if(isAutoAim && armRig_ClipInfo == "railSlideShoot"
               && i == 0)
               player_aims[i].weight = 0.6f;
            else
               body_routines[i] = StartCoroutine(lerpAimWeight(player_aims[i], isAutoAim ? autoAim_aimsWeigths[i] : noAim_aimsWeigths[i], aim_time) );
            // player_aims[i].weight = isAutoAim ? autoAim_aimsWeigths[i] : noAim_aimsWeigths[i];
        }

        //if(plyr_railSliding) StartCoroutine(CheckAllArmRigAxies());

        if(toogleArmRig)
        {
            for(int m = 0; m < arm_aims.Length; m ++)
            {
                arm_aims[m].data.sourceObjects.Clear();

                arm_aims[m].data.constrainedObject = arm_transforms[m];
                arm_aims[m].weight = 0f;


                if(armRig_ClipInfo == "flyShoot" || armRig_ClipInfo == "rifleShoot" || armRig_ClipInfo == "pistolShoot"
                    || armRig_ClipInfo == "groundLeave" || armRig_ClipInfo == "jumpOut" || armRig_ClipInfo == "bump"
                    || armRig_ClipInfo == "jumpFly"
                )
                {
                    arm_aims[m].data.offset = new Vector3(0, 0, 0);

                    if(m == 0) // Special Shoulder Offset
                        arm_aims[m].data.offset = new Vector3(0, -40f, 0);
                    if(m == 1) // Special Arm Offset
                        arm_aims[m].data.offset = new Vector3(20f, -15f, 30f);

                    arm_aims[m].data.aimAxis = all_axisS[1]; // -X
                    arm_aims[m].data.upAxis = all_axisS[3]; // -Y

                }
                else if (armRig_ClipInfo == "slide" || armRig_ClipInfo == "slideDown")
                {
                    arm_aims[m].data.offset = adjustSlide_vectors[m];

                    arm_aims[m].data.aimAxis = armRig_ClipInfo == "slideDown" ?  all_axisS[2] : (all_axisS[1]); // Y or -X
                    arm_aims[m].data.upAxis = armRig_ClipInfo == "slideDown" ?  all_axisS[2] : all_axisS[4]; // Y or Z

                }
                else if ( armRig_ClipInfo == "railSlideShoot" )
                {
                    arm_aims[m].data.aimAxis = all_axisS[1]; // -X
                    arm_aims[m].data.upAxis = all_axisS[2]; // Y
                }


                if(arm_routines[m] != null)
                    StopCoroutine(arm_routines[m]);

                if(armRig_ClipInfo != "slide" && lastArmSetting_type != "slide")
                    arm_routines[m] = StartCoroutine( lerpAimWeight(arm_aims[m], 1.0f, aim_time) );
                else
                    arm_aims[m].weight = 1.0f;

            }

            // if(armRig_ClipInfo == "slide" || armRig_ClipInfo == "railSlide" || armRig_ClipInfo == "slideDown") player_aims[0].weight = 0f;
        }


        rig_animController.enabled = true;
        aimSettingCalled = false;
        player_Rig.Build();
    }

    IEnumerator lerpAimWeight(MultiAimConstraint aim_constraint,float lerp_To, float time)
    {
        float start = aim_constraint.weight;
        float end = lerp_To;
        float t = 0f;

        while(t < 1f)
        {
            yield return null;
            t += Time.deltaTime / time;
            aim_constraint.weight = Mathf.Lerp(start, end, t);
        }
        aim_constraint.weight = end;
    }




    private void FixedUpdate()
    {

        // ---------------------------------------------------------------------------------------------------------
        //                                              TACTILE INPUTS
        // ---------------------------------------------------------------------------------------------------------
        if (Input.touchCount > 0) // user is touching the screen with a single touch
        {
            Touch touch = Input.GetTouch(0); // get the touch
            
            // 1ST - TOUCH
            if (touch.phase == TouchPhase.Began)
            {
                f_tch = touch.position;
                l_tch = touch.position;
                stored_tch = touch.position;
                t_calculated_swipePower = 0f;

                t_untouched = false;
            }
    
            // STATIONARY - TOUCH
            if (touch.phase == TouchPhase.Stationary)
            {
                // if(!(t_stationary))
                //     t_lstStationary_wasLft = (t_leftStrafe) ? (1) : (0);
                t_stationary = true;
            }

            // MOVED - TOUCH
            if (touch.phase == TouchPhase.Moved) 
            {
                
                l_tch = touch.position;  // last touch position.
                t_stationary = false; // touch exists. (not stationary)

                // HORIZONTAL DRAG
                if(Mathf.Abs(l_tch.y - f_tch.y) < (vertical_dragDistance * 0.75f)) // < 1% screen height
                {
                    
                    Vector2 touchDelta = touch.deltaPosition;
                    rotate_speed = ( Mathf.Abs(touchDelta.x * 0.10f) > 4f) ? 
                        (4f) : (Mathf.Abs(touchDelta.x * 0.10f)
                    );


                    // left/right
                    if(touchDelta.x > 0)
                    {
                        t_rightStrafe = true;
                        t_leftStrafe = false;
                    }
                    else if (touchDelta.x < 0)
                    {
                        t_leftStrafe = true;
                        t_rightStrafe = false;
                    }
                    else if(touchDelta.x == 0)
                    {
                        t_stationary = true;
                        t_leftStrafe = false;
                        t_rightStrafe = false;
                    }


                    // Swipe-out [outside swipe area]
                    // Adjust DeadZone [f_tch]   
                    if(true == false)
                    {
                        if( (l_tch.x > (f_tch.x + (swipe_area_width / 2)))
                            || (l_tch.x < (f_tch.x - (swipe_area_width / 2)))
                        )
                        {
                            if(t_leftStrafe)
                            {
                                if(l_tch.x < (f_tch.x - (swipe_area_width / 2)))
                                    f_tch.x -= Math.Abs(l_tch.x - (f_tch.x - (swipe_area_width / 2)));
                            }
                            if(t_rightStrafe)
                            {
                                if(l_tch.x > (f_tch.x + (swipe_area_width / 2)))
                                    f_tch.x += l_tch.x - (f_tch.x + (swipe_area_width / 2));
                            }
                        }
                    }

                    // assign ratios
                    // clamp (> t_minForce < t_maxForce)
                    if(Mathf.Abs(f_tch.x - l_tch.x) * (t_rotation_tick) > (t_maxForce))
                    {
                        t_calculated_swipePower = ((l_tch.x - f_tch.x) > 0) ? (t_maxForce) : (-t_maxForce);
                    }else
                    {
                        t_calculated_swipePower = (Mathf.Abs(f_tch.x - l_tch.x) * (t_rotation_tick) < (t_minForce) ?
                            ((l_tch.x - f_tch.x) > 0) ? (t_minForce) : (-t_minForce)
                            :
                            ((l_tch.x - f_tch.x) * t_rotation_tick)
                        );
                    }
                    
                }



                // VERTICAL DRAG
                if(Mathf.Abs(l_tch.y - f_tch.y) > (vertical_dragDistance) && (can_registerJump))
                {
                    can_registerJump = false;

                    // Up Swipe
                    if (l_tch.y > f_tch.y)
                    {
                        if(movement_auth && jump_auth)
                        {
                            if ((jumpCnt > 0) )
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

                                    StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.67f ) );

                                    StartCoroutine(Dly_bool_anm(0.65f, "DoubleJump"));
                                    cm_movement.jmp(true);
                                    psCollisions_movement.player_paricleArray(null, true, "dblJump");
                                }

                                plyr_rb.AddForce( new Vector3(
                                        0, 
                                        (jumpCnt == 1 ? 
                                            (jumpAmount * 1.1f) 
                                            :
                                            (plyr_flying ? jumpAmount : jumpAmount / 1.65f)
                                        ),
                                        0),
                                    ForceMode.VelocityChange
                                );

                                jumpCnt--;
                            }
                        }

                        // CLIMB-UP
                        if(plyr_saveClimbing)
                        {

                            plyr_saveClimbing = false;
                            StopCoroutine(Dly_bool_anm(1.2f, "climb"));
                            movement_auth = true;
                            plyr_rb.useGravity = true;
                            plyr_rb.AddForce( new Vector3(0f, 4f, -4f), ForceMode.VelocityChange);
                        }
                    }
                    // Down swipe
                    else
                    {   
                        if(movement_auth && jump_auth)
                        {
                            // DASH DOWN
                            if( (dashDownCnt > 0) )
                            {
                                StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.5f ) );

                                plyr_downDashing = true;
                                StopCoroutine(Dly_bool_anm(0.44f, "dashDown"));
                                StartCoroutine(Dly_bool_anm(0.44f, "dashDown"));
                                cm_movement.dash_down();

                                plyr_rb.AddForce( new Vector3(
                                        0f, 
                                        (jumpAmount * -1.2f), 
                                        0f), 
                                    ForceMode.VelocityChange
                                );

                                //dashDownCnt --;
                            }
                        }
                    }
                }

                // if(Math.Abs(stored_tch.x - touch.position.x) > 5)
                stored_tch = touch.position;
            }
            
            if (touch.phase == TouchPhase.Ended)
            {
                l_tch = touch.position;
            }
        }
        else
        {
            can_registerJump = true;
            t_untouched = true;

            t_leftStrafe = false;
            t_rightStrafe = false;
            t_stationary = false;

            t_calculated_swipePower = 0f;
            
            if(!plyr_tyro && !plyr_intro_tyro && !plyr_wallRninng && !plyr_railSliding &&
                !plyr_hanging && !plyr_saveClimbing && !plyr_bareerJumping && !plyr_climbingLadder
                && !plyr_boxFalling
            ){
                rotate_bck();

                // MOVE INSIDE FIXED-UPDATE() <---
                // if(rt_auth)
                // {
                //     if(plyr_trsnfm.rotation.eulerAngles.y >= 0.1f || plyr_trsnfm.rotation.eulerAngles.y <= -0.1f)
                //     {
                //         transform.localRotation = Quaternion.Slerp(plyr_trsnfm.rotation, new Quaternion(0,0,0,1), 3.0f * Time.deltaTime);
                //     }
                // }
            }
            
        }
        // ---------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------




        // ammo attribution
        ammo = player_weaponScrpt.get_ammoInMag;


        // anim is_enemy_aimed
        if((aimed_enemy != null) && (ammo > 0) && !(plyr_reloading) && (weapon_equipped))
        {
            if(!_anim.GetBool("is_enemy_aimed"))
            {
                _anim.SetBool("is_enemy_aimed", true);
            }
        }else{
            if(_anim.GetBool("is_enemy_aimed"))
            {
                _anim.SetBool("is_enemy_aimed", false);
            }
        }


        // anim Y_ROT
        if(y_r != (transform.rotation.eulerAngles.y > 180f))
        {
            AnimatorClipInfo[] m_CurrentClipInfo = _anim.GetCurrentAnimatorClipInfo(0);
            if(m_CurrentClipInfo.Length > 0)
            {
                string s = m_CurrentClipInfo[0].clip.name;
                if((s != "jumpOut") && (s != "groundLeave") && (s != "bareerSide") && (s != "bareer"))
                {
                    _anim.SetBool("Y_ROT", (transform.rotation.eulerAngles.y > 180f));
                    y_r = (transform.rotation.eulerAngles.y > 180f);   
                }
            }
       
        }



         
        // ---------------------------------------------------------------------------------------------------------
        //                                              Player_interacting
        // ---------------------------------------------------------------------------------------------------------
        plyr_interacting = ( 
            // #0 flying
                                      (plyr_flying)
                                            &&
            // #1 jumps
                         (   ((plyr_jumping) || (plyr_downDashing))
                                            ||
            // #2 slide, wallRun, rail, ramps
                ((plyr_rampSliding) || (plyr_wallRninng) || (plyr_sliding) || (plyr_railSliding) || (plyr_wallSliding)) 
                                            ||
            // #3 bareer, launcher, box, tyro, saveclimbing, obstacle, 
                (plyr_saveClimbing || plyr_tyro || plyr_intro_tyro  || plyr_hanging || 
                        plyr_obstclJmping ||  plyr_climbingLadder || plyr_underSliding || 
                plyr_boxFalling || plyr_launchJumping || plyr_bareerJumping || plyr_tapTapJumping)
            )
        );
        // Debug.Log(plyr_interacting + " = " 
        //             +
        //     (((plyr_rampSliding) || (plyr_wallRninng) || (plyr_sliding) || (plyr_railSliding)) )
        //             + "or " +
        //     ((plyr_saveClimbing || plyr_tyro || plyr_intro_tyro  || plyr_hanging || 
        //         plyr_obstclJmping ||  plyr_climbingLadder || plyr_underSliding || 
        //     plyr_boxFalling || plyr_launchJumping || plyr_bareerJumping || || plyr_tapTapJumping))
        //             +  " or " +
        //     ((plyr_jumping) || (plyr_downDashing))
        // );
        // Debug.Log(
        //     plyr_saveClimbing + " or " +  plyr_tyro + " or " + plyr_intro_tyro + " or " + plyr_hanging + 
        //     " or " + plyr_obstclJmping + " or " + plyr_climbingLadder + " or " + plyr_underSliding + " or "
        //     + plyr_boxFalling + " or " + plyr_launchJumping + " or " + plyr_bareerJumping + " or " + plyr_bareerJumping + 
        //     " or " + + " or " + plyr_tapTapJumping 
        // );
        // ---------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------


        
        // ---------------------------------------------------------------------------------------------------------
        //                                          Player_AirShootMode
        // ---------------------------------------------------------------------------------------------------------
        if( (_anim.GetBool("interacting")) != (plyr_interacting) )
        {
            _anim.SetBool("interacting", plyr_interacting);
            // _anim.SetInteger("airShootMode", UnityEngine.Random.Range(0, 6));
            // Debug.Log("set air shoot mode!");
            _anim.SetInteger("airShootMode", 3);
        }


  

        // ---------------------------------------------------------------------------------------------------------
        //                                                 Recoil
        // ---------------------------------------------------------------------------------------------------------
        aim_Recoil = (player_weaponScrpt.get_WeaponRecoil);
        arm_Recoil = (player_weaponScrpt.get_ArmRecoil);

        leftArm_TARGET.position = (
            (saved_WorldPos_armTarget) 
            + ((plyr_railSliding) ? new Vector3(-3.1f, 0f, 0f) : Vector3.zero) 
            + (aim_Recoil)
        );

        if( (aimed_enemy != null && ammo > 0))
        {
            player_aims[0].data.offset = new Vector3(lastArmSetting_type == "flyShoot" ? 30f : 14f, 0f, 0f) + (arm_Recoil / 2);

            for(int m = 0; (m < arm_aims.Length) && (m < 2); m ++)
            {
                Vector3 v3 = Vector3.zero;

                if(lastArmSetting_type == "flyShoot" || lastArmSetting_type == "rifleShoot" || lastArmSetting_type == "pistolShoot"
                    || lastArmSetting_type == "groundLeave" || lastArmSetting_type == "jumpOut" || lastArmSetting_type == "bump"
                    || lastArmSetting_type == "jumpFly"
                ){
                    // Special Shoulder & Arm Offset
                    v3 = (m == 0) ? new Vector3(0, -40f, 0) : new Vector3(20f, -15f, 30f);
                }
                else if (lastArmSetting_type == "slide" || lastArmSetting_type == "slideDown")
                    v3 = adjustSlide_vectors[m];
                else if ( lastArmSetting_type == "railSlideShoot" )
                    v3 = new Vector3(0, 0, 0);
                
                arm_aims[m].data.offset = (v3) + (arm_Recoil);
            }
        }
        // ---------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------


        if (!gameOver_ && !plyr_tyro)
        {


            // ---------------------------------------------------------------------------------------------------------
            //                                              Nothing-To-Aim
            // ---------------------------------------------------------------------------------------------------------
            if ( ((aimed_enemy == null && ammo > 0) || ammo == 0 || (plyr_reloading)) && (weapon_equipped))
            {
                if(lastAimed_enemy != null) setAimSettings(false);


                if(_anim.GetCurrentAnimatorClipInfo(0).Length > 0)
                {
                    if( (rig_animController.enabled) )
                    {
                        rig_animController.enabled = false;
                    }
                }


                // reset pico y previous rotations
                if(!plyr_wallRninng && !plyr_hanging && !plyr_rampSliding && !plyr_bareerJumping && !plyr_climbingLadder)
                {
                    pico_character.transform.localRotation = Quaternion.Euler(
                        pico_character.transform.localRotation.eulerAngles.x, 0f, pico_character.transform.localRotation.eulerAngles.z
                    );
                }
            }
            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------



            // ---------------------------------------------------------------------------------------------------------
            //                                              Auto-aim
            // ---------------------------------------------------------------------------------------------------------
            bool canShot = false;
            if ( (aimed_enemy != null && ammo > 0) && (!plyr_reloading) && (weapon_equipped) && (!plyr_equiping))
            {
                if(_anim.GetCurrentAnimatorClipInfo(0).Length > 0)
                {
                    //Debug.Log("allo salem?");
                    string animClip_info = _anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                    bool armRigMaybe = ((animClip_info == "slide") || (animClip_info == "flyShoot")
                        || (animClip_info == "railSlideShoot") ||  (animClip_info == "slideDown")
                        || (animClip_info == "pistolShoot") || (animClip_info == "rifleShoot")
                        || (animClip_info == "groundLeave") || (animClip_info == "jumpOut")
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
                            setAimSettings(true, aimed_enemy, armRigMaybe, animClip_info);
                            lastArmSetting_type = animClip_info;
                            // aim_Recoil = Vector3.zero;
                            canShot = true;
                            // break;
                        }

                        // Enemy aimed but can't auto-aim !
                        if(!canShot)
                        {
                            // prevent non-autoAim be called too much
                            if(wasLastAim_AutoAim == true)
                            {
                                setAimSettings(false);
                                rig_animController.enabled = false;
                            }

                        }
                    }
                }
            }
            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------



            // ---------------------------------------------------------------------------------------------------------
            //                                              TARGET Adjustments
            // ---------------------------------------------------------------------------------------------------------
            //
            // Aim Shooting [Flying & Sliding & RampSliding]
            if( (aimed_enemy != null) && (ammo > 0) && (plyr_flying || plyr_sliding || plyr_rampSliding)
                    &&  (plyr_shooting) )
            {
                //  FLYING
                if((plyr_flying) && !(plyr_sliding || plyr_rampSliding) && true == false)
                {
                    // horizontal turret
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

                    }
                    // vertical enemy
                    else
                    {
                        float dst = Vector3.Distance(transform.position, aimed_enemy.position);
                        head_TARGET.position = new Vector3(
                            aimed_enemy.position.x  + (dst < 12 ? +2.5f : +7f),
                            aimed_enemy.position.y + 1f,
                            aimed_enemy.position.z + ((aimed_enemy.position.x - transform.position.x > 0 ) ? 1f : -1f)
                        );

                        body_TARGET.position = new Vector3(
                            aimed_enemy.position.x + 1f, aimed_enemy.position.y + 1.5f, aimed_enemy.position.z + 4f
                        );
                    }



                    // rotate model for aim [flying]
                    if(!plyr_wallRninng && !plyr_railSliding && !plyr_hanging && !plyr_rampSliding && !plyr_bareerJumping
                        && (plyr_flying) && !plyr_tapTapJumping && !plyr_climbingLadder
                    )
                    {
                        Vector3 relativePos = aimed_enemy.position - transform.position;
                        Quaternion y_aim = Quaternion.LookRotation(relativePos);
                        float y_fl = ((y_aim.eulerAngles.y > 180f) ? (y_aim.eulerAngles.y - 360f) : (y_aim.eulerAngles.y)) * 0.8f;

                        pico_character.transform.localRotation = Quaternion.Slerp(
                            pico_character.transform.localRotation, Quaternion.Euler(
                                0f,
                                //45f,
                                Math.Abs(y_fl) > 40f ?
                                   (y_fl > 0 ? 40f : -40f) : y_fl,
                                0f
                            ),
                            0.15f
                        );
                        
                    }
                }
                // SLIDING && RAMPSLIDING
                else
                {
                    if(!plyr_rampSliding)
                        pico_character.transform.localRotation = Quaternion.Slerp(
                            pico_character.transform.localRotation,
                            Quaternion.identity,
                            0.15f
                        );


                    if(plyr_sliding)
                    {
                        body_TARGET.position = transform.position + new Vector3(3f, -3f , 3f);
                    }
                    head_TARGET.position = aimed_enemy.position + (plyr_rampSliding ? new Vector3(6f, -8f, 0f) : new Vector3(0, -1f, 0) );
                }

            }
            // Aim Shooting [Running]
            if( (aimed_enemy != null) && (ammo > 0)  && (!plyr_flying && !plyr_sliding) )
            {
                float dst = Vector3.Distance(transform.position, aimed_enemy.position);
                if(dst < 12)
                {
                    body_TARGET.position = aimed_enemy.position  + new Vector3(0f, 0.5f, 0f);
                    head_TARGET.position = aimed_enemy.position  + new Vector3(horizontal_enemy ? 0.9f : -3f, 0f, 0f);
                }else
                {
                    Vector3 running_v3Pos = aimed_enemy.position + (
                        horizontal_enemy ?
                        new Vector3(1.9f, 0, -1f) : new Vector3(0f, -2f, -1f)
                    );

                    body_TARGET.position = running_v3Pos;
                    head_TARGET.position = running_v3Pos + new Vector3(-4f, -2f, 0f);
                }

                Vector3 relativePos = aimed_enemy.position - transform.position;
                Quaternion y_aim = Quaternion.LookRotation(relativePos);
                float y_fl = ((y_aim.eulerAngles.y > 180f) ? (360f - y_aim.eulerAngles.y) : (y_aim.eulerAngles.y)) * 1.2f;

                pico_character.transform.localRotation = Quaternion.Slerp(
                    pico_character.transform.localRotation, Quaternion.Euler(
                        0f,
                        Math.Abs(y_fl) > 30f ?
                            (y_fl > 0 ? 30f : -30f) : y_fl,
                        0f
                    ),
                    0.15f
                );
            }

            // Aim Shooting [RailSliding]
            if( (aimed_enemy != null) && (ammo > 0) && (plyr_railSliding))
            {
                head_TARGET.position = aimed_enemy.position + (
                    new Vector3(12.5f, -3.5f, 0f)
                );
            }
            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------

            // lol
            if(aimed_enemy != null)
                body_TARGET.position = head_TARGET.position = aimed_enemy.position;



            // ---------------------------------------------------------------------------------------------------------
            //                                             RT_AUTH
            // ---------------------------------------------------------------------------------------------------------
            // rotate back [Quaternion Slerp]
            if( (!is_build) || ( (is_build) && (t_stationary == false)))
            {
                if(!plyr_tyro && !plyr_intro_tyro && !plyr_wallRninng && !plyr_railSliding &&
                    !plyr_hanging && !plyr_saveClimbing && !plyr_bareerJumping && !plyr_climbingLadder
                    && !plyr_boxFalling
                ) 
                {
                    if (!Input.GetKey("q") && !Input.GetKey("d")) 
                        rotate_bck();
                    if(Input.GetKey("q") || Input.GetKey("d")) 
                        rt_auth = false;

                    if(rt_auth)
                    {          
                        if((is_build && rt_delay <= 0f && !plyr_jumping && !plyr_downDashing) || (!is_build))
                        {          
                            if(plyr_trsnfm.rotation.eulerAngles.y >= 0.1f || plyr_trsnfm.rotation.eulerAngles.y <= -0.1f)
                            {
                                transform.localRotation = Quaternion.Slerp(
                                        plyr_trsnfm.rotation, 
                                        new Quaternion(0,0,0,1), 
                                        (is_build ? 2.65f : 3.0f) * Time.deltaTime
                                );
                            }
                        }
                    }else
                        rt_delay = 0.3f;
                }else
                    rt_delay = 0.3f;
            }
            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------






            // ---------------------------------------------------------------------------------------------------------
            //                                             MOVEMENT AUTH
            // ---------------------------------------------------------------------------------------------------------
            // movement auth disabled for [ObstacleHit, Swinging, Tyro, GrappleJump]
            if (movement_auth)
            {
                
                // MAIN MOVEMENT SPEED
                // disabled for [slideRail & ladder]
                if(!plyr_railSliding && !plyr_climbingLadder)
                {

                    // [SLIDING SPEED]
                    if(plyr_sliding)
                    {
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x,
                            plyr_rb.velocity.y,
                            ((player_speed + momentum_ + action_momentum) * 1.35f)
                                                *
                                       (plyr_shooting ? 0.7f : 1f)
                        );
                    }

                    // [WALL SLIDE]
                    else if(plyr_wallSliding)
                    {
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, 
                            0.57f,
                            ((player_speed + momentum_ + action_momentum) * 1.5f)
                                            *
                                     (plyr_shooting ? 0.65f : 1f)
                        );
                    }

                    // [BAREER (horizontal)]
                    else if(plyr_bareerJumping && (horizontal_breerJmping))
                    { 
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, 
                            (plyr_rb.velocity.y < -7.5f) ? 
                                (-7.5f) : (plyr_rb.velocity.y)
                            ,
                            ((player_speed + momentum_ + action_momentum) * 0.50f)
                        );
                    }

                    // [BAREER (lateral)]
                    else if (plyr_bareerJumping && (lateral_breerJmping))
                    {
                        plyr_rb.velocity = new Vector3(
                            (plyr_rb.velocity.x),
                            (plyr_rb.velocity.y < -4f) ? (-4f) : (plyr_rb.velocity.y),
                            ((player_speed + momentum_ + action_momentum) * 0.42f)
                        );
                    }

                    // [WALL RUN]
                    else if(plyr_wallRninng && !plyr_wallSliding)
                    {
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, 
                            plyr_obstclJmping ? plyr_rb.velocity.y : 2.6f,
                            ((player_speed + momentum_ + action_momentum) * 1.15f)
                                            *
                                    (plyr_shooting ? 0.65f : 1f)
                        );
                    }


                    // [RAMP SLIDING]
                    else if(plyr_rampSliding)
                    {
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x,
                            plyr_rb.velocity.y,
                            ((player_speed + momentum_ + action_momentum) * 1.30f)
                                                    *
                                    (plyr_shooting ? 0.8f : 1f)
                        );
                    }

                    // [TAP TAP JUMPNG]
                    else if(plyr_tapTapJumping)
                    { 
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, 
                            tapTap_exited ? 
                                (plyr_rb.velocity.y) : (1f),
                            (player_speed * 0.6f) 
                        );
                    }


                    // [RUNNING & FLYING]
                    else
                    {
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, 
                            plyr_rb.velocity.y, 
                            ((player_speed + momentum_ + action_momentum) * 1f)
                                                *
                                        (plyr_shooting || plyr_animKilling ? 0.65f : 1f)
                        );

                        if(Input.GetKeyDown("f"))
                            FLYYY = !(FLYYY);
                        if(FLYYY)
                            plyr_rb.velocity = new Vector3(
                                0f, plyr_rb.velocity.y < 0.50f ? 0.50f : plyr_rb.velocity.y, 0f
                            );
                    }



                    // -----------------------------------------------
                    //                     INPUTS
                    // -----------------------------------------------
                    if( !plyr_saveClimbing && !plyr_boxFalling && !plyr_tapTapJumping)
                    {
                        float max_leftX = (plyr_shooting ? -0.85f : -1.1f) * (player_speed + momentum_ + action_momentum);
                        float max_rightX = (plyr_shooting ? 0.85f : 1.1f) * (player_speed + momentum_ + action_momentum);

                        // [PC, LAPTOP]
                        if(!(is_build))
                        {
                            // LEFT
                            if ( Input.GetKey("q") )
                            {
                                // rt   
                                if((plyr_trsnfm.rotation.eulerAngles.y >= 307f)
                                    ||
                                    (plyr_trsnfm.rotation.eulerAngles.y <= 180f)
                                ){
                                    plyr_.transform.Rotate(
                                        0, 
                                        (plyr_shooting) ? (-2f) : (-3.10f), 
                                        0, 
                                    Space.Self);
                                }
                                

                                // STRAFE FORCES
                                // addforce    
                                if(plyr_rb.velocity.x > ((plyr_sliding) ? (max_leftX * 0.4f) : (max_leftX)))
                                {
                                    plyr_rb.AddForce(
                                        (-4f) * (Vector3.right * strafe_speed), 
                                        ForceMode.VelocityChange
                                    );
                                }
                                else{
                                    plyr_rb.velocity = new Vector3(max_leftX, plyr_rb.velocity.y, plyr_rb.velocity.z);
                                }
                            }

                            // RIGHT
                            if ( Input.GetKey("d"))
                            {
                                // rt
                                if((plyr_trsnfm.rotation.eulerAngles.y >= 180f)
                                    ||
                                    (plyr_trsnfm.rotation.eulerAngles.y <= 53f)
                                ){
                                    plyr_.transform.Rotate(
                                        0, 
                                        (plyr_shooting) ? (2f) : (3.10f), 
                                        0,
                                    Space.Self);
                                }
                            

                                // STRAFE FORCES
                                // addforce
                                if(plyr_rb.velocity.x < ((plyr_sliding) ? (max_rightX * 0.4f) : (max_rightX)))
                                {
                                    plyr_rb.AddForce(
                                        (4f) * (Vector3.right * strafe_speed), 
                                        ForceMode.VelocityChange
                                    );
                                }
                                else{
                                    plyr_rb.velocity = new Vector3(max_rightX, plyr_rb.velocity.y, plyr_rb.velocity.z);
                                }    
                            }
                        }
                        // [MOBILE, TABLET]
                        if((is_build))
                        {
                            if(!(t_untouched))
                            {

                                // rt
                                if((t_stationary))
                                {
                                    transform.rotation = transform.rotation;
                                    // if(t_stationaryDelay > 0f)
                                    //     plyr_.transform.Rotate(0, (t_rtForce), 0, Space.Self);
                                    // else
                                    // {
                                    //     //rotate_bck();
                                    //     t_stationary = false;
                                    // }
                                }else
                                {
                                    // plyr_.transform.rotation = Quaternion.Euler(
                                    //     0f,
                                    //     (t_calculated_swipePower < 0f) ? 
                                    //         (359.9f + (t_calculated_swipePower * 10f))
                                    //          :  
                                    //         (t_calculated_swipePower * 10f),
                                    //     0f
                                    // );
                                    plyr_.transform.Rotate(
                                        0, 
                                        (
                                            (3f) /* (rotate_speed) || Mathf.Abs(t_calculated_swipePower) */
                                            * 
                                            (t_rightStrafe ? 1f : -1f)
                                        ),
                                        0, 
                                    Space.Self); 
                                }
                                
                    

                                // addforce
                                float max_force = (max_rightX) * 1.05f;
                                if(t_stationary)
                                {
                                    // freeze it
                                    plyr_rb.velocity = new Vector3(
                                        stored_strafeForce, 
                                        plyr_rb.velocity.y, 
                                        plyr_rb.velocity.z
                                    );
                                }
                                else
                                {
                                    if(Math.Abs(plyr_rb.velocity.x) < (max_force))
                                    {
                                        plyr_rb.AddForce(
                                            ((3f)  /* (rotate_speed) || Mathf.Abs(t_calculated_swipePower) */ 
                                            * 
                                            (t_rightStrafe ? 1f : -1f)) 
                                            * 
                                            (Vector3.right * strafe_speed), 
                                            ForceMode.VelocityChange
                                        );
        
                                    }
                                    else
                                    {
                                        plyr_rb.velocity = new Vector3(
                                            plyr_rb.velocity.x > 0f ? (max_force) : (-max_force), 
                                            plyr_rb.velocity.y, 
                                            plyr_rb.velocity.z
                                        );
                                    } 
                                    stored_strafeForce = (plyr_rb.velocity).x;
                                }
                            }
                        }


                        // CLAMP ROTATIONS
                        if(!(is_build) || (is_build))
                        {
                            if(transform.rotation.eulerAngles.y < 180f && transform.rotation.eulerAngles.y >= 53f)
                                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 53f, transform.rotation.eulerAngles.z);

                            if(transform.rotation.eulerAngles.y > 180f && transform.rotation.eulerAngles.y < 307f)
                                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 307f, transform.rotation.eulerAngles.z);
                        }
                    }



                    // - - - - - - -  - - - - -
                    // [WALL RUN] STRAFES
                    // - - - - - - -  - - - - - 
                    if(lft_Straf || rght_Straf)
                    {
                        if(lft_Straf)
                        {
                            plyr_rb.velocity = new Vector3(
                                plyr_rb.velocity.x > 0 ? 0 : plyr_rb.velocity.x, plyr_rb.velocity.y, plyr_rb.velocity.z
                            );
                        }

                        if(rght_Straf)
                        {
                            plyr_rb.velocity = new Vector3(
                                plyr_rb.velocity.x < 0 ? 0 : plyr_rb.velocity.x, plyr_rb.velocity.y, plyr_rb.velocity.z
                            );
                        }
                    }

                    // - - - - - - -  - - - - -
                    // SHOOTING Y-AXIS VelocityFix
                    // - - - - - - -  - - - - - 
                    if( (plyr_shooting && plyr_flying)  
                                && 
                            (ammo > 0)
                                && 
                        (!plyr_wallExiting)
                                &&
                        (!plyr_downDashing)
                    )
                    {
                        if(aimed_enemy != null)
                        {
                            // if(plyr_rb.velocity.y < -10f)
                            //     plyr_rb.velocity =  new Vector3(plyr_rb.velocity.x, -10f, plyr_rb.velocity.z);
                        }
                    }


                    // - - - - - - -  - - - - -
                    // DOWN DASHING Y-AXIS VelocityFix
                    // - - - - - - -  - - - - - 
                    if(plyr_downDashing)
                    {
                        if(plyr_rb.velocity.y < -16f)
                            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, -16f, plyr_rb.velocity.z);
                    }

                    if(plyr_wallRninng)
                        transform.rotation = Quaternion.identity;
                }


                // -- RAILSLIDE Cancel --
                if ( (Input.GetKey("q") || Input.GetKey("d") || t_leftStrafe || t_rightStrafe) 
                    && (plyr_railSliding)
                ){
                    if(Input.GetKey("q") || (t_leftStrafe))
                        cm_movement.jumpOut(-1f);
                    else
                        cm_movement.jumpOut(1f);

                    plyr_railSliding = false;
                    plyr_rb.AddForce(( 
                            ((Input.GetKey("q") || t_leftStrafe) ? 80 : -80) * (Vector3.left * strafe_speed) 
                        ),
                        ForceMode.VelocityChange
                    );
                    plyr_rb.AddForce( new Vector3(0, 16f, -16f), ForceMode.VelocityChange );

                    jump_auth = true;
                    _anim.SetBool("slideRail", false);
                    plyr_rb.useGravity = true;
                    plyr_railSliding = false;


                    cm_movement.railSlide_offset(true, false);
                    slideRail_ref = null;
                }


                // -- LADDER --
                if(plyr_climbingLadder)
                {
                    if(plyr_rb.velocity.y < 0.6f) 
                        plyr_rb.velocity =  new Vector3(plyr_rb.velocity.x, 0.6f, plyr_rb.velocity.z);

                    // ladder cancel
                    if ((Input.GetKey("q") || Input.GetKey("d")))
                    {
                        plyr_rb.AddForce(  (Input.GetKey("q") ? 20f : -20f) * (Vector3.left * strafe_speed) , ForceMode.VelocityChange);
                        plyr_rb.AddForce(new Vector3(0f, 0f, -20f), ForceMode.VelocityChange);
                    }
                }


 
            }
            // ----------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------





            // ---------------------------------------------------------------------------------------------------------
            //                                           OUTSIDE (MOVEMENT_AUTH)
            // ---------------------------------------------------------------------------------------------------------
            // --- SIDEHANG ---
            if ((Input.GetKey("q") || Input.GetKey("d")) 
                && _anim.GetBool("sideHang"))
            {
                _anim.SetBool("sideHang", false);
                LeanTween.rotateLocal(pico_character, new Vector3(0, 0, 0), 1f).setEaseInSine();
                plyr_rb.AddForce(new Vector3(0, 10f, 0f), ForceMode.VelocityChange);
                movement_auth = true;
                plyr_rb.useGravity = true;
                StartCoroutine(Dly_bool_anm(0.85f, "specialHANG"));
            }
            // --- OBSTACLE JMUPING ---
            if(plyr_obstclJmping)
            {
                if(plyr_rb.velocity.y < 0.65f)
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0.65f, plyr_rb.velocity.z);
            }
            // --- FALL_DOWN ---
            if(plyr_rb.velocity.y < -13f && (!_anim.GetBool("falling")) )
            {
                _anim.SetBool("falling", true);
                // cm_movement.fall_Down(false);
            }
            if (plyr_rb.velocity.y > -13f && _anim.GetBool("falling") )
            {
                _anim.SetBool("falling", false);
                // cm_movement.fall_Down(true);
            }
            // --- SAVE-CLIMBING climbing ---
            if(plyr_saveClimbing)
            {
                transform.rotation = Quaternion.Euler(0f, rot_y_saveClimbing, 0f);
            }
            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------


            
            // ---------------------------------------------------------------------------------------------------------
            //                                             PLAYER SHOOTING
            // ---------------------------------------------------------------------------------------------------------
            if ( (!plyr_obstclJmping && !plyr_jumping && !plyr_animKilling && !plyr_boxFalling && !plyr_launchJumping)
                                &&
                    (ammo > 0) && (aimed_enemy != null) 
                                &&
                        !(plyr_reloading)
                                &&
                        (weapon_equipped) 
                                &&
                        (movement_auth)
                )
            {
                _anim.SetBool("shooting", true);
                if( !(shoot_blocked) )
                {
                    player_weaponScrpt.Shoot(aimed_enemy);
                    plyr_shooting = true;
                }
            }else
            {
                plyr_shooting = false;
                _anim.SetBool("shooting", false);
            }
            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------

    


            // Swinging Forces
            // if(!movement_auth && plyr_swnging)
            // {
            //     plyr_trsnfm.rotation = Quaternion.Euler(plyr_trsnfm.rotation.eulerAngles.x, plyr_trsnfm.rotation.eulerAngles.y, grpl_sns ? -10 : 10);

            //     // swing strafe
            //     if (Input.GetKey("q"))  plyr_rb.AddForce((2.6f * (Vector3.left * strafe_speed)), ForceMode.VelocityChange); pico_character.transform.Rotate(0.4f, 0, 0, Space.Self);
            //     if (Input.GetKey("d"))  plyr_rb.AddForce((2.6f * (Vector3.right * strafe_speed)), ForceMode.VelocityChange); pico_character.transform.Rotate(-0.4f, 0, 0, Space.Self);

            //     if (Input.GetKey("d") || Input.GetKey("q"))  plyr_rb.AddForce( new Vector3(0f, 0.32f, 0f), ForceMode.VelocityChange);

            //     if (grap_pnt != new Vector3(0,0,0))
            //     {
            //         if(plyr_trsnfm.position.z < grap_pnt.z - 3.5f)
            //         {
            //             plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 17);
            //             plyr_rb.AddForce( new Vector3(0, -0.20f, 0), ForceMode.VelocityChange);
            //         }
            //         else
            //         {
            //             plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 23);
            //             plyr_rb.AddForce( new Vector3(0, -0.20f,  0), ForceMode.VelocityChange);
            //         }
            //     }
            // }

        }
    }


    // Public fnc for cllision
    public void animateCollision(string cls_type, Vector3 cls_size,  GameObject optional_gm = null)
    {
        float jumpForce = Mathf.Sqrt(4.5f * -2 * (Physics.gravity.y));

        if(cls_type != "void" && cls_type != "emptyEnemyAim")
        {
            if(gameOver_ || plyr_saveClimbing || plyr_boxFalling)
                return;
        }
      


        combo(cls_type, optional_gm);


        switch(cls_type)
        {
            case "groundLeave":
                if(!plyr_sliding && !plyr_tapTapJumping && !plyr_railSliding && !plyr_obstclJmping
                    && (!_anim.GetBool("GroundHit"))
                    && !plyr_bareerJumping
                    && !plyr_jumping
                    && !plyr_launchJumping
                )
                {
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z);
                    plyr_rb.AddForce(new Vector3(0f, jumpForce * 0.8f, 0f), ForceMode.VelocityChange);
                    cm_movement.leave_ground(transform.rotation.eulerAngles.y);
                }

                // Debug.Log("GROUND LEAVE " + Time.time);
                _anim.SetBool("Flying", true);
                _anim.SetBool("GroundHit", false);
                plyr_flying = true;
                break;


            case "groundHit":
                if(plyr_saveClimbing)
                    return;

                // Debug.Log("GROUND HIT " + Time.time);
                _anim.SetBool("Flying", false);
                _anim.SetBool("GroundHit", true);

                jumpCnt = 2;
                dashDownCnt = 1;

                if(plyr_bareerJumping)
                    return;
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.6f));

                if (plyr_flying)
                {
                    StartCoroutine(Dly_bool_anm(0.3f, "GroundHit"));

                    plyr_flying = false;
                    if(!plyr_sliding)
                        cm_movement.ground_roll();
                }
                break;

            case "wallObstacleHit":
                if(plyr_wallRninng)
                {
                    if(!plyr_obstclJmping)
                    {
                        animateCollision("obstacleHit", Vector3.zero, optional_gm);
                    }
                }else
                    return ;
                break;
            case "obstacleHit":

                if(!plyr_obstclJmping)
                {
                    last_ObstJumped = optional_gm;

                    Vector3 r = optional_gm.transform.rotation.eulerAngles;
                

                    bool _object_straight = (((r.y >= 70f && r.y <= 110f) || (r.y >= 160f && r.y <= 200f)
                            || (r.y >= 250f && r.y <= 290f) ) ? 
                        (true) : (false));
                    bool _object_travers = !(_object_straight);

                    Mesh mesh = ( !(optional_gm.GetComponent<MeshFilter>()) ? 
                        (null) : ((optional_gm.GetComponent<MeshFilter>())?.sharedMesh) 
                    );
                    Vector3 s_v3;
                    Vector3 top_vertex = new Vector3(0f, float.MinValue, 0f);
                    Vector3 far_away_vertex = new Vector3(0f, 0f, float.MinValue);
                    Vector3 near_vertex = new Vector3(0f, 0f, float.MaxValue);
                    Vector3 min_x = new Vector3(float.MaxValue, 0f, 0f), max_x = new Vector3(float.MinValue, 0f, 0f);
                    if(mesh != null)
                    {
                        // Debug.Log("-mesh");
                        s_v3 = mesh.bounds.size;
                        for(int i = 0; i < mesh.vertexCount; i++)
                        {
                            if(mesh.vertices[i].x < min_x.x)
                                min_x = mesh.vertices[i];
                            if(mesh.vertices[i].x > max_x.x)
                                max_x = mesh.vertices[i];
                            if(mesh.vertices[i].z > far_away_vertex.z)
                                far_away_vertex =  mesh.vertices[i];
                            if(mesh.vertices[i].z < near_vertex.z)
                                near_vertex =  mesh.vertices[i];
                            if(mesh.vertices[i].y > top_vertex.y)
                                top_vertex = mesh.vertices[i];
                        }
                        min_x = optional_gm.transform.TransformPoint(min_x);
                        max_x = optional_gm.transform.TransformPoint(max_x);
                        top_vertex = optional_gm.transform.TransformPoint(top_vertex);
                        far_away_vertex = new Vector3(
                            optional_gm.transform.TransformPoint(far_away_vertex).x,
                            optional_gm.transform.TransformPoint(far_away_vertex).y,
                            (optional_gm.transform.position.z) +
                            (Mathf.Abs(optional_gm.transform.TransformPoint(far_away_vertex).z - optional_gm.transform.position.z))
                        );
                    }else
                    {
                        // Debug.Log("non-mesh");
                        Collider[] c_arr = optional_gm.GetComponents<Collider>();
                        Bounds b_ = new Bounds(Vector3.zero, Vector3.zero);
                        s_v3 = Vector3.zero;
                        for(int i = 0; i < c_arr.Length; i ++)
                        {
                            if((float)(c_arr[i].bounds.size.x) > s_v3.x || (float)(c_arr[i].bounds.size.y) > s_v3.y
                            || (float)(c_arr[i].bounds.size.z) > s_v3.z)
                            {
                                b_ = c_arr[i].bounds;
                                s_v3 = c_arr[i].bounds.size;
                            }
                        }
                        top_vertex = new Vector3(0f, b_.center.y  + (s_v3.y / 2), 0f);
                        min_x = new Vector3(b_.center.x - (s_v3.x / 2), 0f, 0f);
                        max_x = new Vector3(b_.center.x + (s_v3.x / 2), 0f, 0f);
                        far_away_vertex = new Vector3(
                            b_.center.x, 
                            b_.center.y + (s_v3.y / 2), b_.center.z + (s_v3.z / 2)
                        );
                    }
                    
                    /*
                    Debug.Log("----------------------------------------------------------------------------------------------------------------------------");
                    Debug.Log("GM:" + optional_gm + "   RT:" + r + "    BOUNDS:"+ (mesh != null ? mesh.bounds.size : null)
                        + "    STRAIGHT?:" + _object_straight + " TOP[]" + top_vertex);
                    Debug.Log("----------------------------------------------------------------------------------------------------------------------------");
                    */
                    plyr_obstclJmping = true;
                    movement_auth = false;
                    _anim.SetBool("obstacleJump", true);


                    if(plyr_wallRninng)
                    {
                        // obst_4
                        plyr_rb.useGravity = false;
                        _anim.SetInteger("obstacleType", 4);
                        transform.position = new Vector3(
                            transform.position.x, top_vertex.y, transform.position.z
                        );
                        plyr_rb.velocity = Vector3.zero;
                        StartCoroutine(Dly_bool_anm(1f, "obstacleJump"));        
                        LeanTween.move( gameObject, 
                            new Vector3(
                                transform.position.x,
                                transform.position.y, 
                                far_away_vertex.z
                            ),
                            0.95f
                        );
                        bool sns = !(((pico_character.transform.localRotation.eulerAngles.z > 180f) ? (true) : (false)));
                        _anim.SetBool("Y_ROT", (sns));
                        saved_wallRun_picoPos = (pico_character.transform.localPosition);
                        pico_character.transform.localPosition = new Vector3(
                            sns ? -0.4f : 0.4f, 0f, 0f
                        );
                        saved_wallRun_quatn = (Quaternion)(pico_character.transform.localRotation);
                        pico_character.transform.localRotation = Quaternion.identity;
                        obst_type = 4;
                    }
                    else
                    {
                        // o o o
                        if ((_object_travers))
                        {
                            // obst_1
                            _anim.SetInteger("obstacleType", 1);
                            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 3, 0f, plyr_rb.velocity.z);
                            transform.position = new Vector3(
                                transform.position.x, top_vertex.y + (0.01f), transform.position.z - 0.35f
                            );
                            StartCoroutine(Dly_bool_anm(0.43f, "obstacleJump"));
                            plyr_rb.AddForce(
                                new Vector3(
                                    0f, 
                                    0f,
                                    transform.position.z < optional_gm.transform.position.z - (s_v3.z / 4) ?
                                        (1.5f + ((s_v3.z) * (1.2f))) 
                                        : 
                                        (s_v3.z)
                                )
                                , ForceMode.VelocityChange
                            );                     
                            cm_movement.obstacle_jump(1);
                            obst_type = 1;
                        }
                        // o
                        // o
                        // o    
                        else
                        {
                            plyr_rb.velocity = new Vector3(0f, 0f, 0f);
                            plyr_rb.useGravity = false;
                            // obst_2 [SideWays]
                            if((transform.rotation.eulerAngles.y >= 180f && transform.rotation.eulerAngles.y <= 345f )
                                                                    ||
                                (transform.rotation.eulerAngles.y <= 180f && transform.rotation.eulerAngles.y >= 15f ) )
                            {
                                _anim.SetInteger("obstacleType", 2);
                    
                                bool plyr_inZone = (transform.position.x >= (min_x.x + (0.05f)) && transform.position.x <= (max_x.x - (0.05f)) );

                                side_obst_side = ((plyr_inZone)) ? 
                                    (!(transform.rotation.eulerAngles.y >= 180f)) : (transform.position.x < optional_gm.transform.position.x);
                                transform.position = new Vector3(
                                    (side_obst_side) ?
                                        (min_x.x - (0.1f)) :  (max_x.x + (0.1f)),
                                    top_vertex.y, 
                                    transform.position.z
                                );
                                _anim.SetBool("obstacle2ROT", side_obst_side);
                                
                                LeanTween.move( gameObject, 
                                    new Vector3(
                                        (side_obst_side) ?
                                            (max_x.x + (-0.05f)) : (min_x.x - (+0.05f)), 
                                        transform.position.y, 
                                        transform.position.z + (0.1f)
                                    ),
                                    0.6f
                                ); //.setEaseInOutCubic();
                                StartCoroutine(Dly_bool_anm(0.70f, "obstacleJump"));
                                cm_movement.obstacle_jump(2, side_obst_side);
                                obst_type = 2;
                            }
                            // obst 3 [Front]
                            else
                            {
                                _anim.SetInteger("obstacleType", 3);                     

                                transform.position = new Vector3(
                                    transform.position.x, top_vertex.y, transform.position.z
                                );
                                LeanTween.move( gameObject, 
                                    new Vector3(
                                        far_away_vertex.x, 
                                        far_away_vertex.y,
                                        far_away_vertex.z
                                    ),
                                    1.13f
                                );
                                StartCoroutine(Dly_bool_anm(1.15f, "obstacleJump"));    
                                cm_movement.obstacle_jump(3);
                                obst_type = 3;
                            }
                        }
                        _anim.SetBool("Flying", true);
                    }
                }
                break;


            case "obstacleLeave":
                // TODO : ADD RIGIDBODT TO OBJ AND THROW IT AWAY

                // kick obj when alr obst jumping
                // private void kickObst(GameObject obst)
                // {
                //     Rigidbody obst_rb = obst.GetComponent<Rigidbody>() == null ?
                //         obst.AddComponent<Rigidbody>() : obst.GetComponent<Rigidbody>();

                //     obst_rb.mass = 0.01f;
                //     Vector3 randTorque = new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-30, 30f), UnityEngine.Random.Range(15f, -15f));
                //     Vector3 kickForce = new Vector3(
                //         plyr_trsnfm.position.x - obst.transform.position.x,
                //         plyr_trsnfm.position.y - obst.transform.position.y, plyr_trsnfm.position.z - obst.transform.position.z
                //     );
                //     obst_rb.AddForce(new Vector3(kickForce.x * 2, 10, 16 ), ForceMode.VelocityChange);
                //     obst_rb.AddTorque(randTorque, ForceMode.VelocityChange);
                // }
                // optional_gm.tag = "obstacle";
                break;


            case "wallRunHit":
                if(plyr_obstclJmping && 
                    (obst_type == 4 || obst_type == 1))
                    return; 
                if(plyr_saveClimbing || plyr_hanging || plyr_wallRninng)
                    return;
                if(!plyr_boxFalling)
                {
                    if(plyr_bareerJumping && horizontal_breerJmping)
                    {
                        LeanTween.cancel(pico_character);
                        StopCoroutine(Dly_bool_anm(1.2f, "bareerJump"));
                        _anim.SetBool("bareerJump", false);
                        horizontal_breerJmping = plyr_bareerJumping = false;
                    }

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
                    ) * 10 : (Math.Abs(h_y) <= 30 ?
                        h_y : 0
                    );

                    if(!plyr_wallSliding)
                    {
                        cm_movement.wal_rn_offset(false, hitWall.transform, 0f/* y_bonus */);
                    }

                    plyr_rb.velocity = new Vector3(0f, plyr_rb.velocity.y, plyr_rb.velocity.z);
                    if(sns < 0)
                    {
                        StartCoroutine(force_wallRn(true));
                        plyr_rb.AddForce(Vector3.left * 12, ForceMode.VelocityChange);
                        _anim.SetBool("wallRunSide", false);
                    }else
                    {
                        StartCoroutine(force_wallRn(false));
                        plyr_rb.AddForce(Vector3.right * 12, ForceMode.VelocityChange);
                        _anim.SetBool("wallRunSide", true);
                    }

                    plyr_flying = true;

                    Vector3 p_ = new Vector3( sns < 0 ? -0.45f : 0.45f, 0, 0);
                    Quaternion q_ = Quaternion.Euler(0,  /*(sns <  0 ? -30f : -41f) + */ y_bonus, sns <  0 ? -47f : 47f);

                    pico_character.transform.localRotation = q_;
                    pico_character.transform.localPosition = p_;

                    psCollisions_movement.wallRun_aimBox = true;
                    psCollisions_movement.z_wallRun_aimRotation = ( (sns <  0) ? -47f : 47f );

                    wall_running_wall = (hitWall);
                }
                break;


            case "wallRunExit":
                if(!plyr_wallRninng)
                    return ;
                if(!plyr_saveClimbing && !plyr_boxFalling)
                {
                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.5f));

                    if(!(plyr_obstclJmping && obst_type == 4))
                    {
                        if(!plyr_bareerJumping)
                        {
                            plyr_rb.AddForce( new Vector3(0, jumpForce * 0.82f, 0), ForceMode.VelocityChange);
                            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 3, plyr_rb.velocity.y, plyr_rb.velocity.z);
                        }else
                        {
                            plyr_rb.AddForce( new Vector3(0, 4f, 0), ForceMode.VelocityChange);
                        }
                    }

                    _anim.SetBool("Flying", true);
                    _anim.SetBool("wallRun", false);

                    plyr_wallRninng = false;

                    if(plyr_wallSliding)
                    {
                        plyr_wallSliding = false;
                        _anim.SetBool("slideWall", false); 
                    }

                    if(!(plyr_obstclJmping && obst_type == 4))
                    {
                        if(plyr_bareerJumping)
                        {
                            LeanTween.moveLocal(pico_character, Vector3.zero, 1.1f).setEaseInOutCubic();
                            LeanTween.rotateLocal(pico_character, Vector3.zero, 1.3f).setEaseInOutCubic();
                        }
                        else
                        {
                            pico_character.transform.rotation = new Quaternion(0, 0, 0, 0);
                            pico_character.transform.localPosition = new Vector3(0, 0, 0);
                        }
                    }

                    psCollisions_movement.wallRun_aimBox = false;

                    StartCoroutine(wall_exit());    
                    bool sns = (optional_gm.transform.position.x - gameObject.transform.position.x) > 0;
         
                    if(!plyr_bareerJumping)
                    {
                        if(!(plyr_obstclJmping && obst_type == 4))
                        {
                            cm_movement.wall_outttt(sns);
                        }
                    }else
                    {
                        cm_movement.bareer_jmp(true, optional_gm.transform.position.x  < transform.position.x);
                    }
                    if(!(plyr_obstclJmping && obst_type == 4))
                    {
                        cm_movement.wal_rn_offset(true, optional_gm.transform); // turn off offset
                    }
                    lft_Straf = rght_Straf = false;
                    wall_running_wall = null;
                }
                break;


            case "frontWallExit":
                if(!plyr_flying)
                {
                    if(plyr_groundWalling)
                    {
                        
                    }
                }
                break;
            case "frontWallHit":    
                    if( !plyr_saveClimbing && !plyr_climbingLadder
                        && (!plyr_bareerJumping)
                    )
                    {
                        // prevent from front smashing it while wallruning
                        if( (plyr_wallRninng && optional_gm == wall_running_wall))
                            return ;

                        // Matrix4x4 p = optional_gm.transform.localToWorldMatrix;
                        // if is a rebord hit
                        bool is_rebord = false;

                        Mesh m_contact = optional_gm.GetComponent<MeshFilter>().sharedMesh;

                        float msh_y2 = (m_contact.bounds.size.y/2)* (
                            optional_gm.transform.lossyScale.y
                        ); // good

                        // float top_y2 = ((optional_gm.transform.position.y)
                        //     + (msh_y2) +
                        //     ((float) (m_contact.bounds.center.y * optional_gm.transform.lossyScale.y))
                        // ); // good

                        Vector3 topVertex = new Vector3(0,float.NegativeInfinity,0);
                        Vector3[] verts = m_contact.vertices;
                        for(int i = 0; i < m_contact.vertices.Length; i ++)
                        {
                            if(verts[i].y > topVertex.y)
                            {
                                topVertex = verts[i];
                            }
                        }
                        Vector3 l = optional_gm.transform.TransformPoint(topVertex);
                        float top_y2 = l.y;
                        
                        is_rebord = ( ((top_y2 - transform.position.y) <= 4f ? (true) : (false)) );

                        // GROUD WALL
                        if(!plyr_flying)
                        {
                            Debug.Log("FRONT WALL");
                            _anim.SetBool("groundWall", true);
                            movement_auth = false;
                            plyr_rb.AddForce(new Vector3(0f, (top_y2 - transform.position.y) * 4f, 0f), ForceMode.VelocityChange);
                            plyr_groundWalling = true;
                        }
                        // FRONT WALL
                        else
                        {
                            if(plyr_wallRninng)
                            {
                                plyr_wallRninng = false;
                                pico_character.transform.localRotation = Quaternion.identity;
                                pico_character.transform.localPosition = Vector3.zero;   
                            }

                            if(is_rebord)
                            {
                                _anim.SetBool("climb", true);
                                // _anim.SetBool("Flying", true);

                                movement_auth = false;
                                plyr_rb.useGravity = false;
                                plyr_saveClimbing = true;
                                plyr_flying = true;


                                float r_y = optional_gm.transform.rotation.eulerAngles.y;

                                int q = ((int)(r_y) / 45);
                                float y_bonus = q > 0 ? (r_y
                                    - (q * 45)
                                    + ( (r_y/ 45) - (q)
                                ) > 0.75f ?
                                    (r_y/ 45) - (q) : 0
                                ) * 5 : ( r_y <= 40f ? r_y : 0);

                                float y_bonus_2 = (q > 0 ? 
                                ((r_y - (q * 45)) > 22.5f ? 
                                    Math.Abs((r_y - (q * 45)) - 45f) : ((r_y - (q * 45)))
                                ) : 
                                ( r_y > (float)(45/2) ? (r_y - 45/2) : r_y )) * (r_y > 190f ? 1f : -1f);

                                // Debug.Log("[STRUCTURE] Y_EULER : " + r_y + " Q : " + q + " Y_BONUS : " + y_bonus
                                //             + " Y_BONUS2 : " + y_bonus_2);

                                rot_y_saveClimbing = y_bonus_2;
                                transform.rotation = Quaternion.Euler(0, y_bonus_2, 0f);
                                plyr_rb.velocity = new Vector3(0, 0, 0);

                                // throw animation
                                cm_movement.climbUp();
                                StartCoroutine(Dly_bool_anm(12f, "climb"));
                                

                                LeanTween.move(gameObject,
                                    new Vector3(transform.position.x, top_y2 + (0.075f), transform.position.z - 0.04f),
                                0.4f).setEaseInOutCubic();
                            }
                            else
                            {
                                g_ui.gameOver_ui("front", transform.rotation);
                            }
                            _anim.SetBool("wallRun", false);
                        }
                }
                break;


            case "sliderHit":
                if(plyr_sliding || plyr_wallSliding)
                    return ;

                // SLIDE-WALL
                if(optional_gm.transform.rotation.eulerAngles.z > 30f)
                {
                    if(optional_gm == wallSlide_wall)
                        return ;
                    wallSlide_wall = (GameObject) optional_gm;

                    _anim.SetBool("slideWall", true); 
                    plyr_wallSliding = true;
                    transform.position =  new Vector3(
                        transform.position.x,
                        optional_gm.transform.position.y - 0.5f, 
                        transform.position.z
                    );
                    cm_movement.wal_slide_offset(false, optional_gm.transform);
                }
                else // SLIDER
                {
                    _anim.SetBool("slide", true); 
                    plyr_sliding = true;
                    rotate_bck();
                    jumpCnt = 2;
                    dashDownCnt = 1;
                    cm_movement.sld_offset(false);

                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z);

                    GameObject top = optional_gm.transform.parent.GetChild(1).gameObject;
                    if((transform.position.y < top.transform.position.y) )
                    {
                        transform.position = new Vector3(
                            transform.position.x, top.transform.position.y + 0.02f, transform.position.z
                        );
                    }
                    
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z);
                }
                break;


            case "sliderLeave":
                slide_timer = 0f;

                // SLIDER
                if(!plyr_jumping && plyr_sliding && !plyr_wallSliding)
                {
            
                    plyr_rb.AddForce( new Vector3(0, jumpForce * 0.75f, 0f), ForceMode.VelocityChange);
                    plyr_sliding = false;
                    cm_movement.jumpOut(transform.rotation.eulerAngles.y);
                    // Debug.Log("slide out force")
                    _anim.SetBool("slide", false);
                }
                // SLIDE-WALL
                if(plyr_wallSliding)
                {
                    wallSlide_wall = null;
                    plyr_wallSliding = false;
                    _anim.SetBool("slideWall", false); 
                }
                break;


            case "launcherHit":
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1f));

                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z);
                plyr_rb.AddForce( new Vector3(0, jumpForce * 1.1f, 0), ForceMode.VelocityChange);
                plyr_rb.AddTorque(
                    new Vector3((transform.rotation.eulerAngles.y > 180f ? 50f : -50f), 0f, 0f), 
                    ForceMode.VelocityChange
                );
                StartCoroutine(Dly_bool_anm(0.80f, "launcherJump"));

                plyr_launchJumping = true;

                // FindObjectOfType<CameraMovement>().special_jmp();
                cm_movement.special_jmp();
                action_momentum += 10f;
                break;


            case "tapTapJump":
                rotate_bck();
                // #1
                _anim.SetBool("tapTapJump", true);

                // #2
                _anim.SetBool("Flying", true);

                // if(!plyr_tapTapJumping)
                //     Debug.Log("TAP TAP HIT " + Time.time);

                taptap_V3 = (optional_gm.transform.parent).GetChild(5).transform.position;

                if(!plyr_tapTapJumping)
                {
                    transform.position = new Vector3(
                        ( (transform.position.x > (taptap_V3.x + 0.6f)) 
                            || 
                          (transform.position.x < (taptap_V3.x - 0.6f))
                        ) 
                        ?
                            (transform.position.x > taptap_V3.x ? (taptap_V3.x + 0.6f) : (taptap_V3.x - 0.6f)) 
                                :
                            (transform.position.x)
                        ,
                        taptap_V3.y,
                        taptap_V3.z
                    );
                }

                // tapTap end special trigger
                if(optional_gm.transform.GetSiblingIndex() == 0)
                {
                    if(tapTap_exited)
                        return ;
                    plyr_rb.AddForce(new Vector3(0f, 2f, 0f), ForceMode.VelocityChange);
                    cm_movement.tapTapJmp(true);
                    action_momentum += 2f;
                    tapTap_exited = true;
                }
          
            
                if(plyr_tapTapJumping) 
                    return;

                plyr_tapTapJumping = true;
                plyr_rb.velocity = new Vector3(
                    plyr_rb.velocity.x * 0.15f, 0, plyr_rb.velocity.z / 2
                );

                cm_movement.tapTapJmp(false);

                StartCoroutine(Dly_bool_anm((1.25f), "tapTapJump"));
                break;

            case "tapTapJumpExit":
                if(plyr_tapTapJumping)
                {
                    if(optional_gm.transform.GetSiblingIndex() == 2)
                    {
                        plyr_tapTapJumping = false;
                        _anim.SetBool("tapTapJump", false);
                        StopCoroutine(Dly_bool_anm(0f, "tapTapJump"));
                        tapTap_exited = false;
                        plyr_rb.AddForce(new Vector3(0f, 6f, 0f), ForceMode.VelocityChange);
                        action_momentum += 5f;
                    }
                }
                break;


            case "railSlide":
                StopCoroutine(delay_jumpInput(0.0f));
                if(slideRail_ref == optional_gm)
                { return; };

                jump_auth = false;


                tyro_handler_child = null;


                PathCreator[] paths_ = optional_gm.GetComponentsInChildren<PathCreator>();
                actual_path = paths_[0];

                float r_speed =  paths_[0].path.GetPointAtTime(0.99f).z - paths_[0].path.GetPointAtTime(0f).z;

                _anim.SetBool(
                    "railSide",
                    actual_path.path.GetPoint(actual_path.path.NumPoints - 1).x
                    -
                    actual_path.path.GetPoint(0).x > 0 ? true : false
                );
                _anim.SetBool("slideRail", true);

                cm_movement.railSlide_offset(
                    false,
                    _anim.GetBool("railSide")
                );

                if(plyr_wallRninng)
                {
                    plyr_wallRninng = false;
                    _anim.SetBool("wallRun", false);
                }

                // adaptive rail speed
                float rS_adaptiveSpeed = (r_speed / 100) < 0.16f ? 0.16f : r_speed / 100;
                // railSlide_speed = (
                //     2 / (paths_[0].gameObject.transform.lossyScale.z * 1.2f)
                //     + (rS_adaptiveSpeed)
                // );

                end_tyroPos = actual_path.path.GetPoint(actual_path.path.NumPoints - 1);
                last_tyro_trvld = actual_path.path.GetClosestDistanceAlongPath(transform.position);

                // turn off gravity
                plyr_rb.useGravity = false;
                plyr_rb.velocity = new Vector3(0, 0, 0);

                plyr_railSliding = true;

                // ui bonus call
                g_ui.set_countBonus_= true;

                slideRail_ref = optional_gm;
                break;


            case "railSlideExit":
                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 2, plyr_rb.velocity.y, plyr_rb.velocity.z);
                break;


            case "hang":
                if(plyr_hanging || plyr_wallRninng)
                    return;
                    
                movement_auth = false;
                plyr_rb.useGravity = false;
                StartCoroutine(Dly_bool_anm(2f, "hang"));
                plyr_rb.velocity = Vector3.zero;

                hang_bar = optional_gm;
                cm_movement.hang(false);
                transform.rotation = optional_gm.transform.rotation;

                // transform.rotation = Quaternion.Euler(
                //     optional_gm.transform.position.y < (transform.position.y + 1.8f) ? (
                //             ((transform.position.y + 1.5f) - optional_gm.transform.position.y) * 20f
                //     ) : 0f,
                // 0f, 0f);
                plyr_trsnfm.position = new Vector3(
                    plyr_trsnfm.position.x, 
                    (optional_gm.transform.position.y - 1.82f),
                    optional_gm.transform.position.z - 0.625f
                );
            
                LeanTween.moveLocal(pico_character, new Vector3(0f, 0.15f, 0.85f), 1.65f).setEaseInSine();
                StartCoroutine(Dly_bool_anm(1.2f, "hangJump"));


                plyr_hanging = true;
                // // turn off gravity
                // plyr_rb.useGravity = false;
                // plyr_rb.velocity = new Vector3(0, 0, 0);

                // Collider c_ = optional_gm.GetComponent<Collider>();

                // float x_ps = transform.position.x;
                // float x_sz = ( optional_gm.transform.position.x - transform.position.x > 0f ?
                //     ((c_.bounds.size.x/2)): -1 * ((c_.bounds.size.x/2))
                //     );

                // Vector3 pos_ =  optional_gm.transform.position + new Vector3(0f, 0.0f, 0.05f);
                // pos_ = new Vector3( ( Math.Abs(( optional_gm.transform.position.x +
                //         x_sz) - x_ps)
                //         < (0.2f)
                //      ? 3 * (x_sz/4) / 4 : x_ps),
                //     pos_.y, pos_.z
                // );
                // c_.enabled = false;
                // transform.position = pos_;

                // Quaternion forced_y = Quaternion.Euler(0f, optional_gm.transform.rotation.eulerAngles.y > 180f ?
                //     -1 * (360f - optional_gm.transform.rotation.eulerAngles.y) :
                //     optional_gm.transform.rotation.eulerAngles.y
                // ,0f);
                // transform.rotation = forced_y;

                // Transform pico_p = pico_character.transform.parent;
                // pico_character.transform.localPosition = new Vector3(0, -1.050f, -0.532f);
                // plyr_hanging = true;
                // pico_character.transform.localRotation = Quaternion.Euler(0, -13.2f, 0);

                // Transform root_ = game_cam.transform.parent;

                // LTDescr rt_animation = LeanTween.rotateLocal(pico_p.gameObject,new Vector3(-60f, 0f, 0f), 3.5f).setEaseInOutCubic();
                // cm_movement.stored_picoParent = pico_p;

                // moving_hangTraveler = new GameObject("moving_hangTraveler");
                // moving_hangTraveler.transform.parent = pico_p;

                // Vector3 init_camPos = new Vector3(transform.position.x, optional_gm.transform.position.y - 0.35f,
                //     optional_gm.transform.position.z - 5f
                // );

                // moving_hangTraveler.transform.position = init_camPos;
                // moving_hangTraveler.transform.localPosition = new Vector3(0, moving_hangTraveler.transform.localPosition.y, moving_hangTraveler.transform.localPosition.z);

                // game_cam.transform.position = init_camPos;
                // game_cam.transform.position =  new Vector3(moving_hangTraveler.transform.position.x, game_cam.transform.position.y, game_cam.transform.position.z);

                // cm_movement.hang_point = moving_hangTraveler.transform;
                // LeanTween.moveLocal(moving_hangTraveler, new Vector3(0, -2f, 2f), 4.75f).setEaseInOutCubic();

                // game_cam.transform.rotation = Quaternion.Euler( game_cam.transform.rotation.eulerAngles.x, 0f, game_cam.transform.rotation.eulerAngles.z);

                // pico_character.transform.localRotation = Quaternion.Euler(0, -6.2f, 0);
                // StartCoroutine(Dly_bool_anm(1.7f, "hang"));
                // StartCoroutine(hang_transformFixes_andReset(pico_p, moving_hangTraveler, rt_animation));
                break;


            case "rampSlide":
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1f));
                rotate_bck();
                // fix_Cldrs_pos( 0.42f, true);

                plyr_rampSliding = true;
                _anim.SetBool("rampSlide", true);

                pico_character.transform.localRotation = Quaternion.Euler(0f, 92f,
                    (optional_gm.transform.rotation.eulerAngles.x > 180f ? (360f - optional_gm.transform.rotation.eulerAngles.x ) : optional_gm.transform.rotation.eulerAngles.x ) * 0.6f
                    + (optional_gm.transform.rotation.eulerAngles.x - 300) * 1.5f
                );
                pico_character.transform.localPosition = new Vector3(0f, 0.14f, -0.45f);

                Invoke("ramp_bool", 1f);
                jumpCnt = 2;
                break;


            case "rampSlideExit":
                _anim.SetBool("rampSlide", false);
                plyr_rampSliding = false;

                // fix_Cldrs_pos(-0.42f, false);

                pico_character.transform.localRotation = Quaternion.identity;
                pico_character.transform.localPosition = Vector3.zero;

                special_rampDelay = true;
                break;

                
            case "fallBox":

                if(_anim.GetBool("fallBox") || last_landBoxId == optional_gm.GetInstanceID() 
                    || (plyr_interacting && !(plyr_jumping || plyr_downDashing))
                )
                    return;

                last_landBoxId = optional_gm.GetInstanceID();


                StartCoroutine(Dly_bool_anm(0.8f, "fallBox"));
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.2f));

                _anim.SetBool("Flying", false);


                movement_auth = false;
                plyr_flying = false;
                plyr_boxFalling = true;
                _anim.SetBool("flying", false);

                jumpCnt = 2;
                dashDownCnt = 1;

                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z);
                Debug.Log(transform.rotation.eulerAngles);
                cm_movement.fall_box(transform.rotation.eulerAngles.y > 180f);
                plyr_rb.AddForce(
                    new Vector3(0f, -6f, 3f),
                    ForceMode.VelocityChange
                );

                // Mesh box_contact = (
                //     optional_gm.transform.GetChild(1)
                // ).GetComponent<MeshFilter>().sharedMesh;
                // Vector3 back_Vertex = new Vector3(0,float.NegativeInfinity,0);
                // Vector3[] box_verts = box_contact.vertices;
                // for(int i = 0; i < box_contact.vertices.Length; i ++)
                // {
                //     if(box_verts[i].z > back_Vertex.z)
                //     {
                //         back_Vertex = box_verts[i];
                //     }
                // }
                // back_Vertex.z = -(back_Vertex.z);
                // Vector3 m = (optional_gm.transform.GetChild(1)).TransformPoint(back_Vertex);
                // Debug.Log(m);
                // float x_f = Mathf.Abs(optional_gm.transform.position.x) - Mathf.Abs(transform.position.x);
                // transform.rotation = Quaternion.Euler(
                //     0f,
                //     x_f * -8f,
                //     0f
                // );
                // movement_auth = false;

                
                // LeanTween.move(gameObject, 
                //     new Vector3(transform.position.x, transform.position.y, m.z),
                // 1.3f).setEaseInOutCubic();
                break;


            case "fallBoxExit":
                return; 
                if(plyr_boxFalling || optional_gm.GetInstanceID() == last_jumpedOut_landBoxId)
                    return;

                last_jumpedOut_landBoxId = optional_gm.GetInstanceID();


                _anim.SetBool("Flying", true);
                plyr_flying = true;
                
                plyr_rb.AddForce(new Vector3(0f, 5f, 0f), ForceMode.VelocityChange);
                cm_movement.leave_ground(transform.rotation.eulerAngles.y);
                break;


            case "ladderHit":
                _anim.SetBool("ladder", true);

                transform.position = new Vector3(optional_gm.transform.position.x - 0.0f, transform.position.y, transform.position.z);

                plyr_rb.velocity = new Vector3(0, 0, 0);
                plyr_climbingLadder = true;

                cm_movement.ladderClimb_offst(false);
                StartCoroutine(Dly_bool_anm(0.5f, "ladderInputDelay"));

                action_momentum = -10f;
                StartCoroutine(ladder_climb(moving_camTraveler));

                transform.rotation = Quaternion.Euler(0f, 0.1f, 0f);

                Vector3 targetDirection = optional_gm.transform.position - transform.position;
                Vector3 dir = Vector3.RotateTowards(transform.forward, targetDirection, 1.0f * Time.deltaTime, 100f);

                transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(dir).eulerAngles.y, 0f);

                break;

            case "ladderLeave":
                _anim.SetBool("ladder", false);
                plyr_climbingLadder = false;

                plyr_rb.AddForce( new Vector3(0, 12f, -10f), ForceMode.VelocityChange);

                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 3, plyr_rb.velocity.y, plyr_rb.velocity.z);

                StopCoroutine(ladder_climb(moving_camTraveler));
                StopCoroutine(Dly_bool_anm(0.5f, "ladderInputDelay"));
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.5f));

                cm_movement.ladderClimb_offst(true);
                cm_movement.special_jmp();
                break;

    
            case "under":
                StartCoroutine(Dly_bool_anm(1f, "underSlide"));
                movement_auth = false;
                transform.position = new Vector3(transform.position.x, optional_gm.transform.position.y, transform.position.z);
                plyr_rb.velocity = new Vector3(0, 0, 0);
                plyr_rb.useGravity = false;
                plyr_rb.AddForce( new Vector3(0f, 0f, 15f), ForceMode.VelocityChange);

                cm_movement.under();
                plyr_underSliding = true;
                break;

            case "ceiling":
                break;


            case "bareerW":
            case "bareerG":

                if(plyr_bareerJumping || (!plyr_wallRninng && cls_type == "bareerW"))
                    return ;

                plyr_bareerJumping = true;

                // WALL BAREER
                if(optional_gm.transform.rotation.eulerAngles.z > 30f && cls_type == "bareerW" )
                {
                    if(!plyr_wallRninng)
                        return;

                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.5f));    
                    _anim.SetInteger("bareerJmpType", 1);
                    horizontal_breerJmping = true;
                    lateral_breerJmping = false;
                    MeshCollider c = optional_gm.GetComponent<MeshCollider>();
                    c.enabled = false;
                    plyr_rb.AddForce(
                        new Vector3( 
                            optional_gm.transform.rotation.eulerAngles.z > 180f ? 3.5f : -3.5f, 
                            12f, 
                        0f),
                        ForceMode.VelocityChange
                    );
                    action_momentum += 10f + (action_momentum * 0.5f);
                    StartCoroutine(Dly_bool_anm(1.2f, "bareerJump"));
                    // cm_movement.bareer_jmp(true, falsef) ---> moved to WallExit
                }
                else
                {
                    // GROUND BAREER
                    if(cls_type == "bareerG")
                    {
                        horizontal_breerJmping = false;
                        MeshCollider c = optional_gm.GetComponent<MeshCollider>();
                        float bareer_y_rot = optional_gm.transform.rotation.eulerAngles.y;
                        // LATERAL
                        if(bareer_y_rot >= 60f && bareer_y_rot <= 120f)
                        {
                            c.enabled = false;
                            lateral_breerJmping = true;
                            StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.1f));
                            _anim.SetInteger("bareerJmpType", 2);
                            StartCoroutine(Dly_bool_anm(0.8f, "bareerJump"));
                            cm_movement.bareer_jmp(false, optional_gm.transform.position.x > transform.position.x, true);
                            plyr_rb.AddForce(
                                new Vector3( optional_gm.transform.position.x > transform.position.x ? 3f : -3f, 14f, 0f),
                                ForceMode.VelocityChange
                            );
                        }
                        // CLASSIC
                        else
                        {
                            c.enabled = false;
                            lateral_breerJmping = false;
                            _anim.SetInteger("bareerJmpType", 1);
                            action_momentum += 1.5f + (action_momentum * 0.5f);
                            StartCoroutine(Dly_bool_anm(1.2f, "bareerJump"));
                            plyr_rb.AddForce( new Vector3(0f, 15f, -12f), ForceMode.VelocityChange );
                            StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.5f));
                            cm_movement.bareer_jmp(false);
                        }
                    }
                }

                
                break;

                /*
                float bareer_y_rot = optional_gm.transform.rotation.eulerAngles.y;

                // LATERAL BAREER
                if(bareer_y_rot >= 60f && bareer_y_rot <= 120f)
                {
                    if(bareer_j_type == -2 || plyr_wallRninng)
                        return;
                    plyr_bareerJumping = true;
                    bareer_j_type = -2;
                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1f));
                    if(optional_gm.transform.position.x > transform.position.x)
                    {
                        cm_movement.bareer_jmp(3);
                        _anim.SetInteger("bareerJmpType", -2);
                    }else
                    {
                        cm_movement.bareer_jmp(4);
                        _anim.SetInteger("bareerJmpType", -1);
                    }
                    movement_auth = false;
                    transform.position = new Vector3(
                        transform.position.x, 
                        optional_gm.transform.position.y + 0.8f, transform.position.z
                    );
                    plyr_rb.velocity = new Vector3( plyr_rb.velocity.x / 2, 0f, 0f);
                    plyr_rb.AddForce(
                            new Vector3(
                                optional_gm.transform.position.x > transform.position.x ? 5f : -5f, 5f, 3f
                            ),
                            ForceMode.VelocityChange
                    );
                    StartCoroutine(Dly_bool_anm(0.8f, "bareerJump"));
                }
                else // FACE BAREER
                {
                    if(!plyr_flying)
                        return;
                    if(plyr_bareerJumping && bareer_j_type == 1)
                    {
                        cm_movement.bareer_jmp(2);
                        plyr_rb.AddForce(
                            new Vector3(
                                0f, 14.5f,
                            (player_speed + action_momentum + (momentum_ * 3f)) * -0.45f
                            ), 
                        ForceMode.VelocityChange);
                        bareer_j_type = -1;
                        Invoke("bareer_resetMvmnt", 0.46f);
                        return;
                    }

                    if(plyr_bareerJumping && bareer_j_type != 1)
                        return;
                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.5f));

                    int obj_order = optional_gm.transform.GetSiblingIndex();

                    plyr_bareerJumping = true;
                    plyr_rb.velocity = new Vector3(0, 0, 0);

                    movement_auth = false;

                    Vector3 v3 = (
                        (optional_gm.transform.position + new Vector3(-1f, -0.4f, 0f)) - transform.position
                    );

                    bareer_j_type = obj_order != 2 ? 1 : 0;
                    plyr_rb.AddForce(new Vector3(
                        v3.x,
                        obj_order == 2 ?
                            (7f) // upper hit
                                : 
                            (2f) // (v3.y <= 1.35f ? (v3.y * 12f) : (v3.y * 7f) // lowerhit
                            ,
                        obj_order == 2 ? (player_speed + action_momentum + (momentum_ * 3f)) * 0.4f : -1f  //v3.z * 3.7f
                    ), ForceMode.VelocityChange);

                    cm_movement.bareer_jmp(obj_order == 2 ? 0 : 1);
                    // Debug.Log(
                    //     "bareer " +  (obj_order == 2 ? "upper hit" : "lower hit") + //" V3.Y > 0F :" +  (v3.y > 0f) +
                    //     " BAREER = " +
                    //     new Vector3(
                    //         v3.x,
                    //         obj_order == 2 ?
                    //             (11f) 
                    //                 : 
                    //             (!plyr_flying ? v3.y * 52f :
                    //                 v3.y <= 1.35f ? (v3.y * 12f) : (v3.y * 7f)
                    //             ),
                    //         obj_order == 2 ? (player_speed + action_momentum + momentum_) * 0.4f : 0f  //v3.z * 3.7f
                    //     )
                    // );
                    rotate_bck();
                    // classic
                    if(obj_order != 2)
                    {
                        _anim.SetInteger("bareerJmpType", 2);
                    }else // under
                        _anim.SetInteger("bareerJmpType", 1);
                    
                    StartCoroutine(Dly_bool_anm(obj_order == 2 ? 1.15f : 1.36f, "bareerJump"));
                }
                */
                break;
            case "sideHang":
    
                _anim.SetBool("sideHang", true);
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.5f));
                movement_auth = false;
                plyr_rb.velocity = Vector3.zero;
                plyr_rb.useGravity = false;
                anim_bool_hang = true;
                side_hang_obj = optional_gm;

                cm_movement.side_hang(false, 
                    (optional_gm.transform.position.x < transform.position.x) ? (true) : (false)
                );

                pico_character.transform.localRotation = Quaternion.Euler(
                    0f,
                    Quaternion.RotateTowards(
                        pico_character.transform.localRotation , optional_gm.transform.rotation, 150f
                    ).eulerAngles.y + (180f), 
                    0f
                );
                transform.position = optional_gm.transform.GetChild(0).position 
                    + new Vector3(optional_gm.transform.position.x > transform.position.x ? -0.34f : 0.46f, -1f, 0f);
                break;

            case "newEnemyAim":
                if(!weapon_equipped)
                    return;
                    
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


                saved_WorldPos_armTarget = leftArm_TARGET.position;

                aimed_enemy = optional_gm.transform;
                if(ammo > 0)
                    cm_movement.set_aimedTarget = optional_gm.transform;

                shoot_blocked = false;
                // g_ui.newEnemy_UI(aimed_enemy); <-- moved in PlayerCollisions.cs
                break;

            case "emptyEnemyAim":
                head_TARGET.parent = transform; body_TARGET.parent = transform; leftArm_TARGET.parent = transform;
                leftArm_TARGET.localPosition = new Vector3(0, 0, 0);

                int rdm_ = UnityEngine.Random.Range(0, 3);
                body_TARGET.localPosition = rdm_ == 1 ? new Vector3(1.4f, -5.13f, 4.0f) : new Vector3(1.09f,-5.13f,1.74f);

                head_TARGET.localPosition = new Vector3(6.80f,-10.57f,16.06f);

                shoot_blocked = false;
                aimed_enemy = null;
                cm_movement.set_aimedTarget = null;
                break;

            case "blockEnemyAim":
                shoot_blocked = true;
                break;
            case "unblockEnemyAim":
                shoot_blocked = false;
                break;
            case "gun":
                shoot_blocked = false;
                weapon_equipped = false;
                aimed_enemy = null;

                // AnimatorController _anim_controller = _anim.runtimeAnimatorController as AnimatorController;
                _anim.SetBool("gunEquipped", true);
                // _anim_controller.layers[0].avatarMask = lower_body_mask;

                if(plyr_shooting)
                    setAimSettings(false);
                animateCollision("emptyEnemyAim", Vector3.zero);
                StartCoroutine(Dly_bool_anm(1.6f, "pickUpWeapon"));
                StartCoroutine(player_weaponScrpt.throw_weapon(0.6f));

                if(aimed_enemy == null)
                {
                    head_TARGET.parent = transform; body_TARGET.parent = transform;
                    body_TARGET.localPosition = new Vector3(1.4f, -5.13f, 4.0f);
                    head_TARGET.localPosition = new Vector3(6.80f,-10.57f,16.06f);
                }
                break;
            case "void":
                g_ui.gameOver_ui("void", transform.rotation);
                break;
            case "frontTurret_Col_GameOver":
                g_ui.gameOver_ui("turretCollision", transform.rotation);
                break;
            case "frontSpecialGameOver":
                g_ui.gameOver_ui("front", transform.rotation);
                break;
            case "sideVoid":
                g_ui.gameOver_ui("sideVoid", transform.rotation);
                break;
            default:
                break;
        }
    }


  
    // ramp
    private void ramp_bool()
    {
        special_rampDelay = false;
    }


    // LADDER //
    private IEnumerator ladder_climb(GameObject moving_camTraveler = null)
    {
        LTDescr[] lts_ = new LTDescr[2];
        int i = 0;

        while(_anim.GetBool("ladder"))
        {
            // lts_[0] = a; lts_[1] = b;
            plyr_rb.AddForce( new Vector3(0, i % 2 == 0 ? 15f : 13f, 0), ForceMode.VelocityChange);
            yield return new WaitForSeconds(i % 2 == 0 ? 0.62f : 0.6f);
            i++;
        }
        yield break;
    }


    // HANG //
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



    // TYRO //
    public void tyro_movement(GameObject path_obj)
    {
        if(plyr_tyro) return;

        // ui bonus
        g_ui.set_countBonus_ = true;

        // turn off all movements
        movement_auth = false;
        plyr_rb.useGravity = false;
        plyr_rb.velocity = new Vector3(0, 0, 0);

        last_tyro_trvld = 1.25f;


        Transform prnt_ = path_obj.transform.parent;
        PathCreator[] paths_ = prnt_.GetComponentsInChildren<PathCreator>();

        for(int i =0; i < paths_.Length; i ++)
            if(paths_[i].gameObject.transform.parent.tag != "slideRail")
                actual_path = paths_[i];

        end_tyroPos = actual_path.path.GetPoint(actual_path.path.NumPoints - 1);


        // find tyro handler
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
    }
    private void activateTyro(){plyr_tyro = true; plyr_intro_tyro = false;}





    // THROW ANIMATION //
    private IEnumerator Dly_bool_anm(float delay, string anim_bool)
    {

        _anim.SetBool(anim_bool, true);


        if(anim_bool == "Jump" || anim_bool == "DoubleJump") 
            plyr_jumping = true;

        if(anim_bool == "animKill")
            plyr_animKilling = true;


        if(anim_bool == "ladderInputDelay")
            movement_auth = false;

        if(anim_bool == "pickUpWeapon" || anim_bool == "gunUnEquipping" 
            || anim_bool == "gunEquipping")
            plyr_equiping = true;


        //yield on a new YieldInstruction that waits for "delay" seconds.
        yield return new WaitForSeconds(delay);
        // ----------------------------------------

        if(anim_bool == "pickUpWeapon" || anim_bool == "gunUnEquipping" 
            || anim_bool == "gunEquipping")
                plyr_equiping = false;

        if(anim_bool == "gunUnEquipping" || anim_bool == "gunEquipping")
        {
            weapon_equipped = !(weapon_equipped);
            _anim.SetBool("gunEquipped", (weapon_equipped));
        }

        if(anim_bool == "pickUpWeapon")
            playerEquip_weapon(true);

        if(anim_bool == "hangJump" && plyr_hanging)
        {
            plyr_hanging = false;
            //movement_auth = true;
            transform.rotation = Quaternion.Euler(0f, 0.01f, 0f);
            LeanTween.moveLocal(pico_character, Vector3.zero, 1f).setEaseInSine();
            plyr_rb.AddForce(new Vector3(0f, 14f, 10f), ForceMode.VelocityChange);
            StartCoroutine(Dly_bool_anm(0.6f, "hangJump2"));
            plyr_rb.useGravity = true;
            cm_movement.hang(true);
        }
        if(anim_bool == "hangJump2")
            movement_auth = true;

 
        if(anim_bool == "Jump" || anim_bool == "DoubleJump") 
            plyr_jumping = false;

        if(anim_bool == "dashDown")
            plyr_downDashing = false;


        if(anim_bool == "animKill")
        {
            plyr_animKilling = false;
            plyr_rb.AddForce( new Vector3(0f, 12f, 0f), ForceMode.VelocityChange);
        }

        if(anim_bool == "obstacleJump")
        {
  
            if(obst_type == 1)
            {
                plyr_rb.AddForce( new Vector3(0f, 4.5f, 0f), ForceMode.VelocityChange);
                action_momentum += 4f;
                movement_auth = true;
            }
            if(obst_type == 2)
            {
                plyr_rb.AddForce( new Vector3(side_obst_side ? 4f : -4f, 6f, 0f), ForceMode.VelocityChange);
                action_momentum += 2f;
                plyr_rb.useGravity = true;
                movement_auth = true;
            }
            if(obst_type == 3)
            {
                plyr_rb.AddForce( new Vector3(0f, 7f, 0f), ForceMode.VelocityChange);
                action_momentum += 3f;
                plyr_rb.useGravity = true;
                movement_auth = true;
            }
            if(obst_type == 4)
            {
                if(plyr_wallRninng)
                {
                    pico_character.transform.localRotation = (saved_wallRun_quatn);
                    pico_character.transform.localPosition = (saved_wallRun_picoPos);
                }
                else
                    cm_movement.wal_rn_offset(true, null); // turn off offset  
                
                plyr_rb.AddForce( new Vector3(0f, 4f, 0f), ForceMode.VelocityChange);      
                plyr_rb.useGravity = true;   
                movement_auth = true;
                action_momentum += (2f);
            }
            plyr_obstclJmping = false;
        }
     
        if(anim_bool == "ladderInputDelay")
            movement_auth = true;

        if(anim_bool == "fallBox")
        {
            plyr_boxFalling = false;
            action_momentum += 2f;
            movement_auth = true;
        }

        if(anim_bool == "climb")
        {
            if(plyr_saveClimbing)
            {
                StartCoroutine(game_Over("front"));
            }
        }
        if(anim_bool == "climbOut")
        {
            movement_auth = true;
            plyr_rb.useGravity = true;
            plyr_rb.AddForce( new Vector3(0f, 4f, -4f), ForceMode.VelocityChange);
        }

        
        if(anim_bool == "underSlide")
        {
            movement_auth = true;
            plyr_underSliding = false;
            plyr_rb.useGravity = true;
            plyr_rb.AddForce( new Vector3(0f, 10f, 10f), ForceMode.VelocityChange);
        }

        if(anim_bool == "tapTapJump")
        {
            if(_anim.GetBool("tapTapJump"))
            {
                plyr_tapTapJumping = false;
                // fix_Cldrs_pos( 0.25f, false);
                tapTap_exited = false;
            }
        }

        if(anim_bool == "falling")
            if(plyr_rb.velocity.y < 17f)
                yield break;

        if(anim_bool == "bareerJump")
        {
            plyr_bareerJumping = false;
            if(horizontal_breerJmping)
                horizontal_breerJmping = false;
            if(lateral_breerJmping)
                lateral_breerJmping = false;
        }
        
        if(anim_bool == "launcherJmp")
            plyr_launchJumping = false;

        // reload
        if(anim_bool == "reload")
        {
            // reset upper layer weight
            // _anim.SetLayerWeight(1, 0f);

            //AnimatorController _anim_controller = _anim.runtimeAnimatorController as AnimatorController;
            //UnityEditor.Animations.AnimatorControllerLayer[] anim_layers = _anim_controller.layers;
            // remove lowerBoddy mask layer
            // 0-index lower body layer
            //_anim_controller.layers[0].avatarMask = null;

            plyr_reloading = false;
        }

        _anim.SetBool(anim_bool, false);
    }




    private void fix_Cldrs_pos(float y_off_pos, bool default_)
    {
        Collider[] colList = transform.GetChild(3).GetComponentsInChildren<Collider>();
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




    // OBSTACLE ANIMATION //
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
                // FindObjectOfType<CameraMovement>().obs_offset();


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
        yield return new WaitForSeconds(0.16f);
        lft_Straf = rght_Straf = false;
    }



    // PLAYER KILL //
    public void player_kill()
    {
        int a =  UnityEngine.Random.Range(1, 10);
        int b =  UnityEngine.Random.Range(-10, -1);

        int rdm_ = UnityEngine.Random.Range(1, 3) == 1 
            ? a : Math.Abs(b);

        if(plyr_flying && !plyr_sliding && !plyr_wallRninng && !plyr_railSliding && !plyr_jumping
            && !plyr_animKilling && !plyr_downDashing
        )
        {
            cm_movement.kill_am();
            _anim.SetInteger("killAm", rdm_);
            StartCoroutine(Dly_bool_anm(1f, "animKill"));

            action_momentum += 2f;

            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z);

            plyr_rb.AddForce( new Vector3(
                0f,
                17f,
                0f
            ), ForceMode.VelocityChange);

            StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.05f));
        }

        psCollisions_movement.player_paricleArray(null, true, "animKill");
    }





    // RELOAD //
    public void player_reload()
    {
        // stored reload anim frameCounts [running at 60 FPS] //
        // | frames
        // |-------- = anim_time
        // |   60
        // |                 (   anim_time   )
        // |converted time = (---------------) x (anim_time)
        // |                 (  reloadSpeed  )

        int[] reload_animFrameCounts = new int[3]{215, 175, 400};
        float anim_time = (float) reload_animFrameCounts[weapon_reloadType]  / (60f * (weapon_reloadTime)) ; // FOR 1s COEFF TIME

        // _anim.SetLayerWeight(1, 1f);
        plyr_reloading = true;
        
        // AnimatorController _anim_controller = _anim.runtimeAnimatorController as AnimatorController;
        // UnityEditor.Animations.AnimatorControllerLayer[] anim_layers = _anim_controller.layers;
        // RuntimeAnimatorController _anim_controller =  _anim.runtimeAnimatorController;

        // 0-index lower body layer

        //_anim_controller.layers[0].avatarMask = lower_body_mask;
        // s_anim_controller["UpperBody"].avatarMask = lower_body_mask;

        _anim.SetFloat("reload_Speed", anim_time); // 1s * reloadTime
        _anim.SetInteger("reload_Type", weapon_reloadType);

        StartCoroutine(Dly_bool_anm(weapon_reloadTime, "reload"));
    }





    // ---------------
    //  EQUIP WEAPON 
    // ---------------
    public void playerEquip_weapon(bool v_)
    {
        if(plyr_equiping)
            return;
                
        if(!v_) // UN-EQUIP
        {
            setAimSettings(false);
            animateCollision("emptyEnemyAim", Vector3.zero);
            StartCoroutine(Dly_bool_anm(0.8f, "gunUnEquipping"));
            StartCoroutine(player_weaponScrpt.weapon_unequip(0.4f));
            // _anim.SetBool("gunEquipped", false);  <== DELAYED
        }
        else // EQUIP
        {
            // weapon_equipped = true; => DELAYED
            StartCoroutine(Dly_bool_anm(0.7f, "gunEquipping"));
            player_weaponScrpt.equip_Weapon(false);
            _anim.SetBool("gunEquipped", true);
        }
    }




    // ------------
    //    COMBO
    // ------------
    private void combo(string cls_type, GameObject optional_gm)
    {
        if(cls_type == "newEnemyAim" || cls_type == "emptyEnemyAim" 
            || combo_reset > 0f)
            return;

        List<string> combo_hits = new List<string>(new string[10]{
          "bareer", "ladderHit", "sideHangHit", "fallBoxHit", "hang",
          "railSlide", "sliderHit", "tapTapJump", "bumper", "launcherHit"
        } );
        List<string> combo_breakers = new List<string>(new string[2] {"obstacleHit", "groundHit"} );

        // break
        if(combo_breakers.Contains(cls_type) && combo_ > 0)
        {
            if(plyr_sliding && cls_type == "groundHit")
                return;

            combo_reset = 2f;
            combo_ = 0;
            g_ui.combo_hit(true);
        }

        // hit
        if(plyr_downDashing)
        {
            if(combo_hits.Contains(cls_type))
            {
                if(optional_gm == null)
                    return;

                if(optional_gm.GetInstanceID() != last_comboId_)
                {
                    combo_++;
                    int is_s = g_ui.combo_hit(false);

                    if(is_s == 1)
                        psCollisions_movement.player_paricleArray(null, true, "combo");

                    last_comboId_ = optional_gm.GetInstanceID();
                    /*
                    if(cls_type == "slideHit") 
                        //momentum_ += 4f;
                    if(cls_type == "railSlide") 
                        //momentum_ += 2f;
                    // if(cls_type == "tapTapJump") momentum_ += 6f;
                    if(cls_type == "bumper" || cls_type == "launcherHit") 
                        //momentum_ += 2f; */
                }
            }
        }
    }




    // GAMEOVER //
    public IEnumerator game_Over(string mode, GameObject optional_wall = null)
    {
        if(!gameOver_)
        {
            gameOver_ = true;

            ////////////////////////////////////
            //     PLAYER DEATH ANIMATION     //
            if(mode != "void")
            {
                _anim.SetLayerWeight(1, 0f);
                _anim.SetBool("flying", true);
                List<string> anim_bools = new List<string>(new string[21]
                {
                    "slide", "wallRun", "railSlide", "rampSlide", // modules
                        "tyro", "climb", "reload", "obstacleJump", // others
                "bump", "tapTapJump", "bumper", "launcherHit", "falling", "hang", "ladder",  "underSlide", // interacts
                        "Jump", "DoubleJump", "dashDown", "GroundHit", // jumps
                                        "animKill" // animkill
                } );
                for(int i = 0; i < 21; i ++)
                {
                    StopCoroutine(Dly_bool_anm(0f, anim_bools[i]));
                    _anim.SetBool(anim_bools[i], false);
                }
                _anim.SetBool("dead", true);


                Physics.gravity = new Vector3(0, -15.0f, 0);

                if(mode == "front")
                {
                    // Vector3 newDirection = Vector3.RotateTowards(
                    //     transform.forward, 
                    //     optional_wall.transform.position - transform.position, 
                    // Time.deltaTime, 0.0f);
                    // transform.rotation = Quaternion.LookRotation(newDirection);
                    //_anim.SetInteger("deathMode", 0);
                    _anim.SetInteger("deathMode", UnityEngine.Random.Range(-1, 1));
                    plyr_rb.AddForce( new Vector3(0, 35f, -5f + (-1 * plyr_rb.velocity.z) ), ForceMode.VelocityChange);
                }
                if(mode == "damage")
                {
                    plyr_rb.AddForce( new Vector3(0, 10f, 0f), ForceMode.VelocityChange );
                    _anim.SetInteger("deathMode", UnityEngine.Random.Range(1, 7));
                }
                if(mode == "turretCollision")
                {
                    // transform.rotation = Quaternion.Euler
                    plyr_rb.velocity = Vector3.zero;
                    plyr_rb.AddForce(new Vector3(0f, 7f, -10f), ForceMode.VelocityChange);
                    _anim.SetInteger("deathMode", -3);   
                }
                if (mode == "sideVoid")
                {
                    _anim.SetInteger("deathMode", -2);
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y / 2, plyr_rb.velocity.z);
                    plyr_rb.AddForce( new Vector3(plyr_rb.velocity.x, 5f, 10f), ForceMode.VelocityChange );
                }

                cm_movement.cam_death(mode);
            }
            
            ////////////////////////////////////
            //         LOCK-ON RAGDOLL        //
            //        RAGDOLL ACTIVATION      //
            yield return new WaitForSeconds(mode == "void" ? 0f : 
                (mode == "front" ? 1f : 1.5f));
            cm_movement.set_cam_gameOver_mode = 1;

            Physics.gravity = new Vector3(0, -7f, 0);

            psCollisions_movement.set_playerRagdoll(true);
            cm_movement.set_gameOver_cam = true;
            ////////////////////////////////////
            //          RESTART-IDLE          //
            yield return new WaitForSeconds(2f);

            cm_movement.set_cam_gameOver_mode = 2;
            cm_movement.cam_gameOver(mode);

            g_ui.restart_ui(true);
            ////////////////////////////////////
        }
    }



}
