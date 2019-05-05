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

    private GUISkin font;

    public enum Rooms { MainStage, FortressStage, Shop, Data }
    public Rooms activeRoom;
    public bool stageSelected;
    public float stageCooldown;

    [Header("Main Stage Select Elements")]

    public Vector2 stageSelectorOrigin;
    public Vector2Int stageIndex;

    public SpriteRenderer selectMainSprite;
    public Sprite selectFlashStage1;
    public Sprite selectFlashStage2;
    public Sprite selectFlashStage3;
    public Sprite selectFlashStage4;

    public SpriteRenderer pharaohIcon;
    public SpriteRenderer geminiIcon;
    public SpriteRenderer metalIcon;

    [Header("Fortress Stage Select Elements")]

    public Vector2 fortressSelectorOrigin;
    public Vector2Int fortressIndex;

    public Sprite selectFlashFort1;
    public Sprite selectFlashFort2;
    public Sprite selectFlashFort3;
    public Sprite selectFlashFort4;
    public SpriteRenderer fortIconReference;
    public SpriteRenderer[] fortIcons;

    [Header("Shop Elements")]

    public Vector2 shopSelectorOrigin;
    public Vector3Int shopIndex;

    public SpriteRenderer shopSelect;
    public Sprite selectFlashShop1;
    public Sprite selectFlashShop2;

    public Item.Items[] itemCatalog;
    public SpriteRenderer[] itemSlots;

    [Header("Data Elements")]

    public Vector2 dataSelectorOrigin;
    public Vector2Int dataIndex;

    public SpriteRenderer SLButton;
    public Sprite SaveSprite;
    public Sprite LoadSprite;


    public override void Start()
    {
        Time.timeScale = 1.0f;
        stageIndex = GameManager.lastStageSelected;
        inputVec = Vector2.zero;
        inputCooldown = 0.0f;
        lastAngle = -1000;

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
            }
        }

        ChangeRoom(Rooms.MainStage);
        stageSelected = false;
        stageCooldown = 1.0f;

        Helper.SetAspectRatio(cmr);
        prevScreenXY = new Vector2(Screen.width, Screen.height);

        font = (GUISkin)Resources.Load("GUI/8BitFont", typeof(GUISkin));

        GameManager.checkpointActive = false;
    }

    public override void Update()
    {
        if (!stageSelected)
        {
            inputVec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (inputVec.sqrMagnitude > 1f)
                inputVec.Normalize();

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
                        if (stageIndex.x == 0 && stageIndex.y == 0)
                            Helper.GoToStage("Scene");
                        else if (stageIndex.x == 2 && stageIndex.y == 0)
                            Helper.GoToStage("SomeStage");
                        else
                        {
                            stageCooldown = 0.5f;
                            stageSelected = false;
                        }
                        break;
                    case Rooms.FortressStage:
                        stageCooldown = 0.5f;
                        stageSelected = false;
                        break;
                }
            }
        }
        Debug.Log("AAAAAA");

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
            case Rooms.Data:
                GUIDataStageSelect();
                break;
        }
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
                if (GameManager.bossDead_GeminiMan)
                    geminiIcon.enabled = false;
                break;
            case Rooms.FortressStage:
                cmrPos = new Vector2(0, 248);
                break;
            case Rooms.Shop:
                cmrPos = new Vector2(-272, 8);

                int maxY = Mathf.CeilToInt(itemCatalog.Length / 6);
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
        }

        if (stageSelected)
            selectMainSprite.sprite = Time.time % 0.1f < 0.05f ? selectFlashStage3 : selectFlashStage4;
        else
            selectMainSprite.sprite = Time.time % 0.5f < 0.25f ? selectFlashStage1 : selectFlashStage2;
    }

    private void InputFortressStageSelect()
    {
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

        fortressIndex.x = Mathf.Clamp(fortressIndex.x, 0, GameManager.maxFortressStage - 1);
        fortressIndex.y = 0;
    }
    private void UpdateFortressStageSelect()
    {
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

        if (shopIndex.y < shopIndex.z)
            shopIndex.z = shopIndex.y;
        else if (shopIndex.y > shopIndex.z + 1)
            shopIndex.z = shopIndex.y - 1;

        int maxY = Mathf.CeilToInt(itemCatalog.Length / 6);
        shopIndex.x = Mathf.Clamp(shopIndex.x, 0, 5);
        shopIndex.y = Mathf.Clamp(shopIndex.y, 0, maxY - 1);
        shopIndex.z = Mathf.Clamp(shopIndex.z, 0, maxY - 2);

        for (int x = 0; x < 6; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if ((shopIndex.z + z) * 6 + x >= itemCatalog.Length)
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
    }
    private void UpdateShopStageSelect()
    {
        shopSelect.sprite = Time.time % 0.5f < 0.25f ? selectFlashShop1 : selectFlashShop2;

        shopSelect.transform.position = new Vector3(shopSelectorOrigin.x + shopIndex.x * 32,
                                                    shopSelectorOrigin.y - (shopIndex.y - shopIndex.z) * 48,
                                                    shopSelect.transform.position.z);
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

        if (dataIndex.x < 0)
            ChangeRoom(Rooms.MainStage);

        dataIndex.x = Mathf.Clamp(dataIndex.x, 0, 1);
        dataIndex.y = Mathf.Clamp(dataIndex.y, 0, 9);
    }
    private void UpdateDataStageSelect()
    {
        SLButton.sprite = dataIndex.x == 0 ? SaveSprite : LoadSprite;
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
    }

}
