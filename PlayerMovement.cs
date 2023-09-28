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
    [SerializeField] private const float jumpAmount = 32f;
    [SerializeField] private float strafe_speed;

    [Header ("Character Animators")]
    private Animator _anim = null;
    private CharacterController _controller = null;



    [Header ("Animation Center Fixes")]
    [SerializeField] private Transform[] fix_trnsfrms;
    [SerializeField] private GameObject[] fix_objs;
    private string[] mdfied_cldrs_nm = {"","","",""};
    private Vector3[] cons_coldrs = new Vector3[10];


    [Header ("Player Movements Status")]
    private bool wt_fr_DblJmp = false;
    private bool plyr_flying = false;
    [HideInInspector] public bool plyr_sliding = false;
    private bool plyr_wallRninng = false;
    private bool plyr_obstclJmping = false;
    private bool plyr_swnging = false;
    [HideInInspector] public bool plyr_tyro = false;

    
    [Header ("Authorized Movements")]
    private bool jump_auth = true;
    private bool movement_auth = true;

    [Header ("Player Available Jumps")]
    private int jumpCnt = 2;

    [Header ("Player Attached Partcl")]
    public ParticleSystem jump_prtcl;
    public ParticleSystem doubleJump_prtcl;


    [Header ("Rotate booleans")]
    private bool rt_auth = false;


    [Header ("Tyro Vars")]
    private float last_tyro_trvld = 0.05f;
    private Transform tyro_handler_child;
    private PathCreator actual_path;
    private const float tyro_speed = 14f;
    private Vector3 end_tyroPos;

    [Header ("Grapple Vars")]
    private Vector3 grap_pnt = new Vector3(0,0,0);

    [Header ("Game State")]
    private bool gameOver_ = false;






    private void Start()
    {
       _anim = GetComponentInChildren<Animator>();
       if (_anim == null) Debug.Log("nul animtor");


       Collider[] colList = fix_trnsfrms[0].GetComponentsInChildren<Collider>();
       for (int i = 0; i < colList.Length; i ++)
       {
            string[] coldr_type = colList[i].GetType().ToString().Split('.');
            BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr;

            switch(coldr_type[1])
            {
                case "BoxCollider" :
                    b_cldr = fix_objs[0].GetComponent<BoxCollider>();
                    cons_coldrs[i] =  b_cldr.center; break;
                case "SphereCollider" : 
                    s_cldr = fix_objs[0].GetComponent<SphereCollider>();
                    cons_coldrs[i] = s_cldr.center; break;
                case "MeshCollider" : 
                    break;
            }
            
        }
    }






    // Update is called once per frame
    private void Update()
    {

        // get jump input
        // movement auth && jump auth
        if(movement_auth && jump_auth)
        {
            if ( Input.GetKeyDown(KeyCode.Space) && (jumpCnt > 0) ){

                StopCoroutine(delay_jumpInput( 0.55f ) );  StartCoroutine(delay_jumpInput( 0.57f ) );

                // JUMP
                if(jumpCnt == 2)
                {
                    StartCoroutine(Dly_bool_anm(0.55f, "Jump"));
                    FindObjectOfType<CameraMovement>().jmp(false);
                    jump_prtcl.Play();
                }

                // DOUBLE JUMP
                if(jumpCnt == 1)
                {
                    StartCoroutine(Dly_bool_anm(0.55f, "DoubleJump"));
                    FindObjectOfType<CameraMovement>().jmp(true);
                    doubleJump_prtcl.Play();
                }

                jumpCnt--;

                // reset y velocity for jump
                plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0f, plyr_rb.velocity.z / 1.25f);

                // ForceMode.VelocityChange 
                plyr_rb.AddForce( new Vector3(0, (wt_fr_DblJmp ? (jumpAmount * 1.25f) : jumpAmount), 0), ForceMode.VelocityChange);
                
            }
        }


        // TYRO MVMNT
        if(plyr_tyro)
        {
            last_tyro_trvld += (tyro_speed) * Time.fixedDeltaTime;
            plyr_trsnfm.position = actual_path.path.GetPointAtDistance(last_tyro_trvld) + new Vector3(0, -2.1f, -0.1f);
            tyro_handler_child.position = actual_path.path.GetPointAtDistance(last_tyro_trvld);

            Quaternion e = actual_path.path.GetRotationAtDistance(last_tyro_trvld);
            plyr_trsnfm.rotation = new Quaternion(plyr_trsnfm.rotation.x, e.y, plyr_trsnfm.rotation.z, plyr_trsnfm.rotation.w);
            
            tyro_handler_child.rotation = new Quaternion(tyro_handler_child.rotation.x, e.y, tyro_handler_child.rotation.z, tyro_handler_child.rotation.w);
            
            float d_end = Vector3.Distance(plyr_trsnfm.position, end_tyroPos);
            if(d_end < 8f)
            {
                plyr_rb.AddForce( new Vector3(0, 20, 10), ForceMode.VelocityChange);
                _anim.SetBool("tyro", false);
                plyr_tyro = false;

                // turn back on all movements
                movement_auth = true;
            }

        }


    }






    private void FixedUpdate()
    {

        if (!gameOver_ && !plyr_tyro)
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
            



            // movement auth disabled for [ObstacleHit, Swinging, Tyro, GrappleJump]
            if (movement_auth)
            {
                // MAIN MOVEMENT SPEED //
                
                // SLIDING SPEED
                if(plyr_sliding) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 15);
                
                // WALL RUN 
                else if(plyr_wallRninng) plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 12);
                

                // RUNNING SPEED
                else
                {
                    // DEFAULT SPEED
                    if (!Input.GetKey("q") && !Input.GetKey("d"))
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 8);
                    }

                    // STRAFE SPEED
                    if (Input.GetKey("q") || Input.GetKey("d"))
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 5);
                    }
                }


                // STRAFE FORCES
                if (Input.GetKey("q"))
                {
                    // LEFT ROTATION
                    if(!plyr_wallRninng)
                    {
                        if ( (plyr_trsnfm.rotation.eulerAngles.y >= 315.0f && plyr_trsnfm.rotation.eulerAngles.y <= 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y <= 47.0f) )
                        {
                            plyr_.transform.Rotate(0, -2.50f, 0, Space.Self);
                        } 
                    }
                    
                    // LEFT STRAFE
                    plyr_rb.AddForce((plyr_flying ?  -2.8f * (Vector3.right * strafe_speed) :  -5 * (Vector3.right * strafe_speed) ), ForceMode.VelocityChange);
                }

                if (Input.GetKey("d"))
                { 
                    
                    // RIGHT ROTATION
                    if(!plyr_wallRninng)
                    {
                        if ( (plyr_trsnfm.rotation.eulerAngles.y >= 311.0f) || ( Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) >= 0.0f && Math.Abs(plyr_trsnfm.rotation.eulerAngles.y) <= 44.0f) )
                        {
                            plyr_.transform.Rotate(0, 2.50f, 0, Space.Self);
                        }
                    }

                    // RIGHT STRAFE
                    plyr_rb.AddForce((plyr_flying ? 2.8f * (Vector3.right * strafe_speed) : 5 * (Vector3.right * strafe_speed)), ForceMode.VelocityChange);
                }
            }






            // Swinging Forces 
            if(!movement_auth && plyr_swnging)
            {
                // swing strafe
                if (Input.GetKey("q"))  plyr_rb.AddForce((0.6f * (Vector3.left * strafe_speed)), ForceMode.VelocityChange);
                if (Input.GetKey("d"))  plyr_rb.AddForce((0.6f * (Vector3.right * strafe_speed)), ForceMode.VelocityChange);

                if (Input.GetKey("d") || Input.GetKey("q"))  plyr_rb.AddForce( new Vector3(0f, 0.32f, 0f), ForceMode.VelocityChange);

                if (grap_pnt != new Vector3(0,0,0))
                {
                    if(plyr_trsnfm.position.z < grap_pnt.z - 3.5f)
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 10);
                        plyr_rb.AddForce( new Vector3(0, 0.175f, 0), ForceMode.VelocityChange);
                    }
                    else
                    {
                        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, plyr_rb.velocity.y, 7);
                        plyr_rb.AddForce( new Vector3(0, 0.35f,  0), ForceMode.VelocityChange);
                    }
                }
            }

        }

        // TYRO rotations
        if(plyr_trsnfm && rt_auth)
        {
            if(plyr_trsnfm.rotation.eulerAngles.y >= 0.1f || plyr_trsnfm.rotation.eulerAngles.y <= -0.1f) transform.localRotation = Quaternion.Slerp(plyr_trsnfm.rotation, new Quaternion(0,0,0,1), 3.0f * Time.deltaTime);
        }

    }


    // Public fnc for cllision 
    public void animateCollision(string cls_type, Vector3 cls_size,  GameObject optional_gm = null)
    {

        float jumpForce = Mathf.Sqrt(4.5f * -2 * (Physics.gravity.y));
        List<string> interact_Jmps = new List<string>(new string[3] {"tapTapJump", "bumperJump", "launcherHit"} );

        switch(cls_type)
        {
            case "groundLeave":
                _anim.SetBool("Flying", true);
                plyr_flying = true;

                if(gameOver_ || plyr_obstclJmping || plyr_sliding){return;}

                if(mdfied_cldrs_nm[0] == "")
                {
                    fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.12f, true);
                    mdfied_cldrs_nm[0] = "Fly";
                }
                break;

            case "groundHit":
                _anim.SetBool("Flying", false);
                plyr_flying = false;

                jumpCnt = 2;
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.6f));

                if (plyr_flying)
                {           
                    mdfied_cldrs_nm[0] = "";
                    if(plyr_flying){ fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.12f, false);}

                    StartCoroutine(Dly_bool_anm(0.3f, "GroundHit"));
                }
                break; 

            case "obstacleHit":
                rotate_bck();

                if(!plyr_obstclJmping)
                {
                    StartCoroutine(Dly_bool_anm(1.25f, "obstacleJump"));
                    plyr_rb.AddForce( new Vector3(0, 2, 0), ForceMode.VelocityChange);
                    StartCoroutine(obstcl_anim(cls_size));
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
                }
                break;

            case "wallRunExit":
                if(!gameOver_){
                    StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(0.5f));
                    plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 2, plyr_rb.velocity.y, plyr_rb.velocity.z);
                    plyr_rb.AddForce( new Vector3(0, 17f, 0), ForceMode.VelocityChange);
                    _anim.SetBool("Flying", true);
                    _anim.SetBool("wallRun", false);
                    plyr_wallRninng = false;
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
         
                jumpCnt = 2;
                
                plyr_rb.AddForce( new Vector3(0, jumpForce * 0.3f, 5), ForceMode.VelocityChange);
                fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.42f, true);
                break;

            case "sliderLeave":
                plyr_sliding = false;
                _anim.SetBool("slide", false);
                
                plyr_rb.AddForce( new Vector3(0, jumpForce, -6), ForceMode.VelocityChange);
                fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.42f, false);    
                break;

            case string jmp when interact_Jmps.Contains(jmp):
                StopCoroutine(delay_jumpInput(0.0f)); StartCoroutine(delay_jumpInput(1.0f));
                return;

            case "launcherHit":
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.56f, 13), ForceMode.VelocityChange);
                StartCoroutine(Dly_bool_anm(0.60f, "launcherJump"));
                break;

            case "bumperJump":
                rotate_bck();
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.85f, 18), ForceMode.VelocityChange);
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

        plyr_tyro = true;
        last_tyro_trvld = 0.05f;
        Transform prnt_ = path_obj.transform.parent;
        PathCreator[] paths_ = prnt_.GetComponentsInChildren<PathCreator>();
        actual_path = paths_[0];
        end_tyroPos = actual_path.path.GetPoint(actual_path.path.NumPoints - 1);

        _anim.SetBool("tyro", true);

        
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
        
    }




    // THROW AN ANIMATION BOOL 
    private IEnumerator Dly_bool_anm(float delay, string anim_bool)
    {
        _anim.SetBool(anim_bool, true);

        //yield on a new YieldInstruction that waits for "delay" seconds.
        yield return new WaitForSeconds(delay);
                
        _anim.SetBool(anim_bool, false);
    }



    
    private void fix_Cldrs_pos(Transform trnsfrm_, GameObject gm_obj, float y_off_pos, bool default_)
    {
        Collider[] colList = trnsfrm_.GetComponentsInChildren<Collider>();
        // gm_obj is only first 3 coldrs !
        for (int i = 0; i < colList.Length; i ++)
        {
            if(y_off_pos < 0 && colList[i].isTrigger)
            {
                StartCoroutine(disbl_cldr(colList[i], 0.2f));
            }

            string[] coldr_type = colList[i].GetType().ToString().Split('.');
            BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr; CapsuleCollider c_cldr;

            switch(coldr_type[1])
            {
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
        }
    }

    private IEnumerator disbl_cldr(Collider cld, float t_)
    {
        cld.enabled = false;
        yield return new WaitForSeconds(t_);
        cld.enabled = true;
    }




    // OBSTACLE HIT ANIMATION
    private IEnumerator obstcl_anim(Vector3 cls_size)
    {
        float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
        plyr_obstclJmping = true;
        movement_auth = false;

        // reset Y and Z velocity (v / 10)
        plyr_rb.velocity = new Vector3(plyr_rb.velocity.x / 20, 0, 0);

        if (plyr_flying)
        {
            // Precise [ 7 * cls_size.z ] cause alrdy flying
            plyr_rb.AddForce( new Vector3(0, jumpForce * 0.3f, 7 * cls_size.z), ForceMode.VelocityChange);
            yield return new WaitForSeconds(0.15f); 
        }
        else
        {
            plyr_rb.useGravity = false;
            
            // VERTICAL MOVE
            plyr_rb.AddForce( new Vector3(0, 3, -1), ForceMode.VelocityChange);             
            LeanTween.moveY(gameObject,plyr_trsnfm.position.y + (cls_size.y  + 0.1f), 0.320f).setEaseInSine();      
            yield return new WaitForSeconds(0.325f); 


            // HORIZONTAL SLIDE of Precise [ 7 * cls_size.z ] jump dist
            plyr_rb.AddForce( new Vector3(0, -1  * (jumpForce * 0.15f), 7f * cls_size.z), ForceMode.VelocityChange);

            yield return new WaitForSeconds(0.45f); 
                // RE-Reset velocity & UP FORCE ++
                plyr_rb.useGravity = true;
                plyr_rb.AddForce( new Vector3(0, jumpForce * 2.20f, 0), ForceMode.VelocityChange);
                FindObjectOfType<CameraMovement>().obs_offset();

            yield return new WaitForSeconds(0.15f); 
                // FORWARD FORCE ++
                plyr_rb.AddForce( new Vector3(0, 0, 9), ForceMode.VelocityChange);
        }

        yield return new WaitForSeconds(0.25f); 
            plyr_rb.velocity = new Vector3(plyr_rb.velocity.x, 0, plyr_rb.velocity.z / 4);
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
        if(plyr_tyro)
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
            fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.0f, false);

            jump_auth = true;

            grap_pnt = new Vector3(0,0,0);
            _anim.SetBool("swing", false);
            StartCoroutine(Dly_bool_anm(0.65f, "exitSwing"));
            plyr_swnging = false;
            return;
        }
        else
        {
            fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.52f, true);
            grap_pnt = grapl_pnt;
            plyr_rb.AddForce( new Vector3(0, 4f, 0f), ForceMode.VelocityChange);
            
            jump_auth = false;

            _anim.SetBool("swing", true);
            plyr_swnging = true; 
            return;
        }
    }

}
