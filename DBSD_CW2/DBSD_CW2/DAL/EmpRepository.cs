using Dapper;
using DBSD_CW2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace DBSD_CW2.DAL
{
    public class EmpRepository : UniversityEmpRepository
    {
        private const string SQL_SELECT = @"select 
                                                    [EmpID],
                                                    [FirstName],
                                                    [LastName],
                                                    [MiddleName],
                                                    [Email],
                                                    [Salary],
                                                    [NumOfWorkingHours],
                                                    [ExperienceYear],
                                                    [DateOfBirth],
                                                    [Photo],
                                                    [CourseId] from Employee";
        private const string SQL_FILTER = @"select {0}
                                            from Employee {1} 
                                            {2}
                                            {3}";
        private const string SQL_FILTER_PAGING = @"select {0}
                                                 from Employee {1} {2}";
        private const string SQL_INSERT = @"
                                            insert into Employee (
	                                            [FirstName],
	                                            [LastName],
                                                [MiddleName],
	                                            [Email],
	                                            [Salary],
	                                            [NumOfWorkingHours],
	                                            [ExperienceYear],
                                                [DateOfBirth],
                                                [Photo],
                                                [CourseId]
                                            ) values (
	                                            @FirstName,
	                                            @LastName,
                                                @MiddleName,
	                                            @Email,
	                                            @Salary,
	                                            @NumOfWorkingHours,
	                                            @ExperienceYear,
                                                @DateOfBirth,
                                                @Photo,
                                                @CourseId
                                            )";
        private const string SQL_SELECT_BY_ID = @"select 
                                                    [EmpID],
                                                    [FirstName],
                                                    [LastName],
                                                    [MiddleName],
                                                    [Email],
                                                    [Salary],
                                                    [NumOfWorkingHours],
                                                    [ExperienceYear],
                                                    [DateOfBirth],
                                                    [Photo],
                                                    [CourseId] from Employee 
                                                 where EmpID = @EmpID";
        private const string SQL_SELECT_BY_ID_FILTERED = @"select e.EmpID, e.FirstName, e.LastName, e.Email, e.Salary, e.NumOfWorkingHours, e.ExperienceYear, p.Phone, c.CourseName, uInfo.NumberOfStudents, d.Name
										from Employee e full outer join Lecturer lec
											on e.EmpID = lec.EmpID 
											full outer join UnderG_Lec ul 
											on lec.EmpID = ul.EmpID
											full outer join UndergraduateLevel uInfo
											on ul.CourseID = uInfo.CourseID
											full outer join Course c 
											on e.CourseID = c.CourseId
											full outer join Department d
											on e.DeptID = d.Id
											full outer join Person p
											on p.EmployeeId = e.EmpID 
                                                 where e.EmpID = @EmpID";
        private const string SQL_UPDATE = @"update employee set                                                                 [FirstName]          = @FirstName
	                                            ,[LastName]          = @LastName
	                                            ,[Email]             = @Email
	                                            ,[Salary]            = @Salary
	                                            ,[NumOfWorkingHours] = @NumOfWorkingHours
	                                            ,[ExperienceYear]    = @ExperienceYear
                                                ,[DateOfBirth]       = @DateOfBirth
                                                ,[Photo]             = @Photo
                                                ,[CourseId]          = @CourseId
                                                ,[MiddleName]        = @MiddleName
                                           where EmpID = @EmpID";
        private const string SQL_DELETE = @"delete 
                                            from employee 
                                            where EmpID = @EmpID";

        private string ConnStr;

        public EmpRepository(string connStr)
        {
            ConnStr = connStr;
        }

        public void Delete(int id)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL_DELETE;

                    command.Parameters.AddWithValue("@EmpID", id);

                    conn.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        

        public Employee GetById(int id)
        {
            Employee emp = new Employee();
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL_SELECT_BY_ID;
                    command.Parameters.AddWithValue("@EmpID", id);

                    conn.Open();

                    using (var dataReader = command.ExecuteReader())
                    {
                        if(dataReader.Read())
                            emp = Mappping(dataReader);
                    }

                }
            }

            return emp;
        }

        public Employee GetByIdFiltered(int id)
        {
            Employee emp = new Employee();
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL_SELECT_BY_ID_FILTERED;
                    command.Parameters.AddWithValue("@EmpID", id);

                    conn.Open();

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                            emp = MapppingFiltered(dataReader);
                    }

                }
            }

            return emp;
        }

        public List<Employee> GetEmployees()
        {
            var result = new List<Employee>();

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL_SELECT;
                    using (var dataReader = command.ExecuteReader())
                    {
                        while(dataReader.Read())
                        {
                            var emp = Mappping(dataReader);

                            result.Add(emp);
                        }
                    }
                }

            }

            return result;
        }

        public void Insert(Employee emp)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL_INSERT;

                    command.Parameters.AddWithValue("@FirstName", emp.FirstName);
                    command.Parameters.AddWithValue("@LastName", emp.LastName);
                    command.Parameters.AddWithValue("@Email", emp.Email);
                    command.Parameters.AddWithValue("@Salary", emp.Salary ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NumOfWorkingHours", emp.NumOfWorkingHours ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MiddleName", emp.MiddleName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ExperienceYear", emp.ExperienceYear ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DateOfBirth", emp.DateOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Photo", emp.Photo ?? (object)SqlBinary.Null);
                    command.Parameters.AddWithValue("@CourseId", emp.CourseId ?? (object)DBNull.Value);


                    conn.Open();

                    command.ExecuteNonQuery();

                }
            }

        }

        public void Update(Employee emp)
        {
            
            using (var conn = new SqlConnection(ConnStr))
            {
                using ( var command = conn.CreateCommand())
                {
                    command.CommandText = SQL_UPDATE;

                    command.Parameters.AddWithValue("@FirstName", emp.FirstName);
                    command.Parameters.AddWithValue("@LastName", emp.LastName);
                    command.Parameters.AddWithValue("@Email", emp.Email);
                    command.Parameters.AddWithValue("@Salary", emp.Salary ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MiddleName", emp.MiddleName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CourseId", emp.CourseId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NumOfWorkingHours", emp.NumOfWorkingHours ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ExperienceYear", emp.ExperienceYear ?? (object)DBNull.Value);       
                    command.Parameters.AddWithValue("@DateOfBirth", emp.DateOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Photo", emp.Photo ?? (object)SqlBinary.Null);
                    command.Parameters.AddWithValue("@EmpID", emp.EmpID);


                    conn.Open();

                    command.ExecuteNonQuery();

                }
            }

        }

        private Employee Mappping(DbDataReader dataReader)
        {
            return new Employee()
            {
                EmpID = dataReader.GetInt32(dataReader.GetOrdinal("EmpID")),
                LastName = dataReader.GetString(dataReader.GetOrdinal("LastName")),
                FirstName = dataReader.GetString(dataReader.GetOrdinal("FirstName")),
                Email = dataReader.GetString(dataReader.GetOrdinal("Email")),
                Salary = dataReader.IsDBNull(dataReader.GetOrdinal("Salary"))
                                ? (decimal?)null
                                : dataReader.GetDecimal(dataReader.GetOrdinal("Salary")),
                NumOfWorkingHours = dataReader.IsDBNull(dataReader.GetOrdinal("NumOfWorkingHours"))
                                ? (int?)null
                                : dataReader.GetInt32(dataReader.GetOrdinal("NumOfWorkingHours")),
                MiddleName = dataReader.IsDBNull(dataReader.GetOrdinal("MiddleName"))
                                ? (string?)null
                                : dataReader.GetString(dataReader.GetOrdinal("MiddleName")),
                CourseId = dataReader.IsDBNull(dataReader.GetOrdinal("CourseId"))
                                ? (int?)null
                                : dataReader.GetInt32(dataReader.GetOrdinal("CourseId")),
                ExperienceYear = dataReader.IsDBNull(dataReader.GetOrdinal("ExperienceYear"))
                                ? (int?)null
                                : dataReader.GetInt32(dataReader.GetOrdinal("ExperienceYear")),
                DateOfBirth = dataReader.IsDBNull(dataReader.GetOrdinal("DateOfBirth"))
                                ? (DateTime?)null
                                : dataReader.GetDateTime(dataReader.GetOrdinal("DateOfBirth")),
                Photo = dataReader.IsDBNull(dataReader.GetOrdinal("Photo"))
                                ? null
                                : (byte[])dataReader["Photo"]
            };
        }

        private Employee MapppingFiltered(DbDataReader dataReader)
        {
            return new Employee()
            {
                EmpID = dataReader.GetInt32(dataReader.GetOrdinal("EmpID")),
                LastName = dataReader.GetString(dataReader.GetOrdinal("LastName")),
                FirstName = dataReader.GetString(dataReader.GetOrdinal("FirstName")),
                Email = dataReader.GetString(dataReader.GetOrdinal("Email")),
                Salary = dataReader.IsDBNull(dataReader.GetOrdinal("Salary"))
                                ? (decimal?)null
                                : dataReader.GetDecimal(dataReader.GetOrdinal("Salary")),
                NumOfWorkingHours = dataReader.IsDBNull(dataReader.GetOrdinal("NumOfWorkingHours"))
                                ? (int?)null
                                : dataReader.GetInt32(dataReader.GetOrdinal("NumOfWorkingHours")),
                ExperienceYear = dataReader.IsDBNull(dataReader.GetOrdinal("ExperienceYear"))
                                ? (int?)null
                                : dataReader.GetInt32(dataReader.GetOrdinal("ExperienceYear")),
                Phone = dataReader.IsDBNull(dataReader.GetOrdinal("Phone"))
                                ? null
                                : dataReader.GetString(dataReader.GetOrdinal("Phone")),
                NumberOfStudents = dataReader.IsDBNull(dataReader.GetOrdinal("NumberOfStudents"))
                                ? (int?)null
                                : dataReader.GetInt32(dataReader.GetOrdinal("NumberOfStudents")),
                Name = dataReader.IsDBNull(dataReader.GetOrdinal("Name"))
                                ? null
                                : dataReader.GetString(dataReader.GetOrdinal("Name")),
                CourseName = dataReader.IsDBNull(dataReader.GetOrdinal("CourseName"))
                                ? null
                                : dataReader.GetString(dataReader.GetOrdinal("CourseName"))
            };
        }

        public int InsertImport(IEnumerable<Employee> employees)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("FirstName", typeof(string));
            dataTable.Columns.Add("LastName", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));
            dataTable.Columns.Add("Salary", typeof(decimal));
            dataTable.Columns.Add("NumOfWorkingHours", typeof(int));
            dataTable.Columns.Add("ExperienceYear", typeof(int));
            dataTable.Columns.Add("DateOfBirth", typeof(DateTime));


            foreach (var e in employees)
            {
                dataTable.Rows.Add(e.FirstName, e.LastName, e.Email, e.Salary, e.NumOfWorkingHours, e.ExperienceYear, e.DateOfBirth);
            }

            using (var conn = new SqlConnection(ConnStr))
            {
                return conn.Execute("procBInserting", new { Emps = dataTable },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<Employee> FilterStored(string firstName, string lastName, string email, decimal? salary, int? workingHours, out int total, string sortColumn, bool sortDesc = false, int page = 1, int pageSize = 4)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                
                var p = new DynamicParameters();

                p.Add("@FirstName", firstName);
                p.Add("@LastName", lastName);
                p.Add("@Email", email);
                p.Add("@Salary", salary);
                p.Add("@NumOfWorkingHours", workingHours);
                p.Add("@TotalC", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@Page", page);
                p.Add("@SordDesc", sortDesc);
                p.Add("@SortColumn", sortColumn);
                p.Add("@PageSize", pageSize);


                var employees = conn.Query<Employee>("updFiltering", p, commandType: CommandType.StoredProcedure);
                total = p.Get<int>("@TotalC");
                return employees;
            }
        }

        public string ExportingXml(EmpFilteringViewModel model)
        { 

            using (var conn = new SqlConnection(ConnStr))
            {

                var p = new DynamicParameters();
                p.Add("@xmldata", dbType: DbType.Xml, direction: ParameterDirection.Output);

                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@Email", model.Email);
                p.Add("@Salary", model.Salary);
                p.Add("@NumOfWorkingHours", model.NumOfWorkingHours);



                conn.Execute(
                    "udpEmployeeFilteredExportAsXml",
                    param: p,
                    commandType: CommandType.StoredProcedure);

                return p.Get<string>("@xmldata");
            }
        }

        public string ExportAsJSON(string firstName, string lastName, string email, decimal? salary, int? workingHours)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                var p = new DynamicParameters();

                p.Add("@json", "", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);

                p.Add("@FirstName", firstName);
                p.Add("@LastName", lastName);
                p.Add("@Email", email);
                p.Add("@Salary", salary);
                p.Add("@NumOfWorkingHours", workingHours);

                conn.Execute(
                    "udpFilteredEmpExportAsJson",
                    p,
                    commandType: CommandType.StoredProcedure
                    );

                return p.Get<string>("@json");
            }
        }

        public string ExportAsCSV(string firstName, string lastName, string email, decimal? salary, int? workingHours)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                var p = new DynamicParameters();

                p.Add("@FirstName", firstName);
                p.Add("@LastName", lastName);
                p.Add("@Email", email);
                p.Add("@Salary", salary);
                p.Add("@NumOfWorkingHours", workingHours);
                p.Add("@csv", "", dbType: DbType.String, direction: ParameterDirection.Output, size: int.MaxValue);

                conn.Execute(
                    "udpEmployeeFilteredExportTryingAsCSV",
                    p,
                    commandType: CommandType.StoredProcedure
                    );

                return p.Get<string>("@csv");
            }
        }
    }
}
