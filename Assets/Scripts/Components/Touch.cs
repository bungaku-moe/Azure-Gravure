using Kiraio.Azure.Core;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("Azure Gravure/Components/Touch")]
    [RequireComponent(typeof(CubismRaycastable))]
    public class Touch : MonoBehaviour
    {
        CubismRaycastable raycastable;
        CharacterViewer characterViewer;

        public CharacterViewer CharacterViewer
        {
            get => characterViewer;
            set => characterViewer = value;
        }

        void Awake()
        {
            raycastable = GetComponent<CubismRaycastable>();
            raycastable.Precision = CubismRaycastablePrecision.Triangles;
        }

        void OnEnable()
        {
            CharacterViewer.InputManager.PointerClick.performed += Interact;
        }

        void OnDestroy()
        {
            CharacterViewer.InputManager.PointerClick.performed -= Interact;
        }

        void Interact(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                CubismRaycastHit[] results = new CubismRaycastHit[1];
                int hitCount = CharacterViewer.Raycaster.Raycast(ray, results);

                if (hitCount > 0 && results[0].Drawable.TryGetComponent(out Touch _))
                {
                    CharacterViewer.MotionController.PlayAnimation(
                        CharacterViewer.Animations["touch_body"],
                        priority: 3,
                        isLoop: false
                    );
                }
            }
        }
    }
}
