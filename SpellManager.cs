using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Linq;

public class SpellManager : MonoBehaviour
{
	private const string apiKey = "";
	private const string apiUrl = "https://api.openai.com/v1/chat/completions";
	private const string pythonScriptPath = "Assets/Scripts/ClipProcessor.py";
	private const int maxRetries = 3;
	private int retryAttempt = 0;
	private Enemy enemyScript;
	private Player playerScript;
	public Player player;
	public static string caughtError;
	public string imageData;
	public string prefabNames;
	public GameManager gameManager;
	public Text progressText;
	public AssetBundle resourceBundle;

	private void Start()
	{
		gameManager = FindObjectOfType<GameManager>();
		Debug.Log( "GameManager found: " + ( gameManager != null ) );
		
	}

	public void InitAfterAssetBundle()
	{
		StartCoroutine( LoadAssetBundle() );
	}

	private IEnumerator LoadAssetBundle()
	{
		if( !AssetBundleManager.Instance.GetAssetBundle( "resources" ) )
		{
			yield return AssetBundleManager.Instance.LoadAssetBundle( "resources" );
		}
		resourceBundle = AssetBundleManager.Instance.GetAssetBundle( "resources" );

		if( resourceBundle == null )
		{
			Debug.LogError( "Failed to load AssetBundle!" );
			yield break;
		}
	}

	public void SetPrefabNames()
	{
		foreach( GameObject spell in gameManager.spellPrefabs )
		{
			prefabNames += spell.name + "\n";
		}
	}

	public void GetSpellCode( Texture2D image )
	{
		Debug.Log( "Started spell code generation" );
		if( image != null )
		{
			imageData = Convert.ToBase64String( image.EncodeToPNG() );
		}
		else
		{
			Texture2D paintedSprite = resourceBundle.LoadAsset<Texture2D>( "PaintedSprite" );
			imageData = Convert.ToBase64String( paintedSprite.EncodeToPNG() );
		}
		Debug.Log( "Start image analyzing" );
		StartCoroutine( RunPythonScriptAndGenerateSpellCode() );
	}

	private IEnumerator RunPythonScriptAndGenerateSpellCode()
	{
		Debug.Log( "RunPythonScriptAndGenerateSpellCode started" );
		string featuresJson = null;
		int attempt;
		yield return PythonRunner.RunPythonScriptAsync( pythonScriptPath, imageData, UpdateProgress, result => featuresJson = result );
		Debug.Log( "Python script result: " + featuresJson );
		if( string.IsNullOrEmpty( featuresJson ) )
		{
			Debug.LogError( "Received empty or invalid JSON from python script." );
			yield break;
		}
		retryAttempt = 0;
		for( attempt = 1; attempt <= maxRetries; attempt++ )
		{
			var messages = new[]
			{
				new { role = "system", content = "You are a helpful assistant that generates Unity C# code for spells, you do this based on your interpretation of image features and information you get sent. Ensure that all spells are self-contained, with all necessary using directives, class definitions, and methods. Every component must be added in code, with no prefabs. Include error handling and debugging, and ensure all code is properly formatted with correct casing and syntax. " +
				$"To make sure a spell can hit an Enemy, there is an enemy in the game with the script Enemy on it with a public function void TakeDamage which you can target, make sure that all damage is always in int value, you always have to get the target in code as everything is done runtime, if you want to target the player for instance with an healing spell also use the right function for this on the right object, look for the scripts these are: {JsonConvert.SerializeObject(enemyScript)} for the enemy and {JsonConvert.SerializeObject(playerScript)} for the player.It is a unity 2D game, so use 2d variants for components. " +
				"Make sure that each spell has enough time to be seen in the game, spells can be moving at a slower pace and should be visible to the player and should look like magic. spells can be offensive, defensive, or utility based, for instance a shield or healing spell or a damaging spell, they can also sizzle meaning they fail, this is all up to your interpretation of the spell image you get, you should see the image kind of like an incantation, an enemy or player will start with 1000 HP so make sure the spell strength fits this, also make sure the spell feels unique to the image, a spell could be a projectile but it could also be something else, it could be creating more objects, summon companions, make multiple hits or other effects that you could think of when interpreting the image. " +
				"For spell travel speeds and sizes etc, the distance between the player and the enemy you should get from asking the apropriate transforms, they are both tagged (Player is tagged Player and Enemy is tagged Enemy), the player and enemy are both 1 unit in width and 2 in height, make sure a spell's visuals are always correct size to be visible to the player, Also make sure the visuals also move to the target and dont just float in the middle, for moving the spell you can use LeanTween as this is installed. Make sure spells will always destroy themself after finishing their action, so spells should never run infinitely, it should always have a method to destroy itself after a max of 30 seconds." +
				"Every part of the code you will write should be fully written and functional, make sure there are no empty methods or not set variables/fields, if you need a component make sure it is added in code, if you need a library for a certain effect make sure the library is included. Be sure to add every component you need, you can add any component or library necessary for the spell, for instances LeanTween is installed so you can use this. Make sure all the code is finished and all functions are filled in." +
				" Make sure to only reply with the code and no other messages and make sure all syntax is correctly and completed, dont forget semi colons etc, and dont add comments as these are not necessary, float values should always be ended with a lower f (example: 0.5f) and dont forget \";\", \",\", \")\" and \"}\" in the right places, look out for case sensitivity and make sure this is correct. Make sure the shader code is written properly and has no syntax errors and the right case sensitivity. Be sure to add in error handling and debugging into the spell code. Also check for common spelling errors and syntax mistakes and make sure these are correct." },
				new { role = "user", content = $"Generate a complete C# Unity script without syntax errors to create a spell based on the following image features and information: {featuresJson}. All spells should be unique and fitting to the image provided, so for instance a red image has more chance on being fire type while blue is more likely to be ice etc. Ensure the script is self-contained and includes all necessary using directives, class definitions, and methods. Make sure that the spell you create is fitting to the image. Make it so a spell moves with LeanTween so you can make the movement interesting, do however make it slow enough for the player to see, so every spell should take at least 4 seconds but can be more, also be sure to make all visuals be in a high layer so it surely is visible, spells should always be properly visible. Be sure to set all values in code so there are no null ref errors as well as to make everything as visually appealing as possible. Be sure to add every component you need, you can add any component or library necessary for the spell, for instances LeanTween is installed so you can use this for spell movement, with this you can make the spell movement interesting as well, so make the spell move and behave in a way you think is fitting for it. Make sure all the code is finished and all functions are filled in. Make sure to only reply with the code and no other messages and make sure all syntax is correct and complete, don't forget semicolons, commas, parentheses, and braces in the right places, look out for case sensitivity. Be sure to include error handling and debugging. You can use these prefabs where appropriate: {gameManager.spellPrefabs}, to use these prefabs you can ask them from the GameManager it has a public array of prefabs, the names for these prefabs are: {prefabNames}, the prefablist is public so use FindObjectOfType<GameManager>().spellPrefabs, these prefabs are magic visuals, try to make good use of them to make everything look good, they all have particlesystems on them already, so you can hook into that, you could for instance look for the particle system component and change the colour based on the spell image, the names are descriptive of what the prefab looks like. In this list of prefabs are also prefabs which contain the word \"Magiccircle\", this should almost always be used behind the player when they are casting a spell to show that they are casting." },
			};

			var jsonData = new
			{
				model = "gpt-4",
				messages = messages,
				max_tokens = 4000,
				temperature = 0,
				top_p = 0.7,
				frequency_penalty = 0.6,
				presence_penalty = 0.0
			};

			var request = new UnityWebRequest( apiUrl, "POST" );
			string jsonString = JsonConvert.SerializeObject( jsonData );
			byte[] bodyRaw = Encoding.UTF8.GetBytes( jsonString );
			request.uploadHandler = new UploadHandlerRaw( bodyRaw );
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader( "Content-Type", "application/json" );
			request.SetRequestHeader( "Authorization", "Bearer " + apiKey );

			yield return request.SendWebRequest();

			if( request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError )
			{
				Debug.LogError( request.error );
				yield return null;
			}
			else
			{
				string jsonResponse = request.downloadHandler.text;
				Debug.Log( "Parsing spell code" );
				SpellCode spellCode = ParseSpellCode( jsonResponse );
				if( spellCode != null )
				{
					Debug.Log( "Code parsed, start executing" );
					StartCoroutine( ExecuteSpellCoroutine( spellCode ) );
					break; // Exit the retry loop
				}
				else
				{
					Debug.LogWarning( $"Attempt {attempt} failed. Retrying..." );
				}
			}
		}
		if( attempt > maxRetries )
		{
			Debug.LogError( "Failed to generate a complete spell script after maximum retries." );
		}
	}

	private void UpdateProgress( string message )
	{
		Debug.Log( message );
	}

	public IEnumerator FixSpellCode( string codeToFix )
	{
		for( int attempt = 1; attempt <= maxRetries; attempt++ )
		{
			Debug.Log( "Started fixing code" );
			var messages = new[]
			{
				new { role = "system", content = "You are a helpful assistant that fixes Unity C# code errors, make sure to properly fix the code. Check for syntax errors. In destroy() functions make sure the right object is called which should be destroyed. Check if operators are correct and used on a proper value, for instance not using == with transform.position as it is only assignment, call, increment etc. Make sure that all #if are properly closed with an #endif and make sure we dont use any unityeditor windows or other editor ui as the code is only able to be ran dynamically at runtime. When writing arrays make sure all commas are correct. Be sure to place all semicolons, commas, parentheses, and braces in the right places. Fix all syntax errors. Make sure that you're always using existing definitions. Make sure that when using a variable in multiple places that this is global. Be sure to add every component you need, you can add any component or library necessary for the spell, for instances LeanTween is installed so you can use this." },
				new { role = "user", content = $"Fix all errors within this script: \"{codeToFix}\", the found errors are \"{caughtError}\". Ensure the script is self-contained and includes all necessary using directives, class definitions, methods. Make sure that the spell you create is fitting to the provided image, keep in mind the colours of the image as well. Be sure to add every component you need, you can add any component or library necessary for the spell, for instances LeanTween is installed so you can use this. Make it so a spell moves with LeanTween so you can make the movement interesting, do however make it slow enough for the player to see, so every spell should take at least 4 seconds but can be more, also be sure to make all visuals be in a high layer so it surely is visible, spells should always be properly visible. Be sure to set all values in code so there are no null ref errors, really make sure all fields are assigned in code. Be sure to add every component you need, you can add any component or library necessary for the spell. Make sure all the code is finished and all functions are filled in. Make sure to only reply with the code and no other messages and make sure all syntax is correct and complete, don't forget semicolons, commas, parentheses, and braces in the right places, look out for case sensitivity. Be sure to include error handling and debugging. For enemy Targeting make sure you always use the right value, this is the enemy script: \"{JsonConvert.SerializeObject(enemyScript)}\". You can use these prefabs where appropriate: {gameManager.spellPrefabs}, to use these prefabs you can ask them from the GameManager it has a public array of prefabs, the names for these prefabs are: {prefabNames}, the prefablist is public so use FindObjectOfType<GameManager>().spellPrefabs, these prefabs are magic visuals, try to make good use of them to make everything look good, make sure that when you look for an object in the array it actually exists and if it doesnt you use another object, they all have particlesystems on them already, so you can hook into that, you could for instance look for the particle system component and change the colour based on the spell image, the names are descriptive of what the prefab looks like." },
			};

			var jsonData = new
			{
				model = "gpt-4",
				messages = messages,
				max_tokens = 4000,
				temperature = 0,
				top_p = 0.7,
				frequency_penalty = 0.6,
				presence_penalty = 0.0
			};

			var request = new UnityWebRequest( apiUrl, "POST" );
			string jsonString = JsonConvert.SerializeObject( jsonData );
			byte[] bodyRaw = Encoding.UTF8.GetBytes( jsonString );
			request.uploadHandler = new UploadHandlerRaw( bodyRaw );
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader( "Content-Type", "application/json" );
			request.SetRequestHeader( "Authorization", "Bearer " + apiKey );

			yield return request.SendWebRequest();

			if( request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError )
			{
				Debug.LogError( request.error );
				yield break;
			}
			else
			{
				string jsonResponse = request.downloadHandler.text;
				Debug.Log( "RawJSON: " + jsonResponse );
				SpellCode spellCode = ParseSpellCode( jsonResponse );
				if( spellCode != null )
				{
					Debug.Log( "Code parsed, start executing" );
					StartCoroutine( ExecuteSpellCoroutine( spellCode ) );
					yield break; // Exit the retry loop
				}
				else
				{
					Debug.LogWarning( $"Attempt {attempt} failed. Retrying..." );
				}
			}
		}
		Debug.LogError( "Failed to generate a complete spell script after maximum retries." );
	}

	private SpellCode ParseSpellCode( string jsonResponse )
	{
		try
		{
			Debug.Log( "Raw JSON response: " + jsonResponse );
			var chatResponse = JObject.Parse( jsonResponse );

			// Check if 'choices' and 'message' exist in the response
			if( chatResponse["choices"] != null && chatResponse["choices"].Any() &&
				chatResponse["choices"][0]["message"] != null && chatResponse["choices"][0]["message"]["content"] != null )
			{
				string code = chatResponse["choices"][0]["message"]["content"].ToString();
				if( code.Contains( "public class" ) && code.Contains( "MonoBehaviour" ) )
				{
					int startIndex = code.IndexOf( "using " );
					int endIndex = code.LastIndexOf( "}" );
					if( startIndex >= 0 && endIndex > startIndex )
					{
						code = code.Substring( startIndex, endIndex - startIndex + 1 );
					}

					int openBraces = code.Split( '{' ).Length - 1;
					int closeBraces = code.Split( '}' ).Length - 1;

					while( openBraces > closeBraces )
					{
						code += "}";
						closeBraces++;
						Debug.Log( "Currently at braces: " + closeBraces + " of openbraces: " + openBraces );
					}

					Debug.Log( code );
					return new SpellCode { Code = code };
				}
				else
				{
					Debug.LogError( "Unexpected JSON structure." );
					return null;
				}
			}
			else
			{
				return null;
			}
		}
		catch( Exception e )
		{
			Debug.LogError( "Error parsing JSON response: " + e.Message );
			return null;
		}
	}

	private async Task ExecuteSpellCode( SpellCode spellCode )
	{
		DynamicCodeExecutor executor = new DynamicCodeExecutor();
		Debug.Log( "Executor created, now awaiting execution" );
		bool success = await executor.Execute( spellCode.Code );

		if( success )
		{
			Debug.Log( "Spell executed successfully." );
			player.castingSpellObject.SetActive( false );
			retryAttempt = 0;
			gameManager.ResetTurn();
		}
		else
		{
			Debug.LogError( "Failed to execute spell. trying to fix code" );
			if( retryAttempt <= maxRetries )
			{
				Debug.LogWarning( JsonConvert.SerializeObject( spellCode, Formatting.Indented ) );
				StartCoroutine( FixSpellCode( JsonConvert.SerializeObject( spellCode, Formatting.Indented ) ) );
				retryAttempt++;
			}
		}
	}

	private IEnumerator ExecuteSpellCoroutine( SpellCode spellCode )
	{
		yield return ExecuteSpellCode( spellCode );
	}

	public void StartSpell( Texture2D image )
	{
		Debug.Log( "Start execute Coroutine" );
		player.castingSpellObject.SetActive( true );
		GetSpellCode( image );
	}
}

public class SpellCode
{
	public string Code { get; set; }
}
