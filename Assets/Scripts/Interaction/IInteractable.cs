using UnityEngine;
using InteractionTypes;

namespace Interaction
{
    [RequireComponent(typeof(Collider))]
    public class IInteractable : MonoBehaviour
    {
        public RubbishType rubbishType;
        public InteractableState interactableState = InteractableState.IDLE;
    
        // This is done the way it is below for alpha. Once we have a model, the code below can essentially be removed
        private void Awake() => UpdateRubbishColour();
        private void OnValidate() => UpdateRubbishColour();

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private MaterialPropertyBlock mpb;
        private Renderer renderer; 

        public void UpdateRubbishColour()
        {
            renderer ??= GetComponent<Renderer>();
            mpb ??= new MaterialPropertyBlock();
            
            var colour = RubbishHelper.GetColourForRubbishType(rubbishType);
            
            renderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorId, colour);
            renderer.SetPropertyBlock(mpb);
        }
    } 
}


