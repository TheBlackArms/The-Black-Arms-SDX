using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(VRCSDK2.VRC_AvatarDescriptor))]
public class AvatarDescriptorEditor : Editor
{
    VRCSDK2.VRC_AvatarDescriptor avatarDescriptor;
    VRC.Core.PipelineManager pipelineManager;

    SkinnedMeshRenderer selectedMesh;
    List<string> blendShapeNames = null;
    List<string> visemeNames = null;

    public override void OnInspectorGUI()
    {
        if (avatarDescriptor == null)
            avatarDescriptor = (VRCSDK2.VRC_AvatarDescriptor)target;

        if (pipelineManager == null)
        {
            pipelineManager = avatarDescriptor.GetComponent<VRC.Core.PipelineManager>();
            if (pipelineManager == null)
                avatarDescriptor.gameObject.AddComponent<VRC.Core.PipelineManager>();
        }

        // DrawDefaultInspector();

        avatarDescriptor.ViewPosition = EditorGUILayout.Vector3Field("View Position", avatarDescriptor.ViewPosition);
        //avatarDescriptor.Name = EditorGUILayout.TextField("Avatar Name", avatarDescriptor.Name);
        avatarDescriptor.Animations = (VRCSDK2.VRC_AvatarDescriptor.AnimationSet)EditorGUILayout.EnumPopup("Default Animation Set", avatarDescriptor.Animations);
        avatarDescriptor.CustomStandingAnims = (AnimatorOverrideController)EditorGUILayout.ObjectField("Custom Standing Anims", avatarDescriptor.CustomStandingAnims, typeof(AnimatorOverrideController), true, null);
        avatarDescriptor.CustomSittingAnims = (AnimatorOverrideController)EditorGUILayout.ObjectField("Custom Sitting Anims", avatarDescriptor.CustomSittingAnims, typeof(AnimatorOverrideController), true, null);
        avatarDescriptor.ScaleIPD = EditorGUILayout.Toggle("Scale IPD", avatarDescriptor.ScaleIPD);

        avatarDescriptor.lipSync = (VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle)EditorGUILayout.EnumPopup("Lip Sync", avatarDescriptor.lipSync);
        switch (avatarDescriptor.lipSync)
        {
            case VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle.Default:
                if (GUILayout.Button("Auto Detect!"))
                    AutoDetectLipSync();
                break;

            case VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle.JawFlapBlendShape:
                avatarDescriptor.VisemeSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", avatarDescriptor.VisemeSkinnedMesh, typeof(SkinnedMeshRenderer), true);
                if (avatarDescriptor.VisemeSkinnedMesh != null)
                {
                    DetermineBlendShapeNames();

                    int current = -1;
                    for (int b = 0; b < blendShapeNames.Count; ++b)
                        if (avatarDescriptor.MouthOpenBlendShapeName == blendShapeNames[b])
                            current = b;

                    string title = "Jaw Flap Blend Shape";
                    int next = EditorGUILayout.Popup(title, current, blendShapeNames.ToArray());
                    if (next >= 0)
                        avatarDescriptor.MouthOpenBlendShapeName = blendShapeNames[next];
                }
                break;

            case VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone:
                avatarDescriptor.lipSyncJawBone = (Transform)EditorGUILayout.ObjectField("Jaw Bone", avatarDescriptor.lipSyncJawBone, typeof(Transform), true);
                break;

            case VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape:
                avatarDescriptor.VisemeSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", avatarDescriptor.VisemeSkinnedMesh, typeof(SkinnedMeshRenderer), true);
                if (avatarDescriptor.VisemeSkinnedMesh != null)
                {
					DetermineBlendShapeNames();
					FillVisemeNames();
					
					if (avatarDescriptor.VisemeBlendShapes == null || avatarDescriptor.VisemeBlendShapes.Length != (int)VRCSDK2.VRC_AvatarDescriptor.Viseme.Count)
					{
						avatarDescriptor.VisemeBlendShapes = new string[(int)VRCSDK2.VRC_AvatarDescriptor.Viseme.Count];
						
						for (int i = 0; i < visemeNames.Count; ++i)
						{
							int current = -1;
							
							for (int b = 0; b < blendShapeNames.Count; ++b)
							{
								if (visemeNames[i] == blendShapeNames[b])
									current = b;
								else if (blendShapeNames[b] == "vrc.v_ee" && visemeNames[i] == "vrc.v_e")
									current = b;
							}

							string title = "Viseme: " + visemeNames[i].Replace("vrc.v_", "");
							int next = EditorGUILayout.Popup(title, current, blendShapeNames.ToArray());
							if (next >= 0)
								avatarDescriptor.VisemeBlendShapes[i] = blendShapeNames[next];
						}
						break;
					}
					
                    for (int i = 0; i < (int)VRCSDK2.VRC_AvatarDescriptor.Viseme.Count; ++i)
                    {
                        int current = -1;
                        for (int b = 0; b < blendShapeNames.Count; ++b)
                            if (avatarDescriptor.VisemeBlendShapes[i] == blendShapeNames[b])
                                current = b;

                        string title = "Viseme: " + ((VRCSDK2.VRC_AvatarDescriptor.Viseme)i).ToString();
                        int next = EditorGUILayout.Popup(title, current, blendShapeNames.ToArray());
                        if (next >= 0)
                            avatarDescriptor.VisemeBlendShapes[i] = blendShapeNames[next];
                    }
                }
                break;
        }
        EditorGUILayout.LabelField("Unity Version", avatarDescriptor.unityVersion);
    }

    void DetermineBlendShapeNames()
    {
        if (avatarDescriptor.VisemeSkinnedMesh != null &&
            avatarDescriptor.VisemeSkinnedMesh != selectedMesh)
        {
            blendShapeNames = new List<string>();
            blendShapeNames.Add("-none-");
            selectedMesh = avatarDescriptor.VisemeSkinnedMesh;
            for (int i = 0; i < selectedMesh.sharedMesh.blendShapeCount; ++i)
                blendShapeNames.Add(selectedMesh.sharedMesh.GetBlendShapeName(i));
        }
    }

    void FillVisemeNames()
    {
		visemeNames = new List<string>();
		visemeNames.Add("vrc.v_sil");
		visemeNames.Add("vrc.v_pp");
		visemeNames.Add("vrc.v_ff");
		visemeNames.Add("vrc.v_th");
		visemeNames.Add("vrc.v_dd");
		visemeNames.Add("vrc.v_kk");
		visemeNames.Add("vrc.v_ch");
		visemeNames.Add("vrc.v_ss");
		visemeNames.Add("vrc.v_nn");
		visemeNames.Add("vrc.v_rr");
		visemeNames.Add("vrc.v_aa");
		visemeNames.Add("vrc.v_e");
		visemeNames.Add("vrc.v_ih");
		visemeNames.Add("vrc.v_oh");
		visemeNames.Add("vrc.v_ou");
    }

    void AutoDetectLipSync()
    {
		FillVisemeNames();
		
        var smrs = avatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var smr in smrs)
		{
			for (int i = 0; i < visemeNames.Count; ++i)
			{
				for (int b = 0; b < smr.sharedMesh.blendShapeCount; ++b)
				{
					if (visemeNames[i] == smr.sharedMesh.GetBlendShapeName(b) || (smr.sharedMesh.GetBlendShapeName(b) == "vrc.v_ee" && visemeNames[i] == "vrc.v_e"))
					{
						avatarDescriptor.lipSync = VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
						avatarDescriptor.VisemeSkinnedMesh = smr;
						avatarDescriptor.VisemeBlendShapes = null;
						return;
					}
				}
			}
		}
		
        foreach (var smr in smrs)
        {
            if (smr.sharedMesh.blendShapeCount > 0)
            {
                avatarDescriptor.lipSync = VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle.JawFlapBlendShape;
                avatarDescriptor.VisemeSkinnedMesh = null;
                avatarDescriptor.lipSyncJawBone = null;
                return;
            }
        }

        if (avatarDescriptor.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Jaw) != null)
        {
            avatarDescriptor.lipSync = VRCSDK2.VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone;
            avatarDescriptor.lipSyncJawBone = avatarDescriptor.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Jaw);
            avatarDescriptor.VisemeSkinnedMesh = null;
            return;
        }

    }
}
