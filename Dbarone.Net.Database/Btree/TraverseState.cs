using Dbarone.Net.Database;

public class TraverseState<T>
{
    public TraverseState(T startingState)
    {
        this.State = startingState;
    }


    public TraverseState()
    {
        this.State = default!;
    }

    public T? State { get; set; }
    public Page Node { get; set; } = default!;
    public int NodeCounter { get; set; }
    public int IndexInNode { get; set; }
}