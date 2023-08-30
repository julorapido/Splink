using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
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

    [Header ("Movement Values")]
    [SerializeField] public float jumpAmount = 4.5f;
    public float player_speed = 30;
    private float svd_speed; private float uptd_speed;
    public float strafe_speed = 30;
    // public float gravityScale = 10;
    // public float fallingGravityScale = 40;
    private Animator _anim = null;
    private CharacterController _controller = null;
    //public Animation _animTion = null;
    private int jumpCnt = 2;
    private bool canJmp_ = true;
    private bool _jumping = false;
    private bool dbl_jump_ = false;

    [Header ("Animation Center Fixes")]
    public Transform[] fix_trnsfrms;
    public GameObject[] fix_objs;
    private string[] mdfied_cldrs_nm = {"","","",""};

    [Header ("Player Movements Status")]
    private bool wt_fr_DblJmp = false;
    private bool plyr_flying = false;
    [HideInInspector] public bool plyr_sliding = false;
    private bool plyr_wallRninng = false;
    private bool plyr_obstclJmping = false;
    private bool plyr_swnging = false;
    [HideInInspector] public bool plyr_tyro = false;
    private Vector3 end_tyroPos;

    [Header ("Player Speed Informations")]
    public float plyr_speed = 0;
    // tyro stuff //
    private float last_tyro_trvld = 0.05f;
    private Transform tyro_handler_child;
    private PathCreator actual_path;
    private const float tyro_speed = 14f;
    // //// ////  //
    private Vector3 lastPosition = Vector3.zero;
    
    // WHOLE MOVEMENT BOOL (jumps, grapple, swingJump)
    private bool jmp_auth = true;
    public float swipe_offst;

    [Header ("Player Attached Partcl")]
    public ParticleSystem jump_prtcl;

    private bool gameOver_ = false;

    // Obstacle Cancel Running
    private bool stopRunning_ = false;
    private bool wait_obstcl_aft = false;

    [Header ("Rotate booleans")]
    // Bool to rotate bock confrm in [Update()]
    private int trn_back_Lean_id;
    private bool rt_done = false;
    private bool rt_auth = false;

    private Vector3[] cons_coldrs = new Vector3[10];
    private Vector3 grap_pnt = new Vector3(0,0,0);

    private void Start()
    {
       _anim = GetComponentInChildren<Animator>();
       svd_speed = player_speed; uptd_speed = player_speed;
       StartCoroutine(speed_rtn(3f));
       //_controller = GetComponent<CharacterController>();
       //Debug.Log(plyr_cldrs.Length);
       if (_anim == null){
         Debug.Log("nul animtor");
       }

        Collider[] colList = fix_trnsfrms[0].GetComponentsInChildren<Collider>();
        for (int i = 0; i < colList.Length; i ++){
            string[] coldr_type = colList[i].GetType().ToString().Split('.');
            BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr;
            switch(coldr_type[1]){
                case "BoxCollider" :
                    b_cldr = fix_objs[0].GetComponent<BoxCollider>();
                    cons_coldrs[i] =  b_cldr.center; break;
                case "SphereCollider" : 
                    s_cldr = fix_objs[0].GetComponent<SphereCollider>();
                    cons_coldrs[i] = s_cldr.center; break;
                case "MeshCollider" : 
                    break;
                    //cons_coldrs[i] = fix_objs[0].GetComponent<MeshCollider>().center;break;
            }
        }
    }

    // Update is called once per frame
    private float r;
    private void Update()
    {

        plyr_speed = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;
        if (!_anim.GetBool("Flying") && plyr_flying){
            _anim.SetBool("Flying", true);
        }

        if (Input.GetKey("q")){ swipe_offst += 0.01f;
            if(trn_back_Lean_id > 0){
                LeanTween.cancel(gameObject, trn_back_Lean_id);
            }
        }
        if (Input.GetKey("d")){ swipe_offst -= 0.01f; 
            if(trn_back_Lean_id > 0){
                LeanTween.cancel(gameObject, trn_back_Lean_id);
            }
        }

        if(!stopRunning_){
            if (Input.GetKeyDown(KeyCode.Space) && (canJmp_ || wt_fr_DblJmp) && (jumpCnt > 0) && (jmp_auth == true) &&  !_anim.GetBool("GroundHit") ){
                if(_anim.GetBool("slide")){
                    _anim.SetBool("slide", false);
                    _anim.SetBool("Flying", true);
                }
                StartCoroutine(delay_input( 0.42f));
                float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
                jumpCnt--;

                // reset y velocity for jump
                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z / 1.25f);

                // ForceMode.VelocityChange for jump strength
                plyr_rb.AddForce( new Vector3(0, (wt_fr_DblJmp ? (jumpForce * 1.6f) : jumpForce * 1.85f), 0), ForceMode.VelocityChange);
                //plyr_rb.velocity = new Vector3(plyr_rb.position.x, jumpForce, plyr_rb.position.z);
                if (wt_fr_DblJmp == true){dbl_jump_ = true;}
                
                if(wt_fr_DblJmp == false){
                    _jumping = true;
                    StartCoroutine(Dbl_Jmp_Tm(3));
                }
            }
        }

        // TYRO MVMNT
        if(plyr_tyro){
            last_tyro_trvld += (tyro_speed) * Time.fixedDeltaTime;
            plyr_trsnfm.position = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, -2.1f, -0.1f);
            tyro_handler_child.position = actual_path.path.GetPointAtDistance(last_tyro_trvld);
            Quaternion e = actual_path.path.GetRotationAtDistance(last_tyro_trvld);
            plyr_trsnfm.rotation = new Quaternion(plyr_trsnfm.rotation.x, e.y, plyr_trsnfm.rotation.z, plyr_trsnfm.rotation.w);
            tyro_handler_child.rotation = new Quaternion(tyro_handler_child.rotation.x, e.y, tyro_handler_child.rotation.z, tyro_handler_child.rotation.w);
            
            float d_end = Vector3.Distance(plyr_trsnfm.position, end_tyroPos);
            if(d_end < 8f){
                plyr_rb.AddForce( new Vector3(0, 20, 10), ForceMode.VelocityChange);
                _anim.SetBool("tyro", false);
                plyr_tyro = false;
            }
        }
    }

    private void FixedUpdate() {

        if (!gameOver_ && !plyr_tyro){

            if (!Input.GetKey("q") && !Input.GetKey("d")){
                rotate_bck();
                // REPLACED INTO CAMERAMOVEMENT.cs
            }
            if(Input.GetKey("q") || Input.GetKey("d")){ rt_auth = false; }

            if(rt_auth){
                if(plyr_trsnfm.rotation.eulerAngles.y >= 0.1f || plyr_trsnfm.rotation.eulerAngles.y <= -0.1f){
                    transform.localRotation = Quaternion.Slerp(plyr_trsnfm.rotation, new Quaternion(0,0,0,1), 3.0f * Time.deltaTime);
                }
            }
            
            if(_jumping == true){
                // _anim.SetBool("Jump", true);
                // _anim.SetBool("Jump", false);
                _jumping = false;
                //Debug.Log(plyr_trsnfm.transform.rotation.eulerAngles);
                StartCoroutine(Dly_bool_anm(0.3f, "Jump"));
            }

            if(dbl_jump_ == true){
                _jumping = false; dbl_jump_ = false;
                jump_prtcl.Play();
                StopCoroutine(Dbl_Jmp_Tm(2));
                StopCoroutine(Dly_bool_anm(0.3f, "Jump"));
                StartCoroutine(Dly_bool_anm(0.4f, "DoubleJump"));
            }

            // Obstacl Jumping && Swinging ==> stopRunning_ bool
            if (!stopRunning_){
                // MAIN MOVMNT SPEED !  MAIN MOVMNT SPEED  //
                // ForceMode.VelocityChange for persistant movementspeed

                // SLIDING SPEED
                if(plyr_sliding){
                    plyr_rb.AddForce( new Vector3(0, 0, 2 * uptd_speed), ForceMode.VelocityChange);

                // WALL RUN 
                }else if(plyr_wallRninng){
                    plyr_rb.AddForce( new Vector3(0, 1.05f, 1.10f * uptd_speed), ForceMode.VelocityChange);
                }else{
                    // DEFAULT SPEED
                    if (!Input.GetKey("q") && !Input.GetKey("d")){
                        plyr_rb.AddForce( new Vector3(0, 0, (plyr_flying && !plyr_sliding ? uptd_speed/2f : 1.30f * uptd_speed)), ForceMode.VelocityChange);
                    }
                    // STRAFE SPEED
                    if (Input.GetKey("q") || Input.GetKey("d")){
                        plyr_rb.AddForce( new Vector3(0, 0, (plyr_flying ? uptd_speed / 3.85f : uptd_speed / 1.75f )), ForceMode.VelocityChange);
                    }
                }
 
                //////////////////////////////////////////////////////////
                // 
                if (Input.GetKey("q")){
                    // LEFT ROTATION
                    if(!plyr_wallRninng){
                        if ( (plyr_trsnfm.rotation.eulerAngles.y >= 315.0f && plyr_trsnfm.rotation.eulerAngles.y <= 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y <= 47.0f) ){
                            plyr_.transform.Rotate(0, -2.50f, 0, Space.Self);
                        } 
                    }
                    // LEFT STRAFE
                    plyr_rb.AddForce((plyr_flying ?  -2.8f * (Vector3.right * strafe_speed) :  -5 * (Vector3.right * strafe_speed) ), ForceMode.VelocityChange);
                }
                if (Input.GetKey("d")){ 
                    
                    // RIGHT ROTATION
                    if(!plyr_wallRninng){
                        if ( (plyr_trsnfm.rotation.eulerAngles.y >= 311.0f) || ( Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) >= 0.0f && Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) <= 44.0f) ){
                            plyr_.transform.Rotate(0, 2.50f, 0, Space.Self);
                        }else{
                            //Debug.Log("right blocked");
                        }
                    }

                    // RIGHT STRAFE
                    plyr_rb.AddForce((plyr_flying ? 2.8f * (Vector3.right * strafe_speed) : 5 * (Vector3.right * strafe_speed)), ForceMode.VelocityChange);
                }
            }

            // Swinging Forces 
            if(stopRunning_ && plyr_swnging){
                if (Input.GetKey("q")){plyr_rb.AddForce((0.6f * (Vector3.left * strafe_speed)), ForceMode.VelocityChange);}
                if (Input.GetKey("d")){plyr_rb.AddForce((0.6f * (Vector3.right * strafe_speed)), ForceMode.VelocityChange);}
                if (Input.GetKey("d") || Input.GetKey("q")){plyr_rb.AddForce( new Vector3(0f, 0.32f, 0f), ForceMode.VelocityChange);}
                if (grap_pnt != new Vector3(0,0,0)){
                    if(plyr_trsnfm.position.z < grap_pnt.z - 3.5f){
                        plyr_rb.AddForce( new Vector3(0, 0.15f,  0.9f * uptd_speed), ForceMode.VelocityChange);
                    }else{
                        plyr_rb.AddForce( new Vector3(0, 0.35f,  1.15f * uptd_speed), ForceMode.VelocityChange);
                    }
                }
            }


        }
        if(plyr_trsnfm && rt_auth){
            if(plyr_trsnfm.rotation.eulerAngles.y >= 0.1f || plyr_trsnfm.rotation.eulerAngles.y <= -0.1f){
                transform.localRotation = Quaternion.Slerp(plyr_trsnfm.rotation, new Quaternion(0,0,0,1), 3.0f * Time.deltaTime);
            }
        }
    }


    // Public fnc for cllision 
    public void animateCollision(string cls_type, Vector3 cls_size,  GameObject optional_gm = null){
        float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
        switch(cls_type){
            case "groundLeave":
                _anim.SetBool("Flying", true);
                _anim.SetBool("GroundHit", false);
                plyr_flying = true;
                if(gameOver_ || plyr_obstclJmping || plyr_sliding){return;}
                uptd_speed = svd_speed - (svd_speed/5);
                StopCoroutine(speed_rtn(3f));
                if(mdfied_cldrs_nm[0] == ""){
                    fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.12f, true);
                    mdfied_cldrs_nm[0] = "Fly";
                }
                break;
            case "groundHit":
                wait_obstcl_aft = false;
                StopCoroutine(obstcl_aft());
                // BHOP LEAK CANCEL
                if(gameOver_ || plyr_sliding || Input.GetKeyDown(KeyCode.Space) || _anim.GetBool("GroundHit") == true || _anim.GetBool("launcherJump") ){return;}
                plyr_rb.AddForce( new Vector3(0, 0, 4), ForceMode.VelocityChange);             
                _anim.SetBool("Flying", false);
                StopCoroutine(speed_rtn(3f)); StartCoroutine(speed_rtn(3f));
                StopCoroutine(Dbl_Jmp_Tm(1)); wt_fr_DblJmp = false;
                StopCoroutine(delay_input(0.0f)); StartCoroutine(delay_input(0.7f));
                if (plyr_flying){
               
                    mdfied_cldrs_nm[0] = "";
                    if(plyr_flying){ fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.12f, false);}

                    StartCoroutine(Dly_bool_anm(0.3f, "GroundHit"));
                }
                plyr_flying = false;
                break; 
            case "obstacleHit":
                wait_obstcl_aft = false;
                rotate_bck();
                //if(plyr_flying){_anim.SetBool("skipObstAnim", true);}
                _anim.SetBool("groundHit", false); 
                if(_anim.GetBool("obstacleJump") == false && !gameOver_){
                    _anim.SetBool("Flying", false);
                    plyr_rb.AddForce( new Vector3(0, 2, 0), ForceMode.VelocityChange);
                    if(!plyr_flying){
                        StartCoroutine(Dly_bool_anm(1.25f, "obstacleJump"));
                    }
                    //StopCoroutine(delay_input( 0.46f)); StartCoroutine(delay_input( 0.46f));
                    StartCoroutine(obstcl_anim(cls_size));
                }
                break;
            case "obstacleLeave":
                // Not fluid transition so ! ! ! 
                  //plyr_flying = true; -- 
                //
                break;
            case "wallRunHit":
                if(!gameOver_ && !_anim.GetBool("GroundHit")){
                    StopCoroutine(obstcl_aft());
                    _anim.SetBool("Flying", false);
                    _anim.SetBool("wallRun", true);
                    plyr_wallRninng = true;
                }
                break;
            case "wallRunExit":
                if(!gameOver_){
                    StopCoroutine(delay_input(0.0f)); StartCoroutine(delay_input(0.5f));
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 2, plyr_rb.velocity.y, plyr_rb.velocity.z / 1.5f);
                    plyr_rb.AddForce( new Vector3(0, 19f, 0), ForceMode.VelocityChange);
                    _anim.SetBool("Flying", true);
                    _anim.SetBool("wallRun", false);
                    plyr_wallRninng = false;
                }
                break;
            case "frontWallHit":
                if(!plyr_wallRninng){
                    gameOver_ = true;
                    _anim.SetBool("frontWallHit", true);
                    FindObjectOfType<CameraMovement>().cam_GamerOver_();
                    StopCoroutine(Dly_bool_anm(0.0f, "")); jumpCnt = 0;
                    plyr_rb.AddForce( new Vector3(0, 0, -10), ForceMode.VelocityChange);

                    Collider[] colList = gameObject.transform.GetComponentsInChildren<Collider>();
                    for (int i = 0; i < colList.Length; i ++){
                        if (i != 0){
                            colList[i].enabled = false;
                        }else{
                            //colList[i].center = new Vector3(colList[i].center.x, colList[i].center.y + 0.5f, colList[i].center.z);
                        }
                    }
                }
                break;
            case "sliderHit":
                if(gameOver_){return;}
                rotate_bck();
                _anim.SetBool("groundHit", false); 
                if(plyr_flying){
                   // fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.12f);
                }
                StopCoroutine(Dbl_Jmp_Tm(1)); wt_fr_DblJmp = false;
                jumpCnt = 2; plyr_sliding = true;
                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y / 2, plyr_rb.velocity.z / 1.5f);
                plyr_rb.AddForce( new Vector3(0, -1*( jumpForce * 0.3f), 5), ForceMode.VelocityChange);
                _anim.SetBool("Flying", false); _anim.SetBool("slide", true);
                fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.42f, true);
                break;
            case "sliderLeave":
                plyr_sliding = false;
                plyr_flying = true;
                _anim.SetBool("Flying", true); _anim.SetBool("slide", false);
                plyr_rb.AddForce( new Vector3(0, jumpForce, -6), ForceMode.VelocityChange);
                fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.42f, false);    
                break;
            case "launcherHit":
                rotate_bck();
                _anim.SetBool("groundHit", false); 
                StopCoroutine(delay_input(0.0f)); StartCoroutine(delay_input(0.5f));
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.56f, 13), ForceMode.VelocityChange);
                StartCoroutine(Dly_bool_anm(0.5f, "launcherJump"));
                break;
            case "bumperJump":
                rotate_bck();
                _anim.SetBool("groundHit", false); 
                _anim.SetBool("Flying", false); 
                StopCoroutine(delay_input(0.0f)); StartCoroutine(delay_input(0.5f));
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.85f, 18), ForceMode.VelocityChange);
                StartCoroutine(Dly_bool_anm(0.5f, "bumperJump"));
                // Bonus Speed [+35%]
                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, plyr_rb.velocity.z * 1.35f);
                break;
            case "tapTapJump":
                _anim.SetBool("Flying", true); 
                rotate_bck();
                StopCoroutine(delay_input(0.0f)); StartCoroutine(delay_input(1f));
                StartCoroutine(Dly_bool_anm(0.85f, "tapTapJump"));
                if(optional_gm){
                    LeanTween.moveY(gameObject,plyr_trsnfm.position.y + (optional_gm.GetComponent<Collider>().bounds.size.y  + 0.1f), 0.320f).setEaseInSine();    
                    plyr_rb.AddForce( new Vector3(0, 0, 8), ForceMode.VelocityChange);  
                }
                break;
            default:
                break;
        }
    }

    // TYRO
    public void tyro_movement(GameObject path_obj){
        StopCoroutine(delay_input(0.0f)); StartCoroutine(delay_input(1.5f));
        if(plyr_tyro){return;}
        plyr_tyro = true;
        last_tyro_trvld = 0.05f;
        Transform prnt_ = path_obj.transform.parent;
        PathCreator[] paths_ = prnt_.GetComponentsInChildren<PathCreator>();
        actual_path = paths_[0];
        end_tyroPos = actual_path.path.GetPoint(actual_path.path.NumPoints - 1);
        //end_tyroPos = actual_path.path.GetPoint(1);

        _anim.SetBool("tyro", true);
        _anim.SetBool("launcherJump", false);
        _anim.SetBool("GroundHit", false);
        
        // TYRO HANDLER Find
        foreach(Transform child_trsf in prnt_){
            if(child_trsf.gameObject.tag == "tyro_handler"){
                // Assign transform
                tyro_handler_child = child_trsf.gameObject.GetComponent<Transform>();

                // Disable Collectible.cs
                Collectible c_;      
                // Component[] components =  child_trsf.gameObject.GetComponents(typeof(Component));
                // //foreach (T component in child_trsf.gameObject.GetComponents(typeof(Component))){
                // foreach(Component component in components) {
                //     if (component.ToString() == "Collectible"){
                //         c_ = component;
                //     }
                // }
                c_ = tyro_handler_child.gameObject.GetComponent<Collectible>();
                c_.isAnimated = false;

                // 2.6 scale for tyro handler
                tyro_handler_child.localScale = new Vector3(2.6f, 2.6f, 2.6f);
                break;
            }
        };
        
    }

    // WAIT for dbl Jump
    private IEnumerator Dbl_Jmp_Tm(float delay)
    {
        wt_fr_DblJmp = true;
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(delay);
        wt_fr_DblJmp = false;
    }

    // THROW AN ANIMATION BOOL 
    private IEnumerator Dly_bool_anm(float delay, string anim_bool)
    {
        _anim.SetBool(anim_bool, true);
        if (anim_bool == "DoubleJump" || anim_bool == "jump"){canJmp_ = false;}

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(delay);
        
        if (anim_bool == "DoubleJump" || anim_bool == "jump"){canJmp_ = true;}
        if (anim_bool == "GroundHit"){jumpCnt = 2;}
        
        _anim.SetBool(anim_bool, false);
    }

    private void fix_Cldrs_pos(Transform trnsfrm_, GameObject gm_obj, float y_off_pos, bool default_){
        Collider[] colList = trnsfrm_.GetComponentsInChildren<Collider>();
        // gm_obj is only first 3 coldrs !
        for (int i = 0; i < colList.Length; i ++){
            if(y_off_pos < 0 && colList[i].isTrigger){
                StartCoroutine(disbl_cldr(colList[i], 0.2f));
            }
            string[] coldr_type = colList[i].GetType().ToString().Split('.');
            BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr;
            CapsuleCollider c_cldr;
            switch(coldr_type[1]){
                case "BoxCollider" :
                    b_cldr = gm_obj.GetComponent<BoxCollider>();
                    b_cldr.center = !default_ ? cons_coldrs[i] : new Vector3(b_cldr.center.x, b_cldr.center.y + y_off_pos, b_cldr.center.z);
                    break;
                case "SphereCollider" : 
                    s_cldr = gm_obj.GetComponent<SphereCollider>();
                    s_cldr.center = !default_ ? cons_coldrs[i] : new Vector3(s_cldr.center.x, s_cldr.center.y + y_off_pos, s_cldr.center.z);
                    break;
                // case "CapsuleCollider" : 
                //     c_cldr = gm_obj.GetComponent<CapsuleCollider>();
                //     c_cldr.center = !default_ ? cons_coldrs[i] : new Vector3(c_cldr.center.x, c_cldr.center.y + y_off_pos, c_cldr.center.z);
                //     break;
                case "MeshCollider" : 
                    m_cldr = gm_obj.GetComponent<MeshCollider>();
                    //m_cldr.center = new Vector3(m_cldr.center.x, m_cldr.center.y + y_off_pos, m_cldr.center.z);
                    break;
            }
            //colList[i].GetType().center = new Vector3 (colList[i].bounds.center.x, colList[i].bounds.center.y + y_off_pos, colList[i].bounds.center.z);
        }
    }

    // OBSTACLE HIT ANIMATION
    private IEnumerator obstcl_anim(Vector3 cls_size){
        float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
        stopRunning_ = true;
        plyr_obstclJmping = true;
        // reset Y and Z velocity (v / 10)
        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 20, 0, 0);
        //
        if (plyr_flying){
            // Precise [ 5 * cls_size.z ] cause alrdy flying
            plyr_rb.AddForce( new Vector3(0, jumpForce * 0.3f, 6 * cls_size.z), ForceMode.VelocityChange);
            yield return new WaitForSeconds(0.15f); 
        }else{
            plyr_rb.useGravity = false;
            
            // VERTICAL MOVE
            Vector3 aft_jump_v3 = new Vector3(plyr_trsnfm.position.x, plyr_trsnfm.position.y + (cls_size.y + 0.5f), plyr_trsnfm.position.z);
            plyr_rb.AddForce( new Vector3(0, 3, -1), ForceMode.VelocityChange);             
            LeanTween.moveY(gameObject,plyr_trsnfm.position.y + (cls_size.y  + 0.1f), 0.320f).setEaseInSine();      
            yield return new WaitForSeconds(0.325f); 

            // Precise [ 7 * cls_size.z ] jump dist
            // HORIZONTAL SLIDE
            plyr_rb.AddForce( new Vector3(0, -1  * (jumpForce * 0.15f), 7f * cls_size.z), ForceMode.VelocityChange);
            yield return new WaitForSeconds(0.45f); 
                // RE-Reset velocity
                // UP FORCE ++
                plyr_obstclJmping = false;
                plyr_rb.useGravity = true;
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.20f, 0), ForceMode.VelocityChange);
                FindObjectOfType<CameraMovement>().obs_offset();
            yield return new WaitForSeconds(0.15f); 
                // FORWARD FORCE ++
                plyr_rb.AddForce( new Vector3(0, 0, 9), ForceMode.VelocityChange);
        }
        yield return new WaitForSeconds(0.25f); 
            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0, plyr_rb.velocity.z / 4);
            //StartCoroutine(obstcl_aft());
            _anim.SetBool("Flying", true);
            uptd_speed  = 4 * (svd_speed / 5); 
            plyr_flying = true;
            stopRunning_=false;    

    }


    // OBSTACLE WAIT FOR RE-HIT GROUND
    private IEnumerator obstcl_aft(){
        wait_obstcl_aft = true;
        yield return new WaitForSeconds(1.0f);
        if(wait_obstcl_aft){
            _anim.SetBool("Flying", true);
        }
    }

    private IEnumerator disbl_cldr(Collider cld, float t_){
        cld.enabled = false;
        yield return new WaitForSeconds(t_);
        cld.enabled = true;
    }

    private IEnumerator delay_input(float t_){
        jmp_auth = false; yield return new WaitForSeconds(t_); jmp_auth = true;
    }
    
    public void rotate_bck(){
        if(plyr_tyro || rt_done){
            return;
        }
        // float y_to_apply = (plyr_trsnfm.rotation.eulerAngles.y > 70.0f ?  (plyr_trsnfm.rotation.eulerAngles.y-269.0f) : -plyr_trsnfm.rotation.eulerAngles.y);
        // float twn_t = y_to_apply / 120;
        // float rt_bk_tm =  (Math.Abs(y_to_apply) / 120) * 5f;
        // Debug.Log("rt");
        // trn_back_Lean_id = LeanTween.rotate(gameObject, new Vector3(plyr_trsnfm.rotation.eulerAngles.x , 0.0f, plyr_trsnfm.rotation.eulerAngles.z), 1f).setEaseInOutCubic().id;
        // LeanTween.rotate(gameObject, new Vector3(plyr_trsnfm.eulerAngles.x, 0, plyr_trsnfm.eulerAngles.z), 0.8f);
        // rt_done = true;
        rt_auth = true;
    }

    private IEnumerator speed_rtn(float t_){
        float speed_anim_spd = 0.85f;
        float anim_tick = (1.1f - 0.85f) / 30;
 
        uptd_speed  = 4.25f * (svd_speed / 5); 
        float _spd_tick  = (svd_speed / 5) / 30;

        for (int i = 0; i < 30; i ++){
            _anim.SetFloat("speedAnim", speed_anim_spd);
            yield return new WaitForSeconds(t_ / 30);
            speed_anim_spd += anim_tick; uptd_speed += _spd_tick;
        }

    }

    // Swing Animation
    public void swing_anm(bool is_ext, Vector3 grapl_pnt){
        _anim.SetBool("flying", true);
        if(is_ext){
            fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.0f, false);
            StopCoroutine(delay_input(0.0f));
            grap_pnt = new Vector3(0,0,0);
            _anim.SetBool("swing", false);
            StartCoroutine(Dly_bool_anm(0.65f, "exitSwing"));
            plyr_swnging = false;
            stopRunning_ = false;
            return;
        }else{
            fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.52f, true);
            grap_pnt = grapl_pnt;
            plyr_rb.AddForce( new Vector3(0, 4f, 0f), ForceMode.VelocityChange);
            StopCoroutine(delay_input(0.0f)); StartCoroutine(delay_input(4f));
            _anim.SetBool("swing", true);
            plyr_swnging = true; 
            stopRunning_ = true;
            return;
        }
    }
}
