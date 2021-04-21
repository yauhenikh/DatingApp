using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            var users = _context.Users
                                .OrderBy(x => x.UserName)
                                .AsQueryable();
            var likes = _context.Likes
                                .AsQueryable();

            if (predicate == "liked")
            {
                likes = likes.Where(x => x.SourceUserId == userId);
                users = likes.Select(x => x.LikedUser);
            }

            if (predicate == "likedBy")
            {
                likes = likes.Where(x => x.LikedUserId == userId);
                users = likes.Select(x => x.SourceUser);
            }

            return await users.Select(x => new LikeDto
                                           {
                                               Id = x.Id,
                                               Username = x.UserName,
                                               KnownAs = x.KnownAs,
                                               Age = x.DateOfBirth.CalculateAge(),
                                               PhotoUrl = x.Photos.FirstOrDefault(x => x.IsMain).Url,
                                               City = x.City
                                           })
                              .ToListAsync();
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                                 .Include(x => x.LikedUsers)
                                 .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}