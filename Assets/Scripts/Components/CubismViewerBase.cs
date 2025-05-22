using System;
using System.Reflection;
using Gilzoide.SerializableCollections;
using Kiraio.Azure.Core;
using Kiraio.Azure.Utils;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.MotionFade;
using Live2D.Cubism.Framework.Pose;
using Live2D.Cubism.Framework.Raycasting;
using Live2D.Cubism.Rendering;
using Live2D.Cubism.Rendering.Masking;
using UnityEngine;

namespace Kiraio.Azure.Components
{
    public class CubismViewerBase : MonoBehaviour
    {
        public string ModelJsonFile { get; set; }
        public string VoicesDirectory { get; set; }

        protected CubismModel3Json ModelJson { get; set; }
        protected CubismPose3Json PoseJson { get; set; }
        protected CubismModel Model { get; set; }
        public CubismRaycaster Raycaster { get; set; }

        public CubismMotionController MotionController { get; set; }
        public CubismMaskController MaskController { get; set; }
        public CubismFadeController FadeController { get; set; }
        protected CubismUpdateController UpdateController { get; set; }
        protected CubismPoseController PoseController { get; set; }
        protected CubismRenderController RenderController { get; set; }

        public Animator Animator { get; set; }

        public AudioSource VoiceSource { get; set; }

        // public SerializableDictionary<string, AudioClip> AnimationsVoices { get; set; } = new();
        public SerializableDictionary<string, AudioClip> Voices { get; set; } = new();
        public SerializableDictionary<string, AnimationClip> Animations { get; set; } = new();

        public MainControl MainControl { get; set; }
        public InputManager InputManager { get; set; }
        public bool AllowInteraction { get; set; }

        protected virtual void Awake()
        {
            MainControl = FindObjectsByType<MainControl>(FindObjectsSortMode.None)[0];
            InputManager = FindObjectsByType<InputManager>(FindObjectsSortMode.None)[0];
        }

        /// <summary>
        ///     A helper method provided by the Live2D itself to load assets from the path.
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        protected static object LoadAssetAtPath(Type assetType, string absolutePath)
        {
            if (assetType == typeof(byte[]))
                return WebRequestHelper.GetBinaryData(absolutePath);
            if (assetType == typeof(string))
                return WebRequestHelper.GetTextData(absolutePath);
            if (assetType != typeof(Texture2D)) throw new NotSupportedException();
            var texture = new Texture2D(1, 1);
            texture.LoadImage(WebRequestHelper.GetBinaryData(absolutePath));
            return texture;
        }

        /// <summary>
        ///     A helper method to play the motion.
        /// </summary>
        /// <param name="motionName">The animation name.</param>
        /// <param name="isLoop">Loop the animation?</param>
        /// <param name="layerIndex">Which layer to play the animation?</param>
        /// <param name="priority">How important is the animation? Scale from 0 ~ 3.</param>
        public void PlayMotion(
            string motionName,
            bool isLoop = false,
            int layerIndex = 0,
            int priority = CubismMotionPriority.PriorityIdle,
            Action<int> onComplete = null
        )
        {
            if (!Animations.TryGetValue(motionName, out var animationClip))
            {
                Debug.LogWarning($"Motion {motionName} not found.");
                return;
            }

            // Stop the current motion on the target layer (if not looped)
            // if (MotionController.IsPlayingAnimation(layerIndex) && !isLoop)
            // {
            //     MotionController.StopAnimation(0, layerIndex);
            // }

            // Play the motion with the specified priority
            MotionController.PlayAnimation(
                animationClip,
                layerIndex,
                priority,
                isLoop
            );

            // Register completion handler
            if (onComplete != null) MotionController.AnimationEndHandler += onComplete;
        }

        public void PlayVoice(string voiceName)
        {
            if (!string.IsNullOrEmpty(voiceName))
            {
                VoiceSource.clip = Voices[voiceName];
                VoiceSource.Play();
            }
            else
            {
                Debug.LogWarning("No voice clip with the name provided.");
            }
        }

        /// <summary>
        ///     Resets the motion priorities for all layers.
        /// </summary>
        /// <param name="motionController">The CubismMotionController instance.</param>
        public static void ResetMotionPriorities(CubismMotionController motionController)
        {
            if (motionController == null)
            {
                Debug.LogWarning("MotionController is null. Cannot reset priorities.");
                return;
            }

            // Access the private _motionPriorities field using reflection
            var motionPrioritiesField = typeof(CubismMotionController).GetField(
                "_motionPriorities",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (motionPrioritiesField == null)
            {
                Debug.LogError("Failed to access _motionPriorities field. Has the Cubism SDK changed?");
                return;
            }

            // Get the current _motionPriorities array
            var motionPriorities = (int[])motionPrioritiesField.GetValue(motionController);

            if (motionPriorities == null || motionPriorities.Length == 0)
            {
                Debug.LogWarning("_motionPriorities array is null or empty. Cannot reset priorities.");
                return;
            }

            // Reset all priorities to 0 (PriorityNone)
            for (var i = 0; i < motionPriorities.Length; i++) motionPriorities[i] = CubismMotionPriority.PriorityNone;

            // Update the _motionPriorities field
            motionPrioritiesField.SetValue(motionController, motionPriorities);
        }
    }
}
