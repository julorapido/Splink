using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Buildings : MonoBehaviour
{
    public GameObject[] buildngs_prefb;
    private float x_pos = 0.0f;
    private float z_pos = 0.0f;
    private float fnc_gn_w = 80.0f;
    // Start is called before the first frame update
    private void Start(){
        Gen_Bldngs(20);
    }

    private void Gen_Bldngs(int z_len)
    {
        int ln_ = buildngs_prefb.Length;
        // Z MAP
        for(int p = 0; p < z_len; p++){
            x_pos = 0.0f;
            // X MAP
            for(int i = 0; i < ln_; i ++){
                if(x_pos > fnc_gn_w){break;}
                float spc_to_fll = 80.0f;
                GameObject sl = buildngs_prefb[i];
                Vector2 bld_sze = new Vector2(0, 0); // Type of => Vect2 [width(x-axis), profondeur(z-axis)]
                float bld_wdth = 0.0f;

                //////////////////// BLDG SIZE ////////////////////
                Collider[] sl_coldrs = sl.transform.GetComponentsInChildren<Collider>();
                for (int j = 0; j < sl_coldrs.Length; j ++){
                    string[] coldr_type = sl_coldrs[j].GetType().ToString().Split('.');
                    BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr;

                    Vector3 _sze = new Vector3(0, 0, 0); // Box => Vect3
                    float _rds = 0.0f; // Sphere => Radius
                    Vector2 _msh_wdt = new Vector2(0, 0); // Mesh => Vect2 [width(x-axis), profondeur(z-axis)]

                    switch(coldr_type[1]){
                        case "BoxCollider" :
                            b_cldr = sl.GetComponent<BoxCollider>();
                            if(b_cldr){
                                _sze = b_cldr.size;

                                if(bld_wdth < _sze.x){ bld_wdth = _sze.x; }
                            }
                            break;
                        case "SphereCollider" : 
                            s_cldr = sl.GetComponent<SphereCollider>();
                            if(s_cldr){
                                _rds = s_cldr.radius;

                                if(bld_wdth < _rds){ bld_wdth = _rds; }
                            }
                            break;
                        case "MeshCollider" : 
                            m_cldr = sl.GetComponent<MeshCollider>();
                            if(m_cldr){
                                Mesh sharedMsh_ = m_cldr.sharedMesh;
                                if(sharedMsh_){
                                    float[] x_values = new float[sharedMsh_.vertices.Length];
                                    float[] z_values = new float[sharedMsh_.vertices.Length];
                                    for(var x = 0; x < sharedMsh_.vertices.Length; x ++){
                                        x_values[x] = sharedMsh_.vertices[x].x;
                                        z_values[x] = sharedMsh_.vertices[x].z;
                                    }
                                    Array.Sort(x_values);
                                    _msh_wdt = new Vector2(Math.Abs(x_values[0]) + x_values[x_values.Length - 1],
                                        Math.Abs(z_values[0]) + z_values[z_values.Length - 1]
                                    );

                                    if(bld_wdth < _msh_wdt.x){ bld_wdth = _msh_wdt.x; }
                                }    
                            }
                            break;  
                    }
                    // if(bld_wdth < _sze.x || bld_wdth < _rds || bld_wdth < _msh_wdt.x){
                    //     bld_sze.x = _sze.x > 0 ? _sze.x : _msh_wdt.x;
                    //     bld_sze.y = _sze.z > 0 ? _sze.z : _msh_wdt.y;
                    //     bld_wdth = _rds > 0 ? _rds : (_sze.x > 0 ? _sze.x : _msh_wdt.x);
                    // }
                }
                /////////////////////////////////////////////////
                //Debug.Log(bld_wdth);
                x_pos += bld_wdth;
            } 
            z_pos += 5.0f;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
