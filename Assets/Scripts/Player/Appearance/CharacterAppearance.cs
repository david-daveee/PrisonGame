using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterAppearance : MonoBehaviour
{
    [Serializable]
    public sealed class CharacterSet
    {
        [Header("Character")]
        [Tooltip("Root object for this body type.")]
        public GameObject characterRoot;

        [Header("Clothes")]
        public GameObject[] outfitVariants = Array.Empty<GameObject>();
        public GameObject[] pantsVariants = Array.Empty<GameObject>();
        public GameObject[] shoesVariants = Array.Empty<GameObject>();

        [Header("Head")]
        public GameObject[] hairVariants = Array.Empty<GameObject>();
        public GameObject[] beardVariants = Array.Empty<GameObject>();
        public GameObject[] eyebrowVariants = Array.Empty<GameObject>();

        [Header("Accessories")]
        public GameObject[] headAccessoryVariants = Array.Empty<GameObject>();
        public GameObject[] neckAccessoryVariants = Array.Empty<GameObject>();
    }

    [Header("Character sets")]
    [SerializeField] private CharacterSet[] characterSets = Array.Empty<CharacterSet>();

    [Header("Current appearance (0 = first variant)")]
    [Min(0), SerializeField] private int characterId;
    [Min(0), SerializeField] private int outfitId;
    [Min(0), SerializeField] private int pantsId;
    [Min(0), SerializeField] private int shoesId;
    [Min(0), SerializeField] private int hairId;
    [Min(0), SerializeField] private int beardId;
    [Min(0), SerializeField] private int eyebrowId;
    [Min(0), SerializeField] private int headAccessoryId;
    [Min(0), SerializeField] private int neckAccessoryId;

    [SerializeField, HideInInspector] private int setupVersion;

    public int CharacterId => characterId;
    public int OutfitId => outfitId;
    public int PantsId => pantsId;
    public int ShoesId => shoesId;
    public int HairId => hairId;
    public int BeardId => beardId;
    public int EyebrowId => eyebrowId;
    public int HeadAccessoryId => headAccessoryId;
    public int NeckAccessoryId => neckAccessoryId;
    public CharacterSet[] CharacterSets => characterSets;

    public int SetupVersion
    {
        get => setupVersion;
        set => setupVersion = value;
    }

    private void Start()
    {
        ApplyAppearance();
    }

    private void OnValidate()
    {
        ClampIdsOnly();
    }

    // Kept for existing UI/network code.
    public void SetAppearance(
        int newCharacterId,
        int newOutfitId,
        int newHairId,
        int newBeardId,
        int newEyebrowId)
    {
        characterId = newCharacterId;
        outfitId = newOutfitId;
        hairId = newHairId;
        beardId = newBeardId;
        eyebrowId = newEyebrowId;
        ApplyAppearance();
    }

    public void SetAppearance(
        int newCharacterId,
        int newOutfitId,
        int newPantsId,
        int newShoesId,
        int newHairId,
        int newBeardId,
        int newEyebrowId,
        int newHeadAccessoryId,
        int newNeckAccessoryId)
    {
        characterId = newCharacterId;
        outfitId = newOutfitId;
        pantsId = newPantsId;
        shoesId = newShoesId;
        hairId = newHairId;
        beardId = newBeardId;
        eyebrowId = newEyebrowId;
        headAccessoryId = newHeadAccessoryId;
        neckAccessoryId = newNeckAccessoryId;
        ApplyAppearance();
    }

    public void SetCharacter(int value) { characterId = value; ApplyAppearance(); }
    public void SetOutfit(int value) { outfitId = value; ApplyAppearance(); }
    public void SetPants(int value) { pantsId = value; ApplyAppearance(); }
    public void SetShoes(int value) { shoesId = value; ApplyAppearance(); }
    public void SetHair(int value) { hairId = value; ApplyAppearance(); }
    public void SetBeard(int value) { beardId = value; ApplyAppearance(); }
    public void SetEyebrows(int value) { eyebrowId = value; ApplyAppearance(); }
    public void SetHeadAccessory(int value) { headAccessoryId = value; ApplyAppearance(); }
    public void SetNeckAccessory(int value) { neckAccessoryId = value; ApplyAppearance(); }

    [ContextMenu("Apply appearance now")]
    public void ApplyAppearance()
    {
        if (characterSets == null || characterSets.Length == 0)
            return;

        ClampIdsOnly();
        CharacterSet selectedSet = characterSets[characterId];
        if (selectedSet == null)
            return;

        for (int i = 0; i < characterSets.Length; i++)
        {
            CharacterSet set = characterSets[i];
            if (set == null)
                continue;

            if (set.characterRoot != null)
                SetActiveIfNeeded(set.characterRoot, set.characterRoot == selectedSet.characterRoot);

            DisableVariants(set.outfitVariants);
            DisableVariants(set.pantsVariants);
            DisableVariants(set.shoesVariants);
            DisableVariants(set.hairVariants);
            DisableVariants(set.beardVariants);
            DisableVariants(set.eyebrowVariants);
            DisableVariants(set.headAccessoryVariants);
            DisableVariants(set.neckAccessoryVariants);
        }

        outfitId = ApplyVariant(selectedSet.outfitVariants, outfitId);
        pantsId = ApplyVariant(selectedSet.pantsVariants, pantsId);
        shoesId = ApplyVariant(selectedSet.shoesVariants, shoesId);
        hairId = ApplyVariant(selectedSet.hairVariants, hairId);
        beardId = ApplyVariant(selectedSet.beardVariants, beardId);
        eyebrowId = ApplyVariant(selectedSet.eyebrowVariants, eyebrowId);
        headAccessoryId = ApplyVariant(selectedSet.headAccessoryVariants, headAccessoryId);
        neckAccessoryId = ApplyVariant(selectedSet.neckAccessoryVariants, neckAccessoryId);
    }

    private void ClampIdsOnly()
    {
        characterId = Mathf.Max(0, characterId);
        outfitId = Mathf.Max(0, outfitId);
        pantsId = Mathf.Max(0, pantsId);
        shoesId = Mathf.Max(0, shoesId);
        hairId = Mathf.Max(0, hairId);
        beardId = Mathf.Max(0, beardId);
        eyebrowId = Mathf.Max(0, eyebrowId);
        headAccessoryId = Mathf.Max(0, headAccessoryId);
        neckAccessoryId = Mathf.Max(0, neckAccessoryId);

        if (characterSets != null && characterSets.Length > 0)
            characterId = Mathf.Min(characterId, characterSets.Length - 1);
    }

    private static int ApplyVariant(GameObject[] variants, int selectedId)
    {
        if (variants == null || variants.Length == 0)
            return 0;

        int clampedId = Mathf.Clamp(selectedId, 0, variants.Length - 1);

        for (int i = 0; i < variants.Length; i++)
        {
            if (variants[i] != null)
                SetActiveIfNeeded(variants[i], i == clampedId);
        }

        return clampedId;
    }

    private static void DisableVariants(GameObject[] variants)
    {
        if (variants == null)
            return;

        foreach (GameObject variant in variants)
        {
            if (variant != null)
                SetActiveIfNeeded(variant, false);
        }
    }

    private static void SetActiveIfNeeded(GameObject target, bool active)
    {
        if (target.activeSelf != active)
            target.SetActive(active);
    }
}
