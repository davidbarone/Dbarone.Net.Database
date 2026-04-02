using Dbarone.Net.Database;

public class TraverseParameters<T>
{
    public TraverseParameters(T startingState)
    {
        this.State = startingState;
    }


    public TraverseParameters()
    {
        this.State = default!;
    }

    public T? State { get; set; }
    public Page Node { get; set; } = default!;
    public int NodeCounter { get; set; }
    public int IndexInNode { get; set; }
}