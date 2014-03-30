using UnityEngine;
using System.Collections;

public static class TextureGenerator {

	public static Texture2D MakeTexture(Color color)
	{
		return MakeTexture (color.r, color.g, color.b, color.a);
	}
	
	public static Texture2D MakeTexture(float R, float G, float B, float A)
	{
		Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);   	 	
    	texture.SetPixel(0, 0, new Color(R, G, B, A));   
   		texture.Apply();
		return texture;
	}
}
