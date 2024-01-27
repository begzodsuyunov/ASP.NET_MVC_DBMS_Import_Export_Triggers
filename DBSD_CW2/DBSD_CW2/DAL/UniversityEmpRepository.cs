using DBSD_CW2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DBSD_CW2.DAL
{
    public interface UniversityEmpRepository
    {
        string ExportingXml(EmpFilteringViewModel model);
        string ExportAsJSON(string firstName, string lastName, string email, decimal? salary, int? workingHours);
        string ExportAsCSV(string firstName, string lastName, string email, decimal? salary, int? workingHours);


        List<Employee> GetEmployees();

        //List<Employee> Filter(string firstName, string lastName, string email, decimal? salary, int? workingHours);
        //IEnumerable<Employee> Filter(string firstName, string lastName, string email, decimal? salary, int? workingHours, out int total, string sortColumn = null, bool sortDesc = false, int page = 1, int pageSize = 4);

        IEnumerable<Employee> FilterStored(string firstName, string lastName, string email, decimal? salary, int? workingHours, out int total, string sortColumn, bool sortDesc = false, int page = 1, int pageSize = 4);

        Employee GetById(int id);
        Employee GetByIdFiltered(int id);

        void Insert(Employee emp);

        void Update(Employee emp);

        void Delete(int id);

        int InsertImport(IEnumerable<Employee> employees);

    }
}
