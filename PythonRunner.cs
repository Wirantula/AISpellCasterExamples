using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using System.Collections;
using System.IO.Compression;

public static class PythonRunner
{
	public static string GetPythonPath()
	{
#if UNITY_STANDALONE_WIN
		return Path.Combine( Application.persistentDataPath, "venv", "Scripts", "python.exe" );
#else
        return "python";
#endif
	}

	public static void Setup()
	{
		string venvPath = Path.Combine( Application.persistentDataPath, "venv" );
		if( !Directory.Exists( venvPath ) )
		{
			string zipPath = Path.Combine( Application.streamingAssetsPath, "venv.zip" );
			if( File.Exists( zipPath ) )
			{
				// Extract the zip file
				ZipFile.ExtractToDirectory( zipPath, Application.persistentDataPath );
				UnityEngine.Debug.Log( "Virtual environment extracted." );
			}
			else
			{
				UnityEngine.Debug.LogError( "venv.zip not found in StreamingAssets folder." );
			}
		}
	}

	public static IEnumerator RunPythonScriptAsync( string scriptPath, string imageData, Action<string> onProgressUpdate, Action<string> onComplete )
	{
		string tempImagePath = Path.Combine( Application.persistentDataPath, "tempImageData.txt" );

		// Write the image data to a temporary file
		File.WriteAllText( tempImagePath, imageData );

		string pythonExePath = GetPythonPath(); // Use the correct python path
		ProcessStartInfo start = new ProcessStartInfo
		{
			FileName = pythonExePath,
			Arguments = $"\"{scriptPath}\" \"{tempImagePath}\"",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		StringBuilder result = new StringBuilder();
		using( Process process = Process.Start( start ) )
		{
			using( StreamReader reader = process.StandardOutput )
			{
				string line;
				while( ( line = reader.ReadLine() ) != null )
				{
					onProgressUpdate?.Invoke( line );
					result.AppendLine( line );
				}
			}

			// Capture any errors
			using( StreamReader errorReader = process.StandardError )
			{
				string error = errorReader.ReadToEnd();
				if( !string.IsNullOrEmpty( error ) )
				{
					onProgressUpdate?.Invoke( "Error: " + error );
					result.AppendLine( "Error: " + error );
				}
			}
		}

		onComplete?.Invoke( result.ToString().Trim() );
		yield return null;
	}
}
