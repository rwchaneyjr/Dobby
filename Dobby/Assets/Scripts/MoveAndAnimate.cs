using UnityEngine;

public class MoveAndAnimate : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;

    [Header("Animation")]
    public Animator animatorK;

    [Header("Spell")]
    public Transform firePoint;
    public LineRenderer lightningLine;
    public float spellRange = 20f;

    // This tracks whether the lightning already fired once
    private bool hasFired = false;

    // Put your exact spell state name here
    private string spellStateName = "Standing 2H Magic Attack 04";

    void Start()
    {
        if (animatorK == null)
        {
            animatorK = GetComponent<Animator>();
        }

        if (lightningLine != null)
        {
            lightningLine.enabled = false;
            lightningLine.positionCount = 2;
        }
    }

    void Update()
    {
        if (animatorK == null) return;

        // -----------------------------
        // INPUT
        // -----------------------------
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        // -----------------------------
        // MOVEMENT DIRECTION
        // -----------------------------
        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        // -----------------------------
        // ROTATE TOWARD MOVEMENT
        // -----------------------------
        if (moveDir.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // -----------------------------
        // MOVE CHARACTER
        // -----------------------------
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        // -----------------------------
        // WALK ANIMATION
        // -----------------------------
        bool isMoving = moveDir.magnitude > 0f;
        animatorK.SetBool("Walk", isMoving);
        animatorK.SetBool("Back", false);

        // -----------------------------
        // DANCE
        // -----------------------------
        if (Input.GetKeyDown(KeyCode.F))
        {
            animatorK.SetBool("Dance", true);
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            animatorK.SetBool("Dance", false);
        }

        // -----------------------------
        // START SPELL
        // -----------------------------
        if (Input.GetKeyDown(KeyCode.G))
        {
            animatorK.speed = 1f;          // make sure animation can play
            animatorK.SetBool("Spell", true);
            hasFired = false;              // allow lightning to fire this cast
        }

        // -----------------------------
        // STOP SPELL
        // -----------------------------
        if (Input.GetKeyUp(KeyCode.G))
        {
            animatorK.SetBool("Spell", false);
            animatorK.speed = 1f;          // unfreeze animation
            hasFired = false;              // reset for next cast

            if (lightningLine != null)
            {
                lightningLine.enabled = false;
            }
        }

        // -----------------------------
        // CHECK SPELL STATE
        // Freeze at end and fire lightning once
        // -----------------------------
        AnimatorStateInfo state = animatorK.GetCurrentAnimatorStateInfo(0);

        if (state.IsName(spellStateName))
        {
            // normalizedTime goes from 0 to 1 for one playthrough
            if (state.normalizedTime >= 0.95f)
            {
                // Fire lightning once
                if (!hasFired)
                {
                    ShootLightning();
                    hasFired = true;
                }

                // Freeze at the end pose
                animatorK.speed = 0f;
            }
        }
    }

    void ShootLightning()
    {
        if (firePoint == null || lightningLine == null) return;

        RaycastHit hit;

        // Start slightly in front of the hand
        Vector3 start = firePoint.position + firePoint.forward * 0.1f;
        Vector3 direction = firePoint.forward;
        Vector3 end;

        if (Physics.Raycast(start, direction, out hit, spellRange))
        {
            end = hit.point;

            Debug.Log("Hit: " + hit.collider.name);

            // Destroy anything except Ground
            if (!hit.collider.CompareTag("Ground"))
            {
                Destroy(hit.collider.gameObject);
            }
        }
        else
        {
            end = start + direction * spellRange;
        }

        lightningLine.enabled = true;
        lightningLine.SetPosition(0, start);
        lightningLine.SetPosition(1, end);
    }
}