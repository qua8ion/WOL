using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DbData.Entities;
using Dto.AppSettings;
using Dto.Services.Auth;
using Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Crud.Base;
using Services.DbTransactions.Abstracts;
using Services.User.SystemScopeAccessor;

namespace Services.Auth;

public class AuthService : IAuthService
{
    private readonly ICrudService<DbData.Entities.User> _userCrudSrv;
    private readonly IOptions<JwtAppSettings> _jwtAppSettings;
    private readonly ITransactionScopeFactory _scopeFactory;
    private readonly ISystemUserScopeAccessor _accessor;

    public AuthService(ICrudService<DbData.Entities.User> userCrudSrv, IOptions<JwtAppSettings> jwtAppSettings, ITransactionScopeFactory scopeFactory, ISystemUserScopeAccessor accessor)
    {
        _userCrudSrv = userCrudSrv;
        _jwtAppSettings = jwtAppSettings;
        _scopeFactory = scopeFactory;
        _accessor = accessor;
    }

    /// <summary>
    /// Авторизация по логину и паролю
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<LoginResponseModel?> AuthAsync(AuthModel model)
    {
        using var scope = _scopeFactory.Create();

        DbData.Entities.User? user;

        using (_accessor.Create())
        {
            user = await _userCrudSrv.GetAll(scope)
                .Where(u => u.UserName == model.UserName && u.Password == model.Password).FirstOrDefaultAsync();
        }

        if (user == null) return null;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.UserData, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Admin.ToString()),
        };

        // создаем JWT-токен
        var jwt = new JwtSecurityToken(
            issuer: _jwtAppSettings.Value.Issuer,
            audience: _jwtAppSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(_jwtAppSettings.Value.ExpireMinutes)),
            signingCredentials: new SigningCredentials(_jwtAppSettings.Value.Key.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new LoginResponseModel()
        {
            AccessToken = encodedJwt,
            Username = user.UserName,
        };
    }

    /// <summary>
    /// Проверка JWT токена на валидность
    /// </summary>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public TokenValidStateModel ValidateJwtToken(string bearerToken)
    {
        if (string.IsNullOrEmpty(bearerToken))
        {
            return new TokenValidStateModel { AccessToken = bearerToken, Valid = false };
        }

        try
        {
            var token = bearerToken.Split(' ')[1];

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtAppSettings.Value.Key.GetSymmetricSecurityKey(),
                ValidateIssuer = _jwtAppSettings.Value.ValidateIssuer,
                ValidateAudience = _jwtAppSettings.Value.ValidateAudience,
                ValidAudience = _jwtAppSettings.Value.Audience,
                ValidIssuer = _jwtAppSettings.Value.Issuer,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, tokenParams, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            // Токен валиден
            return new TokenValidStateModel { AccessToken = bearerToken, Valid = true };
        }
        catch
        {
            // Токен недействителен
            return new TokenValidStateModel { AccessToken = bearerToken, Valid = false };
        }
    }

}