using UnityEngine;

// a simple class for creating particle pools
public class ParticlePool
{
    private readonly ParticleSystem[] _pool;
    private int _next;

    public ParticlePool(ParticleSystem prefab, int size)
    {
        _pool = new ParticleSystem[size];
        for (var i = 0; i < size; i++)
        {
            _pool[i] = Object.Instantiate(prefab);
            _pool[i].gameObject.SetActive(false);
        }
    }

    public void Play(Vector3 position)
    {
        var ps = _pool[_next];
        _next = (_next + 1) % _pool.Length;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.transform.position = position;
        ps.gameObject.SetActive(true);
        ps.Play();
    }

    public void Destroy()
    {
        foreach (var ps in _pool)
            if (ps) Object.Destroy(ps.gameObject);
    }
}