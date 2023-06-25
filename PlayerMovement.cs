using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject plyr_;
    public Rigidbody plyr_rb;
    public Transform plyr_trsnfm;
    public float jumpAmount = 35;
    public float player_speed = 30;
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
    public Transform[] fix_trnsfrms;
    public GameObject[] fix_objs;
    private bool wt_fr_DblJmp = false;
    private bool plyr_flying = false;
    public float plyr_speed = 0;
    private Vector3 lastPosition = Vector3.zero;
    

    private void Start()
    {
       _anim = GetComponentInChildren<Animator>();
       //_controller = GetComponent<CharacterController>();
       //Debug.Log(plyr_cldrs.Length);
       if (_anim == null){
         Debug.Log("nul animtor");
       }
    }

    private void FixedUpdate() {
        plyr_speed = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if(_jumping == true){
            // _anim.SetBool("Jump", true);
            // _anim.SetBool("Jump", false);
            _jumping = false;
            //Debug.Log(plyr_trsnfm.transform.rotation.eulerAngles);
            StartCoroutine(Dly_bool_anm(0.3f, "Jump"));
        }
        if(dbl_jump_ == true){
            _jumping = false; dbl_jump_ = false;
            StopCoroutine(Dbl_Jmp_Tm(2));
            StartCoroutine(Dly_bool_anm(0.4f, "DoubleJump"));
        }

        // MVMNT SPEED
        // ForceMode.VelocityChange for persistant movementspeed
        plyr_rb.AddForce( new Vector3(0, 0, (plyr_flying ? player_speed/2 : player_speed)), ForceMode.VelocityChange);
        /////

        if (Input.GetKeyDown(KeyCode.Space) && (canJmp_ || wt_fr_DblJmp) && (jumpCnt > 0) ){
            float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));
            jumpCnt--;
            // ForceMode.VelocityChange for jump strength
            plyr_rb.AddForce( new Vector3(0, (wt_fr_DblJmp ? (jumpForce * 2) : jumpForce), 0), ForceMode.VelocityChange);
            //plyr_rb.velocity = new Vector3(plyr_rb.position.x, jumpForce, plyr_rb.position.z);
            if (wt_fr_DblJmp == true){ dbl_jump_ = true;}
            
            if(wt_fr_DblJmp == false){
                _jumping = true;
                StartCoroutine(Dbl_Jmp_Tm(3));
            }
      
        }

        if (Input.GetKey("q")){
            if ( (plyr_trsnfm.rotation.eulerAngles.y > 270.0f && plyr_trsnfm.rotation.eulerAngles.y < 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y > 0.0f && plyr_trsnfm.rotation.eulerAngles.y < 60.0f) ){
                plyr_.transform.Rotate(0, -1, 0, Space.Self);
            } 
            plyr_rb.AddForce( -5 * (Vector3.right * strafe_speed), ForceMode.VelocityChange);
        }

        if (Input.GetKey("d")){ 
            if ( (plyr_trsnfm.rotation.eulerAngles.y > 270.0f && plyr_trsnfm.rotation.eulerAngles.y < 360.0f) || (plyr_trsnfm.rotation.eulerAngles.y > 0.0f && plyr_trsnfm.rotation.eulerAngles.y < 60.0f) ){
                plyr_.transform.Rotate(0, 1, 0, Space.Self);
            }
            plyr_rb.AddForce( 5 * (Vector3.right * strafe_speed), ForceMode.VelocityChange);
        }
    }

    // Public fnc for cllision 
    public void animateCollision(string cls_type){
        switch(cls_type){
            case "groundLeave":
                _anim.SetBool("Flying", true);
                fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], 0.28f);
                plyr_flying = true;
                break;
            case "groundHit":
                _anim.SetBool("Flying", false);
                StopCoroutine(Dbl_Jmp_Tm(1)); wt_fr_DblJmp = false;
                if(plyr_flying){
                    fix_Cldrs_pos(fix_trnsfrms[0], fix_objs[0], -0.28f);
                }
                StartCoroutine(Dly_bool_anm(0.3f, "GroundHit"));
                plyr_flying = false;
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
}
