namespace Application.Interfaces;

public interface IStartupScript
{
    // Execution order number (the lower, the earlier)
    int Order => 0;
    
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
