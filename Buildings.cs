using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Buildings : MonoBehaviour
{
    [Header ("SECTIONS")]
    [SerializeField] private GameObject[] sections_prefabs;
    private GameObject[] buildngs_prefb;
    private Transform last_sectionSpawned = null;

    [Header ("SETTINGS")]
    [SerializeField] private float sections_scale;

    [Header ("TRANSFORMS")]
    [SerializeField] private Transform player_trsf;
    [SerializeField] private Transform bldg_parent;

    [Header ("Optimization")]
    private GameObject[] activated_go_bfr = new GameObject[30]; // 30 fixed bfr
    private GameObject[] optmized_go_bfr = new GameObject[30]; // 30 fixed bfr
    private float o_timer = 0f;

    [Header ("GameOver")]
    private bool game_over = false;
    [HideInInspector] public bool set_game_over{
        get {return false;}
        set {if(value.GetType() == typeof(bool)) game_over = value;}
    }


    // Start
    private void Start()
    {
        RenderSettings.skybox.SetFloat("_Rotation", 0f);

        Gen_PrefabSection();
        Gen_PrefabSection();

        for(int i = 0; i < 30; i ++)
            optmized_go_bfr[i] = activated_go_bfr[i] = null;

        optimize_sections();
    }


    // Update
    private void Update()
    {
        o_timer += Time.deltaTime;
        if(o_timer >= 12f)
        {
            optimize_sections();
            o_timer = 0f;
        }
    }


    // FixedUpdate
    private void FixedUpdate()
    {
        float p =  RenderSettings.skybox.GetFloat("_Rotation");
        if(p == 359.5f)
            RenderSettings.skybox.SetFloat("_Rotation", 0);
        
        RenderSettings.skybox.SetFloat("_Rotation", p + (Time.deltaTime * 0.20f) );


        // infinite game loop
        if(last_sectionSpawned != null && 
            (last_sectionSpawned.position.z - player_trsf.position.z) < 180
        )
        {
            Gen_PrefabSection();
        }
    }


    // -----------------------------
    //       Optimize Sections
    // -----------------------------
    private void optimize_sections()
    {
        GameObject[] active_sections = GameObject.FindGameObjectsWithTag("Section");

        for(int i = 0; i < active_sections.Length; i++)
        {
                // Focus
                List<string> focus_objs = new List<string>(new string[12]
                {
                    "obstacle", // obst
                    "tyro", // special
                    "slider", "slideRail", "fallBox",// modules
                    "tapTapJump", "bumper", "launcher", "hang", "ladder", "underSlide", "bareer", // interacts
                } );
                // Collectibles
                List<string> collectibles_objs = new List<string>(new string[5]
                {
                    "coin", "gun", "healthBig", "healthSmall", "ammoSmall"
                } );
                // Turrets
                List<string> turret_objs = new List<string>(new string[1]{
                    "TURRET"
                } );

                float dst = (active_sections[i].transform.position.z - 
                    ((135f * active_sections[i].transform.localScale.x) / 2)
                ) - (player_trsf.position.z);

                GameObject s = active_sections[i];
                if( dst >= (game_over ? 160 : 70) || dst <= -160) // optimize [behind and below]
                {
                    if( !optmized_go_bfr.Contains(active_sections[i]) )
                    {
                        Transform[] gm_t = active_sections[i].GetComponentsInChildren<Transform>();
                        for(int j = 0; j < gm_t.Length; j++)
                        {
                            if(gm_t[j]?.gameObject.tag == null || !gm_t[j].gameObject.activeSelf)
                                continue;


                            if(focus_objs.Contains(gm_t[j].gameObject.tag) || collectibles_objs.Contains(gm_t[j].gameObject.tag)
                                || turret_objs.Contains(gm_t[j].gameObject.tag)
                            )
                            { gm_t[j].gameObject.SetActive(false); }
                        }

                        // add optimized GO to arr
                        for(int k = 0; k < optmized_go_bfr.Length; k++)
                        {
                            if(optmized_go_bfr[k] == null)
                            {
                                optmized_go_bfr[k] = active_sections[i];
                                break;
                            }
                        }
                    }
                }
                else // cancel optimization
                {
                    if( !activated_go_bfr.Contains(active_sections[i]) 
                        && (dst > -5)
                    )
                    {
                        // Include inactive <GetCompInChild<T>(bool includeInactive)>
                        Transform[] gm_t = active_sections[i].GetComponentsInChildren<Transform>(true);
                        // Debug.Log("ACTIVATE BACK-ON " + active_sections[i] + " TRANSFORMS ?" + gm_t.Length);

                        for(int j = 0; j < gm_t.Length; j++)
                        {
                            if(gm_t[j]?.gameObject.tag == null || gm_t[j].gameObject.activeSelf)
                                continue;
                            
                            if(focus_objs.Contains(gm_t[j].gameObject.tag) || collectibles_objs.Contains(gm_t[j].gameObject.tag) 
                                || turret_objs.Contains(gm_t[j].gameObject.tag))
                                gm_t[j].gameObject.SetActive(true);
                            
                        }

                        // add activated GO to arr
                        for(int k = 0; k < activated_go_bfr.Length; k++)
                        {
                            if(activated_go_bfr[k] == null)
                            {
                                activated_go_bfr[k] = active_sections[i];
                                break;
                            }
                        }
                    }
                }
        }

        // clear [inside] alr activated
        for(int j = 0; j < activated_go_bfr.Length; j++)
        {
            if(activated_go_bfr[j] != null){
                float dst = (activated_go_bfr[j].transform.position.z - (135 / 2)) - player_trsf.position.z;
                if(dst < -5)
                    activated_go_bfr[j] = null;
            }
        } 
        // clear [passed-on] alr optimized
        for(int k = 0; k < optmized_go_bfr.Length; k++)
        {
            if(optmized_go_bfr[k] != null){
                float dst = (optmized_go_bfr[k].transform.position.z - (135 / 2)) - player_trsf.position.z;
                if(dst > -160)
                    optmized_go_bfr[k] = null;
            }
        }
    }




    // ** ** ** ** ** ** ** **
    // Outdated buildings generation function
    private void Gen_Bldngs(int z_len)
    {
        int ln_ = buildngs_prefb.Length;
        float x_pos, space_t_fill, z_pos;
        // Z 
        for(int p = 0; p < z_len; p++)
        {
            x_pos = 0.0f;
            space_t_fill = 0.0f;
            // X 
            for(int i = 0; i < ln_; i ++)
            {
                const float fnc_gn_w = 50.0f;
                if(x_pos > fnc_gn_w){break;}

                int rdm_ = UnityEngine.Random.Range(0, ln_);/// RAND bat indx
                GameObject sl = buildngs_prefb[rdm_];
                Vector2 bld_sze = new Vector2(0, 0); // Type of => Vect2 [width(x-axis), profondeur(z-axis)]
                
                float bld_wdth = 0.0f; // Actual Width of BLDG
                bool prnt_passed = false; bool is_prnt_cldr = false;
                int indx_scale = 0;

                //////////////////// BLDG SIZE ////////////////////
                Collider[] sl_coldrs = sl.transform.GetComponentsInChildren<Collider>();
                for (int j = 0; j < sl_coldrs.Length; j ++)
                {

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

                if(is_prnt_cldr){ bld_wdth = bld_wdth * sl.transform.localScale.x; }else
                {
                    bld_wdth = (bld_wdth * sl_coldrs[indx_scale].gameObject.transform.localScale.x) * sl.transform.localScale.x;
                    //bld_wdth = bld_wdth  * sl.transform.localScale.x;
                }
                Instantiate(sl, new Vector3(x_pos + bld_wdth/2 , 0, 0), new Quaternion(0f, 90f, 0f, 1), bldg_parent);

                x_pos += bld_wdth;
            } 
            //z_pos += 40.0f;
        }
    }
    // ** ** ** ** ** **






    // ========================================
    //             SPAWN NEW SECTION
    // ========================================
    private void Gen_PrefabSection()
    {
        int ln_ = sections_prefabs.Length;
        float z = 0f;
        // Ignored-Scale gameobjects
        const int ign_ln = 3;
        List<string> ignored_scale_tags = new List<string>(new string[ign_ln]
        {
            "TURRET", // Turrets
            "ignoreTYRO", // Tyro things
            "coin"
        } );

        GameObject[] active_sections = GameObject.FindGameObjectsWithTag("Section");
        for(int i = 0; i < active_sections.Length; i ++)
        {
            BoxCollider bx = active_sections[i].GetComponent<BoxCollider>();
            float z_size = (bx.size.z) * (active_sections[i].transform.localScale.z);
            z += z_size;
        }

        int rdm_ = UnityEngine.Random.Range(0, ln_);/// RAND Section indx 
        int rdm_2 = UnityEngine.Random.Range(0, ln_);/// RAND Section indx 
        GameObject sl = sections_prefabs[UnityEngine.Random.Range(1, 3) == 1 ? rdm_ : rdm_2];

        // BoxCldr Size as whole Sect Size
        // (135) Section Z-Size
        Vector3 sl_size = sl.GetComponent<BoxCollider>().size;

        if(sections_scale != 1f)
        {
            sl_size.z *= sections_scale;
            // sl_size.x *= sections_scale;
        }

        GameObject buffer_sect = Instantiate(
            sl, 
            new Vector3(
                -1 * (sl_size.x / 2), 
                0f,
                z + (sl_size.z / 2) // + 1f
            ), 
            new Quaternion(0f, 0f, 0f, 1), 
            bldg_parent
        );
        
        buffer_sect.transform.localScale = new Vector3(sections_scale, sections_scale, sections_scale);
        for(int i = 0; i < ign_ln; i++)
        {
            Transform[] selected_t = ((buffer_sect.transform.GetComponentsInChildren<Transform>()).Where(
                t => t.gameObject.tag == ignored_scale_tags[i]
            ).ToArray()) as Transform[];
            float r = 1f + (1f - sections_scale);
            for(int j = 0; j < selected_t.Length; j ++)
            {
                float s =  selected_t[j].localScale.x * (r);
                selected_t[j].localScale = new Vector3(s, s, s);
            }
        }

        last_sectionSpawned = buffer_sect.transform;

        if(buffer_sect.transform.childCount >= 2)
        {
            // combine_Meshes(buffer_sect, buffer_sect.transform.childCount);
            // generate_SubTerrain(buffer_sect);
            // generateBounds(buffer_sect);
        }
    }






    // ========================================
    //              COMBINE MESHES
    // ========================================
    private void combine_Meshes(GameObject section_parent, int chld_len)
    {
        MeshFilter? filter_Reference = null;
        Transform transform_Refence = null;
        int filter_Ref_chldPos = -1;

        int wholeBatchBldg = 0;
        int wholeBatchMat = 0;


        for (int j = 0; j < chld_len; j ++)
        {
            if(section_parent.transform.GetChild(j).tag == "combinePass") continue;
            
            // filterRef reset (-_-)
            filter_Reference = null; filter_Ref_chldPos = -1;

            GameObject chld_ = section_parent.transform.GetChild(j).gameObject;
            Material[] chld_mats = new Material[8]; // 8 mat buffer
            bool mt_fetched = false;
            int sumbeshesCount = 0;

            //if parent is his own mesh renderer
            if(chld_.GetComponent<MeshFilter>() != null) continue;

            // Right order to get mesh filters [NOT RECURSIVE-UP]
            MeshFilter[] meshFilters_ = new MeshFilter[chld_.transform.childCount];
            for (int c = 0; c < chld_.transform.childCount; c ++)
            {
                MeshFilter chld_c = chld_.transform.GetChild(c).GetComponent<MeshFilter>();
                if(chld_c != null && (chld_.transform.GetChild(c).gameObject.activeSelf == true) )
                {
                    meshFilters_[c] = chld_c;
                }

                // get materials length [ for combineInstance.subMeshIndex ]
                MeshRenderer mesh_r = chld_.transform.GetChild(c)?.GetComponent<MeshRenderer>();
                if(!mesh_r) continue;
                if(mesh_r != null && !mt_fetched)
                {
                    Material[] mesh_Mats = mesh_r.sharedMaterials;
                    foreach (Material localMat in mesh_Mats)
                    {
                        if (mesh_r.gameObject.tag == "ground") 
                        {
                            chld_mats[sumbeshesCount] = localMat;
                            sumbeshesCount++;
                        }
                    }
                    mt_fetched = true;
                }
            }
            

            if(meshFilters_.Length < 2 || chld_mats[0] == null)
            {
                //Debug.Log("MATERIAL CATCH!!!!");
                continue;
            }

            CombineInstance[] cmb_inst = new CombineInstance[sumbeshesCount * meshFilters_.Length];
            int batchedBldg = 0;

            // map meshFilters with correspondant material
            int i = 0, v = 0, x = 0;
            while (i < meshFilters_.Length)
            {
                // CHECK if is meshFilter is same as filterReference
                if (filter_Reference != null)
                {
                    if( ( (meshFilters_[i]?.sharedMesh) != filter_Reference?.sharedMesh) ) 
                    {
                        i++;
                        continue;
                    } 
                }

                // reference assignation    
                if( filter_Reference == null && meshFilters_[i]?.gameObject.tag == "ground")
                {
                    filter_Reference = meshFilters_[i];
                    transform_Refence = meshFilters_[i].gameObject.GetComponent<Transform>();
                    filter_Ref_chldPos = i;
                    continue; 
                };

                if(filter_Reference != null && meshFilters_[i]?.gameObject.tag == "ground")
                {
                    batchedBldg++;
                    for(int o = 0; o < sumbeshesCount; o++)
                    {
                        // one CombineInstance per subMesh 
                        CombineInstance ci = new CombineInstance();
                        ci.mesh = meshFilters_[i].sharedMesh;

                        Matrix4x4 instance_Matrix =  meshFilters_[i].transform.worldToLocalMatrix;
                        Vector3 mesh_pos = meshFilters_[i].gameObject.transform.localPosition;

                        float y_ecart = transform_Refence.localPosition.y - mesh_pos.y;

                        float y__ = (
                            v > 0 ?               
                                ((v) * (( meshFilters_[i].sharedMesh.bounds.size.y * transform_Refence.localScale.y) / 2) )
                            : 0
                        );
                        y__ += (y_ecart - y__);

            
                        //swap parent
                        meshFilters_[i].gameObject.transform.parent = transform_Refence;

                        // new Matrix 4x4.SetTRS(position, quaternion, scale)
                        //Debug.Log(meshFilters_[i].gameObject + " vs  i: " + i +  " rot : " + meshFilters_[i].gameObject.transform.localRotation );
                        //Debug.Log( meshFilters_[i].gameObject.transform.localRotation );
                        instance_Matrix.SetTRS(
                            v > 0 ? meshFilters_[i].gameObject.transform.localPosition : new Vector3(0, 0, 0),
                            v > 0 ? meshFilters_[i].gameObject.transform.localRotation :  Quaternion.identity,
                            Vector3.one
                        );

                        ci.transform = instance_Matrix;
       
                        ci.subMeshIndex = o;
                        cmb_inst[(i * sumbeshesCount) + o] = ci;
                        
                        // turn off gm_tObj
                        if (v > 0) meshFilters_[i].gameObject.SetActive(false);
                    }
                    v++;
                }
        
                i++;
            }
                
            if(filter_Ref_chldPos == -1 || filter_Reference == null) return;

            // clear empty combineInstance spaces
            CombineInstance[] cmb_reformed = new CombineInstance[v * sumbeshesCount];
            for(int p = 0; p < cmb_inst.Length; p ++)
            { 
                if (cmb_inst[p].mesh != null)
                {
                    cmb_reformed[x] = cmb_inst[p];
                    x ++;
                } 
            }
            cmb_inst = cmb_reformed;

            // Flatten into a single mesh.
            Mesh newMesh_ = new Mesh();
            newMesh_.CombineMeshes (cmb_inst, false);
            newMesh_.RecalculateBounds();
            newMesh_.RecalculateNormals();
            newMesh_.Optimize();
                    
 
            //Debug.Log(sumbeshesCount + " submeshes and " + batchedBldg +  " batched instances"); 
            //Debug.Log("------------------------------"); 
            wholeBatchBldg+=batchedBldg; wholeBatchMat+= sumbeshesCount;


            // update materials [materials count ^2 ]
            Material[] mt = new Material[sumbeshesCount * batchedBldg];
            for(int m = 0; m < (sumbeshesCount * batchedBldg); m += sumbeshesCount)
            {
                for(int mM = 0; mM < sumbeshesCount; mM ++) mt[m + mM] = chld_mats[mM];
            }


            // update colliders
            Dictionary<string, Type> _Types = new Dictionary<string, Type> {
                { "Sphere", typeof(SphereCollider) }, { "Mesh", typeof(MeshCollider) },
                { "Capsule", typeof(CapsuleCollider) }, { "Box", typeof(BoxCollider) },
            };
            GameObject ref_go = section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).gameObject;

            // map childs
            for(int cl = 0; cl < ref_go.transform.childCount; cl++)
            {
               if(ref_go.transform.GetChild(cl).gameObject.tag == "ground")
               {
                    Transform trnsfrm_cl = ref_go.transform.GetChild(cl).gameObject.transform;
                    Vector3 colliders_offst = trnsfrm_cl.localPosition;
                
                    BoxCollider[] bx_arr = trnsfrm_cl.GetComponents<BoxCollider>(); SphereCollider[] sph_arr = trnsfrm_cl.GetComponents<SphereCollider>();
                    MeshCollider[] msh_arr = trnsfrm_cl.GetComponents<MeshCollider>(); CapsuleCollider[] cps_arr = trnsfrm_cl.GetComponents<CapsuleCollider>();
                    
                    try{

                        Collider[] colliders = ref_go.transform.GetChild(cl).GetComponents<Collider>();
                        if(colliders.Length > 0)
                        {
                            for (int z = 0; z < colliders.Length; z ++)
                            {
                                string[] typeCldr = (colliders[z].GetType().ToString().Split("UnityEngine."));
                                string forced_Type = (typeCldr[1].Split("Collider"))[0];
                                
                                // Mesh Collider special case
                                if(_Types[forced_Type] == typeof(MeshCollider))
                                {

                                    //GameObject emptyHolder = new GameObject();
                                    GameObject cldrs_holder = Instantiate( new GameObject(),
                                        ref_go.transform.GetChild(cl).position,
                                        ref_go.transform.GetChild(0).rotation,
                                        section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos)
                                    );

                                    Collider mc = cldrs_holder.AddComponent((_Types[forced_Type])) as Collider;
                                    MeshCollider m_p = mc as MeshCollider;
                                    m_p.sharedMesh = ref_go.GetComponent<MeshFilter>().sharedMesh;
                                    m_p.convex = true;
                                }

                                // Box, Sphere and Capsule
                                else
                                {
                                    Collider? ref_ = colliders[z];
                                    Collider? p = section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).gameObject.AddComponent((_Types[forced_Type])) as Collider;
                                    if(_Types[forced_Type] == typeof(CapsuleCollider)) p = p as CapsuleCollider ;
                                    if(_Types[forced_Type] == typeof(BoxCollider)) p = p as BoxCollider;
                                    if(_Types[forced_Type] == typeof(SphereCollider)) p = p as SphereCollider;
                                                        
                                    if(p is BoxCollider)
                                    {
                                        (p as BoxCollider).center = (ref_ as BoxCollider).center + colliders_offst;
                                        (p as BoxCollider).size = (ref_ as BoxCollider).size;
                                    };


                                    if(p is CapsuleCollider)
                                    {
                                        (p as CapsuleCollider).center = (ref_ as CapsuleCollider).center + colliders_offst;
                                        (p as CapsuleCollider).height = (ref_ as CapsuleCollider).height;
                                        (p as CapsuleCollider).radius = (ref_ as CapsuleCollider).radius;
                                    } 

                                    // cast SphereCollider on p (for radius)
                                    if( (p is SphereCollider)) 
                                    {
                                        (p as SphereCollider).center = (ref_ as SphereCollider).center + colliders_offst;
                                        (p as SphereCollider).radius = (ref_ as SphereCollider).radius;

                                    }

                                    // var p_forced = (
                                    //     (_Types[forced_Type] == typeof(BoxCollider) ) ? (p as BoxCollider)
                                    //          :
                                    //     ( _Types[forced_Type] == typeof(SphereCollider) ? (p as SphereCollider) : (p as CapsuleCollider) );
                                    // );
                                }
                    
                            }
                        }
                    
                    } catch(Exception err) {
                        Debug.Log(err);
                    }
    
               } 
            }


            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshRenderer>().sharedMaterials = mt;
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshFilter>().sharedMesh = newMesh_;
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).GetComponent<MeshFilter>().sharedMesh.Optimize();
            section_parent.transform.GetChild(j).transform.GetChild(filter_Ref_chldPos).gameObject.SetActive(true);

        }


        Debug.Log(" Whole Batch : "  + (wholeBatchBldg * wholeBatchMat) );
    }

    

    private Mesh fixFinalVertices(Mesh _mesh, Vector3 scale)
    {
        // Step 1: Get the vertices of the mesh
        Vector3[] vertices = _mesh.vertices;
            Vector3 center = Vector3.zero;
        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
        }
    
        center /= vertices.Length;
        center.y = center.y + _mesh.bounds.size.y;
        // Step 3: Move vertices to the new center
        for (int i = 0; i < vertices.Length; i++)
        {
            //vertices[i] -= center;
            //Vector3 vertex = vertices[i];
            vertices[i].x = vertices[i].x * scale.x;
            vertices[i].y = vertices[i].y * scale.y;
            vertices[i].z = vertices[i].z * scale.z;
        }
    
        // Step 4: Update the mesh with the new vertices
        _mesh.vertices = vertices;
        _mesh.RecalculateBounds();
         _mesh.RecalculateNormals();

        return _mesh;
    }









    private void combine_MeshesAndMergeMaterials(GameObject section_parent, int chld_len){
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

                            if(materialsDbl_indexes.Contains(m_I)) ci.subMeshIndex = m_I ;
                            else ci.subMeshIndex = m_I;
                            
                            //ci.transform = meshFilters_[i].transform.localToWorldMatrix;
                            ci.transform = meshFilters_[i].transform.worldToLocalMatrix;
                            cmb_inst[i] = ci;
                            // turn off gm_tObj
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






    private void generate_SubTerrain(GameObject whole_sect)
    {
        GameObject[] allBuildngs_ = GameObject.FindGameObjectsWithTag("ground");

        // fetch bldg sizes
        for(int i = 0; i < allBuildngs_.Length; i ++)
        {
            if(!allBuildngs_[i].activeSelf || 
                !allBuildngs_[i].transform.IsChildOf(whole_sect.transform)
            ) continue; // since all buildings were baked/combined

            Mesh bldg_mesh = allBuildngs_[i].GetComponent<MeshFilter>().sharedMesh;
            Collider[] bldg_colldrs = allBuildngs_[i].GetComponents<Collider>();
            
            float[] x_ = ((bldg_colldrs as IEnumerable<Collider>).Select(
                x => x.bounds.size.x * allBuildngs_[i].transform.localScale.x)
            ).ToArray();
            float[] z_ = ((bldg_colldrs as IEnumerable<Collider>).Select(
                z => z.bounds.size.z * allBuildngs_[i].transform.localScale.z)
            ).ToArray();

            Array.Sort(x_); Array.Sort(z_);

        }
    }


    private void generateBounds(GameObject whole_sct)
    {
        GameObject[] allBuildngs_ = GameObject.FindGameObjectsWithTag("ground");

        float[] l_ = new float[80]; int l_i = 0;
        float[] r_ = new float[80]; int r_i = 0;

        // fetch bldg sizes
        for(int i = 0; i < allBuildngs_.Length; i ++)
        {
            if(!allBuildngs_[i].activeSelf || 
                !allBuildngs_[i].transform.IsChildOf(whole_sct.transform) || 
                ( allBuildngs_[i].GetComponent<MeshFilter>() == null )
            ) continue; // since all buildings were baked/combined

            Mesh bldg_mesh = allBuildngs_[i].GetComponent<MeshFilter>()?.sharedMesh;
            Collider[] bldg_colldrs = allBuildngs_[i].GetComponents<Collider>();
            
            if(bldg_colldrs.Length == 0) continue;

            float[] x_ = ((bldg_colldrs as IEnumerable<Collider>).Select(
                x => x.bounds.size.x * allBuildngs_[i].transform.localScale.x)
            ).ToArray();
            // float[] z_ = ((bldg_colldrs as IEnumerable<Collider>).Select(
            //     z => z.bounds.size.z * allBuildngs_[i].transform.localScale.z)
            // ).ToArray();

            Array.Sort(x_); // Array.Sort(z_);

            float c_size = ((allBuildngs_[i].transform.localScale.x * x_[x_.Length - 1 >= 0 ? x_.Length - 1 : 0]) / 2) / 10;
            if(c_size < 0.1f) continue;

            if(allBuildngs_[i].transform.localPosition.x > 1){
                r_[r_i] = c_size + allBuildngs_[i].transform.position.x;
                r_i++;
            }else{
                l_[l_i] = c_size - allBuildngs_[i].transform.position.x;
                l_i++;
            }
        }
        Array.Sort(l_); Array.Sort(r_);

        GameObject sc_bounds = new GameObject();
        sc_bounds.name = "section_bounds";
        GameObject bounds = Instantiate(sc_bounds, new Vector3(0,0,0), Quaternion.identity, whole_sct.transform);

        BoxCollider l = bounds.AddComponent<BoxCollider>();
        BoxCollider r = bounds.AddComponent<BoxCollider>();

        r.size = l.size = new Vector3(1, 50, 200);

        l.center = new Vector3(l_[l_.Length - 1], 0, 0);
        r.center = new Vector3(r_[r_.Length - 1], 0, 0);
    }


}


