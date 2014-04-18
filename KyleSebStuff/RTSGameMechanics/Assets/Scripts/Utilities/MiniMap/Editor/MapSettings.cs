using System.IO;

using UnityEngine;
using UnityEditor;


public class MapSettings
{
	public string segmentName{ get; private set; }

	private int _length, _width;
	private int _xmin, _xMax, _zMin, _zMax;

	public int length { 
		get { return this._length; } 
	}
	public int width { 
		get { return this._width; } 
	}
	public int xMin { 
		get { return this._xmin; }
	}

	public int xMax { 
		get { return this._xMax; }
	}

	public int zMin { 
		get { return this._zMin; }
	}

	public int zMax { 
		get { return this._zMax; }
	}

		public MapSettings (string fileName)
		{
			var textAsset =(TextAsset) AssetDatabase.LoadAssetAtPath(string.Format("Assets/Minimap/{0}", fileName), typeof(TextAsset));
			if (textAsset == null)
				throw new System.IO.FileNotFoundException("settings file not found under minimap folder");

			using(var reader = new StringReader(textAsset.text))
			{
				do
				{
					var line = reader.ReadLine();
					line = line.Replace(" ", string.Empty).ToLower();

					var split = line.Split('=');
					if(split.Length < 1)
						throw new System.IO.FileLoadException("map data is corrupted");

					var first = split[0];
					var second = split[1];

					var formattedSecond = second.Substring(1, second.Length - 2);
					
					switch(first)
					{
						case "name":
						{
							if(second[0] == '\"')
							{
								this.segmentName = second.Substring(1, second.Length - 2);
								if(string.IsNullOrEmpty(segmentName))
									throw new System.IO.FileLoadException("map data is corrupted");
								Debug.Log ("segment name is: " + this.segmentName);
							}
						}
						break;
						case "length":
						{
							if(int.TryParse(formattedSecond, out _length) == false){
								throw new System.IO.FileLoadException("map data is corrupted");
							}
						}
						break;

						case "width":
						{
							if(int.TryParse(formattedSecond, out _width) == false){
								throw new System.IO.FileLoadException("map data is corrupted");
							}
						}
						break;

						case "xmin":
						{
							if(int.TryParse(formattedSecond, out _xmin) == false){
								throw new System.IO.FileLoadException("map data is corrupted");
							}
						}
						break;

						case "xmax":
						{
							if(int.TryParse(formattedSecond, out _xMax) == false){
								throw new System.IO.FileLoadException("map data is corrupted");
							}
						}
						break;

						case "zmin":
						{
							if(int.TryParse(formattedSecond, out _zMin) == false){
								throw new System.IO.FileLoadException("map data is corrupted");
							}
						}
						break;

						case "zmax":
						{
							if(int.TryParse(formattedSecond, out _zMax) == false){
								throw new System.IO.FileLoadException("map data is corrupted");
							}
						}
						break;
					}
				}
				while(reader.Peek() != -1);
			}	
		}
}




