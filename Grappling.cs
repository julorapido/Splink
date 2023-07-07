 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header ("References")]
    private PlayerMovement pm_;
    //public Transform cam;
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

    private Vector3 gplr_point;

    [Header ("Cooldown")]
    public float grpl_cd;
    private float grpl_cd_timer;

    [Header ("Input")]
    public KeyCode grapplekey;

    [Header ("Prediction Radius")]
    //public RaycastHit PredictionHit;
    public float predictionSphereCastRadius;
    //public Transform predictionPoint;

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
    }

    private void LateUpdate(){
        if (!hld_joint) return;

        if(is_grpling_)
            lr.SetPosition(0, gunTip.position);

        // if(is_grpling_)
        //     currentGrplPosition = Vector3.Lerp(currentGrplPosition, gplr_point, Time.deltaTime * 8f);
        //     lr.SetPosition(0, gunTip.position);
        //     lr.SetPosition(1, currentGrplPosition);
    }

    private void StartGrapple(){
        if(grpl_cd_timer > 0 || !can_reGrappl) return;
        is_grpling_ = true;

        RaycastHit hit;
        RaycastHit sphereCastHit;
        Physics.Raycast(plyr_pos.position, plyr_pos.forward, out hit, mx_grappl_distance, wt_grappleable);
        Physics.SphereCast(plyr_pos.position, predictionSphereCastRadius, plyr_pos.forward, out sphereCastHit, mx_grappl_distance, wt_grappleable);
        if(hit.point != Vector3.zero || sphereCastHit.point != Vector3.zero)
        {

            // DEFINE GRPL POINT
            gplr_point = hit.point != Vector3.zero ? hit.point : sphereCastHit.point;
            // GRAPPLE JUMP
               // Invoke(nameof(ExecuteGrapple), grpl_dely_time);
    
            // GRAPPLE HOLD
            hld_joint  = gameObject.AddComponent<SpringJoint>();
            hld_joint.autoConfigureConnectedAnchor = false;
            hld_joint.connectedAnchor = gplr_point;

            float dist_frm_point = Vector3.Distance(plyr_pos.position, gplr_point);

            // the distance grapple try to keep from grappl point
            hld_joint.maxDistance = dist_frm_point * 0.8f;
            hld_joint.minDistance = dist_frm_point * 0.25f;

            // cutsom values
            hld_joint.spring = 700f;
            hld_joint.damper = 7f;
            hld_joint.massScale = 8.5f;

            //
            //lr.positionCount = 2;
            lr.enabled = true;
            lr.SetPosition(1, gplr_point);
            
            // Delay Grapple
            StopCoroutine(grappleDelay(3f));
            StartCoroutine(grappleDelay(3f));
        }else{
            gplr_point = plyr_pos.position + plyr_pos.forward * mx_grappl_distance;  
            Invoke(nameof(StopGrapple), grpl_dely_time);
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

    private void StopGrapple(){
        is_grpling_ = false;
        grpl_cd_timer = grpl_cd;
        Destroy(hld_joint);
        StartCoroutine(grappleResetDelay(2f));
        //lr.positionCount = 0;
        lr.enabled = false;
    }

}
