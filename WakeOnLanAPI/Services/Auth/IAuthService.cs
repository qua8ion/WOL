using Dto.Services.Auth;

namespace Services.Auth;

public interface IAuthService
{
    /// <summary>
    /// Авторизация по логину и паролю
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<LoginResponseModel?> AuthAsync(AuthModel model);

    /// <summary>
    /// Проверка JWT токена на валидность
    /// </summary>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    TokenValidStateModel ValidateJwtToken(string bearerToken);
}