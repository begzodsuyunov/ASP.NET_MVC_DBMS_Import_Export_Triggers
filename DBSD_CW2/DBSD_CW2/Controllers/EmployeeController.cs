using DBSD_CW2.DAL;
using DBSD_CW2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using CsvHelper;
using System.Globalization;

namespace DBSD_CW2.Controllers
{
    public class EmployeeController : Controller
    {
        private UniversityEmpRepository _repository;

        public EmployeeController(UniversityEmpRepository repository)
        {
            _repository = repository;
        }


        // GET: EmployeeController
        public ActionResult Index()
        {
            var employees = _repository.GetEmployees();
            return View(employees);
        }

        public ActionResult ExportingXml(EmpFilteringViewModel filter)
        {
            
            

            var xml = _repository.ExportingXml(filter);


            if (string.IsNullOrWhiteSpace(xml))
            {
                return NotFound();
            }
            else
            {


                return File(Encoding.UTF8.GetBytes(xml), "application/xml", $"Emp_{DateTime.Now}.xml");
            }

        }

        public ActionResult ExportAsJSON(EmpFilteringViewModel filter)
        {
            var json = _repository.ExportAsJSON(filter.FirstName, filter.LastName, filter.Email, filter.Salary, filter.NumOfWorkingHours);

            if (!string.IsNullOrWhiteSpace(json))
                return File(Encoding.UTF8.GetBytes(json), "application/json", $"Emp_{DateTime.Now}.json");
            else
                return NotFound();
        }

        public ActionResult ExportAsCSV(EmpFilteringViewModel filter)
        {
            var csv = _repository.ExportAsCSV(filter.FirstName, filter.LastName, filter.Email, filter.Salary, filter.NumOfWorkingHours);

            if (!string.IsNullOrWhiteSpace(csv))
                return File(Encoding.UTF8.GetBytes(csv), "application/csv", $"Emp_{DateTime.Now}.csv");
            else
                return NotFound();
        }



        public ActionResult Searching(string sortColumn, string sortDirection, int? page, EmpFilteringViewModel model)
        {
            int curPage = page ?? 1;
            int total;
            int pageSize=4;
            
            if (string.IsNullOrWhiteSpace(sortColumn)){
                sortColumn = "EmpID";
            }


            var listing = _repository.FilterStored(model.FirstName, model.LastName, model.Email, model.Salary, model.NumOfWorkingHours, out total, sortColumn, "DESC".Equals(sortDirection), curPage, 5);

            
            
            model.Employees = new StaticPagedList<Employee>(listing, curPage, pageSize, total);

            ViewBag.SortDirection = "DESC".Equals(sortDirection) ? "ASC" : "DESC";
            ViewBag.CurrentPage = curPage;

            return View(model);

        }


        // GET: EmployeeController/Details/5
        public ActionResult Details(int id)
        {
            var emp = _repository.GetById(id);
            return View(emp);
        }

        public ActionResult DetailsFiltered(int id)
        {
            var emp = _repository.GetByIdFiltered(id);
            return View(emp);
        }

        // GET: EmployeeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmployeeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee emp, IFormFile imageFile)
        {
            try
            {
                if(imageFile?.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        imageFile.OpenReadStream().CopyTo(stream);
                        emp.Photo = stream.ToArray();


                    }
                }
                _repository.Insert(emp);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: EmployeeController/Edit/5
        public ActionResult Edit(int id)
        {
            var emp = _repository.GetById(id);
            return View(emp);
        }

        // POST: EmployeeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Employee emp, IFormFile imageFile)
        {
            try
            {
                if (imageFile?.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        imageFile.OpenReadStream().CopyTo(stream);
                        emp.Photo = stream.ToArray();


                    }
                }
                _repository.Update(emp);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                return View();
            }
        }

        // GET: EmployeeController/Delete/5
        public ActionResult Delete(int id)
        {
            var emp = _repository.GetById(id);
            return View(emp);
        }

        // POST: EmployeeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                _repository.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                return View();
            }
        }

        public FileResult ShowImage(int id)
        {
            var emp = _repository.GetById(id);

            if(emp != null && emp.Photo?.Length > 0)
            {
                return File(emp.Photo, "image/jpeg", emp.LastName + ".jpeg");
            }
            return null;
        }

        public ActionResult ImportXml()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportXml(IFormFile importFile)
        {
            var emps = new List<Employee>();

            if (importFile != null)
            {
                using (var stream = importFile.OpenReadStream())
                using (var reader = new StreamReader(stream))
                {
                    var serializer = new XmlSerializer(typeof(List<Employee>), new XmlRootAttribute("Employees"));
                    emps = (List<Employee>)serializer.Deserialize(reader);
                }

                _repository.InsertImport(emps);
                return RedirectToAction("Index", "Employee");
            }
            else
            {
                ModelState.AddModelError("", "Empty file");
            }

            return RedirectToAction("Index");
        }

        public ActionResult ImportJson()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportJson(IFormFile importFile)
        {
            IList<Employee> employees = null;
            if (importFile != null)
            {
                using (var stream = importFile.OpenReadStream())
                using (var reader = new StreamReader(stream))
                {
                    var serializer = new JsonSerializer();
                    employees = (List<Employee>)serializer.
                        Deserialize(reader, typeof(List<Employee>));
                }

                _repository.InsertImport(employees);
            }

            return RedirectToAction("Index");

        }

        public ActionResult ImportCsv()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportCsv(IFormFile importFile)
        {
            var empList = new List<Employee>();

            if(importFile != null)
            {
                using (var str = new MemoryStream())
                {
                    importFile.CopyTo(str);

                    byte[] data = str.ToArray();

                    using (var byteStr = new MemoryStream(data))
                    using (var rdr = new StreamReader(byteStr))
                    using (var csv = new CsvReader(rdr, CultureInfo.InvariantCulture))
                    {
                        empList = csv.GetRecords<Employee>().ToList();
                        if(empList != null)
                        {
                            try
                            {
                                _repository.InsertImport(empList);
                            }
                            catch (Exception e)
                            {
                                ModelState.AddModelError("", $"Problem with file; meesage: {e}");
                            }
                        }
                    }
                }

                return RedirectToAction("Index");
            } else
            {
                ModelState.AddModelError("", "Empty File");
            }

            return View();
        }
    }
}
