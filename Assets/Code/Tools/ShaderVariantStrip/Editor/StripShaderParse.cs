using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//解析shader文件
public class StripShaderParse
{
    const string PRAGMA_COMMENT_MARK = "// Edited by Shader Control: ";
    const string PRAGMA_DISABLED_MARK = "// Disabled by Shader Control: ";

    //全局关键字
    const string PRAGMA_MULTICOMPILE = "#pragma multi_compile ";
    const string PRAGMA_SHADER_FEATURE = "#pragma shader_feature ";
    //本地关键字
    const string PRAGMA_MULTI_COMPILE_LOCAL = "#pragma multi_compile_local ";
    const string PRAGMA_SHADER_FEATURE_LOCAL = "#pragma shader_feature_local ";
    //keyword缩写
    const string PRAGMA_MULTI_COMPILE_FWDBASE = "#pragma multi_compile_fwdbase ";
    const string PRAGMA_MULTI_COMPILE_FWDBASE_FULLSHADOWS = "#pragma multi_compile_fwdbase_fullshadows ";
    const string PRAGMA_MULTI_COMPILE_FWDADD = "#pragma multi_compile_fwdadd  ";
    const string PRAGMA_MULTI_COMPILE_FOG = "#pragma multi_compile_fog ";
    //跳过keyword
    const string PRAGMA_SKIP_VARIANTS = "#pragma skip_variants ";
    //渲染平台
    const string PRAGMA_ONLY_RENDERERS = "#pragma only_renderers ";
    const string PRAGMA_EXCLUDE_RENDERERS = "#pragma exclude_renderers ";
    
    const string BACKUP_SUFFIX = "_backup";
    const string PRAGMA_UNDERSCORE = "__ ";
 
    public static void ScanShader (ref StripShader shader)
	{
		//重置shader
		shader.passes.Clear ();
		shader.keywords.Clear ();
		shader.hasBackup = File.Exists (shader.path + BACKUP_SUFFIX);
		shader.pendingChanges = false;
		shader.editedByShaderControl = false;

		//读取shader文本
		string[] shaderLines = File.ReadAllLines (shader.path);
		string[] separator = new string[] { " " };
        StripShaderPass currentPass = new StripShaderPass();
        StripShaderPass basePass = null;
		int pragmaControl = 0;
		int pass = -1;
		bool blockComment = false;
        StripKeywordLine keywordLine = new StripKeywordLine();
		for (int k = 0; k < shaderLines.Length; k++) {
			string line = shaderLines [k].Trim ();
			if (line.Length == 0)
				continue;

			int lineCommentIndex = line.IndexOf ("//");
			int blocCommentIndex = line.IndexOf ("/*");
			int endCommentIndex = line.IndexOf ("*/");
			if (blocCommentIndex > 0 && (lineCommentIndex > blocCommentIndex || lineCommentIndex < 0)) {
				blockComment = true;
			}
			if (endCommentIndex > blocCommentIndex && (lineCommentIndex > endCommentIndex || lineCommentIndex < 0)) {
				blockComment = false;
			}
			if (blockComment)
				continue;
				
			string lineUPPER = line.ToUpper ();
			if (lineUPPER.Equals ("PASS") || lineUPPER.StartsWith ("PASS ")) {
				if (pass >= 0) {
					currentPass.pass = pass;
					if (basePass != null)
						currentPass.Add (basePass.keywordLines);
					shader.Add (currentPass);
				} else if (currentPass.keywordCount > 0) {
					basePass = currentPass;
				}
				currentPass = new StripShaderPass();
				pass++;
				continue;
			}
			int j = line.IndexOf (PRAGMA_COMMENT_MARK);
			if (j >= 0) {
				pragmaControl = 1;
			} else {
				j = line.IndexOf (PRAGMA_DISABLED_MARK);
				if (j >= 0)
					pragmaControl = 3;
			}
			if (lineCommentIndex == 0 && pragmaControl != 1 && pragmaControl != 3) {
				continue; // 跳过注释
			}

            //添加 multicompile
			bool isShaderFeature = false;
			j = line.IndexOf (PRAGMA_MULTICOMPILE);
			if (j < 0) {
				j = line.IndexOf (PRAGMA_SHADER_FEATURE);
				isShaderFeature = (j >= 0);
			}
			if (j >= 0) {
				if (pragmaControl != 2) {
					keywordLine = new StripKeywordLine();
				}
				keywordLine.isFeature = isShaderFeature;
				// 处理在#pragram行中潜在的注释
				int lastStringPos = line.IndexOf ("//", j + 22);
				if (lastStringPos < 0) {
					lastStringPos = line.Length;
				}
				int length = lastStringPos - j - 22;
				string[] kk = line.Substring (j + 22, length).Split (separator, StringSplitOptions.RemoveEmptyEntries);
				// 清理关键字 trim
				for (int i = 0; i < kk.Length; i++)
					kk [i] = kk [i].Trim ();
				// 操作关键字
				switch (pragmaControl) {
				case 1: // 如果是被编辑器修改的
					shader.editedByShaderControl = true;
                        // 在这行添加原始关键字
					for (int s = 0; s < kk.Length; s++) {
						keywordLine.Add (shader.GetKeyword (kk [s]));
					}
					pragmaControl = 2;
					break;
				case 2:
                        // 检查使用的关键字
					keywordLine.DisableKeywords ();
					for (int s = 0; s < kk.Length; s++) {
                        StripKeyword keyword = keywordLine.GetKeyword (kk [s]);
						if (keyword != null)
							keyword.enabled = true;
					}
					currentPass.Add (keywordLine);
					pragmaControl = 0;
					break;
				case 3: // 被编辑器关闭的关键字
					shader.editedByShaderControl = true;
                        // // 在这行添加原始的关键字
                        for (int s = 0; s < kk.Length; s++) {
                        StripKeyword keyword = shader.GetKeyword (kk [s]);
						keyword.enabled = false;
						keywordLine.Add (keyword);
					}
					currentPass.Add (keywordLine);
					pragmaControl = 0;
					break;
				case 0:
                        // 在这行添加关键字
                        for (int s = 0; s < kk.Length; s++) {
						keywordLine.Add (shader.GetKeyword (kk [s]));
					}
					currentPass.Add (keywordLine);
					break;
				}
			}

            //PRAGMA_MULTI_COMPILE_FWDBASE
            j = line.IndexOf(PRAGMA_MULTI_COMPILE_FWDBASE_FULLSHADOWS);
            if (j >= 0)
            {
                
            }
        }
		currentPass.pass = Mathf.Max (pass, 0);
		if (basePass != null)
			currentPass.Add (basePass.keywordLines);
		shader.Add (currentPass);
		shader.UpdateVariantCount ();
	}

}
