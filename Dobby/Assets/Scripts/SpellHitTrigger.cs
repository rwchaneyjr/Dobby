using UnityEngine;
using System.Collections;

public class SpellHitTrigger : MonoBehaviour
{
    public Color hitColor = Color.red;
    public float colorDelay = 1f;
    public float destroyDelay = 0.5f;

    private bool hasBeenHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit) return;
        if (other == null) return;

        Debug.Log("Trigger entered by: " + other.name);

        // Ignore ground
        if (CompareTag("Ground"))
        {
            Debug.Log("This object is ground - ignoring");
            return;
        }

        // Only react to the spell / beam object
        if (!other.CompareTag("Spell"))
        {
            Debug.Log("Not spell, ignoring");
            return;
        }

        hasBeenHit = true;
        StartCoroutine(HandleHit());
    }

    IEnumerator HandleHit()
    {
        Debug.Log("HandleHit started for " + gameObject.name);

        yield return new WaitForSeconds(colorDelay);

        Renderer targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            Material newMat = new Material(targetRenderer.material);
            targetRenderer.material = newMat;

            if (newMat.HasProperty("_BaseColor"))
            {
                newMat.SetColor("_BaseColor", hitColor);
            }
            else if (newMat.HasProperty("_Color"))
            {
                newMat.color = hitColor;
            }
        }

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }
}