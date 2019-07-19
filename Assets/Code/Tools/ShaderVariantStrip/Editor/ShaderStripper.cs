using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

class VariantStripper : IPreprocessShaders
{
	// returns true if the variant should be stripped.
	delegate bool VariantStrippingFunc(Shader shader, ShaderSnippetData snippet, ShaderCompilerData inputData);
	Dictionary<string, VariantStrippingFunc> m_StripperFuncs;

	List<ShaderKeyword> m_keywords = new List<ShaderKeyword>();
	ShaderKeyword m_KeywordNormalMap;
	ShaderKeyword m_KeywordDetailNormalMap1;
	ShaderKeyword m_KeywordDetailNormalMap2;
	ShaderKeyword m_KeywordDetailNormalMap3;
	ShaderKeyword m_KeywordDetailNormalMap4;
	ShaderKeyword m_KeywordTerrainNormalMap;
	ShaderKeyword m_KeywordSpecGloss;
	ShaderKeyword m_KeywordHDR;
	ShaderKeyword m_KeywordLightmap;
	ShaderKeyword m_KeywordScreenShadow;

	public VariantStripper()
	{
		m_StripperFuncs = new Dictionary<string, VariantStrippingFunc>();
		m_StripperFuncs.Add("CY/Standard(Custom)", StandardShaderStripper);
		m_StripperFuncs.Add("CY/Standard (Specular setup)(Custom)", StandardShaderStripper);
		m_StripperFuncs.Add("Standard (Specular setup)", StandardShaderStripper);
		m_StripperFuncs.Add("Standard", StandardShaderStripper);
		m_StripperFuncs.Add("LDJ/Standard", LDJStandardShaderStripper);
        //m_StripperFuncs.Add("LDJ/Role/PBRHair_Light", StandardShaderStripper);
        m_StripperFuncs.Add("LDJ/Role/PBRBodyLight", LDJRoleStandardShaderStripper);
        //m_StripperFuncs.Add("LDJ/Role/PBRBody_Face_Light", StandardShaderStripper);
        //m_StripperFuncs.Add("LDJ/Avatar/PBRBody_Face_Light", StandardShaderStripper);
        //m_StripperFuncs.Add("LDJ/Role/PBREye_Light", LDJRoleStandardShaderStripper);



        m_StripperFuncs.Add("Nature/Terrain/Standard", TerrainShaderStripper);
		m_StripperFuncs.Add("Hidden/TerrainEngine/Splatmap/Standard-AddPass", TerrainShaderStripper);
		m_StripperFuncs.Add("Hidden/TerrainEngine/Splatmap/Standard-Base", TerrainShaderStripper);
		m_StripperFuncs.Add("Nature/Terrain/Specular", TerrainShaderStripper);
		m_StripperFuncs.Add("Hidden/TerrainEngine/Splatmap/Specular-Base", TerrainShaderStripper);
		m_StripperFuncs.Add("Hidden/TerrainEngine/Splatmap/Specular-AddPass", TerrainShaderStripper);
		m_StripperFuncs.Add("Nature/Terrain/Diffuse", TerrainShaderStripper);
		m_StripperFuncs.Add("Hidden/TerrainEngine/Splatmap/Diffuse-AddPass", TerrainShaderStripper);

		m_KeywordNormalMap = new ShaderKeyword("_NORMALMAP");
		m_KeywordTerrainNormalMap = new ShaderKeyword("_TERRAIN_NORMAL_MAP");
		m_KeywordDetailNormalMap1 = new ShaderKeyword("_DETAIL_MULX2");
		m_KeywordDetailNormalMap2 = new ShaderKeyword("_DETAIL_MUL");
		m_KeywordDetailNormalMap3 = new ShaderKeyword("_DETAIL_ADD");
		m_KeywordDetailNormalMap4 = new ShaderKeyword("_DETAIL_LERP");
		m_KeywordSpecGloss = new ShaderKeyword("_SPECGLOSSMAP");
		m_KeywordHDR = new ShaderKeyword("UNITY_HDR_ON");
		m_KeywordLightmap = new ShaderKeyword("LIGHTMAP_ON");
		m_KeywordScreenShadow = new ShaderKeyword("SHADOWS_SCREEN");

		// 以下是假设Standard shader不会用到的功能，就可以直接简化掉变种，具体哪些内置宏有用，哪些没用，要仔细确认
		m_keywords.Add(new ShaderKeyword("DEBUG"));
		// 假设没有用到GpuInstancing
		m_keywords.Add(new ShaderKeyword("INSTANCING_ON"));
		// 假设没有用到内置雾
		m_keywords.Add(new ShaderKeyword("FOG_LINEAR"));
		m_keywords.Add(new ShaderKeyword("FOG_EXP"));
		m_keywords.Add(new ShaderKeyword("FOG_EXP2"));
		// 假设没有用到RealtimeGI和Directional lightmap
		m_keywords.Add(new ShaderKeyword("DYNAMICLIGHTMAP_ON"));
		m_keywords.Add(new ShaderKeyword("DIRLIGHTMAP_COMBINED"));
		// 假设没有用到LightProbes
		//m_keywords.Add(new ShaderKeyword("LIGHTPROBE_SH"));
		// 假设没有用到软阴影
		m_keywords.Add(new ShaderKeyword("SHADOWS_SOFT")); 
		// 假设没有用到AlphaTest和AlphaBlend
		//m_keywords.Add(new ShaderKeyword("_ALPHABLEND_ON"));
		//m_keywords.Add(new ShaderKeyword("_ALPHATEST_ON"));
		m_keywords.Add(new ShaderKeyword("_ALPHAPREMULTIPLY_ON"));

		m_keywords.Add(new ShaderKeyword("VERTEXLIGHT_ON"));
		m_keywords.Add(new ShaderKeyword("SHADOWS_DEPTH"));
		m_keywords.Add(new ShaderKeyword("SHADOWS_CUBE"));
		m_keywords.Add(new ShaderKeyword("DIRECTIONAL_COOKIE"));
		m_keywords.Add(new ShaderKeyword("POINT_COOKIE"));
		m_keywords.Add(new ShaderKeyword("SPOT"));
		m_keywords.Add(new ShaderKeyword("EDITOR_VISUALIZATION"));
	}

    bool LDJStandardShaderStripper(Shader shader, ShaderSnippetData snippet, ShaderCompilerData inputData)
    {
        List<ShaderKeyword> m_keywords = new List<ShaderKeyword>();
        //m_keywords.Add(new ShaderKeyword("SHADOWS_SCREEN"));
        // 假设没有用到内置雾
        m_keywords.Add(new ShaderKeyword("FOG_LINEAR"));
        m_keywords.Add(new ShaderKeyword("FOG_EXP"));
        m_keywords.Add(new ShaderKeyword("FOG_EXP2"));
        // 假设没有用到RealtimeGI和Directional lightmap
        m_keywords.Add(new ShaderKeyword("DYNAMICLIGHTMAP_ON"));
        m_keywords.Add(new ShaderKeyword("DIRLIGHTMAP_COMBINED"));
        // 假设没有用到LightProbes
        //m_keywords.Add(new ShaderKeyword("LIGHTPROBE_SH"));
        // 假设没有用到软阴影
        m_keywords.Add(new ShaderKeyword("SHADOWS_SOFT"));
        m_keywords.Add(new ShaderKeyword("VERTEXLIGHT_ON"));
        m_keywords.Add(new ShaderKeyword("SHADOWS_DEPTH"));
        m_keywords.Add(new ShaderKeyword("SHADOWS_CUBE"));
        m_keywords.Add(new ShaderKeyword("DIRECTIONAL_COOKIE"));
        m_keywords.Add(new ShaderKeyword("POINT_COOKIE"));
        m_keywords.Add(new ShaderKeyword("SPOT"));
        m_keywords.Add(new ShaderKeyword("EDITOR_VISUALIZATION"));

        for (int i = 0; i < m_keywords.Count; ++i)
        {
            if (inputData.shaderKeywordSet.IsEnabled(m_keywords[i]))
            {
                return true;
            }
        }

        List<ShaderKeyword> m_keywordsR = new List<ShaderKeyword>();
        m_keywordsR.Add(new ShaderKeyword("_METALLICGLOSSMAP"));
        m_keywordsR.Add(new ShaderKeyword("_NORMALMAP"));
    
        for (int i = 0; i < m_keywordsR.Count; ++i)
        {
            if (!inputData.shaderKeywordSet.IsEnabled(m_keywordsR[i]))
            {
                return true;
            }
        }
        
        return false;
    }


    bool LDJRoleStandardShaderStripper(Shader shader, ShaderSnippetData snippet, ShaderCompilerData inputData)
    {
        List<ShaderKeyword> m_keywords = new List<ShaderKeyword>();
        m_keywords.Add(new ShaderKeyword("SHADOWS_SCREEN"));
        m_keywords.Add(new ShaderKeyword("DYNAMICLIGHTMAP_ON"));
        m_keywords.Add(new ShaderKeyword("DIRLIGHTMAP_COMBINED"));
        m_keywords.Add(new ShaderKeyword("LIGHTMAP_SHADOW_MIXING"));
        m_keywords.Add(new ShaderKeyword("VERTEXLIGHT_ON"));
        m_keywords.Add(new ShaderKeyword("LIGHTMAP_ON"));
        m_keywords.Add(new ShaderKeyword("SHADOWS_SHADOWMASK"));

        for (int i = 0; i < m_keywords.Count; ++i)
        {
            if (inputData.shaderKeywordSet.IsEnabled(m_keywords[i]))
            {
                return true;
            }
        }

        if (inputData.shaderKeywordSet.GetShaderKeywords().Length > 4)
        {
            return true;
        }
        if(!inputData.shaderKeywordSet.IsEnabled(new ShaderKeyword("DIRECTIONAL")) || !inputData.shaderKeywordSet.IsEnabled(new ShaderKeyword("LIGHTPROBE_SH")))
            {
                return true;
            }

        return false;
    }

    // 以下是对Standard shader的变种简化，项目中其它有大量变种的shader也应该做具体处理
    bool StandardShaderStripper(Shader shader, ShaderSnippetData snippet, ShaderCompilerData inputData)
	{
		for (int i = 0; i < m_keywords.Count; ++i)
		{
			if (inputData.shaderKeywordSet.IsEnabled(m_keywords[i]))
			{
				return true;
			}
		}

		// 假设项目是固定前向渲染，那么延迟渲染pass可以去掉
		if (snippet.passType == PassType.Deferred)
		{
			return true;
		}

		// 假设项目拟定的设备渲染分级是只有Tier3用法线图，那么Tier1和2可以去掉
		if (inputData.graphicsTier != GraphicsTier.Tier3 && inputData.shaderKeywordSet.IsEnabled(m_KeywordNormalMap))
		{
			//return true;
		}

		///////////////////////////////////////////
		/// 根据在GraphicsSettings设置来简化变种
		if (!inputData.platformKeywordSet.IsEnabled(BuiltinShaderDefine.UNITY_USE_NATIVE_HDR) && inputData.shaderKeywordSet.IsEnabled(m_KeywordHDR))
		{
			return true;
		}
		if (!inputData.platformKeywordSet.IsEnabled(BuiltinShaderDefine.UNITY_ENABLE_DETAIL_NORMALMAP))
		{
			if (inputData.shaderKeywordSet.IsEnabled(m_KeywordDetailNormalMap1) ||
				inputData.shaderKeywordSet.IsEnabled(m_KeywordDetailNormalMap2) ||
				inputData.shaderKeywordSet.IsEnabled(m_KeywordDetailNormalMap3) ||
				inputData.shaderKeywordSet.IsEnabled(m_KeywordDetailNormalMap4) )
			{
				return true;
			}
		}
		if (inputData.platformKeywordSet.IsEnabled(BuiltinShaderDefine.UNITY_NO_SCREENSPACE_SHADOWS) && inputData.shaderKeywordSet.IsEnabled(m_KeywordScreenShadow))
		{
			// No cascade shadow map
			return true;
		}


		// 即使是开启了高光图，因为我们确定其在VertexShader中不会有相关处理，所以可以去掉VS的变种
		// 类似这种ShaderStage的stripping应该有不少
		if (snippet.shaderType == ShaderType.Vertex && inputData.shaderKeywordSet.IsEnabled(m_KeywordSpecGloss))
		{
			return true;
		}

		return false;
	}

	bool TerrainShaderStripper(Shader shader, ShaderSnippetData snippet, ShaderCompilerData inputData)
	{
		for (int i = 0; i < m_keywords.Count; ++i)
		{
			if (inputData.shaderKeywordSet.IsEnabled(m_keywords[i]))
			{
				return true;
			}
		}

		// 假设项目是固定前向渲染，那么延迟渲染pass可以去掉
		if (snippet.passType == PassType.Deferred)
		{
			return true;
		}

		// 假设项目拟定的设备渲染分级是只有Tier3用法线图，那么Tier1和2可以去掉
		if (inputData.graphicsTier != GraphicsTier.Tier3 && inputData.shaderKeywordSet.IsEnabled(m_KeywordTerrainNormalMap))
		{
			return true;
		}

		if (!inputData.platformKeywordSet.IsEnabled(BuiltinShaderDefine.UNITY_USE_NATIVE_HDR) && inputData.shaderKeywordSet.IsEnabled(m_KeywordHDR))
		{
			return true;
		}

		if (inputData.platformKeywordSet.IsEnabled(BuiltinShaderDefine.UNITY_NO_SCREENSPACE_SHADOWS) && inputData.shaderKeywordSet.IsEnabled(m_KeywordScreenShadow))
		{
			// No cascade shadow map
			return true;
		}

		return false;
	}

	public int callbackOrder { get { return 0; } }

	public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> inputData)
	{
		// Do we have a shader variant stripper function for this shader?
		VariantStrippingFunc stripperFunc = null;
		m_StripperFuncs.TryGetValue(shader.name, out stripperFunc);
		if (stripperFunc == null || inputData.Count==0)
			return;
		int inputShaderVariantCount = inputData.Count;
		ShaderCompilerData workaround = inputData[0];
		for (int i = 0; i < inputData.Count; ++i)
		{
			ShaderCompilerData input = inputData[i];
			if (stripperFunc(shader, snippet, input))
			{
				inputData.RemoveAt(i);
				i--;
			}
		}
		// Currently if a certain snippet is completely stripped (for example if you remove a whole pass) other passes might get broken
		// To work around that, we make sure that we always have at least one variant.
		if (inputData.Count == 0)
			inputData.Add(workaround);
	}
}