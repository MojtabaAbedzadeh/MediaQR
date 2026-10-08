using MediaQR.Application.DTOs.Dashboard;

namespace MediaQR.Application.Interfaces;

public interface IUserDashboardService
{
    Task<UserDashboardDto> GetDashboardAsync(
        CancellationToken cancellationToken = default);
}