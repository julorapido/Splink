using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;


public class GameUI : MonoBehaviour
{
    [Header ("Player Stats")]
    private int max_health = 200;
    private int player_health = 200;
    private int set_health
    {
        get { return 0; }
        set{ if(value > max_health) player_health = max_health; else value = max_health;}
    }

    [Header ("Weapon")]
    [SerializeField] private GameObject weapon_ui_obj;
    private Weapon weapon_script;
    private TextMeshPro[] weapon_ammoTxts;
    private GameObject weapon_reload;
    private float reload_timerFloat = 0f;

    [Header ("Restart")]
    private Vector3[] saved_ui_initalPos = new Vector3[10];
    private Transform[] saved_restartUi_transforms = new Transform[35];
    private static IDictionary<TextMeshProUGUI?, Image?>[] saved_menuChilds = new Dictionary<TextMeshProUGUI?, Image?>[35];
    private Vector3[] saved_restartUi_scales = new Vector3[35];
    private bool restart_uiNeeds_fade = false;
    // public interface menu_child
    // {
    //     // Property signatures:
    //     Transform trnsform { get; set; }
    //     Image img { get; set; }
    //     TextMeshProUGUI TXT { set; get; }
    //     void F();  
    // }
    // public class m_Child : menu_child 
    // {  
    //     public Transform trnsform;
    //     public void F() {}  
    //     public static void Main() {}  
    // }


    [Header ("Money")]
    [SerializeField] private GameObject money_ui;
    private TextMeshProUGUI[] money_bfr = new TextMeshProUGUI[20]; // 20 sized gm bfr
    private bool m_textGlow_sns = false, is_glowing = false;
    private TextMeshProUGUI money_txt;
    private int player_money = 0;
    private int money_i = 0, money_v = 0;
    private const float money_delay = 0.0075f;
    private float money_timer = 0f;


    [Header ("Speed")]
    [SerializeField] private TextMeshProUGUI speed_txt;  


    [Header ("Timer")]
    [SerializeField] private TextMeshProUGUI timer_txt;  
    private float timer_ = 0f;

    [Header ("Score")]
    [SerializeField] private TextMeshProUGUI score_txt;
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
    private Transform[] parents_3d;

    [Header ("Scripts")]
    private Weapon weapon_scrpt;
    
    [Header ("Player")]
    private Transform plyr_transform;
    private Rigidbody plyr_rgBody;
    private PlayerMovement p_movement;
    private float p_speedValue = 0f;


    [Header ("TopRight Gun")]
    [SerializeField] private GameObject gun_ui;
    [SerializeField] private TextMeshPro ammo_txt;
    [SerializeField] private GameObject gun_prefab;
    private Vector3 saved_prefabPos_, saved_prefabScale_;
    private Quaternion saved_prefabRotation_;

    [Header ("TopRight GunMaterials")]
    [SerializeField] private Material[] Gun_Materials;
    private GameObject[] gun_fetchedPrefabs;


    [Header ("Enemy Ui")]
    private bool can_aim = false;
    [SerializeField] private GameObject enemy_information;
    private Transform aimed_enemy;
    private AutoTurret aimed_turretScrpt;
    private int enemy_health, enemy_shield, enemy_armor;

  
    

    [Header ("Combo")]
    [SerializeField] private Sprite[] combo_masks;
    [SerializeField] private GameObject combo_obj;
    private string combo_text = "Good";
    private int combo_v = 0, combo_goal = 3;
    private LTDescr combo_lt;



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



        // get 3d parents
        List<Transform> ts = new List<Transform>(10); // 10-size buffer
        int c = 0;
        for(int i = 0; i < 2; i++) // TOP-lft & TOP-rght
        {
            for(int j = 0 ; j < transform.GetChild(i).transform.childCount; j++)
            {
                Transform f =  transform.GetChild(i).transform.GetChild(j);
                // var temp_ts = ts.ToList(); temp_ts.Add(f);
                // ts = temp_ts.ToArray();
                ts.Add(f);
                c++;
             }
        }
        // ts[c + 1] = enemy_information.transform;
        parents_3d = ts.ToArray();
        


        // money txt
        money_txt = money_ui.transform.GetChild(1).GetComponent<TextMeshProUGUI>();


        // restart ui registration
        for(int s = 0; s < gameObject.transform.childCount; s ++){
            if(s > 2) break;
            saved_ui_initalPos[s] = gameObject.transform.GetChild(s).localPosition;
        }
        saved_restartUi_transforms = transform.GetChild(5).GetComponentsInChildren<Transform>();
        saved_restartUi_scales =  Array.ConvertAll(saved_restartUi_transforms, t => t.localScale);




        // weapon ammo txt
        weapon_ammoTxts = new TextMeshPro[2]{
            weapon_ui_obj.transform.GetChild(1).GetChild(2).GetComponent<TextMeshPro>(), // stack
            weapon_ui_obj.transform.GetChild(1).GetChild(1).GetComponent<TextMeshPro>(), // magz
        };



        // restart childs
        for(int b = 0; b < transform.GetChild(5).childCount; b ++)
        {
            // TextMeshProUGUI a = transform.GetChild(5).GetChild(b).GetComponent<TextMeshProUGUI>();
            // var b_ = new Dictionary<TextMeshProUGUI?, Image?> {
            //     { 
            //         transform.GetChild(5).GetChild(b).GetComponent<TextMeshProUGUI>(),
            //         transform.GetChild(5).GetChild(b).GetComponent<Image>()
            //     }
            // };
            // saved_menuChilds[b] = b_;
        }

        // weapon reload
        weapon_reload = weapon_ui_obj.transform.parent.GetChild(1).gameObject;
        weapon_reload.SetActive(false);


        newEnemy_UI(true);
        Invoke("countScore", 0.25f);
        Invoke("canAim", 1.5f);
    }
    private void countScore(){ countScore_ = true; }
    private void canAim(){ can_aim = true; }



    // Update
    private void Update()
    {
        timer_ += Time.deltaTime;


        // values attribution
        timer_txt.text = timer_.ToString();
        score_txt.text = score.ToString();
        speed_txt.text = ((uint) plyr_rgBody.velocity.z).ToString();
        money_txt.text = player_money.ToString();
        weapon_ammoTxts[1].text = weapon_scrpt.get_ammo.ToString() + " ";
        weapon_ammoTxts[0].text = "| " + weapon_scrpt.get_ammoInMag.ToString();
        //weapon_reload.transform.GetChild()



        // money
        if(player_money < money_v){
            if(money_timer >= money_delay){
                money_timer = 0f;
                player_money++;
            }
        }
        money_timer += Time.deltaTime;




        // restart fadeIn
        int x = 0, c = 0;
        if(restart_uiNeeds_fade){
            for(int i = 0; i < saved_restartUi_transforms.Length; i ++ ){
                if(saved_restartUi_transforms[i] == null) break;

                Image img_ = saved_restartUi_transforms[i].GetComponent<Image>();
                // Image img_ = saved_menuChilds[i].Keys.GetEnumerator[0];
                if(img_ != null){
                    var tempColor = img_.color;
                    if(tempColor.a < 1f){
                        tempColor.a += (0.7f) * Time.deltaTime;
                        img_.color = tempColor;
                    }else{ c++; }
                }else{
                    TextMeshProUGUI txt_ = saved_restartUi_transforms[i].GetComponent<TextMeshProUGUI>();
                    // TextMeshProUGUI txt_ = saved_menuChilds[i].Values.GetEnumerator(0);
                    if(txt_ == null) continue;

                    var t_Color = txt_.color;
                    if(t_Color.a < 1f){
                        t_Color.a += (0.7f) * Time.deltaTime;
                        txt_.color = t_Color;
                    }else{ c++; }
                }
                x++;
            }
            // fade end
            if(c == x) {
                restart_uiNeeds_fade = false;
                
                for(int n = 0; n < x; n ++){
                   LeanTween.scale(saved_restartUi_transforms[n].gameObject, saved_restartUi_scales[n], 0.2f).setEaseInSine();
                }
            }
        }



        // gain money fadeOut
        for(int m = 0; m < money_i; m ++)
        {
            // var clr = (money_bfr[m].color);
            // if(clr.a > 0f)
            // {
            //     clr.a -=  Time.deltaTime;
            // }
        }



        // reload timer float
        if(reload_timerFloat > 0){
            reload_timerFloat -= Time.deltaTime;
        }


    }   



    // FixedUpdate 
    private void FixedUpdate()
    {

        // ammo
        if( ammo_txt.text.ToString() != weapon_scrpt.get_ammo.ToString())
        {
            ammo_txt.text = weapon_scrpt.get_ammo.ToString();
        }


        // ui movement
        if(plyr_rgBody != null && plyr_transform != null)
        {
            for(int i = 0; i < parents_3d.Length; i++)
            {
                if(parents_3d[i] == null
                    || LeanTween.isTweening ( parents_3d[i].gameObject)
                ) continue;

                parents_3d[i].transform.rotation = Quaternion.Slerp( parents_3d[i].transform.rotation, Quaternion.Euler(
                    plyr_rgBody.velocity.y * 0.10f,
                    (plyr_transform.rotation.eulerAngles.y > 180f ? 
                        (-1 * (plyr_transform.rotation.eulerAngles.y - 360f) ):
                        -1 * plyr_transform.rotation.eulerAngles.y
                    ) * 0.06f,
                    0f
                ), 0.2f);

                parents_3d[i].transform.localPosition = Vector3.Lerp( parents_3d[i].transform.localPosition, new Vector3(
                    parents_3d[i].transform.localPosition.x + (plyr_rgBody.velocity.x* 0.1f),
                    parents_3d[i].transform.localPosition.y, // + (plyr_rgBody.velocity.y* 0.06f),
                    0
                ), 0.2f);
            }  
        }



     
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



        // aimed enemy
        if(aimed_turretScrpt != null)
        {
            if(enemy_health != aimed_turretScrpt.get_health)
            {
                if(aimed_turretScrpt.get_health == 0) // 0 health => turn off ui
                {
                    newEnemy_UI(true);
                }
                else // health damage effect
                {
                    float dmg_done = enemy_health - aimed_turretScrpt.get_health;
                    enemy_health = aimed_turretScrpt.get_health;
                    float x_health = (((float)enemy_health / (float)aimed_turretScrpt.turret_maxHealth) * 100f);
                    LeanTween.scale(enemy_information.transform.GetChild(0).GetChild(1).gameObject, new Vector3(x_health / 100, 1, 1), 0.25f).setEaseInOutCubic();
                    LeanTween.scale(enemy_information.transform.GetChild(0).GetChild(2).gameObject, 
                        new Vector3(x_health / 100, 1, 1), 
                        0.2f + (dmg_done / 70)
                    ).setEaseInOutCubic();
                    // enemy_information.transform.GetChild(1).GetChild(1).localScale = new Vector3(x_health / 100, 1, 1);
                }

            }
        }




        // health

        
    }




    // public Gun Lvl up method
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



    // public enemy
    public void newEnemy_UI(bool is_empty, Transform enemy_ = null)
    {
        
        if(is_empty)
        {
            // Invoke("enemyUi_off", 0.6f);
            aimed_turretScrpt = null;
            enemy_information.SetActive(false);

        }else
        {
            if(!can_aim) return;
            if(enemy_ == null)
                return;

            if(aimed_enemy != null && aimed_enemy != enemy_ )
            {
                Vector3 p = plyr_transform.position - enemy_.position;

                enemy_information.transform.localPosition = new Vector3(p.x * 50, p.y * 30, 0);
                enemy_information.transform.localScale = new Vector3(enemy_information.transform.localScale.x / 10, enemy_information.transform.localScale.y / 10, 1);

                LeanTween.moveLocal(enemy_information, new Vector3(0, -300, 0), 0.7f).setEaseInOutCubic();
                LeanTween.scale(enemy_information, new Vector3(1, 1, 1), 1f).setEaseInOutCubic();
            }else
            {
                enemy_information.transform.localPosition = new Vector3(0, -300, 0);
            }

            aimed_enemy = enemy_;

            enemy_information.SetActive(true);
            AutoTurret at_turret = (enemy_ as Transform).GetComponent<AutoTurret>();
            aimed_turretScrpt = at_turret;
            enemy_information.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text = at_turret.turret_name;
            enemy_information.transform.GetChild(4).GetComponent<TMPro.TextMeshProUGUI>().text = at_turret.turret_level.ToString();

            enemy_health = at_turret.get_health;
            float x_health = (((float)enemy_health / (float)aimed_turretScrpt.turret_maxHealth) * 100f);
            enemy_information.transform.GetChild(0).GetChild(1).localScale = new Vector3( (float.IsNaN(x_health) ? 0 : x_health) / 100, 1, 1);
            enemy_information.transform.GetChild(0).GetChild(2).localScale = new Vector3((float.IsNaN(x_health) ? 0 : x_health) / 100, 1, 1);

        }
    }
    private void enemyUi_off()
    {
        if(aimed_enemy != null) enemy_information.SetActive(false);
    }




    // combo method
    public void combo_hit(bool combo_end)
    {
        if(combo_end){
            
        }else{
            combo_v ++;

            if(combo_v >= 1){
                combo_obj.SetActive(true);
                
                GameObject combo_bar = combo_obj.transform.GetChild(0).GetChild(0).gameObject;

                // yellow
                LeanTween.scale(combo_bar.transform.GetChild(1).gameObject, new Vector3( ((float)combo_v / (float)combo_goal), 1, 1), 0.75f).setEaseInSine();
                // white
                LeanTween.scale(combo_bar.transform.GetChild(0).gameObject, new Vector3( ((float)combo_v / (float)combo_goal), 1, 1), 0.3f).setEaseInSine();

                GameObject tris = combo_obj.transform.GetChild(2).gameObject;

                // x2
                combo_obj.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "X" + combo_v.ToString();

                for(int s = 0; s < 2; s ++){
                    // l tris
                    LeanTween.moveLocal(tris.transform.GetChild(0).GetChild(s).gameObject, 
                        tris.transform.GetChild(1).GetChild(s).localPosition + new Vector3(-20f, 0f, 0f), 1.4f).setEasePunch();
                    // r tris
                    LeanTween.moveLocal(tris.transform.GetChild(1).GetChild(s).gameObject, 
                        tris.transform.GetChild(1).GetChild(s).localPosition + new Vector3(20f, 0f, 0f), 1.4f).setEasePunch();

                    LeanTween.scale(tris.transform.GetChild(1).GetChild(s).gameObject, tris.transform.GetChild(1).GetChild(s).localScale * 2f, 1.4f).setEasePunch();
                    LeanTween.scale(tris.transform.GetChild(0).GetChild(s).gameObject, tris.transform.GetChild(1).GetChild(s).localScale * 2f, 1.4f).setEasePunch();
                }

                if(combo_v == combo_goal){ // PERFECT !
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
    private IEnumerator combo_next(){
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



    // public gain money
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
            m.transform.localPosition = money_ui.transform.GetChild(1).localPosition + new Vector3(0f, (-45f) * money_i, 0f);
            m.transform.localScale = m.transform.localScale * 0.9f;

            LeanTween.moveLocal(m, money_ui.transform.GetChild(1).localPosition + new Vector3(0f, 20f, 0f), 1.4f).setEaseInSine();
            LeanTween.scale(m, m.transform.localScale * 1.6f, 1.7f).setEasePunch();
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
        //fadeout
        LeanTween.alpha(m_obj.transform.GetComponent<RectTransform>(), 0f, 1.2f).setEaseInSine();

        yield return new WaitForSeconds(1f);
        Destroy(m_obj);
        money_i--;
    }
    private IEnumerator text_glow_eff(){
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

    

    // public kill method
    public void kill_ui()
    {

    }




    // public gameOv method
    public void gameOver_ui()
    {

        for(int z = 0; z < 9; z ++){
            if(z < 3){
                LeanTween.moveLocal(
                    gameObject.transform.GetChild(z).gameObject, 
                    new Vector3(
                        gameObject.transform.GetChild(z).localPosition.x +  (z != 2 ? (z == 1 ? -350f : 350f) : 0),
                        gameObject.transform.GetChild(z).localPosition.y + (z == 2 ? 500 : 0),
                        0
                ), 0.6f).setEaseInSine();
            }else{
                if(z == 5){
                    GameObject gm =  gameObject.transform.GetChild(z).gameObject;
                    gm.transform.localPosition = Vector3.zero;

                    Transform[] gm_chlds = gm.GetComponentsInChildren<Transform>();

                    for(int y = 0; y <  gm_chlds.Length; y ++){
                        gm_chlds[y].localPosition =   gm_chlds[y].localPosition + new Vector3(0f, -50f, 0f);
                        Vector3 scl = gm_chlds[y].localScale;
                        gm_chlds[y].localScale =  scl * 0.87f;

                        LeanTween.moveLocal(gm_chlds[y].gameObject, gm_chlds[y].localPosition + new Vector3(0, 50f, 0), 1.9f ).setEaseInSine();
                        LeanTween.scale(gm_chlds[y].gameObject, scl * 0.9f, 1.3f).setEasePunch();

                        Image ui_img = gm_chlds[y].GetComponent<Image>();
                        if(ui_img != null){
                            var tempColor = ui_img.color;
                            tempColor.a = 0f;
                            ui_img.color = tempColor;
                        }else{
                            TextMeshProUGUI ui_txt =  gm_chlds[y].GetComponent<TextMeshProUGUI>();
                            if(ui_txt == null) continue;
                            var tempColor = ui_txt.color;
                            tempColor.a = 0f;
                            ui_txt.color = tempColor;
                        }


                    }
                }else{
                    // RectTransform rect_t = gameObject.transform.GetChild(z).GetComponent<RectTransform>();
                    // if(rect_t != null){
                    //     Debug.Log(rect_t.anchoredPosition);

                    // }
                }
            }

            if(z > 7){
                LeanTween.moveLocal(
                    gameObject.transform.GetChild(z).gameObject,
                    new Vector3( gameObject.transform.GetChild(z).localPosition.x, gameObject.transform.GetChild(z).localPosition.y - 500, 0),
                0.6f).setEaseInSine();
            }
        }
 
        restart_uiNeeds_fade = true;

    }

    public void ui_weaponShoot(){

    }

    public void ui_reload(float reload_time){
        weapon_reload.SetActive(true);
        Invoke("off_wReload", reload_time);

    }
    private void off_wReload(){weapon_reload.SetActive(false);}


}
