using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellParticleHit : MonoBehaviour
{
    public Color hitColor = Color.red;
    public float hitDelay = 1f;

    private HashSet<GameObject> processedObjects = new HashSet<GameObject>();

    // 🔥 This will be set by your raycast script
    public GameObject currentRayTarget;

    void OnParticleCollision(GameObject other)
    {
        if (other == null) return;

        Debug.Log("Particle hit: " + other.name);

        // Ignore ground
        if (other.CompareTag("Ground")) return;

        // 🔴 ONLY allow if raycast is ALSO targeting this object
        if (other != currentRayTarget)
        {
            Debug.Log("Particle hit but NOT raycast target");
            return;
        }

        if (processedObjects.Contains(other)) return;

        processedObjects.Add(other);

        StartCoroutine(HandleHit(other));
    }

    IEnumerator HandleHit(GameObject target)
    {
        Debug.Log("Confirmed hit (ray + particle): " + target.name);

        yield return new WaitForSeconds(hitDelay);

        if (target == null) yield break;

        Renderer targetRenderer = target.GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            targetRenderer = target.GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            Material mat = new Material(targetRenderer.material);
            targetRenderer.material = mat;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", hitColor);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.color = hitColor;
            }
        }

        Destroy(target, 0.5f);
    }

    public void ClearProcessed()
    {
        processedObjects.Clear();
    }
}