using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ReelCrank : XRSimpleInteractable
{
    public Transform crankVisual;
    public float deadzone = 1.5f;

    public float ReelDegrees { get; private set; }

    float lastAngle;

    float AngleOf(IXRSelectInteractor hand)
    {
        Vector3 p = transform.InverseTransformPoint(hand.GetAttachTransform(this).position);
        return Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        lastAngle = AngleOf(args.interactorObject);
    }

    void Update()
    {
        ReelDegrees = 0f;
        if (!isSelected) return;

        float angle = AngleOf(interactorsSelecting[0]);
        float delta = Mathf.DeltaAngle(lastAngle, angle);
        if (Mathf.Abs(delta) < deadzone) return;
        lastAngle = angle;

        ReelDegrees = Mathf.Abs(delta);
        if (crankVisual) crankVisual.Rotate(0f, 0f, delta, Space.Self);
    }
}
