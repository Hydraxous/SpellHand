using UnityEngine;

namespace SpellHand
{
    public class RingController
    {
        public Magic_scr BoundSpell { get; private set; }
        public int HandAnimationIndex { get; private set; }
        public int FingersAnimationIndex { get; private set; }

        private SpellHandController spellHand;
        private GameObject ring;
        private GameObject gemstoneObject;
        private MeshRenderer sphere;
        private MeshRenderer sparkle;
        private ParticleSystem chargeReadyParticles;
        private Transform mainCam;

        private Vector3 sparkleBaseScale;

        private float chargeScaleIncrease = 3f;
        private float costScaleMax = 400f;
        private float chargeLastUpdate;

        public RingController(SpellHandController spellhand, GameObject ring, Magic_scr boundSpell)
        {
            this.ring = ring;
            this.BoundSpell = boundSpell;
            this.spellHand = spellhand;
        }

        public void Initialize()
        {
            gemstoneObject = ring.transform.GetChild(0).gameObject;
            sparkle = gemstoneObject.transform.GetChild(0).GetComponent<MeshRenderer>();
            sphere = gemstoneObject.transform.GetChild(1).GetComponent<MeshRenderer>();
            chargeReadyParticles = gemstoneObject.transform.GetChild(2).GetComponent<ParticleSystem>();
            sparkleBaseScale = sparkle.transform.localScale * 0.75f;

            if (BoundSpell == null)
            {
                return;
            }

            //Set the colors of the ring to match the spell
            sparkle.material.SetColor("_TintColor", BoundSpell.MAG_COLOR);
            sphere.material.color = BoundSpell.MAG_COLOR;
            ParticleSystem.MainModule main = chargeReadyParticles.main;
            main.startColor = BoundSpell.MAG_COLOR;

            mainCam = Camera.main.transform;

            //Get the animation clips for the hand and fingers.
            string magicName = BoundSpell.name.Replace("(Clone)", "");
            HandAnimationIndex = 0;
            FingersAnimationIndex = 0;

            if (SpellHandPlugin.Instance.AnimationMap.ContainsKey(magicName))
            {
                HandAnimationIndex = SpellHandPlugin.Instance.AnimationMap[magicName].Item1;
                FingersAnimationIndex = SpellHandPlugin.Instance.AnimationMap[magicName].Item2;
            }
        }

        public void Update(float deltaTime)
        {
            if (BoundSpell == null)
            {
                ring.SetActive(false);
                return;
            }

            ring.SetActive(true);
            gemstoneObject.transform.LookAt(mainCam);

            bool canCastSpell = ((BoundSpell.MAG_COST <= spellHand.CON.CURRENT_PL_DATA.PLAYER_M) || BoundSpell.MAG_BL) && BoundSpell.cooling < Time.time;

            if (canCastSpell)
            {
                //Mana cost to scale the sparkle size. This is a bit arbitrary, but it's a nice touch in my opinion.
                float costScaling = BoundSpell.MAG_COST / costScaleMax;

                float charge = 0f;
                //Prevent divide by zero
                if (BoundSpell.MAG_CHARGE_TIME > 0)
                {
                    charge = Mathf.Clamp01(BoundSpell.charge / BoundSpell.MAG_CHARGE_TIME);
                }
                else if (BoundSpell.charge > 0f)
                {
                    charge = 1f;
                }

                sparkle.gameObject.SetActive(true);

                Vector3 fullChargeScale = sparkleBaseScale * (chargeScaleIncrease + (costScaling * 0.65f));
                Vector3 currentTargetScale = Vector3.Lerp(sparkleBaseScale * (1 + costScaling), fullChargeScale, charge);

                //If the charge is full, make the sparkle a bit smaller to indicate that it's ready to cast.
                if (charge == 1f)
                {
                    currentTargetScale = fullChargeScale * 0.7f;
                }

                sparkle.transform.localScale = Vector3.MoveTowards(sparkle.transform.localScale, currentTargetScale, 1.5f * deltaTime);

                float spinSpeedMultiplier = 1f + (charge * 2f);
                if (charge == 1f)
                {
                    spinSpeedMultiplier = 1.5f;
                }

                sparkle.transform.Rotate(Vector3.up, 45 * spinSpeedMultiplier * deltaTime, Space.Self);

                //Play the sparkle particles when the charge is full.
                if (chargeLastUpdate < 1f && charge >= 1f)
                {
                    chargeReadyParticles.Play();
                }

                chargeLastUpdate = charge;
            }
            else
            {
                chargeLastUpdate = 0f;
                sparkle.transform.localScale = Vector3.MoveTowards(sparkle.transform.localScale, Vector3.zero, 0.2f * deltaTime);
                sparkle.transform.Rotate(Vector3.up, -15 * deltaTime, Space.Self);
            }
        }
    }
}
