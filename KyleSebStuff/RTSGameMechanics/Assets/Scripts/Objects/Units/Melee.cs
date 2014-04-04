using UnityEngine;
using System.Collections;

public class Melee : Unit {


	// Use this for initialization
	protected override void Start () {
		this.attackRange = 5;
		this.speed = 100;
	}
	
	// Update is called once per frame
    public override void GameUpdate(float deltaTime) {
	
	}
}
