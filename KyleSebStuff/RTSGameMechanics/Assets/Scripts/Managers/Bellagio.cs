using UnityEngine;
using System.Collections;

public static class Bellagio {
	
	public static int gambleUnit(int playerID) {
		var script1 = GameObject.Find("Player").GetComponent<PlayerScript>();
		var script2 = GameObject.Find("Opponent").GetComponent<PlayerScript>();
		//int level = getCurrentTier();
		int playerLevel = playerID == script1.id? script1.getCurrentTier() : script2.getCurrentTier();
		int num = Random.Range(0, 100);
		Debug.Log("rand is: " + num);
		switch(playerLevel) {
			//tier1
			//72% tier 1, 24% tier 2, 3% tier 3
			case 1:
				if(num < 72) {
					Debug.Log("got in the basic bitch");
					return level(0);
				}
				else if(num < 96) {
					return level(1);
				}
				else {
					return level(2);
				}
				break;
			//tier 2
			//30% tier 1, 45% tier 2, 25% tier 3
			case 2:
				if(num < 30) {
					return level(0);
				}
				else if(num < 75) {
					return level(1);
				}
				else {
					return level(2);
				}
				break;
			//tier 3
			//12% tier 1, 28% tier 2, 60% tier 3
			case 3:
				if(num < 12) {
					return level(0);
				}
				else if(num < 40) {
					return level(1);
				}
				else {
					return level(2);
				}
				break;
			default:
				return -1;
		}
	}

	private static int level(int level) {
		int rand = Random.Range(0, 3);
		Debug.Log("return value: " + (level*3 + rand).ToString());
		return level*3 + rand;
	}
}
