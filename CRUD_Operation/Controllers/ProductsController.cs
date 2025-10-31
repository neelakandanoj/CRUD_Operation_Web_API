using CRUD_Operation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CRUD_Operation.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly string connectionString;

        

        public ProductsController(IConfiguration configuration)
        {
            connectionString = configuration["ConnectionStrings:SQLServerDB"];
            //connectionString = configuration["ConnectionStrings:SQLServerDB"] ?? "";

        }

        [Authorize]
        [HttpPost("CreateProduct")]
        public IActionResult CreateProduct(ProductDto product) 
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "Insert into Products (name,brand,category,price,description) values "+
                    "(@name,@brand,@category,@price,@description)";

                    using (var cmd=new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", product.Name);
                        cmd.Parameters.AddWithValue("@brand", product.Brand);
                        cmd.Parameters.AddWithValue("@category", product.Category);  
                        cmd.Parameters.AddWithValue("@price", product.Price);
                        cmd.Parameters.AddWithValue("@description", product.Description);

                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                return Ok(new { Success = true, message="Product Inserted Successfully." });
            }
            catch (Exception e) 
            {
                var errorResponse = new
                {
                    Message = "An error occurred while creating the product.",
                    Details = e.Message // Optionally include exception details for debugging
                };

                return BadRequest(errorResponse);
            }
        }

        [HttpGet("GetProduct")]
        public IActionResult GetAllProduct() 
        {
            try
            {
                List<ProductResponse> products = new List<ProductResponse>();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "Select * from products";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductResponse product = new ProductResponse();
                                product.Id = reader.GetInt32(0);
                                product.Name = reader.GetString(1);
                                product.Brand = reader.GetString(2);
                                product.Category = reader.GetString(3);
                                product.Price = reader.GetDecimal(4);
                                product.Description = reader.GetString(5);
                                product.CreatedAt = reader.GetDateTime(6);
                                products.Add(product);
                            }
                        }
                        conn.Close();
                    }
                }
                return Ok(products);
            }
            catch(Exception e)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while Getting the product.",
                    Details = e.Message // Optionally include exception details for debugging
                };

                return BadRequest(errorResponse);
            }
            
        }

        [HttpGet("GetProductByID/{id}")]
        public IActionResult GetProductByID(int id)
        {
            try
            {
                List<ProductResponse> Response = new List<ProductResponse>();
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("USP_GetProductDetailsByID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                { 
                                    ProductResponse response = new ProductResponse();
                                    response.Id = Convert.ToInt32(dr["id"]);
                                    response.Name = dr["name"].ToString();
                                    response.Brand = dr["brand"].ToString();
                                    response.Price = Convert.ToDecimal(dr["price"]);
                                    response.Category = dr["category"].ToString();
                                    response.Description = dr["description"].ToString();
                                    Response.Add(response);
                                }
                            }
                            conn.Close();
                        }
                    }
                }
                return Ok(Response);
            }
            catch (Exception e)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while getting the product.",
                    Details = e.Message
                };
                return BadRequest(errorResponse);
            }

           
        }
        [HttpPut("UpdateProduct/id={id}")]
        public IActionResult UpdateProduct(int id,ProductDto product)
        {
            try
            {
                using(var conn=new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "Update products set name=@name,brand=@brand,category=@category,price=@price,description=@description WHere id=@id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", product.Name);
                        cmd.Parameters.AddWithValue("@brand", product.Brand);
                        cmd.Parameters.AddWithValue("@category", product.Category);
                        cmd.Parameters.AddWithValue("@price", product.Price);
                        cmd.Parameters.AddWithValue("@description", product.Description);
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while updating the product.",
                    Details = e.Message
                };
                return BadRequest(errorResponse);
            }
        }

        [HttpDelete("DeleteProduct/id={id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "Delete from Products WHere id=@id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while Delete the product.",
                    Details = e.Message
                };
                return BadRequest(errorResponse);
            }
        }

        [HttpPost("CreateProduct1")]
        public async Task<IActionResult> CreateProductAsync(ProductDto product)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync(); // Open the connection asynchronously

                    string sql = "INSERT INTO Products (name, brand, category, price, description) " +
                                 "VALUES (@name, @brand, @category, @price, @description)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", product.Name);
                        cmd.Parameters.AddWithValue("@brand", product.Brand);
                        cmd.Parameters.AddWithValue("@category", product.Category);
                        cmd.Parameters.AddWithValue("@price", product.Price);
                        cmd.Parameters.AddWithValue("@description", product.Description);

                        await cmd.ExecuteNonQueryAsync(); // Execute the query asynchronously
                    }
                }

                return Ok(new { Success = true, Message = "Product inserted successfully." });
            }
            catch (Exception e)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while creating the product.",
                    Details = e.Message // Optionally include exception details for debugging
                };

                return BadRequest(errorResponse);
            }
        }


    }
}
