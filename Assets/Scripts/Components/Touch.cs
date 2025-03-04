using System.Collections;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(CubismRaycastable))]
    public class Touch : MonoBehaviour
    {
        private CubismRaycastable _raycastable;
        public CharacterViewer CharacterViewer { get; set; }

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
                if (touch != null && touch == this)
                {
                    // CharacterViewer.MotionController.StopAllAnimation();
                    CharacterViewer.AllowInteraction = false;
                    StartCoroutine(PlayTouch());
                    CharacterViewer.PlayMotion("touch_body", false, 1, CubismMotionPriority.PriorityIdle,
                        OnTouchBodyComplete);
                }
            }
            else
            {
                Debug.LogWarning("Main Camera is null. Can'\t process Touch interaction.");
            }
        }

        private IEnumerator PlayTouch()
        {
            var duration = 0.3f; // Match fade-in time
            var elapsed = 0f;

            while (elapsed < duration)
            {
                CharacterViewer.MotionController.SetLayerWeight(0, Mathf.Lerp(1f, 0f, elapsed / duration));
                CharacterViewer.MotionController.SetLayerWeight(1, Mathf.Lerp(0f, 1f, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void OnTouchBodyComplete(float instanceId)
        {
            CubismViewerBase.ResetMotionPriorities(CharacterViewer.MotionController);
            CharacterViewer.MotionController.StopAllAnimation();

            CharacterViewer.PlayMotion(
                "idle",
                true
            );

            CharacterViewer.MotionController.AnimationEndHandler -= OnTouchBodyComplete;
            CharacterViewer.AllowInteraction = true;
        }
    }
}
