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
    private Collider[] plyr_cldrs = new Collider[1];
    private bool wt_fr_DblJmp = false;
    private bool plyr_flying = false;

    private void Start()
    {
       _anim = GetComponentInChildren<Animator>();
       //_controller = GetComponent<CharacterController>();
       plyr_cldrs = gameObject.transform.GetComponentsInChildren<Collider>();
       //Debug.Log(plyr_cldrs.Length);
       if (_anim == null){
         Debug.Log("nul animtor");
       }
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

        // ForceMode.VelocityChange for persistant movementspeed
        plyr_rb.AddForce( new Vector3(0, 0, player_speed), ForceMode.VelocityChange);
        float jumpForce = Mathf.Sqrt(jumpAmount * -2 * (Physics.gravity.y));

        if (Input.GetKeyDown(KeyCode.Space) && (canJmp_ || wt_fr_DblJmp) && (jumpCnt > 0) ){
            jumpCnt--;
            // ForceMode.VelocityChange for jump strength
            //plyr_rb.AddForce( new Vector3(0, jumpForce, 0), ForceMode.VelocityChange);
            plyr_rb.velocity = new Vector3(plyr_rb.position.x, jumpForce, plyr_rb.position.z);
            if (wt_fr_DblJmp == true){ dbl_jump_ = true;}
            
            if(wt_fr_DblJmp == false){
                _jumping = true;
                StartCoroutine(Dbl_Jmp_Tm(3));
            }
      
        }

        if (Input.GetKey("q")){
            if(plyr_trsnfm.rotation.eulerAngles.y > (360.0f - 70.0f) ){
                plyr_.transform.Rotate(0, -1, 0, Space.Self);
            } 
            plyr_rb.AddForce( -5 * (Vector3.right * strafe_speed), ForceMode.VelocityChange);
        }

        if (Input.GetKey("d")){ 
            if(plyr_trsnfm.rotation.eulerAngles.y > 360.0f - 270.0f){
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
                break;
            case "groundHit":
                jumpCnt = 2;
                _anim.SetBool("Flying", false);
               // StartCoroutine(Dly_bool_anm(0.4f, "Roll"));
                break; 
            default:
                
                break;
        }
    }

    // WAIT for dbl Jump
    private IEnumerator Dbl_Jmp_Tm(float delay)
    {
        Debug.Log("wait for dbl jmp");
        wt_fr_DblJmp = true;
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(delay);
        wt_fr_DblJmp = false;
    }


    private IEnumerator Dly_bool_anm(float delay, string anim_bool)
    {
        //_anim.SetFloat("fastRun", 0.99f); // End Ru n Animation Cycle
        _anim.SetBool(anim_bool, true);
        if (anim_bool == "DoubleJump" || anim_bool == "jump"){canJmp_ = false;}
        Debug.Log(anim_bool);
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(delay);
        if (anim_bool == "DoubleJump" || anim_bool == "jump"){canJmp_ = true;}
        _anim.SetBool(anim_bool, false);
    }
}
