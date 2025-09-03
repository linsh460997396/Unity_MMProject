using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EmeraldAI.Utility
{
    public class EmeraldHealthBar : MonoBehaviour
    {
        #region Health Bar Variables
        Camera m_Camera;
        Canvas canvas;
        Image HealthBar;
        Image HealthBarDamage;
        CanvasGroup CG;
        Text AINameUI;
        Text AILevelUI;
        Coroutine C;
        EmeraldSystem EmeraldComponent;
        EmeraldUI EmeraldUI;
        EmeraldHealth EmeraldHealth;
        Coroutine CoroutineTransitionDamage;
        #endregion

        void Start()
        {
            InitializeHealthBar();
        }

        /// <summary>
        /// Initialize the health bar.
        /// </summary>
        void InitializeHealthBar ()
        {
            //Since the transform order of the health bar is always the same, get the parent's parent to get all need Emerald AI Components.
            canvas = GetComponent<Canvas>();
            EmeraldUI = transform.parent.parent.GetComponent<EmeraldUI>();
            EmeraldHealth = transform.parent.parent.GetComponent<EmeraldHealth>();
            EmeraldComponent = transform.parent.parent.GetComponent<EmeraldSystem>();
            if (m_Camera == null) m_Camera = GameObject.FindGameObjectWithTag(EmeraldUI.CameraTag).GetComponent<Camera>(); //Get a reference to the camera via the EmeraldUI.CameraTag.

            CG = GetComponent<CanvasGroup>();
            HealthBar = transform.Find("AI Health Bar Background/AI Health Bar").GetComponent<Image>();
            HealthBarDamage = transform.Find("AI Health Bar Background/AI Health Bar (Damage)").GetComponent<Image>();
            AINameUI = transform.Find("AI Name Text").GetComponent<Text>();
            AILevelUI = transform.Find("AI Level Text").GetComponent<Text>();

            EmeraldHealth.OnDeath += FadeOutUI; //Subscribe FadeOutUI to the OnDeath delegate.
            EmeraldHealth.OnTakeDamage += TransitionDamage; //Subscribe TransitionDamage to the OnTakeDamage delegate.
            EmeraldHealth.OnTakeCritDamage += TransitionDamage; //Subscribe TransitionDamage to the OnTakeCritDamage delegate.
            EmeraldHealth.OnHealRateTick += TransitionHealing; //Subscribe TransitionHealing to the OnHealRateTick delegate.
            EmeraldHealth.OnHealthChange += UpdateHealthUI; //Subscribe UpdateHealthUI to the OnHealthChange delegate.
        }

        void Update()
        {
            CalculateUI();
        }

        public void CalculateUI()
        {
            if (m_Camera != null)
            {
                if (HealthBar != null)
                {
                    canvas.transform.parent.LookAt(canvas.transform.parent.position + m_Camera.transform.rotation * Vector3.forward,
                    m_Camera.transform.rotation * Vector3.up);

                    float dist = Vector3.Distance(m_Camera.transform.position, transform.position);
                    if (dist < EmeraldUI.MaxUIScaleSize)
                    {
                        canvas.transform.localScale = new Vector3(dist * 0.085f, dist * 0.085f, dist * 0.085f);
                    }
                    else
                    {
                        canvas.transform.localScale = new Vector3(EmeraldUI.MaxUIScaleSize * 0.085f, EmeraldUI.MaxUIScaleSize * 0.085f, EmeraldUI.MaxUIScaleSize * 0.085f);
                    }
                }
            }
        }

        /// <summary>
        /// Fades out the UI when called. (This is handled automatically through the EmeraldHeath.OnDeath delegate).
        /// </summary>
        void FadeOutUI()
        {
            if (gameObject.activeSelf)
            {
                if (C != null) { StopCoroutine(C); }
                C = StartCoroutine(FadeOutUIInternal(0.0f, 1.5f));
            }
        }

        void OnDisable()
        {
            if (EmeraldComponent != null && !EmeraldComponent.CombatComponent.CombatState) ResetValues(); //Resets the UI values back to their defaults.
        }

        IEnumerator FadeOutUIInternal(float DesiredValue, float TransitionTime)
        {
            HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
            float alpha = CG.alpha;
            float t = 0;

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime;
                Color newColor1 = new Color(1, 1, 1, Mathf.Lerp(alpha, DesiredValue, t));
                CG.alpha = newColor1.a;
                AINameUI.color = new Color(AINameUI.color.r, AINameUI.color.g, AINameUI.color.b, newColor1.a);
                AILevelUI.color = new Color(AILevelUI.color.r, AILevelUI.color.g, AILevelUI.color.b, newColor1.a);
                yield return null;
            }

            gameObject.SetActive(false);
        }

        void TransitionDamage ()
        {
            if (gameObject.activeSelf)
            {
                if (CoroutineTransitionDamage != null) StopCoroutine(CoroutineTransitionDamage);
                CoroutineTransitionDamage = StartCoroutine(TransitionDamageInternal());
            }
            else
            {
                HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
                HealthBarDamage.fillAmount = HealthBar.fillAmount;
            }
        }

        IEnumerator TransitionDamageInternal ()
        {
            HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
            float Start = HealthBarDamage.fillAmount;
            float t = 0;
            yield return new WaitForSeconds(0.75f);

            while ((t / 1f) < 1)
            {
                t += Time.deltaTime;
                HealthBarDamage.fillAmount = Mathf.Lerp(Start, HealthBar.fillAmount, t);
                yield return null;
            }
        }

        void TransitionHealing ()
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(TransitionHealingInternal());
            }
            else
            {
                HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
                HealthBarDamage.fillAmount = HealthBar.fillAmount;
            }
        }

        /// <summary>
        /// Updates the AI's health bar based on its current health.
        /// </summary>
        void UpdateHealthUI ()
        {
            HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
            HealthBarDamage.fillAmount = HealthBar.fillAmount;
        }

        IEnumerator TransitionHealingInternal()
        {
            float HealAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
            float Start = HealthBar.fillAmount;
            float t = 0;

            while ((t / 1f) < 1)
            {
                t += Time.deltaTime;
                HealthBar.fillAmount = Mathf.Lerp(Start, HealAmount, t);
                HealthBarDamage.fillAmount = Mathf.Lerp(Start, HealAmount, t);
                yield return null;
            }
        }

        /// <summary>
        /// Resets the UI values back to their defaults.
        /// </summary>
        void ResetValues ()
        {
            if (CG != null)
            {
                Color newColor1 = new Color(1, 1, 1, 1);
                CG.alpha = newColor1.a;
                AINameUI.color = new Color(AINameUI.color.r, AINameUI.color.g, AINameUI.color.b, newColor1.a);
                AILevelUI.color = new Color(AILevelUI.color.r, AILevelUI.color.g, AILevelUI.color.b, newColor1.a);
                HealthBar.fillAmount = 1;
                HealthBarDamage.fillAmount = 1;
            }
        }
    }
}