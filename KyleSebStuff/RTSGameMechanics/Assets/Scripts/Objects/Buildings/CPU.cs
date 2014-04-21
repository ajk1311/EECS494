using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Pathfinding;

public class CPU : DestructableBuilding 
{
    /*** Orange units ***/
    private static readonly string OrangeTier1MeleeName = "OrangeIntUnit";
    private static readonly string OrangeTier2MeleeName = "OrangePointerUnit";
    private static readonly string OrangeTier3MeleeName = "OrangeBinaryTreeUnit";

    private static readonly string OrangeTier1RangeName = "OrangeLongUnit";
    private static readonly string OrangeTier2RangeName = "OrangeFloatUnit";
    private static readonly string OrangeTier3RangeName = "OrangeStaticUnit";

    private static readonly string OrangeTier1SiegeName = "OrangeDoubleUnit";
    private static readonly string OrangeTier2SiegeName = "OrangeHeapUnit";
    private static readonly string OrangeTIer3SiegeName = "OrangeArrayUnit";

    /*** Magenta units ***/
    private static readonly string MagentaTier1MeleeName = "MagentaIntUnit";
    private static readonly string MagentaTier2MeleeName = "MagentaPointerUnit";
    private static readonly string MagentaTier3MeleeName = "MagentaBinaryTreeUnit";

    private static readonly string MagentaTier1RangeName = "MagentaLongUnit";
    private static readonly string MagentaTier2RangeName = "MagentaFloatUnit";
    private static readonly string MagentaTier3RangeName = "MagentaStaticUnit";

    private static readonly string MagentaTier1SiegeName = "MagentaDoubleUnit";
    private static readonly string MagentaTier2SiegeName = "MagentaHeapUnit";
    private static readonly string MagentaTIer3SiegeName = "MagentaArrayUnit";

	// Tier 1
	public Object tier1meleePrefab;
    public Texture tier1meleeIcon;
    public Object tier1rangePrefab;
    public Texture tier1rangeIcon;
    public Object tier1siegePrefab;
    public Texture tier1siegeIcon;
    // Tier 2
    public Object tier2meleePrefab;
    public Texture tier2meleeIcon;
    public Object tier2rangePrefab;
    public Texture tier2rangeIcon;
    public Object tier2siegePrefab;
    public Texture tier2siegeIcon;
    // Tier 3
    public Object tier3meleePrefab;
    public Texture tier3meleeIcon;
    public Object tier3rangePrefab;
    public Texture tier3rangeIcon;
    public Object tier3siegePrefab;
    public Texture tier3siegeIcon;

	public int spawnOffsetX;
	public Int3 spawnOffset;

    // Not the best design, but the fastest implementation right now
    private const int NONE = -1;
    private const int TIER_SELECT = 0;
    private const int TIER_1 = 1;
    private const int TIER_2 = 2;
    private const int TIER_3 = 3;
    private int mCurrentGuiModel;

    // Creation queue
    private int mCreationProgress;
    private List<CreationEvent> mCreationQueue;

    private Dictionary<Object, Texture> mIconMap;

    struct CreationEvent
    {
        public int cooldown;
        public Object prefab;
    }

	protected override void Start()
    {
		base.Start();
        mCreationProgress = 0;
        mCurrentGuiModel = NONE;
        mCreationQueue = new List<CreationEvent>();
		spawnOffset = new Int3(spawnOffsetX * Int3.Precision, 0, 0);
        mIconMap = new Dictionary<Object, Texture>()
        {
            {tier1meleePrefab, tier1meleeIcon},
            {tier1rangePrefab, tier1rangeIcon},
            {tier1siegePrefab, tier1siegeIcon},
            {tier2meleePrefab, tier2meleeIcon},
            {tier2rangePrefab, tier2rangeIcon},
            {tier2siegePrefab, tier2siegeIcon},
            {tier3meleePrefab, tier3meleeIcon},
            {tier3rangePrefab, tier3rangeIcon},
            {tier3siegePrefab, tier3siegeIcon}
        };
	}

	protected override GUIModelManager.GUIModel GetGUIModel() { return null; }

	public override void OnSelectionChanged(bool selected) 
    {
        SetGuiModel(selected ? TIER_SELECT : NONE);
	}

    public override void GameUpdate(float deltaTime) 
    {
        base.GameUpdate(deltaTime);
        if (mCreationQueue.Count == 0)
        {
            // There are no units waiting in the queue
            return;
        }
        mCreationProgress += (int) System.Math.Round(deltaTime * Int3.FloatPrecision);
        if (mCreationProgress >= mCreationQueue[0].cooldown * Int3.Precision)
        {
            mCreationProgress = 0;
            // Create the next unit
            Object prefab = mCreationQueue[0].prefab;
            Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
            GameObject unit = (GameObject) Instantiate(prefab, (Vector3) spawnPosition, Quaternion.identity);
            unit.transform.Rotate(new Vector3(0, 90, 0));
            unit.GetComponent<WorldObject>().playerID = PlayerID;
            mCreationQueue.RemoveAt(0);
            SetGuiModel(mCurrentGuiModel);
        }
    }

	private GUIModelManager.GUIModel BuildTierSelectionModel() 
	{
        PlayerScript player = GetAppropriatePlayerScript();

		GUIModelManager.GUIModel model = new GUIModelManager.GUIModel();
        model.leftPanelColumns = 2;
        model.centerPanelColumns = 3;

        GUIModelManager.Button random = new GUIModelManager.Button();
        random.text = "Random";
        random.clicked += new GUIModelManager.OnClick(ProduceRandomUnit);

        GUIModelManager.Button upgrade = new GUIModelManager.Button();
        upgrade.text = "Upgrade";
        upgrade.enabled = player.CurrentTier < 3;
        upgrade.clicked += new GUIModelManager.OnClick(UpgradeToNextTier);

		GUIModelManager.Button tier1 = new GUIModelManager.Button();
		tier1.text = "Tier 1";
        tier1.clicked += () => SetGuiModel(TIER_1);

		GUIModelManager.Button tier2 = new GUIModelManager.Button();
		tier2.text = "Tier 2";
		tier2.enabled = player.CurrentTier >= 2;
        tier2.clicked += () => SetGuiModel(TIER_2);

		GUIModelManager.Button tier3 = new GUIModelManager.Button();
		tier3.text = "Tier 3";
		tier3.enabled = player.CurrentTier == 3;
        tier3.clicked += () => {}; // SetGuiModel(TIER_3);

        model.AddButton(0, random);
        model.AddButton(0, upgrade);
        model.AddButton(0, tier1);
        model.AddButton(0, tier2);
        model.AddButton(0, tier3);

        AddQueueButtons(model);

        return model;
	}

	private GUIModelManager.GUIModel BuildTier1UnitCreationModel() 
	{
        GUIModelManager.GUIModel model = new GUIModelManager.GUIModel();
        model.leftPanelColumns = 3;
        model.centerPanelColumns = 3;

		GUIModelManager.Button melee = new GUIModelManager.Button();
        melee.icon = tier1meleeIcon;
        melee.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier1MeleeName : MagentaTier1MeleeName, tier1meleePrefab);

		GUIModelManager.Button range = new GUIModelManager.Button();
        range.icon = tier1rangeIcon;
        range.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier1RangeName : MagentaTier1RangeName, tier1rangePrefab);

		GUIModelManager.Button siege = new GUIModelManager.Button();
        siege.icon = tier1siegeIcon;
        siege.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier1SiegeName : MagentaTier1SiegeName, tier1siegePrefab);

        model.AddButton(0, melee);
        model.AddButton(0, range);
        model.AddButton(0, siege);

        AddBackButton(model);
        AddQueueButtons(model);

        return model;
	}

    private GUIModelManager.GUIModel BuildTier2UnitCreationModel()
    {
        GUIModelManager.GUIModel model = new GUIModelManager.GUIModel();
        model.leftPanelColumns = 3;
        model.centerPanelColumns = 3;

        GUIModelManager.Button melee = new GUIModelManager.Button();
        melee.icon = tier2meleeIcon;
        melee.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier2MeleeName : MagentaTier2MeleeName, tier2meleePrefab);

        GUIModelManager.Button range = new GUIModelManager.Button();
        range.icon = tier2rangeIcon;
        range.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier2RangeName : MagentaTier2RangeName, tier2rangePrefab);

        GUIModelManager.Button siege = new GUIModelManager.Button();
        siege.icon = tier2siegeIcon;
        siege.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier2SiegeName : MagentaTier2SiegeName, tier2siegePrefab);

        model.AddButton(0, melee);
        model.AddButton(0, range);
        model.AddButton(0, siege);

        AddBackButton(model);
        AddQueueButtons(model);

        return model;
    }

    private GUIModelManager.GUIModel BuildTier3UnitCreationModel()
    {
        GUIModelManager.GUIModel model = new GUIModelManager.GUIModel();
        model.leftPanelColumns = 3;
        model.centerPanelColumns = 3;

        GUIModelManager.Button melee = new GUIModelManager.Button();
        melee.icon = tier3meleeIcon;
        melee.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier3MeleeName : MagentaTier3MeleeName, tier3meleePrefab);

        GUIModelManager.Button range = new GUIModelManager.Button();
        range.icon = tier3rangeIcon;
        range.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTier3RangeName : MagentaTier3RangeName, tier3rangePrefab);

        GUIModelManager.Button siege = new GUIModelManager.Button();
        siege.icon = tier3siegeIcon;
        siege.clicked += () => QueueUnitCreation(playerID == 1 ?
            OrangeTIer3SiegeName : MagentaTIer3SiegeName, tier3siegePrefab);

        model.AddButton(0, melee);
        model.AddButton(0, range);
        model.AddButton(0, siege);

        AddBackButton(model);
        AddQueueButtons(model);

        return model;
    }

	private void AddBackButton(GUIModelManager.GUIModel model) 
    {
		GUIModelManager.Button back = new GUIModelManager.Button();
		back.text = "Back";
        back.clicked += () => SetGuiModel(TIER_SELECT);
		model.AddButton(0, back);
	}

    private void AddQueueButtons(GUIModelManager.GUIModel model) 
    {
        foreach (var i in mCreationQueue)
        {
            GUIModelManager.Button button = new GUIModelManager.Button();
            button.icon = mIconMap[i.prefab];
            button.clicked += () =>
                {
                    mCreationProgress = 0;
                    mCreationQueue.Remove(i);
                };
            model.AddButton(1, button);
        }
    }

	void ProduceRandomUnit() 
    {
		int unit = Bellagio.gambleUnit(playerID);				
		switch(unit) 
        {
			case 0:
                QueueUnitCreation(playerID == 1 ? 
                    OrangeTier1RangeName : MagentaTier1RangeName, tier1rangePrefab, true);
				break;

			case 1:
                QueueUnitCreation(playerID == 1 ? 
                    OrangeTier1SiegeName : MagentaTier1SiegeName, tier1siegePrefab, true);
				break;

			case 2:
                QueueUnitCreation(playerID == 1 ? 
                    OrangeTier1MeleeName : MagentaTier1MeleeName, tier1meleePrefab, true);
				break;
                
			case 3:
                QueueUnitCreation(playerID == 1 ?
                    OrangeTier2MeleeName : MagentaTier2MeleeName, tier2meleePrefab, true);
				break;

			case 4:
                QueueUnitCreation(playerID == 1 ?
                    OrangeTier2SiegeName : MagentaTier2SiegeName, tier2siegePrefab, true);
				break;

			case 5:
                QueueUnitCreation(playerID == 1 ?
                    OrangeTier2RangeName : MagentaTier2RangeName, tier2rangePrefab, true);
				break;

            case 6:
                QueueUnitCreation(playerID == 1 ?
                    OrangeTier3RangeName : MagentaTier3RangeName, tier3rangePrefab, true);
                break;

            case 7:
                QueueUnitCreation(playerID == 1 ?
                    OrangeTIer3SiegeName : MagentaTIer3SiegeName, tier3siegePrefab, true);
                break;

            case 8:
                QueueUnitCreation(playerID == 1 ?
                    OrangeTier3MeleeName : MagentaTier3MeleeName, tier3meleePrefab, true);
                break;
		}
	}

    private void QueueUnitCreation(string unitName, Object unitPrefab, bool random = false)
    {
        PlayerScript me = GetAppropriatePlayerScript();
        if (me.canGenerateUnit(unitName))
        {
            ParseManager.LogEvent(ParseManager.ParseEvent.UnitCreation,
                playerID, unitName, "CPU" + (random ? "-random" : ""));
            mCreationQueue.Add(new CreationEvent()
            {
                cooldown = me.getUnitCooldown(unitName),
                prefab = unitPrefab
            });
            SetGuiModel(mCurrentGuiModel);
        }
    }

	void UpgradeToNextTier() 
    {
		PlayerScript me = GetAppropriatePlayerScript();
		if (me.upgradeTier()) 
        {
			BuildTierSelectionModel();
            SetGuiModel(TIER_SELECT);
		}
	}

	private PlayerScript GetAppropriatePlayerScript() 
    {
		PlayerScript po = GameObject.Find("Player").GetComponent<PlayerScript>();
		PlayerScript oo = GameObject.Find("Opponent").GetComponent<PlayerScript>();
		return playerID == po.id ? po : oo;
	}

    private void SetGuiModel(int guiModelType)
    {
        switch (guiModelType)
        {
            case TIER_SELECT:
                GUIModelManager.SetCurrentModel(playerID, BuildTierSelectionModel());
                break;

            case TIER_1:
                GUIModelManager.SetCurrentModel(playerID, BuildTier1UnitCreationModel());
                break;

            case TIER_2:
                GUIModelManager.SetCurrentModel(playerID, BuildTier2UnitCreationModel());
                break;

            case TIER_3:
                GUIModelManager.SetCurrentModel(playerID, BuildTier3UnitCreationModel());
                break;

            case NONE:
                GUIModelManager.SetCurrentModel(playerID, null);
                break;
        }
        mCurrentGuiModel = guiModelType;
    }
}
