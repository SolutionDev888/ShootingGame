/// \file
/// The ScrollBarEssentials utilized from the Game Bar Creation System: http://u3d.as/content/chazix-scripts/game-bar-creation-system/2Es
/// @author: Chase Hutchens

using UnityEngine;

/// <summary>
/// The class that handles the data, base functions and drawing of a Game Bar
/// </summary>
class ScrollBarEssentials
{
    protected GUIStyle style = new GUIStyle();
    protected Vector2 string_size;

    protected Vector2 pivotVector = Vector2.zero;
    protected bool MouseInRect = false;    

    protected int current_value = 0;
    protected int max_value = 0;

    protected Rect ScrollBarDimens = new Rect();
    protected Rect ScrollBarTextureDimens;
    protected bool VerticleBar = false;
    protected float texture_rotation = 0;
    protected Texture ScrollBarBubbleTexture;
    protected Texture ScrollTexture;

    /// <summary>
    /// Creates a ScrollBarEssentials Game Bar System
    /// </summary>
    /// <param name="sb_dimen">Dimensions of the Scroll Bar: x,y,w,h</param>
    /// <param name="vbar">Is this a Verticle Bar?</param>
    /// <param name="sb_bt">The Scroll Bar Bubble Texture</param>
    /// <param name="st">The Scroll Texture</param>
    /// <param name="rot">The Rotation</param>
    public ScrollBarEssentials(Rect sb_dimen, bool vbar, Texture sb_bt, Texture st, float rot)
    {
        ScrollBarDimens         = sb_dimen;
        VerticleBar             = vbar;
        ScrollBarBubbleTexture  = sb_bt;
        ScrollTexture           = st;
        texture_rotation        = rot;

        pivotVector.x = ScrollBarDimens.x + (ScrollBarDimens.width / 2);
        pivotVector.y = ScrollBarDimens.y + (ScrollBarDimens.height / 2);

        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
    }

    /// <summary>
    /// Creates a ScrollBarEssentials HealthSystem Game Bar
    /// </summary>
    /// <param name="sb_dimen">Dimensions of the Scroll Bar: x,y,w,h</param>
    /// <param name="sbv_dimen">Dimensions of the Scroll Texture: x,y,w,h</param>
    /// <param name="vbar">Is this a Verticle Bar?</param>
    /// <param name="sb_bt">The Scroll Bar Bubble Texture</param>
    /// <param name="st">The Scroll Texture</param>
    /// <param name="rot">The Rotation</param>
    public ScrollBarEssentials(Rect sb_dimen, Rect sbv_dimen, bool vbar, Texture sb_bt, Texture st, float rot)
    {
        ScrollBarDimens         = sb_dimen;
        ScrollBarTextureDimens  = sbv_dimen;
        VerticleBar             = vbar;
        ScrollBarBubbleTexture  = sb_bt;
        ScrollTexture           = st;
        texture_rotation        = rot;

        pivotVector.x = ScrollBarDimens.x + (ScrollBarDimens.width / 2);
        pivotVector.y = ScrollBarDimens.y + (ScrollBarDimens.height / 2);

        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
    }

    /// <summary>
    /// Overridable function for determining a max value
    /// </summary>
    protected virtual int DetermineMaxVal(int value)
    {
        // override this formula to anything you wish for your specific ScrollBar needs
        // (Best to use a graphing calculator to determine realistic max_values for your game)

        return value;
    }

    /// <summary>
    /// Function for changing the current_value by a value
    /// </summary>
    protected void ProcessValue(int value)
    {
        current_value += value;
    }

    /// <summary>
    /// Draws the ScrollBarEssentials Game Bar
    /// </summary>
    public virtual void DrawBar()
    {
        Matrix4x4 saved_matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(texture_rotation, pivotVector);

        if (!VerticleBar)
        {
            if (ScrollBarTextureDimens.width != 0 && ScrollBarTextureDimens.height != 0)
                GUI.DrawTexture(new Rect(ScrollBarDimens.x + ScrollBarTextureDimens.x, ScrollBarDimens.y + ScrollBarTextureDimens.y, current_value * (ScrollBarTextureDimens.width / max_value), ScrollBarTextureDimens.height), ScrollTexture);
            else
                GUI.DrawTexture(new Rect(ScrollBarDimens.x, ScrollBarDimens.y, current_value * (ScrollBarDimens.width / max_value), ScrollBarBubbleTexture.height), ScrollTexture);

            for (int i = 0; i < ScrollBarDimens.width / ScrollBarBubbleTexture.width; i++)
                GUI.DrawTexture(new Rect(ScrollBarDimens.x + i * ScrollBarBubbleTexture.width, ScrollBarDimens.y, ScrollBarBubbleTexture.width, ScrollBarBubbleTexture.height), ScrollBarBubbleTexture);
        }
        else
        {
            if (ScrollBarTextureDimens.width != 0 && ScrollBarTextureDimens.height != 0)
                GUI.DrawTexture(new Rect(ScrollBarDimens.x + ScrollBarTextureDimens.x, ScrollBarDimens.y + ScrollBarTextureDimens.y + ScrollBarTextureDimens.height, ScrollBarTextureDimens.width, -current_value * (ScrollBarTextureDimens.height / max_value)), ScrollTexture);
            else
                GUI.DrawTexture(new Rect(ScrollBarDimens.x, ScrollBarDimens.y + ScrollBarDimens.height, ScrollBarBubbleTexture.width, -current_value * (ScrollBarDimens.height / max_value)), ScrollTexture);

            for (int i = 0; i < ScrollBarDimens.height / ScrollBarBubbleTexture.height; i++)
                GUI.DrawTexture(new Rect(ScrollBarDimens.x, ScrollBarDimens.y + i * ScrollBarBubbleTexture.height, ScrollBarBubbleTexture.width, ScrollBarBubbleTexture.height), ScrollBarBubbleTexture);
        }

        if (ScrollBarDimens.Contains(Event.current.mousePosition))
            MouseInRect = true;
        else
            MouseInRect = false;

        if (MouseInRect)
        {
            GUIUtility.RotateAroundPivot(-texture_rotation, pivotVector);
            string_size = style.CalcSize(new GUIContent(current_value + " / " + max_value));
            GUI.Label(new Rect(ScrollBarDimens.x + (ScrollBarDimens.width / 2) - (string_size.x / 2), ScrollBarDimens.y + (ScrollBarDimens.height / 2) - (string_size.y / 2), string_size.x, string_size.y + (string_size.y / 2)), current_value + " / " + max_value, style);
        }

        GUI.matrix = saved_matrix;
    }

    /// <summary>
    /// Incriments or De-incriments the current_value
    /// </summary>
    /// <param name="value">The value to change the current_value by</param>
    public virtual void IncrimentBar(int value)
    {
        ProcessValue(value);
    }

    /// <summary>
    /// The current_value of the Game Bar
    /// </summary>
    public int getCurrentValue()
    {
        int temp_current = current_value;

        return temp_current;
    }

    /// <summary>
    /// The max_value of the Game Bar
    /// </summary>
    public int getMaxValue(int value)
    {
        return DetermineMaxVal(value);
    }

    /// <summary>
    /// The main Scroll Bar Dimensions
    /// </summary>
    public Rect getScrollBarRect()
    {
        return ScrollBarDimens;
    }

    /// <summary>
    /// The Scroll Bar Texture Dimensions
    /// </summary>
    public Rect getScrollBarTextureDimens()
    {
        return ScrollBarTextureDimens;
    }
}