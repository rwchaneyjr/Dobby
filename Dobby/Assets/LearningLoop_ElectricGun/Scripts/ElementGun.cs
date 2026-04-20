using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElementGun : MonoBehaviour
{
    public Transform firePoint;
    public float range = 20f;
    public ParticleSystem beamParticles;
    public Color hitColor = Color.red;
    private HashSet<GameObject> processedObjects = new HashSet<GameObject>();

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.Log("Mouse held");

            ShootBeam();

            if (beamParticles != null && !beamParticles.isPlaying)
            {
                beamParticles.Play();
            }

         
           
        }
        else
        {
            if (beamParticles != null && beamParticles.isPlaying)
            {
                beamParticles.Stop();
            }

           
        }
    }

    void ShootBeam()
    {
        if (firePoint == null)
        {
            Debug.LogError("NO FIREPOINT ASSIGNED");
            return;
        }

        RaycastHit hit;

        Vector3 origin = firePoint.position;
        Vector3 direction = firePoint.forward;

        Debug.Log("Ray Origin: " + origin);
        Debug.Log("Ray Direction: " + direction);

        Debug.DrawRay(origin, direction * range, Color.cyan);

        if (Physics.Raycast(origin, direction, out hit, range))
        {
            GameObject target = hit.collider.gameObject;

            Debug.Log("HIT: " + target.name);

            // Ignore ground
            if (target.CompareTag("Ground"))
            {
                Debug.Log("Hit ground - ignoring");
                return;
            }

            // Prevent processing same target over and over
            if (processedObjects.Contains(target))
            {
                Debug.Log("Already processing: " + target.name);
                return;
            }

            processedObjects.Add(target);

            Transform targetTransform = target.transform;
            Renderer targetRenderer = target.GetComponent<Renderer>();

            if (targetRenderer == null)
            {
                targetRenderer = target.GetComponentInChildren<Renderer>();
            }

            StartCoroutine(HandleHit(target, targetTransform, targetRenderer, target.name));
        }
        else
        {
            Debug.Log("Raycast missed");
        }
    }

    IEnumerator HandleHit(GameObject target, Transform targetTransform, Renderer targetRenderer, string targetName)
    {
        Debug.Log("HandleHit started for " + targetName);

        // Wait 1.5 seconds before turning red
        Debug.Log("Waiting 1.5 seconds before color change");
        yield return new WaitForSeconds(1.5f);

        if (targetRenderer != null)
        {
            Debug.Log("Preparing material instance for " + targetName);
            targetRenderer.material = new Material(targetRenderer.material);

            if (targetRenderer.material.HasProperty("_BaseColor"))
            {
                Debug.Log("Using _BaseColor on " + targetName);
                targetRenderer.material.SetColor("_BaseColor", hitColor);
                Debug.Log("Changed _BaseColor on " + targetName);
            }
            else if (targetRenderer.material.HasProperty("_Color"))
            {
                Debug.Log("Using _Color on " + targetName);
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
            Debug.LogWarning("No Renderer found, cannot change color on " + targetName);
        }

        // Wait 0.5 more seconds before destroy
        Debug.Log("Waiting 0.5 seconds before destroy for " + targetName);
        yield return new WaitForSeconds(0.5f);

        if (targetTransform != null)
        {
            Debug.Log("Destroying " + targetName);
            Destroy(targetTransform.gameObject);
        }
        else
        {
            Debug.LogWarning("targetTransform is NULL, cannot destroy " + targetName);
        }
    }
}