/// \file
/// Represents the Health System Game Bar System
/// @author: Chase Hutchens

using UnityEngine;

/// <summary>
/// This Class extends the ScrollBarEssentials, simulating the behavior for a HealthSystem Game Bar
/// </summary>
class HealthSystem : ScrollBarEssentials
{
    /// <summary>
    /// Creates a ScrollBarEssentials HealthSystem Game Bar
    /// </summary>
    /// <param name="sb_dimen">Dimensions of the Scroll Bar: x,y,w,h</param>
    /// <param name="vbar">Is this a Verticle Bar?</param>
    /// <param name="sb_bt">The Scroll Bar Bubble Texture</param>
    /// <param name="st">The Scroll Texture</param>
    /// <param name="rot">The Rotation</param>
    public HealthSystem(Rect sb_dimen, bool vbar, Texture sb_bt, Texture st, float rot) : base(sb_dimen, vbar, sb_bt, st, rot)
    {
        
    }

    /// <summary>
    /// Initializes the HealthSystem with a current value and max value
    /// </summary>
    public void Initialize()
    {
        current_value = 200;
        max_value = 200;
    }

    /// <summary>
    /// Prevents the current value from exceeding the min and max value. Updates the position of the scroll bar to stay at the x and y locations
    /// </summary>
    /// <param name="x">The X screen location</param>
    /// <param name="y">The Y screen location</param>
    public void Update(int x, int y)
    {
        if (current_value < 0)
            current_value = 0;
        else if (current_value >= max_value)
            current_value = max_value;

        ScrollBarDimens = new Rect(x - ScrollBarDimens.width / 2, y - ScrollBarDimens.height, ScrollBarDimens.width, ScrollBarDimens.height);
        pivotVector.x = ScrollBarDimens.x + (ScrollBarDimens.width / 2);
        pivotVector.y = ScrollBarDimens.y + (ScrollBarDimens.height / 2);
    }

    /// <summary>
    /// Incriments or De-incriments the current value
    /// </summary>
    /// <param name="value">The value to change the current value by</param>
    public override void IncrimentBar(int value)
    {
        ProcessValue(value);
    }
}
