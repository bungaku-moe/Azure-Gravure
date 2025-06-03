using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Gilzoide.SerializableCollections;
using Kiraio.Azure.Core;
using Kiraio.Azure.Utils;
using Live2D.Cubism.Core;
// using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.MotionFade;
// using Live2D.Cubism.Framework.Pose;
using Live2D.Cubism.Framework.Raycasting;
// using Live2D.Cubism.Rendering;
using Live2D.Cubism.Rendering.Masking;
using UnityEngine;
using UnityEngine.Playables;

namespace Kiraio.Azure.Components
{
    public class CubismViewerBase : MonoBehaviour
    {
        public string ModelJsonFile { get; set; }
        public string VoicesDirectory { get; set; }

        protected CubismModel3Json ModelJson { get; set; }

        // protected CubismPhysics3Json PhysicsJson { get; set; }
        protected CubismPose3Json PoseJson { get; set; }
        protected CubismModel Model { get; set; }
        public CubismRaycaster Raycaster { get; set; }

        public CubismMotionController MotionController { get; set; }

        // protected CubismPoseController PoseController { get; set; }
        protected CubismExpressionController ExpressionController { get; set; }
        protected CubismMaskController MaskController { get; set; }

        protected CubismFadeController FadeController { get; set; }
        // protected CubismUpdateController UpdateController { get; set; }
        // protected CubismRenderController RenderController { get; set; }

        // public Animator Animator { get; set; }
        public Animation LegacyAnimation { get; set; }
        public RuntimeAnimatorController AnimatorController { get; set; }
        public AudioSource VoiceSource { get; set; }

        // public SerializableDictionary<string, AudioClip> AnimationsVoices { get; set; } = new();
        public SerializableDictionary<string, AudioClip> Voices { get; set; } = new();
        public SerializableDictionary<string, AnimationClip> Animations { get; set; } = new();

        public MainControl MainControl { get; set; }
        public InputManager InputManager { get; set; }
        public bool AllowInteraction { get; set; } = false;

        protected virtual void Awake()
        {
            MainControl = FindObjectsByType<MainControl>(FindObjectsSortMode.None)[0];
            InputManager = FindObjectsByType<InputManager>(FindObjectsSortMode.None)[0];
        }

        /// <summary>
        ///     Loads asset.
        /// </summary>
        /// <param name="absolutePath">Path to asset.</param>
        /// <returns>The asset on success; <see langword="null" /> otherwise.</returns>
        public static T LoadAsset<T>(string absolutePath) where T : class
        {
            return LoadAssetAtPath(typeof(T), absolutePath) as T;
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
        /// Play animation.
        /// </summary>
        /// <param name="motionName"></param>
        /// <param name="wrapMode"></param>
        /// <param name="layerIndex"></param>
        /// <param name="queueMode"></param>
        /// <param name="playMode"></param>
        /// <param name="onAnimationEnd">Callback to invoke when the animation ends.</param>
        public void PlayMotion(
            string motionName,
            WrapMode wrapMode = WrapMode.Once,
            int layerIndex = 0,
            QueueMode queueMode = QueueMode.PlayNow,
            PlayMode playMode = PlayMode.StopSameLayer,
            Action onAnimationEnd = null)
        {
            if (!Animations.TryGetValue(motionName, out _))
            {
                Debug.LogWarning($"Motion {motionName} not found.");
                return;
            }

            LegacyAnimation.animatePhysics = true;
            LegacyAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
            LegacyAnimation[motionName].wrapMode = wrapMode;
            LegacyAnimation[motionName].layer = layerIndex;

            LegacyAnimation.PlayQueued(motionName, queueMode, playMode);

            if (onAnimationEnd != null)
                WaitForAnimationEndAsync(motionName, onAnimationEnd).Forget();
        }

        private async UniTaskVoid WaitForAnimationEndAsync(string motionName, Action onAnimationEnd)
        {
            while (LegacyAnimation.IsPlaying(motionName))
                await UniTask.Yield();

            onAnimationEnd?.Invoke();
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
    }
}
