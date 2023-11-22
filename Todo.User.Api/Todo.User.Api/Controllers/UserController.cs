using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Todo.User.Api.Interfaces;
using Todo.User.Api.Models;

namespace Todo.User.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IConfiguration configuration;

        public UserController(
            IUserRepository userRepository,
            IMapper mapper,
            IHttpContextAccessor contextAccessor,
            IConfiguration configuration
            )
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.contextAccessor = contextAccessor;
            this.configuration = configuration;
        }

        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<UserInfo> Get()
        {
            var users = userRepository.Get();
            return users;
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public IActionResult Get([FromHeader] string authorizationToken)
        {
            return Ok(DecodeJwtToken(authorizationToken));
        }

        // POST api/<UserController>
        [HttpPost("register")]
        public IActionResult Post([FromBody] UserDto userDto)
        {
            UserInfo userInfo = userRepository.Get(userDto.Login);
            if (userInfo != null)
                return BadRequest("Ошибка! Пользователь найден.");

            var user = mapper.Map<UserInfo>(userDto);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            userRepository.Post(user);

            string token = CreateToken(user);
            return Ok(token);
        }

        [HttpPost("login")]
        public IActionResult Login(UserDto userDto)
        {

            if (userDto.Login == string.Empty || userDto.Password == string.Empty)
                return BadRequest("Incorrect data");

            var user = userRepository.Get(userDto.Login);

            if (userDto.Login != user.Login)
                return BadRequest("User not found.");


            if (!BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
                return BadRequest("Bad password.");


            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken, user);
            string token = CreateToken(user);

            return Ok(token);
        }

        // PUT api/<UserController>/5
        [HttpPut]
        public IActionResult Put([FromBody] UserInfo userInfo)
        {
            var user = userRepository.Get(userInfo.Login);

            if (user == null)
                return BadRequest("User not found.");

            if (user.Login != userInfo.Login)
                return BadRequest("User not found.");

            user.Email = userInfo.Email;

            userRepository.Put(user);

            string token = CreateToken(user);
            return Ok(token);
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            userRepository.Delete(id);
            return Ok();
        }

        [HttpGet("hasExpired")]
        public IActionResult HasTokenExpired(string token)
        {
            var decodedToken = DecodeJwtToken(token);
            if (!decodedToken.ContainsKey("exp"))
            {
                return BadRequest("Invalid Token.");
            }

            long expireDate = Convert.ToInt64(decodedToken["exp"]);

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(expireDate);

            // Convert DateTimeOffset to DateTime if needed
            DateTime dateTime = dateTimeOffset.LocalDateTime;

            if (dateTime.Date > DateTime.Now)
            {
                return Ok(false);
            }

            return Ok(true);
        }

        /// <summary>
        /// Генерирует объект обновления Refresh Token.
        /// </summary>
        /// <returns>Объект Refresh Token.</returns>
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now,
            };
            return refreshToken;
        }

        /// <summary>
        /// Устанавливает Refresh Token для пользователя и создает соответствующий HTTP-кукис.
        /// </summary>
        /// <param name="newRefreshToken">Новый Refresh Token.</param>
        /// <param name="user">Пользователь, для которого устанавливается Refresh Token.</param>
        private void SetRefreshToken(RefreshToken newRefreshToken, UserInfo user)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires,
            };
            contextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.TokenExpires = newRefreshToken.Expires;
            user.TokenCreated = newRefreshToken.Created;
            user.RefreshToken = newRefreshToken.Token;

            userRepository.Put(user);
        }

        /// <summary>
        /// Создает JWT-токен на основе данных пользователя.
        /// </summary>
        /// <param name="user">Пользователь, для которого создается токен.</param>
        private string CreateToken(UserInfo user)
        {
            List<Claim> claims = new()
            {
                new Claim("id", user.Id ?? string.Empty),
                new Claim("name", user.Login),
                new Claim("email", user.Email.ToString()),
            };

            //var tokenSettings = configuration.GetSection("AppSettings:Token").Value;
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings));
            var key = new SymmetricSecurityKey(Encoding.UTF8
              .GetBytes(configuration
              .GetSection("AppSettings:Token").Value!));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: cred
                );
            var result = new JwtSecurityTokenHandler().WriteToken(token);

            return result;

        }

        /// <summary>
        /// Декодирует JWT-токен и возвращает его содержимое как словарь пар "тип-значение".
        /// </summary>
        /// <param name="token">JWT-токен для декодирования.</param>
        /// <returns>Словарь, содержащий пары "тип-значение" из JWT-токена.</returns>
        private static IDictionary<string, string> DecodeJwtToken(string token)
        {

            var jwtHandler = new JwtSecurityTokenHandler();
            var middle = token.Replace("bearer ", "");
            var jwtToken = jwtHandler.ReadJwtToken(middle);

            var claims = new Dictionary<string, string>();
            foreach (var claim in jwtToken.Claims)
            {
                claims.Add(claim.Type, claim.Value);
            }

            return claims;
        }
    }
}
