using UnityEngine;
using System.Collections;

public class RAM : Building {

	public Object sparkPrefab;
	private GameObject currentSpark;

	public void Spark() {
		if (currentSpark == null) {
			currentSpark = (GameObject) Instantiate(sparkPrefab, transform.position, Quaternion.identity);
		}
		CancelInvoke("CancelSpark");
		Invoke("CancelSpark", 0.5f);
	}

	private void CancelSpark() {
		Destroy(currentSpark);
	}
}
