using UnityEngine;

namespace SpellHand
{
    public class SpellHandController : MonoBehaviour
    {
        private GameObject ringModel1;
        private GameObject ringModel2;

        private RingController Ring1;
        private RingController Ring2;

        private Animation handAnimation;
        private Animation fingersAnimation;

        private AnimationClip Fingers_Magic_Equip;
        private AnimationClip Fingers_Magic_Idle;
        private AnimationClip[] Fingers_Magic_Cast_Anims;


        private AnimationClip Hand_Magic_Equip;
        private AnimationClip Hand_Magic_Idle;
        private AnimationClip[] Hand_Magic_Cast_Anims;

        public CONTROL CON;

        private void Awake()
        {
            ringModel1 = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject;
            ringModel2 = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetChild(1).gameObject;

            handAnimation = transform.GetChild(0).GetChild(0).GetComponent<Animation>();
            fingersAnimation = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Animation>();
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

            //I know this is a mess, but it works. :)
            Magic_scr eq1 = CON.EQ_MAG2;
            Magic_scr eq2 = CON.EQ_MAG1;
            Magic_scr temp = null;

            if (CON.CURRENT_SYS_DATA.SETT_LEFT_HAND > 0)
            {
                temp = eq1;
                eq1 = eq2;
                eq2 = temp;
            }

            if (SpellHandPlugin.Instance.FlipRingPositions.Value)
            {
                temp = eq1;
                eq1 = eq2;
                eq2 = temp;
            }

            Ring1 = new RingController(this, ringModel1, eq1);
            Ring2 = new RingController(this, ringModel2, eq2);

            Ring1.Initialize();
            Ring2.Initialize();

            //Load animations
            Fingers_Magic_Idle = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Fingers_Magic_Idle));
            Fingers_Magic_Equip = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Fingers_Magic_Equip));

            //There are 3 finger animations for casting spells.
            int anims = 3;
            Fingers_Magic_Cast_Anims = new AnimationClip[anims];
            for (int i = 0; i < anims; i++)
            {
                Fingers_Magic_Cast_Anims[i] = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>($"Fingers_Magic_Cast{i}");
            }

            Hand_Magic_Idle = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Hand_Magic_Idle));
            Hand_Magic_Equip = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>(nameof(Hand_Magic_Equip));

            //There are 4 hand animations for casting spells.
            int handAnims = 4;
            Hand_Magic_Cast_Anims = new AnimationClip[handAnims];
            for (int i = 0; i < handAnims; i++)
            {
                Hand_Magic_Cast_Anims[i] = SpellHandPlugin.Instance.AssetLoader.LoadAsset<AnimationClip>($"Hand_Magic_Cast{i}");
            }

            handAnimation.clip = Hand_Magic_Equip;
            handAnimation.Play();

            fingersAnimation.clip = Fingers_Magic_Equip;
            fingersAnimation.Play();

            Patches.OnMagicScrCast += Patches_OnMagicScrCast;
        }

        //Invoked when a spell is cast.
        private void Patches_OnMagicScrCast(Magic_scr obj)
        {
            CancelInvoke(nameof(ResetHandAnimation));
            CancelInvoke(nameof(ResetFingerAnimation));

            RingController ring = Ring1.BoundSpell == obj ? Ring1 : Ring2;

            AnimationClip handClip = GetClip(ring.HandAnimationIndex, Hand_Magic_Cast_Anims);
            handAnimation.Stop();
            handAnimation.clip = handClip;
            handAnimation.Play();
            Invoke(nameof(ResetHandAnimation), handClip.length);

            AnimationClip fingerClip = GetClip(ring.FingersAnimationIndex, Fingers_Magic_Cast_Anims);
            fingersAnimation.Stop();
            fingersAnimation.clip = fingerClip;
            fingersAnimation.Play();
            Invoke(nameof(ResetFingerAnimation), fingerClip.length);
        }

        //Returns clamped index or random clip if index is below 0.
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
            fingersAnimation.Stop();
            fingersAnimation.clip = Fingers_Magic_Idle;
            fingersAnimation.Play();
        }

        private void ResetHandAnimation()
        {
            handAnimation.Stop();
            handAnimation.clip = Hand_Magic_Idle;
            handAnimation.Play();
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
}
