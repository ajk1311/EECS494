using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	public string playerName;
	private int power = 0;
	private int memory = 0;
	private int maxMemory = 0;

	// Use this for initialization
	void Start () {
		power = 15;
        memory = 0;
        maxMemory = 30;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Public Getters and Setters
    public int getPower(){
        return power;
    }

    public void setPower(int power){
        this.power = power;
    }

    public int getMemory(){
        return memory;
    }

    public void setMemory(int memory){
        this.memory = memory;
    }

    public int getMaxMemory(){
        return maxMemory;
    }

    public void setMaxMemory(int maxMemory){
        this.maxMemory = maxMemory;
    }
}
