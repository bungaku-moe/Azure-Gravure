using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gilzoide.SerializableCollections;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(CubismHitDrawable))]
    [RequireComponent(typeof(CubismRaycastable))]
    public class Touch : MonoBehaviour
    {
        public SerializableDictionary<string, string> PlayData = new();
        private CubismDrawable _drawable;
        private CubismHitDrawable _hitDrawable;
        private CubismRaycastable _raycastable;

        public CharacterViewer CharacterViewer { get; set; }
        // public string AnimationName { get; set; }
        // public string[] VoicesName { get; set; }

        private void Awake()
        {
            _drawable = GetComponent<CubismDrawable>();
            _hitDrawable = GetComponent<CubismHitDrawable>();
            _raycastable = GetComponent<CubismRaycastable>();
            _raycastable.Precision = CubismRaycastablePrecision.Triangles;

            _hitDrawable.Name = _drawable.name;
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
                var randomData = PlayData.ElementAt(Random.Range(0, PlayData.Count));
                CharacterViewer.PlayMotion(randomData.Key, layerIndex: 1, onAnimationEnd: () => CharacterViewer.AllowInteraction = true);
                // CharacterViewer.PlayVoice(randomData.Value);
            }
            else
            {
                Debug.LogWarning("Main Camera is null. Can'\t process Touch interaction.");
            }
        }
    }
}
