using UnityEngine;

public class WordFishColor
{
    public Color bodyColor;
    public Color finColor;

    public WordFishColor(Color bodyColor, Color finColor)
    {
        this.bodyColor = bodyColor;
        this.finColor = finColor;
    }

    public WordFishColor() { }
    public WordFishColor(WordFishColor color)
    {
        this.bodyColor = color.bodyColor;
        this.finColor = color.finColor;
    }
}