using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Menu_StageSelect : Menu
{

    private Vector2 inputVec;
    public float inputCooldown;
    private float lastAngle;

    public Camera cmr;
    private Vector2 cmrPos;
    private bool cameraInRoom;
    private Vector2 prevScreenXY;
    private bool cmrSnapToRoom;

    private GUISkin font;

    public enum Rooms { MainStage, FortressStage, Shop, Data }
    public Rooms activeRoom;
    public bool stageSelected;
    public float stageCooldown;

    [Header("Main Stage Select Elements")]

    public Vector2 stageSelectorOrigin;
    public Vector2Int stageIndex;

    public AudioClip stageAudioMove;
    public AudioClip stageAudioConfirm;
    public AudioClip stageAudioCancel;

    public SpriteRenderer selectMainSprite;
    public Sprite selectFlashStage1;
    public Sprite selectFlashStage2;
    public Sprite selectFlashStage3;
    public Sprite selectFlashStage4;

    public SpriteRenderer pharaohIcon;
    public SpriteRenderer geminiIcon;
    public SpriteRenderer metalIcon;
    public SpriteRenderer starIcon;
    public SpriteRenderer bombIcon;
    public SpriteRenderer windIcon;
    public SpriteRenderer galaxyIcon;
    public SpriteRenderer commandoIcon;

    [Header("Fortress Stage Select Elements")]

    public AudioClip fortressAudioMove;
    public AudioClip fortressAudioConfirm;
    public AudioClip fortressPathSound;
    public AudioClip fortressTrack;

    public Vector2 fortressSelectorOrigin;
    public Vector2Int fortressIndex;
    private bool fortressPlayingAnimation;

    public Sprite[] fortressStageIcons;

    public Sprite selectFlashFort1;
    public Sprite selectFlashFort2;
    public Sprite selectFlashFort3;
    public Sprite selectFlashFort4;
    public SpriteRenderer fortIconReference;
    public SpriteRenderer[] fortIcons;

    public MeshFilter fortPathFilter;

    [System.Serializable]
    public class FortressPathPoints
    {
        public Vector3[] points;
        public Vector3 this[int i]
        {
            get
            {
                return points[i];
            }
        }
        public int Count
        {
            get { return points.Length; }
        }
    }
    public FortressPathPoints[] fortPathPoints;

    [Header("Shop Elements")]

    public AudioClip shopAudioMove;

    public Vector2 shopSelectorOrigin;
    public Vector3Int shopIndex;

    public SpriteRenderer shopSelect;
    public Sprite selectFlashShop1;
    public Sprite selectFlashShop2;

    public Item.Items[] itemCatalog;
    public SpriteRenderer[] itemSlots;

    public SpriteRenderer itemDisplay;

    public float purchaseCooldown;

    [Header("Data Elements")]

    public AudioClip dataAudioMove;
    public AudioClip dataAudioChange;

    public Vector2 dataSelectorOrigin;
    public Vector2Int dataIndex;

    public SpriteRenderer SLButton;
    public Sprite SaveSprite;
    public Sprite LoadSprite;

    public string dataDesc;


    public override void Start()
    {
        Time.timeScale = 1.0f;
        stageIndex = GameManager.lastStageSelected;
        inputVec = Vector2.zero;
        inputCooldown = 0.0f;
        lastAngle = -1000;

        string s = GameManager.LoadData(dataIndex.y, false);
        SetDataDescription(s);

        ChangeRoom(Rooms.MainStage);
        stageSelected = false;
        stageCooldown = 1.0f;

        RefreshFortressStages();

        itemCatalog = new Item.Items[] { Item.Items.ETank, Item.Items.WTank, Item.Items.MTank, Item.Items.boltBig };

        Helper.SetAspectRatio(cmr);
        prevScreenXY = new Vector2(Screen.width, Screen.height);

        font = (GUISkin)Resources.Load("GUI/8BitFont", typeof(GUISkin));

        GameManager.checkpointActive = false;
        GameManager.stageItems.Clear();
    }
    public override void Update()
    {
        if (!stageSelected)
        {
            inputVec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (inputVec.sqrMagnitude > 1f)
                inputVec.Normalize();

            if (cmrSnapToRoom)
                cmr.transform.position = new Vector3(cmrPos.x, cmrPos.y, cmr.transform.position.z);
            else
                cmr.transform.position = new Vector3(Mathf.MoveTowards(cmr.transform.position.x, cmrPos.x, Time.deltaTime * 320f),
                                                     Mathf.MoveTowards(cmr.transform.position.y, cmrPos.y, Time.deltaTime * 256f),
                                                     cmr.transform.position.z);
            if (Vector2.Distance(cmr.transform.position, cmrPos) < 1f)
                cameraInRoom = true;
            else
                cameraInRoom = false;

            if (inputVec.magnitude < 0.5f)
            {
                inputCooldown = 0.0f;
            }
            else
            {
                float moveAngle = Mathf.Round(Vector2.SignedAngle(Vector2.right, inputVec) / 90) * 90;
                if (moveAngle != lastAngle)
                {
                    lastAngle = moveAngle;
                    inputCooldown = 0.0f;
                }

                if (inputCooldown <= 0.0f && cameraInRoom)
                {
                    inputCooldown = 0.2f;

                    switch (activeRoom)
                    {
                        case Rooms.MainStage:
                            InputMainStageSelect();
                            break;
                        case Rooms.FortressStage:
                            InputFortressStageSelect();
                            break;
                        case Rooms.Shop:
                            InputShopStageSelect();
                            break;
                        case Rooms.Data:
                            InputDataStageSelect();
                            break;
                    }
                }
                else
                {
                    inputCooldown -= Time.deltaTime;
                }
            }
        }
        else
        {
            stageCooldown -= Time.deltaTime;
            if (stageCooldown <= 0)
            {
                switch (activeRoom)
                {
                    case Rooms.MainStage:
                        if (!GoToSelectedScene())
                        {
                            stageCooldown = 0.5f;
                            stageSelected = false;
                        }
                        break;
                    case Rooms.FortressStage:
                        if (!GoToSelectedScene())
                        {
                            stageCooldown = 0.5f;
                            stageSelected = false;
                        }
                        break;
                }
            }
        }

        if (Screen.width != prevScreenXY.x || Screen.height != prevScreenXY.y)
        {
            Helper.SetAspectRatio(cmr);
            prevScreenXY = new Vector2(Screen.width, Screen.height);
        }

        switch (activeRoom)
        {
            case Rooms.MainStage:
                UpdateMainStageSelect();
                break;
            case Rooms.FortressStage:
                UpdateFortressStageSelect();
                break;
            case Rooms.Shop:
                UpdateShopStageSelect();
                break;
            case Rooms.Data:
                UpdateDataStageSelect();
                break;
        }
    }
    public override void DrawGUI()
    {
        switch (activeRoom)
        {
            case Rooms.Shop:
                GUIShopStageSelect();
                break;
            case Rooms.Data:
                GUIDataStageSelect();
                break;
        }
    }
    public void OnDrawGizmos()
    {
        FortressGizmos();
    }
    public void ChangeRoom(Rooms newRoom)
    {
        cameraInRoom = false;

        activeRoom = newRoom;
        switch (newRoom)
        {
            case Rooms.MainStage:
                cmrPos = new Vector2(0, 8);

                if (GameManager.bossDead_PharaohMan)
                    pharaohIcon.enabled = false;
                else
                    pharaohIcon.enabled = true;
                if (GameManager.bossDead_GeminiMan)
                    geminiIcon.enabled = false;
                else
                    geminiIcon.enabled = true;
                if (GameManager.bossDead_MetalMan)
                    metalIcon.enabled = false;
                else
                    metalIcon.enabled = true;
                if (GameManager.bossDead_StarMan)
                    starIcon.enabled = false;
                else
                    starIcon.enabled = true;
                if (GameManager.bossDead_BombMan)
                    bombIcon.enabled = false;
                else
                    bombIcon.enabled = true;
                if (GameManager.bossDead_WindMan)
                    windIcon.enabled = false;
                else
                    windIcon.enabled = true;
                if (GameManager.bossDead_GalaxyMan)
                    galaxyIcon.enabled = false;
                else
                    galaxyIcon.enabled = true;
                if (GameManager.bossDead_CommandoMan)
                    commandoIcon.enabled = false;
                else
                    commandoIcon.enabled = true;
                break;
            case Rooms.FortressStage:
                cmrPos = new Vector2(0, 248);
                break;
            case Rooms.Shop:
                cmrPos = new Vector2(-272, 8);

                for (int x = 0; x < 6; x++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        if (z * 6 + x >= itemCatalog.Length)
                            itemSlots[z * 6 + x].sprite = null;
                        else
                        {
                            GameObject item = Item.GetObjectFromItem(itemCatalog[(shopIndex.z + z) * 6 + x]);
                            if (item != null)
                                itemSlots[z * 6 + x].sprite = item.GetComponentInChildren<SpriteRenderer>().sprite;
                            else
                                itemSlots[z * 6 + x].sprite = null;
                        }

                    }
                }
                break;
            case Rooms.Data:
                cmrPos = new Vector2(272, 8);
                break;
        }
    }
    public bool GoToSelectedScene(bool checkOnly = false, bool snapToRoom = false)
    {
        cmrSnapToRoom = snapToRoom;
        if (activeRoom == Rooms.MainStage)
        {
            if (stageIndex.x == 0 && stageIndex.y == 0)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_PharaohMan");
                return true;
            }
            if (stageIndex.x == 2 && stageIndex.y == 0)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_GeminiMan");
                return true;
            }
            if (stageIndex.x == 2 && stageIndex.y == 1)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_MetalMan");
                return true;
            }
            if (stageIndex.x == 0 && stageIndex.y == 2)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_StarMan");
                return true;
            }
            if (stageIndex.x == 1 && stageIndex.y == 0)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_BombMan");
                return true;
            }
            if (stageIndex.x == 0 && stageIndex.y == 1)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_WindMan");
                return true;
            }
            if (stageIndex.x == 1 && stageIndex.y == 2)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_GalaxyMan");
                return true;
            }
            if (stageIndex.x == 2 && stageIndex.y == 2)
            {
                if (!checkOnly)
                    Helper.GoToStage("Stage_CommandoMan");
                return true;
            }
        }
        else if (activeRoom == Rooms.FortressStage)
        {
            if (fortressIndex.x == 0)
            {
                if (!checkOnly)
                    Helper.GoToStage("Fortress_Devil");
                return true;
            }
            if (fortressIndex.x == 1)
            {
                if (!checkOnly)
                    Helper.GoToStage("Fortress_MegaBot");
                return true;
            }
            if (fortressIndex.x == 2)
            {
                if (!checkOnly)
                    Helper.GoToStage("Fortress_Roll");
                return true;
            }
            if (fortressIndex.x == 3)
            {
                if (!checkOnly)
                    Helper.GoToStage("Fortress_Roll3D");
                return true;
            }
        }

        return false;
    }
    public IEnumerator PlayFortressAnimation(int stage)
    {
        while (!cameraInRoom)
            yield return null;

        fortressPlayingAnimation = true;
        foreach (SpriteRenderer rend in fortIcons)
            rend.gameObject.SetActive(false);
        fortIconReference.enabled = false;

        yield return new WaitForSeconds(8f);

        //  Preparing the mesh.
        Mesh mesh = new Mesh();

        List<Vector3> v = new List<Vector3>();
        List<int> t = new List<int>();
        List<Vector2> u = new List<Vector2>();



        // Deciding the place of each path segment.

        if (stage == 0)
            yield break;
        else if (stage >= fortPathPoints.Length)
            stage = fortPathPoints.Length;


        for (int j = 0; j < stage; j++)
        {
            for (int i = 0; i < fortPathPoints[j].Count - 1; i++)
            {
                Vector2 direction = (fortPathPoints[j][i + 1] - fortPathPoints[j][i]);
                int steps    = (int)(direction.magnitude / 8);
                float distance = direction.magnitude / (steps + 0.75f);
                direction.Normalize();

                for (int s =  0; s <= steps; s++)
                {
                    int vc = v.Count;
                    v.Add((Vector2)fortPathPoints[j][i] + Vector2.Perpendicular(direction) * 4f + direction * distance * (s + 0));
                    v.Add((Vector2)fortPathPoints[j][i] - Vector2.Perpendicular(direction) * 4f + direction * distance * (s + 0));
                    v.Add((Vector2)fortPathPoints[j][i] - Vector2.Perpendicular(direction) * 4f + direction * distance * (s + 1));
                    v.Add((Vector2)fortPathPoints[j][i] + Vector2.Perpendicular(direction) * 4f + direction * distance * (s + 1));

                    t.Add(vc + 0);
                    t.Add(vc + 3);
                    t.Add(vc + 2);
                    t.Add(vc + 0);
                    t.Add(vc + 2);
                    t.Add(vc + 1);

                    u.Add(new Vector2(0, 1));
                    u.Add(new Vector2(0, 0));
                    u.Add(new Vector2(1, 0));
                    u.Add(new Vector2(1, 1));

                    if (j == stage - 1)
                    {
                        mesh = new Mesh();
                        mesh.vertices = v.ToArray();
                        mesh.triangles = t.ToArray();
                        mesh.uv = u.ToArray();

                        fortPathFilter.mesh = mesh;

                        cmr.GetComponent<AudioSource>().PlaySound(fortressPathSound, true);
                        yield return new WaitForSeconds(0.075f);
                    }
                    
                }

            }
        }

        foreach (SpriteRenderer rend in fortIcons)
            rend.gameObject.SetActive(true);
        fortIconReference.enabled = true;

        fortressPlayingAnimation = false;
        GameManager.playFortressStageUnlockAnimation = false;

    }
   


    private void InputMainStageSelect()
    {
        // Normally the moveAngle should be one of [-180, -90, 0, 90, 180].
        if (lastAngle == 0)
            stageIndex.x += 1;
        else if (lastAngle == 180 || lastAngle == -180)
            stageIndex.x -= 1;
        else if (lastAngle == 90)
            stageIndex.y -= 1;
        else
            stageIndex.y += 1;

        if (stageIndex.y < 0)
            ChangeRoom(Rooms.FortressStage);
        else if (stageIndex.x < 0)
            ChangeRoom(Rooms.Shop);
        else if (stageIndex.x > 2)
            ChangeRoom(Rooms.Data);
        else if (stageIndex.y <= 2)
            Helper.PlaySound(stageAudioMove);

        stageIndex.x = Mathf.Clamp(stageIndex.x, 0, 2);
        stageIndex.y = Mathf.Clamp(stageIndex.y, 0, 2);
    }
    private void UpdateMainStageSelect()
    {
        selectMainSprite.transform.position = new Vector3(stageSelectorOrigin.x + stageIndex.x * 64,
                                              stageSelectorOrigin.y - stageIndex.y * 64,
                                              selectMainSprite.transform.position.z);
        if (Input.GetButtonDown("Start"))
        {
            stageSelected = true;
            if (GoToSelectedScene(true))
            {
                Helper.PlaySound(stageAudioConfirm);
                cmr.GetComponent<AudioSource>().Stop();
            }
            else
                Helper.PlaySound(stageAudioCancel);
        }

        if (stageSelected)
            selectMainSprite.sprite = Time.time % 0.1f < 0.05f ? selectFlashStage3 : selectFlashStage4;
        else
            selectMainSprite.sprite = Time.time % 0.5f < 0.25f ? selectFlashStage1 : selectFlashStage2;
    }

    private void InputFortressStageSelect()
    {
        if (fortressPlayingAnimation)
            return;

        // Normally the moveAngle should be one of [-180, -90, 0, 90, 180].
        if (lastAngle == 0)
            fortressIndex.x += 1;
        else if (lastAngle == 180 || lastAngle == -180)
            fortressIndex.x -= 1;
        else if (lastAngle == 90)
            fortressIndex.y -= 1;
        else
            fortressIndex.y += 1;

        if (fortressIndex.y > 0)
            ChangeRoom(Rooms.MainStage);
        else if (fortressIndex.y == 0 && fortressIndex.x >= 0 && fortressIndex.x <= GameManager.maxFortressStage - 1)
            Helper.PlaySound(fortressAudioMove);

        fortressIndex.x = Mathf.Clamp(fortressIndex.x, 0, GameManager.maxFortressStage - 1);
        fortressIndex.y = 0;
    }
    private void UpdateFortressStageSelect()
    {
        if (Input.GetButtonDown("Start"))
        {
            stageSelected = true;
            if (GoToSelectedScene(true))
            {
                Helper.PlaySound(stageAudioConfirm);
                cmr.GetComponent<AudioSource>().Stop();
            }
            else
                Helper.PlaySound(stageAudioCancel);
        }

        for (int i = 0; i < fortIcons.Length; i++)
        {
            if (fortressIndex.x == i)
            {
                if (stageSelected)
                    fortIcons[i].sprite = Time.time % 0.1f < 0.05f ? selectFlashFort3 : selectFlashFort4;
                else
                    fortIcons[i].sprite = Time.time % 0.5f < 0.25f ? selectFlashFort1 : selectFlashFort2;
            }
            else
                fortIcons[i].sprite = selectFlashFort1;
        }
    }
    public void RefreshFortressStages()
    {
        for (int i = 1; i < fortIcons.Length; i++)
        {
            if (fortIcons[i] != null)
                Object.Destroy(fortIcons[i].gameObject);
        }
        if (GameManager.maxFortressStage > 0)
        {
            fortIcons = new SpriteRenderer[GameManager.maxFortressStage];
            fortIcons[0] = fortIconReference;
            for (int i = 0; i < fortIcons.Length; i++)
            {
                if (i > 0)
                    fortIcons[i] = Object.Instantiate(fortIcons[0]);
                fortIcons[i].transform.position = new Vector3(272f * (1 + i) / (1 + GameManager.maxFortressStage) - 136f,
                                                              fortIcons[0].transform.position.y,
                                                              fortIcons[0].transform.position.z);
                fortIcons[i].transform.parent = fortIcons[0].transform.parent;

                SpriteRenderer icon = fortIcons[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
                if (fortressStageIcons.Length > i)
                    icon.sprite = fortressStageIcons[i];
            }
        }

        if (GameManager.playFortressStageUnlockAnimation)
        {
            ChangeRoom(Rooms.FortressStage);
            cmrSnapToRoom = true;
            Object.FindObjectOfType<Menu_Controller>().StartCoroutine(PlayFortressAnimation(GameManager.maxFortressStage));
            cmr.GetComponent<AudioSource>().PlaySound(fortressTrack, true);
            cmr.GetComponent<AudioSource>().loop = false;
            Object.Destroy(cmr.GetComponent<Misc_PlayRandomTrack>());
        }
    }
    public void FortressGizmos()
    {
        int stage = 6;

        if (stage == 0)
            return;
        else if (stage >= fortPathPoints.Length)
            stage = fortPathPoints.Length;


        for (int j = 0; j < stage; j++)
        {
            for (int i = 0; i < fortPathPoints[j].Count - 1; i++)
            {
                Vector2 direction = (fortPathPoints[j][i + 1] - fortPathPoints[j][i]);
                int steps = (int)(direction.magnitude / 8);
                float distance = direction.magnitude / steps;
                direction.Normalize();

                for (int s = 0; s <= steps; s++)
                {
                    Gizmos.color = Color.cyan * ((float)s / steps) + Color.blue * (1f - (float)s / steps);
                    Gizmos.DrawSphere((Vector2)fortPathPoints[j][i] + direction * distance * s, 4);
                }

            }
        }
    }

    private void InputShopStageSelect()
    {
        // Normally the moveAngle should be one of [-180, -90, 0, 90, 180].
        if (lastAngle == 0)
            shopIndex.x += 1;
        else if (lastAngle == 180 || lastAngle == -180)
            shopIndex.x -= 1;
        else if (lastAngle == 90)
            shopIndex.y -= 1;
        else
            shopIndex.y += 1;

        if (shopIndex.x > 5)
            ChangeRoom(Rooms.MainStage);
        else
            Helper.PlaySound(shopAudioMove);

        if (shopIndex.y < shopIndex.z)
            shopIndex.z = shopIndex.y;
        else if (shopIndex.y > shopIndex.z + 1)
            shopIndex.z = shopIndex.y - 1;

        int maxY = Mathf.CeilToInt(itemCatalog.Length / 6);
        shopIndex.x = Mathf.Clamp(shopIndex.x, 0, 5);
        shopIndex.y = Mathf.Clamp(shopIndex.y, 0, maxY - 1);
        shopIndex.z = Mathf.Clamp(shopIndex.z, 0, maxY - 2);
        if (shopIndex.y < 0)
            shopIndex.y = 0;
        if (shopIndex.z < 0)
            shopIndex.z = 0;
        GameObject item;

        for (int x = 0; x < 6; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if ((shopIndex.z + z) * 6 + x >= itemCatalog.Length)
                    itemSlots[z * 6 + x].sprite = null;
                else
                {
                    item = Item.GetObjectFromItem(itemCatalog[(shopIndex.z + z) * 6 + x]);
                    if (item != null)
                        itemSlots[z * 6 + x].sprite = item.GetComponentInChildren<SpriteRenderer>().sprite;
                    else
                        itemSlots[z * 6 + x].sprite = null;
                }

            }    
        }

        item = Item.GetObjectFromItem(itemCatalog[(shopIndex.y) * 6 + shopIndex.x]);
        if (item != null)
            itemDisplay.sprite = item.GetComponentInChildren<SpriteRenderer>().sprite;
        else
            itemDisplay.sprite = null;
    }
    private void UpdateShopStageSelect()
    {
        shopSelect.sprite = Time.time % 0.5f < 0.25f ? selectFlashShop1 : selectFlashShop2;

        shopSelect.transform.position = new Vector3(shopSelectorOrigin.x + shopIndex.x * 32,
                                                    shopSelectorOrigin.y - (shopIndex.y - shopIndex.z) * 48,
                                                    shopSelect.transform.position.z);

        if (purchaseCooldown > 0.0f)
            purchaseCooldown -= Time.deltaTime;
        else if (Input.GetButtonDown("Start"))
        {

            if ((shopIndex.y) * 6 + shopIndex.x >= Item.itemList.Length)
                return;

            Item.Items item = itemCatalog[(shopIndex.y) * 6 + shopIndex.x];
            if (GameManager.bolts >= Item.itemList[(int)item].boltCost)
            {
                Item.AddItemQuantity(item, 1);
                GameManager.bolts -= Item.itemList[(int)item].boltCost;
                Helper.PlaySound(stageAudioConfirm);
            }
            else
                Helper.PlaySound(stageAudioCancel);

            purchaseCooldown = 0.25f;
        }
    }
    private void GUIShopStageSelect()
    {
        if (!cameraInRoom)
            return;

        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);
        int blockSize = (int)(Camera.main.pixelWidth / 16);

        font.label.fontSize = (int)(blockSize * 0.5625f);

        GameObject item;

        // Draw the price of each item.
        for (int x = 0; x < 6; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if ((shopIndex.z + z) * 6 + x >= itemCatalog.Length)
                    itemSlots[z * 6 + x].sprite = null;
                else
                {
                    item = Item.GetObjectFromItem(itemCatalog[(shopIndex.z + z) * 6 + x]);
                    if (item != null)
                    {
                        GUI.Label(new Rect(cmrBase.x + blockSize * (2.425f + 2.00f * x),
                                           cmrBase.y + blockSize * (3.5f + 2.75f * z),
                                           blockSize * 2f,
                                           blockSize * 2f),
                                           Item.itemList[(int)itemCatalog[(shopIndex.z + z) * 6 + x]].boltCost.ToString("000"),
                                           font.label);
                    }
                }
            }
        }

        // Draw the quantity of the currently selected item.
        if ((shopIndex.y) * 6 + shopIndex.x >= itemCatalog.Length)
            item = null;
        else
            item = Item.GetObjectFromItem(itemCatalog[(shopIndex.y) * 6 + shopIndex.x]);

        if (item != null)
        {
            GUI.Label(new Rect(cmrBase.x + 3.5f * blockSize,
                               cmrBase.y + 12.5f * blockSize,
                               2.125f * blockSize,
                               1.0f * blockSize),
                               Item.GetItemQuantity(itemCatalog[(shopIndex.y) * 6 + shopIndex.x]).ToString("000"),
                               font.label);
        }

        // Draw the number of bolts left.

        GUI.Label(new Rect(cmrBase.x + 11.5f * blockSize,
                           cmrBase.y + 12.5f * blockSize,
                           2.125f * blockSize,
                           1.0f * blockSize),
                           GameManager.bolts.ToString("0000"),
                           font.label);

    }

    private void InputDataStageSelect()
    {
        // Normally the moveAngle should be one of [-180, -90, 0, 90, 180].
        if (lastAngle == 0)
            dataIndex.x += 1;
        else if (lastAngle == 180 || lastAngle == -180)
            dataIndex.x -= 1;
        else if (lastAngle == 90)
            dataIndex.y -= 1;
        else
            dataIndex.y += 1;

        string s = GameManager.LoadData(dataIndex.y, false);
        SetDataDescription(s);

        if (dataIndex.x < 0)
            ChangeRoom(Rooms.MainStage);
        else if (lastAngle == 0 || lastAngle == 180 || lastAngle == -180)
            Helper.PlaySound(dataAudioChange);
        else
            Helper.PlaySound(dataAudioMove);


        dataIndex.x = Mathf.Clamp(dataIndex.x, 0, 1);
        dataIndex.y = Mathf.Clamp(dataIndex.y, 0, 9);
    }
    private void UpdateDataStageSelect()
    {
        SLButton.sprite = dataIndex.x == 0 ? SaveSprite : LoadSprite;

        if (Input.GetButtonDown("Start"))
        {
            if (dataIndex.x == 0)
                GameManager.SaveData(dataIndex.y);
            else
            {
                GameManager.LoadData(dataIndex.y, true);
                RefreshFortressStages();
            }
        }
    }
    private void SetDataDescription(string s)
    {
        dataDesc = "";

        if (s == null || s.Length < 26)
            return;

        int output = 0;
        int.TryParse(s[0].ToString(), out output);
        dataDesc += "BombMan:         " + (output == 1 ? "Dead" : "Alive") + "\n";
        int.TryParse(s[1].ToString(), out output);
        dataDesc += "MetalMan:        " + (output == 1 ? "Dead" : "Alive") + "\n";
        int.TryParse(s[2].ToString(), out output);
        dataDesc += "GeminiMan:       " + (output == 1 ? "Dead" : "Alive") + "\n";
        int.TryParse(s[3].ToString(), out output);
        dataDesc += "PharaohMan:   " + (output == 1 ? "Dead" : "Alive") + "\n";
        int.TryParse(s[4].ToString(), out output);
        dataDesc += "StarMan:          " + (output == 1 ? "Dead" : "Alive") + "\n";
        int.TryParse(s[5].ToString(), out output);
        dataDesc += "WindMan:          " + (output == 1 ? "Dead" : "Alive") + "\n";
        int.TryParse(s[6].ToString(), out output);
        dataDesc += "GalaxyMan:      " + (output == 1 ? "Dead" : "Alive") + "\n";
        int.TryParse(s[7].ToString(), out output);
        dataDesc += "CommandoMan: " + (output == 1 ? "Dead" : "Alive") + "\n";

        dataDesc += "\n";
        int.TryParse(s.Substring(8, 2), out output);
        dataDesc += "Fortress Stages: " + output.ToString() + "\n";

        dataDesc += "\n";
        int.TryParse(s.Substring(10, 2), out output);
        dataDesc += "E Tanks:  " + output.ToString() + "\n";
        int.TryParse(s.Substring(12, 2), out output);
        dataDesc += "W Tanks:  " + output.ToString() + "\n";
        int.TryParse(s.Substring(14, 2), out output);
        dataDesc += "M Tanks:  " + output.ToString() + "\n";
        int.TryParse(s.Substring(16, 2), out output);
        dataDesc += "L Tanks:  " + output.ToString() + "\n";
        int.TryParse(s.Substring(18, 2), out output);
        dataDesc += "RedBulls: " + output.ToString() + "\n";
        int.TryParse(s.Substring(20, 2), out output);
        dataDesc += "Yashichi: " + output.ToString() + "\n";

        dataDesc += "\n";
        int.TryParse(s.Substring(22, 4), out output);
        dataDesc += "Bolts: " + output;

    }
    private void GUIDataStageSelect()
    {
        if (!cameraInRoom)
            return;

        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);
        float pixelSize = (Camera.main.pixelWidth / 272f);
        font.label.fontSize = (int)(pixelSize * 8);

        for (int i = 0; i < 10; i++)
        {
            string text = "Slot " + (i + 1).ToString();
            if (i == dataIndex.y)
                text = "> " + text;
            GUI.Label(new Rect(cmrBase.x + dataSelectorOrigin.x * pixelSize,
                               cmrBase.y + dataSelectorOrigin.y * pixelSize + i * 16f * pixelSize,
                               64f * pixelSize,
                               16 * pixelSize),
                      text,
                      font.label);
        }

        GUI.Label(new Rect(cmrBase.x + dataSelectorOrigin.x * pixelSize + 94f * pixelSize,
                   cmrBase.y + dataSelectorOrigin.y * pixelSize,
                   128f * pixelSize,
                   240f * pixelSize),
                   dataDesc,
                   font.label);
    }

}
