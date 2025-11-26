using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace _Scripts.UI
{
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage; // set Image type = Filled
        [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
        [SerializeField] private bool hideWhenFull = true;
        [SerializeField] private bool hideWhenDead = true;

        private Health enemyHealth;
        private Transform mainCam;
        private UnityAction<int,int> onHealthChanged;
        private UnityAction onDeath;

        private void Awake()
        {
            onHealthChanged = (current, max) =>
            {
                if (fillImage == null) return;
                float t = Mathf.Clamp01((float)current / Mathf.Max(1, max));
                fillImage.fillAmount = t;
                if (hideWhenFull)
                    fillImage.transform.parent.gameObject.SetActive(!(t >= 0.999f));
            };

            onDeath = () =>
            {
                if (hideWhenDead && fillImage != null)
                    fillImage.transform.parent.gameObject.SetActive(false);
            };
        }

        private void OnEnable()
        {
            enemyHealth = GetComponentInParent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged.AddListener(onHealthChanged);
                enemyHealth.OnDeath.AddListener(onDeath);
                // immediate update
                onHealthChanged(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
            }

            mainCam = Camera.main?.transform;
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnHealthChanged.RemoveListener(onHealthChanged);
                enemyHealth.OnDeath.RemoveListener(onDeath);
            }
        }

        private void LateUpdate()
        {
            if (enemyHealth == null || fillImage == null) return;

            // Keep UI positioned above the enemy
            transform.position = enemyHealth.transform.position + offset;

            // Face the camera
            if (mainCam != null)
                transform.forward = (transform.position - mainCam.position).normalized;
        }
    }
}
