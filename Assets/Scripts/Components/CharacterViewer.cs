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
using Live2D.Cubism.Rendering;
using UnityEngine;
// using Live2D.Cubism.Framework.Physics;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("Azure Gravure/Components/Character Viewer")]
    public class CharacterViewer : CubismViewerBase
    {
        [SerializeField] private string m_Path;

        // TouchBody = Upper Body, TouchHead = Head, TouchSpecial = Bust
        private readonly string[] _touchAreas = { "TouchBody", "TouchHead", "TouchSpecial" };

        public override void Awake()
        {
            base.Awake();
            Initialize().AsAsyncUnitUniTask();
        }

        private async UniTask Initialize()
        {
            ModelJson = CubismModel3Json.LoadAtPath(StorageHelper.NormalizePath(m_Path), LoadAssetAtPath);
            ModelJson.FileReferences.Physics = Path.GetFileName(m_Path.Replace(".model3.json", ".physics3.json"));

            Model = ModelJson.ToModel();
            Model.transform.parent = transform;
            gameObject.name = Model.name;
            Model.gameObject.SetActive(false); // Disable the model gameObject to stop the components being initialized

            Model.gameObject.AddComponent<CubismUpdateController>();
            Model.gameObject.AddComponent<CubismParameterStore>();
            Model.gameObject.AddComponent<CubismPoseController>();
            Model.gameObject.AddComponent<CubismExpressionController>();
            //! Physics Rig is somehow became null after the initialization, disable the Physics for now
            // Model.gameObject.AddComponent<CubismPhysicsController>();

            // Add Raycaster
            Raycaster = Model.gameObject.AddComponent<CubismRaycaster>();

            // Add CubismMotionController after assigning CubismFadeMotionList
            MotionController = Model.gameObject.AddComponent<CubismMotionController>();
            FadeController = Model.gameObject.GetComponent<CubismFadeController>();
            MotionController.enabled = false;
            MotionController.LayerCount = 2;

            // Fix AnimationEvent errors by bypassing the "InstanceId" AnimationEvent with empty callback
            Model.gameObject.AddComponent<FixAnimationEvent>();
            Animator = GetComponentInChildren<Animator>(true);

            // Create Fade Motion List
            var fadeMotionList =
                ScriptableObject.CreateInstance<CubismFadeMotionList>();
            fadeMotionList.name = $"{Path.GetFileNameWithoutExtension(m_Path).Split(".")[0]}";

            var motionsData =
                new Dictionary<int, CubismFadeMotionData>();

            foreach (
                var motion in ModelJson
                    .FileReferences
                    .Motions
                    .Motions
            )
            {
                var motionName = motion[0].File.Split('/', '.')[1];
                var motionJsonPath = Path.Combine(Path.GetDirectoryName(m_Path) ?? string.Empty, motion[0].File);
                var motionJson = CubismMotion3Json.LoadFrom(
                    await WebRequestHelper.GetTextDataAsync(motionJsonPath)
                );

                // Create FadeMotionData
                var fadeMotion = CubismFadeMotionData.CreateInstance(
                    motionJson,
                    Path.GetFileName(motionJsonPath),
                    motionJson.Meta.Duration,
                    true,
                    true
                );
                fadeMotion.name = $"{motionName}.fade";

                // fadeMotion.FadeOutTime = 2f;
                // fadeMotion.FadeInTime = 2f;
                // for (int i = 0; i < fadeMotion.ParameterFadeInTimes.Length; i++)
                // {
                //     fadeMotion.ParameterFadeInTimes[i] = 2f;
                //     fadeMotion.ParameterFadeOutTimes[i] = 2f;
                // }

                // Create AnimationClip
                var animationClip = motionJson.ToAnimationClip();
                animationClip.name = motionName;

                // Create "InstanceId" AnimationEvent at the start
                var instanceEvent = new AnimationEvent
                {
                    functionName = "InstanceId",
                    time = 0,
                    intParameter = animationClip.GetInstanceID()
                };
                animationClip.events = Array.Empty<AnimationEvent>(); // Clear any events
                animationClip.AddEvent(instanceEvent);

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
                });

            // Enable the model gameObject to initialize the components
            MotionController.enabled = true;
            Model.gameObject.SetActive(true);

            // Animator.runtimeAnimatorController =
            //     Instantiate(Resources.Load<RuntimeAnimatorController>("BaseController"));
            // Animator.runtimeAnimatorController.name = Model.name;

            UpdateController = GetComponentInChildren<CubismUpdateController>();
            PoseController = GetComponentInChildren<CubismPoseController>();
            PoseController.Refresh();
            FadeController.Refresh();
            UpdateController.Refresh();

            // Play the login animation
            await UniTask.WaitForSeconds(0.1f);
            PlayMotion("login");
            MotionController.AnimationEndHandler += OnLoginComplete;
        }

        private void OnLoginComplete(float instanceId)
        {
            PlayMotion("idle", true);
            MotionController.AnimationEndHandler -= OnLoginComplete;
            ResetMotionPriorities(MotionController);
            AllowInteraction = true;
        }
    }
}
