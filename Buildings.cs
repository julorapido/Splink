using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Buildings : MonoBehaviour
{
    public GameObject[] buildngs_prefb;
    public GameObject[] sections_prefb;

    private float x_pos = 0.0f;
    private float z_pos = 0.0f;
    private float space_t_fill = 0.0f;

    // GLOBAL WIDTH OF BUILDGS GEN
    private const float fnc_gn_w = 50.0f;

    public Transform bldg_parent;
    // Start is called before the first frame update
    private void Start(){
        //Gen_Bldngs(20);
        Gen_PrefbSections(3);
    }

    private void Gen_Bldngs(int z_len)
    {
        int ln_ = buildngs_prefb.Length;
        // Z MAP
        for(int p = 0; p < z_len; p++){
            x_pos = 0.0f;
            space_t_fill = 0.0f;
            // X MAP
            for(int i = 0; i < ln_; i ++){
                if(x_pos > fnc_gn_w){break;}
                int rdm_ = UnityEngine.Random.Range(0, ln_);/// RAND bat indx
                GameObject sl = buildngs_prefb[rdm_];
                Vector2 bld_sze = new Vector2(0, 0); // Type of => Vect2 [width(x-axis), profondeur(z-axis)]
                float bld_wdth = 0.0f; // Actual Width of BLDG
                bool prnt_passed = false; bool is_prnt_cldr = false;
                int indx_scale = 0;

                //////////////////// BLDG SIZE ////////////////////
                Collider[] sl_coldrs = sl.transform.GetComponentsInChildren<Collider>();
                for (int j = 0; j < sl_coldrs.Length; j ++){

                    string[] coldr_type = sl_coldrs[j].GetType().ToString().Split('.');
                    //  //  //  //  //  //
                    Collider parent_cldr_ = sl.transform.GetComponent<Collider>();
                    
                    if(j == 0 && parent_cldr_ && !prnt_passed)
                    {
                        prnt_passed = true;
                        coldr_type = parent_cldr_.GetType().ToString().Split('.');
                        is_prnt_cldr = true;
                        j--;
                    }
                    //  //  //  //  //  // 
                    BoxCollider b_cldr; SphereCollider s_cldr; MeshCollider m_cldr;

                    Vector3 _sze = new Vector3(0, 0, 0); // Box => Vect3
                    float _rds = 0.0f; // Sphere => Radius
                    Vector2 _msh_wdt = new Vector2(0, 0); // Mesh => Vect2 [width(x-axis), profondeur(z-axis)]

                    switch(coldr_type[1]){
                        case "BoxCollider" :
                            b_cldr = is_prnt_cldr ? (sl.GetComponent<BoxCollider>()) : (sl.GetComponentsInChildren<BoxCollider>()[0]);
                            if(b_cldr)
                            {
                                _sze = b_cldr.size;

                                if(bld_wdth < _sze.x){indx_scale = j; bld_wdth = _sze.x; bld_sze = new Vector2(_sze.x, _sze.y);}
                            }
                            break;
                        case "SphereCollider" : 
                            s_cldr = is_prnt_cldr ? (sl.GetComponent<SphereCollider>()) : (sl.GetComponentsInChildren<SphereCollider>()[0]);
                            if(s_cldr)
                            {
                                _rds = s_cldr.radius;

                                if(bld_wdth < _rds){indx_scale = j; bld_wdth = _rds; }
                            }
                            break;
                        case "MeshCollider" : 
                            m_cldr = is_prnt_cldr ? (sl.GetComponent<MeshCollider>()) : (sl.GetComponentsInChildren<MeshCollider>()[0]);
                            if(m_cldr){
                                Mesh sharedMsh_ = m_cldr.sharedMesh;
                                if(sharedMsh_){
                                    float[] x_values = new float[sharedMsh_.vertices.Length];
                                    float[] z_values = new float[sharedMsh_.vertices.Length];
                                    for(var x = 0; x < sharedMsh_.vertices.Length; x ++)
                                    {
                                        x_values[x] = sharedMsh_.vertices[x].x;
                                        z_values[x] = sharedMsh_.vertices[x].z;
                                    }
                                    Array.Sort(x_values);
                                    _msh_wdt = new Vector2(Math.Abs(x_values[0]) + x_values[x_values.Length - 1],
                                        Math.Abs(z_values[0]) + z_values[z_values.Length - 1]
                                    );

                                    if(bld_wdth < _msh_wdt.x){
                                        indx_scale = j; bld_wdth = _msh_wdt.x; bld_sze = _msh_wdt;
                                    }
                                }    
                            }
                            break;  
                    }
                    if(bld_wdth < _sze.x || bld_wdth < _rds || bld_wdth < _msh_wdt.x)
                    {
                        bld_sze.x = _sze.x > 0 ? _sze.x : _msh_wdt.x;
                        bld_sze.y = _sze.z > 0 ? _sze.z : _msh_wdt.y;
                        bld_wdth = _rds > 0 ? _rds : (_sze.x > 0 ? _sze.x : _msh_wdt.x);
                    }
                }
                /////////////////////////////////////////////////

                if(is_prnt_cldr){ bld_wdth = bld_wdth * sl.transform.localScale.x; }else{
                    //bld_wdth = (bld_wdth * sl_coldrs[indx_scale].gameObject.transform.localScale.x) * sl.transform.localScale.x;
                    bld_wdth = bld_wdth  * sl.transform.localScale.x;
                }
                Instantiate(sl, new Vector3(x_pos + bld_wdth/2 , 0, z_pos), new Quaternion(0f, 90f, 0f, 1), bldg_parent);

                x_pos += bld_wdth;
            } 
            z_pos += 40.0f;
        }
    }

    private void Gen_PrefbSections(int z_len){
        int ln_ = sections_prefb.Length;
        float z_step = 19.0f; // STEP ON Z-AXIS
        float svd_z = 0.0f;
        float y_ = 0.0f;
        // Z MAP
        for(int p = 0; p < z_len; p++){
            // X MAP
            x_pos = 0f;
            for(int i = 0; i < ln_; i ++){
                if(x_pos > fnc_gn_w){break;}
                int rdm_ = UnityEngine.Random.Range(0, ln_);/// RAND Section indx 
                GameObject sl = sections_prefb[rdm_];


                // Get Box Cldr Size as whole Sect Size
                Vector3 sl_size = sl.GetComponent<BoxCollider>().size;
                
                GameObject buffer_sect = Instantiate(sl, new Vector3(x_pos - sl_size.x/2, 0 + y_, z_pos + sl_size.z / 2 + 20f), new Quaternion(0f, 0f, 0f, 1), bldg_parent);
                //GameObject[] buffer_go_arr = buffer_sect.GetComponentsInChildren<Transform>();
                if(buffer_sect.transform.childCount >= 2)
                {
                    combine_Meshes(buffer_sect, buffer_sect.transform.childCount);
                }

                x_pos += sl_size.x;
                svd_z = sl_size.z;
                y_ -= 7.5f;
            }
            z_pos += svd_z + 10.0f;
        }
    }


    private void combine_Meshes(GameObject section_parent, int chld_len){
        MeshFilter? filter_Reference = null;
        int filter_Ref_chldPos = 0;
        Debug.Log(section_parent);

        for (int j = 0; j < chld_len; j ++)
        {
            GameObject chld_ = section_parent.transform.GetChild(j).gameObject;

            //IF PARENT IS HIS OWN MESH RENDERER
            if(chld_.GetComponent<MeshFilter>() != null) continue;

            MeshFilter[] meshFilters_ = new MeshFilter[chld_.transform.childCount];
            for (int c = 0; c < chld_.transform.childCount; c ++)
            {
                MeshFilter chld_c = chld_.transform.GetChild(c).GetComponent<MeshFilter>();
                if(chld_c != null){
                    meshFilters_[c] = chld_c;
                }
            }
            //GameObject[] chld_arr = chld_.GetComponentsInChildren<GameObject>();
            //MeshFilter[] meshFilters_ = chld_.GetComponentsInChildren<MeshFilter>();
            //for(int z = 0; z < meshFilters_.Length; z ++) Debug.Log(meshFilters_[z]);
            if(meshFilters_.Length < 2)
            {
                continue;
            }
            CombineInstance[] cmb_inst = new CombineInstance[meshFilters_.Length];
            Debug.Log(chld_);
            int i = 0, v = 0, x = 0;
            while (i < meshFilters_.Length)
            {
                if (i < (meshFilters_.Length - 1)  && filter_Reference != null)
                {
                    if( ( (meshFilters_[i]?.sharedMesh) != filter_Reference?.sharedMesh) ) 
                    {
                        i++;
                        continue;
                    } 
                }

                // REFERENCE ASSIGNATION
                if( filter_Reference == null && meshFilters_[i]?.gameObject.tag == "ground") {
                    filter_Reference = meshFilters_[i];
                    filter_Ref_chldPos = i;
                    Debug.Log(filter_Reference.mesh.subMeshCount);
                    continue;
                };

                if(filter_Reference != null && meshFilters_[i]?.gameObject.tag == "ground")
                {
                    v++;
                    cmb_inst[i].mesh = meshFilters_[i].sharedMesh;
                    cmb_inst[i].transform = meshFilters_[i].transform.localToWorldMatrix;
                    meshFilters_[i].gameObject.SetActive(false);
                }
        

                i++;
            }
            CombineInstance[] cmb_reformed = new CombineInstance[v];
            for(int p = 0; p < cmb_inst.Length; p ++)
            { 
                if (cmb_inst[p].mesh != null)
                {
                    cmb_reformed[x] = cmb_inst[p];
                    x ++;
                } 
            }
    
            Mesh newMesh_ = new Mesh();
            int ref_sbMeshes_cnt = filter_Reference.mesh.subMeshCount;

            newMesh_.subMeshCount = ref_sbMeshes_cnt;
            if(ref_sbMeshes_cnt > 1)
            {
                for(int sb = 0; sb < ref_sbMeshes_cnt; sb++)
                {
                    UnityEngine.Rendering.SubMeshDescriptor sb_msh = filter_Reference.mesh.GetSubMesh(sb);
                    Debug.Log(newMesh_.subMeshCount + " vs " + sb);
                    newMesh_.SetSubMesh(sb, sb_msh);
                }
            }
  

            newMesh_.CombineMeshes(cmb_reformed);
            newMesh_.RecalculateBounds();


            ( section_parent.transform.GetChild(j) ).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshFilter>().sharedMesh = newMesh_;
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).gameObject.SetActive(true);
        }

    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
