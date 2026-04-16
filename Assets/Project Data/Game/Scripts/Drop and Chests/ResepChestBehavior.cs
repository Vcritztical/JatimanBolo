using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class ResepChestBehavior : AbstractChestBehavior
    {
        protected static readonly int IS_OPEN_HASH = Animator.StringToHash("IsOpen");

        [SerializeField] Animator rvAnimator;
        [SerializeField] Button rvButton;
        [SerializeField] GameObject ambilResep;

        private void Awake()
        {
            rvButton.onClick.AddListener(OnButtonClick);
        }

        public override void Init(List<DropData> drop)
        {
            base.Init(drop);

            rvAnimator.transform.localScale = Vector3.zero;

            isRewarded = true;
        }

        public override void ChestApproached()
        {
            if (opened)
                return;

            animatorRef.SetTrigger(SHAKE_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, true);


        }

        public override void ChestLeft()
        {
            if (opened)
                return;

            animatorRef.SetTrigger(IDLE_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, false);


        }

        private void OnButtonClick()
        {
            ambilResep.SetActive(true);

                {
                    opened = true;

                    animatorRef.SetTrigger(OPEN_HASH);
                    rvAnimator.SetBool(IS_OPEN_HASH, false);

                    Tween.DelayedCall(0.3f, () =>
                    {
                        DropResources();
                        particle.SetActive(false);
                        Vibration.Vibrate(VibrationIntensity.Light);
                    });

                   
                }
        }
    }
}