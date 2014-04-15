using UnityEngine;
using System.Collections;

public class MoveCommandCursor : MonoBehaviour {

    public Texture2D[] moveCursors;
    // Use this for initialization
    void Start() {
        Invoke("Destroy", 2f);
        renderer.material.shader = Shader.Find("Transparent/Diffuse");
    }
    
    // Update is called once per frame
    void Update() {
        Texture2D currentTexture = moveCursors[(int)(Time.time * 5) % moveCursors.Length];
        renderer.material.mainTexture = currentTexture;
    }

    void Destroy() {
        Destroy(this.gameObject);
    }
}
