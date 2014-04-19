using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class PlayerScript : MonoBehaviour, SSGameManager.IUpdatable {

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

	private int currentTierIndex;

	void Start() {
		power = 15;
		maxPower = 15;
        memory = 0;
        maxMemory = 30;
		cycleLength = 5;
		powerPerCycle = 5;

		defaultMaxPower = 15;
		defaultMaxMemory = 30;
		defaultCycleLength = 5;
		defaultPowerPerCycle = 5;

		currentTime = 0;

		currentTierIndex = 1;

		createUnitCostRef ();
		createUnitCooldownRef ();
		createUnitMemoryRef ();
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
		unitCostRef.Add("MagentaDoubleUnit", 1);
		unitCostRef.Add("MagentaIntUnit", 1);
		unitCostRef.Add("MagentaLongUnit", 1);

		//tier 1 Orange Unit Costs
		unitCostRef.Add("OrangeDoubleUnit", 1);
		unitCostRef.Add("OrangeIntUnit", 1);
		unitCostRef.Add("OrangeLongUnit", 1);

		//Tier 2 Magenta Unit Costs
		unitCostRef.Add("MagentaBinaryTreeUnit", 3);
		unitCostRef.Add("MagentaHeapUnit", 3);
		unitCostRef.Add("MagentaStaticUnit", 3);
		
		//tier 2 Orange Unit Costs
		unitCostRef.Add("OrangeBinaryTreeUnit", 3);
		unitCostRef.Add("OrangeHeapUnit", 3);
		unitCostRef.Add("OrangeStaticUnit", 3);

	}

	private void createUnitCooldownRef() {
		unitCooldownRef = new Dictionary<string, int> ();
		
		//Tier 1 Magenta Unit Cooldowns
		unitCooldownRef.Add("MagentaDoubleUnit", 1);
		unitCooldownRef.Add("MagentaIntUnit", 1);
		unitCooldownRef.Add("MagentaLongUnit", 1);
		
		//tier 1 Orange Unit Costs
		unitCooldownRef.Add("OrangeDoubleUnit", 1);
		unitCooldownRef.Add("OrangeIntUnit", 1);
		unitCooldownRef.Add("OrangeLongUnit", 1);
		
		//Tier 2 Magenta Unit Costs
		unitCooldownRef.Add("MagentaBinaryTreeUnit", 3);
		unitCooldownRef.Add("MagentaHeapUnit", 3);
		unitCooldownRef.Add("MagentaStaticUnit", 3);
		
		//tier 2 Orange Unit Costs
		unitCooldownRef.Add("OrangeBinaryTreeUnit", 3);
		unitCooldownRef.Add("OrangeHeapUnit", 3);
		unitCooldownRef.Add("OrangeStaticUnit", 3);
	}

	public void createUnitMemoryRef() {
		unitMemoryRef = new Dictionary<string, int> ();
		
		//Tier 1 Magenta Unit Cooldowns
		unitMemoryRef.Add("MagentaDoubleUnit", 1);
		unitMemoryRef.Add("MagentaIntUnit", 1);
		unitMemoryRef.Add("MagentaLongUnit", 1);
		
		//tier 1 Orange Unit Costs
		unitMemoryRef.Add("OrangeDoubleUnit", 1);
		unitMemoryRef.Add("OrangeIntUnit", 1);
		unitMemoryRef.Add("OrangeLongUnit", 1);
		
		//Tier 2 Magenta Unit Costs
		unitMemoryRef.Add("MagentaBinaryTreeUnit", 1);
		unitMemoryRef.Add("MagentaHeapUnit", 1);
		unitMemoryRef.Add("MagentaStaticUnit", 1);
		
		//tier 2 Orange Unit Costs
		unitMemoryRef.Add("OrangeBinaryTreeUnit", 1);
		unitMemoryRef.Add("OrangeHeapUnit", 1);
		unitMemoryRef.Add("OrangeStaticUnit", 1);
	}

	public void createTierUpgradeCostRef() {
		tierUpgradeCostRef = new Dictionary<string, int> ();

		tierUpgradeCostRef.Add ("Tier2", 30);
		tierUpgradeCostRef.Add ("Tier3", 50);
	}

    //Public Getters and Setters
    public int getPower(){
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
		return unitCostRef [unitName];
	}

	public int getUnitMemoryCost(string unitName) {
		return unitMemoryRef [unitName];
	}

	public bool generateUnit(string unitName) {
		int powerCheck = power;
		int memoryCheck = memory;
		powerCheck -= getUnitPowerCost(unitName);
		memoryCheck += getUnitMemoryCost (unitName);

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

	//Technology Funcitons
	public int getTierCost(string tier) {
		return tierUpgradeCostRef [tier];
	}



	public bool upgradeTier() {
		string nextTier = "Tier" + currentTierIndex;
		int cost = getTierCost(nextTier);
		int powerCheck = power - cost;

		if(powerCheck >= 0) {
			power = powerCheck;
			currentTierIndex++;
			return true;
		}

		return false;
	}

	public string getCurrentTier() {
		return ("Tier" + currentTierIndex);
	}
}
