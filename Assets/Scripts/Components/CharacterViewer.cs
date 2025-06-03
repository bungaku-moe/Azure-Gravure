using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kiraio.Azure.Utils;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.MotionFade;
using Live2D.Cubism.Framework.Pose;
using Live2D.Cubism.Framework.Raycasting;
using Live2D.Cubism.Rendering.Masking;
using UnityEngine;
using UnityEngine.Animations;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("Azure Gravure/Components/Character Viewer")]
    public class CharacterViewer : CubismViewerBase
    {
        // TouchBody = Upper Body, TouchHead = Head, TouchSpecial = Bust
        private readonly string[] _touchAreas = { "TouchBody", "TouchHead", "TouchSpecial" };

        private void OnDestroy()
        {
            foreach (var clip in Animations.Select(animationClip => animationClip.Value))
                clip.events = Array.Empty<AnimationEvent>();
        }

        public async UniTask Initialize()
        {
            // Load the *.model3.json file
            var baseModelName = Path.GetFileName(ModelJsonFile).Replace(".model3.json", "");
            ModelJson = CubismModel3Json.LoadAtPath(
                StorageHelper.NormalizePath(ModelJsonFile),
                LoadAssetAtPath
            );
            ModelJson.FileReferences.DisplayInfo = $"{baseModelName}.cdi3.json";
            ModelJson.FileReferences.Moc = $"{baseModelName}.moc3";
            // ModelJson.FileReferences.Expressions = $"{baseModelName}.exp3.json";
            // ModelJson.FileReferences.Motions = $"{baseModelName}.motion3.json";
            ModelJson.FileReferences.Physics = $"{baseModelName}.physics3.json";
            // PhysicsJson =
            // CubismPhysics3Json.LoadFrom(await WebRequestHelper.GetTextDataAsync(ModelJson.FileReferences.Physics));

            PoseJson = CubismPose3Json.LoadFrom(await WebRequestHelper.GetTextDataAsync(ModelJson.FileReferences.Pose));

            ModelJson.FileReferences.Pose = $"{baseModelName}.pose3.json";
            ModelJson.HitAreas = _touchAreas
                .Select(area => new CubismModel3Json.SerializableHitArea { Name = area, Id = area }).ToArray();

            Model = ModelJson.ToModel();
            Model.gameObject.SetActive(false); // Disable the model gameObject to stop the components being initialized
            Model.transform.parent = transform;
            gameObject.name = Model.name;

            //! Physics Rig is somehow became null after the initialization, disable the Physics for now
            // Model.gameObject.AddComponent<CubismPhysicsController>();

            Raycaster = Model.gameObject.AddComponent<CubismRaycaster>();
            VoiceSource = Model.gameObject.AddComponent<AudioSource>();
            LegacyAnimation = Model.gameObject.AddComponent<Animation>();
            // Animator = Model.gameObject.GetComponent<Animator>();
            // Animator.runtimeAnimatorController = AnimatorController;

            // Add CubismMotionController after assigning CubismFadeMotionList
            MotionController = Model.gameObject.AddComponent<CubismMotionController>();
            FadeController = Model.gameObject.GetComponent<CubismFadeController>();
            // MotionController.enabled = false;
            MotionController.LayerCount = 3;

            // Workaround for the "InstanceId" AnimationEvent error
            Model.gameObject
                .AddComponent<
                    FixAnimationEvent>(); // Fix AnimationEvent errors by bypassing the "InstanceId" AnimationEvent with empty callback

            // Create Mask Texture
            MaskController = Model.gameObject.GetComponent<CubismMaskController>();
            var modelMaskTexture =
                ScriptableObject.CreateInstance<CubismMaskTexture>();
            modelMaskTexture.name = $"{Model.name}MaskTexture";
            MaskController.MaskTexture = modelMaskTexture;

            ExpressionController = Model.gameObject.AddComponent<CubismExpressionController>();
            var expressionList = ScriptableObject.CreateInstance<CubismExpressionList>();
            expressionList.name =
                $"{Path.GetFileNameWithoutExtension(ModelJsonFile).Split(".")[0]}";
            var expressionDataList = new List<CubismExpressionData>();
            foreach (var expression in ModelJson
                         .FileReferences
                         .Expressions)
            {
                var expressionName = expression.File.Split('/', '.')[1];
                var expressionJsonPath = Path.Combine(
                    Path.GetDirectoryName(ModelJsonFile) ?? string.Empty,
                    expression.File
                );
                var expressionJson = CubismExp3Json.LoadFrom(
                    await WebRequestHelper.GetTextDataAsync(expressionJsonPath)
                );

                // Create ExpressionData
                var expressionData = CubismExpressionData.CreateInstance(expressionJson);
                expressionData.name = $"{expressionName}.exp";
                expressionDataList.Add(expressionData);
            }

            expressionList.CubismExpressionObjects = expressionDataList.ToArray();
            ExpressionController.ExpressionsList = expressionList;

            // Create Fade Motion List
            // Ref: https://docs.live2d.com/en/cubism-sdk-manual/motionfade/
            var fadeMotionList = ScriptableObject.CreateInstance<CubismFadeMotionList>();
            fadeMotionList.name =
                $"{Path.GetFileNameWithoutExtension(ModelJsonFile).Split(".")[0]}";
            var motionsData = new Dictionary<int, CubismFadeMotionData>();

            foreach (
                var motion in ModelJson
                    .FileReferences
                    .Motions
                    .Motions
            )
            {
                var motionName = motion[0].File.Split('/', '.')[1];
                var motionJsonPath = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(ModelJsonFile) ?? string.Empty,
                    motion[0].File
                ));
                var motionJson = CubismMotion3Json.LoadFrom(await WebRequestHelper.GetTextDataAsync(motionJsonPath), false);
                motionJson.Meta.FadeInTime = 1f;
                motionJson.Meta.FadeOutTime = 1f;

                // Create FadeMotionData
                var fadeMotion = CubismFadeMotionData.CreateInstance(
                    motionJson,
                    Path.GetFileName(motionJsonPath),
                    motionJson.Meta.Duration,
                    false,
                    false,
                    ModelJson
                );
                fadeMotion.name = $"{motionName}.fade";
                fadeMotion.FadeInTime = 1f;
                fadeMotion.FadeOutTime = 1f;

                for (var i = 0; i < fadeMotion.ParameterFadeInTimes.Length; i++)
                {
                    fadeMotion.ParameterFadeInTimes[i] = 1f;
                    fadeMotion.ParameterFadeOutTimes[i] = 1f;
                }

                // Create AnimationClip
                var animationClip = motionJson.ToAnimationClip(false, false, false, PoseJson);
                animationClip.name = motionName;
                animationClip.legacy = true;

                // NOTE: Not clearing animation events first causing fade motion list errors.
                // Create "InstanceId" AnimationEvent at the start
                // var animationClipEvents = animationClip.events;
                animationClip.events = Array.Empty<AnimationEvent>(); // Clear any events
                // for (var i = 0; i < animationClipEvents.Length; i++)
                // {
                var newEvent = new AnimationEvent
                {
                    functionName = "InstanceId",
                    time = 0,
                    intParameter = animationClip.GetInstanceID()
                };
                animationClip.AddEvent(newEvent);
                // }

                motionsData.Add(animationClip.GetInstanceID(), fadeMotion);
                Animations.Add(motionName, animationClip);
                LegacyAnimation.AddClip(animationClip, motionName); // Add the animation clip to the Animation component
            }

            // Assign the CubismFadeMotionList to the CubismFadeController
            fadeMotionList.MotionInstanceIds = motionsData.Keys.ToArray();
            fadeMotionList.CubismFadeMotionObjects = motionsData.Values.ToArray();
            FadeController.CubismFadeMotionList = fadeMotionList;

            // Add touch area
            _touchAreas
                .Select(area => transform.Find($"{transform.name}/Drawables/{area}").gameObject)
                .Where(target => target != null)
                .ToList()
                .ForEach(target =>
                {
                    var touchArea = target.AddComponent<Touch>();
                    touchArea.CharacterViewer = this;

                    switch (touchArea.name)
                    {
                        case "TouchBody":
                            touchArea.PlayData.Add("touch_body", "touch");
                            break;
                        case "TouchHead":
                            touchArea.PlayData.Add("touch_head", "headtouch");
                            break;
                        case "TouchSpecial":
                            touchArea.PlayData.Add("touch_special", "touch2");
                            break;
                        default:
                            Debug.LogError("No touch data.");
                            break;
                    }
                });

            // Fetch voices audio
            if (Directory.Exists(VoicesDirectory))
            {
                var directoryInfo = new DirectoryInfo(VoicesDirectory);
                var files = directoryInfo.GetFiles();
                foreach (var file in files)
                {
                    try
                    {
                        var clip = await WebRequestHelper.GetAudioClip(file.FullName);
                        if (clip != null)
                            Voices.Add(Path.GetFileNameWithoutExtension(file.Name), clip);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Can't load audio: {ex.Message}");
                    }
                }
            }

            // Enable the model gameObject to initialize the components
            Model.gameObject.SetActive(true);
            Model.ForceUpdateNow();
            Model.RefreshParameterStore();
            FadeController.Refresh();

            // Play intro animations
            PlayMotion("effect", WrapMode.Loop, 2);
            PlayMotion("login", WrapMode.Once, 0, QueueMode.CompleteOthers, onAnimationEnd: () => AllowInteraction = true);
            PlayMotion("idle", WrapMode.Loop, 0, QueueMode.CompleteOthers);
        }
    }
}
