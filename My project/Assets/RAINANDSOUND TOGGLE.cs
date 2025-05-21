
using UnityEngine;

public class RAINANDSOUNDTOGGLE : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();

        if (particleSystem == null)
        {
            Debug.LogWarning("no ParticleSystem");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (particleSystem != null)
            {
                particleSystem.gameObject.SetActive(!particleSystem.gameObject.activeSelf);
            }
            else if (particleSystem = null)
            {
                particleSystem.gameObject.SetActive(!particleSystem.gameObject.activeSelf);
            }
        }
    }
}
