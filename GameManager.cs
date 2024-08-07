using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
	public string assetBundleName = "resources";
	public TMP_InputField inputField;
	public SpellManager spellManager;
	public Texture2D image;
	public GameObject[] spellPrefabs;
	public GameObject paintCanvasButton;
	public AssetBundle bundle;
	public string[] assetNames;

	private async void Awake()
	{
		PythonRunner.Setup();
		await LoadAssets();
	}

	private async Task LoadAssets()
	{
		await AssetBundleManager.Instance.LoadAssetBundle( assetBundleName );
		if( AssetBundleManager.Instance.GetAssetBundle( assetBundleName ) != null )
		{
			bundle = AssetBundleManager.Instance.GetAssetBundle( assetBundleName );
			List<GameObject> list = new List<GameObject>();
			assetNames = AssetBundleManager.Instance.GetAssetBundle( assetBundleName ).GetAllAssetNames();

			foreach( string assetName in assetNames )
			{
				if( assetName.StartsWith( "assets/resources/prefabs" ) )
				{
					GameObject prefab = AssetBundleManager.Instance.GetAssetBundle( assetBundleName ).LoadAsset<GameObject>( assetName );
					if( prefab != null )
					{
						list.Add( prefab );
					}
				}
			}

			spellPrefabs = list.ToArray();
			spellManager.SetPrefabNames();
			spellManager.resourceBundle = bundle;
		}
	}

	public void EndTurn()
	{
		spellManager.StartSpell( image );
	}

	public void ResetTurn()
	{
		paintCanvasButton.SetActive( true );
	}

	private void OnDestroy()
	{
		AssetBundleManager.Instance.UnloadAssetBundle( assetBundleName, true );
	}
}
