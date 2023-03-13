using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ONEDaraAPI.MyClass
{
    public class authenHelper
    {
        public string GenerateToken(string strU_ID, string strU_ROLE, string strU_NAME , int intPeriod , string strPeriod)
        {
            // oUser.U_ID = "1";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appconfig.DaraSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", strU_ID), new Claim("role", strU_ROLE), new Claim("uname", strU_NAME) }),
                //Expires = DateTime.UtcNow.AddDays(7),
                Expires = strPeriod == "m"? DateTime.UtcNow.AddMinutes(intPeriod) : strPeriod == "h" ? DateTime.UtcNow.AddHours(intPeriod) : strPeriod == "D" ? DateTime.UtcNow.AddDays(intPeriod) : strPeriod == "M" ? DateTime.UtcNow.AddMonths(intPeriod) : DateTime.UtcNow.AddSeconds(0),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool checkValidToken(string token, string secretKey)
        {
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            string strToken;
            if (token == null || token == "")
                return false;
            else
            {
                string[] arrVal = token.Split(' ');
                if (arrVal.Length != 2)
                {
                    return false;
                }
                else
                {
                    strToken = arrVal[1];
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(secretKey);
                    try
                    {
                        tokenHandler.ValidateToken(strToken, new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                            ClockSkew = TimeSpan.Zero
                        }, out SecurityToken validatedToken);

                        var jwtToken = (JwtSecurityToken)validatedToken;
                        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                        // return user id from JWT token if validation successful
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public int? ValidateToken(string token)
        {
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            if (token == null)
                return -1;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appconfig.DaraSecretKey);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                // return user id from JWT token if validation successful
                return userId;
            }
            catch
            {
                // return null if validation fails
                return -1;
            }
        }
    }
}
