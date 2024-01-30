using ASPCoreWebAPI.DAL;
using ASPCoreWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using System.Text;

namespace ASPCoreWebAPI.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _tokenService;
        private DBHelper dao;

        public AuthController(IConfiguration configuration, JwtService tokenService)
        {
            _configuration = configuration;
            dao = new DBHelper(_configuration);
            _tokenService = tokenService;
        }

        [Route("[Controller]/Login")]
        [HttpPost]
        public async Task<IActionResult> UserLogin([FromBody] Mdl_User user)
        {
            int nReturn = 0;
            string strMsg = string.Empty, empFullName = string.Empty;
            long empId = 0, result = 0;
            dynamic Authtoken = Empty;
            if (user != null)
                try
                {
                    using (SqlConnection connection = dao.GetConnection())
                    {
                        using (SqlCommand cmd = new SqlCommand("spLogin", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@iUserName", user.Username);
                            cmd.Parameters.AddWithValue("@iPassword", user.Password);
                            cmd.Parameters.AddWithValue("@returnVal", 0).Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add("@oEmpName", SqlDbType.VarChar, 35).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@oEmpId", SqlDbType.BigInt, 35).Direction = ParameterDirection.Output;

                            await cmd.ExecuteNonQueryAsync();

                            result = Convert.ToInt64(cmd.Parameters["@returnVal"].Value);

                            if (result > 0)
                            {
                                nReturn = 1;
                                empId = Convert.ToInt64(cmd.Parameters["@oEmpId"].Value);
                                empFullName = cmd.Parameters["@oEmpName"].Value.ToString();
                                strMsg = "Login Success";
                                Authtoken = _tokenService.GenerateToken(user.Username);
                            }
                            else
                            {
                                return Unauthorized(new
                                {
                                    RetVal = -1,
                                    Msg = "Invalid Username or Password."
                                });
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    nReturn = -2;
                    strMsg = "Exception: " + e.Message;
                }
            return Ok(new
            {
                RetVal = nReturn,
                EmpId = empId,
                EmpFullName = empFullName,
                Msg = strMsg,
                Authtoken = Authtoken
            });
        }
    }
}
