using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gilzoide.SerializableCollections;
using Kiraio.Azure.Utils;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.MotionFade;
using UnityEngine;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("Azure Gravure/Components/Character Viewer")]
    public class CharacterViewer : MonoBehaviour
    {
        [SerializeField]
        string m_Path;

        readonly SerializableDictionary<string, AnimationClip> animations =
            new SerializableDictionary<string, AnimationClip>();
        CubismModel3Json modelJson;
        CubismModel model;
        CubismMotionController motionController;
        CubismFadeController fadeController;

        void Awake()
        {
            Initialize();
        }

        async void Initialize()
        {
            modelJson = CubismModel3Json.LoadAtPath(m_Path, BuiltinLoadAssetAtPath);
            model = modelJson.ToModel();
            model.transform.parent = transform;
            gameObject.name = model.name;
            model.gameObject.SetActive(false); // Disable the model gameObject to stop the components being initialized

            // Add CubismMotionController after assigning CubismFadeMotionList
            motionController = model.gameObject.AddComponent<CubismMotionController>();
            fadeController = model.gameObject.GetComponent<CubismFadeController>();
            motionController.enabled = false;

            // Fix AnimationEvent errors by bypassing the AnimationEvent with empty callback
            model.gameObject.AddComponent<FixAnimationEvent>();

            // Create Fade Motion List
            CubismFadeMotionList fadeMotionList =
                ScriptableObject.CreateInstance<CubismFadeMotionList>();
            fadeMotionList.name = $"{Path.GetFileNameWithoutExtension(m_Path).Split(".")[0]}";

            Dictionary<int, CubismFadeMotionData> motionsData =
                new Dictionary<int, CubismFadeMotionData>();

            foreach (
                CubismModel3Json.SerializableMotion[] motion in modelJson
                    .FileReferences
                    .Motions
                    .Motions
            )
            {
                string motionName = motion[0].File.Split('/', '.')[1];

                string motionJsonPath = Path.Combine(Path.GetDirectoryName(m_Path), motion[0].File);
                CubismMotion3Json motionJson = CubismMotion3Json.LoadFrom(
                    await WebRequestHelper.GetTextDataAsync(motionJsonPath)
                );

                // Create FadeMotionData
                CubismFadeMotionData fadeMotion = CubismFadeMotionData.CreateInstance(
                    motionJson,
                    Path.GetFileName(motionJsonPath),
                    motionJson.Meta.Duration
                );
                fadeMotion.name = $"{motionName}.fade";

                // Create AnimationClip
                AnimationClip animation = motionJson.ToAnimationClip();
                animation.name = motionName;

                // Create AnimationEvent "InstanceId" at the start
                AnimationEvent instanceEvent = new AnimationEvent()
                {
                    functionName = "InstanceId",
                    time = 0,
                    intParameter = animation.GetInstanceID(),
                };
                animation.events = new AnimationEvent[0];
                animation.AddEvent(instanceEvent);

                motionsData.Add(animation.GetInstanceID(), fadeMotion);
                animations.Add(motionName, animation);
            }

            // Assign the CubismFadeMotionList to the fade controller
            fadeMotionList.MotionInstanceIds = motionsData.Keys.ToArray();
            fadeMotionList.CubismFadeMotionObjects = motionsData.Values.ToArray();
            fadeController.CubismFadeMotionList = fadeMotionList;

            // Enable the model gameObject to initialize the components
            motionController.enabled = true;
            model.gameObject.SetActive(true);

            // Play the intro animation
            motionController.PlayAnimation(animations["home"], isLoop: false);
            motionController.AnimationEndHandler += _ =>
                motionController.PlayAnimation(animations["idle"]);
        }

        object BuiltinLoadAssetAtPath(Type assetType, string absolutePath)
        {
            if (assetType == typeof(byte[]))
            {
                return WebRequestHelper.GetBinaryData(absolutePath);
            }
            else if (assetType == typeof(string))
            {
                return WebRequestHelper.GetTextData(absolutePath);
            }
            else if (assetType == typeof(Texture2D))
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(WebRequestHelper.GetBinaryData(absolutePath));
                return texture;
            }
            throw new NotSupportedException();
        }
    }
}
