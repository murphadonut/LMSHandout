﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            // Create new department object
            Department newDepartment = new Department();
            newDepartment.Subject = subject;
            newDepartment.Name = name;

            // Try and save to database
            db.Departments.Add(newDepartment);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            } catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var courses =
                from c in db.Courses
                where c.Listing == subject
                select new
                {
                    number = c.Number,
                    name = c.Name,
                };
            return Json(courses.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            // Query for getting all the professors in a certain subject.
            var professors =
                from p in db.Professors
                where p.WorksIn == subject
                select new
                {
                    lname = p.LastName,
                    fname = p.FirstName,
                    uid = p.UId
                };

            return Json(professors.ToArray());

        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            // Create new course
            Course newCourse = new Course();
            // If course number is longer than 4 digits, this will just grab the first four
            newCourse.Number = ushort.Parse(number.ToString().Substring(0, number.ToString().Length > 4 ? 4 : number.ToString().Length));
            newCourse.Listing = subject;
            newCourse.Name = name;

            // Try and add it to the course catalog
            db.Courses.Add(newCourse);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            } catch
            {
                return Json(new { success = false });
            }
            
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            // Get course offereing ID
            int cid =
                (from c in db.Courses
                where c.Number == number && c.Listing == subject
                select c.CId).First();

            // Get other classes at the same location
            var otherClassesAtSameLocation = 
                from c in db.Classes
                where c.Location == location
                select c;

            // Check for double bookings
            foreach (var c in otherClassesAtSameLocation)
            {
                if(c.Season.Equals(season) && c.StartTime.IsBetween(TimeOnly.FromDateTime(start), TimeOnly.FromDateTime(end)) || c.EndTime.IsBetween(TimeOnly.FromDateTime(start), TimeOnly.FromDateTime(end))){
                    return Json(new { success = false });
                }
            }

            // Create new class
            Class newClass = new Class();
            newClass.CId = cid;
            newClass.Year = ushort.Parse(year.ToString().Substring(0, 4));
            newClass.StartTime = TimeOnly.FromDateTime(start);
            newClass.EndTime = TimeOnly.FromDateTime(end);
            newClass.Location = location;
            newClass.Season = season;
            newClass.Teacher = instructor;

            // Try and add class
            db.Classes.Add(newClass);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /*******End code to modify********/

    }
}

