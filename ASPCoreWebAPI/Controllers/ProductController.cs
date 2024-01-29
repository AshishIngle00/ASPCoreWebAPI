using ASPCoreWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace ASPCoreWebAPI.Controllers
{

    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("api/[Controller]/GetAllProducts")]
        [HttpGet]
        public IEnumerable<mdl_Product> GetAllProducts()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpHandleProducts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@iHandle", "GetProducts");

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<mdl_Product> products = new List<mdl_Product>();

                            while (reader.Read())
                            {
                                mdl_Product product = new mdl_Product
                                {
                                    P_id = Convert.ToInt32(reader["P_id"]),
                                    P_Name = reader["P_Name"].ToString(),
                                    P_Price = Convert.ToDecimal(reader["P_Price"])
                                };

                                products.Add(product);
                            }

                            return products;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Exception: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        [Route("api/[Controller]/CreateProduct")]
        [HttpPost]
        public IActionResult CreateProduct([FromBody] mdl_Product request)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpHandleProducts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@iHandle", "CreateProduct");
                        command.Parameters.AddWithValue("@iP_Name", request.P_Name);
                        command.Parameters.AddWithValue("@iP_Price", request.P_Price);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected <=0)
                        {
                            return BadRequest("Failed to insert record.");
                           
                        }
                        return Ok("Record inserted successfully.");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Route("api/[Controller]/DeleteProduct/{P_id}")]
        [HttpDelete]
        public IActionResult DeleteProduct(int P_id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpHandleProducts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@iHandle", "DeleteProduct");
                        command.Parameters.AddWithValue("@iP_id", P_id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected <= 0)
                        {
                            return BadRequest("Failed to delete record.");
                        }

                        return Ok("Record deleted successfully.");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Route("api/[Controller]/GetProductById/{P_id}")]
        [HttpGet]
        public IActionResult GetProductById(int P_id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpHandleProducts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@iHandle", "GetProductById");
                        command.Parameters.AddWithValue("@iP_id", P_id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                mdl_Product product = new mdl_Product
                                {
                                    P_id = Convert.ToInt32(reader["P_id"]),
                                    P_Name = reader["P_Name"].ToString(),
                                    P_Price = Convert.ToDecimal(reader["P_Price"])
                                };

                                return Ok(product);
                            }
                            else
                            {
                                // No record found
                                return NotFound("Product not found.");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Route("api/[Controller]/UpdateProduct/{P_id}")]
        [HttpPut]
        public IActionResult UpdateProduct(int P_id, [FromBody] mdl_Product request)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SpHandleProducts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@iHandle", "UpdateProductById");
                        command.Parameters.AddWithValue("@iP_id", P_id);
                        command.Parameters.AddWithValue("@iP_Name", request.P_Name);
                        command.Parameters.AddWithValue("@iP_Price", request.P_Price);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected <= 0)
                        {
                            return BadRequest("Failed to update record.");
                        }

                        return Ok("Record updated successfully.");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
