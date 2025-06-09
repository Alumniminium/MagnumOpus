namespace MagnumOpus.Enums;

public enum MsgDialogType : byte
{
    /// <summary>
    /// Adds text to the chat window. This does not equal a line.
    /// A line is around 40-50 characters long
    /// </summary>
    Text = 1,
    /// <summary>
    /// Adds clickable text to the chat window. This equals column in the grid.
    /// The grid is 2 columns 4 lines 
    Link = 2,
    /// <summary>
    /// Adds a box to the chat window. This equals a column in the grid.
    /// The grid is 2 columns 4 lines 
    /// </summary>
    InputBox = 3,
    /// <summary>
    /// Adds a face to the chat window. Not entirely sure where the faces come from. 
    /// </summary>
    Face = 4,
    /// <summary>
    /// Unknown
    /// </summary>
    AlternateLink = 5,
    /// <summary>
    /// Marks the end of the chat window. We send this when we added all the data we wanted to send.
    /// </summary>
    End = 100
}