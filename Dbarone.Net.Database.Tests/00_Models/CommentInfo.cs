namespace Dbarone.Net.Database;

public class CommentInfo
{
    public int CommentId { get; set; }
    public string Comment { get; set; } = default!;
    public CommentInfo(int commentId, string comment)
    {
        this.CommentId = commentId;
        this.Comment = comment;
    }
    public CommentInfo() { }
}