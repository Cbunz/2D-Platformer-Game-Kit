using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour {

    public Damageable representedDamageable;
    public GameObject heartIconPrefab;

    protected Animator[] heartIconAnimators;

    protected readonly int hashHealthPointsParameter = Animator.StringToHash("HealthPoints");
    public float heartIconAnchorWidth = 0.05f;

    IEnumerator Start()
    {
        representedDamageable = GameObject.Find("Player").GetComponent<Damageable>();
        if (representedDamageable == null)
            yield break;


        yield return null;

        heartIconAnimators = new Animator[representedDamageable.startingHealth / 2];

        for (int i = 0; i < (representedDamageable.startingHealth / 2); i++)
        {
            GameObject heartIcon = Instantiate(heartIconPrefab);
            heartIcon.transform.SetParent(transform);
            RectTransform heartIconRect = heartIcon.transform as RectTransform;
            heartIconRect.anchoredPosition = Vector2.zero;
            heartIconRect.sizeDelta = Vector2.zero;
            heartIconRect.anchorMin += new Vector2(heartIconAnchorWidth, 0f) * i;
            heartIconRect.anchorMax += new Vector2(heartIconAnchorWidth, 0f) * i;
            heartIconAnimators[i] = heartIcon.GetComponent<Animator>();
        }

        for (int i = 0; i < heartIconAnimators.Length; i++)
        {
            heartIconAnimators[i].SetInteger(hashHealthPointsParameter, ((representedDamageable.CurrentHealth >= 2 * (i + 1)) ? 2 : ((representedDamageable.CurrentHealth >= 2 * (i + 1) - 1) ? 1 : 0)));
        }
    }

    public void ChangeHitPointUI(Damageable damageable)
    {
        if (heartIconAnimators == null)
            return;

        for (int i = 0; i < heartIconAnimators.Length; i++)
        {
            heartIconAnimators[i].SetInteger(hashHealthPointsParameter, ((damageable.CurrentHealth > 2 * (i + 1) - 1 ? 2 : ((damageable.CurrentHealth > 2 * (i + 1) - 2) ? 1 : 0))));
        }
    }
}
