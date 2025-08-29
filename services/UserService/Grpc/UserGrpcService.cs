using Database.DbContext;
using Grpc.Core;
using Microservice.Communication.GRPC.UserServiceProtos.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UserService.DBContext;
using UserService.Models;
using user = Microservice.Communication.GRPC.UserServiceProtos.Protos;
namespace UserService.Grpc
{
    [AllowAnonymous]
    public class UserGrpcService : user.UserGrpcService.UserGrpcServiceBase
    {
        private readonly UserDbContext _db;
        private readonly ILogger<UserGrpcService> _logger;
        public UserGrpcService(UserDbContext db, ILogger<UserGrpcService> logger) { _db = db; _logger = logger; }

        public override async Task<CreateUserReply> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            try
            {

                var existing = await _db.user.FirstOrDefaultAsync(u => u.Auth0Id == request.Auth0Id);
                if (existing != null)
                {
                    return new CreateUserReply
                    {

                        UserId = existing.Id,
                        FullName = existing.Name,
                        Email = existing.Email,
                    };
                }
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Auth0Id = request.Auth0Id,
                    Email = request.Email,
                    Name = request.FullName,
                    Nickname = request.Nickname,
                    Picture = request.PictureUrl,
                    EmailVerified = request.EmailVerified,
                    CreatedAt = DateTime.UtcNow
                };
                _db.user.Add(user);
                await _db.SaveChangesAsync();
                return new CreateUserReply
                {

                    UserId = user.Id,
                    FullName = request.FullName,
                    Email = request.Email,
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in CreateUser");
                throw;
            }
        }
    }
}

