 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    private PlayerMovement pm_;
    //public Transform cam;
    [Header ("References")]
    public Rigidbody rb_;
    public Transform plyr_pos;
    public Transform gunTip;
    public LayerMask wt_grappleable;
    public LineRenderer lr;

    [Header ("Grappling")]
    public float mx_grappl_distance;
    public float grpl_dely_time;

    [Header ("Grappnling Jump Overshoot")]
    public float overshootYAxis;

    [Header ("Grappnling Point & GM")]
    private Vector3 gplr_point;
    private GameObject gplr_gm;

    [Header ("Cooldown")]
    public float grpl_cd;
    public float max_grpl_time;
    private float grpl_cd_timer;

    [Header ("Input")]
    public KeyCode grapplekey;

    [Header ("Delay Booleans")]

    private bool is_grpling_;
    private bool activ_grapple = false;

    private SpringJoint hld_joint;
    private bool grappl_ended = false;
    private bool can_reGrappl = true;

    // Start is called before the first frame update
    private void Start()
    {
        pm_ = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown("e")) StartGrapple();
        if (Input.GetKeyUp("e") || grappl_ended) StopGrapple();

        if(grpl_dely_time > 0)
            grpl_cd_timer -= Time.deltaTime;

        // Detect Grapple Ballz
        Collider[] hitColliders = Physics.OverlapSphere(plyr_pos.position, 20f);
        if(hitColliders.Length > 0){
            foreach (var hitCollider in hitColliders)
            {
                if(hitCollider.gameObject.tag == "grapple_ball"){
                    //Debug.Log("grappl ball in sight");
                }
            }
        }
    }

    private void LateUpdate(){
        if (!hld_joint) return;

        if(is_grpling_)
            lr.SetPosition(0, gunTip.position);

    }

    private void StartGrapple(){
        if(grpl_cd_timer > 0 || !can_reGrappl) return;
        is_grpling_ = true;
        bool trgrd_ = false;

        RaycastHit[] ray_hits = new RaycastHit[(155 * 2) + 1];
        int indx_ = 1;
        Physics.Raycast(plyr_pos.position, plyr_pos.forward, out ray_hits[0], mx_grappl_distance, wt_grappleable);
        //Physics.SphereCast(plyr_pos.position, predictionSphereCastRadius, plyr_pos.forward, out sphereCastHit, mx_grappl_distance, wt_grappleable);
        for(int i = 1; i < 155; i++){// LEFT
            Physics.Raycast(plyr_pos.position + new Vector3(-i/12, 0.65f + (i/12), 0.2f), plyr_pos.forward, out ray_hits[indx_], mx_grappl_distance, wt_grappleable);
            if(ray_hits[indx_].point != Vector3.zero ){
                trgrd_ = true;
                //break;
                indx_++;
            }
        }
        for(int j = 1; j < 155; j++){// RIGHT
            Physics.Raycast(plyr_pos.position + new Vector3(j/12, 0.65f + (j/12), 0.2f), plyr_pos.forward , out ray_hits[indx_], mx_grappl_distance, wt_grappleable);
            if(ray_hits[indx_].point != Vector3.zero){
                trgrd_ = true;
                //break; 
                indx_++;
            }
        }
        for(int k = 0; k < indx_; k++){
            Vector3  p = ray_hits[k].point;
            Debug.Log(p);
        }
  
        // DEFINE GRPL POINT
        // var rng = new Random();
        // ray_hits = ray_hits.OrderBy(e => rng.NextDouble()).ToArray();
        for(int k = 0; k < indx_; k++){
            int pt_ = Random.Range(1, indx_);
            if(ray_hits[k].point != Vector3.zero){
                Vector3  p = ray_hits[k].point;
                float d_fm_p = Vector3.Distance(plyr_pos.position, p);
                if(d_fm_p > 10){
                    //if(ray_hits[k].transform.gameObject.collider.tag == "ground"){
                        gplr_point = p;
                        gplr_gm = ray_hits[k].transform.gameObject;
                        break;
                    //}
                }
            }
        }
        if(gplr_point == Vector3.zero){return;}

        // GRAPPLE JUMP
        // Invoke(nameof(ExecuteGrapple), grpl_dely_time);

        // GRAPPLE HOLD
        hld_joint  = gameObject.AddComponent<SpringJoint>();
        hld_joint.autoConfigureConnectedAnchor = false;
        hld_joint.connectedAnchor = gplr_point;

    
        float dist_frm_point = Vector3.Distance(plyr_pos.position, gplr_point);
        Debug.Log("gpl indx :   " + gplr_point + " grpl dist = " + dist_frm_point);
        //if(dist_frm_point < mx_grappl_distance / 4){lr.enabled = false;return;}

        // Player Mvmnt fnc call
        //FindObjectOfType<PlayerMovement>().grapple_anim(false);

        // the distance grapple try to keep from grappl point
        hld_joint.maxDistance = dist_frm_point * 0.8f;
        hld_joint.minDistance = dist_frm_point * 0.25f;

        // cutsom values
        hld_joint.spring = 120f; // ELASTIC STRENGTH
        hld_joint.damper = 100f;
        hld_joint.massScale = 10f;

        //
        //lr.positionCount = 2;

        lr.enabled = true;
        lr.SetPosition(1, gplr_point);
        
        // Delay Grapple
        StopCoroutine(grappleDelay(max_grpl_time));
        grappl_ended = false;
        StartCoroutine(grappleDelay(max_grpl_time));

        if(!trgrd_){
            lr.enabled = false;
        }else{
            // CALL PLAYER MOVEMENT GRAPPLE ANIMATION
            FindObjectOfType<PlayerMovement>().swing_anm(false, gplr_point);
            FindObjectOfType<CameraMovement>().grpl_offset(false, gplr_gm.transform);
        }
      
    }

    private IEnumerator grappleDelay(float t_){
        grappl_ended = false;
        yield return new WaitForSeconds(t_);
        grappl_ended = true;
    }

    private IEnumerator grappleResetDelay(float t_){
        can_reGrappl = false;
        yield return new WaitForSeconds(t_);
        can_reGrappl = true;
    }


    // // // // // // // GRAPPLE DASH // // // // // // // // // // 
    private void JumpToPosition (Vector3 targetPos, float traj_height){
        activ_grapple = true;
        rb_.velocity = CalculateJumpVelocity(plyr_pos.position, targetPos, traj_height);
        activ_grapple = false;
    }

    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight){
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
                + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        
        return velocityXZ + velocityY;
    }

    private void ExecuteGrapple(){  
        Vector3 lowestPoint = new Vector3(plyr_pos.transform.position.x, plyr_pos.transform.position.y - 1.5f, plyr_pos.transform.position.z);
        float grapplePointRelativeYPos = gplr_point.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis; // ADDING THE Y AXIS ARC VALUE

        if(grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        JumpToPosition(gplr_point,  highestPointOnArc); 
    }
    // // // // // // // // // // // // // // // // // // // // // // // // 


    
    private void StopGrapple(){
        is_grpling_ = false;
        grpl_cd_timer = grpl_cd;
        Destroy(hld_joint);
        StartCoroutine(grappleResetDelay(2f));
        //lr.positionCount = 0;
        // Player Mvmnt fnc call
        FindObjectOfType<PlayerMovement>().swing_anm(true, new Vector3(0,0,0));
        FindObjectOfType<CameraMovement>().grpl_offset(true);
        lr.enabled = false;
    }

}
