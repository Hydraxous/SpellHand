using UnityEngine;

namespace SpellHand
{
    public class SpellHandController : MonoBehaviour
    {
        public GameObject RingModel1;
        public GameObject RingModel2;

        private RingController Ring1;
        private RingController Ring2;

        public Animation HandAnimation;
        public Animation FingersAnimation;

        private AnimationClip Fingers_Magic_Equip;
        private AnimationClip Fingers_Magic_Idle;
        private AnimationClip[] Fingers_Magic_Cast_Anims;


        private AnimationClip Hand_Magic_Equip;
        private AnimationClip Hand_Magic_Idle;
        private AnimationClip[] Hand_Magic_Cast_Anims;

        public CONTROL CON;

        private void Awake()
        {
            RingModel1 = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject;
            RingModel2 = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetChild(1).gameObject;

            HandAnimation = transform.GetChild(0).GetChild(0).GetComponent<Animation>();
            FingersAnimation = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Animation>();
        }

        private void Start()
        {
            CON = FindObjectOfType<CONTROL>();

            if (CON == null || (CON.EQ_MAG1 == null && CON.EQ_MAG2 == null))
            {
                //No spells equipped or something is broken, just disable the entire hand.
                gameObject.SetActive(false);
                return;
            }

            Ring1 = new RingController(this, RingModel1, CON.EQ_MAG2);
            Ring2 = new RingController(this, RingModel2, CON.EQ_MAG1);

            Ring1.Initialize();
            Ring2.Initialize();

            Fingers_Magic_Idle = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Fingers_Magic_Idle));
            Fingers_Magic_Equip = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Fingers_Magic_Equip));

            int anims = 3;
            Fingers_Magic_Cast_Anims = new AnimationClip[anims];
            for (int i = 0; i < anims; i++)
            {
                Fingers_Magic_Cast_Anims[i] = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>($"Fingers_Magic_Cast{i}");
            }

            Hand_Magic_Idle = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Hand_Magic_Idle));
            Hand_Magic_Equip = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Hand_Magic_Equip));

            int handAnims = 4;
            Hand_Magic_Cast_Anims = new AnimationClip[handAnims];
            for (int i = 0; i < handAnims; i++)
            {
                Hand_Magic_Cast_Anims[i] = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>($"Hand_Magic_Cast{i}");
            }

            HandAnimation.clip = Hand_Magic_Equip;
            HandAnimation.Play();

            FingersAnimation.clip = Fingers_Magic_Equip;
            FingersAnimation.Play();

            Patches.OnMagicScrCast += Patches_OnMagicScrCast;
        }

        //Invoked when a spell is cast.
        private void Patches_OnMagicScrCast(Magic_scr obj)
        {
            CancelInvoke(nameof(ResetHandAnimation));
            CancelInvoke(nameof(ResetFingerAnimation));

            RingController ring = Ring1.BoundSpell == obj ? Ring1 : Ring2;

            AnimationClip handClip = GetClip(ring.HandAnimationIndex, Hand_Magic_Cast_Anims);
            HandAnimation.Stop();
            HandAnimation.clip = handClip;
            HandAnimation.Play();
            Invoke(nameof(ResetHandAnimation), handClip.length);

            AnimationClip fingerClip = GetClip(ring.FingersAnimationIndex, Fingers_Magic_Cast_Anims);
            FingersAnimation.Stop();
            FingersAnimation.clip = fingerClip;
            FingersAnimation.Play();
            Invoke(nameof(ResetFingerAnimation), fingerClip.length);
        }


        private AnimationClip GetClip(int index, AnimationClip[] clips)
        {
            if(index < 0)
            {
                return clips[UnityEngine.Random.Range(0, clips.Length)];
            }else if(index >= clips.Length)
            {
                return clips[clips.Length - 1];
            }

            return clips[index];
        }

        private void ResetFingerAnimation()
        {
            FingersAnimation.Stop();
            FingersAnimation.clip = Fingers_Magic_Idle;
            FingersAnimation.Play();
        }

        private void ResetHandAnimation()
        {
            HandAnimation.Stop();
            HandAnimation.clip = Hand_Magic_Idle;
            HandAnimation.Play();
        }


        private void Update()
        {
            if(Ring1.BoundSpell == null && Ring2.BoundSpell == null)
            {
                //No spells equipped or something is broken, just disable the entire hand.
                gameObject.SetActive(false);
                return;
            }

            Ring1.Update(Time.deltaTime);
            Ring2.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            Patches.OnMagicScrCast -= Patches_OnMagicScrCast;
        }

    }

    public class RingController
    {
        public Magic_scr BoundSpell;
        public SpellHandController Hand;

        public int HandAnimationIndex;
        public int FingersAnimationIndex;

        public RingController(SpellHandController spellhand, GameObject ring, Magic_scr boundSpell)
        {
            Ring = ring;
            BoundSpell = boundSpell;
            Hand = spellhand;
        }

        public void Initialize()
        {
            Orb = Ring.transform.GetChild(0).gameObject;
            sparkle = Orb.transform.GetChild(0).GetComponent<MeshRenderer>();
            sphere = Orb.transform.GetChild(1).GetComponent<MeshRenderer>();
            sparkleParticles = Orb.transform.GetChild(2).GetComponent<ParticleSystem>();
            sparkleScale = sparkle.transform.localScale;

            if (BoundSpell == null)
            {
                return;
            }

            //Set the colors of the ring to match the spell :3
            sparkle.material.SetColor("_TintColor", BoundSpell.MAG_COLOR);
            sphere.material.color = BoundSpell.MAG_COLOR;
            ParticleSystem.MainModule main = sparkleParticles.main;
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

        public GameObject Ring;
        public GameObject Orb;
        private MeshRenderer sphere;
        private MeshRenderer sparkle;
        private ParticleSystem sparkleParticles;

        private Vector3 sparkleScale;
        private float chargeScaleIncrease = 3f;
        private Transform mainCam;

        private float chargeLastUpdate;

        public void Update(float dt)
        {
            if(BoundSpell == null)
            {
                Ring.SetActive(false);
                return;
            }

            Ring.SetActive(true);
            Orb.transform.LookAt(mainCam);

            bool canCast = ((BoundSpell.MAG_COST <= Hand.CON.CURRENT_PL_DATA.PLAYER_M) || BoundSpell.MAG_BL) && BoundSpell.cooling < Time.time;
            if (canCast)
            {
                float charge = 0f;
                if (BoundSpell.MAG_CHARGE_TIME > 0)
                {
                    charge = Mathf.Clamp01(BoundSpell.charge / BoundSpell.MAG_CHARGE_TIME);
                }
                else if(BoundSpell.charge > 0f)
                {
                    charge = 1f;
                }

                sparkle.gameObject.SetActive(true);

                Vector3 bigScale = sparkleScale * chargeScaleIncrease;
                Vector3 targetScale = Vector3.Lerp(sparkleScale, bigScale, charge);
                sparkle.transform.localScale = Vector3.MoveTowards(sparkle.transform.localScale, targetScale, 1.5f * dt);

                float spinSpeedBonus = 1f + (charge * 2f);
                sparkle.transform.Rotate(Vector3.up, 45 * spinSpeedBonus * dt, Space.Self); //Spin the sparkle :3

                if(chargeLastUpdate < 1f && charge >= 1f)
                {
                    //Play the sparkle particles when the charge is full.
                    sparkleParticles.Play();
                }

                chargeLastUpdate = charge;
            }
            else
            {
                chargeLastUpdate = 0f;
                sparkle.transform.localScale = Vector3.MoveTowards(sparkle.transform.localScale, Vector3.zero, 0.2f * dt);
                sparkle.transform.Rotate(Vector3.up, 15 * dt, Space.Self); //Spin the sparkle :3
            }
        }
    }
}
