using Application.Dtos;
using Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Metrics;

internal sealed class GetAarrrMetricsQueryHandler : IRequestHandler<GetAarrrMetricsQuery, ErrorOr<AarrrMetricsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAarrrMetricsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AarrrMetricsResponse>> Handle(GetAarrrMetricsQuery request, CancellationToken cancellationToken)
    {
        // 1. ACQUISITION (Залучення)
        // Рахуємо загальну кількість зареєстрованих користувачів
        var totalUsers = await _context.Users.CountAsync(cancellationToken);

        // 2. ACTIVATION (Активація)
        // Користувачі, які зробили хоча б одну цільову дію (створили публікацію або залишили оцінку)
        var activeUsers = await _context.Users
            .Where(u => _context.Publications.Any(p => p.UserId == u.Id) || _context.Assessments.Any(a => a.UserId == u.Id))
            .CountAsync(cancellationToken);

        // 3. RETENTION (Утримання)
        // Користувачі, які повернулися і залишили оцінку більше ніж через 7 днів після своєї реєстрації
        var retainedUsers = await _context.Users
            .Where(u => _context.Assessments.Any(a => a.UserId == u.Id && a.CreatedAt > u.CreatedAt.AddDays(7)))
            .CountAsync(cancellationToken);

        // 4. REFERRAL (Віральність)
        // В якості метрики Referral беремо кількість підписок один на одного (Follows), як внутрішню рекомендацію
        var totalFollows = await _context.Follows.CountAsync(cancellationToken);

        // 5. REVENUE (Монетизація)
        // Кількість переглядів рекламних інтеграцій у додатку
        var totalAdViews = await _context.AdvertisementViews.CountAsync(cancellationToken);

        return new AarrrMetricsResponse(
            totalUsers,
            activeUsers,
            retainedUsers,
            totalFollows,
            totalAdViews
        );
    }
}
