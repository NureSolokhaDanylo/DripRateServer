namespace Application.Interfaces;

public interface IStartupScript
{
    // Порядковый номер выполнения (чем меньше, тем раньше)
    int Order => 0;
    
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
