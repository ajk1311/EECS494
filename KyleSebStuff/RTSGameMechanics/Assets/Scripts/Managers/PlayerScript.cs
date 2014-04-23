using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class PlayerScript : MonoBehaviour, SSGameManager.IUpdatable {

	public static readonly int Tier2UpgradeCost = 50;
	public static readonly int Tier3UpgradeCost = 100;

	public static readonly int RandomCost = 3;

	public int id;
	public string playerName;
	private int power;
	private int maxPower;
	private int memory;
	private int maxMemory;

	private int powerPerCycle;
	private float cycleLength;
	private float currentTime;

	private int defaultMaxPower;
	private int defaultMaxMemory;
	private int defaultCycleLength;
	private int defaultPowerPerCycle;

	public Dictionary<string, int> unitCostRef;
	public Dictionary<string, int> unitCooldownRef;
	public Dictionary<string, int> unitMemoryRef;
	public Dictionary<string, int> tierUpgradeCostRef;

	public bool centerTowerBuff;

	private int currentTierIndex;
	public int CurrentTier {
		get { return currentTierIndex; }
	}

	void Start() {
		SSGameManager.Register(this);

		power = 10;
		maxPower = 200;
        memory = 0;
        maxMemory = 100;
		cycleLength = 8;
		powerPerCycle = 6;

		defaultMaxPower = 15;
		defaultMaxMemory = 30;
		defaultCycleLength = 5;
		defaultPowerPerCycle = 5;

		currentTime = 0;

		currentTierIndex = 1;

		createUnitCostRef();
		createUnitCooldownRef();
		createUnitMemoryRef();
		createTierUpgradeCostRef();

		centerTowerBuff = false;
	}

    public void GameUpdate(float deltaTime) {
		currentTime += (int) System.Math.Round(deltaTime * Int3.FloatPrecision);;
		if(currentTime >= (int) System.Math.Round(cycleLength * Int3.FloatPrecision)) {
			currentTime = 0;
			addPower(powerPerCycle);
		}
    }

	private void createUnitCostRef() {
		unitCostRef = new Dictionary<string, int> ();

		//Tier 1 Magenta Unit Costs
		unitCostRef.Add("MagentaDoubleUnit", 2);
		unitCostRef.Add("MagentaIntUnit", 2);
		unitCostRef.Add("MagentaLongUnit", 2);

		//tier 1 Orange Unit Costs
		unitCostRef.Add("OrangeDoubleUnit", 2);
		unitCostRef.Add("OrangeIntUnit", 2);
		unitCostRef.Add("OrangeLongUnit", 2);

		//Tier 2 Magenta Unit Costs
		unitCostRef.Add("MagentaPointerUnit", 6);
		unitCostRef.Add("MagentaHeapUnit", 6);
		unitCostRef.Add("MagentaFloatUnit", 6);
		
		//tier 2 Orange Unit Costs
		unitCostRef.Add("OrangePointerUnit", 6);
		unitCostRef.Add("OrangeHeapUnit", 6);
		unitCostRef.Add("OrangeFloatUnit", 6);

		//tier 3 Magenta Unit Costs
		unitCostRef.Add("MagentaBinaryTreeUnit", 18);
		unitCostRef.Add("MagentaStaticUnit", 18);
		unitCostRef.Add ("MagentaArrayUnit", 18);

		//tier3 Orange Unit Costs
		unitCostRef.Add("OrangeBinaryTreeUnit", 18);
		unitCostRef.Add ("OrangeStaticUnit", 18);
		unitCostRef.Add ("OrangeArrayUnit", 18);
	}

	private void createUnitCooldownRef() {
		unitCooldownRef = new Dictionary<string, int> ();
		
		//Tier 1 Magenta Unit Cooldowns
		unitCooldownRef.Add("MagentaDoubleUnit", 1);
		unitCooldownRef.Add("MagentaIntUnit", 1);
		unitCooldownRef.Add("MagentaLongUnit", 1);
		
		//tier 1 Orange Unit Cooldowns
		unitCooldownRef.Add("OrangeDoubleUnit", 1);
		unitCooldownRef.Add("OrangeIntUnit", 1);
		unitCooldownRef.Add("OrangeLongUnit", 1);
		
		//Tier 2 Magenta Unit Cooldowns
		unitCooldownRef.Add("MagentaPointerUnit", 3);
		unitCooldownRef.Add("MagentaHeapUnit", 3);
		unitCooldownRef.Add("MagentaFloatUnit", 3);
		
		//tier 2 Orange Unit Cooldowns
		unitCooldownRef.Add("OrangePointerUnit", 3);
		unitCooldownRef.Add("OrangeHeapUnit", 3);
		unitCooldownRef.Add("OrangeFloatUnit", 3);
		
		//tier 3 Magenta Unit Cooldowns
		unitCooldownRef.Add("MagentaBinaryTreeUnit", 9);
		unitCooldownRef.Add("MagentaStaticUnit", 9);
		unitCooldownRef.Add ("MagentaArrayUnit", 9);
		
		//tier3 Orange Unit Cooldowns
		unitCooldownRef.Add("OrangeBinaryTreeUnit", 9);
		unitCooldownRef.Add ("OrangeStaticUnit", 9);
		unitCooldownRef.Add ("OrangeArrayUnit", 9);
	}

	public void createUnitMemoryRef() {
		unitMemoryRef = new Dictionary<string, int> ();
		
		//Tier 1 Magenta Unit Memory Cost
		unitMemoryRef.Add("MagentaDoubleUnit", 1);
		unitMemoryRef.Add("MagentaIntUnit", 1);
		unitMemoryRef.Add("MagentaLongUnit", 1);
		
		//tier 1 Orange Unit Memory Cost
		unitMemoryRef.Add("OrangeDoubleUnit", 1);
		unitMemoryRef.Add("OrangeIntUnit", 1);
		unitMemoryRef.Add("OrangeLongUnit", 1);
		
		//Tier 2 Magenta Unit Memory Cost
		unitMemoryRef.Add("MagentaPointerUnit", 1);
		unitMemoryRef.Add("MagentaHeapUnit", 1);
		unitMemoryRef.Add("MagentaFloatUnit", 1);
		
		//tier 2 Orange Unit Memory Cost
		unitMemoryRef.Add("OrangePointerUnit", 1);
		unitMemoryRef.Add("OrangeHeapUnit", 1);
		unitMemoryRef.Add("OrangeFloatUnit", 1);
		
		//tier 3 Magenta Unit Memory Cost
		unitMemoryRef.Add("MagentaBinaryTreeUnit", 1);
		unitMemoryRef.Add("MagentaStaticUnit", 1);
		unitMemoryRef.Add ("MagentaArrayUnit", 1);
		
		//tier3 Orange Unit Memory Cost
		unitMemoryRef.Add("OrangeBinaryTreeUnit", 1);
		unitMemoryRef.Add ("OrangeStaticUnit", 1);
		unitMemoryRef.Add ("OrangeArrayUnit", 1);
	}

	public void createTierUpgradeCostRef() {
		tierUpgradeCostRef = new Dictionary<string, int> ();
		tierUpgradeCostRef.Add("Tier1", Tier2UpgradeCost);
		tierUpgradeCostRef.Add("Tier2", Tier3UpgradeCost);
	}

    //Public Getters and Setters
    public int getPower() {
        return power;
    }
    public void setPower(int amount) {
        power = amount;
    }

    public int getMemory() {
        return memory;
    }
    public void setMemory(int amount) {
        memory = amount;
    }

    public int getMaxMemory(){
        return maxMemory;
    }
    public void setMaxMemory(int amount) {
        maxMemory = amount;
    }

	public int getMaxPower(int amount) {
		return maxPower;
	}
	public void setMaxPower(int amount) {
		maxPower = amount;
	}

	public void setCycleLength(int amount) {
		cycleLength = amount;
	}

	public void setPowerPerCycle(int amount) {
		powerPerCycle = amount;
	}

	public void addPowerPerCycle(int amount) {
		powerPerCycle += amount;
	}

	public void removePowerPerCycle(int amount) {
		powerPerCycle -= amount;
	}

	//Reset Functioins
	public void resetMaxPower() {
		maxPower = defaultMaxPower;
	}

	public void resetMaxMemory() {
		maxMemory = defaultMaxMemory;
	}

	public void resetCycleLength() {
		cycleLength = defaultCycleLength;
	}

	public void resetMemoryPerCycle() {
		powerPerCycle = defaultPowerPerCycle;
	}

	//Unit Functions
	public int getUnitCooldown(string unitName) {
		return unitCooldownRef[unitName];
	}

	public int getUnitPowerCost(string unitName) {
		return unitCostRef[unitName];
	}

	public int getUnitMemoryCost(string unitName) {
		return unitMemoryRef[unitName];
	}

	public bool canGenerateUnit(string unitName, bool random = false) {
		int powerCheck = power;
		int memoryCheck = memory;
		powerCheck -= random ? RandomCost : getUnitPowerCost(unitName);
		memoryCheck += getUnitMemoryCost(unitName);

		if(powerCheck >= 0 && memoryCheck <= maxMemory) {
			power = powerCheck;
			memory = memoryCheck;
			return true;
		}
		else {
			return false;
		}
	}

	public void updateMemoryUnitDied(string unitName) {
		memory -= unitMemoryRef [unitName];
		if(memory <= 0) {
			memory = 0;
		}
	}

	public void updatePowerUnitDied(string unitName) {
		power += unitCostRef[unitName];
		if (power > maxPower) {
			power = maxPower;
		}
	}

	public void combinationMemoryUpdate(string unitName) {
		memory += unitMemoryRef [unitName];
	}

	//Power Functions
	public void addPower(int amount) {
		power += amount;
		if(power > maxPower) {
			power = maxPower;
		}
	}

	//Center Tower Buff
	public void setCenterTowerBuff(bool val) {
		centerTowerBuff = val;
	}

	//Technology Funcitons
	public int getTierCost(string tier) {
		return tierUpgradeCostRef[tier];
	}

	public bool upgradeTier() {
		if (currentTierIndex == 3) {
			return false;
		}

		string nextTier = "Tier" + (currentTierIndex);
		int cost = getTierCost(nextTier);
		int powerCheck = power - cost;

		if (powerCheck >= 0) {
			power = powerCheck;
			currentTierIndex++;
			return true;
		}
		return false;
	}

	public int getCurrentTier() {
		return currentTierIndex;
	}

	private void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
