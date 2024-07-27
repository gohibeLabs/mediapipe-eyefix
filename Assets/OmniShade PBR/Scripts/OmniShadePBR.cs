//------------------------------------
//           OmniShade PBR
//     Copyright© 2023 OmniShade     
//------------------------------------

#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * This class contains shader constants and utility functions.
 **/
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class OmniShadePBR {
	public const string NAME = "OmniShade PBR";
	public const string DOCS_URL = "https://pbr.omnishade.io";
	
#if UNITY_EDITOR
	const string SHADER_VARIANT_LIMIT_KEY = "UnityEditor.ShaderGraph.VariantLimit";
	const int SHADER_VARIANT_LIMIT = 100000;

	// Increase shader variant limit in Editor Prefs
	static OmniShadePBR() {
		int limit = EditorPrefs.GetInt(SHADER_VARIANT_LIMIT_KEY);
		if (limit < SHADER_VARIANT_LIMIT)
			EditorPrefs.SetInt(SHADER_VARIANT_LIMIT_KEY, SHADER_VARIANT_LIMIT);
	}
#endif
}
