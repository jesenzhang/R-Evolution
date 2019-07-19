
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace ShaderVariantsStripper
{
    public class ShaderVariantsStripperPackage : IPreprocessShaders
    {
        public int callbackOrder => (int)ShaderVariantsStripperOrder.Package;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
        }
    }
}