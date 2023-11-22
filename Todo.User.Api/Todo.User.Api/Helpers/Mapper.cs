using Todo.User.Api.Models;
using AutoMapper;

namespace Todo.User.Api.Helpers
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<UserDto, UserInfo>();
        }
    }
}
