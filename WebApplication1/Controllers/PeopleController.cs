using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly restdemoContext _context;
        private readonly IConfiguration _configuration;
        private string _connectionString;


        public PeopleController(restdemoContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/People
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPeople()
        {
            //return await _context.People.ToListAsync();

            return await _context.People.FromSqlRaw("GetPeople").ToListAsync();
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            //var person = await _context.People.FindAsync(id);

            //if (person == null)
            //{
            //    return NotFound();
            //}

            //return person;

            Person person;

            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetPerson";
                command.Parameters.AddWithValue("@ID", id);
                var reader = await command.ExecuteReaderAsync();

                // get details and add to person
                if (reader.Read())
                {
                    person = new Person();
                    person.Id = Convert.ToInt32(reader["ID"]);
                    person.FirstName = reader["FirstName"].ToString();
                    person.LastName = reader["LastName"].ToString();
                    person.Age = Convert.ToInt32(reader["Age"]);
                    reader.Close();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
            finally
            {
                connection.Close();
            }

            return person;
        }

        // PUT: api/People/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, Person person)
        {
            //if (id != person.Id)
            //{
            //    return BadRequest();
            //}

            //_context.Entry(person).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!PersonExists(id))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            //return NoContent();

            if (id != person.Id)
            {
                return BadRequest();
            }

            SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "UpdatePerson";
                command.Parameters.AddWithValue("@ID", id);
                command.Parameters.AddWithValue("@FirstName", person.FirstName);
                command.Parameters.AddWithValue("@LastName", person.LastName);
                command.Parameters.AddWithValue("@Age", person.Age);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            finally
            {
                connection.Close();
            }

            return NoContent();
        }

        // POST: api/People
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            //_context.People.Add(person);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetPerson", new { id = person.Id }, person);

            Person newPerson;
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "AddPerson";
                command.Parameters.AddWithValue("@FirstName", person.FirstName);
                command.Parameters.AddWithValue("@LastName", person.LastName);
                command.Parameters.AddWithValue("@Age", person.Age);

                await command.ExecuteNonQueryAsync();

                //get the latest person added to display back
                newPerson = new Person();
                var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM People WHERE ID = (SELECT MAX(ID) FROM People)";
                var reader = await cmd.ExecuteReaderAsync();
                reader.Read();
                newPerson.Id = Convert.ToInt32(reader["ID"]);
                newPerson.FirstName = reader["FirstName"].ToString();
                newPerson.LastName = reader["LastName"].ToString();
                newPerson.Age = Convert.ToInt32(reader["Age"]);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            finally
            {
                connection.Close();
            }

            return CreatedAtAction("GetPerson", new { id = newPerson.Id }, newPerson);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            //var person = await _context.People.FindAsync(id);
            //if (person == null)
            //{
            //    return NotFound();
            //}

            //_context.People.Remove(person);
            //await _context.SaveChangesAsync();

            //return NoContent();

            SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "DeletePerson";
                command.Parameters.AddWithValue("@ID", id);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            finally
            {
                connection.Close();
            }

            return NoContent();
        }

        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.Id == id);
        }
    }
}
