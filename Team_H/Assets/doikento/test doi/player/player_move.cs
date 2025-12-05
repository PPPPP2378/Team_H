using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EquipmentPlacementType
{
    Grassland,   // 床専用
    Wall,        // 壁専用
    Both         // どちらでも設置可能
}

[System.Serializable]
public class EquipmentData
{
    public string name;                       // 設備名
    public EquipmentLevelData[] levels;       // レベルごとのデータ
    public EquipmentPlacementType placementType = EquipmentPlacementType.Both;
    public bool isWallEquipment = false;      // 壁専用設備かどうか
}

public class player_move : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 3f;
    private Rigidbody2D rb;
    private Collider2D currentTarget;
    private Animator animator;

    [Header("設備リスト")]
    [SerializeField] private EquipmentData[] equipments;
    private int selectedEquipmentIndex = -1;

    [Header("畑のスプライト設定")]
    [SerializeField] private Sprite plowedSprite;
    [SerializeField] private Sprite wateredSprite;
    [SerializeField] private Sprite plowSprite;

    [Header("作物リスト設定")]
    [SerializeField] private CropData[] crops; // 複数の作物データ
    private int selectedCropIndex = 0; // 現在選択中の作物

    [Header("スコア・経験値設定")]
    private int currentScore = 150;
    private int currentExp = 0;
    private int expToNext = 50;
    private int playerLevel = 1;
    private float speedGrowthRate = 0.2f;
    private float holdReductionRate = 0.1f;

    [Header("操作設定")]
    [SerializeField] private float holdInterval = 0.8f;
    private float holdTimer = 0f;
    private bool isHolding = false;

    [Header("ハイライト関連")]
    [SerializeField] private Sprite highlightSprite;
    private GameObject highlightFrame;
    public int on_in_layer = 2;

    [Header("操作範囲設定")]
    [SerializeField] private float interactRadius = 2.5f;

    [Header("レベル設定")]
    [SerializeField] private int maxLevel = 15; //レベル上限

    [Header("効果音設定")]
    public AudioSource audioSource;      // SE を鳴らすための AudioSource
    public AudioClip plowSE;             // 土を耕す
    public AudioClip waterSE;            // 水やり
    public AudioClip seedSE;             // 種を植える
    public AudioClip harvestSE;          // 収穫
    public AudioClip placeEquipmentSE;   // 設備設置
    public AudioClip upgradeSE;          // 設備アップグレード
    public AudioClip errorSE;            // エラー（スコア足りない など）


    //UIManagerを参照する
    private UIManager uiManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();

        //UIマネージャーを取得
        uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManagerがシーンに見つかりません！");
        }

        // 初期UI更新
        UpdateScoreUI();
        UpdateLevelUI();

        // ハイライト初期化
        if (highlightSprite != null)
        {
            highlightFrame = new GameObject("HighlightFrame");
            var sr = highlightFrame.AddComponent<SpriteRenderer>();
            sr.sprite = highlightSprite;
            sr.sortingOrder = on_in_layer;
            highlightFrame.SetActive(false);
        }

        //UIは非表示から開始
        uiManager?.ShowProgress(false);
    }

    void FixedUpdate()
    {
        if (!WaveManager.PlayerCanControl) return;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector2 move = new Vector2(x, y).normalized;
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);

        if (animator != null)
        {
            animator.SetFloat("MoveX", move.x);
            animator.SetFloat("MoveY", move.y);
            animator.SetFloat("Speed", move.sqrMagnitude);
        }
    }

    void Update()
    {
        if (!WaveManager.PlayerCanControl) return;

        HandleHoldProgress();
        UpdateMouseTarget();

        //Eでインベントリを開く
        if(Input.GetKeyDown(KeyCode.E))
        {
            uiManager?.ToggleInventory();
        }
    }

    // --- 長押し処理 ---
    private void HandleHoldProgress()
    {
        if (Input.GetMouseButton(0))
        {
            if (!isHolding)
            {
                isHolding = true;
                holdTimer = 0f;
                uiManager?.ShowProgress(true);
            }

            holdTimer += Time.deltaTime;
            uiManager?.SetProgress(holdTimer / holdInterval);

            if (holdTimer >= holdInterval)
            {
                TryInteract();
                holdTimer = 0f;
                isHolding = false;
                uiManager?.ShowProgress(false);
            }
        }
        else if (isHolding)
        {
            isHolding = false;
            holdTimer = 0f;
            uiManager?.ShowProgress(false);
        }
    }

    // --- マウス位置更新 ---
    private void UpdateMouseTarget()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

        if (hit != null)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist <= interactRadius && IsValidTarget(hit))
            {
                currentTarget = hit;
                if (highlightFrame != null)
                {
                    highlightFrame.SetActive(true);
                    highlightFrame.transform.position = hit.transform.position;
                    SpriteRenderer targetSR = hit.GetComponent<SpriteRenderer>();
                    if (targetSR != null)
                    {
                        float sizeX = targetSR.bounds.size.x;
                        float sizeY = targetSR.bounds.size.y;
                        highlightFrame.transform.localScale = new Vector3(sizeX, sizeY, 1f);
                    }
                }
                return;
            }
        }

        currentTarget = null;
        if (highlightFrame != null)
            highlightFrame.SetActive(false);
    }

    // --- タグ判定 ---
    private bool IsValidTarget(Collider2D other)
    {
        return other.CompareTag("Plow") || other.CompareTag("Plowed") ||
               other.CompareTag("Moist_Plowe") || other.CompareTag("Seed") ||
               other.CompareTag("Seed_") || other.CompareTag("Grown") ||
               other.CompareTag("Grassland") || other.CompareTag("Wall") ||
               IsPlacedEquipment(other.tag);
    }

    // --- インタラクト ---
    private void TryInteract()
    {
        if (currentTarget == null) return;
        SpriteRenderer sr = currentTarget.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (currentTarget.CompareTag("Plow"))
        {
            sr.sprite = plowedSprite;
            currentTarget.tag = "Plowed";
            GainExp(5);
            PlaySE(plowSE);
        }
        else if (currentTarget.CompareTag("Plowed"))
        {
            sr.sprite = wateredSprite;
            currentTarget.tag = "Moist_Plowe";
            GainExp(5);
            PlaySE(waterSE);
        }
        else if (currentTarget.CompareTag("Moist_Plowe"))
        {
            if (crops.Length == 0) return;
            CropData crop = crops[selectedCropIndex];
            sr.sprite = crop.seedSprite;
            currentTarget.tag = "Seed";
            GainExp(crop.expGain);
            PlaySE(seedSE);
            StartCoroutine(GrowPlant(currentTarget, sr,crop));
        }
        else if (currentTarget.CompareTag("Grown"))
        {
            HarvestCrop(sr);
            GainExp(15);
            PlaySE(harvestSE);
        }
        else if (currentTarget.CompareTag("Grassland") || currentTarget.CompareTag("Wall")||IsPlacedEquipment(currentTarget.tag))
        {
            if (selectedEquipmentIndex == -1)
            {
                Debug.Log("設備が選択されていません");
                return;
            }

            EquipmentData selected = equipments[selectedEquipmentIndex];
            // 設置タイプ判定
            if ((selected.placementType == EquipmentPlacementType.Grassland && currentTarget.CompareTag("Wall")) ||
                (selected.placementType == EquipmentPlacementType.Wall && currentTarget.CompareTag("Grassland")))
            {
                Debug.Log("この設備はこの場所には設置できません");
                return;
            }
            PlacedEquipment placed = currentTarget.GetComponent<PlacedEquipment>();

            if (placed == null)
            {
                if (currentScore >= selected.levels[0].cost)
                {
                    sr.sprite = selected.levels[0].sprite;
                    currentTarget.tag = selected.name;

                    PlacedEquipment eq = currentTarget.gameObject.AddComponent<PlacedEquipment>();
                    eq.data = selected;
                    eq.level = 0;

                    currentScore -= selected.levels[0].cost;
                    UpdateScoreUI();
                    GainExp(20);
                    PlaySE(placeEquipmentSE);
                }
                else
                {
                    Debug.Log("スコアが足りません！");
                    PlaySE(errorSE);
                }
            }
            else
            {
                bool upgraded = placed.TryUpgrade(ref currentScore);
                if (upgraded)
                {
                    UpdateScoreUI();
                    PlaySE(upgradeSE);
                }
            }
        }
    }

    // --- 植物成長 ---
    private IEnumerator GrowPlant(Collider2D target, SpriteRenderer sr,CropData crop)
    {
        float timer = 0f;

        while (timer < crop.growTime)
        {
            if (WaveManager.CanGrow)
                timer += Time.deltaTime;
            if (target == null || !target.CompareTag("Seed"))
                yield break;
            yield return null;
        }

        sr.sprite = crop.growingSprite;
        target.tag = "Seed_";

        timer = 0f;
        while (timer < crop.fullGrowTime)
        {
            if (WaveManager.CanGrow)
                timer += Time.deltaTime;
            if (target == null || !target.CompareTag("Seed_"))
                yield break;
            yield return null;
        }

        sr.sprite = crop.grownSprite;
        target.tag = "Grown";

        GainExp(crop.expGain);
    }

    private void HarvestCrop(SpriteRenderer sr)
    {
        CropData crop = crops[selectedCropIndex];
        currentScore += crop.harvestPoints;
        UpdateScoreUI();
        sr.sprite = plowSprite;
        currentTarget.tag = "Plow";
    }

    // --- UI更新 ---
    private void UpdateScoreUI() => uiManager?.UpdateScore(currentScore);
    private void UpdateLevelUI() => uiManager?.UpdateLevel(playerLevel, currentExp, expToNext);

    // --- 経験値 ---
    private void GainExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToNext)
            LevelUp();
        UpdateLevelUI();
    }

    private void LevelUp()
    {
        if(playerLevel>=maxLevel)
        {
            playerLevel = maxLevel;
            currentExp = expToNext;
            return;
        }
        playerLevel++;
        currentExp -= expToNext;
        expToNext = Mathf.RoundToInt(expToNext * 1.2f);

        // 成長処理
        speed += speedGrowthRate;
        holdInterval = Mathf.Max(0.3f, holdInterval - holdReductionRate);

        UpdateLevelUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    public void SelectEquipment(int index)
    {
        if (index >= 0 && index < equipments.Length)
        {
            selectedEquipmentIndex = index;
        }
    }

    public void DeselectEquipment()
    {
        selectedEquipmentIndex = -1;
    }

    public void SelectCrop(int index)
    {
        if (index >= 0 && index < crops.Length)
        {
            selectedCropIndex = index;
            Debug.Log("選択された作物: " + crops[index].cropName);
        }
    }

    private bool IsPlacedEquipment(string tag)
    {
        foreach (var eq in equipments)
        {
            if (eq.name == tag) return true;
        }
        return false;
    }

    //移動のアニメーション空白の時間
    public void PauseWalk()
    {
        animator.speed = 0f;
        StartCoroutine(ResumeWalk());
    }

    private IEnumerator ResumeWalk()
    {
        yield return new WaitForSeconds(0.3f); // 秒停止
        animator.speed = 1f;
    }

    private void PlaySE(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public int PlayerLevel => playerLevel;
}