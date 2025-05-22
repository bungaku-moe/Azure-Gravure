using System.Linq;
using Gilzoide.SerializableCollections;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(CubismRaycastable))]
    public class Touch : MonoBehaviour
    {
        public SerializableDictionary<string, string> PlayData = new();
        private CubismRaycastable _raycastable;

        public CharacterViewer CharacterViewer { get; set; }
        // public string AnimationName { get; set; }
        // public string[] VoicesName { get; set; }

        private void Awake()
        {
            _raycastable = GetComponent<CubismRaycastable>();
            _raycastable.Precision = CubismRaycastablePrecision.Triangles;
        }

        private void OnEnable()
        {
            if (CharacterViewer == null)
            {
                Debug.LogWarning("Character Viewer is null. No need to register Touch interaction");
                return;
            }

            if (CharacterViewer.InputManager == null)
            {
                Debug.Log("Input Manager is null. Can'\t register Touch interaction.");
                return;
            }

            if (CharacterViewer != null && CharacterViewer.InputManager != null)
                CharacterViewer.InputManager.PointerClick.performed += Interact;
        }

        private void OnDisable()
        {
            if (CharacterViewer != null && CharacterViewer.InputManager != null)
                CharacterViewer.InputManager.PointerClick.performed -= Interact;
        }

        private void Interact(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || !CharacterViewer.AllowInteraction) return;
            if (Camera.main != null)
            {
                var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                var results = new CubismRaycastHit[1];
                var hitCount = (byte)CharacterViewer.Raycaster.Raycast(ray, results);

                if (hitCount <= 0 || !results[0].Drawable.TryGetComponent(out Touch touch)) return;
                if (touch == null && touch != this) return;

                CharacterViewer.AllowInteraction = false;
                // CharacterViewer.MotionController.StopAnimation(0);
                // StartCoroutine(PlayTouch());
                var randomData = PlayData.ElementAt(Random.Range(0, PlayData.Count));
                CharacterViewer.PlayMotion(randomData.Key, layerIndex: 0, priority: 2,
                    onComplete: OnTouchBodyComplete);
                CharacterViewer.PlayVoice(randomData.Value);
            }
            else
            {
                Debug.LogWarning("Main Camera is null. Can'\t process Touch interaction.");
            }
        }

        // private IEnumerator PlayTouch()
        // {
        //     var elapsed = 0f;
        //
        //     while (elapsed < CharacterViewer.FadeDuration)
        //     {
        //         CharacterViewer.MotionController.SetLayerWeight(0,
        //             Mathf.Lerp(1f, 0f, elapsed / CharacterViewer.FadeDuration));
        //         CharacterViewer.MotionController.SetLayerWeight(1,
        //             Mathf.Lerp(0f, 1f, elapsed / CharacterViewer.FadeDuration));
        //         elapsed += Time.smoothDeltaTime;
        //         yield return null;
        //     }
        // }

        private void OnTouchBodyComplete(int instanceId)
        {
            // CharacterViewer.MotionController.StopAllAnimation();

            CharacterViewer.PlayMotion(
                "idle",
                true, priority: 3
            );
            CubismViewerBase.ResetMotionPriorities(CharacterViewer.MotionController);

            CharacterViewer.MotionController.AnimationEndHandler -= OnTouchBodyComplete;
            CharacterViewer.AllowInteraction = true;
        }
    }
}
