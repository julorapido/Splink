using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVectors : MonoBehaviour
{
    // Init grnds to be slippery arr that gets updated
    private GameObject[] acutal_grnds = new GameObject[15];

    [Header ("Player Inspector")]
    public Transform plyr_trsnfm;

    [Header ("Player Side Htboxs")]
    public Collider side_plyr_cldr;

    // Slippery Inspector Mat
    [Header ("Slippery Material")]
    public PhysicMaterial slippery_mat;

    [Header ("Vector Gmobj")]
    public GameObject animated_vector;

    [Header ("Gameobjects Vector to apply")]
    private GameObject[] g_v;

    [Header ("Last inheritant Gameobject")]
    private GameObject last_gm;

    private int i = 0;
    private void Start(){
        g_v = new GameObject[60];
    }
    private void FixedUpdate()
    {
        // Detect Side Bldgs
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, 30f);
        if(hitColliders.Length > 0)
        {
            foreach (var cl_ in hitColliders)
            {
                if(cl_.gameObject.tag == "ground")
                {
                    Vector3 col_sz = cl_.bounds.size;
                    if(col_sz.x > 20f && col_sz.y > 7f)
                    {
                        if(cl_?.gameObject != null)
                        {
                            g_v[i] = cl_?.gameObject;
                            i++;
                        }
                    }
                }
            }
            //apply_vector();
            i = 0;
        }
    }

    // Update and Replace slippery
    // And rotate player
    public void slippery_trigr(bool is_exit, GameObject init_gmbj)
    {
 
        

        // Htbox I/O
        //StartCoroutine(cld_reactivate(0.6f));
        side_plyr_cldr.enabled = true;

        if(is_exit) side_plyr_cldr.enabled = false;
        else
        {
            // make plyr rotate 
            float y_rt = init_gmbj.transform.rotation.eulerAngles.y;

            if(  (y_rt < -43.0f) ||  (y_rt > 43.0f) )
            {
                Quaternion n_ = Quaternion.identity;
                n_.eulerAngles = new Vector3(0, y_rt < 0f ? -37.0f : 37.0f, 0);

                plyr_trsnfm.rotation = n_;
            }else
            {
                Quaternion p_  = Quaternion.identity;
                p_.eulerAngles =  new Vector3(0, y_rt, 0);

                plyr_trsnfm.rotation = p_;
            }

        }

        if(init_gmbj == last_gm) return;
        else last_gm = init_gmbj;

        // Disable previous slippery arr
        if(acutal_grnds[0] != null)
        {
            for(int j = 0; j < acutal_grnds.Length; j++)
            {
                if(acutal_grnds[j] == null) break;
                Collider col_ = acutal_grnds[j].GetComponent<Collider>();
                col_.material = null;
            }
        }
 
        Collider pr_col_ = init_gmbj.transform.parent.gameObject.GetComponent<Collider>();
        GameObject prnt_gmbj =  ((pr_col_ != null) && (pr_col_?.tag == "ground")) ? null : init_gmbj.transform.parent.gameObject;
        
        int i = 0;
        // maybe parent is ground obj
        if(prnt_gmbj == null)
        {
            // acutal_grnds[0]
            acutal_grnds[i] = init_gmbj.transform.parent.gameObject;
            i = 1;
        }else
        {
            foreach(Transform chld_ in prnt_gmbj.transform)
            {
                Collider chld_cldrs = chld_.gameObject.GetComponent<Collider>();
                if(chld_cldrs != null)
                {
                    //if(chld_?.gameObject?.GetComponent<BoxCollider>()?.tag == "ground" || chld_?.gameObject?.GetComponent<MeshCollider>()?.tag == "ground"){
                    if(chld_cldrs?.tag == "ground")
                    {
                        acutal_grnds[i] = chld_.gameObject;
                        i++;
                    }
                }
            }
        }

        // Clear rest of prev slippery arr
        for(int p = i; p < acutal_grnds.Length; p ++){ acutal_grnds[p] = null;}

        // Apply slippery
        for (int k = 0; k < i; k++)
        {
            // get main collider
            Collider col_ = acutal_grnds[k].GetComponent<Collider>();
            col_.material = slippery_mat;
        }
    }



    private IEnumerator cld_reactivate(float t_)
    {
        yield return new WaitForSeconds(t_);
        Debug.Log("solid activation");
        side_plyr_cldr.enabled = true;
    }




    // Apply vectors to near walls
    private void apply_vector()
    {
        for (int i = 0; i < g_v.Length; i ++)
        { 

            GameObject j = g_v[i];
            if(j != null)
            {
                Vector3 k = j.GetComponent<Collider>().bounds.size;
                Vector3 c_ =  j.GetComponent<Collider>().bounds.center;
                Instantiate(
                    animated_vector,
                    new Vector3(c_.x + (k.x / 2), c_.y + (k.y / 2), c_.z),
                    new Quaternion(0,0,0,1),
                    j.transform
                );
            }else{ break; }
        }
    }

    
}
