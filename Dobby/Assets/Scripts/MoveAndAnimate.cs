using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveAndAnimate : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;

    [Header("Animation")]
    public Animator animatorK;

    [Header("Spell")]
    public Transform firePoint;
    public ParticleSystem beamParticles;
    public float range = 20f;
    public float spellDuration = 1.5f;
    public float hitDelay = 1.0f;
    public Color hitColor = Color.red;

    private bool isCasting = false;
    private HashSet<GameObject> processedObjects = new HashSet<GameObject>();

    void Start()
    {
        if (animatorK == null)
        {
            animatorK = GetComponent<Animator>();
        }

        if (beamParticles != null)
        {
            beamParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        if (animatorK == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        // Turn player toward movement
        if (moveDir.magnitude > 0.01f && !isCasting)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // Move player
        if (!isCasting)
        {
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }

        // Walk animation
        bool isMoving = moveDir.magnitude > 0f && !isCasting;
        animatorK.SetBool("Walk", isMoving);
        animatorK.SetBool("Back", false);

        // Dance
        if (Input.GetKeyDown(KeyCode.F))
        {
            animatorK.SetBool("Dance", true);
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            animatorK.SetBool("Dance", false);
        }

        // Cast with G
        if (Input.GetKeyDown(KeyCode.G) && !isCasting)
        {
            StartCoroutine(CastSpell());
        }
    }

    IEnumerator CastSpell()
    {
        isCasting = true;
        processedObjects.Clear();

        animatorK.SetBool("Spell", true);

        // small delay so animation starts first
        yield return new WaitForSeconds(0.15f);

        if (beamParticles != null)
        {
            beamParticles.Play();
        }

        ShootBeam();

        yield return new WaitForSeconds(spellDuration);

        if (beamParticles != null)
        {
            beamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        animatorK.SetBool("Spell", false);
        isCasting = false;
    }

    void ShootBeam()
    {
        if (firePoint == null)
        {
            Debug.LogError("NO FIREPOINT ASSIGNED");
            return;
        }

        RaycastHit hit;

        // start at hand position
        Vector3 origin = firePoint.position;

        // use PLAYER forward, flattened, so the ray does not tilt up/down
       // Vector3 direction = Camera.main.transform.forward;
        Vector3 direction = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        // push ray a little forward so it does not start inside the player
        origin += direction * 0.2f;

        Debug.Log("Ray Origin: " + origin);
        Debug.Log("Ray Direction: " + direction);

        Debug.DrawRay(origin, direction * range, Color.cyan, 2f);

        // aim particle system the same way
        if (beamParticles != null)
        {
            beamParticles.transform.position = origin;
            beamParticles.transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Physics.SphereCast(origin, 0.6f, direction, out hit, range))
            {
            GameObject target = hit.collider.gameObject;

            Debug.Log("HIT: " + target.name);

            if (target.CompareTag("Ground"))
            {
                Debug.Log("Hit ground - ignoring");
                return;
            }

            if (processedObjects.Contains(target))
            {
                Debug.Log("Already processing: " + target.name);
                return;
            }

            processedObjects.Add(target);

            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                targetRenderer = target.GetComponentInChildren<Renderer>();
            }

            StartCoroutine(HandleHit(target, targetRenderer, target.name));
        }
        else
        {
            Debug.Log("Raycast missed");
        }
    }

    IEnumerator HandleHit(GameObject target, Renderer targetRenderer, string targetName)
    {
        Debug.Log("HandleHit started for " + targetName);

        yield return new WaitForSeconds(hitDelay);

        if (target == null) yield break;

        if (targetRenderer != null)
        {
            Debug.Log("Preparing material instance for " + targetName);
            targetRenderer.material = new Material(targetRenderer.material);

            if (targetRenderer.material.HasProperty("_BaseColor"))
            {
                targetRenderer.material.SetColor("_BaseColor", hitColor);
                Debug.Log("Changed _BaseColor on " + targetName);
            }
            else if (targetRenderer.material.HasProperty("_Color"))
            {
                targetRenderer.material.color = hitColor;
                Debug.Log("Changed _Color on " + targetName);
            }
            else
            {
                Debug.LogWarning("Material has NO _BaseColor or _Color on " + targetName);
            }
        }
        else
        {
            Debug.LogWarning("No Renderer found for " + targetName);
        }

        yield return new WaitForSeconds(0.5f);

        if (target != null)
        {
            Debug.Log("Destroying " + targetName);
            Destroy(target);
        }
    }
}