using UnityEngine;

public abstract class Item : MonoBehaviour, IInteractable, ICollectible
{
    private bool isCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        Interact(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCollected) return;
        Interact(collision.gameObject);
    }

    public virtual void Interact(GameObject interactor)
    {
        if (isCollected) return;

        if (!interactor.TryGetComponent<Michael>(out _))
        {
            return;
        }

        Collect(interactor);
    }

    public virtual void Collect(GameObject collector)
    {
        if (isCollected) return;

        if (!collector.TryGetComponent<Michael>(out Michael michael))
        {
            return;
        }

        ApplyTo(michael);
        isCollected = true;
        Destroy(gameObject);
    }

    protected abstract void ApplyTo(Michael michael);
}
