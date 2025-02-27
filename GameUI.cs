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

    [Header ("RARITY COLORS")]
    [SerializeField] private GameObject[] weapon_rarity_partSys;
    private Color common = new Color((float)(200f/255f), (float)(227f/255f), (float)(255f/255f), 1f),
        rare = new Color((float)(40f/255f), (float)(255f/255f), 0f, 1f),
        super_rare = new Color(0f, (float)(155f/255f), (float)(255f/255f), 1f), 
        epic = new Color((float)(150f/255f), 0f, (float)(255f/255f), 1f),
        legendary = new Color((float)(255f/255f), (float)(187f/255f), 0f, 1f), 
        mythic = new Color((float)(255f/255f), (float)(20f/255f), 0f, 1f),
        eternal = new Color((float)(255f/255f), 0f, (float)(150f/255f), 1f);
        

    [Header ("ANNOUNCER")]
    [SerializeField] private GameObject ui_announcer_;
    [SerializeField] private GameObject _3d_ui_announcer;

    [Header ("TOP -- LEFT")]
    [SerializeField] private GameObject healh_ui;
    [SerializeField] private GameObject money_ui;

    [Header ("TOP -- RIGHT")]
    [SerializeField] private GameObject gun_ui;
    [SerializeField] private TextMeshPro ammo_ui;


    [Header ("TOP -- CENTER")]
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private TextMeshProUGUI timer_txt;  
    [SerializeField] private TextMeshProUGUI fps_txt;  

    [Header ("PLAYER HEALTH")]
    private TextMeshProUGUI healh_text;
    private int max_health = 10000;
    private int player_health = 0; // = 200;
    private int set_health
    {
        get { return 0; }
        set{ if(value > max_health) player_health = max_health; else value = max_health;}
    }

    [Header ("WEAPON")]
    private Weapon weapon_script;
    private TextMeshPro[] weapon_ammoTxts;
    private GameObject weapon_reload;
    private float reload_timerFloat = 0f;
    private int weapon_level = 0;

    [Header ("MONEY")]
    private TextMeshProUGUI[] money_bfr = new TextMeshProUGUI[20]; // 20 sized gm bfr
    private bool m_textGlow_sns = false, is_glowing = false;
    private TextMeshProUGUI money_txt;
    private int player_money = 0;
    private int money_i = 0, money_v = 0;
    private const float money_delay = 0.0075f;
    private float money_timer = 0f;


    [Header ("TIMER & SCORE & FPS_COUNTER")]
    private float timer_ = 0f, fps_timer = 0f;
    private const float fps_polling_time = 0.2f;
    private bool countScore_ = false, countBonus_ = false;
    private uint lastZ = 0, score = 0;
    private decimal v_score = 0;
    private int frame_count = 0, temp_bonus = 0;
    [HideInInspector] public bool set_countBonus_
    {
        get { return false; }
        set{ if(value.GetType() == typeof(bool)) countBonus_ = value;}
    }


    [Header ("UI Objects")]
    [SerializeField] private Transform[] parents_2d;

    [Header ("PLAYER && Scripts")]
    private Weapon weapon_scrpt;
    private PlayerMovement p_movement;
    private Buildings building_s;
    private Transform plyr_transform;
    private Rigidbody plyr_rgBody;


    [Header ("KILLS")]
    [SerializeField] private GameObject kill_obj;



    [Header ("GUN UI")]
    [SerializeField] private Material[] Gun_Materials;
    private Vector3 saved_prefabPos_, saved_prefabScale_;
    private Quaternion saved_prefabRotation_;
    private GameObject[] gun_fetchedPrefabs;


    [Header ("DAMAGES HITS && ENEMY HEALTH BAR")]
    [SerializeField] private GameObject enemy_health_bar;
    [SerializeField] private GameObject damage_hit;
    [SerializeField] private Transform er;
    [SerializeField] private Vector3 aimed_offset;
    private Canvas _canvas_;
    private Vector3 _canvas_rectSize = Vector3.zero;
    private Vector3 h_bar_initalScale;
    private struct t_enemy_ui
    {
        public Transform _tr;
        public GameObject _healthBar;
        public Vector3 _healthBar_offset;
        public AutoTurret _auto_turret;
        private Enemy _enemy;
        public RectTransform _rt;

        // [Constructor] to initialize struct
        public t_enemy_ui(Vector3 v,
            Transform t, AutoTurret at, 
            Enemy en, GameObject go)
        {
            this._healthBar_offset = (v);
            this._tr = t;
            this._auto_turret = at;
            this._enemy = en;
            this._healthBar = go;
            this._rt = go.GetComponent<RectTransform>();
        }
        // modify position
        public void set_pos(Vector3 v_, float canvas_scale)
        {
            // this._healthBar.transform.position = v_;
            this._rt.anchoredPosition = new Vector2(
                (v_.x) * canvas_scale, 
                (v_.y) * (canvas_scale * 0.80f)
            );

            this._healthBar.transform.localPosition = new Vector3(
                this._healthBar.transform.localPosition.x, 
                this._healthBar.transform.localPosition.y, 
                v_.z
            );
        }
        // empty struct
        public void delete()
        {
            this._enemy = null;
            this._auto_turret = null;
            this._tr = null;
            this._healthBar = (null);
        }
    }
    private t_enemy_ui[] ui_enemies = new t_enemy_ui[25];
    /*    
    /////////////////////////////////////////////
    [SerializeField] private GameObject damage_obj;
    private GameObject[] damage_hits = new GameObject[40]; // 25 fixed bfr
    */
    private struct t_damage_ui
    {
        public GameObject _damageHit;
        public Transform _hit_enemy;
        public Vector3 _offset;
        public Vector3 _hit_gap;
        public RectTransform _rt;
        // [Constructor] to initialize struct
        public t_damage_ui(
            Transform enemy,
            Vector3 offset, 
            Vector3 g,
            GameObject go
        ){
            this._hit_enemy = (enemy);
            this._offset = offset;
            this._hit_gap = (g);
            this._damageHit = (go);
            this._rt = go.GetComponent<RectTransform>();
        }
        // empty struct
        public void delete()
        {
            this._damageHit = null;
            this._hit_enemy = null;
        }  
    }
    private t_damage_ui[] ui_damageHits = new t_damage_ui[75];
    [HideInInspector] public int used_playerRange = 0;



    [Header ("COMBO")]
    [SerializeField] private Sprite[] combo_masks;
    [SerializeField] private GameObject combo_obj;
    private string combo_text = "Good";
    private int combo_v = 0, combo_goal = 3, last_combo_s = 0;
    private LTDescr combo_lt;



    // [Awake]
    private void Awake()
    {
        player_health = max_health;

        weapon_scrpt = FindObjectOfType<Weapon>();
        building_s = FindObjectOfType<Buildings>();
        
        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        p_movement = pm;
        plyr_transform = pm.transform;
        plyr_rgBody  = pm.transform.GetComponent<Rigidbody>();
        _canvas_ = gameObject.GetComponent<Canvas>();
        RectTransform rt = (_canvas_).gameObject.GetComponent<RectTransform>();
        _canvas_rectSize = new Vector3(rt.rect.width, rt.rect.height, 0f);
        h_bar_initalScale = (enemy_health_bar).transform.localScale;

        Debug.Log("DEVICE: "+ Application.platform);
        Debug.Log("FRAMERATE: " + Screen.currentResolution.refreshRate);
        Debug.Log("[SCREEN] RESOLUTION:  " + Screen.currentResolution.width + "x" + Screen.currentResolution.height);
        Debug.Log("[SCREEN] X-Y:   " + Screen.width+ " x " + Screen.height);
        Debug.Log("[CANVAS] X-Y:   " + _canvas_rectSize.x + " x " + _canvas_rectSize.y);

       
        if (Application.platform == RuntimePlatform.Android 
            || (Application.platform == RuntimePlatform.IPhonePlayer)
            || (Application.platform == RuntimePlatform.OSXPlayer)
            || (Application.isMobilePlatform)
        ){
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            if (Screen.currentResolution.refreshRate > 61f)
            {
                Application.targetFrameRate = 90;
            }
            // Application.targetFrameRate = Screen.currentResolution.refreshRate;

            p_movement.set_build_mode = true;
            FindObjectOfType<CameraMovement>().is_build = true;
            FindObjectOfType<PlayerCollisions>().is_build = true;
        }else
        {
            p_movement.set_build_mode = false;
            FindObjectOfType<CameraMovement>().is_build = false;
            FindObjectOfType<PlayerCollisions>().is_build = false;
        }
    }




    // [Start]
    private void Start()
    {
        gun_fetchedPrefabs = weapon_scrpt.get_weaponsResourcesPrefab_buffer;

        int l_prefb = gun_ui.transform.childCount - 1;
        saved_prefabPos_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.position;
        saved_prefabScale_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.localScale;
        saved_prefabRotation_ = gun_ui.transform.GetChild(l_prefb).GetChild(0).transform.localRotation;

        // money txt
        money_txt = money_ui.transform.GetChild(1).GetComponent<TextMeshProUGUI>();


        // weapon ammo txt
        weapon_ammoTxts = new TextMeshPro[2]{
            gun_ui.transform.GetChild(1).GetChild(2).GetComponent<TextMeshPro>(), // stack
            gun_ui.transform.GetChild(1).GetChild(1).GetComponent<TextMeshPro>(), // magz
        };


        // weapon reload
        weapon_reload = gun_ui.transform.parent.GetChild(1).gameObject;
        weapon_reload.SetActive(false);

        // health 
        healh_text = healh_ui.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        healh_ui.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = max_health.ToString();

        Invoke("countScore", 0.25f);
    }
    private void countScore(){ countScore_ = true; }


    


    // [Update]
    private void Update()
    {
        timer_ += Time.deltaTime;
        fps_timer += Time.deltaTime;

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

        
        // fps counter
        frame_count++;
        if(fps_timer >= fps_polling_time)
        {
            int frameRate = Mathf.RoundToInt((frame_count)/(fps_timer));
            fps_txt.text = frameRate.ToString() + " FPS";

            fps_timer -= fps_polling_time;
            frame_count = 0;
        }
    }



    // FixedUpdate()
    private void FixedUpdate()
    {
        // ammo
        if( ammo_ui.text.ToString() != weapon_scrpt.get_ammo.ToString())
        {
            ammo_ui.text = weapon_scrpt.get_ammo.ToString();
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




    // LateUpdate()
    private void LateUpdate()
    {
        // floating damages
        /* if(ui_damageHits[0]._damageHit != null) */
        for(int i = 0; i < ui_damageHits.Length; i++)
        {
            if(ui_damageHits[i]._hit_enemy == null)
                continue;

            if(ui_damageHits[i]._hit_enemy.position.z < plyr_transform.position.z)
            {
                Destroy(ui_damageHits[i]._damageHit);
                ui_damageHits[i].delete();
                continue;
            }else
            {   
                // Step 1: Convert the target object's world position to screen space
                Vector3 screen_pos = Camera.main.WorldToViewportPoint(
                    ui_damageHits[i]._hit_enemy.position
                    + (ui_damageHits[i]._hit_gap)
                    + (ui_damageHits[i]._offset)
                );

                // Step 2: Convert the screen position to local position relative to the canvas
                Vector2 v2_pos = new Vector2(
                    (screen_pos.x) * (_canvas_rectSize.x), 
                    (screen_pos.y) * (_canvas_rectSize.y)
                );

                // Step 3: Apply the position to UI element && remove Z-axis
                ui_damageHits[i]._rt.anchoredPosition = (v2_pos);
                ui_damageHits[i]._damageHit.transform.localPosition = new Vector3(
                    ui_damageHits[i]._damageHit.transform.localPosition.x,
                    ui_damageHits[i]._damageHit.transform.localPosition.y,
                    0f
                );
                
            }
        }
        

        // enemies ui
        /* if(ui_enemies[0]._tr != null) */
        for(int i = 0; i < ui_enemies.Length; i++)
        {
            if(ui_enemies[i]._tr == null)
                continue;
            /*
            float distanceToCamera = Vector3.Distance(ui_camera.transform.position, enemies_tr[i].position);
            float cam_scale = (
                    2.0f * (distanceToCamera * 0.55f) * 
                    Mathf.Tan(Mathf.Deg2Rad * (ui_camera.fieldOfView * 0.5f)
                )
            ) * (7f);
            cam_scale = (cam_scale < 105f) ? 105f : (cam_scale > 150f ? 150f : cam_scale);

            ui_enemies[i].transform.localScale = new Vector3(cam_scale, cam_scale, cam_scale);

            ui_enemies[i].transform.position = enemies_tr[i].position + enemies_offst[i];
            ui_enemies[i].transform.LookAt(ui_camera.transform);        
            ui_enemies[i].transform.rotation = Quaternion.Euler(
                -1 * ui_enemies[i].transform.rotation.eulerAngles.x,
                ui_enemies[i].transform.rotation.eulerAngles.y +  180f,
                ui_enemies[i].transform.rotation.eulerAngles.z
            );
            */
            if(ui_enemies[i]._tr.position.z < plyr_transform.position.z)
            {
                Destroy(ui_enemies[i]._healthBar);
                ui_enemies[i].delete();
                continue;
            }else
            {   
                // Step 1: Convert the target object's world position to screen space
                Vector3 screen_pos = Camera.main.WorldToViewportPoint(
                    (ui_enemies[i]._tr.position) +  (aimed_offset)
                    + (ui_enemies[i]._healthBar_offset)
                );

                // Step 2: Convert the screen position to local position relative to the canvas
                Vector2 v2_pos = new Vector2(
                    (screen_pos.x) * (_canvas_rectSize.x), 
                    (screen_pos.y) * (_canvas_rectSize.y)
                );

                // Step 3: Apply the position to UI element && remove Z-axis
                ui_enemies[i]._rt.anchoredPosition = (v2_pos);
                ui_enemies[i]._healthBar.transform.localPosition = new Vector3(
                    ui_enemies[i]._healthBar.transform.localPosition.x, ui_enemies[i]._healthBar.transform.localPosition.y, 0f
                );
                
                // Step 4: Attribute scale relative to player distance
                if(ui_enemies[i]._tr.position.z > (plyr_transform.position.z + (used_playerRange * 0.625f)))
                {
                    ui_enemies[i]._healthBar.transform.localScale = new Vector3(
                        h_bar_initalScale.x * 0.65f, h_bar_initalScale.y * 0.65f, 1f
                    );
                }else
                {
                    ui_enemies[i]._healthBar.transform.localScale = (h_bar_initalScale);
                }
            }
        }
    }






    // ==============================================
    //                  ENEMY UI
    // ==============================================
    public void newEnemy_UI(Transform enemy_, bool destroy_enemy_ui = false)
    {
        if(enemy_ == null)
                return;
        if(destroy_enemy_ui)
        {
            for(int i = 0; i < 25; i ++)
            {
                if(ui_enemies[i]._tr == null)
                    continue;
                if(GameObject.ReferenceEquals(ui_enemies[i]._tr.gameObject, (enemy_.gameObject)))
                {
                    Destroy(ui_enemies[i]._healthBar);
                    ui_enemies[i].delete();
                    return;
                }
            }
        }else
        {    
            for(int i = 0; i < 25; i ++)
            {
                if(ui_enemies[i]._tr == null)
                    continue;
                if(GameObject.ReferenceEquals(ui_enemies[i]._tr.gameObject, (enemy_.gameObject)))
                    return;
            }            
            for(int i = 0; i < 25; i ++)
            {
                if(ui_enemies[i]._tr == null)
                {
                    GameObject health_bar = Instantiate(
                        enemy_health_bar, 
                        new Vector3(0f, 1f, 0f),
                        Quaternion.identity,
                        enemy_health_bar.transform.parent
                    );
                    AutoTurret at_turret = enemy_.GetComponent<AutoTurret>();
                    Enemy en_ = enemy_.GetComponent<Enemy>();
                    Vector3 offset = ((at_turret != null) ?
                        (
                            ((bool)(at_turret?.is_horizontal)) ?
                            ((bool)(at_turret?.is_left) ? (new Vector3(2f, 2.2f, 0f)) :  (new Vector3(-2f, 2.2f, 0f)))
                            :
                            (new Vector3(0f, 3.40f, 0f))
                        )
                        : 
                        ( new Vector3(0f, 1f, 0f) )
                    );
                    ui_enemies[i] = new t_enemy_ui(
                        offset,
                        enemy_,
                        at_turret,
                        en_,
                        health_bar
                    );

                    StartCoroutine(fade_ui_obj(ui_enemies[i]._healthBar, 0.5f));
                    Transform t = health_bar.transform;

                    int enemy_health = at_turret.get_health;
                    int enemy_maxhealth = at_turret.get_maxHealth;

                    // // Name & Level Assignation
                    if(at_turret != null)
                    {
                        t.GetChild(3).GetComponent<TextMeshProUGUI>().text = at_turret.turret_name;
                        t.GetChild(4).GetComponent<TextMeshProUGUI>().text = "LEVEL "  + at_turret.turret_level.ToString();
                        t.GetChild(5).GetComponent<TextMeshProUGUI>().text = enemy_health.ToString() + "/"  + enemy_maxhealth.ToString();
                    }

                    // // Health Attribution
                    float x_health = (((float)enemy_health / (float)enemy_maxhealth) * 100f);
                    t.GetChild(1).localScale = t.GetChild(2).localScale = new Vector3( (float.IsNaN(x_health) ? 0 : x_health) / 100, 1, 1);
                    return;
                }
            }
        }
    }




    // ==============================================
    //                  ENEMY_DAMAGE
    // ==============================================
    public void damage_ui(Transform enemy, int damage_value, bool? is_crit_hit, Vector3 hit_gap)
    {        
        if(damage_value.GetType() != typeof(int) || enemy == null)
            return;

        // float damage
        for(int i = 0; i < ui_damageHits.Length; i ++)
        {
            if(ui_damageHits[i]._damageHit == null)
            {
                GameObject t_hit = Instantiate(
                    damage_hit,
                    new Vector3(-100f, -100f, 0f), 
                    Quaternion.identity,
                    damage_hit.transform.parent
                ); 
                Vector3 off_set = new Vector3(
                    UnityEngine.Random.Range(-0.2f, 0.2f),
                    (0.15f),
                    0f
                );
                ui_damageHits[i] = new t_damage_ui(
                    enemy,
                    off_set,
                    hit_gap,
                    (t_hit)
                );

                if(damage_value > 0){
                    LeanTween.value(t_hit, 
                        ui_damageHits[i]._offset, 
                        ui_damageHits[i]._offset + new Vector3(
                            UnityEngine.Random.Range(-0.5f, 0.5f),
                             UnityEngine.Random.Range(0.25f, 2.5f), 
                            0f
                        ), 
                    1.2f).setOnUpdate((Vector3 value) => {
                        ui_damageHits[i]._offset = (value); 
                    });
                }
            
                TextMeshProUGUI txt = t_hit.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if(damage_value < 0)
                {
                    txt.text = "MISS!";        
                }else
                {
                    if(is_crit_hit == null) 
                        txt.text = "CRTICIAL";
                    else
                        txt.text = damage_value.ToString();
                }

                LeanTween.scale(t_hit, t_hit.transform.localScale * 1.6f, 1.2f).setEasePunch();
                StartCoroutine(fade_ui_obj(t_hit, 1f, true));

                if(damage_value > 0)
                    if((bool?)is_crit_hit == true)
                        damage_ui(enemy, 0, null, hit_gap + new Vector3( UnityEngine.Random.Range(-1.5f, 1.5f), 0f, 0f));

                break;
            }
        }
        

        // health bar
        if(enemy != null)
        {
            for(int j = 0; j < 25; j++)
            {
                if(ui_enemies[j]._tr == null)
                    continue;
                if(GameObject.ReferenceEquals(ui_enemies[j]._tr.gameObject, (enemy.gameObject)))
                {
                    AutoTurret a = ui_enemies[j]._auto_turret;
                    float x_health = (((float)(a.get_health) / (float)(a.get_maxHealth)) * 100f);
                    Transform t = ui_enemies[j]._healthBar.transform;

                    if(x_health == 0)
                    {
                        StartCoroutine(fade_ui_obj(ui_enemies[j]._healthBar, 0.6f, true));
                    }

                    LeanTween.scale(
                        t.GetChild(1).gameObject, new Vector3( (float.IsNaN(x_health) ? 0 : x_health) / 100, 1, 1), 
                    0.3f).setEaseInOutCubic();
                    LeanTween.scale(
                        t.GetChild(2).gameObject, new Vector3( (float.IsNaN(x_health) ? 0 : x_health) / 100, 1, 1), 
                    0.15f).setEaseInOutCubic();
                } 
            }
        }
    }






    // =======================
    //          COMBO
    // =======================
    public int combo_hit(bool combo_end)
    {
        GameObject combo_bar = combo_obj.transform.GetChild(0).GetChild(0).gameObject;
        TextMeshProUGUI combo_txt = combo_obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        GameObject combo_txt_obj = combo_obj.transform.GetChild(1).gameObject;

        if(combo_end)   
        {
            combo_v = last_combo_s = combo_goal = 0;
            StartCoroutine(fade_ui_obj(combo_obj, 1.5f, true));
            LeanTween.moveLocal(combo_obj,
                combo_obj.transform.localPosition +  new Vector3 (0, -350, 0), 
            1.3f).setEaseInSine();
            LeanTween.scale(combo_bar.transform.GetChild(1).gameObject, 
                new Vector3( 0, 1, 1), 0.65f).setEaseInSine();
            LeanTween.scale(combo_bar.transform.GetChild(0).gameObject, 
                    new Vector3(0, 1, 1), 0.3f).setEaseInSine();

            return (0);
        }
        else
        {
            combo_v ++;
            last_combo_s ++;
            combo_obj.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "X" + combo_v.ToString() + "/" + combo_goal.ToString();


            // COMBO Initalization
            if(combo_v == 1)
            {
                Image combo_msk = combo_obj.transform.GetChild(0).GetComponent<Image>();
                int randm_flt_vrt =  UnityEngine.Random.Range(0, 3);

                combo_obj.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "X" + combo_v.ToString() + "/" + combo_goal.ToString();
                combo_goal = (randm_flt_vrt + 1);
                combo_msk.sprite = combo_masks[randm_flt_vrt > 0 ? (randm_flt_vrt - 1) : 0];
                combo_txt.text = "";

                StartCoroutine(fade_ui_obj(combo_obj, 1.5f, false));
                LeanTween.moveLocal(combo_obj, (combo_obj.transform.localPosition) + new Vector3 (0, 350, 0), 1.3f).setEaseInSine();
            }

            // COMBO Hit
            if(combo_v > 1)
            {
                // yellow
                LeanTween.scale(combo_bar.transform.GetChild(1).gameObject, 
                    new Vector3( (
                        (float)last_combo_s / (float)(combo_goal - (combo_v - last_combo_s))
                    ), 1, 1), 0.65f).setEaseInSine();
                // white
                LeanTween.scale(combo_bar.transform.GetChild(0).gameObject, 
                    new Vector3( (
                        (float)last_combo_s / (float)combo_goal - (combo_v - last_combo_s))
                    , 1, 1), 0.3f).setEaseInSine();

                // tick
                GameObject tick = combo_obj.transform.GetChild(2).gameObject;
                Image[] im = tick.transform.GetComponentsInChildren<Image>();
                LeanTween.value( tick, tick_f, 0f, 1f, 2.4f).setEasePunch();
                void tick_f( float val )
                {
                    im[0].color = im[1].color = new Color(im[0].color.r, im[0].color.g, im[0].color.b, val);

                }
            }

            // COMBO GoalHit
            if(combo_v == combo_goal) // PERFECT !
            {
                GameObject combo_x1 = combo_obj.transform.GetChild(3).gameObject;

                // Reset Shine Position [x2 and text]
                combo_x1.transform.GetChild(0).localPosition = combo_txt_obj.transform.GetChild(0).localPosition = new Vector3(-250, 0, 0);

                combo_txt_obj.transform.localPosition = new Vector3(0, 0f, 0);
                combo_txt_obj.transform.localScale = Vector3.one * 0.8f;
                combo_txt.text = "PERFECT";

                // combo_txt [MoveLocal, Fade]
                StartCoroutine(fade_ui_obj(combo_txt_obj, 1f));
                LeanTween.moveLocal(combo_txt_obj, combo_txt_obj.transform.localPosition + new Vector3(0f, 57f , 0f), 1f).setEaseInSine();
                LeanTween.scale(combo_txt_obj, new Vector3(1.05f, 1.05f, 1), 0.8f).setEaseInSine();

                // [x2, combo_txt] shine
                LeanTween.moveLocal(combo_txt_obj.transform.GetChild(0).gameObject, combo_txt_obj.transform.GetChild(0).transform.localPosition + new Vector3(500, 0, 0), 1.8f).setEaseInSine();
                LeanTween.moveLocal(combo_x1.transform.GetChild(0).gameObject, combo_x1.transform.GetChild(0).transform.localPosition + new Vector3(700, 0, 0), 1.5f).setEaseInSine();
                
                StartCoroutine(combo_next());

                return (1);
            }else
            {
                return (0);
            }
        }
    }
    private IEnumerator combo_next()
    {
        yield return new WaitForSeconds(1.7f);
        GameObject combo_bar = combo_obj.transform.GetChild(0).GetChild(0).gameObject;
        GameObject combo_txt_obj = combo_obj.transform.GetChild(1).gameObject;

        LeanTween.cancel(combo_bar);
        LeanTween.cancel(combo_txt_obj);
        // scale down
        // yellow
        LeanTween.scale(combo_bar.transform.GetChild(1).gameObject, new Vector3(0f, 1f, 1f), 0.45f).setEaseInSine();
        // white 
        LeanTween.scale(combo_bar.transform.GetChild(0).gameObject, new Vector3(0f, 1f, 1f), 0.30f).setEaseInSine();

        StartCoroutine(fade_ui_obj(combo_txt_obj, 0.5f, true));

        // new combo goal
        int randm_flt_vrt =  UnityEngine.Random.Range(0, 3);

        combo_goal +=(randm_flt_vrt + 1);
        last_combo_s = 0;

        Image combo_msk = combo_obj.transform.GetChild(0).GetComponent<Image>();
        combo_msk.sprite = combo_masks[randm_flt_vrt > 0 ? (randm_flt_vrt - 1) : 0];

        combo_obj.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "X" + combo_v.ToString() + "/" + combo_goal.ToString();
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
        building_s.set_game_over = true;

        switch(death_mode)
        {
            case "front":
            case "sideVoid":
                StartCoroutine(p_movement.game_Over(death_mode, optional_wall));
                break;
            case "damage":
            case "turretCollision":
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
                        + new Vector3(0f, 2000f, 0f)
                    ) : (
                        new Vector3(0f, -2000f, 0f)
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
    //        GAIN_HEALTH
    // =====================
    public void gain_health(int health_v)
    {
        set_health = player_health + health_v;
    }



    // =====================
    //        GAIN_AMMO
    // =====================
    public void gain_ammo(int ammo_v)
    {
        
    }
    

    // =====================
    //       ANNOUNCER
    // =====================
    public void ui_announcer(string s)
    {
        if(s.Length == 0)
            return ;

        switch(s)
        {
            case "weapon_levelUp":
                for(int i = 0; i < ui_announcer_.transform.childCount; i++)
                {
                    GameObject go = ui_announcer_.transform.GetChild(i).gameObject;
                    if(go.activeSelf && (go.ToString()).Contains("GUN_LEVELUP"))
                        Destroy(go);
                }
                GameObject g = ui_announcer_.transform.GetChild(0).gameObject;
                string[] rarity_names = new string[7] 
                    { "COMMON", "RARE", "SUPER RARE", "EPIC", "LEGENDARY", "MYTHIC", "ETERNAL"};
                Color[] rarity_colors_c = new Color[7]
                    { common, rare, super_rare, epic, legendary, mythic, eternal };


                // 2D-UI [WEAPON LEVEL UP]
                GameObject new_g = Instantiate(g, Vector3.zero, Quaternion.identity, ui_announcer_.transform);
                new_g.transform.localPosition = new Vector3(0f, -120f, 0f);
                new_g.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                new_g.SetActive(true);
                TextMeshProUGUI txt = new_g.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                txt.text = rarity_names[weapon_level]; txt.color = rarity_colors_c[weapon_level];
                LeanTween.moveLocal(new_g, new_g.transform.localPosition + new Vector3(0f, 120f, 0f), 2.3f).setEaseInOutCubic();
                LeanTween.moveLocal(new_g.transform.GetChild(0).gameObject, new_g.transform.GetChild(0).localPosition + new Vector3(0f, 100f, 0f), 3f).setEaseInOutCubic();
                LeanTween.scale(new_g, Vector3.one, 0.5f).setEaseInOutCubic();
                StartCoroutine(fade_ui_obj(new_g, 1.4f));


                // 3D-GUN [WEAPON LEVEL UP]
                GameObject gun_3D = Instantiate (
                    gun_fetchedPrefabs[weapon_level],
                    Vector3.zero, Quaternion.Euler(100f, 0f, 40f), 
                    _3d_ui_announcer.transform.GetChild(0)
                );
                GameObject shine_p_Sys = Instantiate(
                    weapon_rarity_partSys[weapon_level], 
                    Vector3.zero, Quaternion.identity, _3d_ui_announcer.transform.GetChild(0) 
                );
                shine_p_Sys.transform.localPosition = gun_3D.transform.localPosition = (
                        Vector3.zero + new Vector3(0f, -600f, 150f)
                );
                shine_p_Sys.transform.localScale = gun_3D.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                shine_p_Sys.layer = gun_3D.layer = 5; // UI Layer
                Transform[] trs_ = gun_3D.GetComponentsInChildren<Transform>(), 
                        trs_p = shine_p_Sys.GetComponentsInChildren<Transform>();
                for(int i = 0; i < trs_.Length; i ++)
                    trs_[i].gameObject.layer = 5;
                for(int j = 0; j < trs_p.Length; j ++)
                    trs_p[j].gameObject.layer = 5;

                LeanTween.scale(gun_3D, Vector3.one * 38f, 1.7f).setEaseInOutCubic();
                LeanTween.scale(shine_p_Sys, Vector3.one * 460f, 1.7f).setEaseInOutCubic();

                LeanTween.moveLocal(gun_3D, new Vector3(0f, 50f, 150f), 2f).setEaseInOutCubic();
                LeanTween.moveLocal(shine_p_Sys, new Vector3(0f, 50f, 300f), 2f).setEaseInOutCubic();

                LeanTween.rotateY(gun_3D, -90f, 3.5f).setEaseInOutCubic();
                LeanTween.rotateZ(gun_3D, 30f, 2f).setEasePunch();
                shine_p_Sys.SetActive(true);
                (shine_p_Sys.GetComponent<ParticleSystem>()).Play();

                // run fade out routines
                StartCoroutine(ui_announcer_out(shine_p_Sys, 3.2f, "3d_weapon_levelUp"));
                StartCoroutine(ui_announcer_out(gun_3D, 3.2f, "3d_weapon_levelUp"));
                StartCoroutine(ui_announcer_out(new_g, 2.5f, s));

                weapon_level ++;
                for(int i = 0; i < 7; i++) // stars
                {
                    Transform stars_l = new_g.transform.GetChild(5), stars_r = new_g.transform.GetChild(6);
                    stars_l.GetChild(6 - i).gameObject.SetActive((i < weapon_level) ? (true) : (false));
                    stars_r.GetChild(i).gameObject.SetActive((i < weapon_level) ? (true) : (false));
                }
                break;

            case "start_":
                GameObject go_ = ui_announcer_.transform.GetChild(1).gameObject;
                go_.transform.localPosition = go_.transform.localPosition + new Vector3(0f, -50f, 0f);

                go_.SetActive(true);
                LeanTween.moveLocal(go_, go_.transform.localPosition + new Vector3(0f, 50f, 0f), 0.6f).setEaseInOutCubic();

                StartCoroutine(fade_ui_obj(go_, 0.6f));
                StartCoroutine(ui_announcer_out(go_, 2.5f, "3d_weapon_levelUp"));
                break;
        }
    }
    private IEnumerator ui_announcer_out(GameObject obj_to_fadeOut, float t_, string m_)
    {
        yield return new WaitForSeconds(t_);
        switch(m_)
        {
            case "3d_weapon_levelUp":
                // Destroy(obj_to_fadeOut);
                LeanTween.scale(obj_to_fadeOut, new Vector3(0.001f, 0.001f, 0.001f), 0.2f).setEaseOutCubic();
                break;
            case "weapon_levelUp":
                StartCoroutine(fade_ui_obj(obj_to_fadeOut, 0.65f, true));
                break;
            case "start_":
                break;
        }
    }




    // =====================
    //        FADE_UI
    // =====================
    private IEnumerator fade_ui_obj(GameObject obj_, float fade_time, bool fadeOut_ = false, bool fadeIntAndOut_ = false)
    {
        // yield break;
        const int refresh_rate = 120;
        float tick  = (fade_time) / (refresh_rate);
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
                alpha_refs[0][r_] = texts[x].color.a > 10f ? texts[x].color.a  : 1;
                texts[x].color = new Color(texts[x].color.r, texts[x].color.g, texts[x].color.b, 0);
                r_++;
            }
            for(int y = r_ = 0; y < sprites.Length; y++){
                alpha_refs[1][r_] = sprites[y].color.a > 10f ? sprites[y].color.a: 1;
                sprites[y].color = new Color(sprites[y].color.r, sprites[y].color.g, sprites[y].color.b, 0);
                r_++;
            }
            for(int z = r_ = 0; z < images.Length; z++){
                alpha_refs[2][r_] = images[z].color.a > 10f ? images[z].color.a : 1;
                images[z].color = new Color(images[z].color.r, images[z].color.g, images[z].color.b, 0);
                r_++;
            }
            for(int a = r_ = 0; a < texts_3D.Length; a++){
                alpha_refs[3][r_] = texts_3D[a].color.a > 10f ? texts_3D[a].color.a : 1;
                texts_3D[a].color = new Color(texts_3D[a].color.r, texts_3D[a].color.g, texts_3D[a].color.b, 0);
                r_++;
            }
        }

        int i = 1;
        while(i < refresh_rate)
        {
            float fade_outV =  1f - ((tick * i) / fade_time);
            float fade_inV =  (tick * i) / fade_time;

            for(int j = 0; j < (texts.Length); j ++)
            {
                if(texts[j] == null)
                    continue; 
                texts[j].color = new Color( texts[j].color.r, texts[j].color.g, texts[j].color.b,
                    fadeOut_ ? (fade_outV): (fade_inV * alpha_refs[0][j])
                );
            }
            for(int k = 0; k < (sprites.Length); k ++)
            {
                if(sprites[k] == null)
                    continue;
                (sprites[k]).color = new Color( sprites[k].color.r, sprites[k].color.g, sprites[k].color.b,
                    fadeOut_ ? (fade_outV) : (fade_inV * alpha_refs[1][k])
                );
            }
            for(int l = 0; l < (images.Length); l ++)
            {
                if(images[l] == null)
                    continue;
                (images[l]).color = new Color( images[l].color.r, images[l].color.g, images[l].color.b,
                    fadeOut_ ? (fade_outV) : (fade_inV * alpha_refs[2][l])
                );
            }
            for(int l = 0; l < (texts_3D.Length); l ++)
            {
                if(texts_3D[l])
                    continue;
                (texts_3D[l]).color = new Color( texts_3D[l].color.r, texts_3D[l].color.g, texts_3D[l].color.b,
                    fadeOut_ ? (fade_outV) : (fade_inV * alpha_refs[3][l])
                );
            }

            yield return new WaitForSeconds(fade_time / refresh_rate);
            i++;
        }

        if(i == refresh_rate && fadeIntAndOut_)
        {
            StartCoroutine(fade_ui_obj(obj_, 0.1f, true));
            yield break;
        }else
            yield break;
    }


}
