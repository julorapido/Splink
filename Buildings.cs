using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Buildings : MonoBehaviour
{
    [SerializeField]
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
            Debug.Log(chld_);
            List<Material?> chld_materials = new List<Material?>(new Material[7] {null, null, null, null, null, null, null});
            List<int> materialsDbl_indexes = new List<int>(new int[7] {-1, -1, -1, -1, -1, -1, -1});

            int chld_i = 0;

            //if parent is his own mesh renderer
            if(chld_.GetComponent<MeshFilter>() != null) continue;
            
            bool mt_fetched = false;
            // Right order to get mesh filters [NOT RECURSIVE-UP]
            MeshFilter[] meshFilters_ = new MeshFilter[chld_.transform.childCount];
            for (int c = 0; c < chld_.transform.childCount; c ++)
            {
                MeshFilter chld_c = chld_.transform.GetChild(c).GetComponent<MeshFilter>();
                if(chld_c != null){
                    meshFilters_[c] = chld_c;
                }

                // get materials 
                MeshRenderer mesh_r = chld_.transform.GetChild(c).GetComponent<MeshRenderer>();
                if(mesh_r != null && !mt_fetched){
                    Material[] mesh_Mats = mesh_r.sharedMaterials;
                    foreach (Material localMat in mesh_Mats){
                        //if (!chld_materials.Contains(localMat) && mesh_r.gameObject.tag == "ground") {
                        if (mesh_r.gameObject.tag == "ground") 
                        {
                            // if material appears twice in meshRenderer
                            if ( chld_materials.Contains(localMat) )
                            {
                                materialsDbl_indexes[chld_i] = chld_i; 
                            }

                            chld_materials[chld_i] = localMat;
                            chld_i++;
                        }
                    }
                    mt_fetched = true;
                }
       
            }
            
            //for(int z = 0; z < meshFilters_.Length; z ++) Debug.Log(meshFilters_[z]);

            if(meshFilters_.Length < 2)
            {
                continue;
            }
            CombineInstance[] cmb_inst = new CombineInstance[meshFilters_.Length];

            int sb_Mcount = 0;
            Mesh[] subMeshes_ = new Mesh[chld_i];

            // loop for each material => [chld_materials]
            // submeshes creation => a combiner for each (sub)mesh that is mapped to the right material.
            //foreach (Material searchdMaterial_ in chld_materials)
            for(int y = 0; y < chld_materials.Count; y++)
            {
                Material searchdMaterial_ = chld_materials[y];
                if(searchdMaterial_ == null) break;

                // map meshFilters with correspondant material
                int i = 0, v = 0, x = 0;
                while (i < meshFilters_.Length)
                {
                    // CHECK if is meshFilter is same as filterReference
                    if (i < (meshFilters_.Length - 1)  && filter_Reference != null)
                    {
                        if( ( (meshFilters_[i]?.sharedMesh) != filter_Reference?.sharedMesh) ) 
                        {
                            i++;
                            continue;
                        } 
                    }

                    // reference assignation
                    if( filter_Reference == null && meshFilters_[i]?.gameObject.tag == "ground") {
                        filter_Reference = meshFilters_[i];
                        filter_Ref_chldPos = i;
                        continue;
                    };

                    if(filter_Reference != null && meshFilters_[i]?.gameObject.tag == "ground")
                    {
                        // Let's see if their materials are the one we want right now.
                        Material[] loopMaterials = meshFilters_[i].GetComponent<Renderer>().sharedMaterials;
                        for (int m_I = 0; m_I < loopMaterials.Length; m_I++)
                        {
                            if (loopMaterials[m_I] != searchdMaterial_)
                            continue;

                            // This submesh is the material we're looking for right now.
                            // each Material gets a new CombineInstance() [subMesh]
                            CombineInstance ci = new CombineInstance();
                            ci.mesh = meshFilters_[i].sharedMesh;

                            if(materialsDbl_indexes.Contains(m_I)) ci.subMeshIndex = m_I + 1;
                            else ci.subMeshIndex = m_I;
                            
                            //ci.transform = meshFilters_[i].transform.localToWorldMatrix;
                            ci.transform = meshFilters_[i].transform.worldToLocalMatrix;
                            cmb_inst[i] = ci;
                            // turn off gmObj
                            meshFilters_[i].gameObject.SetActive(false);

                            v++;
                        }
              
                    }
            

                    i++;
                }
            
                // clear empty combineInstance spaces
                CombineInstance[] cmb_reformed = new CombineInstance[v];
                for(int p = 0; p < cmb_inst.Length; p ++)
                { 
                    if (cmb_inst[p].mesh != null)
                    {
                        cmb_reformed[x] = cmb_inst[p];
                        x ++;
                    } 
                }
        
                // Flatten into a single mesh.
                Mesh newMesh_ = new Mesh();
                newMesh_.CombineMeshes (cmb_reformed, true);
                subMeshes_[sb_Mcount] = newMesh_;

                sb_Mcount ++;
            }


            // The final mesh: combine all the material-specific meshes as independent submeshes.
            CombineInstance[] finalCombiners = new CombineInstance[meshFilters_.Length];
            int fC = 0;
            foreach (Mesh mesh in subMeshes_)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = 0;
                ci.transform = Matrix4x4.identity;
                finalCombiners[fC] = (ci);
                fC ++;
            }

            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(finalCombiners, false);

            // reduce final mesh renderer materials list ==> before modifying new [finalMesh]
            chld_materials.RemoveAll(item => item == null);
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshRenderer>().SetMaterials(chld_materials);

            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshFilter>().sharedMesh = finalMesh;
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).gameObject.SetActive(true);

            Debug.Log ("Final mesh has " + subMeshes_.Length + " materials.");
        }

    }


}




    //   private void AdvancedMerge()
    //     {
    //     // All our children (and us)
    //     MeshFilter[] filters = GetComponentsInChildren (false);

    //     // All the meshes in our children (just a big list)
    //     //   List materials = new List();
    //     //   MeshRenderer[] renderers = GetComponentsInChildren (false); // <-- can optimize this

    //     //   foreach (MeshRenderer renderer in renderers)
    //     //   {
    //     //    if (renderer.transform == transform)
    //     //     continue;
    //     //    Material[] localMats = renderer.sharedMaterials;
    //     //    foreach (Material localMat in localMats)
    //     //     if (!materials.Contains (localMat))
    //     //      materials.Add (localMat);
    //     //   }


    //     // Each material will have a mesh for it.
    //     List submeshes = new List();
    //     foreach (Material material in materials)
    //     {
    //         // Make a combiner for each (sub)mesh that is mapped to the right material.
    //         List combiners = new List ();
    //         foreach (MeshFilter filter in filters)
    //         {
    //                 // if (filter.transform == transform) continue;
    //                 // // The filter doesn't know what materials are involved, get the renderer.
    //                 // MeshRenderer renderer = filter.GetComponent ();  // <-- (Easy optimization is possible here, give it a try!)
    //                 // if (renderer == null)
    //                 // {
    //                 //     Debug.LogError (filter.name + " has no MeshRenderer");
    //                 //     continue;
    //                 // }

    //                 // Let's see if their materials are the one we want right now.
    //                 Material[] localMaterials = GetComponent<Renderer>().sharedMaterials;
    //                 for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
    //                 {
    //                     if (localMaterials [materialIndex] != material)
    //                     continue;

    //                     // This submesh is the material we're looking for right now.
    //                     CombineInstance ci = new CombineInstance();
    //                     ci.mesh = filter.sharedMesh;
    //                     ci.subMeshIndex = materialIndex;
    //                     ci.transform = Matrix4x4.identity;
    //                     combiners.Add (ci);
    //                 }
    //         }
    //         // Flatten into a single mesh.
    //         Mesh mesh = new Mesh ();
    //         mesh.CombineMeshes (combiners.ToArray(), true);
    //         submeshes.Add (mesh);
    //     }

    //     // The final mesh: combine all the material-specific meshes as independent submeshes.
    //     List finalCombiners = new List ();
    //     foreach (Mesh mesh in submeshes)
    //     {
    //         CombineInstance ci = new CombineInstance ();
    //         ci.mesh = mesh;
    //         ci.subMeshIndex = 0;
    //         ci.transform = Matrix4x4.identity;
    //         finalCombiners.Add (ci);
    //     }
    //     Mesh finalMesh = new Mesh();
    //     finalMesh.CombineMeshes (finalCombiners.ToArray(), false);
    //     myMeshFilter.sharedMesh = finalMesh;
    //     Debug.Log ("Final mesh has " + submeshes.Count + " materials.");


    //     }