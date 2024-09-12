using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.MotionFade;
using UnityEngine;

namespace Kiraio.Azure.Components
{
    public class CharacterViewer : MonoBehaviour
    {
        [SerializeField]
        string m_Path;

        List<AnimationClip> animations = new List<AnimationClip>();
        CubismModel3Json modelJson;
        CubismModel model;
        CubismMotionController motionController;
        CubismFadeController fadeController;
        // CubismFadeMotionList fadeMotionList;

        void Start()
        {
            modelJson = CubismModel3Json.LoadAtPath(m_Path, BuiltinLoadAssetAtPath);
            model = modelJson.ToModel();
            model.transform.parent = transform;
            gameObject.name = model.name;
            model.gameObject.SetActive(false); // Disable the model gameobject to stop the components being initialized

            // Add CubismMotionController after assigning CubismFadeMotionList
            motionController = model.gameObject.AddComponent<CubismMotionController>();
            motionController.enabled = false;
            fadeController = model.gameObject.GetComponent<CubismFadeController>();

            // Fix AnimationEvent errors
            model.gameObject.AddComponent<FixAnimationEvent>();

            // Create Fade Motion List
            CubismFadeMotionList fadeMotionList = ScriptableObject.CreateInstance<CubismFadeMotionList>();
            fadeMotionList.MotionInstanceIds = Array.Empty<int>();
            fadeMotionList.CubismFadeMotionObjects = Array.Empty<CubismFadeMotionData>();
            fadeMotionList.name = $"{Path.GetFileNameWithoutExtension(m_Path).Split(".")[0]}";

            List<int> instanceIds = new List<int>();
            List<CubismFadeMotionData> fadeMotions = new List<CubismFadeMotionData>();

            foreach (
                CubismModel3Json.SerializableMotion[] motion in modelJson
                    .FileReferences
                    .Motions
                    .Motions
            )
            {
                string motionJsonPath = Path.Combine(Path.GetDirectoryName(m_Path), motion[0].File);
                CubismMotion3Json motionJson = CubismMotion3Json.LoadFrom(
                    File.ReadAllText(motionJsonPath)
                );
                string motionNormalizedName = motion[0].File.Split('/', '.')[1];

                // Create Fade Motion Data
                CubismFadeMotionData fadeMotion = CubismFadeMotionData.CreateInstance(
                    motionJson,
                    Path.GetFileName(motionJsonPath),
                    motionJson.Meta.Duration
                );
                fadeMotion.name = $"{motionNormalizedName}.fade";
                fadeMotions.Add(fadeMotion);

                // Create AnimationClip
                AnimationClip animation = motionJson.ToAnimationClip();
                animation.name = motionNormalizedName;

                // Create AnimationEvent "InstanceId" at start
                AnimationEvent instanceEvent = new AnimationEvent()
                {
                    functionName = "InstanceId",
                    time = 0,
                    intParameter = animation.GetInstanceID()
                };
                animation.events = new AnimationEvent[0];
                animation.AddEvent(instanceEvent);

                instanceIds.Add(animation.GetInstanceID());
                animations.Add(animation);
            }

            // Assign the CubismFadeMotionList to the fade controller
            fadeMotionList.MotionInstanceIds = instanceIds.ToArray();
            fadeMotionList.CubismFadeMotionObjects = fadeMotions.ToArray();
            fadeController.CubismFadeMotionList = fadeMotionList;

            motionController.enabled = true;
            model.gameObject.SetActive(true);

            // Play animation at index 2 (idle) using the fade controller
            motionController.PlayAnimation(animations[2], isLoop: true);
        }

        object BuiltinLoadAssetAtPath(Type assetType, string absolutePath)
        {
            if (assetType == typeof(byte[]))
            {
                return File.ReadAllBytes(absolutePath);
            }
            else if (assetType == typeof(string))
            {
                return File.ReadAllText(absolutePath);
            }
            else if (assetType == typeof(Texture2D))
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(File.ReadAllBytes(absolutePath));
                return texture;
            }
            throw new NotSupportedException();
        }
    }
}
