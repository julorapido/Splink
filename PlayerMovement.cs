using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject plyr_;
    public Rigidbody plyr_rb;
    public Transform plyr_trsnfm;

    [Header ("Movement Values")]
    public float jumpAmount = 35;
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

    private bool wt_fr_DblJmp = false;
    private bool plyr_flying = false;
    private bool plyr_sliding = false;
    private bool plyr_wallRninng = false;

    [Header ("Player Speed")]
    public float plyr_speed = 0;
    private Vector3 lastPosition = Vector3.zero;
    
    private bool jmp_auth = true;
    public float swipe_offst;

    [Header ("Player Attached Partcl")]
    public ParticleSystem jump_prtcl;

    private int trn_back_Lean_id;
    private bool gameOver_ = false;
    
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
    }

    // Update is called once per frame
    private void Update()
    {
        plyr_speed = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;
        if (!_anim.GetBool("Flying") && plyr_flying){
            _anim.SetBool("Flying", true);
        }

        if (Input.GetKeyUp("q") || Input.GetKeyUp("d")  &&   (!(Input.GetKeyUp("q") && !Input.GetKeyUp("d"))) ){
            rotate_bck();
        }

        if (Input.GetKeyDown(KeyCode.Space) && (canJmp_ || wt_fr_DblJmp) && (jumpCnt > 0) && (jmp_auth == true)){
            if(_anim.GetBool("slide")){
                _anim.SetBool("slide", false);
                _anim.SetBool("Flying", true);
            }
            StartCoroutine(delay_input( 0.21f));
            float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
            jumpCnt--;

            // reset y velocity
            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, 0f);

            // ForceMode.VelocityChange for jump strength
            plyr_rb.AddForce( new Vector3(0, (wt_fr_DblJmp ? (jumpForce * 2) : jumpForce), 0), ForceMode.VelocityChange);
            //plyr_rb.velocity = new Vector3(plyr_rb.position.x, jumpForce, plyr_rb.position.z);
            if (wt_fr_DblJmp == true){ Debug.Log("double jump");dbl_jump_ = true;}
            
            if(wt_fr_DblJmp == false){
                _jumping = true;
                StartCoroutine(Dbl_Jmp_Tm(3));
            }
        }
    }

    private void FixedUpdate() {

        if (!gameOver_){
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
                StartCoroutine(Dly_bool_anm(0.4f, "DoubleJump"));
            }

            // MVMNT SPEED
            // ForceMode.VelocityChange for persistant movementspeed
            if (!Input.GetKey("q") && !Input.GetKey("d")){
                plyr_rb.AddForce( new Vector3(0, 0, (plyr_flying ? uptd_speed/2f : uptd_speed)), ForceMode.VelocityChange);
            }
            if (Input.GetKey("q") || Input.GetKey("d")){
                plyr_rb.AddForce( new Vector3(0, 0, (uptd_speed/3.5f)), ForceMode.VelocityChange);
            }
            /////


            if (Input.GetKey("q")){
                if ( (plyr_trsnfm.rotation.eulerAngles.y >= 325.0f && plyr_trsnfm.rotation.eulerAngles.y <= 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y <= 37.0f) ){
                    plyr_.transform.Rotate(0, -2.75f, 0, Space.Self);
                } 
                plyr_rb.AddForce((plyr_flying ?  -2f * (Vector3.right * strafe_speed) :  -5 * (Vector3.right * strafe_speed) ), ForceMode.VelocityChange);
            }

            if (Input.GetKey("d")){ 
                if ( (plyr_trsnfm.rotation.eulerAngles.y >= 323.0f) || (plyr_trsnfm.rotation.eulerAngles.y >= 0.0f && plyr_trsnfm.rotation.eulerAngles.y <= 35.0f) ){
                    plyr_.transform.Rotate(0, 2.75f, 0, Space.Self);
                }
                plyr_rb.AddForce((plyr_flying ? 2f * (Vector3.right * strafe_speed) : 5 * (Vector3.right * strafe_speed)), ForceMode.VelocityChange);
            }
        }
    }


    // Public fnc for cllision 
    public void animateCollision(string cls_type, Vector3 cls_size){
        switch(cls_type){
            case "groundLeave":
                Debug.Log("leave");
                if(gameOver_ || plyr_sliding){return;}
                uptd_speed = svd_speed - (svd_speed/5);
                FindObjectOfType<CameraMovement>().fly_dynm(true);
                StopCoroutine(speed_rtn(3f));
                _anim.SetBool("Flying", true);
                if(mdfied_cldrs_nm[0] == ""){
                    fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.12f);
                    mdfied_cldrs_nm[0] = "Fly";
                }
                plyr_flying = true;
                break;
            case "groundHit":
                if(gameOver_ || plyr_sliding){return;}
                _anim.SetBool("Flying", false);
                FindObjectOfType<CameraMovement>().fly_dynm(false);
                StopCoroutine(speed_rtn(3f)); StartCoroutine(speed_rtn(3f));
                StopCoroutine(Dbl_Jmp_Tm(1)); wt_fr_DblJmp = false;
                if (plyr_flying){
                    StopCoroutine(delay_input(0.0f));
                    StartCoroutine(delay_input(0.3f));

                    mdfied_cldrs_nm[0] = "";
                    if(plyr_flying){ fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.12f);}

                    StartCoroutine(Dly_bool_anm(0.3f, "GroundHit"));
                }
                plyr_flying = false;
                break; 
            case "obstacleHit":
                if(_anim.GetBool("obstacleJump") == false && !gameOver_){
                    _anim.SetBool("Flying", false);
                    StartCoroutine(Dly_bool_anm(0.45f, "obstacleJump"));
                    StopCoroutine(delay_input( 0.46f)); StartCoroutine(delay_input( 0.46f));
                    StartCoroutine(obstcl_anim(cls_size));
                }
                break;
            case "obstacleLeave":
                plyr_flying = true;
                break;
            case "wallRunHit":
                if(!gameOver_){
                    _anim.SetBool("Flying", false);
                    _anim.SetBool("wallRun", true);
                    plyr_wallRninng = true;
                }
                break;
            case "wallRunExit":
                if(!gameOver_){
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
                Debug.Log("hit");
                if(plyr_flying){
                    fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.12f);
                }
                StopCoroutine(Dbl_Jmp_Tm(1)); wt_fr_DblJmp = false;
                jumpCnt = 2; plyr_sliding = true;
                float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
                plyr_rb.AddForce( new Vector3(0, jumpForce * 0.25f, -3), ForceMode.VelocityChange);
                _anim.SetBool("Flying", false); _anim.SetBool("slide", true);
                fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.42f);
                break;
            case "sliderLeave":
                plyr_sliding = false;
                Debug.Log("leave");
                _anim.SetBool("Flying", true); _anim.SetBool("slide", false);
                //float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
                //plyr_rb.AddForce( new Vector3(0, jumpForce * 0.45f, -3), ForceMode.VelocityChange);
                fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.42f);    
                break;
            default:
                break;
        }
    }

    // WAIT for dbl Jump
    private IEnumerator Dbl_Jmp_Tm(float delay)
    {
        wt_fr_DblJmp = true;
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(delay);
        wt_fr_DblJmp = false;
    }


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

    private void fix_Cldrs_pos(Transform trnsfrm_, GameObject gm_obj, float y_off_pos){
        Collider[] colList = trnsfrm_.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colList.Length; i ++){
            if(y_off_pos < 0 && colList[i].isTrigger){
                StartCoroutine(disbl_cldr(colList[i], 0.3f));
            }
            string[] coldr_type = colList[i].GetType().ToString().Split('.');
            BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr;
            switch(coldr_type[1]){
                case "BoxCollider" :
                    b_cldr = gm_obj.GetComponent<BoxCollider>();
                    b_cldr.center = new Vector3(b_cldr.center.x, b_cldr.center.y + y_off_pos, b_cldr.center.z);
                    break;
                case "SphereCollider" : 
                    s_cldr = gm_obj.GetComponent<SphereCollider>();
                    s_cldr.center = new Vector3(s_cldr.center.x, s_cldr.center.y + y_off_pos, s_cldr.center.z);
                    break;
                case "MeshCollider" : 
                    m_cldr = gm_obj.GetComponent<MeshCollider>();
                    //m_cldr.center = new Vector3(m_cldr.center.x, m_cldr.center.y + y_off_pos, m_cldr.center.z);
                    break;
            }
            //colList[i].GetType().center = new Vector3 (colList[i].bounds.center.x, colList[i].bounds.center.y + y_off_pos, colList[i].bounds.center.z);
        }
    }

    private IEnumerator obstcl_anim(Vector3 cls_size){
        if (plyr_flying){
            float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
            plyr_rb.AddForce( new Vector3(0, jumpForce * 0.25f, -5), ForceMode.VelocityChange);
        }else{
            Vector3 aft_jump_v3 = new Vector3(plyr_trsnfm.position.x, plyr_trsnfm.position.y + (cls_size.y + 0.5f), plyr_trsnfm.position.z);
            LeanTween.moveY(gameObject,plyr_trsnfm.position.y + (cls_size.y  + 0.4f), 0.175f).setEaseOutCubic();      
            yield return new WaitForSeconds(0.18f); 
            plyr_rb.AddForce( new Vector3(0, 0, 7), ForceMode.VelocityChange);             
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
    
    private void rotate_bck(){
        float y_to_apply = (plyr_trsnfm.rotation.eulerAngles.y > 70.0f ?  (plyr_trsnfm.rotation.eulerAngles.y-269.0f) : -plyr_trsnfm.rotation.eulerAngles.y);
        float twn_t = y_to_apply / 120;
        //Debug.Log(y_to_apply);
        float rt_bk_tm =  (Math.Abs(y_to_apply) / 120) * 5f;
        trn_back_Lean_id = LeanTween.rotateLocal(gameObject, new Vector3(plyr_trsnfm.rotation.eulerAngles.x , 0.0f, plyr_trsnfm.rotation.eulerAngles.z), rt_bk_tm).setEaseInOutCubic().id;
    }

    private IEnumerator speed_rtn(float t_){
        float speed_anim_spd = 0.65f;
        float anim_tick = (1.2f - 0.65f) / 30;

        uptd_speed  = 4 * (svd_speed / 5); 
        float _spd_tick  = (svd_speed / 5) / 30;

        for (int i = 0; i < 30; i ++){
            _anim.SetFloat("speedAnim", speed_anim_spd);
            yield return new WaitForSeconds(t_ / 30);
            speed_anim_spd += anim_tick; uptd_speed += _spd_tick;
        }

    }
}
