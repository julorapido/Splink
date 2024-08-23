//
//  PlayerMovement.cs
//  PlayerMovement
//
//  Created by Jules Sainthorant on 01/08/2023.
//  Copyright © 2023 Sainthorant Jules. All rights reserved.
//
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
    [Header ("Transforms & Rb")]
    [SerializeField] private GameObject plyr_;
    [SerializeField] private Rigidbody plyr_rb;
    [SerializeField] private Transform plyr_trsnfm;
    [SerializeField] private Transform plyr_cam;


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
    private const float jumpAmount = 30f;
    private const float strafe_speed = 0.225f;
    private const float player_speed = 7f;
    private const float tyro_speed = 28f;
    private float railSlide_speed = 0.5f; // 1f


    [Header ("Player Movements Status")]
    // [#0 fly]
    private bool plyr_flying = false;

    // [#1 jumps]
    private bool plyr_jumping = false, plyr_downDashing = false;

    // #2 [wallrun, slide, rampslide, saveclimb ...]
    private bool plyr_rampSliding = false, plyr_wallRninng = false, plyr_wallExiting = false;
    public bool plyr_sliding = false;

    // #3 [railSlide, tyro, saveclimbing, obstacles]
    [HideInInspector] public bool plyr_saveClimbing = false, plyr_tyro = false, plyr_railSliding = false;
    private bool plyr_obstclJmping = false, plyr_swnging = false, plyr_intro_tyro = false;

    // #4 interact jumps [boxFall, hang, sideHang, rails, underslide, bareer, ladder...]
    [HideInInspector] public bool plyr_hanging = false;
    private bool plyr_underSliding = false, plyr_boxFalling = false, plyr_launchJumping = false, plyr_climbingLadder = false;
    private bool plyr_bareerJumping = false, plyr_bumping = false, plyr_tapTapJumping = false;
    private bool plyr_sideHanging = false, plyr_sideExitHanging = false;

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

    [Header ("Hang Vars")]
    private GameObject hang_obj;
    private GameObject moving_hangTraveler; // hang
    private GameObject moving_camTraveler; // ladder


    [Header ("TapTap, SideHang, Fallbox, Bareer, SaveClimbing, Obstacle && Hang Vars")]
    /////////////////////   vars     //////////////////////////
    ///// Hang
    private bool sHng_leftSide = false;
    private bool anim_bool_hang = false;
    ///// TapTap
    private bool tapTap_exited = false;
    ///// Fallbox
    private int last_landBoxId = 0;
    private int last_jumpedOut_landBoxId = 0;
    ///// Bareer
    private bool front_notRegister_bareer = false;
    private int bareer_j_type = 0;
    ///// Sidehang
    private GameObject side_hang_obj = null;
    ///// Hang
    private GameObject hang_bar = null;
    ///// Obstacle
    private GameObject last_ObstJumped;
    ///// SaveClimbing
    private float rot_y_saveClimbing = 0f;

    [Header ("Game State")]
    private bool gameOver_ = false;

    [Header ("WallRun Forced Strafes")]
    private bool lft_Straf = false;
    private bool rght_Straf = false;


    [Header ("WEAPON")]
    [HideInInspector] public int ammo = 0;
    private float weapon_reloadTime = 0f;
    private int weapon_reloadType = 0;
    private Weapon player_weaponScrpt;
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
    [SerializeField] private UnityEditor.Animations.AnimatorController _anim_controller;
    [SerializeField] private AvatarMask lower_body_mask;
    [SerializeField] private RigBuilder player_Rig;
    [SerializeField] private MultiAimConstraint[] player_aims;
    private float[] noAim_aimsWeigths = new float[4]{0.80f, 1.0f, 0.0f, 0.0f};
    private float[] autoAim_aimsWeigths = new float[4]{1.0f,  0.90f, 0f, 0.0f};
    [SerializeField] private Transform[] headAndNeck;
    private bool lastAimSettings_isArm;
    private string lastArmSetting_type;
    private bool wasLastAim_AutoAim = false;

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
    private const string authorizedShooting_s = "rifleShoot jumpFly jumpOut groundLeave pistolShoot flyArmed slide wallRun railSlideShoot hang slideDown bump";
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


    [Header ("Attached Scripts")]
    private CameraMovement cm_movement;
    private PlayerCollisions psCollisions_movement;
    private GameUI g_ui;


    [Header ("Touch Variables")]
    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered [5% width of the screen]


    [Header ("Camera")]
    private GameObject game_cam;

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
        game_cam = GameObject.FindGameObjectsWithTag("MainCamera")[0];
        player_weaponScrpt = FindObjectOfType<Weapon>();
        g_ui = FindObjectOfType<GameUI>();
        _anim = GetComponentInChildren<Animator>();
        cm_movement = FindObjectOfType<CameraMovement>();

        movement_auth = false;
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

        PlayerCollisions ar =  GameObject.FindGameObjectsWithTag("mainHitbox")[0].GetComponent<PlayerCollisions>();
        psCollisions_movement = ar;

        dragDistance = Screen.width * 4 / 100; //dragDistance is 5% width of the screen

        StartCoroutine(start_game());
    }



    // Update
    private void Update()
    {
        if(!gameOver_)
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

                        StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.67f ) );

                        StartCoroutine(Dly_bool_anm(0.65f, "DoubleJump"));
                        cm_movement.jmp(true);
                        psCollisions_movement.player_paricleArray(null, true, "dblJump");
                    }

                    // ForceMode.VelocityChange
                    plyr_rb.AddForce( new Vector3(0, 
                        (jumpCnt == 1 ? 
                            (jumpAmount * 1.2f) :
                            (plyr_flying ? jumpAmount : jumpAmount / 1.65f)
                        ),
                        0),
                    ForceMode.VelocityChange);


                    jumpCnt--;
                }


                // DASH DOWN
                if(Input.GetKey("s") && (dashDownCnt > 0) )
                {
                    StopCoroutine(delay_jumpInput( 0f ) );  StartCoroutine(delay_jumpInput( 0.5f ) );

                    plyr_downDashing = true;
                    StopCoroutine(Dly_bool_anm(0.44f, "dashDown"));
                    StartCoroutine(Dly_bool_anm(0.44f, "dashDown"));
                    cm_movement.dash_down();

                    // ForceMode.VelocityChange
                    plyr_rb.AddForce( new Vector3(0f, (jumpAmount * -0.1f), 0f), ForceMode.VelocityChange);

                    //dashDownCnt --;
                }
            }

            if(Input.GetKeyDown("e"))
            {
                if(!plyr_equiping)
                {
                    playerEquip_weapon(!(weapon_equipped));
                }
            }



            // Tyro/SlideRail
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

                    plyr_rb.AddForce( new Vector3(0, 15, 0), ForceMode.VelocityChange);
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


            // ------------------------------------------------
            // ------------------    HANG    ------------------
            // ------------------------------------------------
            if(plyr_hanging)
            {
                transform.RotateAround(hang_bar.transform.position, Vector3.left, 52 * Time.deltaTime);
            }


            // momentum
            if(momentum_ > 0)
                momentum_ -= 0.5f * Time.deltaTime;
            else
                momentum_ = 0;


            // action momentum
            if(action_momentum > 2f && action_momentum > 0)
                action_momentum -= 10f * Time.deltaTime;
            else if(action_momentum < 2f && action_momentum < 0)
                action_momentum += 10f * Time.deltaTime;
            else
                (action_momentum) = 0f;


            // combo
            if(combo_timer > 0f)
            {combo_timer -= 5 * Time.deltaTime;}

            else if (combo_timer < 0f)
            { combo_timer = 0f; }

            else
            {  }

            if(combo_reset > 0f)
            {
                combo_reset -= Time.deltaTime;
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
                player_aims[i].data.offset = new Vector3(armRig_ClipInfo == "flyArmed" ? 30f : 14f, 0, 0);
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


                if(armRig_ClipInfo == "flyArmed" || armRig_ClipInfo == "rifleShoot" || armRig_ClipInfo == "pistolShoot"
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
            // #2 off
                ((plyr_rampSliding) || (plyr_wallRninng) || (plyr_sliding) || (plyr_railSliding)) 
                                            ||
            // #3 railSlide, tyro, saveclimbing, obstacles
                (plyr_saveClimbing || plyr_tyro || plyr_intro_tyro  || plyr_hanging || 
                        plyr_obstclJmping ||  plyr_climbingLadder || plyr_underSliding || 
                plyr_boxFalling || plyr_launchJumping || plyr_bareerJumping || plyr_bumping || plyr_tapTapJumping)
            )
        );
        // Debug.Log(plyr_interacting + " = " 
        //             +
        //     (((plyr_rampSliding) || (plyr_wallRninng) || (plyr_sliding) || (plyr_railSliding)) )
        //             + "or " +
        //     ((plyr_saveClimbing || plyr_tyro || plyr_intro_tyro  || plyr_hanging || 
        //         plyr_obstclJmping ||  plyr_climbingLadder || plyr_underSliding || 
        //     plyr_boxFalling || plyr_launchJumping || plyr_bareerJumping || plyr_bumping || plyr_tapTapJumping))
        //             +  " or " +
        //     ((plyr_jumping) || (plyr_downDashing))
        // );
        // Debug.Log(
        //     plyr_saveClimbing + " or " +  plyr_tyro + " or " + plyr_intro_tyro + " or " + plyr_hanging + 
        //     " or " + plyr_obstclJmping + " or " + plyr_climbingLadder + " or " + plyr_underSliding + " or "
        //     + plyr_boxFalling + " or " + plyr_launchJumping + " or " + plyr_bareerJumping + " or " + plyr_bareerJumping + 
        //     " or " + plyr_bumping + " or " + plyr_tapTapJumping 
        // );
        // ---------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------


        
        // ---------------------------------------------------------------------------------------------------------
        //                                          Player_AirShootMode
        // ---------------------------------------------------------------------------------------------------------
        if( (_anim.GetBool("interacting")) != (plyr_interacting) )
        {
            _anim.SetBool("interacting", plyr_interacting);
            _anim.SetInteger("airShootMode", UnityEngine.Random.Range(0, 6));
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
            player_aims[0].data.offset = new Vector3(lastArmSetting_type == "flyArmed" ? 30f : 14f, 0f, 0f) + (arm_Recoil / 2);

            for(int m = 0; (m < arm_aims.Length) && (m < 2); m ++)
            {
                Vector3 v3 = Vector3.zero;

                if(lastArmSetting_type == "flyArmed" || lastArmSetting_type == "rifleShoot" || lastArmSetting_type == "pistolShoot"
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
                if(!plyr_wallRninng && !plyr_hanging && !plyr_rampSliding && !plyr_bumping && !plyr_bareerJumping
                    && !plyr_climbingLadder && !plyr_sideHanging
                )
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
                    string animClip_info = _anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                    bool armRigMaybe = ((animClip_info == "slide") || (animClip_info == "flyArmed")
                        || (animClip_info == "railSlideShoot") ||  (animClip_info == "slideDown")
                        || (animClip_info == "pistolShoot") || (animClip_info == "rifleShoot")
                        || (animClip_info == "groundLeave") || (animClip_info == "jumpOut")
                        || (animClip_info == "bump")
                        || (animClip_info == "jumpFly")
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
            if( (aimed_enemy != null) && (ammo > 0) && (plyr_flying || plyr_sliding || plyr_rampSliding))
            {
                //  FLYING
                if(plyr_flying && !plyr_sliding && !plyr_rampSliding)
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
                                Math.Abs(y_fl) > 45f ?
                                   (y_fl > 0 ? 45f : -45f) : y_fl,
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





            // ---------------------------------------------------------------------------------------------------------
            //                                             RT_AUTH
            // ---------------------------------------------------------------------------------------------------------
            // rotate back [Quaternion Slerp]
            if(!plyr_tyro && !plyr_intro_tyro && !plyr_wallRninng && !plyr_railSliding &&
                !plyr_hanging && !plyr_saveClimbing && !plyr_bareerJumping && !plyr_climbingLadder
                && !plyr_boxFalling && !plyr_sideHanging
            ) 
            {
                if (!Input.GetKey("q") && !Input.GetKey("d"))  rotate_bck();
                if(Input.GetKey("q") || Input.GetKey("d"))  rt_auth = false;

                if(rt_auth)
                {
                    if(plyr_trsnfm.rotation.eulerAngles.y >= 0.1f || plyr_trsnfm.rotation.eulerAngles.y <= -0.1f)
                    {
                        transform.localRotation = Quaternion.Slerp(plyr_trsnfm.rotation, new Quaternion(0,0,0,1), 3.0f * Time.deltaTime);
                    }
                }
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
                            plyr_rb.velocity.x, plyr_rb.velocity.y,
                            ((player_speed + momentum_ + action_momentum) * 1.40f)
                                                *
                                    (plyr_shooting ? 0.6f : 1f)
                        );
                    }

                    // [WALL RUN]
                    else if(plyr_wallRninng)
                    {
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, 
                            2.6f,
                            ((player_speed + momentum_ + action_momentum) * 1.15f)
                                            *
                                (plyr_shooting ? 0.65f : 1f)
                        );
                    }

                    // [RAMP SLIDING]
                    else if(plyr_rampSliding)
                    {
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, plyr_rb.velocity.y,
                            ((player_speed + momentum_ + action_momentum) * 1.30f)
                                                    *
                                        (plyr_shooting ? 0.8f : 1f)
                        );
                    }

                    // [TAP TAP JUMPNG]
                    else if(plyr_tapTapJumping)
                    { 
                        plyr_rb.velocity = new Vector3(
                            plyr_rb.velocity.x, plyr_rb.velocity.y,
                            (plyr_rb.velocity.z < (tapTap_exited ? (player_speed * 0.3f) : (player_speed * 0.7f)) ?
                                (tapTap_exited ? player_speed * 0.3f : player_speed * 0.7f) 
                                :
                                (plyr_rb.velocity.z)
                            )
                        );
                    }

                    // [SIDEHANG exiting]
                    else if(plyr_sideExitHanging)
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 
                            plyr_rb.velocity.y < -4 ? -4 : plyr_rb.velocity.y,
                            ((player_speed + momentum_ + action_momentum) * 1.30f)
                                                    *
                                        (plyr_shooting ? 0.2f : 0.4f)
                        );
                    }


                    // [RUNNING & FLYING]
                    else
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 
                                ((player_speed + momentum_ + action_momentum) * 1f)
                                                    *
                                    (plyr_shooting || plyr_animKilling ? 0.55f : 1f)
                        );

                        if(Input.GetKeyDown("f"))
                            FLYYY = !(FLYYY);
                        if(FLYYY)
                            plyr_rb.velocity = new Vector3(0f, plyr_rb.velocity.y < 0.60f ? 0.60f : plyr_rb.velocity.y, 0f);
                    }




                    // STRAFE FORCES
                    // LEFT
                    if ( (Input.GetKey("q") || lft_Straf ) )
                    {
                        if(!plyr_wallRninng && !plyr_saveClimbing)
                        {
                            if ( (plyr_trsnfm.rotation.eulerAngles.y >= 311.0f && plyr_trsnfm.rotation.eulerAngles.y <= 360.0f) 
                                                                                || 
                                                            (plyr_trsnfm.rotation.eulerAngles.y <= 43.0f) 
                            )
                            {
                                plyr_.transform.Rotate(0, -3.0f, 0, Space.Self);
                            }
                        }


                        float max_leftX = (plyr_shooting ? -0.7f : -1.1f) * (player_speed + momentum_ + action_momentum);

                        if(plyr_rb.velocity.x > max_leftX)
                            plyr_rb.AddForce((-4 * (Vector3.right * strafe_speed) ), ForceMode.VelocityChange);
                        else
                            plyr_rb.velocity = new Vector3(max_leftX, plyr_rb.velocity.y, plyr_rb.velocity.z);
                        
                    }


                    // RIGHT
                    if ( (Input.GetKey("d") || rght_Straf ) )
                    {

                        if(!plyr_wallRninng && !plyr_saveClimbing)
                        {
                            if ( ( Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) >= 0.0f && Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) <= 41.0f) 
                                                                                        ||
                                                                    (plyr_trsnfm.rotation.eulerAngles.y >= 309.0f)
                            )
                            {
                                plyr_.transform.Rotate(0, 3.0f, 0, Space.Self);
                            }
                        }

                      
                        float max_rightX = (plyr_shooting ? 0.7f : 1.1f) * (player_speed + momentum_ + action_momentum);

                        if(plyr_rb.velocity.x < max_rightX)
                            plyr_rb.AddForce((-4 * (Vector3.left * strafe_speed) ), ForceMode.VelocityChange);
                        else
                            plyr_rb.velocity = new Vector3(max_rightX, plyr_rb.velocity.y, plyr_rb.velocity.z);
                        
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
                            if(plyr_rb.velocity.y < -7f)
                                plyr_rb.velocity =  new Vector3(plyr_rb.velocity.x, -7f, plyr_rb.velocity.z);
                        }
                    }


                    // - - - - - - -  - - - - -
                    // DOWN DASHING Y-AXIS VelocityFix
                    // - - - - - - -  - - - - - 
                    if(plyr_downDashing)
                    {
                        if(plyr_rb.velocity.y < -10f)
                            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, -10f, plyr_rb.velocity.z);
                    }

                    if(plyr_wallRninng)
                        transform.rotation = Quaternion.identity;
                }


                // -- RAILSLIDE Cancel --
                if ((Input.GetKey("q") || Input.GetKey("d")) 
                    && plyr_railSliding)
                {
                    if(Input.GetKey("q"))
                        cm_movement.jumpOut(-1f);
                    else
                        cm_movement.jumpOut(1f);

                    plyr_railSliding = false;
                    plyr_rb.AddForce(( 
                            (Input.GetKey("q") ? 80 : -80) * (Vector3.left * strafe_speed) 
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
                && plyr_sideHanging && _anim.GetBool("sideHang"))
            {
                _anim.SetBool("sideHang", false);
                cm_movement.side_hang(true);
                cm_movement.side_hang_jumpOut();
                LeanTween.rotateLocal(pico_character, new Vector3(0, 0, 0), 1f).setEaseInSine();
                plyr_rb.AddForce(new Vector3(0, 10f, 0f), ForceMode.VelocityChange);
                movement_auth = true;
                plyr_sideHanging = false;
                plyr_rb.useGravity = true;
                StartCoroutine(Dly_bool_anm(0.85f, "specialHANG"));
            }
            // --- FALL BOX ---
            if(plyr_boxFalling)
            {
                if(plyr_rb.velocity.z < 5f)
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 5f);

            }
            // --- BAREER JUMP ---
            if(plyr_bareerJumping)
            {
                // front under
                if(plyr_rb.velocity.y < 5f && bareer_j_type == 1)
                    plyr_rb.velocity = new Vector3( plyr_rb.velocity.x, 5f, plyr_rb.velocity.z );

                // front upper
                if(plyr_rb.velocity.z < 1f && bareer_j_type == 0)
                    plyr_rb.velocity = new Vector3( plyr_rb.velocity.x, plyr_rb.velocity.y, 1f);

                // lateral
                if(plyr_rb.velocity.y < -5f && bareer_j_type == -2)
                   plyr_rb.velocity = new Vector3( plyr_rb.velocity.x, -5f, plyr_rb.velocity.z);
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
            if ( (!plyr_obstclJmping && !plyr_jumping && !plyr_animKilling && !plyr_boxFalling)
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
                player_weaponScrpt.Shoot(aimed_enemy);
                plyr_shooting = true;
            }else
            {
                plyr_shooting = false;
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





            // ---------------------------------------------------------------------------------------------------------
            //                                              TACTILE INPUTS
            // ---------------------------------------------------------------------------------------------------------
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
                            {
                                 //Right swipe
                                Debug.Log("Right Swipe");
                                if(!plyr_wallRninng){
                                   if ( (plyr_trsnfm.rotation.eulerAngles.y >= 309.0f) || ( Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) >= 0.0f && Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) <= 41.0f) )
                                   {
                                           plyr_.transform.Rotate(0, 2.50f, 0, Space.Self);
                                   }
                                }

                                if(!plyr_animKilling || true)
                                {
                                    float max_rightX = 1.25f * (player_speed + momentum_ + action_momentum);
                                    if(plyr_rb.velocity.x < max_rightX) plyr_rb.AddForce((-4 * (Vector3.left * strafe_speed) ), ForceMode.VelocityChange);
                                    else  plyr_rb.velocity = new Vector3(max_rightX, plyr_rb.velocity.y, plyr_rb.velocity.z);
                                }
                            }
                            else
                            {   //Left swipe
                                Debug.Log("Left Swipe");
                                if(!plyr_wallRninng){
                                    if ( (plyr_trsnfm.rotation.eulerAngles.y >= 311.0f && plyr_trsnfm.rotation.eulerAngles.y <= 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y <= 43.0f) )
                                    {
                                        plyr_.transform.Rotate(0, -2.50f, 0, Space.Self);
                                    }
                                }

                                if(!plyr_animKilling || true)
                                {
                                    float max_leftX = -1.25f * (player_speed + momentum_ + action_momentum);
                                    if(plyr_rb.velocity.x > max_leftX)  plyr_rb.AddForce((-4 * (Vector3.right * strafe_speed) ), ForceMode.VelocityChange);
                                    else  plyr_rb.velocity = new Vector3(max_leftX, plyr_rb.velocity.y, plyr_rb.velocity.z);
                                }
                            }
                        }

                        else
                        {
                            //the vertical movement is greater than the horizontal movement
                            if (lp.y > fp.y)  //If the movement was up
                            {   //Up swipe
                                Debug.Log("Up Swipe");
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
            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------------------




        }
    }


    // Public fnc for cllision
    public void animateCollision(string cls_type, Vector3 cls_size,  GameObject optional_gm = null)
    {

        float jumpForce = Mathf.Sqrt(4.5f * -2 * (Physics.gravity.y));
        List<string> interact_Jmps = new List<string>(new string[3] {"tapTapJump", "bumper", "launcherHit"} );


        if(gameOver_ || plyr_saveClimbing) return;

        if(interact_Jmps.Contains(cls_type))
        {
            StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.0f));
        };

        // hang special
        if((plyr_hanging) && (cls_type == "groundHit" || cls_type == "wallRunHit"))
        {
            plyr_hanging = false;
            movement_auth = true;
            pico_character.transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            //plyr_rb.AddForce(new Vector3(0f, 14f, 10f), ForceMode.VelocityChange);
            StopCoroutine(Dly_bool_anm(0.6f, "hangJump2"));
            plyr_rb.useGravity = true;
            //cm_movement.hang(true);
        }
        if((plyr_hanging) && !(cls_type == "groundHit" || cls_type == "wallRunHit"))
            return;

        combo(cls_type, optional_gm);
        // Debug.Log("COLLISION - "  + cls_type);
        switch(cls_type)
        {
            case "groundLeave":
                if(!plyr_sliding && !plyr_tapTapJumping && !plyr_railSliding && !plyr_obstclJmping
                    && (!_anim.GetBool("GroundHit"))
                    && !plyr_bareerJumping
                    && !plyr_jumping
                )
                {
                    plyr_rb.AddForce(new Vector3(0f, 10f, 0f), ForceMode.VelocityChange);
                    cm_movement.leave_ground(transform.rotation.eulerAngles.y);
                }


                _anim.SetBool("Flying", true);
                _anim.SetBool("GroundHit", false);
                plyr_flying = true;

                fix_Cldrs_pos(0.12f, true);

                break;

            case "groundHit":
                _anim.SetBool("Flying", false);
                _anim.SetBool("GroundHit", true);

                jumpCnt = 2;
                dashDownCnt = 1;

                if(plyr_saveClimbing || plyr_bareerJumping)
                    return;
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.6f));

                if (plyr_flying)
                {
                    StartCoroutine(Dly_bool_anm(0.3f, "GroundHit"));
                    fix_Cldrs_pos(-0.12f, false);

                    plyr_flying = false;
                    if(!plyr_sliding)
                        cm_movement.ground_roll();
                }
                break;

            case "obstacleHit":
                if(plyr_bareerJumping) return;

                rotate_bck();

                if(!plyr_obstclJmping && optional_gm.GetComponent<Rigidbody>() == null)
                {
                    last_ObstJumped = optional_gm;

                    StartCoroutine(Dly_bool_anm(plyr_flying ? 1.75f : 1.25f, "obstacleJump"));
                    plyr_rb.AddForce( new Vector3(0, 2, 0), ForceMode.VelocityChange);
                    StartCoroutine( obstcl_anim(
                        cls_size,
                        // Vector3.Scale(cls_size, optional_gm.transform.lossyScale),
                        optional_gm)
                    );
                }else
                {
                    if(last_ObstJumped != optional_gm) kickObst(optional_gm);
                }
                break;

            case "obstacleLeave":
                // TODO : ADD RIGIDBODT TO OBJ AND THROW IT AWAY
                break;

            case "wallRunHit":
                if(!_anim.GetBool("GroundHit") && !plyr_saveClimbing
                    && !plyr_bareerJumping && !plyr_boxFalling
                    && !plyr_sideHanging
                )
                {
                    if (plyr_saveClimbing || 
                        plyr_hanging) return;

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

                    cm_movement.wal_rn_offset(false, hitWall.transform, 0f/* y_bonus */);

                    if(sns < 0){
                        StartCoroutine(force_wallRn(true));
                        plyr_rb.AddForce(Vector3.left * 10, ForceMode.VelocityChange);
                        _anim.SetBool("wallRunSide", false);
                    }else{
                        StartCoroutine(force_wallRn(false));
                        plyr_rb.AddForce(Vector3.right * 10, ForceMode.VelocityChange);
                        _anim.SetBool("wallRunSide", true);
                    }

                    plyr_flying = true;

                    Vector3 p_ = new Vector3( sns < 0 ? -0.45f : 0.45f, 0, 0);
                    Quaternion q_ = Quaternion.Euler(0,  /*(sns <  0 ? -30f : -41f) + */ y_bonus, sns <  0 ? -47f : 47f);

                    pico_character.transform.localRotation = q_;
                    pico_character.transform.localPosition = p_;

                    psCollisions_movement.wallRun_aimBox = true;
                    psCollisions_movement.z_wallRun_aimRotation = ( (sns <  0) ? -47f : 47f );
                }
                break;

            case "wallRunExit":
                if(!plyr_saveClimbing && !plyr_bareerJumping && !plyr_boxFalling)
                {
                    if(plyr_saveClimbing)
                        return;

                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.5f));

                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 3, plyr_rb.velocity.y, plyr_rb.velocity.z);
                    plyr_rb.AddForce( new Vector3(0, 19f, 0), ForceMode.VelocityChange);
                    _anim.SetBool("Flying", true);
                    _anim.SetBool("wallRun", false);
                    plyr_wallRninng = false;

                    pico_character.transform.rotation = new Quaternion(0, 0, 0, 0);
                    pico_character.transform.localPosition = new Vector3(0, 0, 0);

                    psCollisions_movement.wallRun_aimBox = false;

                    StartCoroutine(wall_exit());
                    // cm_movement.wall_out()

                    lft_Straf = rght_Straf = false;
                }
                break;

            case "frontWallHit":
                if(
                    // !plyr_wallRninng && 
                    !plyr_saveClimbing && !plyr_climbingLadder
                        && (!plyr_bareerJumping)
                        && (!front_notRegister_bareer)
                        && (!plyr_sideHanging)
                )
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

                    is_rebord = (((top_y2 - transform.position.y) <= 3f ? (true) : (false))
                        // && (optional_gm.transform.GetSiblingIndex() )
                    );

                    if(plyr_wallRninng)
                    {
                        plyr_wallRninng = false;
                        pico_character.transform.localRotation = Quaternion.identity;
                        pico_character.transform.localPosition = Vector3.zero;   
                    }

                    if(is_rebord)
                    {
                        _anim.SetBool("climb", true);

                        movement_auth = false;
                        plyr_rb.useGravity = false;
                        plyr_saveClimbing = true;


                        float r_y = optional_gm.transform.rotation.eulerAngles.y;

                        int q = ((int)(r_y) / 45);
                        float y_bonus = q > 0 ? (r_y
                            - (q * 45)
                            + ( (r_y/ 45) - (q)
                        ) > 0.75f ?
                            (r_y/ 45) - (q) : 0
                        ) * 10 : ( r_y <= 40f ? r_y : 0);

                        rot_y_saveClimbing = y_bonus;
                        transform.rotation = Quaternion.Euler(0, y_bonus, 0f);
                        plyr_rb.velocity = new Vector3(0, 0, 0);

                        cm_movement.climbUp();

                        // rotate_bck();
                        StartCoroutine(Dly_bool_anm(1.1f, "climb"));
                        

                        LeanTween.move(gameObject,
                            new Vector3(transform.position.x, top_y2 - 0.55f , transform.position.z - 0.02f),
                        0.9f).setEaseInOutCubic();

                        plyr_flying = true;
                        _anim.SetBool("Flying", true);
                    }else
                    {
                        g_ui.gameOver_ui("front", transform.rotation);
                    }

                    _anim.SetBool("wallRun", false); 
                }
                break;

            case "sliderHit":
                plyr_sliding = true;
                _anim.SetBool("slide", true);
                rotate_bck();

                jumpCnt = 2;
                dashDownCnt = 1;

                if(!plyr_flying)
                    plyr_rb.AddForce( new Vector3(0, jumpForce * 0.25f, 0), ForceMode.VelocityChange);
                // fix_Cldrs_pos( 0.42f, true);
                break;

            case "sliderLeave":
                if(plyr_sliding)

                if(!plyr_jumping && plyr_sliding)
                {
                    cm_movement.jumpOut(transform.rotation.eulerAngles.y);
                    plyr_rb.AddForce( new Vector3(0, jumpForce * 1.0f, 0f), ForceMode.VelocityChange);
                }
                plyr_sliding = false;
                _anim.SetBool("slide", false);

           
                // fix_Cldrs_pos(-0.42f, false);

                break;


            case "launcherHit":
                plyr_rb.AddForce( new Vector3(0, jumpForce * 1.5f, 0), ForceMode.VelocityChange);
                StartCoroutine(Dly_bool_anm(0.90f, "launcherJump"));

                plyr_launchJumping = true;

                FindObjectOfType<CameraMovement>().special_jmp();
                action_momentum += 10f;
                break;

            case "tapTapJump":
                _anim.SetBool("Flying", true);

                // tapTap end special trigger
                if(optional_gm.transform.GetSiblingIndex() == 2){
                    plyr_rb.velocity = new Vector3(0, 0, plyr_rb.velocity.z/2);
                    plyr_rb.AddForce(new Vector3(0f, 10f, 0f), ForceMode.VelocityChange);
                    cm_movement.tapTapJmp(true);
                    action_momentum += 0.5f;
                    tapTap_exited = true;
                }

                if(plyr_tapTapJumping) return;

                fix_Cldrs_pos( -0.25f, true);

                plyr_tapTapJumping = true;
                plyr_rb.velocity = new Vector3(0, 0, plyr_rb.velocity.z/2);
                plyr_rb.AddForce( new Vector3(
                    0f, -2f,
                    optional_gm.transform.GetSiblingIndex() == 0 ? 3f : 16f
                ) , ForceMode.VelocityChange);

                cm_movement.tapTapJmp(false);

                rotate_bck();
                StartCoroutine(Dly_bool_anm(0.7f, "tapTapJump"));

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
                // Debug.Log("diff : " + optional_gm.transform.position.y + " vs " + transform.position.y);
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
                fix_Cldrs_pos( 0.42f, true);

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

                fix_Cldrs_pos(-0.42f, false);

                pico_character.transform.localRotation = Quaternion.identity;
                pico_character.transform.localPosition = Vector3.zero;

                special_rampDelay = true;
                break;
            case "land":

                if(_anim.GetBool("fallBox") || last_landBoxId == optional_gm.GetInstanceID()
                    || plyr_wallRninng)
                    return;

                last_landBoxId = optional_gm.GetInstanceID();
                StartCoroutine(Dly_bool_anm(1.2f, "fallBox"));
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.7f));

                _anim.SetBool("Flying", false);
                movement_auth = false;

                plyr_boxFalling = true;

                jumpCnt = 2;
                dashDownCnt = 1;

                transform.rotation = Quaternion.Euler(
                    0f,
                    Quaternion.RotateTowards(
                        transform.rotation, optional_gm.transform.rotation, 150f
                    ).eulerAngles.y, 
                    0f
                );
                cm_movement.fall_box(
                    (transform.rotation.eulerAngles.y >= 180f ?
                        (-1 * (360 - transform.rotation.eulerAngles.y)): transform.rotation.eulerAngles.y)
                    * (1f)
                );

                fix_Cldrs_pos(-0.12f, false);
                plyr_flying = false;

                Vector3 a = Vector3.MoveTowards(transform.position, optional_gm.transform.position, 100f);
                float x_f = optional_gm.transform.position.x - transform.position.x;

    
                a = new Vector3(
                    x_f * 1.2f,
                    (transform.position.y > optional_gm.transform.GetChild(0).transform.position.y ? 
                        (4f) : (15f)
                    ),
                    transform.position.z > optional_gm.transform.position.z ? a.z * 0.02f : a.z * 0.06f
                );
                // Debug.Log("AHHH JUMP " + a);
                // Debug.Log(Quaternion.RotateTowards(
                //         transform.rotation, optional_gm.transform.rotation, 100f
                //     ).eulerAngles.y);
                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, 0f);
                plyr_rb.AddForce(a, ForceMode.VelocityChange);

                break;

            case "landExit":
                fix_Cldrs_pos(0.12f, true);


                if(plyr_boxFalling || optional_gm.GetInstanceID() == last_jumpedOut_landBoxId)
                    return;

                last_jumpedOut_landBoxId = optional_gm.GetInstanceID();


                _anim.SetBool("Flying", true);
                plyr_flying = true;
                
                plyr_rb.AddForce(new Vector3(0f, 10f, 0f), ForceMode.VelocityChange);
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
                // transform.rotation = Quaternion.RotateTowards(transform.rotation, optional_gm.transform.rotation, 100f);

                break;

            case "ladderLeave":
                _anim.SetBool("ladder", false);
                plyr_climbingLadder = false;

                plyr_rb.AddForce( new Vector3(0, 15f, -10f), ForceMode.VelocityChange);

                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 3, plyr_rb.velocity.y, plyr_rb.velocity.z);

                StopCoroutine(ladder_climb(moving_camTraveler));
                StopCoroutine(Dly_bool_anm(0.5f, "ladderInputDelay"));
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.5f));

                cm_movement.ladderClimb_offst(true);
                cm_movement.special_jmp();
                break;
            case "bumper":

                float y_ = ( Math.Abs(optional_gm.transform.rotation.eulerAngles.y) > 42f ?
                    (optional_gm.transform.rotation.eulerAngles.y > 0 ?
                         42f : -42f
                    ) : optional_gm.transform.rotation.eulerAngles.y
                );
                // plyr_rb.AddTorque( new Vector3(0, y_ * 50, 0), ForceMode.VelocityChange);

                plyr_rb.AddForce( new Vector3(0, jumpForce * 1.5f, 0), ForceMode.VelocityChange);

                StartCoroutine(Dly_bool_anm(0.8f, "bumper"));
                plyr_bumping = true;

                cm_movement.bump(y_);
                action_momentum += 1f;
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

            case "bareerFrontDelay":
                front_notRegister_bareer = true;
                Invoke("bareerFrontDelayFnc", 0.7f);
                break;

            case "bareer":
                float bareer_y_rot = optional_gm.transform.rotation.eulerAngles.y;

                if(bareer_y_rot >= 60f && bareer_y_rot <= 120f)
                {
                    if(bareer_j_type == -2 || plyr_wallRninng)
                        return;
                    plyr_bareerJumping = true;
                    // LATERAL BAREER
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
                    // plyr_rb.useGravity = false;
                    StartCoroutine(Dly_bool_anm(0.8f, "bareerJump"));
                }else
                {
                    // FACE BAREER
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

                break;
            case "sideHang":
    
                _anim.SetBool("sideHang", true);
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.5f));
                movement_auth = false;
                plyr_sideHanging = true;
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

                g_ui.newEnemy_UI(aimed_enemy);
                break;

            case "emptyEnemyAim":
                head_TARGET.parent = transform; body_TARGET.parent = transform; leftArm_TARGET.parent = transform;
                leftArm_TARGET.localPosition = new Vector3(0, 0, 0);

                int rdm_ = UnityEngine.Random.Range(0, 3);
                body_TARGET.localPosition = rdm_ == 1 ? new Vector3(1.4f, -5.13f, 4.0f) : new Vector3(1.09f,-5.13f,1.74f);

                head_TARGET.localPosition = new Vector3(6.80f,-10.57f,16.06f);

                aimed_enemy = null;
                cm_movement.set_aimedTarget = null;
                break;
            case "gun":
                weapon_equipped = false;
                aimed_enemy = null;

                _anim.SetBool("gunEquipped", true);
                _anim_controller.layers[0].avatarMask = lower_body_mask;

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


    // bareer
    private void bareerFrontDelayFnc()
    {
        front_notRegister_bareer = false;
    }
    private void bareer_resetMvmnt()
    {
        movement_auth = true;
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

        if(anim_bool == "specialHANG")
            plyr_sideExitHanging = true;

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

        if(anim_bool == "specialHANG")
        {
            plyr_sideExitHanging = false;
        }
        if(anim_bool == "Jump" || anim_bool == "DoubleJump") 
            plyr_jumping = false;

        if(anim_bool == "dashDown")
            plyr_downDashing = false;


        if(anim_bool == "animKill")
        {
            plyr_animKilling = false;
            plyr_rb.AddForce( new Vector3(0f, 12f, 0f), ForceMode.VelocityChange);
        }

     
        if(anim_bool == "fallBox" || anim_bool == "ladderInputDelay")
            movement_auth = true;

        if(anim_bool == "fallBox")
        {
            plyr_boxFalling = false;
            // cm_movement.fall_box(true);
        }

        if(anim_bool == "climb")
        {
            plyr_saveClimbing = false;
            movement_auth = true;
            plyr_rb.useGravity = true;
            plyr_rb.AddForce( new Vector3(0f, 15f, -10f), ForceMode.VelocityChange);
        }


        if(anim_bool == "bumper") { plyr_bumping  = false; }
        if(anim_bool == "underSlide")
        {
            movement_auth = true;
            plyr_bumping  = false;
            plyr_underSliding = false;
            plyr_rb.useGravity = true;
            plyr_rb.AddForce( new Vector3(0f, 10f, 10f), ForceMode.VelocityChange);
        }

        if(anim_bool == "tapTapJump")
        {
            if(_anim.GetBool("tapTapJump"))
            {
                plyr_tapTapJumping = false;
                fix_Cldrs_pos( 0.25f, false);
                tapTap_exited = false;
            }
        }

        if(anim_bool == "falling")
            if(plyr_rb.velocity.y < 17f)
                yield break;

        if(anim_bool == "bareerJump")
        {
            movement_auth = true;
            plyr_bareerJumping = false;
            if(bareer_j_type == -2)
                plyr_rb.useGravity = true;
        }
        
        if(anim_bool == "launcherJmp")
            plyr_launchJumping = false;

        // reload
        if(anim_bool == "reload")
        {
            // reset upper layer weight
            // _anim.SetLayerWeight(1, 0f);

            UnityEditor.Animations.AnimatorControllerLayer[] anim_layers = _anim_controller.layers;
            // remove lowerBoddy mask layer
            // 0-index lower body layer
            _anim_controller.layers[0].avatarMask = null;

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
        Rigidbody obst_rb = obst.GetComponent<Rigidbody>() == null ?
            obst.AddComponent<Rigidbody>() : obst.GetComponent<Rigidbody>();

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
        yield return new WaitForSeconds(0.1f);
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

        UnityEditor.Animations.AnimatorControllerLayer[] anim_layers = _anim_controller.layers;
        // 0-index lower body layer
        _anim_controller.layers[0].avatarMask = lower_body_mask;

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
                    plyr_rb.AddForce(new Vector3(0f, 15f, 10f), ForceMode.VelocityChange);
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
