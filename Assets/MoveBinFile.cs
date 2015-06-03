using UnityEngine;
using System.Collections;

public class MoveBinFile : MonoBehaviour {

	string filePath;// = System.IO.Path.Combine(Application.streamingAssetsPath, "bin.bin");
	string destinationPath;// = System.IO.Path.Combine(Application.dataPath, "bin.bin");

	// Use this for initialization
	IEnumerator Start () {
		filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "bin.bin");
		destinationPath = System.IO.Path.Combine(Application.persistentDataPath, "bin.bin");

		if (filePath.Contains ("://")) {
			WWW www = new WWW (filePath);
			yield return www;
			System.IO.StreamWriter writer = new System.IO.StreamWriter (destinationPath);
			writer.Write (www.bytes);
			writer.Close ();
		} else {
			System.IO.File.Copy (filePath, destinationPath, true);
		}

		Debug.Log ("xxx");

	}
	
	// Update is called once per frame
	void Update () {
	
	}



}
