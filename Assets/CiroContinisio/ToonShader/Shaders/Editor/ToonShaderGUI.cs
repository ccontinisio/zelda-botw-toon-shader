using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class ToonShaderGUI : ShaderGUI 
{
    private enum ShadingStyle
    {
        SkinAndTextiles,
        HairAndMetal,
    }
    private ShadingStyle _shadingStyle;

    private bool _toonPropsVisible = true;
    private bool _emission;

    private const string SkinKeyword = "_SHADING_STYLE_SKIN_AND_TEXTILES";
    private const string HairKeyword = "_SHADING_STYLE_HAIR_AND_METAL";

    public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMaterial = materialEditor.target as Material;

        //======= Base Properties
        
        GUILayout.Label("Base properties", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        materialEditor.TextureProperty(FindProperty("_BaseMap", properties), "Base map");
        materialEditor.ColorProperty(FindProperty("_BaseColor", properties), "Tint");
        materialEditor.TextureProperty(FindProperty("_NormalMap", properties), "Normal map");
        
        //Emission property
        bool emissionPreviousValue = _emission;
        _emission = DrawEmissionToggle(materialEditor, FindProperty("_UseEmission", properties));
        if (_emission)
        {
            EditorGUI.indentLevel++;
            materialEditor.TextureProperty(FindProperty("_EmissionMap", properties), "Emission map");
            materialEditor.ColorProperty(FindProperty("_EmissionColor", properties), "Emission tint");
            EditorGUI.indentLevel--;
            if (emissionPreviousValue != _emission)
            {
                targetMaterial.EnableKeyword("_EMISSION");
                targetMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
        }
        else
        {
            if (emissionPreviousValue != _emission)
            {
                targetMaterial.DisableKeyword("_EMISSION");
                targetMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        materialEditor.RangeProperty(FindProperty("_Smoothness", properties), "Smoothness");
        materialEditor.ColorProperty(FindProperty("_SpecularColor", properties), "Specular color");
        
        EditorGUI.indentLevel--;
        
        //======= Shading style
        
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Shading style", EditorStyles.boldLabel);
        EditorGUILayout.Space(-3f);
        EditorGUI.indentLevel++;
        
        string[] options = {"Skin or Textiles", "Hair or Metal"};
        _shadingStyle = (ShadingStyle)materialEditor.PopupShaderProperty(FindProperty("_SHADING_STYLE", properties), new GUIContent("Material"), options);
        
        switch (_shadingStyle)
        {
            case ShadingStyle.SkinAndTextiles:
                materialEditor.RangeProperty(FindProperty("_PaintbrushStrokesSize", properties), "Paintbrush strokes size");
                materialEditor.RangeProperty(FindProperty("_PaintbrushStrokesAngle", properties), "Paintbrush strokes angle");
                break;
            case ShadingStyle.HairAndMetal:
                materialEditor.RangeProperty(FindProperty("_SpecularNoiseSize", properties), "Specular noise size");
                materialEditor.RangeProperty(FindProperty("_ExtraBandThickness", properties), "Extra band thickness");
                break;
        }
        
        targetMaterial.EnableKeyword(_shadingStyle == ShadingStyle.SkinAndTextiles ? SkinKeyword : HairKeyword);
        targetMaterial.DisableKeyword(_shadingStyle == ShadingStyle.SkinAndTextiles ? HairKeyword : SkinKeyword);
        
        EditorGUI.indentLevel--;
        
        //======= Toon Properties
        
        EditorGUILayout.Space(4f);
        _toonPropsVisible = EditorGUILayout.Foldout(_toonPropsVisible, "Toon properties", EditorStyles.foldoutHeader);
        if (_toonPropsVisible)
        {
            EditorGUI.indentLevel++;
            materialEditor.RangeProperty(FindProperty("_ToonCutoffPoint", properties), "Toon cutoff");
            materialEditor.RangeProperty(FindProperty("_AdditionalLightCutoffPoint", properties), "Additional light cutoff");
            materialEditor.RangeProperty(FindProperty("_ShadowsFloor", properties), "Shadows floor value");
            materialEditor.RangeProperty(FindProperty("_StrongRimStrength", properties), "Strong rim strength");
            EditorGUI.indentLevel--;
        }
    }

    private bool DrawEmissionToggle(MaterialEditor materialEditor, MaterialProperty prop)
    {
        bool value = (prop.floatValue != 0.0f);

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        // Show the toggle control
        value = EditorGUILayout.Toggle("Emission", value);

        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = value ? 1.0f : 0.0f;
        }

        return value && !prop.hasMixedValue;
    }
}