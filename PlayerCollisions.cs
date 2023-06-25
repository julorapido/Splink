using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    // Start is called before the first frame update
    //public Collider mmbr_collider;
    public  string[] colsions_values = new string[2]{"ground", "sidewall"};
    public string slcted_clsion;
    public float strt_delay;
    private bool can_trgr = false;
    //ImmutableList<string> colors = ImmutableList.Create("Red", "Green", "Blue");
    private void Start(){
        StartCoroutine(delay_trgrs(strt_delay));
    }

    private void OnTriggerEnter(Collider collision) {
        if(slcted_clsion.Length > 0 && can_trgr){
            switch (slcted_clsion){
                case "ground":
                    if(collision.gameObject.tag == "ground"){
                        FindObjectOfType<PlayerMovement>().animateCollision("groundHit");
                    }
                    break;
                case "sidewall":

                    break; 
                default:
                    break;
            }
        }
    }
    
    private void OnTriggerExit(Collider collision) {
        if(slcted_clsion.Length > 0 && can_trgr){
            switch (slcted_clsion)
            {
                case "ground":
                    if(collision.gameObject.tag == "ground"){
                        FindObjectOfType<PlayerMovement>().animateCollision("groundLeave");
                    }
                    break;
                case "sidewall":

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
