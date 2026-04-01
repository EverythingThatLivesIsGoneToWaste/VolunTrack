using System.Security.Claims;
using VolunTrack.Repositories;
using VolunTrack.Services;

namespace VolunTrack.Middlewares
{
    public class UserStatusMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

        public async Task InvokeAsync(HttpContext context, IUserRepository userRepo, ILoginService loginService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var user = await userRepo.GetByIdAsync(userId);

                    if (user == null || !user.IsActive)
                    {
                        await loginService.LogoutAsync();

                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsJsonAsync(new { reason = "inactive" });
                            return;
                        }

                        context.Response.Redirect("/Login?reason=inactive");
                        return;
                    }

                    var roleFromDb = user.Role.ToString();
                    var roleFromClaim = context.User.FindFirstValue(ClaimTypes.Role);

                    if (roleFromDb != roleFromClaim)
                    {
                        await loginService.LogoutAsync();
                        context.Response.Redirect("/Login?reason=role-changed");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
