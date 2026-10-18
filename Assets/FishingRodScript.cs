using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FishingRodScript : MonoBehaviour
{
    [Header("Objects/References")]
    public XRGrabInteractable grabber;
    public Transform rodTip;
    public Rigidbody bobber;
    public LineRenderer line;
    public ReelCrank crank;
    public XROrigin xrOrigin;
    public LayerMask groundMask;
    public MonoBehaviour[] lockWhileReeling;

    [Header("Variables")]
    public float castSpeed = 3f;
    public float castPower = 1.2f;
    public float maxLength = 25f;
    public float movementPerTurn = 1.5f;

    enum State { Idle, Flying, Reeling }
    State state;
    Vector3 tipVel, lastTip, lastBobber, target, pendingMove;
    float nextCast, castTime;
    bool wasHeld;

    void Start()
    {
        line.positionCount = 2;
        lastTip = rodTip.position;
        ResetCast();
    }

    void Update()
    {
        // smooth the rod tip velocity
        Vector3 tip = rodTip.position;
        bool held = grabber.isSelected;

        if (held && wasHeld)
        {
            tipVel = Vector3.Lerp(tipVel, (tip - lastTip) / Time.deltaTime, 0.3f);
        }
        else
        {
            tipVel = Vector3.zero;
            // 0.5 second cooldown after picking up rod
            // I kept having issues with the rod teleporting to your hand being detected as a huge instant fling
            if (held) nextCast = Time.time + 0.5f;
        }
        wasHeld = held;
        lastTip = tip;

        // reset if fishing rod dropped
        if (state != State.Idle && !held) ResetCast();

        switch (state)
        {
            case State.Idle:
                bobber.position = tip;
                if (held &&
                    Time.time > nextCast &&
                    tipVel.magnitude > castSpeed) Cast();
                break;
            case State.Flying:  Fly(); break;
            case State.Reeling: Reel(); break;
        }

        line.SetPosition(0, tip);
        line.SetPosition(1, bobber.position);
    }

    void Cast()
    {
        state = State.Flying;
        castTime = Time.time;
        lastBobber = bobber.position;
        bobber.isKinematic = false;
        bobber.linearVelocity = Vector3.ClampMagnitude(tipVel * castPower, 12f);
    }

    void Fly()
    {
        Vector3 pos = bobber.position;
        bool hit = Physics.Linecast(lastBobber, pos, out RaycastHit h, groundMask);
        lastBobber = pos;

        if (hit && h.normal.y > 0.5f)
        {
            target = h.point;
            bobber.isKinematic = true;
            bobber.position = h.point;
            state = State.Reeling;
            foreach (var b in lockWhileReeling) b.enabled = false;
        }
        else if (hit ||
            Time.time - castTime > 5f ||
            Vector3.Distance(pos, rodTip.position) > maxLength)
        {
            ResetCast();
        }
    }

    void Reel()
    {
        Vector3 cam = xrOrigin.Camera.transform.position;
        Vector3 feet = new Vector3(cam.x, xrOrigin.transform.position.y, cam.z);
        Vector3 toTarget = target - feet;

        if (toTarget.magnitude < 0.3f) { ResetCast(); return; }   // arrived

        // bugfix: crank feedback loop, apply movement in LateUpdate instead
        float step = crank.ReelDegrees / 360f * movementPerTurn;
        pendingMove = Vector3.ClampMagnitude(toTarget, step);
    }

    void LateUpdate()
    {
        if (pendingMove == Vector3.zero) return;
        xrOrigin.transform.position += pendingMove;
        pendingMove = Vector3.zero;
    }

    void ResetCast()
    {
        state = State.Idle;
        pendingMove = Vector3.zero;
        nextCast = Time.time + 0.5f;
        bobber.isKinematic = true;
        foreach (var b in lockWhileReeling) b.enabled = true;
    }
}
