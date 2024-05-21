using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq; // Make 'Select' extension available

// [ExecuteInEditMode]
public class GameUI : MonoBehaviour
{
    [Header ("CAMERA")]
    [SerializeField] private Camera ui_camera;

    [Header ("PLAYER HEALTH")]
    [SerializeField] private GameObject healh_ui;
    private TextMeshProUGUI healh_text;
    private int max_health = 4000;
    private int player_health = 0; // = 200;
    private int set_health
    {
        get { return 0; }
        set{ if(value > max_health) player_health = max_health; else value = max_health;}
    }

    [Header ("WEAPON")]
    [SerializeField] private GameObject weapon_ui_obj;
    private Weapon weapon_script;
    private TextMeshPro[] weapon_ammoTxts;
    private GameObject weapon_reload;
    private float reload_timerFloat = 0f;


    [Header ("MONEY")]
    [SerializeField] private GameObject money_ui;
    private TextMeshProUGUI[] money_bfr = new TextMeshProUGUI[20]; // 20 sized gm bfr
    private bool m_textGlow_sns = false, is_glowing = false;
    private TextMeshProUGUI money_txt;
    private int player_money = 0;
    private int money_i = 0, money_v = 0;
    private const float money_delay = 0.0075f;
    private float money_timer = 0f;


    [Header ("TIMER & SCORE")]
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private TextMeshProUGUI timer_txt;  
    private float timer_ = 0f;
    private bool countScore_ = false, countBonus_ = false;
    private uint lastZ = 0, score = 0;
    private decimal v_score = 0;
    private int temp_bonus = 0;
    [HideInInspector] public bool set_countBonus_
    {
        get { return false; }
        set{ if(value.GetType() == typeof(bool)) countBonus_ = value;}
    }


    [Header ("UI Objects")]
    [SerializeField] private Transform[] parents_2d;

    [Header ("Scripts")]
    private Weapon weapon_scrpt;
    private PlayerMovement p_movement;

    [Header ("Player")]
    private Transform plyr_transform;
    private Rigidbody plyr_rgBody;

    [Header ("KILLS")]
    [SerializeField] private GameObject kill_obj;


    [Header ("DAMAGE")]
    [SerializeField] private GameObject damage_obj;
    private GameObject[] damage_hits = new GameObject[40]; // 25 fixed bfr
    private Transform[] damage_hits_t = new Transform[40]; // 25 fixed bfr
    private Vector3[] damage_hits_offst = new Vector3[40]; // 25 fixed bfr

    [Header ("GUN UI")]
    [SerializeField] private GameObject gun_ui;
    [SerializeField] private TextMeshPro ammo_txt;
    [SerializeField] private Material[] Gun_Materials;
    private Vector3 saved_prefabPos_, saved_prefabScale_;
    private Quaternion saved_prefabRotation_;
    private GameObject[] gun_fetchedPrefabs;


    [Header ("ENEMY UI")]
    [SerializeField] private GameObject enemy_ui_instance;
    private GameObject[] ui_enemies = new GameObject[25]; // 25 fixed bfr
    private Transform[] enemies_tr = new Transform[25]; // 25 fixed bfr
    private Vector3[] enemies_offst = new Vector3[25]; // 25 fixed bfr
    [SerializeField] private Transform er;


    [Header ("COMBO")]
    [SerializeField] private Sprite[] combo_masks;
    [SerializeField] private GameObject combo_obj;
    private string combo_text = "Good";
    private int combo_v = 0, combo_goal = 3;
    private LTDescr combo_lt;


    // Awake
    private void Awake()
    {
        player_health = max_health;
    }

    // Start
    private void Start()
    {
        weapon_scrpt = FindObjectOfType<Weapon>();

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        p_movement = pm;
        plyr_transform = pm.transform;
        plyr_rgBody  = pm.transform.GetComponent<Rigidbody>();


        gun_fetchedPrefabs = weapon_scrpt.get_weaponsResourcesPrefab_buffer;


        int l_prefb = gun_ui.transform.childCount - 1;
        saved_prefabPos_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.position;
        saved_prefabScale_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.localScale;
        saved_prefabRotation_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.localRotation;


    
        // money txt
        money_txt = money_ui.transform.GetChild(1).GetComponent<TextMeshProUGUI>();


        // weapon ammo txt
        weapon_ammoTxts = new TextMeshPro[2]{
            weapon_ui_obj.transform.GetChild(1).GetChild(2).GetComponent<TextMeshPro>(), // stack
            weapon_ui_obj.transform.GetChild(1).GetChild(1).GetComponent<TextMeshPro>(), // magz
        };


        // weapon reload
        weapon_reload = weapon_ui_obj.transform.parent.GetChild(1).gameObject;
        weapon_reload.SetActive(false);

        // health 
        healh_text = healh_ui.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        healh_ui.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = max_health.ToString();

        Invoke("countScore", 0.25f);
    }
    private void countScore(){ countScore_ = true; }



    // Update
    private void Update()
    {
        timer_ += Time.deltaTime;


        // values attribution
        timer_txt.text = timer_.ToString();
        score_txt.text = score.ToString();
        money_txt.text = player_money.ToString();
        weapon_ammoTxts[1].text = weapon_scrpt.get_ammo.ToString() + " ";
        weapon_ammoTxts[0].text = "| " + weapon_scrpt.get_ammoInMag.ToString();
        healh_text.text = player_health.ToString();


        // money
        if(player_money < money_v)
        {
            if(money_timer >= money_delay)
            {
                money_timer = 0f;
                player_money++;
            }
        }
        money_timer += Time.deltaTime;



        // reload timer float
        if(reload_timerFloat > 0)
        {
            reload_timerFloat -= Time.deltaTime;
        }




     


        // floating damages
        for(int i = 0; i < 40; i++)
        {
            if(damage_hits[i] != null && damage_hits_t[i] != null)
            {
                damage_hits[i].transform.position = damage_hits_t[i].position + damage_hits_offst[i];
                damage_hits[i].transform.LookAt(ui_camera.transform);
                damage_hits[i].transform.rotation = Quaternion.Euler(
                    -1 * damage_hits[i].transform.rotation.eulerAngles.x,
                    damage_hits[i].transform.rotation.eulerAngles.y + 180,
                    damage_hits[i].transform.rotation.eulerAngles.z
                );

                float distanceToCamera = Vector3.Distance(ui_camera.transform.position, damage_hits[i].transform.position);
                float cam_scale = (
                        2.0f * (distanceToCamera * 0.15f) * 
                        Mathf.Tan(Mathf.Deg2Rad * (ui_camera.fieldOfView * 0.5f)
                    )
                ) * (7f);
                cam_scale = (cam_scale < 65f) ? 65f : (cam_scale > 140f ? 140f : cam_scale);
                damage_hits[i].transform.localScale = new Vector3(cam_scale, cam_scale, cam_scale);
            }
        }



        // enemies ui
        if(ui_enemies[0] != null)
        {
            for(int i = 0; i < 25; i++)
            {
                if(ui_enemies[i] == null)
                    continue;
        
                float distanceToCamera = Vector3.Distance(ui_camera.transform.position, enemies_tr[i].position);
                float cam_scale = (
                        2.0f * (distanceToCamera * 0.85f) * 
                        Mathf.Tan(Mathf.Deg2Rad * (ui_camera.fieldOfView * 0.5f)
                    )
                ) * (7f);
                cam_scale = (cam_scale < 125f) ? 125f : (cam_scale > 200f ? 200f : cam_scale);

                ui_enemies[i].transform.localScale = new Vector3(cam_scale, cam_scale, cam_scale);

                ui_enemies[i].transform.position = enemies_tr[i].position + enemies_offst[i];
                ui_enemies[i].transform.LookAt(ui_camera.transform);        
                ui_enemies[i].transform.rotation = Quaternion.Euler(
                    -1 * ui_enemies[i].transform.rotation.eulerAngles.x,
                    ui_enemies[i].transform.rotation.eulerAngles.y +  180f,
                    ui_enemies[i].transform.rotation.eulerAngles.z
                );
            }
        }

    }   


    // -----------
    // FixedUpdate 
    // -----------
    private void FixedUpdate()
    {

        // ammo
        if( ammo_txt.text.ToString() != weapon_scrpt.get_ammo.ToString())
        {
            ammo_txt.text = weapon_scrpt.get_ammo.ToString();
        }


        // ui movement
        // if(plyr_rgBody != null && plyr_transform != null)
        // {
        //     for(int i = 0; i < parents_2d.Length; i++)
        //     {
        //         if(parents_2d[i] == null
        //             || LeanTween.isTweening ( parents_2d[i].gameObject)
        //         ) continue;

        //         // parents_2d[i].transform.rotation = Quaternion.Slerp( parents_2d[i].transform.rotation, Quaternion.Euler(
        //         //     plyr_rgBody.velocity.y * 0.10f,
        //         //     (plyr_transform.rotation.eulerAngles.y > 180f ? 
        //         //         (-1 * (plyr_transform.rotation.eulerAngles.y - 360f) ):
        //         //         -1 * plyr_transform.rotation.eulerAngles.y
        //         //     ) * 0.06f,
        //         //     0f
        //         // ), 0.2f);

        //         parents_2d[i].transform.localPosition = Vector3.Lerp( parents_2d[i].transform.localPosition, new Vector3(
        //             parents_2d[i].transform.localPosition.x + (plyr_rgBody.velocity.x* 0.1f),
        //             parents_2d[i].transform.localPosition.y, // + (plyr_rgBody.velocity.y* 0.06f),
        //             0
        //         ), 0.2f);
        //     }  
        // }



        // score
        if(v_score == 0 && countScore_)
        {
            v_score = (uint) plyr_transform.position.z;
        }

        if( (((decimal) plyr_transform.position.z - v_score) > ((decimal) 0.10f))  &&  (v_score > 0))
        { 
            v_score += (decimal) 0.1f;
            score += (( v_score % 2) == 0 ? (uint)4 : (uint)3);
        }
    }




    // =======================
    //        ENEMY UI
    // =======================
    public void newEnemy_UI(Transform enemy_)
    {
        if(enemy_ == null)
            return;

        for(int i = 0; i < 25; i ++)
        {
            if(enemies_tr[i] == enemy_)
                break;

            if(ui_enemies[i] == null)
            {
                ui_enemies[i] = Instantiate(enemy_ui_instance, 
                    enemy_.transform.position +  new Vector3(0f, 1.5f, 0f),
                    Quaternion.identity,
                    er
                    //Quaternion.Euler(0f, enemy_.transform.rotation.eulerAngles.y, 0f)
                    //,damage_obj.transform
                );

                enemies_tr[i] = enemy_;
                StartCoroutine(fade_ui_obj(ui_enemies[i], 1.5f));

                Transform t = ui_enemies[i].transform;

                AutoTurret at_turret = enemy_.GetComponent<AutoTurret>();


                enemies_offst[i] = at_turret.is_horizontal ? 
                    (at_turret.is_left ? 
                        new Vector3(2.45f, 2.4f, -0.3f) : new Vector3(-2.45f, 2.4f, -0.3f)
                    ) 
                    : (new Vector3(0f, 3.3f, 0f)
                );

                int enemy_health = at_turret.get_health;
                int enemy_maxhealth = at_turret.get_maxHealth;

                // // Name & Level Assignation
                t.GetChild(1).GetComponent<TMPro.TextMeshPro>().text = at_turret.turret_name;
                t.GetChild(2).GetComponent<TMPro.TextMeshPro>().text = ""  + at_turret.turret_level.ToString();
                t.GetChild(3).GetComponent<TMPro.TextMeshPro>().text = enemy_health.ToString() + "/"  + enemy_maxhealth.ToString();

                // // Health Attribution
                float x_health = (((float)enemy_health / (float)enemy_maxhealth) * 100f);
                //t.GetChild(0).GetChild(1).localScale = new Vector3( (float.IsNaN(x_health) ? 0 : x_health) / 100, 1, 1);
                //t.GetChild(0).GetChild(2).localScale = new Vector3((float.IsNaN(x_health) ? 0 : x_health) / 100, 1, 1);
                break;
            }
        }
        
    }



    // =======================
    //       ENEMY_DAMAGE
    // =======================
    public void damage_ui(Transform enemy, int damage_value, bool? is_crit_hit, Vector3 offset)
    {
        if(damage_value.GetType() != typeof(int))
            return;

        for(int i = 0; i < 45; i ++)
        {
            if(damage_hits[i] == null)
            {

                damage_hits[i] = Instantiate(
                    damage_obj,
                    enemy.position, 
                    Quaternion.identity,
                    er
                ); 

                damage_hits[i].transform.localScale = new Vector3(100f, 100f, 100f);
                damage_hits_t[i] = enemy;
                damage_hits_offst[i] = offset + new Vector3(0f, 0.5f, 0f);

                float y = UnityEngine.Random.Range(0f, 1f);
                float x = UnityEngine.Random.Range(-1f, 1f);

                LeanTween.value(damage_hits[i], damage_hits_offst[i], offset + new Vector3(x, 1.5f + y, 0f), 1.3f )
                    .setEaseOutCubic()
                    .setOnUpdate( (Vector3 value) => {
                        damage_hits_offst[i] = value; 
                    });
            
                TMPro.TextMeshPro txt = damage_hits[i].GetComponent<TMPro.TextMeshPro>();
                if(is_crit_hit == null) 
                    txt.text = "CRTICIAL";
                else
                    txt.text = damage_value.ToString();

                LeanTween.scale(damage_hits[i], damage_hits[i].transform.localScale * 1.4f, 1f).setEasePunch();

                StartCoroutine(reset_damage_slot(i));

                if((bool?)is_crit_hit == true)
                    damage_ui(enemy, 0, null, offset);

                break;
            }
     

        }
    }
    private IEnumerator reset_damage_slot(int i_)
    {
        yield return new WaitForSeconds(0.6f);
        StartCoroutine(fade_ui_obj(damage_hits[i_], 0.4f, true));
        yield return new WaitForSeconds(0.4f);
        Destroy(damage_hits[i_]);
        damage_hits[i_] = null;
        damage_hits_t[i_] = null;  
    }





    // =======================
    //          COMBO
    // =======================
    public void combo_hit(bool combo_end)
    {
        if(combo_end)   
        {
            // StartCoroutine(); 
        }else{
            combo_v ++;

            if(combo_v >= 1)
            {
                combo_obj.SetActive(true);
                
                GameObject combo_bar = combo_obj.transform.GetChild(0).GetChild(0).gameObject;

                // yellow
                LeanTween.scale(combo_bar.transform.GetChild(1).gameObject, new Vector3( ((float)combo_v / (float)combo_goal), 1, 1), 0.75f).setEaseInSine();
                // white
                LeanTween.scale(combo_bar.transform.GetChild(0).gameObject, new Vector3( ((float)combo_v / (float)combo_goal), 1, 1), 0.3f).setEaseInSine();

                GameObject tris = combo_obj.transform.GetChild(2).gameObject;

                // x2
                combo_obj.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "X" + combo_v.ToString();

                for(int s = 0; s < 2; s ++)
                {
                    // l tris
                    LeanTween.moveLocal(tris.transform.GetChild(0).GetChild(s).gameObject, 
                        tris.transform.GetChild(1).GetChild(s).localPosition + new Vector3(-20f, 0f, 0f), 1.4f).setEasePunch();
                    // r tris
                    LeanTween.moveLocal(tris.transform.GetChild(1).GetChild(s).gameObject, 
                        tris.transform.GetChild(1).GetChild(s).localPosition + new Vector3(20f, 0f, 0f), 1.4f).setEasePunch();

                    LeanTween.scale(tris.transform.GetChild(1).GetChild(s).gameObject, tris.transform.GetChild(1).GetChild(s).localScale * 2f, 1.4f).setEasePunch();
                    LeanTween.scale(tris.transform.GetChild(0).GetChild(s).gameObject, tris.transform.GetChild(1).GetChild(s).localScale * 2f, 1.4f).setEasePunch();
                }

                if(combo_v == combo_goal) // PERFECT !
                {
                    GameObject combo_txt_obj = combo_obj.transform.GetChild(1).gameObject;
                    GameObject combo_x1 = combo_obj.transform.GetChild(3).gameObject;

                    // reset shine pos
                    combo_x1.transform.GetChild(0).localPosition =  combo_txt_obj.transform.GetChild(0).localPosition = new Vector3(-250, 0, 0);

                    combo_txt_obj.transform.localScale = Vector3.zero;
                    Vector3 cmbo_pos = combo_txt_obj.transform.localPosition;
                    combo_txt_obj.transform.localPosition = cmbo_pos + new Vector3(0, -40f, 0);

                    TextMeshProUGUI combo_txt = combo_obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    combo_txt.text = "PERFECT !";
                
                    // scale and rotate combo_txt
                    LeanTween.moveLocal(combo_txt_obj, cmbo_pos, 1f).setEaseInSine();
                    LeanTween.scale(combo_txt_obj, new Vector3(1, 1, 1), 1f).setEaseInSine();
                    LeanTween.rotate(combo_txt_obj, new Vector3(15, 0, 0), 1f).setEasePunch();

                    // combo_txt shine
                    LeanTween.moveLocal(combo_txt_obj.transform.GetChild(0).gameObject, 
                        combo_txt_obj.transform.GetChild(0).transform.localPosition + new Vector3(500, 0, 0),
                    1.5f).setEaseInSine();

    
                    // x2 shine
                    LeanTween.moveLocal(combo_x1.transform.GetChild(0).gameObject, 
                        combo_x1.transform.GetChild(0).transform.localPosition + new Vector3(700, 0, 0),
                    1.5f).setEaseInSine();
                    

                    //
                    GameObject p_100 = combo_obj.transform.GetChild(4).gameObject; 


                    StartCoroutine(combo_next());
                }   
            }
        }
    }
    private IEnumerator combo_next()
    {
        yield return new WaitForSeconds(1.5f);
        GameObject combo_bar = combo_obj.transform.GetChild(0).GetChild(0).gameObject;
        GameObject combo_txt_obj = combo_obj.transform.GetChild(1).gameObject;

        // scale down
        // yellow
        LeanTween.scale(combo_bar.transform.GetChild(1).gameObject, new Vector3(0f, 1f, 1f), 0.45f).setEaseInSine();
        // white 
        LeanTween.scale(combo_bar.transform.GetChild(0).gameObject, new Vector3(0f, 1f, 1f), 0.30f).setEaseInSine();

        LeanTween.scale(combo_txt_obj, new Vector3(0, 0, 0), 0.4f).setEaseInSine();

        // new combo goal
        int randm_flt_vrt =  UnityEngine.Random.Range(0, 3);

        combo_goal += (randm_flt_vrt + 1);


        Image combo_msk = combo_obj.transform.GetChild(0).GetComponent<Image>();
        combo_msk.sprite = combo_masks[randm_flt_vrt];
    }







    // =======================
    //        GAIN MONEY
    // =======================
    public void gain_money(int money_vv)
    {
        if(money_vv > 0){
            money_i++;

            GameObject m = Instantiate(
                money_ui.transform.GetChild(2).gameObject,
                money_ui.transform.GetChild(1).localPosition,
                Quaternion.identity, money_ui.transform
            );

            m.SetActive(true);

            TextMeshProUGUI m_t = m.GetComponent<TextMeshProUGUI>();
            m_t.text = "+ " + money_vv.ToString() + "$";
            m_t.color = new Color32(255, 255, 0, 255);
            m.transform.localPosition = money_ui.transform.GetChild(1).localPosition + new Vector3(0f, (-30) * money_i, 0f);
            m.transform.localScale = m.transform.localScale * 0.9f;

            LeanTween.moveLocal(m, money_ui.transform.GetChild(1).localPosition + new Vector3(0f, 20f, 0f), 1.2f).setEaseInSine();
            LeanTween.scale(m, m.transform.localScale * 1.2f, 1.7f).setEasePunch();
            LeanTween.rotate(m, new Vector3(0, 0, 3f), 2.4f).setEasePunch();
            
            LeanTween.cancel(money_ui.transform.GetChild(1).gameObject);
            LeanTween.scale(money_ui.transform.GetChild(1).gameObject, m.transform.localScale * 1.13f, 2f).setEasePunch();

            int c = money_i;
            StartCoroutine(off_m(m, c));
            StartCoroutine(text_glow_eff());

            money_v += money_vv;
        }
    }
    private IEnumerator off_m(GameObject m_obj, int index_at_time)
    {
        LeanTween.alpha(m_obj.transform.GetComponent<RectTransform>(), 0f, 1.2f).setEaseInSine();
        yield return new WaitForSeconds(0.9f);
        Destroy(m_obj);
        money_i--;
    }
    private IEnumerator text_glow_eff()
    {
        if(is_glowing) yield break;

        is_glowing = true;

        LeanTween.moveLocal(
            money_ui.transform.GetChild(1).GetChild(0).gameObject, 
            new Vector3( m_textGlow_sns ? -60 : 60, 0, 0),
        1f).setEaseInSine();

        yield return new WaitForSeconds(1f);
        
        is_glowing = false;
        m_textGlow_sns = !(m_textGlow_sns);
    }





    // =======================
    //       GUN LEVEL-UP
    // =======================
    public void Gun_levelUp(int level)
    {
        if(level == 2) 
            saved_prefabPos_ = (gun_ui.transform.GetChild(gun_ui.transform.childCount - 1)).GetChild(0).position;

        if(gun_ui.transform.GetChild(gun_ui.transform.childCount - 1).GetChild(0).gameObject != null)
        {
            Destroy(
                (gun_ui.transform.GetChild(gun_ui.transform.childCount - 1)).GetChild(0).gameObject
            );
        }

        GameObject ui_gunz = Instantiate (
            gun_fetchedPrefabs[level - 1],
            saved_prefabPos_, 
            saved_prefabRotation_, 
            gun_ui.transform.GetChild(gun_ui.transform.childCount - 1)
        );
        ui_gunz.SetActive(false);

        Transform[] ui_t = ui_gunz.GetComponentsInChildren<Transform>();
        for(int j = 0; j < ui_t.Length; j ++)
        { 
            ui_t[j].gameObject.layer = 5;  // => UI LAYER
        } 

        ui_gunz.transform.localScale = saved_prefabScale_;
        ui_gunz.SetActive(true);


        if(level == 1)
        {
            LeanTween.moveLocal(gun_ui, new Vector3(0, 0, gun_ui.transform.position.z), 0.7f).setEaseInOutCubic();
            LeanTween.scale(gun_ui, gun_ui.transform.localScale * 1.4f, 1.4f).setEasePunch();
            LeanTween.rotateZ(gun_ui, 20f, 1.8f).setEasePunch();

             
        }else
        {
            LeanTween.scale(gun_ui, gun_ui.transform.localScale * 1.35f, 1.2f).setEasePunch();
            LeanTween.rotateZ(gun_ui, 40f, 1f).setEasePunch();
            Transform[] rects_ = (gun_ui.transform.GetChild(0).gameObject).GetComponentsInChildren<Transform>();

            for(int i = 0; i < rects_.Length; i++)
            {
                if(rects_[i].gameObject == gun_ui.transform.GetChild(0).gameObject)
                    continue;
                MeshRenderer mr  = rects_[i].gameObject.GetComponent<MeshRenderer>();

                if( (i % 2) != 0)
                {  
                    mr.sharedMaterial = Gun_Materials[level < 2 ? level - 1 : (2 * (level -1 ))];
                } 
                else { 
                    mr.sharedMaterial = Gun_Materials[level < 2 ? level : (2 * (level -1 )) + 1];
                }
            }
        }

    }











    // =======================
    //       PLAYER_DAMAGE
    // =======================
    public void player_damage(int dmg_v)
    {
        if(player_health == 0)
            return ;

        if(player_health - dmg_v <= 0)
        {
            gameOver_ui("damage", Quaternion.identity);
            player_health = 0;
        }else
        {
            player_health -= dmg_v;
        }


        if(LeanTween.isTweening(healh_ui.transform.GetChild(2).GetChild(1).gameObject))
        {
            LeanTween.cancel(healh_ui.transform.GetChild(2).GetChild(1).gameObject);
        }

        float health = ( ( (float)player_health / (float)max_health) );

        LeanTween.scale(
            healh_ui.transform.GetChild(2).GetChild(1).gameObject, 
            new Vector3(health, 1f, 0f), 1f
        ).setEaseInOutCubic();
    } 




    // =====================
    //        RELOAD
    // =====================
    public void ui_reload(float reload_time)
    {
        weapon_reload.SetActive(true);
        Invoke("off_wReload", reload_time);

    }
    private void off_wReload(){weapon_reload.SetActive(false);}





    // public KILL method
    public void kill_ui()
    {
        // StartCoroutine()
        p_movement.player_kill();
    }




    // =====================
    //       GAME OVER
    // =====================
    public void gameOver_ui(
        string death_mode, Quaternion player_rotation, GameObject optional_wall = null
    )
    {
        switch(death_mode)
        {
            case "front":
            case "sideVoid":
                StartCoroutine(p_movement.game_Over(death_mode, optional_wall));
                break;
            case "damage":
                StartCoroutine(p_movement.game_Over(death_mode));
                break;
            case "void":
                StartCoroutine(p_movement.game_Over(death_mode));
                // p_movement.game_Over(death_mode);
                break;
        }
        return;
    }



    // =====================
    //        RESTART
    // =====================
    public void restart_ui(bool is_aGameOver)
    {
        // Menu ui
        for(int i = 0; i < 3; i ++)
        {
            GameObject restart_slice = transform.GetChild(4 + i).gameObject;

            StartCoroutine(fade_ui_obj(restart_slice, 2f, false));
            LeanTween.moveLocal(
                restart_slice, 
                (is_aGameOver) ? (
                        (transform.GetChild(4 + i).transform.localPosition)
                        + new Vector3(0f, 1700f, 0f)
                    ) : (
                        new Vector3(0f, -1700, 0f)
                    ), 
                2f
            ).setEaseInOutCubic();
        }

        // Gameplay ui
        for(int i = 0; i < 3; i ++)
        {
            GameObject gameplay_slice = transform.GetChild(0 + i).gameObject;

            StartCoroutine(fade_ui_obj(gameplay_slice, 2f, true));
            LeanTween.moveLocal(
                gameplay_slice, 
                (is_aGameOver) ? 
                    (new Vector3(gameplay_slice.transform.localPosition.x, 1700, gameplay_slice.transform.localPosition.z))
                    : 
                    (transform.GetChild(0 + i).transform.localPosition - new Vector3(0f, 1700, 0f))
                , 
                2f
            ).setEaseInOutCubic();
        }
    }






    // =====================
    //        FADE_UI
    // =====================
    private IEnumerator fade_ui_obj(GameObject obj_, float fade_time, bool fadeOut_ = false)
    {
        const int refresh_rate = 120;
        // const float fade_time = 1.5f;
   
        TextMeshProUGUI[] texts = obj_.transform.GetComponentsInChildren<TextMeshProUGUI>();
        SpriteRenderer[] sprites = obj_.transform.GetComponentsInChildren<SpriteRenderer>();
        Image[] images = obj_.transform.GetComponentsInChildren<Image>();
        TMPro.TextMeshPro[] texts_3D = obj_.transform.GetComponentsInChildren<TMPro.TextMeshPro>();

        float[][] alpha_refs = new float[4][]{new float[25], new float [25], new float [25], new float [25]};
        int r_ = 0;
    
        if(!fadeOut_)
        {
            for(int x = r_ = 0; x < texts.Length; x++){
                alpha_refs[0][r_] = texts[x].color.a;
                texts[x].color = new Color(texts[x].color.r, texts[x].color.g, texts[x].color.b, 0);
                r_++;
            }
            for(int y = r_ = 0; y < sprites.Length; y++){
                alpha_refs[1][r_] = sprites[y].color.a;
                sprites[y].color = new Color(sprites[y].color.r, sprites[y].color.g, sprites[y].color.b, 0);
                r_++;
            }
            for(int z = r_ = 0; z < images.Length; z++){
                alpha_refs[2][r_] = images[z].color.a;
                images[z].color = new Color(images[z].color.r, images[z].color.g, images[z].color.b, 0);
                r_++;
            }
            for(int a = r_ = 0; a < texts_3D.Length; a++){
                alpha_refs[3][r_] = texts_3D[a].color.a;
                texts_3D[a].color = new Color(texts_3D[a].color.r, texts_3D[a].color.g, texts_3D[a].color.b, 0);
                r_++;
            }
        }

        int i = 1;
        while(i < refresh_rate)
        {
            float tick = (fade_time) / (refresh_rate);
            float fade_outV =  1f - ((tick * i) / fade_time);
            
            for(int j = 0; j < (texts.Length); j ++)
            {
                texts[j].color = new Color( texts[j].color.r, texts[j].color.g, texts[j].color.b,
                    fadeOut_ ? (fade_outV): ((tick * i) / fade_time) * alpha_refs[0][j]
                );
            }
            for(int k = 0; k < (sprites.Length); k ++)
            {
                (sprites[k]).color = new Color( sprites[k].color.r, sprites[k].color.g, sprites[k].color.b,
                    fadeOut_ ? (fade_outV) : ((tick * i) / fade_time) * alpha_refs[1][k]
                );
            }
            for(int l = 0; l < (images.Length); l ++)
            {
                (images[l]).color = new Color( images[l].color.r, images[l].color.g, images[l].color.b,
                    fadeOut_ ? (fade_outV) : ((tick * i) / fade_time) * alpha_refs[2][l]
                );
            }
            for(int l = 0; l < (texts_3D.Length); l ++)
            {
                (texts_3D[l]).color = new Color( texts_3D[l].color.r, texts_3D[l].color.g, texts_3D[l].color.b,
                    fadeOut_ ? (fade_outV) : ((tick * i) / fade_time) * alpha_refs[3][l]
                );
            }

            yield return new WaitForSeconds(fade_time / refresh_rate);
            i++;
        }

    }


}
