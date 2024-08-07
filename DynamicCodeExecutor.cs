using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DynamicCodeExecutor
{
	public Task<bool> Execute( string code )
	{
		return Task.Run( () =>
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			// Optimize the script options
			var options = ScriptOptions.Default
				.WithReferences( AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.IsDynamic && !string.IsNullOrWhiteSpace( a.Location ) ) )
				.WithImports( "System", "System.Linq", "System.Collections.Generic", "UnityEngine" );

			try
			{
				// Compile the script into a C# assembly
				var script = CSharpScript.Create( code, options );
				var compilation = script.GetCompilation();
				using( var ms = new System.IO.MemoryStream() )
				{
					var result = compilation.Emit( ms );
					if( !result.Success )
					{
						Debug.LogError( $"Code compilation failed: {string.Join( "\n", result.Diagnostics )}" );
						SpellManager.caughtError = $"Code compilation failed: {string.Join( "\n", result.Diagnostics )}";
						return false;
					}

					ms.Seek( 0, System.IO.SeekOrigin.Begin );
					var assembly = System.Reflection.Assembly.Load( ms.ToArray() );

					// Find the first type that derives from MonoBehaviour
					var monoBehaviourType = assembly.GetTypes().FirstOrDefault( t => t.IsSubclassOf( typeof( MonoBehaviour ) ) );
					if( monoBehaviourType == null )
					{
						Debug.LogError( "Failed to find a MonoBehaviour type in the generated code." );
						return false;
					}

					// Enqueue the GameObject creation on the main thread
					UnityMainThreadDispatcher.Enqueue( () =>
					{
						GameObject spellObject = new GameObject( "GeneratedSpell" );
						spellObject.AddComponent( monoBehaviourType );
						Debug.Log( "Code execution completed successfully." );
					} );

					return true;
				}
			}
			catch( CompilationErrorException e )
			{
				Debug.LogError( $"Code compilation failed: {string.Join( "\n", e.Diagnostics )}" );
				return false;
			}
			catch( Exception e )
			{
				Debug.LogError( $"Runtime error: {e.Message}" );
				return false;
			}
			finally
			{
				stopwatch.Stop();
				Debug.Log( $"Total execution time: {stopwatch.ElapsedMilliseconds} ms" );
			}
		} );
	}
}
