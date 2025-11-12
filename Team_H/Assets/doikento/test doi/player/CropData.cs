using UnityEngine;

[System.Serializable]
public class CropData
{
    public string cropName;         // ì•¨–¼
    public Sprite seedSprite;       // í‚Ü‚«‚ÌŒ©‚½–Ú
    public Sprite growingSprite;    // ¬’·“r’†‚ÌŒ©‚½–Ú
    public Sprite grownSprite;      // ûŠn‚ÌŒ©‚½–Ú
    public float growTime = 5f;     // ”­‰è‚Ü‚Å‚ÌŠÔ
    public float fullGrowTime = 5f; // ¬n‚Ü‚Å‚ÌŠÔ
    public int harvestPoints = 10;  // ûŠn‚ÌƒXƒRƒA
    public int expGain = 10;        // ¬’·EûŠn‚ÌŒoŒ±’l
}