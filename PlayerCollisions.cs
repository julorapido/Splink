using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    // Start is called before the first frame update
    //public Collider mmbr_collider;
    public  string[] colsions_values = new string[4]{"ground", "frontwall","sidewall", "slider"};
    public string slcted_clsion;
    public float strt_delay;
    private bool can_trgr = false;
    public Transform ply_transform;
    //ImmutableList<string> colors = ImmutableList.Create("Red", "Green", "Blue");
    private void Start(){
        StartCoroutine(delay_trgrs(strt_delay));
    }

    private void OnTriggerEnter(Collider collision) {
        if(slcted_clsion.Length > 0 && can_trgr){
            Vector3 _size = collision.bounds.size;
            switch (slcted_clsion){
                case "ground":
                    if(collision.gameObject.tag == "ground"){
                        FindObjectOfType<PlayerMovement>().animateCollision("groundHit", _size);
                    }
                    if(collision.gameObject.tag == "obstacle"){
                        FindObjectOfType<PlayerMovement>().animateCollision("obstacleHit", _size);
                    }
                    break;
                case "frontwall":
                    if(collision.gameObject.tag == "ground"){
                        FindObjectOfType<PlayerMovement>().animateCollision("frontWallHit", _size);
                    }
                    break;
                case "sidewall":
                    if(collision.gameObject.tag == "ground"){
                        Vector3 targetDir = collision.gameObject.transform.position - ply_transform.position;
                        float angle = Vector3.Angle(targetDir, transform.forward);
                        Debug.Log(angle);
                        FindObjectOfType<PlayerMovement>().animateCollision("wallRunHit", _size);
                    }
                    break; 
                case "slider":
                    if(collision.gameObject.tag == "slider"){
                        Debug.Log("er");
                        LeanTween.scale(collision.gameObject, collision.gameObject.transform.localScale * 1.08f, 1f).setEasePunch();
                        FindObjectOfType<PlayerMovement>().animateCollision("sliderHit", _size);
                    }  
                    break;
                default:
                    break;
            }
        }
    }
    
    private void OnTriggerExit(Collider collision) {
        if(slcted_clsion.Length > 0 && can_trgr){
            Vector3 _size = collision.bounds.size;
            switch (slcted_clsion)
            {
                case "ground":
                    if(collision.gameObject.tag == "ground"){
                        FindObjectOfType<PlayerMovement>().animateCollision("groundLeave", _size);
                        //FindObjectOfType<PlayerMovement>().animateCollision("groundLeave");
                    }
                    if(collision.gameObject.tag == "obstacle"){
                        FindObjectOfType<PlayerMovement>().animateCollision("obstacleLeave", _size);
                    }
                    break;
                case "sidewall":
                    if(collision.gameObject.tag == "ground"){
                        FindObjectOfType<PlayerMovement>().animateCollision("wallRunExit", _size);
                    }
                    break; 
                case "slider":
                    if(collision.gameObject.tag == "slider"){
                        LeanTween.scale(collision.gameObject, collision.gameObject.transform.localScale * 1.08f, 1f).setEasePunch();
                        FindObjectOfType<PlayerMovement>().animateCollision("sliderLeave", _size);
                    }  
                    break;
                default:
                    break;
            }
        }
    }

    private IEnumerator delay_trgrs(float dl){
        yield return new WaitForSeconds(dl);
        can_trgr = true;
    }
 
}
