using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Building : WorldObject  {

	public Object explosionPrefab;

	protected override void OnDestroy() {
		base.OnDestroy ();
		GameObject obj = (GameObject)Instantiate (explosionPrefab, transform.position, Quaternion.identity);
	}
}
