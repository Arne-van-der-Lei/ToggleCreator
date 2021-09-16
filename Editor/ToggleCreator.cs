using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDK3.Avatars.ScriptableObjects;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRC.SDK3.Avatars.Components;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

public class ToggleCreator : EditorWindow
{

    public GameObject SelectedAvi;
    public GameObject ObjectToAnimate;
    public Vector2 data;
    public string ToggleName;
    public string LayerName;
    public string BlendShapeName;
    public bool SaveDefaultsOnAvatarload;
    public bool SwitchToBlenShaped;
    public bool fliped;
    public VRCExpressionsMenu MenuToggle;

    [MenuItem("VRChat SDK/Tools/Avatar Toggle creator")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ToggleCreator window = (ToggleCreator)EditorWindow.GetWindow(typeof(ToggleCreator));
        window.Show();
    }

    public void OnGUI()
    {
        bool error = false;
        SelectedAvi = (GameObject)EditorGUILayout.ObjectField(new GUIContent(language.getString("AVATAR.MAIN"), language.getString("AVATAR.DESCRIPTION")), SelectedAvi, typeof(GameObject), true);

        if (SelectedAvi != null)
        {
            VRCAvatarDescriptor desc = SelectedAvi.GetComponentInChildren<VRCAvatarDescriptor>();
            if (desc == null)
            {
                EditorGUILayout.HelpBox(language.getString("AVATAR.ERROR"), MessageType.Warning);
                return;
            }

            VRCExpressionsMenu Menu = desc.expressionsMenu;
            VRCExpressionParameters Param = desc.expressionParameters;

            EditorGUILayout.LabelField(language.getString("MENU.HEADER"), EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(new GUIContent(language.getString("MENU.AVATAR.MENU"),language.getString("MENU.AVATAR.DESCRIPTION")), Menu, typeof(VRCExpressionsMenu), true);
            MenuToggle = (VRCExpressionsMenu)EditorGUILayout.ObjectField(new GUIContent(language.getString("MENU.TOGGLE.MENU"), language.getString("MENU.TOGGLE.DESCRIPTION")), MenuToggle, typeof(VRCExpressionsMenu), true);

            EditorGUILayout.LabelField(language.getString("PARAM.HEADER"), EditorStyles.boldLabel);

            EditorGUILayout.ObjectField(new GUIContent(language.getString("PARAM.TOGGLE.MENU"), language.getString("PARAM.TOGGLE.DESCRIPTION")), Param, typeof(VRCExpressionParameters), true);
            ToggleName = EditorGUILayout.TextField(new GUIContent(language.getString("PARAM.TOGGLE.NAME"), language.getString("PARAM.TOGGLE.NAME.DESCRIPTION")), ToggleName);
            if (ToggleName == "")
            {
                EditorGUILayout.HelpBox(language.getString("PARAM.TOGGLE.ERROR"), MessageType.Warning);
                error = true;
            }

            SaveDefaultsOnAvatarload = EditorGUILayout.Toggle(new GUIContent(language.getString("PARAM.DEFAULT"), language.getString("PARAM.DEFAULT.DESCRIPTION")), SaveDefaultsOnAvatarload);

            EditorGUILayout.LabelField(language.getString("LAYER.HEADER"), EditorStyles.boldLabel);
            LayerName = EditorGUILayout.TextField(new GUIContent(language.getString("LAYER.NAME"), language.getString("LAYER.NAME.DESCRIPTION")), LayerName);
            if (LayerName == "")
            {
                EditorGUILayout.HelpBox(language.getString("LAYER.ERROR"), MessageType.Warning);
                error = true;
            }

            EditorGUILayout.LabelField(language.getString("OBJECT.HEADER"), EditorStyles.boldLabel);

            SwitchToBlenShaped = EditorGUILayout.Toggle(new GUIContent(language.getString("OBJECT.BLEND.TOGGLE"), language.getString("OBJECT.BLEND.TOGGLE.DESCRIPTION")), SwitchToBlenShaped);

            if (!SwitchToBlenShaped)
            {
                ObjectToAnimate = (GameObject)EditorGUILayout.ObjectField(new GUIContent(language.getString("OBJECT.OBJECT"), language.getString("OBJECT.OBJECT.DESCRIPTION")), ObjectToAnimate, typeof(GameObject), true);
                
                if(ObjectToAnimate == null)
                {
                    EditorGUILayout.HelpBox(language.getString("OBJECT.OBJECT.ERROR"), MessageType.Warning);
                    error = true;
                }
            }
            else
            {
                ObjectToAnimate = (GameObject)EditorGUILayout.ObjectField(new GUIContent(language.getString("OBJECT.BLEND"), language.getString("OBJECT.BLEND.DESCRIPTION")), ObjectToAnimate, typeof(GameObject), true);
                BlendShapeName = EditorGUILayout.TextField(new GUIContent(language.getString("OBJECT.BLEND.NAME"), language.getString("OBJECT.BLEND.NAME.DESCRIPTION")), BlendShapeName);
                fliped = EditorGUILayout.Toggle(new GUIContent(language.getString("OBJECT.BLEND.FLIP"), language.getString("OBJECT.BLEND.FLIP.DESCRIPTION")), fliped);
                if (BlendShapeName == "")
                {
                    EditorGUILayout.HelpBox(language.getString("OBJECT.BLEND.ERROR"), MessageType.Warning);
                    error = true;
                }
                if (ObjectToAnimate == null)
                {
                    EditorGUILayout.HelpBox(language.getString("OBJECT.BLEND.OBJECT.ERROR"), MessageType.Warning);
                    error = true;
                }
            }

            if (GUILayout.Button("Add Toggle") && !error)
            {
                EditorUtility.DisplayProgressBar("ToggleCreator", "Creating param", 0.1f);

                /// PARAM
                VRCExpressionParameters.Parameter param = Param.FindParameter(ToggleName);
                if (param == null)
                {
                    VRCExpressionParameters.Parameter[] paramArr = Param.parameters;
                    Param.parameters = new VRCExpressionParameters.Parameter[paramArr.Length + 1];

                    for (int i = 0; i < paramArr.Length; i++)
                    {
                        Param.parameters[i] = paramArr[i];
                    }

                    int val = Param.parameters.Length - 1;
                    Param.parameters[val] = new VRCExpressionParameters.Parameter();
                    Param.parameters[val].defaultValue = 0;
                    Param.parameters[val].name = ToggleName;
                    Param.parameters[val].valueType = VRCExpressionParameters.ValueType.Bool;
                    Param.parameters[val].saved = SaveDefaultsOnAvatarload;
                }
                else
                {
                    param.defaultValue = 0;
                    param.valueType = VRCExpressionParameters.ValueType.Bool;
                    param.saved = SaveDefaultsOnAvatarload;
                }

                EditorUtility.DisplayProgressBar("ToggleCreator", "Creating animations", 0.2f);

                /// menu
                VRCExpressionsMenu.Control control = MenuToggle.controls.Find((x) => x.name == ToggleName );
                if(control == null)
                {
                    control = new VRCExpressionsMenu.Control();
                    MenuToggle.controls.Add(control);
                }

                control.name = ToggleName;
                control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                control.parameter = new VRCExpressionsMenu.Control.Parameter();
                control.parameter.name = ToggleName;

                /// Animator
                /// Folders
                if (!AssetDatabase.IsValidFolder("Assets/ToggleAnimations"))
                    AssetDatabase.CreateFolder("Assets", "ToggleAnimations");

                string assetPathHeader = "Assets/ToggleAnimations/" + SelectedAvi.name;

                if (!AssetDatabase.IsValidFolder(assetPathHeader))
                    AssetDatabase.CreateFolder("Assets/ToggleAnimations", SelectedAvi.name);

                assetPathHeader += "/";

                ///clips
                AnimationClip ClipOff = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPathHeader + LayerName + "Off");
                if (ClipOff == null)
                {
                    ClipOff = new AnimationClip();
                    ClipOff.name = LayerName + "Off";
                    AssetDatabase.CreateAsset(ClipOff, assetPathHeader + ClipOff.name + ".anim");
                }

                AnimationClip ClipOn = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPathHeader + LayerName + "On");
                if (ClipOn == null)
                {
                    ClipOn = new AnimationClip();
                    ClipOn.name = LayerName + "On";
                    AssetDatabase.CreateAsset(ClipOn, assetPathHeader + ClipOn.name + ".anim");
                }

                if (!SwitchToBlenShaped)
                {
                    ClipOff.SetCurve(GetAnimationNameFor(ObjectToAnimate, SelectedAvi), typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe[] { new Keyframe(0, 0) }));
                    ClipOn.SetCurve(GetAnimationNameFor(ObjectToAnimate, SelectedAvi), typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe[] { new Keyframe(0, 1) }));
                }
                else
                {
                    ClipOff.SetCurve(GetAnimationNameFor(ObjectToAnimate, SelectedAvi), typeof(SkinnedMeshRenderer), "blendShape." + BlendShapeName, new AnimationCurve(new Keyframe[] { new Keyframe(0, fliped ? 100 : 0) }));
                    ClipOn.SetCurve(GetAnimationNameFor(ObjectToAnimate, SelectedAvi), typeof(SkinnedMeshRenderer), "blendShape." + BlendShapeName, new AnimationCurve(new Keyframe[] { new Keyframe(0, fliped ? 0 : 100) }));
                }

                EditorUtility.DisplayProgressBar("ToggleCreator", "Creating controller", 0.3f);
                /// Controller
                CustomAnimLayer[] layers = desc.baseAnimationLayers;

                AnimatorController controller;
                if (layers[4].animatorController == null)
                {
                    controller = AnimatorController.CreateAnimatorControllerAtPath(assetPathHeader + "FXController.controller");
                    layers[4].animatorController = controller;
                    layers[4].isDefault = false;
                }
                else
                {
                    controller = (AnimatorController)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(layers[4].animatorController), typeof(AnimatorController));
                }

                EditorUtility.DisplayProgressBar("ToggleCreator", "Creating Layer", 0.4f);
                /// Layer
                AnimatorControllerLayer layer = null;
                if (controller.layers.Length != 0)
                {
                    layer = controller.layers.GetFirst((x) => x.name == LayerName);
                }

                if (layer == null)
                {
                    layer = new AnimatorControllerLayer();
                }

                layer.name = LayerName;
                layer.stateMachine = new AnimatorStateMachine();
                layer.defaultWeight = 1;
                
                EditorUtility.DisplayProgressBar("ToggleCreator", "Creating Animation states", 0.5f);
                AnimatorStateMachine stateMachine = new AnimatorStateMachine();
                stateMachine.name = LayerName;
                AssetDatabase.AddObjectToAsset(stateMachine, controller);
                stateMachine.hideFlags = HideFlags.HideInHierarchy;

                /// Animation States
                AnimatorState OnState = stateMachine.AddState(ClipOn.name);
                AnimatorState OffState = stateMachine.AddState(ClipOff.name);

                AnimatorStateTransition TransitionOnOff = OnState.AddTransition(OffState);
                TransitionOnOff.AddCondition(AnimatorConditionMode.IfNot, 0, ToggleName);
                TransitionOnOff.name = "OnOff";

                AnimatorStateTransition TransitionOffOn = OffState.AddTransition(OnState);
                TransitionOffOn.AddCondition(AnimatorConditionMode.If, 0, ToggleName);
                TransitionOffOn.name = "OffOn";

                OnState.motion = ClipOn;
                OffState.motion = ClipOff;

                stateMachine.defaultState = OffState;

                layer.stateMachine = stateMachine;
                int index = controller.layers.Find((x) => x.name == LayerName);
                if(index != -1)
                    controller.RemoveLayer(index);

                controller.AddLayer(layer);

                AnimatorControllerParameter parameter = null;
                if (controller.parameters.Length != 0)
                {
                    parameter = controller.parameters.GetFirst((x) => x.name == ToggleName );

                }

                if (parameter == null)
                    controller.AddParameter(ToggleName, AnimatorControllerParameterType.Bool);
                else
                {
                    parameter.name = ToggleName;
                    parameter.type = AnimatorControllerParameterType.Bool;
                }

                EditorUtility.DisplayProgressBar("ToggleCreator", "Saving", 0.6f);
                EditorUtility.SetDirty(controller);


                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();
            }
        }
    }


    public void ShowProp(SerializedProperty prop)
    {
        if (prop.Next(true))
        {
            do
            {
                SerializedProperty property = prop;
                if (property != null)
                {
                    if (property.isArray)
                    {
                        for(int i = 0; i < property.arraySize; i++)
                        {
                            SerializedProperty propertys = property.GetArrayElementAtIndex(i);
                            ShowProp(propertys);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(property.propertyPath);
                    }
                }
            }
            while (prop.Next(true));
        }
    }

    public string GetAnimationNameFor(GameObject from, GameObject to)
    {
        if (from.transform.parent.name != to.name)
        {
            return GetAnimationNameFor(from.transform.parent.gameObject, to) + "/" + from.name ;
        }
        return from.name;
    }
}
