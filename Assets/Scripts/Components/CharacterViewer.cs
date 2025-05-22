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
            var physicsJson = Path.GetFileName(ModelJsonFile.Replace(".model3.json", ".physics3.json"));
            var poseJson = Path.GetFileName(ModelJsonFile.Replace(".model3.json", ".pose3.json"));
            ModelJson = CubismModel3Json.LoadAtPath(
                StorageHelper.NormalizePath(ModelJsonFile),
                LoadAssetAtPath
            );
            ModelJson.FileReferences.Physics = File.Exists(physicsJson)
                ? physicsJson
                : string.Empty;
            ModelJson.FileReferences.Pose = File.Exists(poseJson) ? poseJson : string.Empty;
            PoseJson = CubismPose3Json.LoadFrom(await WebRequestHelper.GetTextDataAsync(ModelJson.FileReferences.Pose));

            Model = ModelJson.ToModel();
            Model.transform.parent = transform;
            Model.gameObject.SetActive(false); // Disable the model gameObject to stop the components being initialized
            gameObject.name = Model.name;

            Model.gameObject.AddComponent<CubismUpdateController>();
            Model.gameObject.AddComponent<CubismParameterStore>();
            Model.gameObject.AddComponent<CubismPoseController>();
            Model.gameObject.AddComponent<CubismExpressionController>();
            Raycaster = Model.gameObject.AddComponent<CubismRaycaster>();
            Animator = GetComponentInChildren<Animator>(true);
            VoiceSource = Model.gameObject.AddComponent<AudioSource>();

            //! Physics Rig is somehow became null after the initialization, disable the Physics for now
            // Model.gameObject.AddComponent<CubismPhysicsController>();

            // Add CubismMotionController after assigning CubismFadeMotionList
            MotionController = Model.gameObject.AddComponent<CubismMotionController>();
            FadeController = Model.gameObject.GetComponent<CubismFadeController>();
            MotionController.enabled = false;
            MotionController.LayerCount = 3;

            // Workaround for the "InstanceId" AnimationEvent error
            Model.gameObject
                    .AddComponent<
                        FixAnimationEvent>(); // Fix AnimationEvent errors by bypassing the "InstanceId" AnimationEvent with empty callback
            // fixAnimationEvent.Viewer = this;

            // Create Mask Texture
            MaskController = Model.gameObject.GetComponent<CubismMaskController>();
            var modelMaskTexture =
                ScriptableObject.CreateInstance<CubismMaskTexture>();
            modelMaskTexture.name = $"{Model.name}MaskTexture";
            MaskController.MaskTexture = modelMaskTexture;

            // Create Fade Motion List
            // Ref: https://docs.live2d.com/en/cubism-sdk-manual/motionfade/
            var fadeMotionList = ScriptableObject.CreateInstance<CubismFadeMotionList>();
            fadeMotionList.name =
                $"{Path.GetFileNameWithoutExtension(ModelJsonFile).Split(".")[0]}";
            var motionsData = new Dictionary<int, CubismFadeMotionData>();

            int fps = 30;

            foreach (
                var motion in ModelJson
                    .FileReferences
                    .Motions
                    .Motions
            )
            {
                var motionName = motion[0].File.Split('/', '.')[1];
                var motionJsonPath = Path.Combine(
                    Path.GetDirectoryName(ModelJsonFile) ?? string.Empty,
                    motion[0].File
                );
                var motionJson = CubismMotion3Json.LoadFrom(
                    await WebRequestHelper.GetTextDataAsync(motionJsonPath)
                );
                motionJson.Meta.FadeInTime = 1f;
                motionJson.Meta.FadeOutTime = 1f;
                fps = Convert.ToInt16(motionJson.Meta.Fps);

                // Create FadeMotionData
                var fadeMotion = CubismFadeMotionData.CreateInstance(
                    motionJson,
                    Path.GetFileName(motionJsonPath),
                    motionJson.Meta.Duration,
                    true,
                    true,
                    ModelJson
                );
                fadeMotion.name = $"{motionName}.fade";
                fadeMotion.FadeInTime = 1f;
                fadeMotion.FadeOutTime = 1f;

                // Create AnimationClip
                var animationClip = motionJson.ToAnimationClip(true, false, true, PoseJson);
                animationClip.name = motionName;
                animationClip.legacy = false;

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

            // Enable the model gameObject to initialize the components
            MotionController.enabled = true;
            Model.gameObject.SetActive(true);

            UpdateController = GetComponentInChildren<CubismUpdateController>();
            PoseController = GetComponentInChildren<CubismPoseController>();
            PoseController.Refresh();
            FadeController.Refresh();
            UpdateController.Refresh();

            Application.targetFrameRate = fps;

            // Play the login animation
            PlayInitialMotions();
        }

        private void PlayInitialMotions()
        {
            // Play the effect motion on layer 2
            PlayMotion("effect", true, 2);

            // Play the login motion on layer 0
            PlayMotion("login", onComplete: OnLoginComplete);

            PlayVoice("login");
        }

        private void OnLoginComplete(int instanceId)
        {
            // Play the idle motion on layer 0 after login completes
            PlayMotion("idle", true);

            // Unregister the completion handler
            MotionController.AnimationEndHandler -= OnLoginComplete;

            // Allow interactions
            AllowInteraction = true;
        }
    }
}
