using UnityEngine;
using System.Collections;

public class Melee : Unit {


	// Use this for initialization
	protected override void Start () {
		attackRange = 2;
		speed = 100;
	}
	
	// Update is called once per frame
    public override void GameUpdate(float deltaTime) {
	
	}
}
